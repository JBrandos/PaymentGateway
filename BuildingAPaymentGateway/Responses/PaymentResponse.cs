using PaymentGateway.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaymentGateway.API.Responses
{
    public class PaymentResponse
    {
        public Guid PaymentId { get; private set; }
        public Card Card { get; private set; }
        public Amount Amount { get; private set; }
        public DateTimeOffset CreatedDateTimeUTC { get; private set; }
        public DateTimeOffset UpdatedDateTimeUTC { get; private set; }
        public PaymentStatus Status { get; private set; } //"..along with a status code which indicates the result of the payment."
                                                          //I think this sent as a string would be more readable but the spec says "status code"

        public PaymentResponse(List<PaymentEvent> paymentEvents)
        {            
            var paymentCreated = Newtonsoft.Json.JsonConvert.DeserializeObject<PaymentCreated>(paymentEvents.Find(x => x.Status == PaymentStatus.Pending).EventData);
            var latestEvent = paymentEvents.OrderByDescending(x => x.CreatedDateTimeUTC).FirstOrDefault();

            PaymentId = latestEvent.PaymentId;
            Card = paymentCreated.Card;
            Card.CardNumber = "XXXXXXXXXXXX" + paymentCreated.Card.CardNumber.Substring((paymentCreated.Card.CardNumber.Length -4), 4);
            Amount = paymentCreated.Amount;
            CreatedDateTimeUTC = paymentCreated.CreatedDateTimeUTC;
            UpdatedDateTimeUTC = paymentCreated.CreatedDateTimeUTC;

            if (latestEvent.Status != PaymentStatus.Pending)
            {
                UpdatedDateTimeUTC = latestEvent.CreatedDateTimeUTC;
                Status = latestEvent.Status;
            }
        }
    }    
}
