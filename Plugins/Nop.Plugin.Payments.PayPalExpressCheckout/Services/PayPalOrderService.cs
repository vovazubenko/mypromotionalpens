﻿using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.PayPalExpressCheckout.Helpers;
using Nop.Plugin.Payments.PayPalExpressCheckout.PayPalAPI;
using Nop.Services.Common;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using System;

namespace Nop.Plugin.Payments.PayPalExpressCheckout.Services
{
    public class PayPalOrderService : IPayPalOrderService
    {
        private readonly IWorkContext _workContext;
        private readonly PayPalExpressCheckoutPaymentSettings _payPalExpressCheckoutPaymentSettings;
        private readonly IPayPalCurrencyCodeParser _payPalCurrencyCodeParser;
        private readonly IPayPalCartItemService _payPalCartItemService;
        private readonly IShippingService _shippingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;

        public PayPalOrderService(IWorkContext workContext,
                                  PayPalExpressCheckoutPaymentSettings payPalExpressCheckoutPaymentSettings,
                                  IPayPalCurrencyCodeParser payPalCurrencyCodeParser,
                                  IPayPalCartItemService payPalCartItemService,
                                  IShippingService shippingService,
                                  IGenericAttributeService genericAttributeService,
                                  IStoreContext storeContext,
                                  ICheckoutAttributeParser checkoutAttributeParser)
        {
            _workContext = workContext;
            _payPalExpressCheckoutPaymentSettings = payPalExpressCheckoutPaymentSettings;
            _payPalCurrencyCodeParser = payPalCurrencyCodeParser;
            _payPalCartItemService = payPalCartItemService;
            _shippingService = shippingService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _checkoutAttributeParser = checkoutAttributeParser;
        }

        public PaymentDetailsType[] GetPaymentDetails(IList<ShoppingCartItem> cart)
        {
            var currencyCode = _payPalCurrencyCodeParser.GetCurrencyCodeType(_workContext.WorkingCurrency);

            decimal orderTotalDiscountAmount;
            List<DiscountForCaching> appliedDiscounts;
            int redeemedRewardPoints;
            decimal redeemedRewardPointsAmount;
            List<AppliedGiftCard> appliedGiftCards;
            var orderTotalWithDiscount = _payPalCartItemService.GetCartTotal(cart, out orderTotalDiscountAmount,
                out appliedDiscounts,
                out redeemedRewardPoints,
                out redeemedRewardPointsAmount,
                out appliedGiftCards);

            decimal subTotalWithDiscount;
            decimal subTotalWithoutDiscount;
            List<DiscountForCaching> subTotalAppliedDiscounts;
            decimal subTotalDiscountAmount;
            var itemTotalWithDiscount = _payPalCartItemService.GetCartItemTotal(cart,
                out subTotalDiscountAmount,
                out subTotalAppliedDiscounts,
                out subTotalWithoutDiscount,
                out subTotalWithDiscount);

            var giftCardsAmount = appliedGiftCards.Sum(x => x.AmountCanBeUsed);

            itemTotalWithDiscount = itemTotalWithDiscount - orderTotalDiscountAmount - giftCardsAmount;

            var taxTotal = _payPalCartItemService.GetTax(cart);
            var shippingTotal = _payPalCartItemService.GetShippingTotal(cart);
            var items = GetPaymentDetailsItems(cart);

            //calculate setup
            foreach (var item in cart) {
                var setupCost = new PaymentDetailsItemType
                {
                    Name ="SetupFee for"+item.Product.Name,
                    Amount =Convert.ToDecimal(item.SetupFee).GetBasicAmountType(currencyCode),
                    Quantity = "1"
                };
                items.Add(setupCost);
            }

            // checkout attributes
            var customer = cart.GetCustomer();
            if (customer != null)
            {
                var checkoutAttributesXml = customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
                var caValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttributesXml);
                if (caValues != null)
                {
                    foreach (var caValue in caValues)
                    {
                        if (caValue.PriceAdjustment > 0)
                        {
                            var checkoutAttrItem = new PaymentDetailsItemType
                            {
                                Name = caValue.Name,
                                Amount = caValue.PriceAdjustment.GetBasicAmountType(currencyCode),
                                Quantity = "1"
                            };
                            items.Add(checkoutAttrItem);
                        }
                    }
                }
            }
            if (orderTotalDiscountAmount > 0 || subTotalDiscountAmount > 0)
            {
                var discountItem = new PaymentDetailsItemType
                                       {
                                           Name = "Discount",
                                           Amount =
                                               (-orderTotalDiscountAmount + -subTotalDiscountAmount).GetBasicAmountType(
                                                   currencyCode),
                                           Quantity = "1"
                                       };

                items.Add(discountItem);
            }

            foreach (var appliedGiftCard in appliedGiftCards)
            {
                var giftCardItem = new PaymentDetailsItemType
                                       {
                                           Name = string.Format("Gift Card ({0})", appliedGiftCard.GiftCard.GiftCardCouponCode),
                                           Amount = (-appliedGiftCard.AmountCanBeUsed).GetBasicAmountType(currencyCode),
                                           Quantity = "1"
                                       };

                items.Add(giftCardItem);

            }

            return new[]
            {
                new PaymentDetailsType
                    {
                        OrderTotal = orderTotalWithDiscount.GetBasicAmountType(currencyCode),
                        ItemTotal = itemTotalWithDiscount.GetBasicAmountType(currencyCode),
                        TaxTotal = taxTotal.GetBasicAmountType(currencyCode),
                        ShippingTotal = shippingTotal.GetBasicAmountType(currencyCode),
                        PaymentDetailsItem = items.ToArray(),
                        PaymentAction = _payPalExpressCheckoutPaymentSettings.PaymentAction,
                        PaymentActionSpecified = true,
                        ButtonSource = PayPalHelper.BnCode
                    }
            };
        }

        public BasicAmountType GetMaxAmount(IList<ShoppingCartItem> cart)
        {
            var getShippingOptionResponse = _shippingService.GetShippingOptions(cart, _workContext.CurrentCustomer.ShippingAddress);
            decimal toAdd = 0;
            if (getShippingOptionResponse.ShippingOptions != null && getShippingOptionResponse.ShippingOptions.Any())
            {
                toAdd = getShippingOptionResponse.ShippingOptions.Max(option => option.Rate);
            }
            var currencyCode = _payPalCurrencyCodeParser.GetCurrencyCodeType(_workContext.WorkingCurrency);
            var cartTotal = _payPalCartItemService.GetCartItemTotal(cart);
            return (cartTotal + toAdd).GetBasicAmountType(currencyCode);
        }

        private IList<PaymentDetailsItemType> GetPaymentDetailsItems(IList<ShoppingCartItem> cart)
        {
            return cart.Select(item => _payPalCartItemService.CreatePaymentItem(item)).ToList();
        }

        public string GetBuyerEmail()
        {
            return _workContext.CurrentCustomer != null
                       ? _workContext.CurrentCustomer.Email
                       : null;
        }
    }
}