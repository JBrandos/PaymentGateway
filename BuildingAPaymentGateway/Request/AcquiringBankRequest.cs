using Newtonsoft.Json.Converters;
using PaymentGateway.API.Models;
using System;
using System.Text.Json.Serialization;

namespace PaymentGateway.API.Request
{
    public class AcquiringBankRequest
    {
        public Guid PaymentId { get; set; }
        public Card Card { get; set; }
        public Amount Amount { get; set; }
        public DateTimeOffset CreatedDateTimeUTC { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentStatus Status { get; set; }
    }
}
