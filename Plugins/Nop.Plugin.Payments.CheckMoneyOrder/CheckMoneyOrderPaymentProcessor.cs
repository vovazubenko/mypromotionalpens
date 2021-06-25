using System;
using System.Collections.Generic;
using System.Web.Routing;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.CheckMoneyOrder.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Payments.CheckMoneyOrder
{
    /// <summary>
    /// CheckMoneyOrder payment processor
    /// </summary>
    public class CheckMoneyOrderPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly CheckMoneyOrderPaymentSettings _checkMoneyOrderPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ISettingService _settingService;
        
        #endregion

        #region Ctor

        public CheckMoneyOrderPaymentProcessor(CheckMoneyOrderPaymentSettings checkMoneyOrderPaymentSettings,
            ILocalizationService localizationService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ISettingService settingService)
        {
            this._checkMoneyOrderPaymentSettings = checkMoneyOrderPaymentSettings;
            this._localizationService = localizationService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._settingService = settingService;
        }

        #endregion
        
        #region Methods
        
        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country

            if (_checkMoneyOrderPaymentSettings.ShippableProductRequired && !cart.RequiresShipping())
                return true;

            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _checkMoneyOrderPaymentSettings.AdditionalFee, _checkMoneyOrderPaymentSettings.AdditionalFeePercentage);
            return result;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //it's not a redirection payment method. So we always return false
            return false;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentCheckMoneyOrder";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.CheckMoneyOrder.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentCheckMoneyOrder";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.CheckMoneyOrder.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Get the type of controller
        /// </summary>
        /// <returns>Type</returns>
        public Type GetControllerType()
        {
            return typeof(PaymentCheckMoneyOrderController);
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new CheckMoneyOrderPaymentSettings
            {
                DescriptionText = "<p>Mail Personal or Business Check, Cashier's Check or money order to:</p><p><br /><b>NOP SOLUTIONS</b> <br /><b>your address here,</b> <br /><b>New York, NY 10001 </b> <br /><b>USA</b></p><p>Notice that if you pay by Personal or Business Check, your order may be held for up to 10 days after we receive your check to allow enough time for the check to clear.  If you want us to ship faster upon receipt of your payment, then we recommend your send a money order or Cashier's check.</p><p>P.S. You can edit this text from admin panel.</p>"
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.AdditionalFee.Hint", "The additional fee.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.AdditionalFeePercentage", "Additional fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.DescriptionText", "Description");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.DescriptionText.Hint", "Enter info that will be shown to customers during checkout");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.PaymentMethodDescription", "Pay by cheque or money order");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.ShippableProductRequired", "Shippable product required");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.ShippableProductRequired.Hint", "An option indicating whether shippable products are required in order to display this payment method during checkout.");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<CheckMoneyOrderPaymentSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.AdditionalFeePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.DescriptionText");
            this.DeletePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.DescriptionText.Hint");
            this.DeletePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.PaymentMethodDescription");
            this.DeletePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.ShippableProductRequired");
            this.DeletePluginLocaleResource("Plugins.Payment.CheckMoneyOrder.ShippableProductRequired.Hint");
            
            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get { return RecurringPaymentType.NotSupported; }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Standard; }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayPal site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payment.CheckMoneyOrder.PaymentMethodDescription"); }
        }

        #endregion

        public CustomerProfileResponse CreateCustomerProfile(Customer customer) { return null; }
        public CustomerProfileResponse UpdateCustomerProfile(CreditCardInfo cardInfo) { return null; }
        public CustomerProfileResponse DeleteCustomerProfile(string customerProfileId) { return null; }
        public CustomerProfileResult GetCustomerProfile(string customerProfileId) { return null; }

        public CustomerPaymentProfileResponse CreateCustomerPaymentProfile(CreditCardInfo paymentProfile) { return null; }
        public CustomerPaymentProfileResponse UpdateCustomerPaymentProfile(CreditCardInfo paymentProfile) { return null; }
        public CustomerPaymentProfileResponse DeleteCustomerPaymentProfile(CreditCardInfo paymentProfile) { return null; }
        public CustomerPaymentProfileResult GetCustomerPaymentProfile(string customerProfileId, long paymentProfileId) { return null; }
        public ProcessPaymentResult CreateTransaction(string customerProfileId, string customerPaymentProfileId, decimal Amount, string orderGuid,string description)
        {
            return null;
        }
        public ProcessPaymentResult RefundTransaction(string customerProfileId, string customerPaymentProfileId, decimal Amount, string orderGuid, string description)
        {
            return null;
        }

        public ProcessPaymentResult AuthorizeTransaction(string customerProfileId, string customerPaymentProfileId, decimal Amount, string ordernumber, string description) {
            return null;
        }
    }
}
