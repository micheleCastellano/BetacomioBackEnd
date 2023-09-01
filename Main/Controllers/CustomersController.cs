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
using UtilityLibrary;
using System.Net.Mail;

namespace Main.Controllers
{
    //[BasicAuthorization]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _adventureDB;
        private readonly ICredentialRepository _credentialRepository;
        private readonly BetacomioContext _betacomioDB;
        private readonly ILogger<CustomersController> _logger;


        public CustomersController(AdventureWorksLt2019Context adventurDB, ICredentialRepository credentialRepository, BetacomioContext betacomioDB, ILogger<CustomersController> logger)
        {
            _adventureDB = adventurDB;
            _credentialRepository = credentialRepository;
            _betacomioDB = betacomioDB;
            _logger = logger;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            if (_adventureDB.Customers == null)
            {
                return NotFound();
            }
            return await _adventureDB.Customers.ToListAsync();
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            if (_adventureDB.Customers == null)
            {
                return NotFound();
            }
            var customer = await _adventureDB.Customers.FindAsync(id);

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

            _adventureDB.Entry(customer).State = EntityState.Modified;

            try
            {
                await _adventureDB.SaveChangesAsync();
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
            if (_adventureDB.Customers == null)
            {
                return Problem("Entity set 'AdventureWorksLt2019Context.Customers'  is null.");
            }

            if (string.IsNullOrEmpty(registerCustomer.firstName))
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(registerCustomer.lastName))
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(registerCustomer.password))
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(registerCustomer.emailAddress))
            {
                return BadRequest();
            }
            if (!MyValidator.IsEmailAddress(registerCustomer.emailAddress))
            {
                return BadRequest();
            }

            bool exist = await _credentialRepository.EmailAddressExist(registerCustomer.emailAddress);

            if (exist)
            {
                BadRequest();
            }


            using var transAdventure = _adventureDB.Database.BeginTransaction();
            using var transBetacomio = _betacomioDB.Database.BeginTransaction();

            try
            {
                Customer customer = new Customer();
                customer.FirstName = registerCustomer.firstName;
                customer.LastName = registerCustomer.lastName;
                customer.EmailAddress = registerCustomer.emailAddress;
                customer.Phone = registerCustomer.phone;
                _adventureDB.Customers.Add(customer);
                await _adventureDB.SaveChangesAsync();


                (string encryptedPwd, string salt) = MyEncryptor.EncryptStringSaltInsideHashing(registerCustomer.password);
                Credential cred = new Credential()
                {
                    PasswordSalt = salt,
                    EmailAddress = registerCustomer.emailAddress,
                    PasswordHash = encryptedPwd
                };
                _betacomioDB.Credentials.Add(cred);
                await _betacomioDB.SaveChangesAsync();

                await transAdventure.CommitAsync();
                await transBetacomio.CommitAsync();
                return Ok(customer);
            }
            catch (Exception)
            {
                await transAdventure.RollbackAsync();
                await transBetacomio.RollbackAsync();
                return Problem();
            }
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            if (_adventureDB.Customers == null)
            {
                return NotFound();
            }
            var customer = await _adventureDB.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _adventureDB.Customers.Remove(customer);
            await _adventureDB.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return (_adventureDB.Customers?.Any(e => e.CustomerId == id)).GetValueOrDefault();
        }
    }
}
