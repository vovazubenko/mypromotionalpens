using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.AuthorizeNet.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;

using AuthorizeNetSDK = AuthorizeNet;
using Nop.Core.Infrastructure;
using Nop.Core.Domain.Customers;
using Newtonsoft.Json;
using Nop.Core.Domain.Logging;

namespace Nop.Plugin.Payments.AuthorizeNet
{
    /// <summary>
    /// AuthorizeNet payment processor
    /// </summary>
    public class AuthorizeNetPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IWebHelper _webHelper;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger _logger;
        private readonly CurrencySettings _currencySettings;
        private readonly AuthorizeNetPaymentSettings _authorizeNetPaymentSettings;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public AuthorizeNetPaymentProcessor(ISettingService settingService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IWebHelper webHelper,
            IOrderTotalCalculationService orderTotalCalculationService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IEncryptionService encryptionService,
            ILogger logger,
            CurrencySettings currencySettings,
            AuthorizeNetPaymentSettings authorizeNetPaymentSettings,
            ILocalizationService localizationService)
        {
            this._authorizeNetPaymentSettings = authorizeNetPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._customerService = customerService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._encryptionService = encryptionService;
            this._logger = logger;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._localizationService = localizationService;
        }

        #endregion

        #region Utilities

        private void PrepareAuthorizeNet()
        {
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = _authorizeNetPaymentSettings.UseSandbox
                ? AuthorizeNetSDK.Environment.SANDBOX
                : AuthorizeNetSDK.Environment.PRODUCTION;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType
            {
                name = _authorizeNetPaymentSettings.LoginId,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = _authorizeNetPaymentSettings.TransactionKey
            };
        }

        private static createTransactionResponse GetApiResponse(createTransactionController controller, IList<string> errors)
        {
            var response = controller.GetApiResponse();

            if (response != null)
            {
                if (response.transactionResponse != null && response.transactionResponse.errors != null)
                {
                    foreach (var transactionResponseError in response.transactionResponse.errors)
                    {
                        errors.Add(string.Format("Error #{0}: {1}", transactionResponseError.errorCode,
                            transactionResponseError.errorText));
                    }

                    return null;
                }

                if (response.transactionResponse != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    switch (response.transactionResponse.responseCode)
                    {
                        case "1":
                            {
                                return response;
                            }
                        case "2":
                            {
                                var description = response.transactionResponse.messages.Any()
                                    ? response.transactionResponse.messages.First().description
                                    : String.Empty;
                                errors.Add(
                                    string.Format("Declined ({0}: {1})", response.transactionResponse.responseCode,
                                        description).TrimEnd(':', ' '));
                                return null;
                            }
                    }
                }
                else if (response.transactionResponse != null && response.messages.resultCode == messageTypeEnum.Error)
                {
                    if (response.messages != null && response.messages.message != null && response.messages.message.Any())
                    {
                        var message = response.messages.message.First();

                        errors.Add(string.Format("Error #{0}: {1}", message.code, message.text));
                        return null;
                    }
                }
            }
            else
            {
                var error = controller.GetErrorResponse();
                if (error != null && error.messages != null && error.messages.message != null && error.messages.message.Any())
                {
                    var message = error.messages.message.First();

                    errors.Add(string.Format("Error #{0}: {1}", message.code, message.text));
                    return null;
                }
            }
            var controllerResult = controller.GetResults().FirstOrDefault();
            const string unknownError = "Authorize.NET unknown error";
            errors.Add(String.IsNullOrEmpty(controllerResult) ? unknownError : String.Format("{0} ({1})", unknownError, controllerResult));
            return null;
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
            string CustomerProfileId = "";
            string CustomerPaymentProfileId = "";
            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            //result.AllowStoringCreditCardNumber = true;
            PrepareAuthorizeNet();

            var creditCard = new creditCardType
            {
                cardNumber = processPaymentRequest.CreditCardNumber,
                expirationDate =
                    processPaymentRequest.CreditCardExpireMonth.ToString("D2") + processPaymentRequest.CreditCardExpireYear,
                cardCode = processPaymentRequest.CreditCardCvv2
            };

            //standard api call to retrieve response
            var paymentType = new paymentType { Item = creditCard };

            transactionTypeEnum transactionType;

            switch (_authorizeNetPaymentSettings.TransactMode)
            {
                case TransactMode.Authorize:
                    transactionType = transactionTypeEnum.authOnlyTransaction;
                    break;
                case TransactMode.AuthorizeAndCapture:
                    transactionType = transactionTypeEnum.authCaptureTransaction;
                    break;
                default:
                    throw new NopException("Not supported transaction mode");
            }

            var billTo = new customerAddressType
            {
                firstName = customer.BillingAddress.FirstName,
                lastName = customer.BillingAddress.LastName,
                email = customer.BillingAddress.Email,
                address = customer.BillingAddress.Address1,
                city = customer.BillingAddress.City,
                zip = customer.BillingAddress.ZipPostalCode
            };

            if (!string.IsNullOrEmpty(customer.BillingAddress.Company))
                billTo.company = customer.BillingAddress.Company;

            if (customer.BillingAddress.StateProvince != null)
                billTo.state = customer.BillingAddress.StateProvince.Abbreviation;

            if (customer.BillingAddress.Country != null)
                billTo.country = customer.BillingAddress.Country.TwoLetterIsoCode;

            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionType.ToString(),
                amount = Math.Round(processPaymentRequest.OrderTotal, 2),
                payment = paymentType,
                currencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode,
                billTo = billTo,
                customerIP = _webHelper.GetCurrentIpAddress(),
                order = new orderType
                {
                    //x_invoice_num is 20 chars maximum. hece we also pass x_description
                    //invoiceNumber = processPaymentRequest.OrderGuid.ToString().Substring(0, 20),
                    invoiceNumber = processPaymentRequest.OrderNumber,
                    description = string.Format("Full order #{0}", processPaymentRequest.OrderNumber)  //processPaymentRequest.OrderGuid
                }
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the contoller that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = GetApiResponse(controller, result.Errors);

            //validate
            if (response == null)
                return result;
            else {

                var _creditCardInfoService = EngineContext.Current.Resolve<ICreditCardInfoService>();
                var creditCardInfoList = _creditCardInfoService.GetCreditCardInfosByCustomerId(customer.Id);
                //if (creditCardInfoList.Count == 0)
                //{

                int cardType = 0;
                if (response.transactionResponse != null)
                {
                    if (response.transactionResponse.accountType.ToLower().Contains("american")) cardType = 1;
                    else if (response.transactionResponse.accountType.ToLower().Contains("discover")) cardType = 2;
                    else if (response.transactionResponse.accountType.ToLower().Contains("master")) cardType = 3;
                    else if (response.transactionResponse.accountType.ToLower().Contains("visa")) cardType = 4;
                    else if (response.transactionResponse.accountType.ToLower().Contains("jcb")) cardType = 5;

                }

                CreditCardInfo paymentProfile = new CreditCardInfo();
                paymentProfile.CardNumber = processPaymentRequest.CreditCardNumber;
                paymentProfile.CardType = cardType;
                paymentProfile.CVVNumber = processPaymentRequest.CreditCardCvv2;
                //paymentProfile.CVVNumber = "";
                paymentProfile.ExpireMonth = processPaymentRequest.CreditCardExpireMonth;
                paymentProfile.ExpireYear = processPaymentRequest.CreditCardExpireYear;
                paymentProfile.PaymentProfileId = 0;
                paymentProfile.Address = customer.BillingAddress;
                paymentProfile.CustomerId = customer.Id;
                paymentProfile.Customer = customer;
                paymentProfile.IsDefault = true;
                var responsePaymentProfile = _creditCardInfoService.InsertCreditCardInfo(paymentProfile);
                if (responsePaymentProfile.Success)
                    _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "Successfully created payment profile for " + customer.Email + " at time of order");
                else
                    _logger.InsertLog(Core.Domain.Logging.LogLevel.Error, "Unable to create payment profile for " + customer.Email + " at time of order. Error: " + responsePaymentProfile.Error);
                if (responsePaymentProfile != null)
                {
                    CustomerProfileId = responsePaymentProfile.CustomerProfileId;
                    CustomerPaymentProfileId = responsePaymentProfile.CustomerPaymentProfileId;
                }
                //}

                //    string transactionId = response.transactionResponse.transId;

                //    var customerProfile = new customerProfileBaseType
                //    {
                //        merchantCustomerId = customer.Id.ToString(),
                //        email = customer.Email
                //    };

                //    var request2 = new createCustomerProfileFromTransactionRequest
                //    {
                //        transId = transactionId,
                //        // You can either specify the customer information in form of customerProfileBaseType object
                //        customer = customerProfile
                //    };
                //    var controller2 = new createCustomerProfileFromTransactionController(request2);
                //    controller2.Execute();

                //    var responseCreateCust = controller2.GetApiResponse();

                //    //Update CustomerProfileId and CustomerPaymentProfileId
                //    customer.CustomerPaymentProfileId = responseCreateCust.customerPaymentProfileIdList[0].ToString();
                //    customer.CustomerProfileId = responseCreateCust.customerProfileId;
                //    _customerService.UpdateCustomer(customer);
            }
            if (_authorizeNetPaymentSettings.TransactMode == TransactMode.Authorize)
            {
                result.AuthorizationTransactionId = response.transactionResponse.transId;
                result.AuthorizationTransactionCode = string.Format("{0},{1}", response.transactionResponse.transId, response.transactionResponse.authCode);
            }
            if (_authorizeNetPaymentSettings.TransactMode == TransactMode.AuthorizeAndCapture)
                result.CaptureTransactionId = string.Format("{0},{1}", response.transactionResponse.transId, response.transactionResponse.authCode);

