using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Main.Data;
using Main.Models;

namespace Main.Controllers
{
    [Route("api/[controller]")]
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

        // GET: api/CreditCards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreditCard>>> GetCreditCards()
        {
            if (_contextBet.CreditCards == null)
            {
                return NotFound();
            }
            return await _contextBet.CreditCards.ToListAsync();
        }

        // GET: api/CreditCards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CreditCard>> GetCreditCard(int id)
        {
            if (_contextBet.CreditCards == null)
            {
                return NotFound();
            }
            var creditCard = await _contextBet.CreditCards.FindAsync(id);

            if (creditCard == null)
            {
                return NotFound();
            }

            return creditCard;
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


            try
            {
                var customer = await _contextAdv.Customers.FindAsync(creditCard.CustomerId);
                if (customer == null)
                    return BadRequest("customerID does not exist");

                _contextBet.CreditCards.Add(creditCard);
                await _contextBet.SaveChangesAsync();
                return CreatedAtAction("GetCreditCard", new { id = creditCard.CreditCardId }, creditCard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return BadRequest();
            }

        }
    }
}
