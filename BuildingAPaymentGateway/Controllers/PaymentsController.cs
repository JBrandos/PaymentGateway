using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.API.Commands;
using PaymentGateway.API.Filters;
using PaymentGateway.API.Queries;
using PaymentGateway.API.Request;
using System;
using System.Threading.Tasks;

namespace BuildingAPaymentGateway.Controllers
{
    //[Authorize]
    [ApiKeyAuth]
    [ApiController]
    [Route("[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request, [FromServices] IMapper mapper)
        {
            var command = mapper.Map<CreatePaymentCommand>(request);
            var result = await _mediator.Send(command);
            return CreatedAtAction("GetPayment", new { paymentId = result.PaymentId }, result);
        }

        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetPayment(Guid paymentId)
        {
            var query = new GetPaymentByIdQuery(paymentId);
            var result = await _mediator.Send(query);
            return result != null ? (IActionResult) Ok(result) : NotFound();
        }
    }
}
