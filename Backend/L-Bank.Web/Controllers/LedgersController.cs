using System.Security.Claims;
using L_Bank_W_Backend.DbAccess;
using L_Bank_W_Backend.DbAccess.Repositories;
using L_Bank_W_Backend.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace L_Bank_W_Backend.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LedgersController : ControllerBase
    {
        private readonly ILedgerRepository ledgerRepository;

        public LedgersController(ILedgerRepository ledgerRepository)
        {
            this.ledgerRepository = ledgerRepository;
        }

        [HttpGet]
        [Authorize(Roles = "Administrators,Users")]
        public async Task<IEnumerable<Ledger>> Get()
        {
            var allLedgers = await this.ledgerRepository.GetAllLedgers();
            return allLedgers;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrators,Users")]
        public Ledger? Get(int id)
        {
            var ledger = this.ledgerRepository.SelectOne(id);
            return ledger;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrators")]
        public void Put(int id, [FromBody] Ledger ledger)
        {
            this.ledgerRepository.Update(ledger);
        }

        // Controller, um ein neues Ledger zu erstellen
        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Post([FromBody] Ledger ledger)
        {
            if (ledger == null || string.IsNullOrWhiteSpace(ledger.Name))
            {
                return BadRequest("Invalid ledger data.");
            }

            try
            {
                var ledgerId = await this.ledgerRepository.AddLedger(ledger);
                return CreatedAtAction(nameof(Get), new { id = ledgerId }, ledger);
            }
            catch
            {
                return StatusCode(500, "An error occurred while creating the ledger.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrators")]
        public IActionResult Delete(int id)
        {
            this.ledgerRepository.DeleteLedger(id);
            return Ok();
        }

        [HttpGet("totalBalance")]
        [Authorize(Roles = "Administrators,Users")]
        public decimal GetTotalBalance()
        {
            return this.ledgerRepository.GetTotalMoney();
        }
    }
}
