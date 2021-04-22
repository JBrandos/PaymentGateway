using PaymentGateway.API.Models;
using PaymentGateway.API.Responses;
using System;

namespace PaymentGateway.API
{
    public interface IBankResponseNormaliser
    {
        AcquiringBankResponse Normalise(string response);
    }

    public class BankResponseNormaliser : IBankResponseNormaliser
    {
        public AcquiringBankResponse Normalise(string response)
        {
            // here code would go to normalise the likely different responses received from different banks into an AcquiringBankResponse
            var random = new Random();
            var typeOfAcquiringBankResponse = random.Next(0, 3);            
            switch (typeOfAcquiringBankResponse)
            {
                case 0:
                    return new AcquiringBankResponse(Guid.NewGuid(), PaymentStatus.BankValidated, response, DateTime.UtcNow);
                case 1:
                    return new AcquiringBankResponse(Guid.NewGuid(), PaymentStatus.BankRejected, response, DateTime.UtcNow);
                default:
                    return new AcquiringBankResponse(Guid.NewGuid(), PaymentStatus.BankUnavailable, response, DateTime.UtcNow);                    
            }
        }
    }
}
