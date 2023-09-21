using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Main.Data;
using Main.Models;
using Main.Structures;
using Main.Authentication;
using UtilityLibrary;
using Microsoft.AspNetCore.Authorization;

namespace Main.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SalesOrdersController : ControllerBase
    {

        private readonly BetacomioContext _contextBetacomio;
        private readonly AdventureWorksLt2019Context _contextAdventure;
        private readonly ILogger<SalesOrdersController> _logger;


        public SalesOrdersController(BetacomioContext contextBetacomio, AdventureWorksLt2019Context contextAdventure, ILogger<SalesOrdersController> logger)
        {
            _contextBetacomio = contextBetacomio;
            _contextAdventure = contextAdventure;
            _logger = logger;
        }
        [HttpPost]
        public async Task<ActionResult> PostSalesOrder(SalesOrder order)
        {
            (string emailAddress, string password) = BasicAuthenticationUtilities.GetUsernamePassword(Request.Headers["Authorization"].ToString());

            SalesOrderHeader header = new();
            header.OrderDate = DateTime.Now;
            header.Status = order.Status;
            header.ShipToAddressId = order.ShipToAddressID;
            header.BillToAddressId = order.BillToAddressID;
            header.SubTotal = order.SubTotal;
            header.TaxAmt = order.TaxAmt;
            header.TotalDue = order.TotalDue;
            header.Comment = order.Comment;
            header.CreditCardId = order.CreditCardID;
            header.DueDate = DateTime.Now;
            header.ModifiedDate = DateTime.Now;
            header.ShipDate = DateTime.Now;
            header.ShipMethod = "";

            using (var transAdventure = _contextAdventure.Database.BeginTransaction())
            {

                try
                {
                    var customer = await _contextAdventure.Customers.FirstAsync(c => c.EmailAddress == emailAddress);
                    if (customer == null)
                        return BadRequest();
                    header.CustomerId = customer.CustomerId;

                    _contextAdventure.SalesOrderHeaders.Add(header);

                    await _contextAdventure.SaveChangesAsync();
                    foreach (SalesOrderDetail d in order.SalesOrderDetails)
                    {
                        d.SalesOrderId = header.SalesOrderId;
                        d.ModifiedDate = DateTime.Now;
                        header.SalesOrderDetails.Add(d);
                        _contextAdventure.SalesOrderDetails.Add(d);
                    }
                    await _contextAdventure.SaveChangesAsync();
                    await transAdventure.CommitAsync();
                    return Ok(header);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);

                    await transAdventure.RollbackAsync();
                    return BadRequest();
                }
            }


        }

        [HttpGet]
        public async Task<ActionResult> GetSalesOrderHeaders()
        {
            if (_contextAdventure.SalesOrderHeaders == null)
            {
                return BadRequest();
            }
            try
            {
                var res = await _contextAdventure.SalesOrderHeaders
                    .Include(o => o.SalesOrderDetails)
                    .ToListAsync();
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Problem("database problem", statusCode: 500);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ICollection<SalesOrderHeader>>> GetOrdersOrderedByDate()
        {
            if (_contextAdventure.SalesOrderHeaders == null)
            {
                return NotFound();
            }
            if (_contextAdventure.SalesOrderDetails == null)
            {
                return NotFound();
            }

            return await _contextAdventure.SalesOrderHeaders
                .Include(s => s.SalesOrderDetails)
                .OrderBy(s => s.OrderDate)
                .ToListAsync();
        }

        [Authorize(Policy = "Customer")]
        [HttpGet]
        public async Task<ActionResult> GetOrdersByCustomer()
        {
            (string emailAddress, string password) = BasicAuthenticationUtilities.GetUsernamePassword(Request.Headers["Authorization"].ToString());
            try
            {
                Customer c = await _contextAdventure.Customers.FirstAsync(c => c.EmailAddress == emailAddress);

                var orders = await (from h in _contextAdventure.SalesOrderHeaders
                                    join d in _contextAdventure.SalesOrderDetails on h.SalesOrderId equals d.SalesOrderId
                                    join p in _contextAdventure.Products on d.ProductId equals p.ProductId
                                    where h.CustomerId == c.CustomerId
                                    select new
                                    {
                                        h.SalesOrderId,
                                        d.SalesOrderDetailId,
                                        p.ProductId,
                                        h.OrderDate,
                                        h.Status,
                                        ProductName = p.Name,
                                        ThumbNailPhoto = p.ThumbNailPhoto == null ? null : Convert.ToBase64String(p.ThumbNailPhoto),
                                        p.ThumbnailPhotoFileName,
                                        d.OrderQty,
                                        d.UnitPrice,
                                        d.UnitPriceDiscount,
                                        d.LineTotal
                                    }).ToListAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return BadRequest();
            }

        }

    }
}
