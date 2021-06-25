using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.ASI;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nop.Data.Mapping.ASI
{
    public partial class ASI_ProductsCSVGenerationRequestsMap : NopEntityTypeConfiguration<ASI_ProductsCSVGenerationRequests>
    {
        public ASI_ProductsCSVGenerationRequestsMap() {

            HasKey(t => t.Id);
            Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            ToTable("ASI_ProductsCSVGenerationRequests");

           
        }
    }
}
