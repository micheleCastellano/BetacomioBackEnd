using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Main.Data;
using Main.Models;
using Microsoft.CodeAnalysis;
using Main.Structures;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace Main.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

        private readonly AdventureWorksLt2019Context _Adventure;
        private readonly ILogger<ProductsController> _logger;
        public ProductsController(AdventureWorksLt2019Context adventure, ILogger<ProductsController> logger)
        {
            _Adventure = adventure;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetProducts()
        {
            if (_Adventure.Products == null)
            {
                return NotFound();
            }
            try
            {
                var res = await (from p in _Adventure.Products
                                 join pc in _Adventure.ProductCategories on p.ProductCategoryId equals pc.ProductCategoryId
                                 join pm in _Adventure.ProductModels on p.ProductModelId equals pm.ProductModelId
                                 join pmpd in _Adventure.ProductModelProductDescriptions on pm.ProductModelId equals pmpd.ProductModelId
                                 join pd in _Adventure.ProductDescriptions on pmpd.ProductDescriptionId equals pd.ProductDescriptionId

                                 select new
                                 {
                                     p.ProductId,
                                     ProductName = p.Name,
                                     p.Color,
                                     p.Size,
                                     p.Weight,
                                     ThumbNailPhoto = p.ThumbNailPhoto == null ? null : Convert.ToBase64String(p.ThumbNailPhoto),
                                     p.ThumbnailPhotoFileName,
                                     p.ListPrice,

                                     pc.ProductCategoryId,
                                     ProductCategory = pc.Name,
                                     ParentName = pc.ParentProductCategory == null ? null : _Adventure.ProductCategories.FirstOrDefault(c => c.ProductCategoryId == pc.ParentProductCategoryId)!.Name,

                                     pm.ProductModelId,
                                     ModelName = pm.Name,

                                     pd.Description,
                                     p.Quantity
                                 }).ToListAsync();

                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return BadRequest();
                throw;
            }

        }

        [Authorize(Policy = "Admin")]
        [HttpGet]
        public ActionResult GetBestSellerProducts()
        {
            if (_Adventure.Products == null)
            {
                return NotFound();
            }
            try
            {
                List<ShortProduct> products = new ();
                
                using (SqlConnection conn = new SqlConnection(config.GetConnectionString("AdventureWorksLt2019")))
                {
                    using (SqlCommand command = new SqlCommand("exec sp_GetBestSellerProduct", conn))
                    {
                        conn.Open();
                        SqlDataReader res = command.ExecuteReader();

                        while (res.Read())
                        {
                            products.Add(new ShortProduct
                            {
                                Name = res["Name"].ToString(),
                                QuantitySold = Convert.ToInt32( res["QuantitySold"])
                            });
                        }
                    }
                }

                return Ok(products);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return Problem(e.Message, statusCode: 500);
            }

        }

        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult> GetProductsOrderedByDate()
        {
            if (_Adventure.Products == null)
            {
                return NotFound();
            }
            try
            {
                var res = await (from p in _Adventure.Products
                                 join pc in _Adventure.ProductCategories on p.ProductCategoryId equals pc.ProductCategoryId
                                 join pm in _Adventure.ProductModels on p.ProductModelId equals pm.ProductModelId
                                 join pmpd in _Adventure.ProductModelProductDescriptions on pm.ProductModelId equals pmpd.ProductModelId
                                 join pd in _Adventure.ProductDescriptions on pmpd.ProductDescriptionId equals pd.ProductDescriptionId
                                 orderby p.ModifiedDate
                                 select new
                                 {
                                     p.ProductId,
                                     ProductName = p.Name,
                                     p.Color,
                                     p.Size,
                                     p.Weight,
                                     ThumbNailPhoto = p.ThumbNailPhoto == null ? null : Convert.ToBase64String(p.ThumbNailPhoto),
                                     p.ThumbnailPhotoFileName,
                                     p.ListPrice,

                                     pc.ProductCategoryId,
                                     ProductCategory = pc.Name,
                                     ParentName = pc.ParentProductCategory == null ? null : _Adventure.ProductCategories.FirstOrDefault(c => c.ProductCategoryId == pc.ParentProductCategoryId)!.Name,

                                     pm.ProductModelId,
                                     ModelName = pm.Name,

                                     pd.Description,
                                     p.Quantity
                                 }).ToListAsync();

                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return BadRequest();
                throw;
            }

        }


        //this method does not return only 1 product but a list of the same product with the same ID with different model or category
        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(int id)
        {
            if (_Adventure.Products == null)
            {
                return NotFound();
            }
            try
            {
                var product = await (from p in _Adventure.Products
                                     join pc in _Adventure.ProductCategories on p.ProductCategoryId equals pc.ProductCategoryId
                                     join pm in _Adventure.ProductModels on p.ProductModelId equals pm.ProductModelId
                                     join pmpd in _Adventure.ProductModelProductDescriptions on pm.ProductModelId equals pmpd.ProductModelId
                                     join pd in _Adventure.ProductDescriptions on pmpd.ProductDescriptionId equals pd.ProductDescriptionId
                                     where p.ProductId == id
                                     select new
                                     {
                                         p.ProductId,
                                         ProductName = p.Name,
                                         p.Color,
                                         p.Size,
                                         p.Weight,
                                         ThumbNailPhoto = p.ThumbNailPhoto == null ? null : Convert.ToBase64String(p.ThumbNailPhoto!),
                                         p.ThumbnailPhotoFileName,
                                         p.ListPrice,

                                         pc.ProductCategoryId,
                                         ProductCategory = pc.Name,
                                         ParentName = pc.ParentProductCategory == null ? null : _Adventure.ProductCategories.FirstOrDefault(c => c.ProductCategoryId == pc.ParentProductCategoryId)!.Name,


                                         pm.ProductModelId,
                                         ModelName = pm.Name,

                                         pd.Description,
                                         p.Quantity

                                     }).FirstAsync();
                if (product == null)
                {
                    return NotFound();
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return BadRequest();
            }



        }

        [HttpPut]
        public ActionResult PatchProduct(ProductForPatch product)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(config.GetConnectionString("AdventureWorksLt2019")))
                {
                    using (SqlCommand command = new SqlCommand("sp_UpdateProductInfo", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@ProductId", SqlDbType.Int).Value=product.ProductId;
                        command.Parameters.Add("@CategoryId", SqlDbType.Int).Value=product.CategoryId;
                        command.Parameters.Add("@NameProduct", SqlDbType.NVarChar).Value=product.NameProduct;
                        command.Parameters.Add("@Price", SqlDbType.Money).Value=product.Price;
                        command.Parameters.Add("@Size", SqlDbType.NVarChar).Value=product.Size;
                        command.Parameters.Add("@Weight", SqlDbType.Decimal).Value=product.Weight;
                        command.Parameters.Add("@Quantity", SqlDbType.Int).Value=product.Quantity;
                        command.Parameters.Add("@Photo", SqlDbType.VarBinary).Value= string.IsNullOrEmpty(product.Photo)? DBNull.Value:  Convert.FromBase64String(product.Photo);
                        command.Parameters.Add("@PhotoName", SqlDbType.NVarChar).Value=product.PhotoName;
                        command.Parameters.Add("@Description", SqlDbType.NVarChar).Value=product.Description;
                        conn.Open();
                        int x=command.ExecuteNonQuery();
                        if (x > 0)
                            return Ok();

                        return BadRequest();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return Problem(e.Message, statusCode: 500);
            }
        }


        [Authorize(Policy = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest();
            }

            _Adventure.Entry(product).State = EntityState.Modified;

            try
            {
                await _Adventure.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        //[Authorize(Policy = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            if (_Adventure.Products == null)
            {
                return Problem("Entity set 'AdventureWorksLt2019Context.Products'  is null.");
            }
            product.ModifiedDate = DateTime.Now;
            product.SellStartDate = DateTime.Now;
            product.DiscontinuedDate = DateTime.Now;
            product.SellEndDate = DateTime.Now;

            _Adventure.Products.Add(product);
            try
            {
                await _Adventure.SaveChangesAsync();
                return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        [Authorize(Policy = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (_Adventure.Products == null)
            {
                return NotFound();
            }
            var product = await _Adventure.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _Adventure.Products.Remove(product);
            try
            {
                await _Adventure.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Problem(ex.Message, statusCode: 500);
            }

        }
        private bool ProductExists(int id)
        {
            return (_Adventure.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
    }
}
