using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Main.Data;
using Main.Models;
using Main.Structures;
using Microsoft.CodeAnalysis;
using System.Data.SqlTypes;
using System.IO;
using UtilityLibrary;

namespace Main.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {

        private readonly AdventureWorksLt2019Context _Adventure;
        private readonly ILogger<ProductsController> _logger;
        public ProductsController(AdventureWorksLt2019Context adventure, ILogger<ProductsController> logger)
        {
            _Adventure = adventure;
            _logger = logger;
        }


        // GET: api/Products
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

        // GET: api/Products/5
        //this does not return only 1 product but a list of the same product with different model or category
        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(int id)
        {
            if (_Adventure.Products == null)
            {
                return NotFound();
            }
            var product = (from p in _Adventure.Products
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

                           }).First();

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // PUT: api/Products/5
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

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            if (_Adventure.Products == null)
            {
                return Problem("Entity set 'AdventureWorksLt2019Context.Products'  is null.");
            }

            _Adventure.Products.Add(product);
            await _Adventure.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
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
            await _Adventure.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return (_Adventure.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
    }
}
