using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.API.Models
{
    public class Merchant
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
