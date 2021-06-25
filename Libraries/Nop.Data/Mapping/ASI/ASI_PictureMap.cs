using Nop.Core.Domain.ASI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.ASI
{
    public partial class ASI_PictureMap : NopEntityTypeConfiguration<ASI_Picture>
    {
        public ASI_PictureMap() {
            this.HasKey(x => x.Id);
            this.ToTable("ASI_Picture");
        }
    }
}
