using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Helpers;
using Nop.Admin.Models.Orders;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Admin.Models.Vendors;
using Nop.Core.Domain.Vendors;
using Nop.Services.Customers;
using Nop.Web.Framework.Security;

namespace Nop.Admin.Controllers
{
    public class PurchaseOrderController : BaseAdminController
    {

        #region Fields
        private readonly IOrderService _orderService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly TaxSettings _taxSettings;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IAffiliateService _affiliateService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IDiscountService _discountService;
        private readonly IEncryptionService _encryptionService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly AddressSettings _addressSettings;
        private readonly IPictureService _pictureService;
        private readonly IGiftCardService _giftCardService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICacheManager _cacheManager;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICustomerService _customerService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IPdfService _pdfService;
        private readonly OrderSettings _orderSettings;



        #endregion


        #region ctor
        public PurchaseOrderController(IOrderService orderService,
            IPurchaseOrderService purchaseOrderService, IPermissionService permissionService
            , IWorkContext workContext, ILocalizationService localizationService,
            IStoreService storeService, IVendorService vendorService,
            IDateTimeHelper dateTimeHelper, TaxSettings taxSettings,
            IPriceFormatter priceFormatter,
            IAffiliateService affiliateService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IDiscountService discountService,
            IEncryptionService encryptionService,
            IPaymentService paymentService,
            IOrderProcessingService orderProcessingService,
            IAddressAttributeFormatter addressAttributeFormatter,
            AddressSettings addressSettings,
            IPictureService pictureService,
            IGiftCardService giftCardService,
            IProductAttributeParser productAttributeParser,
            ICacheManager cacheManager,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ICustomerService customerService,
            IWorkflowMessageService workflowMessageService,
            IPdfService pdfService,
            OrderSettings orderSettings
            )
        {

            this._orderService = orderService;
            this._purchaseOrderService = purchaseOrderService;
            this._permissionService = permissionService;
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._storeService = storeService;
            this._vendorService = vendorService;
            this._dateTimeHelper = dateTimeHelper;
            this._taxSettings = taxSettings;
            this._priceFormatter = priceFormatter;
            this._affiliateService = affiliateService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._discountService = discountService;
            this._encryptionService = encryptionService;
            this._paymentService = paymentService;
            this._orderProcessingService = orderProcessingService;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._addressSettings = addressSettings;
            this._pictureService = pictureService;
            this._giftCardService = giftCardService;
            this._productAttributeParser = productAttributeParser;
            this._cacheManager = cacheManager;
            this._addressService = addressService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._customerService = customerService;
            this._workflowMessageService = workflowMessageService;
            this._pdfService = pdfService;
            this._orderSettings = orderSettings;


        }
        #endregion

        public ActionResult Index()
        {
            return View();
        }



        public virtual ActionResult PurchaseOrder(int orderid)
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                    return AccessDeniedView();

                var order = _orderService.GetOrderById(orderid);

                if (order == null || order.Deleted)
                    return RedirectToAction("Edit", "Order", new { id = orderid });

                var purchaseOrder = _purchaseOrderService.GetPOByOrderId(orderid);
                //if (purchaseOrder == null)
                //    return RedirectToAction("Edit", "Order", new { id = orderid });

                var model = new PurchaseOrderModel();
                var ordermodel = new OrderModel();
                PrepareOrderDetailsModel(ordermodel, order);

                PreparePurchaseOrdeDetailsModel(model, purchaseOrder, ordermodel);

