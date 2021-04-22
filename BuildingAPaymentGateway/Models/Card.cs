using System;

namespace PaymentGateway.API.Models
{
    public class Card
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CardNumber { get; set; }
        public string Expiry { get; set; }
        public string Cvv { get; set; }
    }
}
