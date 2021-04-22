using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PaymentGateway.API.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.API.Models
{
    public class PaymentRejected
    {
        public AcquiringBankResponse Response { get; set; }

        public PaymentRejected(AcquiringBankResponse response)
        {
            Response = response;
        }

    }
}
