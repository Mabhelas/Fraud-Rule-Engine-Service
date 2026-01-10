using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Fraud_Rule_Engine_Service.Application.Commands;
using Fraud_Rule_Engine_Service.Application.Queries;
using Fraud_Rule_Engine_Service.Domain;

namespace Fraud_Rule_Engine_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints in this controller
    public class FraudController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FraudController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST api/fraud/transactions
        // Only callers in the 'Ingestor' or 'FraudAnalyst' roles may submit transactions
        [HttpPost("transactions")]
        [Authorize(Policy = "CanSubmit")]
        public async Task<IActionResult> PostTransaction([FromBody] TransactionEvent tx)
        {
            if (tx == null) return BadRequest("Transaction payload required.");
            var result = await _mediator.Send(new SubmitTransactionCommand(tx));
            return CreatedAtAction(nameof(GetFraudResult), new { id = result.Id }, result);
        }

        // GET api/fraud/results?onlyFraud=true
        // Only callers with the 'FraudAnalyst' (or Compliance) role may view results
        [HttpGet("results")]
        [Authorize(Policy = "CanViewResults")]
        public async Task<IActionResult> GetResults([FromQuery] bool? onlyFraud = null)
        {
            var results = await _mediator.Send(new GetFraudResultsQuery(onlyFraud));
            return Ok(results);
        }

        // GET api/fraud/results/{id}
        [HttpGet("results/{id:guid}")]
        [Authorize(Policy = "CanViewResults")]
        public async Task<IActionResult> GetFraudResult(Guid id)
        {
            var result = await _mediator.Send(new GetFraudResultQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}