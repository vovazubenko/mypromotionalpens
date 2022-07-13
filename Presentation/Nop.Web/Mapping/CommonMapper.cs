using System.Collections.Generic;
using System.Linq;
using Nop.Services.Common.Models;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Mapping
{
    public static class CommonMapper
    {
        public static IList<ProductDetailsModel.TierPriceModel> MappingTierPriceModels(IList<TierPriceEntity> tierPriceEntities)
        {
            return tierPriceEntities
                .Select(tierPrice => new ProductDetailsModel.TierPriceModel()
                {
                    Id = tierPrice.Id,
                    Quantity = tierPrice.Quantity,
                    Price = tierPrice.Price,
                    PriceBase = tierPrice.PriceBase,
                    MSRP = tierPrice.MSRP
                })
                .ToList();
        }
        
        public static List<ProductDetailsModel.DiscountRange> MappingDiscountModels(List<DiscountRangeEntity> discountRangeEntities)
        {
            return discountRangeEntities
                .Select(discount => new ProductDetailsModel.DiscountRange()
                {
                    DiscountID = discount.DiscountID,
                    Discount = discount.Discount,
                    MinQty = discount.MinQty,
                    MaxMiniQty = discount.MaxMiniQty,
                    Amount = discount.Amount,
                    MSRP = discount.MSRP
                })
                .ToList();
        }
    }
}