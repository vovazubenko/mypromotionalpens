using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Services.Common.Models;

namespace Nop.Services.Common
{
    public interface ITierPriceAndDiscountService
    {
        /// <summary>
        /// Prepare the product tier price models
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>List of tier price model</returns>
        List<TierPriceEntity> PrepareProductTierPriceModels(Product product);

        /// <summary>
        /// Update discount model from tier list 
        /// </summary>
        /// <param name="tierList">Tier List Prices</param>
        /// <returns>List of updated discount models</returns>
        List<DiscountRangeEntity> UpdateDiscountModelFromTierList(List<TierPriceEntity> tierList);

        /// <summary>
        /// Prepare the product tier price from general product info
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>List of tier price model</returns>
        List<TierPriceEntity> GenerateTierPriceFromGeneralProductInfo(Product product);

        /// <summary>
        /// Prepare the product tier price 
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>Tier price entity</returns>
        TierPriceEntity GetTierPriceEntity(Product product, int QTY, int tierId);
    }
}