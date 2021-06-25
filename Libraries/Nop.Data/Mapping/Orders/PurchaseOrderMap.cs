using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.Orders
{
    public partial class PurchaseOrderMap : NopEntityTypeConfiguration<PurchaseOrder>
    {
        public PurchaseOrderMap() {
            this.ToTable("PurchaseOrder");
            this.HasKey(o => o.Id);
           

        }
    }
}
