using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.ASI;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nop.Data.Mapping.ASI
{
    public partial class ASI_ProductsSearchOptionsMap : NopEntityTypeConfiguration<ASI_ProductsSearchOptions>
    {
        public ASI_ProductsSearchOptionsMap() {

            HasKey(t => t.Id);
            Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.ToTable("ASI_ProductsSearchOptions");
            this.HasRequired(x => x.Request)
            .WithMany(y => y.SearchOptions)
            .HasForeignKey(x => x.RequestId)
            .WillCascadeOnDelete(true);

        }
    }
}
