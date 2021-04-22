using MediatR;
using PaymentGateway.API.Models;
using PaymentGateway.API.Responses;

namespace PaymentGateway.API.Commands
{
    public class CreatePaymentCommand : IRequest<PaymentResponse>
    {
        public Card Card { get; set; }
        public Amount Amount { get; set; }
    }
}
