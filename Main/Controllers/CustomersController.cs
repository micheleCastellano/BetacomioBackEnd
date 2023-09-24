using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Main.Data;
using Main.Models;
using Main.Structures;
using Main.Repository;
using UtilityLibrary;
using Microsoft.AspNetCore.JsonPatch;
using Main.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Main.Controllers
{
    //[BasicAuthorization]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _adventureDB;
        private readonly ICredentialRepository _credentialRepository;
        private readonly BetacomioContext _betacomioDB;
        private readonly ILogger<CustomersController> _logger;
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
        public CustomersController(AdventureWorksLt2019Context adventurDB, ICredentialRepository credentialRepository, BetacomioContext betacomioDB, ILogger<CustomersController> logger)
        {
            _adventureDB = adventurDB;
            _credentialRepository = credentialRepository;
            _betacomioDB = betacomioDB;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            if (_adventureDB.Customers == null)
            {
                return NotFound();
            }
            return await _adventureDB.Customers.ToListAsync();
        }

        [Authorize(Policy ="Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomersOrderedByDateRegister()
        {
            if (_adventureDB.Customers == null)
            {
                return NotFound();
            }
            return await (from c in _adventureDB.Customers
                          orderby c.ModifiedDate
                          select c).ToListAsync();
        }

        [Authorize(Policy ="Customer")]
        [HttpGet]
        public async Task<ActionResult<Customer>> GetCustomerByHeader()
        {
            if (_adventureDB.Customers == null)
            {
                return NotFound();
            }
            (string emailAddress, string password) = BasicAuthenticationUtilities.GetUsernamePassword(Request.Headers["Authorization"].ToString());

            try
            {
                var customer = await _adventureDB.Customers.FirstAsync(c => c.EmailAddress == emailAddress);
                if (customer == null)
                    return NotFound();
                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Problem(ex.Message, statusCode: 500);
            }

        }

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


                (string encryptedPwd, string salt) = MyEncryptor.EncryptStringSaltInsideHashing(registerCustomer.password.Trim());
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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                await transAdventure.RollbackAsync();
                await transBetacomio.RollbackAsync();
                return Problem();
            }
        }

        //[BasicAuthorization]
        //[HttpPatch]
        //public async Task<IActionResult> PatchCustomer(RegisterCustomer data)
        //{
        //    (string emailAddress, string password) = BasicAuthenticationUtilities.GetUsernamePassword(Request.Headers["Authorization"].ToString());

        //    try
        //    {
        //        var customer = await _adventureDB.Customers.FirstAsync(c => c.EmailAddress==emailAddress);

        //        Credential cred = new Credential();
        //        Customer c = new Customer();

        //        if(!string.IsNullOrEmpty(data.emailAddress))
        //        {
        //            c.EmailAddress=data.emailAddress;
        //        }

        //        if(!string.IsNullOrEmpty(data.firstName))
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(ex.Message);
        //    }

        //}

        [Authorize(Policy = "Customer")]
        [HttpPut("{id}")]
        public IActionResult PutCustomer(int id, RegisterCustomer customer)
        {
            if (string.IsNullOrEmpty(customer.firstName) ||
                string.IsNullOrEmpty(customer.lastName) ||
                (string.IsNullOrEmpty(customer.emailAddress) || !MyValidator.IsEmailAddress(customer.emailAddress)) ||
                string.IsNullOrEmpty(customer.phone) ||
                string.IsNullOrEmpty(customer.password))
            {
                return BadRequest();
            }
            // Password encryption
            (string encryptedPwd, string salt) = MyEncryptor.EncryptStringSaltInsideHashing(customer.password.Trim());
            using (SqlConnection conn = new SqlConnection(config.GetConnectionString("AdventureWorksLt2019")))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    using (SqlCommand command = new SqlCommand("sp_UpdateCustomer", conn)
                    )
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        command.Parameters.Add("@FirstName", SqlDbType.NVarChar).Value = customer.firstName;
                        command.Parameters.Add("@LastName", SqlDbType.NVarChar).Value = customer.lastName;
                        command.Parameters.Add("@EmailAddress", SqlDbType.NVarChar).Value = customer.emailAddress;
                        command.Parameters.Add("@Phone", SqlDbType.NVarChar).Value = customer.phone;
                        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar).Value = encryptedPwd;
                        command.Parameters.Add("@PasswordSalt", SqlDbType.NVarChar).Value = salt;
                        command.Transaction = trans;
                        int res = command.ExecuteNonQuery();
                        if (res == -1)
                        {
                            trans.Rollback();
                            return BadRequest();
                        }
                        trans.Commit();
                        return Ok();
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    trans.Rollback();
                    if (!CustomerExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex.Message, ex);
                        throw;
                    }
                }
            }
        }

        [Authorize(Policy ="Admin")]
        [HttpPut("{id}")]
        public IActionResult PutCustomerAdmin(int id, Customer customer)
        {
            if(id != customer.CustomerId ||
                string.IsNullOrEmpty(customer.FirstName) ||
                string.IsNullOrEmpty(customer.LastName) ||
                (string.IsNullOrEmpty(customer.EmailAddress) || !MyValidator.IsEmailAddress(customer.EmailAddress)) ||
                string.IsNullOrEmpty(customer.Phone)) 
            {
                return BadRequest();
            }
            using (SqlConnection conn = new SqlConnection(config.GetConnectionString("AdventureWorksLt2019")))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    using (SqlCommand command = new SqlCommand("sp_UpdateCustomer", conn)
                    )
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        command.Parameters.Add("@FirstName", SqlDbType.NVarChar).Value = customer.FirstName;
                        command.Parameters.Add("@LastName", SqlDbType.NVarChar).Value = customer.LastName;
                        command.Parameters.Add("@EmailAddress", SqlDbType.NVarChar).Value = customer.EmailAddress;
                        command.Parameters.Add("@Phone", SqlDbType.NVarChar).Value = customer.Phone;
                        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar).Value = DBNull.Value;
                        command.Parameters.Add("@PasswordSalt", SqlDbType.NVarChar).Value = DBNull.Value;
                        command.Transaction = trans;
                        int res = command.ExecuteNonQuery();
                        if (res == -1)
                        {
                            trans.Rollback();
                            return BadRequest();
                        }
                        trans.Commit();
                        return Ok();
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    trans.Rollback();
                    if (!CustomerExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex.Message, ex);
                        throw;
                    }
                }
            }
        }

        [Authorize(Policy = "Admin")]
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
            try
            {
                await _adventureDB.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Problem("not elimated", statusCode: 500);
            }


        }

        private bool CustomerExists(int id)
        {
            return (_adventureDB.Customers?.Any(e => e.CustomerId == id)).GetValueOrDefault();
        }
    }
}
