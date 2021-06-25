using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
    public partial class PurchaseOrder :BaseEntity
    {

        private ICollection<PurchaseOrderItem> _purchaseOrderItems;

        public Guid POGuid { get; set; }

        public string PONumber { get; set; }

        public int OrderId { get; set; }

        public DateTime? PoDate { get; set; }

        public int VendorId { get; set; }

        public string VendorTitle { get; set; }

        public string VendorAddress { get; set; }

        public string VendorCity { get; set; }

        public string VendorState { get; set; }

        public string VendorPostalCode { get; set; }

        public string VendorEmail { get; set; }

        public string POShipVia { get; set; }

        public string POTerm { get; set; }

        public string PONotes { get; set; }

        public string POAuthorizedBy { get; set; }

        public decimal? POShippingCost { get; set; }

        public DateTime? PODeliveryDate { get; set; }

        public DateTime? POCreateDate { get; set; }

        public virtual Order Order { get; set; }
        public virtual Vendor Vendor { get; set; }

        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems
        {
            get { return _purchaseOrderItems ?? (_purchaseOrderItems = new List<PurchaseOrderItem>()); }
            protected set { _purchaseOrderItems = value; }
        }


    }
}
