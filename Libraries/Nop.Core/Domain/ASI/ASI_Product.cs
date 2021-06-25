using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.ASI
{
    public class ASI_Product : BaseEntity
    {
        public string HideProduct { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string FullDescription { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public int MinStockQuantity { get; set; }

        public int OrderMinimumQuantity { get; set; }

        public int OrderMaximumQuantity { get; set; }

        public bool HasDiscountsApplied { get; set; }

        public decimal? SetupFee { get; set; }


        public string Material { get; set; }

        public string Size { get; set; }

        public string ImprintType { get; set; }

        public string ImprintArea { get; set; }

        public string InkColor { get; set; }

        public string MultiColorImprintAvailable { get; set; }

        public string PriceIncludes { get; set; }

        public string NormalProductionDays { get; set; }

        public string RushProductionDays { get; set; }
        public decimal? Price { get; set; }

        public int requestId { get; set; }

        public string ProductManufacturer { get; set;}
        public DateTime? AddedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string shortDescription { get; set; }

        public int isProductExist { get; set; }

        public decimal Weight { get; set; }
        
        public decimal Length { get; set; }
        
        public decimal Width { get; set; }
        
        public decimal Height { get; set; }
    }
}
