using PaymentGateway.API.Models;
using System;

namespace PaymentGateway.API.Responses
{
    public class AcquiringBankResponse
    {
        public Guid Id { get; }        
        public PaymentStatus Status {get; }
        public string BankResponse { get; }
        public DateTimeOffset CreatedDateTimeUTC { get; }

        public AcquiringBankResponse(Guid id, PaymentStatus status, string data, DateTime createdDateTimeUTC)
        {
            Id = id;
            Status = status;
            BankResponse = data;
            CreatedDateTimeUTC = createdDateTimeUTC; 
        }

    }
}
