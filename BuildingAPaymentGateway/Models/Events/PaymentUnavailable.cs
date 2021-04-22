using Newtonsoft.Json;
using PaymentGateway.API.Responses;

namespace PaymentGateway.API.Models
{
    public class PaymentUnavailable
    {
        public AcquiringBankResponse Response { get; set; }

        public PaymentUnavailable(AcquiringBankResponse response)
        {
            Response = response;
        }

    }
}
