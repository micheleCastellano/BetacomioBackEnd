using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Main.Data;
using Main.Models;
using UtilityLibrary;
using Main.Authentication;

namespace Main.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CreditCardsController : ControllerBase
    {
        private readonly BetacomioContext _contextBet;
        private readonly AdventureWorksLt2019Context _contextAdv;
        private readonly ILogger<CreditCardsController> _logger;


        public CreditCardsController(BetacomioContext contextBet, ILogger<CreditCardsController> logger, AdventureWorksLt2019Context contextAdv)
        {
            _contextBet = contextBet;
            _logger = logger;
            _contextAdv = contextAdv;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreditCard>>> GetAllCreditCards()
        {
            if (_contextBet.CreditCards == null)
            {
                return NotFound();
            }
            return await _contextBet.CreditCards.ToListAsync();
        }

        [BasicAuthorization]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreditCard>>> GetCreditCardsByCustomer()
        {
            if (_contextBet.CreditCards == null)
            {
                return NotFound();
            }

            (string emailAddress, string password) = BasicAuthenticationUtilities.GetUsernamePassword(Request.Headers["Authorization"].ToString());

            try
            {
                var customer = await _contextAdv.Customers.FirstAsync(c => c.EmailAddress == emailAddress);
                if (customer == null)
                    return BadRequest();
                var creditcards = await _contextBet.CreditCards.Where(c => c.CustomerId == customer.CustomerId).ToListAsync();
                return Ok(creditcards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Problem(ex.Message, statusCode: 500);
            }
        }

        [HttpPost]
        public async Task<ActionResult<CreditCard>> PostCreditCard(CreditCard creditCard)
        {
            if (_contextBet.CreditCards == null)
            {
                return Problem("Entity set 'BetacomioContext.CreditCards'  is null.");
            }
            if (_contextAdv.Customers == null)
            {
                return Problem("Entity set 'AdventureWorksLt2019Context.Customers'  is null.");
            }
            (string emailAddress, string password) = BasicAuthenticationUtilities.GetUsernamePassword(Request.Headers["Authorization"].ToString());


            try
            {
                var creditCardID = await _contextBet.CreditCards.Where(c =>
                c.ExpireDate==creditCard.ExpireDate && c.FirstName==creditCard.FirstName && c.Number==creditCard.Number && c.LastName==creditCard.LastName && c.Cvv==creditCard.Cvv)
                .Select(c => c.CreditCardId).FirstOrDefaultAsync();
                if (creditCardID == 0)
                {
                    await _contextBet.CreditCards.AddAsync(creditCard);
                    await _contextAdv.SaveChangesAsync();
                    creditCardID = creditCard.CreditCardId;
                }

                var customer = await _contextAdv.Customers.FirstAsync(c => c.EmailAddress == emailAddress);
                if (customer == null)
                    return BadRequest("customer address does not exist");
                creditCard.CustomerId = customer.CustomerId;
                _contextBet.CreditCards.Add(creditCard);
                await _contextBet.SaveChangesAsync();
                return Ok(creditCard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Problem(ex.Message, statusCode: 500);
            }
        }
    }
}