                return View(model);
            }
            catch (Exception ex)
            {
                ErrorNotification(ex, false);
                ErrorNotification(ex.Message, true);
                return RedirectToAction("Edit", "Order", new { id = orderid });
            }
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired(FormValueRequirement.StartsWith, "save")]
        public virtual ActionResult PurchaseOrder(PurchaseOrderModel model, bool continueEditing)
        {

            if (model.Id <= 0)
            {
                var po = new PurchaseOrder();
                try
                {
                    if (ModelState.IsValid)
                    {
                        po.PONumber = model.PONumber;
                        po.OrderId = model.OrderId;

                        po.PoDate = (model.PoDate == null ||
                        model.PoDate.Value.Date == ((DateTime?)_dateTimeHelper.ConvertToUserTime(Convert.ToDateTime(null), DateTimeKind.Utc)).Value.Date) ? null
                                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.PoDate.Value, _dateTimeHelper.CurrentTimeZone);

                        po.VendorId = model.VendorId;
                        po.VendorTitle = model.VendorTitle;
                        po.VendorAddress = model.VendorAddress;
                        po.VendorCity = model.VendorCity;
                        po.VendorState = model.VendorState;
                        po.VendorPostalCode = model.VendorPostalCode;
                        po.VendorEmail = model.VendorEmail;
                        po.POShipVia = model.POShipVia;
                        po.POTerm = model.POTerm;
                        po.PONotes = model.PONotes;
                        po.VendorTitle = model.VendorTitle;
                        po.POAuthorizedBy = model.POAuthorizedBy;
                        po.POShippingCost = model.POShippingCost;
                        po.PODeliveryDate = (model.PODeliveryDate == null ||
                        model.PODeliveryDate.Value.Date == ((DateTime?)_dateTimeHelper.ConvertToUserTime(Convert.ToDateTime(null), DateTimeKind.Utc)).Value.Date) ? null
                               : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.PODeliveryDate.Value, _dateTimeHelper.CurrentTimeZone);
                        model.POCreateDate = (DateTime?)_dateTimeHelper.ConvertToUtcTime(System.DateTime.Now, _dateTimeHelper.CurrentTimeZone);

                        var order = _orderService.GetOrderById(model.OrderId);
                        if (order != null)
                        {
                            foreach (var item in order.OrderItems)
                            {
                                var purchaseOrderItem = new PurchaseOrderItem()
                                {
                                    AttributeDescription = item.AttributeDescription,
                                    AttributesXml = item.AttributesXml,
                                    CreateDate = DateTime.Now,
                                    OrderDetailId = item.Id,
                                    PONumber = model.PONumber,
                                    Price = item.UnitPriceExclTax,
                                    ProductCode = item.Product.Sku,
                                    ProductName = item.Product.Name,
                                    Quantity = item.Quantity,
                                    QuantityReceived = 0,
                                    ShippingCost = 0,
                                    Subtotal = item.PriceExclTax,
                                    UpdateDate = DateTime.Now
                                };

                                po.PurchaseOrderItems.Add(purchaseOrderItem);
                            }
                        }
                        _purchaseOrderService.InsertPurchaseOrder(po);
                        SuccessNotification(_localizationService.GetResource("Admin.PurchaseOrder.Created"));

                    }
                    else {
                        var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                        ErrorNotification(message, true);
                    }
                }
                catch (Exception ex)
                {

                    ErrorNotification(ex, false);
                    ErrorNotification(ex.Message, true);
                }
            }
            else {
                try
                {
                    var po = _purchaseOrderService.GetPurchaseOrderById(model.Id);
                    if (po == null)
                        return RedirectToAction("Edit", "Order", new { Id = model.OrderId });
                    if (ModelState.IsValid)
                    {
                        po.PONumber = model.PONumber;
                        po.OrderId = model.OrderId;

                        po.PoDate = (model.PoDate == null ||
                        model.PoDate.Value.Date == ((DateTime?)_dateTimeHelper.ConvertToUserTime(Convert.ToDateTime(null), DateTimeKind.Utc)).Value.Date) ? null
                                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.PoDate.Value, _dateTimeHelper.CurrentTimeZone);

                        po.VendorId = model.VendorId;
                        po.VendorTitle = model.VendorTitle;
                        po.VendorAddress = model.VendorAddress;
                        po.VendorCity = model.VendorCity;
                        po.VendorState = model.VendorState;
                        po.VendorPostalCode = model.VendorPostalCode;
                        po.VendorEmail = model.VendorEmail;
                        po.POShipVia = model.POShipVia;
                        po.POTerm = model.POTerm;
                        po.PONotes = model.PONotes;
                        po.VendorTitle = model.VendorTitle;
                        po.POAuthorizedBy = model.POAuthorizedBy;
                        po.POShippingCost = model.POShippingCost;
                        po.PODeliveryDate = (model.PODeliveryDate == null ||
                        model.PODeliveryDate.Value.Date == ((DateTime?)_dateTimeHelper.ConvertToUserTime(Convert.ToDateTime(null), DateTimeKind.Utc)).Value.Date) ? null
                               : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.PODeliveryDate.Value, _dateTimeHelper.CurrentTimeZone);

                        _purchaseOrderService.UpdatePurchaseOrder(po);
                        SuccessNotification(_localizationService.GetResource("Admin.PurchaseOrder.updated"));

                    }
                    else {
                        var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                        ErrorNotification(message, true);
                    }
                }
                catch (Exception ex)
                {
                    ErrorNotification(ex, false);
                    ErrorNotification(ex.Message, true);
                }
            }


            if (continueEditing)
            {
                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("PurchaseOrder", new { orderid = model.OrderId });
            }

            return RedirectToAction("Edit", "Order", new { Id = model.OrderId });
        }

        [HttpPost]

        public virtual ActionResult Delete(int id)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var po = _purchaseOrderService.GetPurchaseOrderById(id);
            if (po == null)
                return RedirectToAction("Edit", "Order", new { id = po.OrderId });
            try
            {
                _purchaseOrderService.Deletepurchseorder(po);
                SuccessNotification(_localizationService.GetResource("Admin.PurchaseOrder.DeleteSuccess"));

            }
            catch (Exception ex)
            {
                ErrorNotification(ex, false);
                ErrorNotification(ex.Message, true);
            }

            return RedirectToAction("Edit", "Order", new { id = po.OrderId });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult GetVendorDetails(string vendorId)
        {
            //permission validation is not required here


            // This action method gets called via an ajax request
            if (String.IsNullOrEmpty(vendorId))
                throw new ArgumentNullException("vendorId");

            var vendor = _vendorService.GetVendorById(Convert.ToInt32(vendorId));
            var vendormodel = vendor.ToModel();
            PrepareVendorModel(vendormodel, vendor, false, true);
            return Json(vendormodel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ActionName("PurchaseOrder")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnSaveOrderItem")]
        [ValidateInput(false)]
        public virtual ActionResult EditOrderItem(int id, FormCollection form, int OrderId)
        {
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                    return AccessDeniedView();

                var pOrder = _purchaseOrderService.GetPurchaseOrderById(id);
                if (pOrder == null)
                    //No order found with the specified id
                    return RedirectToAction("Edit", "Order", new { Id = OrderId });


                //get order item identifier
                int orderItemId = 0;
                foreach (var formValue in form.AllKeys)
                    if (formValue.StartsWith("btnSaveOrderItem", StringComparison.InvariantCultureIgnoreCase))
                        orderItemId = Convert.ToInt32(formValue.Substring("btnSaveOrderItem".Length));

                var orderItem = pOrder.PurchaseOrderItems.FirstOrDefault(x => x.Id == orderItemId);
                if (orderItem == null)
                    throw new ArgumentException("No purchase order item found with the specified id");


                decimal price;
                int quantity, receivedQuantity;

                if (!decimal.TryParse(form["pvPrice" + orderItemId], out price))
                    price = Convert.ToDecimal(orderItem.Price);


                if (!int.TryParse(form["pvQuantity" + orderItemId], out quantity))
                    quantity = Convert.ToInt32(orderItem.Quantity);

                if (!int.TryParse(form["pvReceivedQuantity" + orderItemId], out receivedQuantity))
                    receivedQuantity = Convert.ToInt32(orderItem.QuantityReceived);

                if (quantity > 0)
                {
                    int qtyDifference = Convert.ToInt32(orderItem.Quantity) - quantity;

                    orderItem.Price = price;
                    orderItem.Quantity = quantity;
                    orderItem.QuantityReceived = receivedQuantity;
                    orderItem.Subtotal = quantity * price;
                    _purchaseOrderService.UpdatePurchaseOrderItem(orderItem);

                    pOrder = _purchaseOrderService.GetPurchaseOrderById(id);

                    SuccessNotification(_localizationService.GetResource("Admin.PurchaseOrder.updated"));
                }
                else {
                    ErrorNotification(_localizationService.GetResource("Admin.PurchaseOrder.PurchaseOrderItem.Quantity"));
                }
            }
            catch (Exception ex)
            {

                ErrorNotification(ex, false);
                ErrorNotification(ex.Message, true);
            }
            SaveSelectedTabName();
            return RedirectToAction("PurchaseOrder", new { orderid = OrderId });
            //return View(model);
        }

        [NonAction]
        protected virtual void PrepareOrderDetailsModel(OrderModel model, Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (model == null)
                throw new ArgumentNullException("model");

            model.Id = order.Id;
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.OrderStatusId = order.OrderStatusId;
            model.OrderGuid = order.OrderGuid;
            model.CustomOrderNumber = order.CustomOrderNumber;
            var store = _storeService.GetStoreById(order.StoreId);
            model.StoreName = store != null ? store.Name : "Unknown";
            model.CustomerId = order.CustomerId;
            var customer = order.Customer;
            model.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
            model.CustomerIp = order.CustomerIp;
            model.VatNumber = order.VatNumber;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
            model.AllowCustomersToSelectTaxDisplayType = _taxSettings.AllowCustomersToSelectTaxDisplayType;
            model.TaxDisplayType = _taxSettings.TaxDisplayType;

            var affiliate = _affiliateService.GetAffiliateById(order.AffiliateId);
            if (affiliate != null)
            {
                model.AffiliateId = affiliate.Id;
                model.AffiliateName = affiliate.GetFullName();
            }

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            //custom values
            model.CustomValues = order.DeserializeCustomValues();

            #region Order totals

            var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (primaryStoreCurrency == null)
                throw new Exception("Cannot load primary store currency");

            //subtotal
            model.OrderSubtotalInclTax = _priceFormatter.FormatPrice(order.OrderSubtotalInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            model.OrderSubtotalExclTax = _priceFormatter.FormatPrice(order.OrderSubtotalExclTax, true, _workContext.WorkingCurrency.CurrencyCode, false, _workContext.WorkingLanguage);
            model.OrderSubtotalInclTaxValue = order.OrderSubtotalInclTax;
            model.OrderSubtotalExclTaxValue = order.OrderSubtotalExclTax;
            //discount (applied to order subtotal)
            string orderSubtotalDiscountInclTaxStr = _priceFormatter.FormatPrice(order.OrderSubTotalDiscountInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            string orderSubtotalDiscountExclTaxStr = _priceFormatter.FormatPrice(order.OrderSubTotalDiscountExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            if (order.OrderSubTotalDiscountInclTax > decimal.Zero)
                model.OrderSubTotalDiscountInclTax = orderSubtotalDiscountInclTaxStr;
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
                model.OrderSubTotalDiscountExclTax = orderSubtotalDiscountExclTaxStr;
            model.OrderSubTotalDiscountInclTaxValue = order.OrderSubTotalDiscountInclTax;
            model.OrderSubTotalDiscountExclTaxValue = order.OrderSubTotalDiscountExclTax;

            //shipping
            model.OrderShippingInclTax = _priceFormatter.FormatShippingPrice(order.OrderShippingInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            model.OrderShippingExclTax = _priceFormatter.FormatShippingPrice(order.OrderShippingExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            model.OrderShippingInclTaxValue = order.OrderShippingInclTax;
            model.OrderShippingExclTaxValue = order.OrderShippingExclTax;

            //payment method additional fee
            if (order.PaymentMethodAdditionalFeeInclTax > decimal.Zero)
            {
                model.PaymentMethodAdditionalFeeInclTax = _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
                model.PaymentMethodAdditionalFeeExclTax = _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            }
            model.PaymentMethodAdditionalFeeInclTaxValue = order.PaymentMethodAdditionalFeeInclTax;
            model.PaymentMethodAdditionalFeeExclTaxValue = order.PaymentMethodAdditionalFeeExclTax;


            //tax
            model.Tax = _priceFormatter.FormatPrice(order.OrderTax, true, false);
            SortedDictionary<decimal, decimal> taxRates = order.TaxRatesDictionary;
            bool displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
            bool displayTax = !displayTaxRates;
            foreach (var tr in order.TaxRatesDictionary)
            {
                model.TaxRates.Add(new OrderModel.TaxRate
                {
                    Rate = _priceFormatter.FormatTaxRate(tr.Key),
                    Value = _priceFormatter.FormatPrice(tr.Value, true, false),
                });
            }
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            model.TaxValue = order.OrderTax;
            model.TaxRatesValue = order.TaxRates;

            //discount
            if (order.OrderDiscount > 0)
                model.OrderTotalDiscount = _priceFormatter.FormatPrice(-order.OrderDiscount, true, false);
            model.OrderTotalDiscountValue = order.OrderDiscount;

            //gift cards
            foreach (var gcuh in order.GiftCardUsageHistory)
            {
                model.GiftCards.Add(new OrderModel.GiftCard
                {
                    CouponCode = gcuh.GiftCard.GiftCardCouponCode,
                    Amount = _priceFormatter.FormatPrice(-gcuh.UsedValue, true, false),
                });
            }

            //reward points
            if (order.RedeemedRewardPointsEntry != null)
            {
                model.RedeemedRewardPoints = -order.RedeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-order.RedeemedRewardPointsEntry.UsedAmount, true, false);
            }

            //total
            model.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false);
            model.OrderTotalValue = order.OrderTotal;

            //refunded amount
            if (order.RefundedAmount > decimal.Zero)
                model.RefundedAmount = _priceFormatter.FormatPrice(order.RefundedAmount, true, false);

            //used discounts
            var duh = _discountService.GetAllDiscountUsageHistory(orderId: order.Id);
            foreach (var d in duh)
            {
                model.UsedDiscounts.Add(new OrderModel.UsedDiscountModel
                {
                    DiscountId = d.DiscountId,
                    DiscountName = d.Discount.Name
                });
            }



            #endregion

            #region Payment info

            if (order.AllowStoringCreditCardNumber)
            {
                try
                {
                    //card type
                    if (order.CardType != "")
                        model.CardType = _encryptionService.DecryptText(order.CardType);
                    //cardholder name
                    if (order.CardName != "")
                        model.CardName = _encryptionService.DecryptText(order.CardName);
                    //card number

                    if (order.CardNumber != "")
                        model.CardNumber = _encryptionService.DecryptText(order.CardNumber);
                    //cvv
                    if (order.CardCvv2 != "")
                        model.CardCvv2 = _encryptionService.DecryptText(order.CardCvv2);
                    //expiry date

                    if (order.CardExpirationMonth != "")
                    {
                        string cardExpirationMonthDecrypted = _encryptionService.DecryptText(order.CardExpirationMonth);
                        if (!String.IsNullOrEmpty(cardExpirationMonthDecrypted) && cardExpirationMonthDecrypted != "0")
                            model.CardExpirationMonth = cardExpirationMonthDecrypted;
                        string cardExpirationYearDecrypted = _encryptionService.DecryptText(order.CardExpirationYear);
                        if (!String.IsNullOrEmpty(cardExpirationYearDecrypted) && cardExpirationYearDecrypted != "0")
                            model.CardExpirationYear = cardExpirationYearDecrypted;
                    }
                }
                catch (Exception)
                {
                }

                model.AllowStoringCreditCardNumber = true;
            }
            else
            {
                try
                {

                    string maskedCreditCardNumberDecrypted = _encryptionService.DecryptText(order.MaskedCreditCardNumber);
                    if (!String.IsNullOrEmpty(maskedCreditCardNumberDecrypted))
                        model.CardNumber = maskedCreditCardNumberDecrypted;

                }
                catch (Exception)
                {

                }
            }


            //payment transaction info
            model.AuthorizationTransactionId = order.AuthorizationTransactionId;
            model.CaptureTransactionId = order.CaptureTransactionId;
            model.SubscriptionTransactionId = order.SubscriptionTransactionId;

            //payment method info
            var pm = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            model.PaymentMethod = pm != null ? pm.PluginDescriptor.FriendlyName : order.PaymentMethodSystemName;
            model.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.PaymentStatusId = order.PaymentStatusId;
            //payment method buttons
            model.CanCancelOrder = _orderProcessingService.CanCancelOrder(order);
            model.CanCapture = _orderProcessingService.CanCapture(order);
            model.CanMarkOrderAsPaid = _orderProcessingService.CanMarkOrderAsPaid(order);
            model.CanRefund = _orderProcessingService.CanRefund(order);
            model.CanRefundOffline = _orderProcessingService.CanRefundOffline(order);
            model.CanPartiallyRefund = _orderProcessingService.CanPartiallyRefund(order, decimal.Zero);
            model.CanPartiallyRefundOffline = _orderProcessingService.CanPartiallyRefundOffline(order, decimal.Zero);
            model.CanVoid = _orderProcessingService.CanVoid(order);
            model.CanVoidOffline = _orderProcessingService.CanVoidOffline(order);

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.MaxAmountToRefund = order.OrderTotal - order.RefundedAmount;

            //recurring payment record
            var recurringPayment = _orderService.SearchRecurringPayments(initialOrderId: order.Id, showHidden: true).FirstOrDefault();
            if (recurringPayment != null)
            {
                model.RecurringPaymentId = recurringPayment.Id;
            }
            #endregion

            #region Billing & shipping info

            model.BillingAddress = order.BillingAddress.ToModel();
            model.BillingAddress.FormattedCustomAddressAttributes = _addressAttributeFormatter.FormatAttributes(order.BillingAddress.CustomAttributes);
            model.BillingAddress.FirstNameEnabled = true;
            model.BillingAddress.FirstNameRequired = true;
            model.BillingAddress.LastNameEnabled = true;
            model.BillingAddress.LastNameRequired = true;
            model.BillingAddress.EmailEnabled = true;
            model.BillingAddress.EmailRequired = true;
            model.BillingAddress.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.BillingAddress.CompanyRequired = _addressSettings.CompanyRequired;
            model.BillingAddress.CountryEnabled = _addressSettings.CountryEnabled;
            model.BillingAddress.CountryRequired = _addressSettings.CountryEnabled; //country is required when enabled
            model.BillingAddress.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.BillingAddress.CityEnabled = _addressSettings.CityEnabled;
            model.BillingAddress.CityRequired = _addressSettings.CityRequired;
            model.BillingAddress.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.BillingAddress.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.BillingAddress.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.BillingAddress.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.BillingAddress.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.BillingAddress.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.BillingAddress.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.BillingAddress.PhoneRequired = _addressSettings.PhoneRequired;
            model.BillingAddress.FaxEnabled = _addressSettings.FaxEnabled;
            model.BillingAddress.FaxRequired = _addressSettings.FaxRequired;

            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext); ;
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;

                model.PickUpInStore = order.PickUpInStore;
                if (!order.PickUpInStore)
                {
                    model.ShippingAddress = order.ShippingAddress.ToModel();
                    model.ShippingAddress.FormattedCustomAddressAttributes = _addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes);
                    model.ShippingAddress.FirstNameEnabled = true;
                    model.ShippingAddress.FirstNameRequired = true;
                    model.ShippingAddress.LastNameEnabled = true;
                    model.ShippingAddress.LastNameRequired = true;
                    model.ShippingAddress.EmailEnabled = true;
                    model.ShippingAddress.EmailRequired = true;
                    model.ShippingAddress.CompanyEnabled = _addressSettings.CompanyEnabled;
                    model.ShippingAddress.CompanyRequired = _addressSettings.CompanyRequired;
                    model.ShippingAddress.CountryEnabled = _addressSettings.CountryEnabled;
                    model.ShippingAddress.CountryRequired = _addressSettings.CountryEnabled; //country is required when enabled
                    model.ShippingAddress.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
                    model.ShippingAddress.CityEnabled = _addressSettings.CityEnabled;
                    model.ShippingAddress.CityRequired = _addressSettings.CityRequired;
                    model.ShippingAddress.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
                    model.ShippingAddress.StreetAddressRequired = _addressSettings.StreetAddressRequired;
                    model.ShippingAddress.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
                    model.ShippingAddress.StreetAddress2Required = _addressSettings.StreetAddress2Required;
                    model.ShippingAddress.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
                    model.ShippingAddress.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
                    model.ShippingAddress.PhoneEnabled = _addressSettings.PhoneEnabled;
                    model.ShippingAddress.PhoneRequired = _addressSettings.PhoneRequired;
                    model.ShippingAddress.FaxEnabled = _addressSettings.FaxEnabled;
                    model.ShippingAddress.FaxRequired = _addressSettings.FaxRequired;

                    model.ShippingAddressGoogleMapsUrl = string.Format("http://maps.google.com/maps?f=q&hl=en&ie=UTF8&oe=UTF8&geocode=&q={0}", Server.UrlEncode(order.ShippingAddress.Address1 + " " + order.ShippingAddress.ZipPostalCode + " " + order.ShippingAddress.City + " " + (order.ShippingAddress.Country != null ? order.ShippingAddress.Country.Name : "")));
                }
                else
                {
                    if (order.PickupAddress != null)
                    {
                        model.PickupAddress = order.PickupAddress.ToModel();
                        model.PickupAddressGoogleMapsUrl = string.Format("http://maps.google.com/maps?f=q&hl=en&ie=UTF8&oe=UTF8&geocode=&q={0}",
                            Server.UrlEncode(string.Format("{0} {1} {2} {3}", order.PickupAddress.Address1, order.PickupAddress.ZipPostalCode, order.PickupAddress.City,
                                order.PickupAddress.Country != null ? order.PickupAddress.Country.Name : string.Empty)));
                    }
                }
                model.ShippingMethod = order.ShippingMethod;

                model.CanAddNewShipments = order.HasItemsToAddToShipment();
            }

            #endregion

            #region Products

            model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;
            bool hasDownloadableItems = false;
            var products = order.OrderItems;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                products = products
                    .Where(orderItem => orderItem.Product.VendorId == _workContext.CurrentVendor.Id)
                    .ToList();
            }
            foreach (var orderItem in products)
            {
                if (orderItem.Product.IsDownload)
                    hasDownloadableItems = true;

                var orderItemModel = new OrderModel.OrderItemModel
                {
                    Id = orderItem.Id,
                    ProductId = orderItem.ProductId,
                    ProductName = orderItem.Product.Name,
                    Sku = orderItem.Product.Sku,
                    Quantity = orderItem.Quantity,
                    IsDownload = orderItem.Product.IsDownload,
                    DownloadCount = orderItem.DownloadCount,
                    DownloadActivationType = orderItem.Product.DownloadActivationType,
                    IsDownloadActivated = orderItem.IsDownloadActivated,
                    SetupFee = _priceFormatter.FormatPrice(Convert.ToDecimal(orderItem.SetupFee), true, primaryStoreCurrency, _workContext.WorkingLanguage, true, false),
                    SetupFeeValue = Convert.ToDecimal(orderItem.SetupFee)
                };
                //picture
                var orderItemPicture = orderItem.Product.GetProductPicture(orderItem.AttributesXml, _pictureService, _productAttributeParser);
                orderItemModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(orderItemPicture, 75, true);

                orderItemModel.CustomPictureThumbnailUrl = _pictureService.GetPictureUrl(orderItem.PictureId == null ? 0 : Convert.ToInt32(orderItem.PictureId), 75, true);


                //vendor
                var vendor = _vendorService.GetVendorById(orderItem.Product.VendorId);
                orderItemModel.VendorName = vendor != null ? vendor.Name : "";

                //unit price
                orderItemModel.UnitPriceInclTaxValue = orderItem.UnitPriceInclTax;
                orderItemModel.UnitPriceExclTaxValue = orderItem.UnitPriceExclTax;
                orderItemModel.UnitPriceInclTax = _priceFormatter.FormatPrice(orderItem.UnitPriceInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                orderItemModel.UnitPriceExclTax = _priceFormatter.FormatPrice(orderItem.UnitPriceExclTax, true, _workContext.WorkingCurrency.CurrencyCode, false, _workContext.WorkingLanguage);
                //discounts
                orderItemModel.DiscountInclTaxValue = orderItem.DiscountAmountInclTax;
                orderItemModel.DiscountExclTaxValue = orderItem.DiscountAmountExclTax;
                orderItemModel.DiscountInclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                orderItemModel.DiscountExclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);
                //subtotal
                orderItemModel.SubTotalInclTaxValue = orderItem.PriceInclTax;
                orderItemModel.SubTotalExclTaxValue = orderItem.PriceExclTax;
                orderItemModel.SubTotalInclTax = _priceFormatter.FormatPrice(orderItem.PriceInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                orderItemModel.SubTotalExclTax = _priceFormatter.FormatPrice(orderItem.PriceExclTax, true, _workContext.WorkingCurrency.CurrencyCode, false, _workContext.WorkingLanguage);

                orderItemModel.AttributeInfo = orderItem.AttributeDescription;
                if (orderItem.Product.IsRecurring)
                    orderItemModel.RecurringInfo = string.Format(_localizationService.GetResource("Admin.Orders.Products.RecurringPeriod"), orderItem.Product.RecurringCycleLength, orderItem.Product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));
                //rental info
                if (orderItem.Product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalStartDateUtc.Value) : "";
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalEndDateUtc.Value) : "";
                    orderItemModel.RentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
                }


                //gift cards
                orderItemModel.PurchasedGiftCardIds = _giftCardService.GetGiftCardsByPurchasedWithOrderItemId(orderItem.Id)
                    .Select(gc => gc.Id).ToList();

                model.Items.Add(orderItemModel);
            }
            model.HasDownloadableProducts = hasDownloadableItems;
            #endregion


            model.POInHandDate = order.POInHandDate;
            model.ShipByDate = order.ShipByDate;
            model.POSentDate = order.POSentDate;
            model.CustInHandDate = order.CustInHandDate;
            model.OrderCost =order.OrderCost;
            model.MarketingChannelId =order.MarketingChannelId;
            model.Supplier = order.Supplier;
            IEnumerable<MarketingChannel> MarketingChannelTypes = Enum.GetValues(typeof(MarketingChannel)).Cast<MarketingChannel>();

            foreach (var marketinTpye in MarketingChannelTypes)
            {
                model.AvailableMarketingChannel.Add(new SelectListItem
                {
                    Text = marketinTpye.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id),
                    Value = ((int)marketinTpye).ToString()

                });


            }
        }


        [NonAction]
        protected virtual void PreparePurchaseOrderItemDetailsModel(PurchaseOrderModel model, PurchaseOrder pOrder)
        {
            if (pOrder == null)
                throw new ArgumentNullException("PurchaseOrder");

            if (model == null)
                throw new ArgumentNullException("model");

            #region Products


            var products = pOrder.PurchaseOrderItems;

            foreach (var orderItem in products)
            {
                var orderItemModel = new PurchaseOrderModel.PurchaseOrderItemModel
                {
                    Id = orderItem.Id,
                    Po_id = orderItem.Po_id,
                    ProductName = orderItem.ProductName,
                    ProductCode = orderItem.ProductCode,
                    Quantity = orderItem.Quantity,
                    QuantityReceived = orderItem.QuantityReceived,
                    PONumber = orderItem.PONumber
                };

                //unit price
                orderItemModel.Price = orderItem.Price;
                orderItemModel.UnitPrice = _priceFormatter.FormatPrice(Convert.ToDecimal(orderItem.Price), true, _workContext.WorkingCurrency.CurrencyCode, false, _workContext.WorkingLanguage);

                //subtotal
                orderItemModel.SubtotalValue = orderItem.Quantity * orderItemModel.Price;
                orderItemModel.Subtotal = _priceFormatter.FormatPrice(Convert.ToDecimal(orderItem.Quantity * orderItemModel.Price), true, _workContext.WorkingCurrency.CurrencyCode, false, _workContext.WorkingLanguage);
                orderItemModel.AttributeDescription = orderItem.AttributeDescription;

                model.Items.Add(orderItemModel);
            }

            #endregion

        }



        [NonAction]
        protected virtual void PrepareVendorModel(VendorModel model, Vendor vendor, bool excludeProperties, bool prepareEntireAddressModel)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var address = _addressService.GetAddressById(vendor != null ? vendor.AddressId : 0);

            if (vendor != null)
            {
                if (!excludeProperties)
                {
                    if (address != null)
                    {
                        model.Address = address.ToModel();
                    }
                }

                //associated customer emails
                model.AssociatedCustomers = _customerService
                    .GetAllCustomers(vendorId: vendor.Id)
                    .Select(c => new VendorModel.AssociatedCustomerInfo()
                    {
                        Id = c.Id,
                        Email = c.Email
                    })
                    .ToList();
            }

            if (prepareEntireAddressModel)
            {
                model.Address.CountryEnabled = true;
                model.Address.StateProvinceEnabled = true;
                model.Address.CityEnabled = true;
                model.Address.StreetAddressEnabled = true;
                model.Address.StreetAddress2Enabled = true;
                model.Address.ZipPostalCodeEnabled = true;
                model.Address.PhoneEnabled = true;
                model.Address.FaxEnabled = true;

                //address
                model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
                foreach (var c in _countryService.GetAllCountries(showHidden: true))
                    model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (address != null && c.Id == address.CountryId) });

                var states = model.Address.CountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId.Value, showHidden: true).ToList() : new List<StateProvince>();
                if (states.Any())
                {
                    foreach (var s in states)
                        model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (address != null && s.Id == address.StateProvinceId) });
                }
                else
                    model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
            }
        }

        public virtual ActionResult SendPurchaseOrder(int id)
        {
            var model = _purchaseOrderService.GetPurchaseOrderById(id);

            if (model == null)
                throw new ArgumentNullException("model");
            try
            {
                if (model != null)
                {
                    var purchaseOrderAttachmentFilePath = _pdfService.PrintPurchaseOrderToPdf(model);
                    var purchaseOrderAttachmentFileName = string.Format("PurchaseOrder_{0}.pdf", model.PONumber);
                    var poDate = _dateTimeHelper.ConvertToUserTime(Convert.ToDateTime(model.PoDate), DateTimeKind.Utc).ToString("D", new CultureInfo(_workContext.WorkingLanguage.LanguageCulture));
                    var poDeliverydate = model.PODeliveryDate == null ? "" : _dateTimeHelper.ConvertToUserTime(Convert.ToDateTime(model.PODeliveryDate), DateTimeKind.Utc).ToString("D", new CultureInfo(_workContext.WorkingLanguage.LanguageCulture));
                    var msgid = _workflowMessageService.SendPurchaseOrderNotification(model, poDate, poDeliverydate, _workContext.WorkingLanguage.Id, purchaseOrderAttachmentFilePath, purchaseOrderAttachmentFileName);
                    if (msgid > 0)
                    {
                        SuccessNotification(_localizationService.GetResource("Admin.PurchaseOrder.PurchaseOrderSent"));
                    }
                    else {
                        ErrorNotification(_localizationService.GetResource("Admin.PurchaseOrder.PurchaseOrderSentError"), true);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorNotification(ex, false);
                ErrorNotification(ex.Message, true);
            }

            return RedirectToAction("PurchaseOrder", new { orderid = model.OrderId });
        }


        protected virtual void PreparePurchaseOrdeDetailsModel(PurchaseOrderModel model, PurchaseOrder purchaseOrder, OrderModel ordermodel)
        {
            model.OrderModel = ordermodel;

            //vendors

            model.AvailableVendors.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.Vendor.None"),
                Value = ""
            });

            model.OrderId = ordermodel.Id;
            model.POShipVia = ordermodel.ShippingMethod;
            model.POShippingCost = ordermodel.OrderShippingExclTaxValue;

            if (purchaseOrder == null)
            {

                foreach (var item in ordermodel.Items)
                {
                    var purchaseOrderItemModel = new PurchaseOrderModel.PurchaseOrderItemModel
                    {
                        Po_id = 0,
                        Id = 0,
                        PONumber = "",
                        ProductCode = item.Sku,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPriceExclTax,
                        Price = item.UnitPriceExclTaxValue,
                        ShippingCost = 0,
                        OrderDetailId = item.Id,
                        AttributeDescription = item.AttributeInfo,
                        SubtotalValue = item.SubTotalExclTaxValue,
                        Subtotal = item.SubTotalExclTax,
                        QuantityReceived = 0
                    };
                    model.Items.Add(purchaseOrderItemModel);

                }
            }

            if (purchaseOrder != null)
            {
                model.VendorId = purchaseOrder.VendorId;
                model.PoDate = purchaseOrder.PoDate == null ? purchaseOrder.PoDate : _dateTimeHelper.ConvertToUserTime(Convert.ToDateTime(purchaseOrder.PoDate), DateTimeKind.Utc);
                model.PONumber = purchaseOrder.PONumber;
                model.POShipVia = purchaseOrder.POShipVia;
                model.POTerm = purchaseOrder.POTerm;
                model.PODeliveryDate = purchaseOrder.PODeliveryDate == null ? purchaseOrder.PODeliveryDate : _dateTimeHelper.ConvertToUserTime(Convert.ToDateTime(purchaseOrder.PODeliveryDate), DateTimeKind.Utc);
                model.VendorAddress = purchaseOrder.VendorAddress;
                model.VendorCity = purchaseOrder.VendorCity;
                model.VendorState = purchaseOrder.VendorState;
                model.VendorPostalCode = purchaseOrder.VendorPostalCode;
                model.VendorEmail = purchaseOrder.VendorEmail;
                model.Id = purchaseOrder.Id;
                model.VendorTitle = purchaseOrder.VendorTitle;
                model.OrderId = purchaseOrder.OrderId;
                model.POShippingCost = purchaseOrder.POShippingCost;
                model.PONotes = purchaseOrder.PONotes;
                model.POAuthorizedBy = purchaseOrder.POAuthorizedBy;

                PreparePurchaseOrderItemDetailsModel(model, purchaseOrder);
            }

            var vendors = SelectListHelper.GetVendorList(_vendorService, _cacheManager, true);
            foreach (var v in vendors)
                model.AvailableVendors.Add(v);
        }
    }
}