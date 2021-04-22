using PaymentGateway.API.Responses;


namespace PaymentGateway.API.Models
{
    public class PaymentSuccessful
    {
        public AcquiringBankResponse Response { get; set; }

        public PaymentSuccessful(AcquiringBankResponse response)
        {
            Response = response;
        }

    }
}
