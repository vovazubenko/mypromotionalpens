using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.ASI;

namespace Nop.Data.Mapping.ASI
{
    public class ASI_ProductsAddedToCSVMap :NopEntityTypeConfiguration<ASI_ProductsAddedToCSV>
    {
        public ASI_ProductsAddedToCSVMap() {
            this.ToTable("ASI_ProductsAddedToCSV");
            this.HasKey(x=>x.Id);
        }
    }
}
