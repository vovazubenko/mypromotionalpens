using Nop.Core.Domain.Orders;

namespace Nop.Data.Mapping.Orders
{
    public partial class OrderTransactionsMap : NopEntityTypeConfiguration<OrderTransactions>
    {
        public OrderTransactionsMap() {
            this.ToTable("OrderTransactions");
            this.HasKey(ordertransactions => ordertransactions.Id);
            this.Property(ordertransactions => ordertransactions.TransferAmount).HasPrecision(18, 4);
            this.HasRequired(ordertransactions => ordertransactions.Order)
                .WithMany(o => o.OrderTransactions)
                .HasForeignKey(ordertransactions => ordertransactions.OrderId);
        }
    }
}
