using Nop.Core.Domain.Configuration;

namespace Nop.Data.Mapping.Configuration
{
    public partial class RestrictionDomainMap : NopEntityTypeConfiguration<RestrictionDomain> 
    {
        public RestrictionDomainMap()
        {
            this.ToTable("RestrictionDomain");
            this.HasKey(d => d.Id);
            this.Property(d => d.DomainURL).IsRequired().HasMaxLength(400);
        }
    }
}
