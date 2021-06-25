using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
    public partial class PurchaseOrderItem:BaseEntity
    {
        public int Po_id { get; set; }

        public string PONumber { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public int? Quantity { get; set; }

        public decimal? Price { get; set; }

        public int? QuantityReceived { get; set; }

        public decimal? ShippingCost { get; set; }

        public int? OrderDetailId { get; set; }

        public string AttributeDescription { get; set; }

        public string AttributesXml { get; set; }

        public DateTime? UpdateDate { get; set; }

        public DateTime? CreateDate { get; set; }

        public virtual PurchaseOrder PurchaseOrder { get; set; }

        public decimal? Subtotal { get; set; }
    }
}
