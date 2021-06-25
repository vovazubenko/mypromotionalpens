using Nop.Core.Domain.ASI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.ASI
{
    public partial class ASI_Product_Category_MappingMap :NopEntityTypeConfiguration<ASI_Product_Category_Mapping>
    {
        public ASI_Product_Category_MappingMap(){
            this.ToTable("ASI_Product_Category_Mapping");
            this.HasKey(x => x.Id);
            }
    }
}
