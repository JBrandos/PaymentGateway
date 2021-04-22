using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace PaymentGateway.API.Models
{
    public class PaymentCreated
    {
        public Guid PaymentId { get; private set; }
        public Merchant Merchant { get; private set; }
        public Bank Bank { get; private set; }
        public Card Card { get; private set; }
        public Amount Amount { get; set; }
        public DateTimeOffset CreatedDateTimeUTC { get; private set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentStatus Status = PaymentStatus.Pending;


        public PaymentCreated (Guid id, Merchant merchant, Bank bank, Card card, Amount amount)
        {
            PaymentId = id;
            Merchant = merchant;
            Bank = bank;
            Card = card;
            Amount = amount;
            CreatedDateTimeUTC = DateTime.UtcNow;
        }  
    }
}
