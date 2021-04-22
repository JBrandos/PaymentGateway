using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.API.Models
{
    public class Amount
    {
        public string Currency { get; set; }
        public decimal Value { get; set; }
    }
}
