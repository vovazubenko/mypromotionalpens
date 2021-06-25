using Nop.Core.Domain.ZohoDesk;

namespace Nop.Data.Mapping.ZohoDesk
{
    public partial class ZohoTicketThreadMap : NopEntityTypeConfiguration<ZohoTicketThread>
    {
        public ZohoTicketThreadMap() {
            this.ToTable("ZohoTicketThread");
            this.HasKey(c => c.Id);
        }
    }
}
