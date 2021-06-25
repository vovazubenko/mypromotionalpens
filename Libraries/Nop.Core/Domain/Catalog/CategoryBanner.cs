using Nop.Core.Domain.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    public partial class CategoryBanner : BaseEntity
    {
        public int CategoryId { get; set; }
        public int BannerId { get; set; }
        public int DisplayOrder { get; set; }
        public int BannerNumber { get; set; }
        public string BannerUrl { get; set; }
        public virtual Picture Banner { get; set; }
        public virtual Category Category { get; set; }
    }
}
