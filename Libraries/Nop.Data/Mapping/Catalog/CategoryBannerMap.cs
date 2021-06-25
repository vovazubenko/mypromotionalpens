using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.Catalog
{
    public partial class CategoryBannerMap :NopEntityTypeConfiguration<CategoryBanner>
    {
        public CategoryBannerMap()
        {
            this.ToTable("Category_Banner_Mapping");
            this.HasKey(pp => pp.Id);

            this.HasRequired(pp => pp.Banner)
                .WithMany()
                .HasForeignKey(pp => pp.BannerId);


            this.HasRequired(pp => pp.Category)
                .WithMany(p => p.CategoryBanners)
                .HasForeignKey(pp => pp.CategoryId);
        }
    }
}
