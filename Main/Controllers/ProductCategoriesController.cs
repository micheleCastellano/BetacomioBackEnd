using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Main.Data;

namespace Main.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly ILogger<ProductCategoriesController> _logger;


        public ProductCategoriesController(AdventureWorksLt2019Context context, ILogger<ProductCategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ProductCategories
        [HttpGet]
        public async Task<ActionResult> GetProductCategories()
        {
            if (_context.ProductCategories == null)
            {
                return NotFound();
            }
            try
            {
                var res = await (from pc in _context.ProductCategories
                                 where pc.ParentProductCategoryId != null
                                 select new
                                 {
                                     pc.ProductCategoryId,
                                     pc.Name,
                                     ParentCategory = pc.ParentProductCategory == null ? null : _context.ProductCategories.FirstOrDefault(c => c.ProductCategoryId == pc.ParentProductCategoryId)!.Name
                                 }).ToListAsync();
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return BadRequest();
            }


        }
    }

}
