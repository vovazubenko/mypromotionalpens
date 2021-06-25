using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.Orders
{
   public partial class PurchaseOrderItemMap: NopEntityTypeConfiguration<PurchaseOrderItem>
    {
        public PurchaseOrderItemMap()
        {
            this.ToTable("PurchaseOrderItem");
            this.HasKey(o => o.Id);

            this.HasRequired(purchaseorderItem => purchaseorderItem.PurchaseOrder)
            .WithMany(o => o.PurchaseOrderItems)
            .HasForeignKey(purchaseorderItem => purchaseorderItem.Po_id);
        }
    }
}
