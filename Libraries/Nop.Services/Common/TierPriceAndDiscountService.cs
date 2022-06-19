using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Common.Models;
using Nop.Services.Directory;
using Nop.Services.Tax;

namespace Nop.Services.Common
{
    public class TierPriceAndDiscountService : ITierPriceAndDiscountService
    {
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;

        public TierPriceAndDiscountService(
            IStoreContext storeContext,
            IWorkContext workContext,
            ITaxService taxService,
            IPriceCalculationService priceCalculationService,
            CatalogSettings catalogSettings,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter)
        {
            _storeContext = storeContext;
            _workContext = workContext;
            _taxService = taxService;
            _priceCalculationService = priceCalculationService;
            _catalogSettings = catalogSettings;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
        }
        
        public List<TierPriceEntity> PrepareProductTierPriceModels(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            List<TierPriceEntity> model = product.TierPrices
                .OrderBy(x => x.Quantity)
                .FilterByStore(_storeContext.CurrentStore.Id)
                .FilterForCustomer(_workContext.CurrentCustomer)
                .FilterByDate()
                .RemoveDuplicatedQuantities()
                .Select(tierPrice => GetTierPriceEntity(product, tierPrice.Quantity,tierPrice.Id))
                .ToList();

            return model;
        }
        
        public List<DiscountRangeEntity> UpdateDiscountModelFromTierList(List<TierPriceEntity> tierList)
        {
            List<DiscountRangeEntity> newDiscountList = new List<DiscountRangeEntity>();

            if (tierList.Any())
            {
                var sortedList = tierList.OrderBy(x => x.Quantity).ToList();

                for (int i = 0; i < sortedList.Count; i++)
                {
                    DiscountRangeEntity discount = new DiscountRangeEntity()
                    {
                        DiscountID = sortedList[i].Id,
                        Amount = sortedList[i].PriceBase < 0 ? 0 : sortedList[i].PriceBase,
                        MinQty = sortedList[i].Quantity
                    };

                    if (sortedList.Count() == i + 1)
                        discount.MaxMiniQty = null;
                    else
                        discount.MaxMiniQty = sortedList[i + 1].Quantity - 1;

                    discount.Discount = "Discount_(" + discount.MinQty + "_" + discount.MaxMiniQty + ")";

                    newDiscountList.Add(discount);
                };
            }

            return newDiscountList;
        }

        public List<TierPriceEntity> GenerateTierPriceFromGeneralProductInfo(Product product)
        {
            TierPriceEntity ent = GetTierPriceEntity(product, product.OrderMinimumQuantity, 0);
            return new List<TierPriceEntity>() { ent };
        }

        public TierPriceEntity GetTierPriceEntity(Product product, int QTY, int tierId)
        {
            decimal taxRate;
            var priceBase = _taxService.GetProductPrice(
                    product, 
                    _priceCalculationService.GetFinalPrice(product,_workContext.CurrentCustomer, decimal.Zero, 
                        false, QTY), 
                    out taxRate);
            var price = _currencyService.ConvertFromPrimaryStoreCurrency(priceBase, _workContext.WorkingCurrency);
            var priceText = _priceFormatter.FormatPrice(price, false, false);

            return new TierPriceEntity
            {
                Id = tierId,
                PriceBase = price,
                Quantity = QTY,
                Price = priceText
            };
        }
    }
}