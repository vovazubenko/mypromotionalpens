using System.Collections.Generic;

namespace Nop.Core.Domain.Catalog
{
    public class ProductTierPriceVM
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public bool Published { get; set; }
        public string SKU { get; set; }
        public int OrderMinimumQuantity { get; set; }
        public decimal Setup { get; set; }
        public decimal SetupCost { get; set; }
        public List<TierPrice> TierPrices { get; set; }
    }
}