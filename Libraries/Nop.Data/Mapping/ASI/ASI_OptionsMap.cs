using Nop.Core.Domain.ASI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.ASI
{
    public partial class ASI_OptionsMap : NopEntityTypeConfiguration<ASI_Options>
    {
        public ASI_OptionsMap() {
            this.ToTable("ASI_Options");
            this.HasKey(x => x.Id);
        }
    }
}
