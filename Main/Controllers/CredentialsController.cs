using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Main.Data;
using Main.Models;
using Main.Repository;
using Microsoft.Data.SqlClient;
using System.Data;
using Main.Structures;
using Microsoft.AspNetCore.Http.HttpResults;
using UtilityLibrary;
using System.ComponentModel.DataAnnotations;

namespace Main.Controllers
{

    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class CredentialsController : ControllerBase
    {
        private readonly BetacomioContext _context;

        private readonly ICredentialRepository _credentialRepository;

        public CredentialsController(BetacomioContext context, ICredentialRepository credentialRepository)
        {
            _context = context;
            _credentialRepository = credentialRepository;
        }

        // POST: api/Credentials/Login
        [HttpPost]
        public async Task<ActionResult<Credential>> Login([FromBody] LoginCredential loginCredential)
        {
            
            if (_context.Credentials == null)
            {
                return NotFound();
            }
            if (!await _context.Database.CanConnectAsync())
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(loginCredential.EmailAddress))
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(loginCredential.Password))
            {
                return BadRequest();
            }
            if (!MyValidator.IsEmailAddress(loginCredential.EmailAddress))
            {
                return BadRequest();
            }

            try
            {
                (bool result, Credential credential) = _credentialRepository.CheckLogin(loginCredential.EmailAddress, loginCredential.Password);
                if (!result)
                {
                    return NotFound();
                }
                return Ok(credential);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }






        // GET: api/Credentials/GetCredentials
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Credential>>> GetCredentials()
        {
            if (_context.Credentials == null)
            {
                return NotFound();
            }
            if (!await _context.Database.CanConnectAsync())
            {
                return NotFound();
            }
            try
            {
                return await _context.Credentials.ToListAsync();
            }catch(Exception)
            {
                return NotFound();
            }
        }

        // GET: api/Credentials/GetCredentialById/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Credential>> GetCredentialById(int id)
        {
            if (_context.Credentials == null)
            {
                return NotFound();
            }
            if (!await _context.Database.CanConnectAsync())
            {
                return NotFound(new ResponseToFrontEnd(500, false, "database not available"));
            }

            var credential = await _context.Credentials.FindAsync(id);

            if (credential == null)
            {
                return NotFound();
            }

            return credential;
        }

        // PUT: api/Credentials/PutCredential/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCredential(int id, Credential credential)
        {
            if (id != credential.CredentialsId)
            {
                return BadRequest();
            }
            if (!await _context.Database.CanConnectAsync())
            {
                return NotFound();
            }

            _context.Entry(credential).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CredentialExists(id))
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

        // DELETE: api/Credentials/DeleteCredential/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCredential(int id)
        {
            if (_context.Credentials == null)
            {
                return NotFound();
            }
            var credential = await _context.Credentials.FindAsync(id);
            if (credential == null)
            {
                return NotFound();
            }

            _context.Credentials.Remove(credential);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CredentialExists(int id)
        {
            return (_context.Credentials?.Any(e => e.CredentialsId == id)).GetValueOrDefault();
        }
    }
}
