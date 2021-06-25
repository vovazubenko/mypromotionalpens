using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Shipping
{
    public partial class ShippingRates : BaseEntity
    {
        public string CarrierId { get; set; }
        public bool IsDomestic { get; set; }
        public decimal Rate { get; set; }
        public string CarrierName { get; set; }

        public string ShippingCarrier { get; set; }
    }
}
