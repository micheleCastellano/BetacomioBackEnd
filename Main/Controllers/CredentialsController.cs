using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Main.Data;
using Main.Models;
using Main.Repository;
using Main.Structures;
using UtilityLibrary;

namespace Main.Controllers
{

    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class CredentialsController : ControllerBase
    {
        private readonly BetacomioContext _contextBet;
        private readonly ICredentialRepository _credentialRepository;
        private readonly ILogger<CredentialsController> _logger;

        public CredentialsController(BetacomioContext contextBet, ICredentialRepository credentialRepository, AdventureWorksLt2019Context _contextAdv, ILogger<CredentialsController> logger)
        {
            _contextBet = contextBet;
            _credentialRepository = credentialRepository;
            _logger = logger;   
        }

        [HttpPost]
        public async Task<ActionResult<Credential>> Login([FromBody] LoginCredential loginCredential)
        {

            if (string.IsNullOrEmpty(loginCredential.user))
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(loginCredential.pwd))
            {
                return BadRequest();
            }

            if (!MyValidator.IsEmailAddress(loginCredential.user))
            {
                return BadRequest();
            }

            //Search if credential exists in Betacomio Database
            var credential = _credentialRepository.CheckLoginBetacomio(loginCredential.user, loginCredential.pwd);

            if (credential != null)
            {
                
                return Ok(credential);
            }


            //Search if credential exists in Adventure Database
            var credential2= await _credentialRepository.CheckLoginAdventure(loginCredential.user);

            if (credential2 != null)
            {
                return Accepted(credential2);
            }

            //Credential doesn't exist neither in Betacomio nor in Adventure
            return NotFound();
        }

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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message,ex);
                return NotFound();
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Credential>> GetCredentialById(int id)
        {
            if (_contextBet.Credentials == null)
            {
                return NotFound();
            }
            if (!await _contextBet.Database.CanConnectAsync())
            {
                return NotFound("database didn't work");
            }

            var credential = await _contextBet.Credentials.FindAsync(id);

            if (credential == null)
            {
                return NotFound();
            }

            return credential;
        }

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
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex.Message);
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
            try
            {
                await _contextBet.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Problem("savechanges didn't work",statusCode:500);
            }
        }

        private bool CredentialExists(int id)
        {
            return (_contextBet.Credentials?.Any(e => e.CredentialsId == id)).GetValueOrDefault();
        }
    }
}
