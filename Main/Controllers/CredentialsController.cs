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
using Main.EmailSender;

namespace Main.Controllers
{

    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class CredentialsController : ControllerBase
    {
        private readonly BetacomioContext _contextBet;

        private readonly ICredentialRepository _credentialRepository;
        private readonly IEmailSender _emailSender;

        public CredentialsController(BetacomioContext contextBet, ICredentialRepository credentialRepository, IEmailSender emailSender)
        {
            _contextBet = contextBet;
            _credentialRepository = credentialRepository;
            _emailSender = emailSender;
        }

        // POST: api/Credentials/Login
        [HttpPost]
        public async Task<ActionResult<Credential>> Login([FromBody] LoginCredential loginCredential)
        {

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

            //Search if credential exists in Betacomio Database
            var credential = _credentialRepository.CheckLoginBetacomio(loginCredential.EmailAddress, loginCredential.Password);

            if (credential != null)
            {
                return Ok(credential);
            }


            //Search if credential exists in Adventure Database
            var credential2= await _credentialRepository.CheckLoginAdventure(loginCredential.EmailAddress);

            if (credential2 != null)
            {
                return Accepted(credential2);
            }

            //Credential doesn't exist neither in Betacomio nor in Adventure
            return NotFound();

        }

        [HttpPost]
        public async Task<ActionResult<Credential>> Addcredential([FromBody] LoginCredential loginCredential)
        {

            return await _credentialRepository.AddCredentialAsync(loginCredential.EmailAddress, loginCredential.Password);

        }

        // GET: api/Credentials/GetCredentials
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Credential>>> GetCredentials()
        {
            if (_contextBet.Credentials == null)
            {
                return NotFound();
            }
            if (!await _contextBet.Database.CanConnectAsync())
            {
                return NotFound();
            }
            try
            {
                return await _contextBet.Credentials.ToListAsync();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // GET: api/Credentials/GetCredentialById/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Credential>> GetCredentialById(int id)
        {
            if (_contextBet.Credentials == null)
            {
                return NotFound();
            }
            if (!await _contextBet.Database.CanConnectAsync())
            {
                return NotFound(new ResponseToFrontEnd(500, false, "database not available"));
            }

            var credential = await _contextBet.Credentials.FindAsync(id);

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
            if (!await _contextBet.Database.CanConnectAsync())
            {
                return NotFound();
            }

            _contextBet.Entry(credential).State = EntityState.Modified;

            try
            {
                await _contextBet.SaveChangesAsync();
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
            if (_contextBet.Credentials == null)
            {
                return NotFound();
            }
            var credential = await _contextBet.Credentials.FindAsync(id);
            if (credential == null)
            {
                return NotFound();
            }

            _contextBet.Credentials.Remove(credential);
            await _contextBet.SaveChangesAsync();

            return NoContent();
        }

        private bool CredentialExists(int id)
        {
            return (_contextBet.Credentials?.Any(e => e.CredentialsId == id)).GetValueOrDefault();
        }
    }
}
