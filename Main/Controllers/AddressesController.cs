using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Main.Data;
using Main.Models;
using UtilityLibrary;
using Main.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Main.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _contextAdv;
        private readonly ILogger<AddressesController> _logger;


        public AddressesController(AdventureWorksLt2019Context contextAdv, ILogger<AddressesController> logger)
        {
            _contextAdv = contextAdv;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Address>>> GetAllAddresses()
        {
            if (_contextAdv.Addresses == null)
            {
                return NotFound();
            }
            return await _contextAdv.Addresses.ToListAsync();
        }

        [Authorize(Policy = "Customer")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddressesByCustomer()
        {
            if (_contextAdv.Addresses == null)
            {
                return NotFound();
            }
            (string emailAddress, string password) = BasicAuthenticationUtilities.GetUsernamePassword(Request.Headers["Authorization"].ToString());
            try
            {
                var customer = await _contextAdv.Customers.FirstAsync(c => c.EmailAddress == emailAddress);
                var addresses =await (from a in _contextAdv.Addresses
                                 join ca in _contextAdv.CustomerAddresses on a.AddressId equals ca.AddressId
                                 where ca.CustomerId == customer.CustomerId
                                 select a).ToListAsync();
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Problem(ex.Message, statusCode: 500);
            }
        }

        [Authorize(Policy="Customer")]
        [HttpPost]
        public async Task<ActionResult<Address>> PostAddress(Address address)
        {
            if (_contextAdv.Addresses == null)
            {
                return Problem("Entity set 'AdventureWorksLt2019Context.Addresses'  is null.");
            }

            (string emailAddress, string password) = BasicAuthenticationUtilities.GetUsernamePassword(Request.Headers["Authorization"].ToString());
            using (var trans = await _contextAdv.Database.BeginTransactionAsync())
            {
                try
                {
                    var addressID = await _contextAdv.Addresses.Where(a =>
                    a.AddressLine1 == address.AddressLine1 && a.City == address.City && a.StateProvince == address.StateProvince && a.CountryRegion == address.CountryRegion && a.PostalCode == address.PostalCode)
                        .Select(a => a.AddressId).FirstOrDefaultAsync();

                    if (addressID == 0)
                    {
                        address.ModifiedDate = DateTime.Now;
                        await _contextAdv.Addresses.AddAsync(address);
                        await _contextAdv.SaveChangesAsync();
                        addressID = address.AddressId;
                    }

                    var customer = await _contextAdv.Customers.FirstAsync(c => c.EmailAddress == emailAddress);
                    CustomerAddress ca = new CustomerAddress()
                    {
                        AddressId = addressID,
                        CustomerId = customer.CustomerId,
                        AddressType = "Shipping",
                        ModifiedDate = DateTime.Now
                    };
                    await _contextAdv.CustomerAddresses.AddAsync(ca);
                    await _contextAdv.SaveChangesAsync();
                    await trans.CommitAsync();

                    return Ok(address);

                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex.Message, ex);
                    return Problem(ex.Message, statusCode: 500);
                }
            }
        }
    }
}
