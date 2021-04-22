using PaymentGateway.API.Models;

namespace PaymentGateway.API.Request
{
    public class PaymentRequest
    {
        public Card Card { get; set; }
        public Amount Amount { get; set; }
    }
}
