using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.ZohoDesk;

namespace Nop.Data.Mapping.ZohoDesk
{
    public partial class ZohoContactMap : NopEntityTypeConfiguration<ZohoContact>
    {
        public ZohoContactMap()
        {
            this.ToTable("ZohoContact");
            this.HasKey(c => c.Id);

        }
    }
}
