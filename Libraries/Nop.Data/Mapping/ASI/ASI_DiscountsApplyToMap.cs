using Nop.Core.Domain.ASI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.ASI
{
    public partial class ASI_DiscountsApplyToMap :NopEntityTypeConfiguration<ASI_DiscountsApplyTo>
    {
        public ASI_DiscountsApplyToMap() {
            this.ToTable("ASI_DiscountsApplyTo");
            this.HasKey(c => c.Id);
        }
    }
}
