using Nop.Core.Domain.ZohoDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.ZohoDesk
{
    public partial class ZohoTicketMap : NopEntityTypeConfiguration<ZohoTicket>
    {
        public ZohoTicketMap()
        {
            this.ToTable("ZohoTicket");
            this.HasKey(c => c.Id);
        }
    }
}
