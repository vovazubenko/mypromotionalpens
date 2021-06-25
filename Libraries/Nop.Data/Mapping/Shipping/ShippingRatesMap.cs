using Nop.Core.Domain.Shipping;

namespace Nop.Data.Mapping.Shipping
{
    public partial class ShippingRatesMap : NopEntityTypeConfiguration<ShippingRates>
    {
        public ShippingRatesMap()
        {
            this.ToTable("ShippingRates");
            this.HasKey(s => s.Id);
        }
    }
}
