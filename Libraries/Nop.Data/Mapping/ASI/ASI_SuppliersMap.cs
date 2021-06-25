using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.ASI;

namespace Nop.Data.Mapping.ASI
{
    public partial class ASI_SuppliersMap : NopEntityTypeConfiguration<ASI_Suppliers>
    {
        public ASI_SuppliersMap() {
            this.ToTable("ASI_Suppliers");
            this.HasKey(x => x.Id);
        }
    }
}