            result.AuthorizationTransactionResult = string.Format("Approved ({0}: {1})", response.transactionResponse.responseCode, response.transactionResponse.messages[0].description);
            result.AvsResult = response.transactionResponse.avsResultCode;
            result.NewPaymentStatus = _authorizeNetPaymentSettings.TransactMode == TransactMode.Authorize ? PaymentStatus.Authorized : PaymentStatus.Paid;
            result.CustomerProfileId = CustomerProfileId;
            result.CustomerPaymentProfileId = CustomerPaymentProfileId;
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
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _authorizeNetPaymentSettings.AdditionalFee, _authorizeNetPaymentSettings.AdditionalFeePercentage);
            return result;
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
            return false;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            PrepareAuthorizeNet();

            var codes = capturePaymentRequest.Order.AuthorizationTransactionCode.Split(',');
            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.priorAuthCaptureTransaction.ToString(),
                //amount = Math.Round(capturePaymentRequest.Order.OrderTotal, 2),
                amount = Math.Round(capturePaymentRequest.Order.OrderBaseAmount, 2), // pass only authorized amount
                refTransId = codes[0],
                currencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode,
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the contoller that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = GetApiResponse(controller, result.Errors);

            //validate
            if (response == null)
                return result;

            result.CaptureTransactionId = string.Format("{0},{1}", response.transactionResponse.transId, response.transactionResponse.authCode);
            result.CaptureTransactionResult = string.Format("Approved ({0}: {1})", response.transactionResponse.responseCode, response.transactionResponse.messages[0].description);
            result.NewPaymentStatus = PaymentStatus.Paid;

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

            PrepareAuthorizeNet();
            var maskedCreditCardNumberDecrypted = refundPaymentRequest.Order.MaskedCreditCardNumber;

            try { maskedCreditCardNumberDecrypted = _encryptionService.DecryptText(refundPaymentRequest.Order.MaskedCreditCardNumber); }
            catch { }

            if (String.IsNullOrEmpty(maskedCreditCardNumberDecrypted) || maskedCreditCardNumberDecrypted.Length < 4)
            {
                result.AddError("Last four digits of Credit Card Not Available");
                return result;
            }

            var lastFourDigitsCardNumber = maskedCreditCardNumberDecrypted.Substring(maskedCreditCardNumberDecrypted.Length - 4);
            var creditCard = new creditCardType
            {
                cardNumber = lastFourDigitsCardNumber,
                expirationDate = "XXXX"
            };

            var codes = (string.IsNullOrEmpty(refundPaymentRequest.Order.CaptureTransactionId) ? refundPaymentRequest.Order.AuthorizationTransactionCode : refundPaymentRequest.Order.CaptureTransactionId).Split(',');
            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.refundTransaction.ToString(),
                amount = Math.Round(refundPaymentRequest.AmountToRefund, 2),
                refTransId = codes[0],
                currencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode,

                order = new orderType
                {
                    //x_invoice_num is 20 chars maximum. hece we also pass x_description
                    invoiceNumber = refundPaymentRequest.Order.OrderNumber,  //refundPaymentRequest.Order.OrderGuid.ToString().Substring(0, 20),
                    description = string.Format("Full order #{0}", refundPaymentRequest.Order.OrderNumber)
                },

                payment = new paymentType { Item = creditCard }
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the contoller that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            GetApiResponse(controller, result.Errors);
            if (refundPaymentRequest.IsPartialRefund)
                result.NewPaymentStatus = PaymentStatus.PartiallyRefunded;
            else
                result.NewPaymentStatus = PaymentStatus.Refunded;

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

            PrepareAuthorizeNet();

            var maskedCreditCardNumberDecrypted = _encryptionService.DecryptText(voidPaymentRequest.Order.MaskedCreditCardNumber);

            if (String.IsNullOrEmpty(maskedCreditCardNumberDecrypted) || maskedCreditCardNumberDecrypted.Length < 4)
            {
                result.AddError("Last four digits of Credit Card Not Available");
                return result;
            }

            var lastFourDigitsCardNumber = maskedCreditCardNumberDecrypted.Substring(maskedCreditCardNumberDecrypted.Length - 4);
            var expirationDate = voidPaymentRequest.Order.CardExpirationMonth + voidPaymentRequest.Order.CardExpirationYear;

            if (!expirationDate.Any())
                expirationDate = "XXXX";

            var creditCard = new creditCardType
            {
                cardNumber = lastFourDigitsCardNumber,
                expirationDate = expirationDate
            };

            var codes = (string.IsNullOrEmpty(voidPaymentRequest.Order.CaptureTransactionId) ? voidPaymentRequest.Order.AuthorizationTransactionCode : voidPaymentRequest.Order.CaptureTransactionId).Split(',');
            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.voidTransaction.ToString(),
                refTransId = codes[0],
                payment = new paymentType { Item = creditCard }
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the contoller that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            var response = GetApiResponse(controller, result.Errors);

            //validate
            if (response == null)
                return result;

            result.NewPaymentStatus = PaymentStatus.Voided;

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

            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

            PrepareAuthorizeNet();

            var creditCard = new creditCardType
            {
                cardNumber = processPaymentRequest.CreditCardNumber,
                expirationDate =
                    processPaymentRequest.CreditCardExpireMonth.ToString("D2") + processPaymentRequest.CreditCardExpireYear,
                cardCode = processPaymentRequest.CreditCardCvv2
            };

            //standard api call to retrieve response
            var paymentType = new paymentType { Item = creditCard };

            var billTo = new nameAndAddressType
            {
                firstName = customer.BillingAddress.FirstName,
                lastName = customer.BillingAddress.LastName,
                //email = customer.BillingAddress.Email,
                address = customer.BillingAddress.Address1,
                //address = customer.BillingAddress.Address1 + " " + customer.BillingAddress.Address2;
                city = customer.BillingAddress.City,
                zip = customer.BillingAddress.ZipPostalCode
            };

            if (!string.IsNullOrEmpty(customer.BillingAddress.Company))
                billTo.company = customer.BillingAddress.Company;

            if (customer.BillingAddress.StateProvince != null)
                billTo.state = customer.BillingAddress.StateProvince.Abbreviation;

            if (customer.BillingAddress.Country != null)
                billTo.country = customer.BillingAddress.Country.TwoLetterIsoCode;

            var dtNow = DateTime.UtcNow;

            // Interval can't be updated once a subscription is created.
            var paymentScheduleInterval = new paymentScheduleTypeInterval();
            switch (processPaymentRequest.RecurringCyclePeriod)
            {
                case RecurringProductCyclePeriod.Days:
                    paymentScheduleInterval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength);
                    paymentScheduleInterval.unit = ARBSubscriptionUnitEnum.days;
                    break;
                case RecurringProductCyclePeriod.Weeks:
                    paymentScheduleInterval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength * 7);
                    paymentScheduleInterval.unit = ARBSubscriptionUnitEnum.days;
                    break;
                case RecurringProductCyclePeriod.Months:
                    paymentScheduleInterval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength);
                    paymentScheduleInterval.unit = ARBSubscriptionUnitEnum.months;
                    break;
                case RecurringProductCyclePeriod.Years:
                    paymentScheduleInterval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength * 12);
                    paymentScheduleInterval.unit = ARBSubscriptionUnitEnum.months;
                    break;
                default:
                    throw new NopException("Not supported cycle period");
            }

            var paymentSchedule = new paymentScheduleType
            {
                startDate = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day),
                totalOccurrences = Convert.ToInt16(processPaymentRequest.RecurringTotalCycles),
                interval = paymentScheduleInterval
            };

            var subscriptionType = new ARBSubscriptionType
            {
                name = processPaymentRequest.OrderNumber.ToString(),
                amount = Math.Round(processPaymentRequest.OrderTotal, 2),
                payment = paymentType,
                billTo = billTo,
                paymentSchedule = paymentSchedule,
                customer = new customerType
                {
                    email = customer.BillingAddress.Email
                    //phone number should be in one of following formats: 111- 111-1111 or (111) 111-1111.
                    //phoneNumber = customer.BillingAddress.PhoneNumber
                },

                order = new orderType
                {
                    //x_invoice_num is 20 chars maximum. hece we also pass x_description
                    invoiceNumber = processPaymentRequest.OrderNumber, //processPaymentRequest.OrderGuid.ToString().Substring(0, 20),
                    description = String.Format("Recurring payment #{0}", processPaymentRequest.OrderNumber)
                }
            };

            if (customer.ShippingAddress != null)
            {
                var shipTo = new nameAndAddressType
                {
                    firstName = customer.ShippingAddress.FirstName,
                    lastName = customer.ShippingAddress.LastName,
                    address = customer.ShippingAddress.Address1,
                    city = customer.ShippingAddress.City,
                    zip = customer.ShippingAddress.ZipPostalCode
                };

                if (customer.ShippingAddress.StateProvince != null)
                {
                    shipTo.state = customer.ShippingAddress.StateProvince.Abbreviation;
                }

                subscriptionType.shipTo = shipTo;
            }

            var request = new ARBCreateSubscriptionRequest { subscription = subscriptionType };

            // instantiate the contoller that will call the service
            var controller = new ARBCreateSubscriptionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            //validate
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                result.SubscriptionTransactionId = response.subscriptionId;
                result.AuthorizationTransactionCode = response.refId;
                result.AuthorizationTransactionResult = string.Format("Approved ({0}: {1})", response.refId, response.subscriptionId);
            }
            else if (response != null)
            {
                foreach (var responseMessage in response.messages.message)
                {
                    result.AddError(string.Format("Error processing recurring payment #{0}: {1}", responseMessage.code, responseMessage.text));
                }
            }
            else
            {
                result.AddError("Authorize.NET unknown error");
            }

            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="transactionId">AuthorizeNet transaction ID</param>
        public void ProcessRecurringPayment(string transactionId)
        {
            PrepareAuthorizeNet();
            var request = new getTransactionDetailsRequest { transId = transactionId };

            // instantiate the controller that will call the service
            var controller = new getTransactionDetailsController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            if (response == null)
            {
                _logger.Error(String.Format("Authorize.NET unknown error (transactionId: {0})", transactionId));
                return;
            }

            var transaction = response.transaction;
            if (transaction == null)
            {
                _logger.Error(String.Format("Authorize.NET: Transaction data is missing (transactionId: {0})", transactionId));
                return;
            }

            if (transaction.transactionStatus == "refundTransaction")
            {
                return;
            }

            var orderDescriptions = transaction.order.description.Split('#');

            if (orderDescriptions.Length < 2)
            {
                _logger.Error(String.Format("Authorize.NET: Missing order Invoice Number (transactionId: {0})", transactionId));
                return;
            }

            //var order = _orderService.GetOrderByGuid(new Guid(orderDescriptions[1]));
            var order = _orderService.GetOrderByOrderNumber(orderDescriptions[1]);
            if (order == null)
            {
                _logger.Error(String.Format("Authorize.NET: Order cannot be loaded (order Invoice Number: {0})", orderDescriptions[1]));
                return;
            }

            var recurringPayments = _orderService.SearchRecurringPayments(initialOrderId: order.Id);

            foreach (var rp in recurringPayments)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    var recurringPaymentHistory = rp.RecurringPaymentHistory;
                    var orders = _orderService.GetOrdersByIds(recurringPaymentHistory.Select(rph => rph.OrderId).ToArray()).ToList();

                    var transactionsIds = new List<string>();
                    transactionsIds.AddRange(orders.Select(o => o.AuthorizationTransactionId).Where(tId => !string.IsNullOrEmpty(tId)));
                    transactionsIds.AddRange(orders.Select(o => o.CaptureTransactionId).Where(tId => !string.IsNullOrEmpty(tId)));

                    //skip the re-processing of transactions
                    if (transactionsIds.Contains(transactionId))
                        continue;

                    var newPaymentStatus = transaction.transactionType == "authOnlyTransaction" ? PaymentStatus.Authorized : PaymentStatus.Paid;

                    if (recurringPaymentHistory.Count == 0)
                    {
                        //first payment
                        var rph = new RecurringPaymentHistory
                        {
                            RecurringPaymentId = rp.Id,
                            OrderId = order.Id,
                            CreatedOnUtc = DateTime.UtcNow
                        };
                        rp.RecurringPaymentHistory.Add(rph);

                        if (newPaymentStatus == PaymentStatus.Authorized)
                            rp.InitialOrder.AuthorizationTransactionId = transactionId;
                        else
                            rp.InitialOrder.CaptureTransactionId = transactionId;

                        _orderService.UpdateRecurringPayment(rp);
                    }
                    else
                    {
                        //next payments
                        var processPaymentResult = new ProcessPaymentResult
                        {
                            NewPaymentStatus = newPaymentStatus
                        };

                        if (newPaymentStatus == PaymentStatus.Authorized)
                            processPaymentResult.AuthorizationTransactionId = transactionId;
                        else
                            processPaymentResult.CaptureTransactionId = transactionId;

                        _orderProcessingService.ProcessNextRecurringPayment(rp, processPaymentResult);
                    }
                }
                else
                {
                    var processPaymentResult = new ProcessPaymentResult();
                    processPaymentResult.AuthorizationTransactionId = processPaymentResult.CaptureTransactionId = transactionId;
                    processPaymentResult.RecurringPaymentFailed = true;
                    processPaymentResult.Errors.Add(string.Format("Authorize.Net Error: {0} - {1} (transactionId: {2})", response.messages.message[0].code, response.messages.message[0].text, transactionId));
                    _orderProcessingService.ProcessNextRecurringPayment(rp, processPaymentResult);
                }
            }
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            PrepareAuthorizeNet();

            var request = new ARBCancelSubscriptionRequest { subscriptionId = cancelPaymentRequest.Order.SubscriptionTransactionId };
            var controller = new ARBCancelSubscriptionController(request);
            controller.Execute();

            var response = controller.GetApiResponse();

            //validate
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                return result;

            if (response != null)
            {
                foreach (var responseMessage in response.messages.message)
                {
                    result.AddError(string.Format("Error processing recurring payment #{0}: {1}", responseMessage.code, responseMessage.text));
                }
            }
            else
            {
                result.AddError("Authorize.NET unknown error");
            }

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
            controllerName = "PaymentAuthorizeNet";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.AuthorizeNet.Controllers" }, { "area", null } };
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
            controllerName = "PaymentAuthorizeNet";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.AuthorizeNet.Controllers" }, { "area", null } };
        }

        public ProcessPaymentResult CreateTransaction(string customerProfileId, string customerPaymentProfileId, decimal Amount, string orderGuid, string description)
        {
            var result = new ProcessPaymentResult();
            Console.WriteLine("Charge Customer Profile");
            PrepareAuthorizeNet();

            //create a customer payment profile
            customerProfilePaymentType profileToCharge = new customerProfilePaymentType();
            profileToCharge.customerProfileId = customerProfileId;
            profileToCharge.paymentProfile = new paymentProfile { paymentProfileId = customerPaymentProfileId };


            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),    // refund type
                amount = Amount,
                order = new orderType
                {
                    //x_invoice_num is 20 chars maximum. hece we also pass x_description
                    invoiceNumber = orderGuid,
                    description = description
                },
                profile = profileToCharge
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };



            // instantiate the collector that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            _logger.InsertLog(LogLevel.Information, "Authorize net create transaction response", JsonConvert.SerializeObject(response));

            if (response == null)
                return result;

            if (response != null && response.transactionResponse.errors != null && response.transactionResponse.errors.Count() > 0)
            {
                foreach (var e in response.transactionResponse.errors)
                    result.Errors.Add("Error Code :" + e.errorCode + " " + e.errorText);
            }
            else if (response != null && response.messages != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (_authorizeNetPaymentSettings.TransactMode == TransactMode.Authorize)
                {
                    result.AuthorizationTransactionId = response.transactionResponse.transId;
                    result.AuthorizationTransactionCode = string.Format("{0},{1}", response.transactionResponse.transId, response.transactionResponse.authCode);
                }
                if (_authorizeNetPaymentSettings.TransactMode == TransactMode.AuthorizeAndCapture)
                    result.CaptureTransactionId = string.Format("{0},{1}", response.transactionResponse.transId, response.transactionResponse.authCode);
                result.AuthorizationTransactionResult = string.Format("Approved ({0}: {1})", response.transactionResponse.responseCode, response.transactionResponse.messages == null ? "" : response.transactionResponse.messages[0].description);

                result.AvsResult = response.transactionResponse.avsResultCode;
                result.NewPaymentStatus = _authorizeNetPaymentSettings.TransactMode == TransactMode.Authorize ? PaymentStatus.Authorized : PaymentStatus.Paid;

            }
            else
            {
                if (response != null && response.transactionResponse.errors != null && response.transactionResponse.errors.Count() > 0)
                {
                    foreach (var e in response.transactionResponse.errors)
                        result.Errors.Add("Error Code :" + e.errorCode + " " + e.errorText);
                }
                else
                {
                    if (response.messages != null)
                        result.Errors.Add("Error Code :" + response.messages.message[0]);
                    else
                        result.Errors.Add("Something wrong try again!.");
                }
            }
            return result;


        }
        public Type GetControllerType()
        {
            return typeof(PaymentAuthorizeNetController);
        }

        public override void Install()
        {
            //settings
            var settings = new AuthorizeNetPaymentSettings
            {
                UseSandbox = true,
                TransactMode = TransactMode.Authorize,
                TransactionKey = "123",
                LoginId = "456"
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Notes", "If you're using this gateway, ensure that your primary store currency is supported by Authorize.NET.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.UseSandbox", "Use Sandbox");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.UseSandbox.Hint", "Check to enable Sandbox (testing environment).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactModeValues", "Transaction mode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactModeValues.Hint", "Choose transaction mode.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactionKey", "Transaction key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactionKey.Hint", "Specify transaction key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.LoginId", "Login ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.LoginId.Hint", "Specify login identifier.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.PaymentMethodDescription", "Pay by credit / debit card");

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<AuthorizeNetPaymentSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Notes");
            this.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.UseSandbox");
            this.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.UseSandbox.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactModeValues");
            this.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactModeValues.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactionKey");
            this.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactionKey.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.LoginId");
            this.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.LoginId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFeePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.PaymentMethodDescription");

            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.Automatic;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Standard;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            get { return _localizationService.GetResource("Plugins.Payments.AuthorizeNet.PaymentMethodDescription"); }
        }

        #endregion

        public CustomerProfileResponse CreateCustomerProfile(Customer customer)
        {

            var result = new CustomerProfileResponse();
            PrepareAuthorizeNet();

            List<customerPaymentProfileType> paymentProfileList = new List<customerPaymentProfileType>();
            //customerPaymentProfileType ccPaymentProfile = new customerPaymentProfileType();
            //if (cardInfo.Address.Address1 != null && cardInfo.Address.Address1.Trim() != "")
            //{
            //    // Create the bill to type
            //    ccPaymentProfile.billTo = new customerAddressType();
            //    ccPaymentProfile.billTo.firstName = Convert.ToString(cardInfo.FirstName);
            //    ccPaymentProfile.billTo.lastName = Convert.ToString(cardInfo.LastName);
            //    ccPaymentProfile.billTo.company = "";
            //    ccPaymentProfile.billTo.address = Convert.ToString(cardInfo.Address.Address1 + " " + cardInfo.Address.Address2);
            //    ccPaymentProfile.billTo.city = Convert.ToString(cardInfo.Address.City);
            //    ccPaymentProfile.billTo.state = Convert.ToString(cardInfo.Address.StateProvince.Name);             
            //    ccPaymentProfile.billTo.zip = Convert.ToString(cardInfo.Address.ZipPostalCode);
            //    ccPaymentProfile.billTo.country = Convert.ToString(cardInfo.Address.Country.Name);                
            //    ccPaymentProfile.billTo.phoneNumber = Convert.ToString(cardInfo.Address.PhoneNumber);
            //}          
            //ccPaymentProfile.payment = null;

            //paymentProfileList.Add(ccPaymentProfile);
            customerProfileType customerProfile = new customerProfileType();
            customerProfile.merchantCustomerId = Convert.ToString(customer.Id);
            customerProfile.email = customer.Email;
            //customerProfile.paymentProfiles = paymentProfileList.ToArray();

            var request = new createCustomerProfileRequest { profile = customerProfile, validationMode = validationModeEnum.none };

            var controller = new createCustomerProfileController(request);          // instantiate the contoller that will call the service
            controller.Execute();

            createCustomerProfileResponse response = controller.GetApiResponse();   // get the response from the service (errors contained if any)

            //validate
            if (response != null && response.messages != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response != null && response.messages.message != null)
                {
                    //result.customerPaymentProfileId = (response.customerPaymentProfileIdList != null) ? response.customerPaymentProfileIdList[0] : null;
                    result.customerPaymentProfileId = string.Empty;
                    result.customerProfileId = response.customerProfileId;
                    result.refId = response.refId;
                    result.resultCode = 0;
                    result.sessionToken = response.sessionToken;
                    result.Success = true;
                    result.message = response.messages.message[0].text;
                }
            }
            else
            {
                if (response != null)
                {
                    result.message = "Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text;
                    if (response.messages.message[0].code == "E00039")
                        result.message = "Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text;
                }
                else
                    result.message = "Error: Something Wrong. Try Again!";

                result.refId = response.refId;
                result.resultCode = 1;
                result.Success = false;
            }

            return result;
        }

        public CustomerProfileResponse UpdateCustomerProfile(CreditCardInfo cardInfo)
        {
            var result = new CustomerProfileResponse();
            PrepareAuthorizeNet();
            var profile = new customerProfileExType
            {
                merchantCustomerId = Convert.ToString(cardInfo.CustomerId),
                description = " ",
                email = cardInfo.Customer.Email,
                customerProfileId = cardInfo.Customer.CustomerProfileId
            };

            var request = new updateCustomerProfileRequest();
            request.profile = profile;

            // instantiate the controller that will call the service
            var controller = new updateCustomerProfileController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            if (response != null && response.messages != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                //   Console.WriteLine(response.messages.message[0].text);                
                result.refId = response.refId;
                result.resultCode = 0;
                result.sessionToken = response.sessionToken;
                result.Success = true;
                result.message = response.messages.message[0].text;
            }
            else if (response != null)
            {

                result.refId = response.refId;
                result.resultCode = Convert.ToInt32(response.messages.resultCode);
                result.sessionToken = response.sessionToken;
                result.resultCode = Convert.ToInt32(response.messages.resultCode);
                result.Success = false;
                result.message = "Error: " + response.messages.message[0].code + "  " +
                                  response.messages.message[0].text;
            }
            return result;
        }

        public CustomerProfileResponse DeleteCustomerProfile(string customerProfileId)
        {
            //please update the subscriptionId according to your sandbox credentials
            var result = new CustomerProfileResponse();
            PrepareAuthorizeNet();
            var request = new deleteCustomerProfileRequest
            {
                customerProfileId = customerProfileId
            };

            //Prepare Request
            var controller = new deleteCustomerProfileController(request);
            controller.Execute();

            //Send Request to EndPoint
            deleteCustomerProfileResponse response = controller.GetApiResponse();
            if (response != null && response.messages != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response != null && response.messages.message != null)
                {
                    Console.WriteLine("Success, ResultCode : " + response.messages.resultCode.ToString());
                    result.refId = response.refId;
                    result.resultCode = Convert.ToInt32(response.messages.resultCode);
                    result.sessionToken = response.sessionToken;
                    result.Success = true;
                    result.message = response.messages.message[0].text;
                }
            }
            else if (response != null && response.messages.message != null)
            {
                Console.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);
                result.refId = response.refId;
                result.resultCode = Convert.ToInt32(response.messages.resultCode);
                result.sessionToken = response.sessionToken;
                result.Success = false;
                result.message = "Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text;
            }

            return result;
        }

        public CustomerProfileResult GetCustomerProfile(string customerProfileId)
        {

            var result = new CustomerProfileResult();
            PrepareAuthorizeNet();

            var request = new getCustomerProfileRequest();
            request.customerProfileId = customerProfileId;

            // instantiate the controller that will call the service
            var controller = new getCustomerProfileController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            if (response != null)
            {
                if (response.messages != null && response.messages != null && response.messages.resultCode == messageTypeEnum.Error)
                {
                    result.Success = false;
                    result.Message = "Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text;
                    return result;
                }
                var out_profile = response.profile;
                result.Success = true;
                result.CustomerProfileId = customerProfileId;
                result.Email = out_profile.email;
                result.CustomerId = int.Parse(out_profile.merchantCustomerId);
                result.Message = response.messages.message[0].text;
                return result;
            }
            else
            {
                result.Success = false;
                result.Message = "Please Try Again!";
                return result;
            }
        }


        public CustomerPaymentProfileResponse CreateCustomerPaymentProfile(CreditCardInfo paymentProfile)
        {
            //var paymentMethod = LoadPaymentMethodBySystemName("Payments.AuthorizeNet");
            var result = new CustomerPaymentProfileResponse();
            //if (paymentMethod != null)
            //{
            PrepareAuthorizeNet();

            customerPaymentProfileType new_payment_profile = new customerPaymentProfileType();
            paymentType new_payment = new paymentType();
            creditCardType new_card = new creditCardType();
            new_card.cardNumber = paymentProfile.CardNumber;//"4111111111111111";
            new_card.expirationDate = paymentProfile.ExpireYear + "-" + paymentProfile.ExpireMonth.ToString("00"); //"2015-10";
            new_card.cardCode = paymentProfile.CVVNumber.ToString();// "";
            new_payment.Item = new_card;

            new_payment_profile.payment = new_payment;

            if (paymentProfile.Address.Address1 != null && paymentProfile.Address.Address1.Trim() != "")
            {
                // Create the bill to type
                new_payment_profile.billTo = new customerAddressType();
                //new_payment_profile.billTo.firstName = Convert.ToString(paymentProfile.FirstName);
                //new_payment_profile.billTo.lastName = Convert.ToString(paymentProfile.LastName);
                new_payment_profile.billTo.firstName = Convert.ToString(paymentProfile.Address.FirstName);
                new_payment_profile.billTo.lastName = Convert.ToString(paymentProfile.Address.LastName);
                new_payment_profile.billTo.company = "";
                new_payment_profile.billTo.address = ((paymentProfile.Address != null && !string.IsNullOrEmpty(paymentProfile.Address.Address1)) ? Convert.ToString(paymentProfile.Address.Address1 + " " + paymentProfile.Address.Address2) : string.Empty);
                new_payment_profile.billTo.city = ((paymentProfile.Address != null && !string.IsNullOrEmpty(paymentProfile.Address.City)) ? Convert.ToString(paymentProfile.Address.City) : string.Empty);
                new_payment_profile.billTo.state = ((paymentProfile.Address != null && paymentProfile.Address.StateProvince != null) ? Convert.ToString(paymentProfile.Address.StateProvince.Name) : string.Empty);
                new_payment_profile.billTo.zip = ((paymentProfile.Address != null && !string.IsNullOrEmpty(paymentProfile.Address.ZipPostalCode)) ? Convert.ToString(paymentProfile.Address.ZipPostalCode) : string.Empty);
                new_payment_profile.billTo.country = ((paymentProfile.Address != null && paymentProfile.Address.Country != null) ? Convert.ToString(paymentProfile.Address.Country.Name) : string.Empty);
                new_payment_profile.billTo.phoneNumber = ((paymentProfile.Address != null && !string.IsNullOrEmpty(paymentProfile.Address.PhoneNumber)) ? Convert.ToString(paymentProfile.Address.PhoneNumber) : string.Empty);
            }


            var request = new createCustomerPaymentProfileRequest
            {
                customerProfileId = paymentProfile.Customer.CustomerProfileId,
                paymentProfile = new_payment_profile,
                validationMode = validationModeEnum.none
            };

            //Prepare Request
            var controller = new createCustomerPaymentProfileController(request);
            controller.Execute();

            //Send Request to EndPoint
            createCustomerPaymentProfileResponse response = controller.GetApiResponse();
            if (response != null && response.messages != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response != null && response.messages.message != null)
                {
                    //Console.WriteLine("Success, createCustomerPaymentProfileID : " + response.customerPaymentProfileId);
                    result.Success = true;
                    result.PaymentProfileId = Convert.ToInt32(response.customerPaymentProfileId);
                    result.CustomerProfileId = response.customerProfileId;
                    if (paymentProfile.Address != null)
                        paymentProfile.CardHolderName = paymentProfile.Address.FirstName + " " + paymentProfile.Address.LastName;
                    result.PaymentProfile = paymentProfile;
                }
            }
            else
            {
                if (response == null)
                {
                    var errorresponse = controller.GetErrorResponse();
                    if (errorresponse.messages.message.Length > 0)
                    {
                        result.Message = "Error: " + errorresponse.messages.message[0].code + "  " + errorresponse.messages.message[0].text;
                    }
                    else
                        result.Message = "Error: Something Wrong. Try Again!";
                }
                else
                {
                    result.Message = "Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text;
                    if (response.messages.message[0].code == "E00039")
                    {
                        result.Message = "Error: " + "Duplicate ID: " + response.customerPaymentProfileId;
                        result.PaymentProfileId = Convert.ToInt32(response.customerPaymentProfileId);
                        result.CustomerProfileId = response.customerProfileId;
                    }
                }
                result.Success = false;
                result.PaymentProfile = null;
            }

            // }
            //else
            //{
            //    result.Message = "Error: Something Wrong. Try Again!";
            //    result.Success = false;
            //    result.PaymentProfile = null;
            //}
            return result;

        }

        public CustomerPaymentProfileResponse UpdateCustomerPaymentProfile(CreditCardInfo paymentProfile)
        {
            var result = new CustomerPaymentProfileResponse();
            PrepareAuthorizeNet();

            var existsProfile = GetCustomerPaymentProfile(paymentProfile.Customer.CustomerProfileId, paymentProfile.PaymentProfileId);
            // Setup the request
            updateCustomerPaymentProfileRequest request = new updateCustomerPaymentProfileRequest();

            // Create a new credit card
            customerPaymentProfileExType new_payment_profile = new customerPaymentProfileExType();
            paymentType new_payment = new paymentType();
            creditCardType new_card = new creditCardType();
            new_card.cardNumber = existsProfile.CardNumber;
            new_card.expirationDate = existsProfile.ExpireDate;
            new_payment.Item = new_card;
            new_payment_profile.payment = new_payment;

            if (paymentProfile.Address.Address1 != null && paymentProfile.Address.Address1.Trim() != "")
            {
                // Create the bill to type
                new_payment_profile.billTo = new customerAddressType();
                new_payment_profile.billTo.firstName = Convert.ToString(paymentProfile.Address.FirstName);
                new_payment_profile.billTo.lastName = Convert.ToString(paymentProfile.Address.LastName);
                new_payment_profile.billTo.company = "";
                new_payment_profile.billTo.address = Convert.ToString(paymentProfile.Address.Address1 + " " + paymentProfile.Address.Address2);
                new_payment_profile.billTo.city = Convert.ToString(paymentProfile.Address.City);
                new_payment_profile.billTo.state = Convert.ToString(paymentProfile.Address.StateProvince.Name);
                new_payment_profile.billTo.zip = Convert.ToString(paymentProfile.Address.ZipPostalCode);
                new_payment_profile.billTo.country = Convert.ToString(paymentProfile.Address.Country.Name);
                new_payment_profile.billTo.phoneNumber = Convert.ToString(paymentProfile.Address.PhoneNumber);
            }

            // Apply the new card over the existing card
            request.customerProfileId = Convert.ToString(paymentProfile.Customer.CustomerProfileId);
            request.paymentProfile = new_payment_profile;
            request.paymentProfile.customerPaymentProfileId = Convert.ToString(paymentProfile.PaymentProfileId);

            var controller = new updateCustomerPaymentProfileController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            if (response != null && response.messages != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response != null && response.messages.message != null)
                {
                    result.Success = true;
                    result.PaymentProfileId = paymentProfile.Id;
                    result.CustomerProfileId = paymentProfile.Customer.CustomerProfileId;
                    result.PaymentProfile = null;
                    result.Message = response.messages.message[0].text;
                }
            }
            else
            {
                if (response == null)
                    result.Message = "Error: Something Wrong. Try Again!";
                else
                    result.Message = "Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text;

                result.Success = false;
                result.PaymentProfile = null;
            }
            return result;
        }

        public CustomerPaymentProfileResponse DeleteCustomerPaymentProfile(CreditCardInfo paymentProfile)
        {

            var result = new CustomerPaymentProfileResponse();
            PrepareAuthorizeNet();

            deleteCustomerPaymentProfileRequest request = new deleteCustomerPaymentProfileRequest();
            request.customerProfileId = paymentProfile.Customer.CustomerProfileId.ToString();
            request.customerPaymentProfileId = Convert.ToString(paymentProfile.PaymentProfileId);

            //Prepare Request
            var controller = new deleteCustomerPaymentProfileController(request);
            controller.Execute();

            //Send Request to EndPoint
            deleteCustomerPaymentProfileResponse response = controller.GetApiResponse();

            if (response != null && response.messages != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response != null && response.messages.message != null)
                {
                    result.Success = true;
                    result.PaymentProfileId = paymentProfile.PaymentProfileId;
                    result.CustomerProfileId = paymentProfile.Customer.CustomerProfileId;
                    result.PaymentProfile = null;
                    result.Message = response.messages.message[0].text;

                }
            }
            else
            {
                if (response == null)
                    result.Message = "Error: Something Wrong. Try Again!";
                else
                    result.Message = "Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text;

                result.Success = false;
                result.PaymentProfile = null;
            }
            return result;
        }

        public CustomerPaymentProfileResult GetCustomerPaymentProfile(string customerProfileId, long paymentProfileId)
        {
            var result = new CustomerPaymentProfileResult();
            PrepareAuthorizeNet();
            var request = new getCustomerPaymentProfileRequest();
            request.customerProfileId = customerProfileId;
            request.customerPaymentProfileId = Convert.ToString(paymentProfileId);

            // instantiate the controller that will call the service
            var controller = new getCustomerPaymentProfileController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();


            if (response != null && response.messages != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response != null && response.messages.message != null)
                {
                    result.PaymentProfileId = response.paymentProfile.customerPaymentProfileId;
                    result.CustomerProfileId = response.paymentProfile.customerProfileId;
                    if (response.paymentProfile.payment.Item is creditCardMaskedType)
                    {
                        result.CardNumber = (response.paymentProfile.payment.Item as creditCardMaskedType).cardNumber;
                        result.ExpireDate = (response.paymentProfile.payment.Item as creditCardMaskedType).expirationDate;
                    }
                    result.Success = true;
                    result.Message = response.messages.message[0].text;
                }
            }
            else
            {
                if (response == null)
                    result.Message = "Error: Something Wrong. Try Again!";
                else
                    result.Message = "Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text;

                result.PaymentProfileId = Convert.ToString(paymentProfileId);
                result.CustomerProfileId = customerProfileId;
                result.Success = false;
            }
            return result;

        }


        public ProcessPaymentResult RefundTransaction(string customerProfileId, string customerPaymentProfileId, decimal Amount, string orderGuid, string description)
        {
            var result = new ProcessPaymentResult();
            Console.WriteLine("Charge Customer Profile");
            PrepareAuthorizeNet();

            //create a customer payment profile
            customerProfilePaymentType profileToCharge = new customerProfilePaymentType();
            profileToCharge.customerProfileId = customerProfileId;
            profileToCharge.paymentProfile = new paymentProfile { paymentProfileId = customerPaymentProfileId };


            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.refundTransaction.ToString(),    // refund type
                amount = Amount,
                order = new orderType
                {
                    //x_invoice_num is 20 chars maximum. hece we also pass x_description
                    invoiceNumber = orderGuid,
                    description = description
                },
                profile = profileToCharge
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the collector that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();



            if (response == null)
                return result;

            if (response != null && response.messages != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (_authorizeNetPaymentSettings.TransactMode == TransactMode.Authorize)
                {
                    result.AuthorizationTransactionId = response.transactionResponse.transId;
                    result.AuthorizationTransactionCode = string.Format("{0},{1}", response.transactionResponse.transId, response.transactionResponse.authCode);
                }
                if (_authorizeNetPaymentSettings.TransactMode == TransactMode.AuthorizeAndCapture)
                    result.CaptureTransactionId = string.Format("{0},{1}", response.transactionResponse.transId, response.transactionResponse.authCode);

                result.AuthorizationTransactionResult = string.Format("Approved ({0}: {1})", response.transactionResponse.responseCode, response.transactionResponse.messages[0].description);
                result.AvsResult = response.transactionResponse.avsResultCode;
                result.NewPaymentStatus = _authorizeNetPaymentSettings.TransactMode == TransactMode.Authorize ? PaymentStatus.Authorized : PaymentStatus.Paid;
            }
            else
            {
                if (response != null && response.transactionResponse.errors != null && response.transactionResponse.errors.Count() > 0)
                {
                    foreach (var e in response.transactionResponse.errors)
                        result.Errors.Add("Error Code :" + e.errorCode + " " + e.errorText);
                }
                else
                {
                    if (response.messages != null)
                        result.Errors.Add("Error Code :" + response.messages.message[0]);
                    else
                        result.Errors.Add("Something wrong try again!.");
                }
            }
            return result;


        }

        public ProcessPaymentResult AuthorizeTransaction(string customerProfileId, string customerPaymentProfileId, decimal Amount, string ordernumber, string description)
        {
            var result = new ProcessPaymentResult();
            Console.WriteLine("Charge Customer Profile");
            PrepareAuthorizeNet();

            //create a customer payment profile
            customerProfilePaymentType profileToCharge = new customerProfilePaymentType();
            profileToCharge.customerProfileId = customerProfileId;
            profileToCharge.paymentProfile = new paymentProfile { paymentProfileId = customerPaymentProfileId };


            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authOnlyTransaction.ToString(),    // refund type
                amount = Amount,
                order = new orderType
                {
                    //x_invoice_num is 20 chars maximum. hece we also pass x_description
                    invoiceNumber = ordernumber,
                    description = description
                },
                profile = profileToCharge
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the collector that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();



            if (response == null)
                return result;

            if (response != null && response.messages != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (_authorizeNetPaymentSettings.TransactMode == TransactMode.Authorize)
                {
                    result.AuthorizationTransactionId = response.transactionResponse.transId;
                    result.AuthorizationTransactionCode = string.Format("{0},{1}", response.transactionResponse.transId, response.transactionResponse.authCode);
                }
                if (_authorizeNetPaymentSettings.TransactMode == TransactMode.AuthorizeAndCapture)
                    result.CaptureTransactionId = string.Format("{0},{1}", response.transactionResponse.transId, response.transactionResponse.authCode);

                result.AuthorizationTransactionResult = string.Format("Approved ({0}: {1})", response.transactionResponse.responseCode, response.transactionResponse.messages[0].description);
                result.AvsResult = response.transactionResponse.avsResultCode;
                result.NewPaymentStatus = _authorizeNetPaymentSettings.TransactMode == TransactMode.Authorize ? PaymentStatus.Authorized : PaymentStatus.Paid;
            }
            else
            {
                if (response != null && response.transactionResponse.errors != null && response.transactionResponse.errors.Count() > 0)
                {
                    foreach (var e in response.transactionResponse.errors)
                        result.Errors.Add("Error Code :" + e.errorCode + " " + e.errorText);
                }
                else
                {
                    if (response.messages != null)
                        result.Errors.Add("Error Code :" + response.messages.message[0]);
                    else
                        result.Errors.Add("Something wrong try again!.");
                }
            }
            return result;


        }
    }
}
