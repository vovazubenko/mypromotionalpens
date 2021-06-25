using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.ASI;

namespace Nop.Data.Mapping.ASI
{
    public partial class ASI_SuppliersUpdateStatusMap : NopEntityTypeConfiguration<ASI_SuppliersUpdateStatus>
    {
        public ASI_SuppliersUpdateStatusMap() {
            this.ToTable("ASI_SuppliersUpdateStatus");
            this.HasKey(x => x.Id);

        }
    }
}
