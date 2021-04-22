using System;

namespace PaymentGateway.API.Models
{
    public class PaymentEvent
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTimeOffset CreatedDateTimeUTC { get; set; }
        public string EventData { get; set; }

        public PaymentEvent(Guid id, Guid paymentId, PaymentStatus status, DateTimeOffset createdDateTimeUTC, string data)
        {
            Id = id;
            PaymentId = paymentId;
            Status = status;
            CreatedDateTimeUTC = createdDateTimeUTC.UtcDateTime;
            EventData = data;
        }
    }    

    public enum PaymentStatus
    {
        Pending,
        BankValidated,
        BankRejected,
        BankUnavailable
    }
    
}
