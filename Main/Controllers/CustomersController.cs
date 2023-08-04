using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Main.Data;
using Main.Models;
using Main.Authentication;
using Main.Structures;
using Main.Repository;

namespace Main.Controllers
{
    //[BasicAuthorization]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly ICredentialRepository _credentialRepository;

        public CustomersController(AdventureWorksLt2019Context context, ICredentialRepository credentialRepository)
        {
            _context = context;
            _credentialRepository = credentialRepository;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            if (_context.Customers == null)
            {
                return NotFound();
            }
            return await _context.Customers.ToListAsync();
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            if (_context.Customers == null)
            {
                return NotFound();
            }
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        // PUT: api/Customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return BadRequest();
            }

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
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

        // POST: api/Customers
        [HttpPost]
        public async Task<ActionResult> PostCustomer(RegisterCustomer registerCustomer)
        {
            if (_context.Customers == null)
            {
                return Problem("Entity set 'AdventureWorksLt2019Context.Customers'  is null.");
            }
            Customer customer = new Customer();
            customer.FirstName = registerCustomer.FirstName;
            customer.LastName = registerCustomer.LastName;
            customer.EmailAddress = registerCustomer.EmailAddress;
            customer.Phone = registerCustomer.Phone;

            _context.Customers.Add(customer);

            try
            {

                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("errore: " + ex.Message);
            }


            await _credentialRepository.AddCredentialAsync(registerCustomer.EmailAddress, registerCustomer.Password);


            return Ok(customer);



            //if (_context.Customers == null)
            //{
            //    return Problem("Entity set 'AdventureWorksLt2019Context.Customers'  is null.");
            //}
            //  _context.Customers.Add(customer);
            //  await _context.SaveChangesAsync();

            // return Ok(); //CreatedAtAction("GetCustomer", new { id = customer.CustomerId }, customer);
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            if (_context.Customers == null)
            {
                return NotFound();
            }
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return (_context.Customers?.Any(e => e.CustomerId == id)).GetValueOrDefault();
        }
    }
}
