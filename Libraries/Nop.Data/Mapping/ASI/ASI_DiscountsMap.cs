using Nop.Core.Domain.ASI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.ASI
{
    public partial class ASI_DiscountsMap : NopEntityTypeConfiguration<ASI_Discounts>
    {
        public ASI_DiscountsMap()
        {
            this.ToTable("ASI_Discounts");
            this.HasKey(c => c.Id);
        }
    }
}
