using Nop.Core.Domain.ASI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.ASI
{
    public partial class ASI_ProductMap :NopEntityTypeConfiguration<ASI_Product>
    {
        public ASI_ProductMap()
        {
            this.ToTable("ASI_Product");
            this.HasKey(x => x.Id);
        }
    }
}
