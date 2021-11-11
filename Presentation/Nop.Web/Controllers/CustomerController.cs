using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tax;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Security.Captcha;
using Nop.Web.Framework.Security.Honeypot;
using Nop.Web.Models.Customer;
using Nop.Services.ZohoDesk;
using Nop.Core.Infrastructure;
using Nop.Core.Data;
using Nop.Core.Domain.ZohoDesk;
using Nop.Services.Payments;
using Nop.Core.Plugins;
using Nop.Core.Domain.Payments;
using Nop.Services.Security;

namespace Nop.Web.Controllers
{
    public partial class CustomerController : BasePublicController
    {
        #region Fields

        private readonly IAddressModelFactory _addressModelFactory;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IAuthenticationService _authenticationService;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ITaxService _taxService;
        private readonly CustomerSettings _customerSettings;
        private readonly AddressSettings _addressSettings;
        private readonly ForumSettings _forumSettings;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IStoreService _storeService;
        private readonly IEventPublisher _eventPublisher;

        private readonly MediaSettings _mediaSettings;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly StoreInformationSettings _storeInformationSettings;

        private readonly ICreditCardInfoService _creditCardInfoService;
        private readonly IStateProvinceService _stateProvinceService;
        #endregion

        #region Ctor

        public CustomerController(IAddressModelFactory addressModelFactory,
            ICustomerModelFactory customerModelFactory,
            IAuthenticationService authenticationService,
            DateTimeSettings dateTimeSettings,
            TaxSettings taxSettings,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            ITaxService taxService,
            CustomerSettings customerSettings,
            AddressSettings addressSettings,
            ForumSettings forumSettings,
            IAddressService addressService,
            ICountryService countryService,
            IOrderService orderService,
            IPictureService pictureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IShoppingCartService shoppingCartService,
            IOpenAuthenticationService openAuthenticationService,
            IWebHelper webHelper,
            ICustomerActivityService customerActivityService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IStoreService storeService,
            IEventPublisher eventPublisher,
            MediaSettings mediaSettings,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            CaptchaSettings captchaSettings,
            StoreInformationSettings storeInformationSettings)
        {
            this._addressModelFactory = addressModelFactory;
            this._customerModelFactory = customerModelFactory;
            this._authenticationService = authenticationService;
            this._dateTimeSettings = dateTimeSettings;
            this._taxSettings = taxSettings;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._customerService = customerService;
            this._customerAttributeParser = customerAttributeParser;
            this._customerAttributeService = customerAttributeService;
            this._genericAttributeService = genericAttributeService;
            this._customerRegistrationService = customerRegistrationService;
            this._taxService = taxService;
            this._customerSettings = customerSettings;
            this._addressSettings = addressSettings;
            this._forumSettings = forumSettings;
            this._addressService = addressService;
            this._countryService = countryService;
            this._orderService = orderService;
            this._pictureService = pictureService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._shoppingCartService = shoppingCartService;
            this._openAuthenticationService = openAuthenticationService;
            this._webHelper = webHelper;
            this._customerActivityService = customerActivityService;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;
            this._storeService = storeService;
            this._eventPublisher = eventPublisher;
            this._mediaSettings = mediaSettings;
            this._workflowMessageService = workflowMessageService;
            this._localizationSettings = localizationSettings;
            this._captchaSettings = captchaSettings;
            this._storeInformationSettings = storeInformationSettings;
            _creditCardInfoService = EngineContext.Current.Resolve<ICreditCardInfoService>();
            _stateProvinceService= EngineContext.Current.Resolve<IStateProvinceService>();
        }
        
        #endregion

        #region Utilities

        [NonAction]
        protected virtual void TryAssociateAccountWithExternalAccount(Customer customer)
        {
            var parameters = ExternalAuthorizerHelper.RetrieveParametersFromRoundTrip(true);
            if (parameters == null)
                return;

            if (_openAuthenticationService.AccountExists(parameters))
                return;

            _openAuthenticationService.AssociateExternalAccountWithUser(customer, parameters);
        }

        [NonAction]
        protected virtual string ParseCustomCustomerAttributes(FormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            string attributesXml = "";
            var attributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in attributes)
            {
                string controlId = string.Format("customer_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!String.IsNullOrEmpty(ctrlAttributes))
                        {
                            int selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }
                        break;
                    case AttributeControlType.Checkboxes:
                    {
                        var cblAttributes = form[controlId];
                        if (!String.IsNullOrEmpty(cblAttributes))
                        {
                            foreach (var item in cblAttributes.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                            )
                            {
                                int selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                    }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //load read-only (already server-side selected) values
                        var attributeValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                        {
                            attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                        }
                    }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!String.IsNullOrEmpty(ctrlAttributes))
                        {
                            string enteredText = ctrlAttributes.Trim();
                            attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                attribute, enteredText);
                        }
                    }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported customer attributes
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        #endregion

        #region Login / logout

        [NopHttpsRequirement(SslRequirement.Yes)]
        //available even when a store is closed
        [StoreClosed(true)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult Login(bool? checkoutAsGuest)
        {
            var model = _customerModelFactory.PrepareLoginModel(checkoutAsGuest);
            return View(model);
        }

        [HttpPost]
        [CaptchaValidator]
        //available even when a store is closed
        [StoreClosed(true)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult Login(LoginModel model, string returnUrl, bool captchaValid)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }
                var loginResult =
                    _customerRegistrationService.ValidateCustomer(
                        _customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                    {
                        var customer = _customerSettings.UsernamesEnabled
                            ? _customerService.GetCustomerByUsername(model.Username)
                            : _customerService.GetCustomerByEmail(model.Email);

                        //migrate shopping cart
                        _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer, true);

                        //sign in new customer
                        _authenticationService.SignIn(customer, model.RememberMe);

                        //raise event       
                        _eventPublisher.Publish(new CustomerLoggedinEvent(customer));

                        //activity log
                        _customerActivityService.InsertActivity(customer, "PublicStore.Login", _localizationService.GetResource("ActivityLog.PublicStore.Login"));

                            //if (String.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                            if (String.IsNullOrEmpty(returnUrl))
                                return RedirectToRoute("HomePage");

                        return Redirect(returnUrl);
                    }
                    case CustomerLoginResults.CustomerNotExist:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist"));
                        break;
                    case CustomerLoginResults.Deleted:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.Deleted"));
                        break;
                    case CustomerLoginResults.NotActive:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotActive"));
                        break;
                    case CustomerLoginResults.NotRegistered:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered"));
                        break;
                    case CustomerLoginResults.LockedOut:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.LockedOut"));
                        break;
                    case CustomerLoginResults.WrongPassword:
                    default:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials"));
                        break;
                }
            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareLoginModel(model.CheckoutAsGuest);
            return View(model);
        }

        //available even when a store is closed
        [StoreClosed(true)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult Logout()
        {
            //external authentication
            ExternalAuthorizerHelper.RemoveParameters();

            if (_workContext.OriginalCustomerIfImpersonated != null)
            {
                //activity log
                _customerActivityService.InsertActivity(_workContext.OriginalCustomerIfImpersonated,
                    "Impersonation.Finished",
                    _localizationService.GetResource("ActivityLog.Impersonation.Finished.StoreOwner"),
                    _workContext.CurrentCustomer.Email, _workContext.CurrentCustomer.Id);
                _customerActivityService.InsertActivity("Impersonation.Finished",
                    _localizationService.GetResource("ActivityLog.Impersonation.Finished.Customer"),
                    _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id);

                //logout impersonated customer
                _genericAttributeService.SaveAttribute<int?>(_workContext.OriginalCustomerIfImpersonated,
                    SystemCustomerAttributeNames.ImpersonatedCustomerId, null);

                //redirect back to customer details page (admin area)
                return this.RedirectToAction("Edit", "Customer",
                    new {id = _workContext.CurrentCustomer.Id, area = "Admin"});

            }

            //activity log
            _customerActivityService.InsertActivity("PublicStore.Logout", _localizationService.GetResource("ActivityLog.PublicStore.Logout"));
            
            //standard logout 
            _authenticationService.SignOut();

            //raise logged out event       
            _eventPublisher.Publish(new CustomerLoggedOutEvent(_workContext.CurrentCustomer));

            //EU Cookie
            if (_storeInformationSettings.DisplayEuCookieLawWarning)
            {
                //the cookie law message should not pop up immediately after logout.
                //otherwise, the user will have to click it again...
                //and thus next visitor will not click it... so violation for that cookie law..
                //the only good solution in this case is to store a temporary variable
                //indicating that the EU cookie popup window should not be displayed on the next page open (after logout redirection to homepage)
                //but it'll be displayed for further page loads
                TempData["nop.IgnoreEuCookieLawWarning"] = true;
            }

            return RedirectToRoute("HomePage");
        }

        #endregion

        #region Password recovery

        [NopHttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult PasswordRecovery()
        {
            var model = _customerModelFactory.PreparePasswordRecoveryModel();
            return View(model);
        }

        [HttpPost, ActionName("PasswordRecovery")]
        [PublicAntiForgery]
        [FormValueRequired("send-email")]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult PasswordRecoverySend(PasswordRecoveryModel model)
        {
            model.Status =false;
            if (ModelState.IsValid)
            {
                var customer = _customerService.GetCustomerByEmail(model.Email);
                if (customer != null && customer.Active && !customer.Deleted)
                {
                    //save token and current date
                    var passwordRecoveryToken = Guid.NewGuid();
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken,
                        passwordRecoveryToken.ToString());
                    DateTime? generatedDateTime = DateTime.UtcNow;
                    _genericAttributeService.SaveAttribute(customer,
                        SystemCustomerAttributeNames.PasswordRecoveryTokenDateGenerated, generatedDateTime);

                    //send email
                    _workflowMessageService.SendCustomerPasswordRecoveryMessage(customer,
                        _workContext.WorkingLanguage.Id);

                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.EmailHasBeenSent");
                    model.Status = true;
                }
                else
                {
                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.EmailNotFound");
                }

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }


        [NopHttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult PasswordRecoveryConfirm(string token, string email)
        {
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return RedirectToRoute("HomePage");

            if (string.IsNullOrEmpty(customer.GetAttribute<string>(SystemCustomerAttributeNames.PasswordRecoveryToken)))
            {
                return View(new PasswordRecoveryConfirmModel
                {
                    DisablePasswordChanging = true,
                    Result = _localizationService.GetResource("Account.PasswordRecovery.PasswordAlreadyHasBeenChanged")
                });
            }

            var model = _customerModelFactory.PreparePasswordRecoveryConfirmModel();

            //validate token
            if (!customer.IsPasswordRecoveryTokenValid(token))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.WrongToken");
            }

            //validate token expiration date
            if (customer.IsPasswordRecoveryLinkExpired(_customerSettings))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.LinkExpired");
            }

            return View(model);
        }

        [HttpPost, ActionName("PasswordRecoveryConfirm")]
        [PublicAntiForgery]
        [FormValueRequired("set-password")]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult PasswordRecoveryConfirmPOST(string token, string email, PasswordRecoveryConfirmModel model)
        {
            model.Status = false;
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return RedirectToRoute("HomePage");

            //validate token
            if (!customer.IsPasswordRecoveryTokenValid(token))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.WrongToken");
                return View(model);
            }

            //validate token expiration date
            if (customer.IsPasswordRecoveryLinkExpired(_customerSettings))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.LinkExpired");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var response = _customerRegistrationService.ChangePassword(new ChangePasswordRequest(email,
                    false, _customerSettings.DefaultPasswordFormat, model.NewPassword));
                if (response.Success)
                {
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken,
                        "");

                    model.DisablePasswordChanging = true;
                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.PasswordHasBeenChanged");
                    model.Status = true;
                }
                else
                {
                    model.Result = response.Errors.FirstOrDefault();
                }

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Register

        [NopHttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult Register()
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new {resultId = (int) UserRegistrationType.Disabled});

            var model = new RegisterModel();
            model = _customerModelFactory.PrepareRegisterModel(model, false, setDefaultValues: true);

            return View(model);
        }

        [HttpPost]
        [CaptchaValidator]
        [HoneypotValidator]
        [PublicAntiForgery]
        [ValidateInput(false)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult Register(RegisterModel model, string returnUrl, bool captchaValid, FormCollection form)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new {resultId = (int) UserRegistrationType.Disabled});

            if (_workContext.CurrentCustomer.IsRegistered())
            {
                //Already registered customer. 
                _authenticationService.SignOut();

                //raise logged out event       
                _eventPublisher.Publish(new CustomerLoggedOutEvent(_workContext.CurrentCustomer));

                //Save a new record
                _workContext.CurrentCustomer = _customerService.InsertGuestCustomer();
            }
            var customer = _workContext.CurrentCustomer;
            customer.RegisteredInStoreId = _storeContext.CurrentStore.Id;

            //custom customer attributes
            var customerAttributesXml = ParseCustomCustomerAttributes(form);
            var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            // Validation Rules for customer
            if (ValidationRulesForNewCustomer.ValidationRulesForNewCustomerByEmail(model.Email))
            {
                ModelState.AddModelError("", _localizationService.GetResource("Newsletter.Email.Wrong"));
            }

            if (ValidationRulesForNewCustomer.ValidationRulesForNewCustomerByCompanyName(model.Company))
            {
                ModelState.AddModelError("", _localizationService.GetResource("Newsletter.Company.Wrong"));
            }

            string fullName = model.FirstName + ' ' + model.LastName;
            if (ValidationRulesForNewCustomer.ValidationRulesForNewCustomerByFullName(fullName))
            {
                ModelState.AddModelError("", _localizationService.GetResource("Newsletter.FullName.Wrong"));
            }

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }

                bool isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                var registrationRequest = new CustomerRegistrationRequest(customer,
                    model.Email,
                    _customerSettings.UsernamesEnabled ? model.Username : model.Email,
                    model.Password,
                    _customerSettings.DefaultPasswordFormat,
                    _storeContext.CurrentStore.Id,
                    isApproved);
                var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
                if (registrationResult.Success)
                {
                    var paymentMethod = LoadPaymentMethodBySystemName("Payments.AuthorizeNet");
                    ILogger _logger = EngineContext.Current.Resolve<ILogger>();
                    if (paymentMethod != null)
                    {
                        CustomerProfileResponse resultCustomerProfile = new CustomerProfileResponse();
                        if (string.IsNullOrEmpty(customer.CustomerProfileId))
                        {
                            resultCustomerProfile = paymentMethod.CreateCustomerProfile(customer);
                            if (resultCustomerProfile.Success)
                            {
                                _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "Successfully created custome profile ");
                                customer.CustomerProfileId = resultCustomerProfile.customerProfileId;
                                _customerService.UpdateCustomer(customer);
                            }
                            else
                            {
                                _logger.InsertLog(Core.Domain.Logging.LogLevel.Error, "Error while creating custome profile ", resultCustomerProfile.message);
                                //response.Success = false;
                                //response.Error = resultCustomerProfile.message;
                            }
                        }
                    }

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
                    }
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);

                        string vatName;
                        string vatAddress;
                        var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out vatName,
                            out vatAddress);
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumberStatusId, (int) vatNumberStatus);
                        //send VAT number admin notification
                        if (!String.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);

                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        DateTime? dateOfBirth = model.ParseDateOfBirth();
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, dateOfBirth);
                    }
                    if (_customerSettings.CompanyEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
                    if (_customerSettings.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
                    if (_customerSettings.CityEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                    if (_customerSettings.CountryEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId,
                            model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(model.Email, _storeContext.CurrentStore.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                            }
                            //else
                            //{
                            //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                            //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                            //}
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = model.Email,
                                    Active = true,
                                    StoreId = _storeContext.CurrentStore.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    //save customer attributes
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, customerAttributesXml);

                    //login customer now
                    if (isApproved)
                        _authenticationService.SignIn(customer, true);

                    //associated with external account (if possible)
                    TryAssociateAccountWithExternalAccount(customer);

                    //insert default address (if possible)
                    var defaultAddress = new Address
                    {
                        FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                        LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                        Email = customer.Email,
                        Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company),
                        CountryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId) > 0
                            ? (int?) customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId)
                            : null,
                        StateProvinceId = customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId) > 0
                            ? (int?) customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId)
                            : null,
                        City = customer.GetAttribute<string>(SystemCustomerAttributeNames.City),
                        Address1 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress),
                        Address2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2),
                        ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode),
                        PhoneNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                        FaxNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax),
                        CreatedOnUtc = customer.CreatedOnUtc
                    };
                    if (this._addressService.IsAddressValid(defaultAddress))
                    {
                        //some validation
                        if (defaultAddress.CountryId == 0)
                            defaultAddress.CountryId = null;
                        if (defaultAddress.StateProvinceId == 0)
                            defaultAddress.StateProvinceId = null;
                        //set default address
                        customer.Addresses.Add(defaultAddress);
                        customer.BillingAddress = defaultAddress;
                        customer.ShippingAddress = defaultAddress;
                        _customerService.UpdateCustomer(customer);
                    }

                    //notifications
                    if (_customerSettings.NotifyNewCustomerRegistration)
                        _workflowMessageService.SendCustomerRegisteredNotificationMessage(customer,
                            _localizationSettings.DefaultAdminLanguageId);

                    if (_customerSettings.NewsletterEnabled) {
                        if (model.Newsletter) {
                            _workflowMessageService.SendNewsLetterSubscriptionActivationMessage(new NewsLetterSubscription
                            {
                                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                Email = model.Email,
                                Active = true,
                                StoreId = _storeContext.CurrentStore.Id,
                                CreatedOnUtc = DateTime.UtcNow
                            }, _localizationSettings.DefaultAdminLanguageId);
                        }
                    }

                    //raise event       
                    _eventPublisher.Publish(new CustomerRegisteredEvent(customer));

                    switch (_customerSettings.UserRegistrationType)
                    {
                        case UserRegistrationType.EmailValidation:
                        {
                            //email validation message
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
                            _workflowMessageService.SendCustomerEmailValidationMessage(customer, _workContext.WorkingLanguage.Id);

                            //result
                            return RedirectToRoute("RegisterResult",
                                new {resultId = (int) UserRegistrationType.EmailValidation});
                        }
                        case UserRegistrationType.AdminApproval:
                        {
                            return RedirectToRoute("RegisterResult",
                                new {resultId = (int) UserRegistrationType.AdminApproval});
                        }
                        case UserRegistrationType.Standard:
                        {
                            //send customer welcome message
                            _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

                            var redirectUrl = Url.RouteUrl("RegisterResult", new {resultId = (int) UserRegistrationType.Standard});
                            if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                                redirectUrl = _webHelper.ModifyQueryString(redirectUrl, "returnurl=" + HttpUtility.UrlEncode(returnUrl), null);
                            return Redirect(redirectUrl);
                        }
                        default:
                        {
                            return RedirectToRoute("HomePage");
                        }
                    }
                }

                //errors
                foreach (var error in registrationResult.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareRegisterModel(model, true, customerAttributesXml);
            return View(model);
        }

        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult RegisterResult(int resultId)
        {
            var model = _customerModelFactory.PrepareRegisterResultModel(resultId);
            return View(model);
        }

        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        [HttpPost]
        public virtual ActionResult RegisterResult(string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return RedirectToRoute("HomePage");

            return Redirect(returnUrl);
        }

        [HttpPost]
        [PublicAntiForgery]
        [ValidateInput(false)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult CheckUsernameAvailability(string username)
        {
            var usernameAvailable = false;
            var statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.NotAvailable");

            if (_customerSettings.UsernamesEnabled && !String.IsNullOrWhiteSpace(username))
            {
                if (_workContext.CurrentCustomer != null &&
                    _workContext.CurrentCustomer.Username != null &&
                    _workContext.CurrentCustomer.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                {
                    statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.CurrentUsername");
                }
                else
                {
                    var customer = _customerService.GetCustomerByUsername(username);
                    if (customer == null)
                    {
                        statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.Available");
                        usernameAvailable = true;
                    }
                }
            }

            return Json(new {Available = usernameAvailable, Text = statusText});
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult AccountActivation(string token, string email)
        {
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return RedirectToRoute("HomePage");

            var cToken = customer.GetAttribute<string>(SystemCustomerAttributeNames.AccountActivationToken);
            if (string.IsNullOrEmpty(cToken))
                return
                    View(new AccountActivationModel
                    {
                        Result = _localizationService.GetResource("Account.AccountActivation.AlreadyActivated")
                    });

            if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return RedirectToRoute("HomePage");

            //activate user account
            customer.Active = true;
            _customerService.UpdateCustomer(customer);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AccountActivationToken, "");
            //send welcome message
            _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

            var model = new AccountActivationModel();
            model.Result = _localizationService.GetResource("Account.AccountActivation.Activated");
            return View(model);
        }

        #endregion

        #region My account / Info

        [ChildActionOnly]
        public virtual ActionResult CustomerNavigation(int selectedTabId = 0)
        {
            var model = _customerModelFactory.PrepareCustomerNavigationModel(selectedTabId);
            return PartialView(model);
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult Info()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var model = new CustomerInfoModel();
            model = _customerModelFactory.PrepareCustomerInfoModel(model, _workContext.CurrentCustomer, false);

            var _zohoContactRepository = EngineContext.Current.Resolve<IRepository<ZohoContact>>();
            var zohoContact = _zohoContactRepository.Table.Where(x => x.zohoContactEmail == model.Email).FirstOrDefault();
            if (zohoContact != null)
            {
                model.ZohocontactId = zohoContact.zohoContactId;
            }
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        [ValidateInput(false)]
        public virtual ActionResult Info(CustomerInfoModel model, FormCollection form)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            //custom customer attributes
            var customerAttributesXml = ParseCustomCustomerAttributes(form);
            var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            try
            {
                if (ModelState.IsValid)
                {
                    //username 
                    if (_customerSettings.UsernamesEnabled && this._customerSettings.AllowUsersToChangeUsernames)
                    {
                        if (
                            !customer.Username.Equals(model.Username.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            //change username
                            _customerRegistrationService.SetUsername(customer, model.Username.Trim());

                            //re-authenticate
                            //do not authenticate users in impersonation mode
                            if (_workContext.OriginalCustomerIfImpersonated == null)
                                _authenticationService.SignIn(customer, true);
                        }
                    }
                    //email
                    if (!customer.Email.Equals(model.Email.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change email
                        var requireValidation = _customerSettings.UserRegistrationType ==
                                                UserRegistrationType.EmailValidation;
                        _customerRegistrationService.SetEmail(customer, model.Email.Trim(), requireValidation);

                        //do not authenticate users in impersonation mode
                        if (_workContext.OriginalCustomerIfImpersonated == null)
                        {
                            //re-authenticate (if usernames are disabled)
                            if (!_customerSettings.UsernamesEnabled && !requireValidation)
                                _authenticationService.SignIn(customer, true);
                        }
                    }

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId,
                            model.TimeZoneId);
                    }
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        var prevVatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);

                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber,
                            model.VatNumber);
                        if (prevVatNumber != model.VatNumber)
                        {
                            string vatName;
                            string vatAddress;
                            var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out vatName,
                                out vatAddress);
                            _genericAttributeService.SaveAttribute(customer,
                                SystemCustomerAttributeNames.VatNumberStatusId, (int) vatNumberStatus);
                            //send VAT number admin notification
                            if (!String.IsNullOrEmpty(model.VatNumber) &&
                                _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer,
                                    model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                        }
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender,
                            model.Gender);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName,
                        model.FirstName);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName,
                        model.LastName);
                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        DateTime? dateOfBirth = model.ParseDateOfBirth();
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth,
                            dateOfBirth);
                    }
                    if (_customerSettings.CompanyEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company,
                            model.Company);
                    if (_customerSettings.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress,
                            model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2,
                            model.StreetAddress2);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode,
                            model.ZipPostalCode);
                    if (_customerSettings.CityEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                    if (_customerSettings.CountryEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId,
                            model.CountryId);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId,
                            model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var newsletter =
                            _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email,
                                _storeContext.CurrentStore.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                            }
                            else
                                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customer.Email,
                                    Active = true,
                                    StoreId = _storeContext.CurrentStore.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Signature,
                            model.Signature);

                    //save customer attributes
                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        SystemCustomerAttributeNames.CustomCustomerAttributes, customerAttributesXml);

                    try {
                        CreateUpdateZohoContact(model, customer);
                    }
                    catch (Exception ex) {

                    }
                    

                    return RedirectToRoute("CustomerInfo");
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }


            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareCustomerInfoModel(model, customer, true, customerAttributesXml);
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual ActionResult RemoveExternalAssociation(int id)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            //ensure it's our record
            var ear = _openAuthenticationService.GetExternalIdentifiersFor(_workContext.CurrentCustomer)
                .FirstOrDefault(x => x.Id == id);

            if (ear == null)
            {
                return Json(new
                {
                    redirect = Url.Action("Info"),
                });
            }

            _openAuthenticationService.DeleteExternalAuthenticationRecord(ear);

            return Json(new
            {
                redirect = Url.Action("Info"),
            });
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult EmailRevalidation(string token, string email)
        {
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return RedirectToRoute("HomePage");

            var cToken = customer.GetAttribute<string>(SystemCustomerAttributeNames.EmailRevalidationToken);
            if (string.IsNullOrEmpty(cToken))
                return View(new EmailRevalidationModel
                {
                        Result = _localizationService.GetResource("Account.EmailRevalidation.AlreadyChanged")
                    });

            if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return RedirectToRoute("HomePage");

            if (String.IsNullOrEmpty(customer.EmailToRevalidate))
                return RedirectToRoute("HomePage");

            if (_customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation)
                return RedirectToRoute("HomePage");

            //change email
            try
            {
                _customerRegistrationService.SetEmail(customer, customer.EmailToRevalidate, false);
            }
            catch (Exception exc)
            {
                return View(new EmailRevalidationModel
                {
                    Result = _localizationService.GetResource(exc.Message)
                });
            }
            customer.EmailToRevalidate = null;
            _customerService.UpdateCustomer(customer);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.EmailRevalidationToken, "");

            //re-authenticate (if usernames are disabled)
            if (!_customerSettings.UsernamesEnabled)
            {
                _authenticationService.SignIn(customer, true);
            }

            var model = new EmailRevalidationModel()
            {
                Result = _localizationService.GetResource("Account.EmailRevalidation.Changed")
            };
            return View(model);
        }

        #endregion

        #region My account / Addresses

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult Addresses()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var model = _customerModelFactory.PrepareCustomerAddressListModel();
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult AddressDelete(int addressId)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address != null)
            {
                customer.RemoveAddress(address);
                _customerService.UpdateCustomer(customer);
                //now delete the address record
                _addressService.DeleteAddress(address);
            }

            //redirect to the address list page
            return Json(new
            {
                redirect = Url.RouteUrl("CustomerAddresses"),
            });
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult AddressAdd()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var model = new CustomerAddressEditModel();
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: null,
                excludeProperties: false,
                addressSettings:_addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        [ValidateInput(false)]
        public virtual ActionResult AddressAdd(CustomerAddressEditModel model, FormCollection form)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            //custom address attributes
            var customAttributes = form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                customer.Addresses.Add(address);
                _customerService.UpdateCustomer(customer);

                return RedirectToRoute("CustomerAddresses");
            }

            //If we got this far, something failed, redisplay form
            _addressModelFactory.PrepareAddressModel(model.Address, 
                address: null,
                excludeProperties: true,
                addressSettings:_addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id),
                overrideAttributesXml: customAttributes);

            return View(model);
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult AddressEdit(int addressId)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;
            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address == null)
                //address is not found
                return RedirectToRoute("CustomerAddresses");

            var model = new CustomerAddressEditModel();
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        [ValidateInput(false)]
        public virtual ActionResult AddressEdit(CustomerAddressEditModel model, int addressId, FormCollection form)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;
            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address == null)
                //address is not found
                return RedirectToRoute("CustomerAddresses");

            //custom address attributes
            var customAttributes = form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;
                _addressService.UpdateAddress(address);

                return RedirectToRoute("CustomerAddresses");
            }

            //If we got this far, something failed, redisplay form
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: address,
                excludeProperties: true,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id),
                overrideAttributesXml: customAttributes);
            return View(model);
        }

        #endregion

        #region My account / Downloadable products

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult DownloadableProducts()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            if (_customerSettings.HideDownloadableProductsTab)
                return RedirectToRoute("CustomerInfo");

            var model = _customerModelFactory.PrepareCustomerDownloadableProductsModel();
            return View(model);
        }

        public virtual ActionResult UserAgreement(Guid orderItemId)
        {
            var orderItem = _orderService.GetOrderItemByGuid(orderItemId);
            if (orderItem == null)
                return RedirectToRoute("HomePage");

            var product = orderItem.Product;
            if (product == null || !product.HasUserAgreement)
                return RedirectToRoute("HomePage");

            var model = _customerModelFactory.PrepareUserAgreementModel(orderItem, product);
            return View(model);
        }

        #endregion

        #region My account / Change password

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult ChangePassword()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var model = _customerModelFactory.PrepareChangePasswordModel();

            //display the cause of the change password 
            if (_workContext.CurrentCustomer.PasswordIsExpired())
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Account.ChangePassword.PasswordIsExpired"));

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            if (ModelState.IsValid)
            {
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                var changePasswordResult = _customerRegistrationService.ChangePassword(changePasswordRequest);
                if (changePasswordResult.Success)
                {
                    model.Result = _localizationService.GetResource("Account.ChangePassword.Success");
                    model.status = true;
                    return View(model);
                }
                
                //errors
                foreach (var error in changePasswordResult.Errors)
                    ModelState.AddModelError("", error);
            }


            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region My account / Avatar

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult Avatar()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return RedirectToRoute("CustomerInfo");

            var model = new CustomerAvatarModel();
            model = _customerModelFactory.PrepareCustomerAvatarModel(model);
            return View(model);
        }

        [HttpPost, ActionName("Avatar")]
        [PublicAntiForgery]
        [FormValueRequired("upload-avatar")]
        public virtual ActionResult UploadAvatar(CustomerAvatarModel model, HttpPostedFileBase uploadedFile)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return RedirectToRoute("CustomerInfo");

            var customer = _workContext.CurrentCustomer;
            
            if (ModelState.IsValid)
            {
                try
                {
                    var customerAvatar = _pictureService.GetPictureById(customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId));
                    if ((uploadedFile != null) && (!String.IsNullOrEmpty(uploadedFile.FileName)))
                    {
                        int avatarMaxSize = _customerSettings.AvatarMaximumSizeBytes;
                        if (uploadedFile.ContentLength > avatarMaxSize)
                            throw new NopException(string.Format(_localizationService.GetResource("Account.Avatar.MaximumUploadedFileSize"), avatarMaxSize));

                        byte[] customerPictureBinary = uploadedFile.GetPictureBits();
                        if (customerAvatar != null)
                            customerAvatar = _pictureService.UpdatePicture(customerAvatar.Id, customerPictureBinary, uploadedFile.ContentType, null);
                        else
                            customerAvatar = _pictureService.InsertPicture(customerPictureBinary, uploadedFile.ContentType, null);
                    }

                    int customerAvatarId = 0;
                    if (customerAvatar != null)
                        customerAvatarId = customerAvatar.Id;

                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AvatarPictureId, customerAvatarId);

                    model.AvatarUrl = _pictureService.GetPictureUrl(
                        customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId),
                        _mediaSettings.AvatarPictureSize,
                        false);
                    return View(model);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError("", exc.Message);
                }
            }


            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareCustomerAvatarModel(model);
            return View(model);
        }

        [HttpPost, ActionName("Avatar")]
        [PublicAntiForgery]
        [FormValueRequired("remove-avatar")]
        public virtual ActionResult RemoveAvatar(CustomerAvatarModel model, HttpPostedFileBase uploadedFile)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return RedirectToRoute("CustomerInfo");

            var customer = _workContext.CurrentCustomer;

            var customerAvatar = _pictureService.GetPictureById(customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId));
            if (customerAvatar != null)
                _pictureService.DeletePicture(customerAvatar);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AvatarPictureId, 0);

            return RedirectToRoute("CustomerAvatar");
        }

        #endregion

        #region Update Zoho Contact
        public virtual void CreateUpdateZohoContact(CustomerInfoModel model, Customer customer)
        {
            var _zohoDeskApi = EngineContext.Current.Resolve<IZohoDeskApi>();
            var _zohoContactRepository = EngineContext.Current.Resolve<IRepository<ZohoContact>>();
            
            if (string.IsNullOrEmpty(model.ZohocontactId))
            {
                //create contact
                ResponseZohoContact ResponseZohoContact = _zohoDeskApi.CreateNewContact(model.Email, customer.GetFullName());
                if (ResponseZohoContact != null)
                    if (!string.IsNullOrEmpty(ResponseZohoContact.id))
                    {
                        ZohoContact contact = new ZohoContact();
                        contact.CreateDate = DateTime.UtcNow;
                        contact.zohoContactEmail = ResponseZohoContact.email;
                        contact.zohoContactId = ResponseZohoContact.id;
                        _zohoContactRepository.Insert(contact);
                    }
            }
            else {
                //update contact
                ResponseZohoContact ResponseZohoContact = _zohoDeskApi.UpdateZohoContact(model.Email, customer.GetFullName(), model.ZohocontactId);
                if (ResponseZohoContact != null)
                    if (!string.IsNullOrEmpty(ResponseZohoContact.id))
                    {
                        ZohoContact contact = _zohoContactRepository.Table.Where(x => x.zohoContactId == model.ZohocontactId).FirstOrDefault();
                        if (contact != null)
                        {
                            contact.zohoContactEmail = ResponseZohoContact.email;
                            _zohoContactRepository.Update(contact);
                        }
                    }
            }
        }
        public virtual IPaymentMethod LoadPaymentMethodBySystemName(string systemName)
        {
            var _pluginFinder = EngineContext.Current.Resolve<IPluginFinder>();
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IPaymentMethod>(systemName);
            if (descriptor != null)
                return descriptor.Instance<IPaymentMethod>();

            return null;
        }
        #endregion

        #region CreditCard
        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult CustomerCreditCards()
        {
            //string path1 = Server.MapPath("~/Zoho/CustProfile.xlsx");

            //string connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
            //var paymentInfo = new CreditCardInfo();
            //DataTable dt = ConvertXSLXtoDataTable(path1, connString);
            //if (dt.Rows.Count > 0)
            //{
            //    foreach (DataRow dtRow in dt.Rows)
            //    {
            //        paymentInfo.PaymentProfileId =Convert.ToInt64(dtRow["id"].ToString());
            //        _creditCardInfoService.DeleteCreditCardInfo(paymentInfo);
            //    }
            //}


            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var model = new PaymentInformationModel();
            model.Result = string.Empty;
            model.deletePaymentProfileFlag = false;
            model = PreparePaymentInformationModel(model);
            return View(model);
        }

        //public DataTable ConvertXSLXtoDataTable(string strFilePath, string connString)
        //{
        //    OleDbConnection oledbConn = new OleDbConnection(connString);
        //    DataTable dt = new DataTable();
        //    try
        //    {

        //        oledbConn.Open();
        //        using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", oledbConn))
        //        {
        //            OleDbDataAdapter oleda = new OleDbDataAdapter();
        //            oleda.SelectCommand = cmd;
        //            DataSet ds = new DataSet();
        //            oleda.Fill(ds);

        //            dt = ds.Tables[0];
        //        }
        //    }
        //    catch
        //    {
        //    }
        //    finally
        //    {

        //        oledbConn.Close();
        //    }

        //    return dt;

        //}

        [HttpPost]
        [PublicAntiForgery]
        public virtual ActionResult CustomerCreditCards(PaymentInformationModel model)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            try
            {
                var customer = _workContext.CurrentCustomer;
                var response = new PaymentInfoResult();
                var existingPaymentInfo = new CreditCardInfo();

                if (ModelState.IsValid)
                {
                    Address address;

                    if (!model.deletePaymentProfileFlag && model.ExistingPaymentProfileId == 0)
                    {
                        FormCollection form = new FormCollection();
                        form.Add("CardholderName", model.FirstName + " " + model.LastName);
                        form.Add("CardNumber", model.CardNumber);
                        form.Add("CardCode", model.CVVNumber);
                        form.Add("ExpireMonth", model.ExpireMonth.ToString());
                        form.Add("ExpireYear", model.ExpireYear.ToString());

                        var paymentMethod = LoadPaymentMethodBySystemName("Payments.AuthorizeNet");
                        if (paymentMethod == null)
                            throw new NopException("Payment method couldn't be loaded");
                        var paymentControllerType = paymentMethod.GetControllerType();
                        var paymentController = DependencyResolver.Current.GetService(paymentControllerType) as BasePaymentController;
                        if (paymentController == null) throw new Exception("Payment controller cannot be loaded");
                        var warnings = paymentController.ValidatePaymentForm(form);
                        if (warnings.Count > 0)
                        {

                            foreach (var warning in warnings)
                                model.Result += " " + warning;


                            model.Success = false;
                            model = PreparePaymentInformationModel(model);
                            return View(model);
                        }
                    }

                    if (model.ExistingPaymentProfileId == 0)
                        address = new Address();
                    else {
                        existingPaymentInfo = _creditCardInfoService.GetCreditCardInfoById(model.ExistingPaymentProfileId);
                        address = existingPaymentInfo.Address;
                    }

                    address.FirstName = model.FirstName;
                    address.LastName = model.LastName;
                    address.Address1 = model.Address1;
                    address.Address2 = model.Address2;
                    address.City = model.City;
                    address.StateProvinceId = model.StateId;
                    address.StateProvince = _stateProvinceService.GetStateProvinceById(model.StateId);
                    address.CountryId = model.CountryId;
                    address.Country = _countryService.GetCountryById(model.CountryId);
                    address.Email = customer.Email;
                    address.ZipPostalCode = model.ZipCode;
                    address.CreatedOnUtc = DateTime.Now;

                    CreditCardInfo paymentProfile = new CreditCardInfo();
                    if (model.ExistingPaymentProfileId == 0)
                    {
                        paymentProfile.CardNumber = model.CardNumber;
                        paymentProfile.CardType = model.CartTypeId;
                        //paymentProfile.CVVNumber = model.CVVNumber;
                        paymentProfile.CVVNumber ="";
                        paymentProfile.ExpireMonth = model.ExpireMonth;
                        paymentProfile.ExpireYear = model.ExpireYear;
                        paymentProfile.PaymentProfileId = 0;
                    }
                    else
                    {
                        paymentProfile.PaymentProfileId = model.PaymentProfileId;
                        paymentProfile.Id = existingPaymentInfo.Id;
                        address.Id = model.addressId;
                        paymentProfile = existingPaymentInfo;
                        paymentProfile.IsDefault = model.IsDefault;
                    }
                    paymentProfile.Address = address;
                    paymentProfile.CustomerId = customer.Id;
                    paymentProfile.Customer = customer;
                    var creditCardInfoList = _creditCardInfoService.GetCreditCardInfosByCustomerId(_workContext.CurrentCustomer.Id);
                    paymentProfile.CreatedDate = DateTime.UtcNow;
                    if (!model.deletePaymentProfileFlag && (creditCardInfoList == null || creditCardInfoList.Count == 0))
                        paymentProfile.IsDefault = true;
                    else
                        paymentProfile.IsDefault = model.IsDefault;


                    //delete Payment Profile
                    if (model.deletePaymentProfileFlag)
                    {
                        if (creditCardInfoList.Count > 1)
                        {
                            response = _creditCardInfoService.DeleteCreditCardInfo(existingPaymentInfo);
                            //delete Payment Profile which are Set as default
                            if (response.Success)
                            {
                                if (model.IsDefault)
                                {
                                    if (creditCardInfoList != null && creditCardInfoList.Count > 0)
                                    {
                                        //Set as default Payment Profile which are last inserted
                                        var creditcard = creditCardInfoList.Where(x => x.Id != existingPaymentInfo.Id).OrderByDescending(c => c.Id).FirstOrDefault();
                                        if (creditcard != null)
                                        {
                                            creditcard.IsDefault = true;
                                            _creditCardInfoService.UpdateCreditCardInfo(creditcard);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            model.Result = "Minimum one creditcard must required.";
                            model.Success = false;
                            model = PreparePaymentInformationModel(model);
                            return View(model);
                        }
                    }
                    else if (model.ExistingPaymentProfileId > 0)
                        response = _creditCardInfoService.UpdateCreditCardInfo(paymentProfile);
                    else
                        response = _creditCardInfoService.InsertCreditCardInfo(paymentProfile);



                    if (!model.deletePaymentProfileFlag && creditCardInfoList != null && creditCardInfoList.Count > 0)
                    {
                        if (model.IsDefault)
                        {
                            foreach (var creditCard in creditCardInfoList.Where(x => x.Id != paymentProfile.Id).ToList())
                            {
                                creditCard.IsDefault = false;
                                _creditCardInfoService.UpdateCreditCardInfo(creditCard);
                            }
                        }
                    }


                    if (!response.Success)
                    {
                        model.Result = response.Error;
                        model.Success = false;
                    }
                    else {

                        model.Result = "Payment info has been saved successfully.";
                        model.Success = true;
                        if (!model.deletePaymentProfileFlag && model.ExistingPaymentProfileId > 0)
                        {
                            _addressService.UpdateAddress(address);
                            model.Result = "Payment info has been updated successfully.";
                        }
                        else if (model.deletePaymentProfileFlag)
                        {
                            var modelPayment = new PaymentInformationModel();
                            modelPayment.Result = "Payment info has been deleted successfully.";
                            modelPayment.Success = true;
                            modelPayment.deletePaymentProfileFlag = false;
                            modelPayment.ExistingPaymentProfileId = 0;
                            modelPayment = PreparePaymentInformationModel(modelPayment);
                            return View(modelPayment);
                        }

                    }
                }
                model = PreparePaymentInformationModel(model);
                return View(model);

            }
            catch (Exception e)
            {
                model.Success = false;
                model.Result = e.InnerException.Message;
                return View(model);
            }


        }

        public PaymentInformationModel PreparePaymentInformationModel(PaymentInformationModel model)
        {

            //year
            model.ExpireYears.Add(new SelectListItem { Text = "Select Year", Value = "" });
            for (int i = 0; i < 15; i++)
            {
                string year = Convert.ToString(DateTime.Now.Year + i);
                model.ExpireYears.Add(new SelectListItem
                {
                    Text = year,
                    Value = year,
                });
            }

            //months
            model.ExpireMonths.Add(new SelectListItem { Text = "Select Month", Value = "" });
            for (int i = 1; i <= 12; i++)
            {
                string text = (i < 10) ? "0" + i : i.ToString();
                model.ExpireMonths.Add(new SelectListItem
                {
                    Text = text,
                    Value = i.ToString(),
                });
            }

            //Card Types
            model.CardTypes.Add(new SelectListItem { Text = "Select Card Type", Value = "" });
            foreach (EnumCardType val in Enum.GetValues(typeof(EnumCardType)))
            {
                string text = val.ToString();
                model.CardTypes.Add(new SelectListItem
                {
                    Text = (val == EnumCardType.AmericanExpress) ? "American Express" : (val == EnumCardType.MasterCard) ? "Master Card" : text,
                    Value = val.GetHashCode().ToString(),
                    Selected = val.GetHashCode() == model.CartTypeId
                });
            }

            //model.ExistingPaymentProfiles.Add(new SelectListItem { Text = "Add New Card", Value = "0" });
            var existingPaymentProfiles = _creditCardInfoService.GetCreditCardInfosByCustomerId(_workContext.CurrentCustomer.Id);
            if (existingPaymentProfiles.Count() > 0)
            {
                //model.ExistingPaymentProfiles.Add(new SelectListItem { Text = "Modify an Existing Card or Add New Card", Value = "-1" });
                foreach (var cInfo in existingPaymentProfiles)
                    model.ExistingPaymentProfiles.Add(new SelectListItem {

                        Text = cInfo.CardNumber,
                        Value = cInfo.Id.ToString()
                    });
            }

            var _encryptionService = EngineContext.Current.Resolve<IEncryptionService>();
            var _paymentService = EngineContext.Current.Resolve<IPaymentService>();
            var _dateTimeHelper= EngineContext.Current.Resolve<IDateTimeHelper>();



            foreach (var item in existingPaymentProfiles) {
                model.CreditCardInfos.Add(new CreditCardInfoModel() {
                    CardType= Constant.GetEnumDescription((EnumCardType)item.CardType),
                    IsDefault=item.IsDefault,
                    MaskedCreditCardNumber= _encryptionService.DecryptText(item.MaskedCreditCardNumber),
                    CreatedOn= _dateTimeHelper.ConvertToUserTime(item.CreatedDate, DateTimeKind.Utc),
                    ExpireMonth=item.ExpireMonth,
                    ExpireYear=item.ExpireYear,
                    CardHolderName=item.CardHolderName,
                    Id =item.Id
                });
            }


            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "" });

            foreach (var c in _countryService.GetAllCountries(_workContext.WorkingLanguage.Id))
            {
                model.AvailableCountries.Add(new SelectListItem
                {
                    Text = c.GetLocalized(x => x.Name),
                    Value = c.Id.ToString(),
                    Selected = c.Id == model.CountryId
                });
            }

            //states
            var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId, _workContext.WorkingLanguage.Id).ToList();
            if (states.Any())
            {
                model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "" });

                foreach (var s in states)
                {
                    model.AvailableStates.Add(new SelectListItem { Text = s.GetLocalized(x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.StateId) });
                }
            }
            else
            {
                bool anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);
                model.AvailableStates.Add(new SelectListItem
                {
                    Text = _localizationService.GetResource(anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"),
                    Value = ""
                });
            }
            return model;
        }

        [HttpGet]
        public virtual JsonResult GetCreditCardDetail(string selectedPaymentProfileId)
        {
            PaymentInformationModel model = new PaymentInformationModel();
            var creditCardInfo = new CreditCardInfo();
            creditCardInfo = _creditCardInfoService.GetCreditCardInfoById(Convert.ToInt32(selectedPaymentProfileId));

            if (creditCardInfo != null)
            {
                model.Address1 = creditCardInfo.Address.Address1;
                model.Address2 = creditCardInfo.Address.Address2;
                model.FirstName = creditCardInfo.Address.FirstName;
                model.LastName = creditCardInfo.Address.LastName;
                model.ZipCode = creditCardInfo.Address.ZipPostalCode;
                model.StateId = Convert.ToInt32(creditCardInfo.Address.StateProvinceId);
                model.City = creditCardInfo.Address.City;
                model.CountryId = Convert.ToInt32(creditCardInfo.Address.CountryId);
                model.addressId = Convert.ToInt32(creditCardInfo.AddressId);
                model.PaymentProfileId = creditCardInfo.PaymentProfileId;
                model.IsDefault = creditCardInfo.IsDefault;
                model.CustomerProfileId = creditCardInfo.CustomerProfileId;
            }
            return Json(new { creditCarddata = model }, JsonRequestBehavior.AllowGet);
        }

        public PaymentInformationModel GetCreditCardInfo(CreditCardInfo creditCardInfo, PaymentInformationModel model) {
            
                model.Address1 = creditCardInfo.Address.Address1;
                model.Address2 = creditCardInfo.Address.Address2;
                model.FirstName = creditCardInfo.Address.FirstName;
                model.LastName = creditCardInfo.Address.LastName;
                model.ZipCode = creditCardInfo.Address.ZipPostalCode;
                model.StateId = Convert.ToInt32(creditCardInfo.Address.StateProvinceId);
                model.City = creditCardInfo.Address.City;
                model.CountryId = Convert.ToInt32(creditCardInfo.Address.CountryId);
                model.addressId = Convert.ToInt32(creditCardInfo.AddressId);
                model.PaymentProfileId = creditCardInfo.PaymentProfileId;
                model.IsDefault = creditCardInfo.IsDefault;
                model.CustomerProfileId = creditCardInfo.CustomerProfileId;
                model.Id = creditCardInfo.Id;
            return model;
        }

        [HttpPost]
        [PublicAntiForgery]
        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult CreditCardInfoDelete(int id)
        {
            var model = new PaymentInformationModel();
            try {
                if (!_workContext.CurrentCustomer.IsRegistered())
                    return new HttpUnauthorizedResult();

                var creditCardInfo = new CreditCardInfo();

                creditCardInfo = _creditCardInfoService.GetCreditCardInfoById(Convert.ToInt32(id));
                var response = new PaymentInfoResult();

                var creditCardInfoList = _creditCardInfoService.GetCreditCardInfosByCustomerId(_workContext.CurrentCustomer.Id);


                if (creditCardInfo != null)
                {

                    //if (creditCardInfoList.Count > 1)
                    //{
                        response = _creditCardInfoService.DeleteCreditCardInfo(creditCardInfo);
                        //delete Payment Profile which are Set as default
                        if (response.Success)
                        {
                            if (creditCardInfo.IsDefault)
                            {
                                if (creditCardInfoList != null && creditCardInfoList.Count > 0)
                                {
                                    //Set as default Payment Profile which are last inserted
                                    var creditcard = creditCardInfoList.Where(x => x.Id != creditCardInfo.Id).OrderByDescending(c => c.Id).FirstOrDefault();
                                    if (creditcard != null)
                                    {
                                        creditcard.IsDefault = true;
                                        _creditCardInfoService.UpdateCreditCardInfo(creditcard);
                                    }
                                }
                            }
                        }
                        model.Result = _localizationService.GetResource("Account.CreditCard.Delete.success");
                        model.Success = true;
                        SuccessNotification(model.Result, true);
                    //}
                    //else
                    //{
                     //   model.Result = "Minimum one creditcard must required.";
                     //   model.Success = false;
                     //   ErrorNotification(model.Result, true);

                    //}
                   
                    //SuccessNotification(_localizationService.GetResource("Account.CreditCard.Delete.success"), true);
                }
            }
            catch (Exception ex) {
                model.Result = _localizationService.GetResource("Account.CreditCard.Delete.Error");
                model.Success = false;
                ErrorNotification(model.Result, true);
                ErrorNotification(ex.StackTrace, false);
                //ErrorNotification(_localizationService.GetResource("Account.CreditCard.Delete.Error"), true);
            }
            model = PreparePaymentInformationModel(model);
            return Json(new
            {
                redirect = Url.RouteUrl("PaymentInfo"),
                model=model
            });
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult AddCreditCard()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var model = new PaymentInformationModel();
            model = PreparePaymentInformationModel(model);

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        [ValidateInput(false)]
        public virtual ActionResult AddCreditCard(PaymentInformationModel model, FormCollection form,string IsDefault)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;
            var response = new PaymentInfoResult();
            try {
                FormCollection form1 = new FormCollection();
                form1.Add("CardholderName", Convert.ToString(form["PaymentInformation.FirstName"]) + " " + Convert.ToString(form["PaymentInformation.LastName"]));
                form1.Add("CardNumber", Convert.ToString(form["PaymentInformation.CardNumber"]));
                form1.Add("CardCode", Convert.ToString(form["PaymentInformation.CVVNumber"]));
                form1.Add("ExpireMonth", Convert.ToString(form["PaymentInformation.ExpireMonth"].ToString()));
                form1.Add("ExpireYear", Convert.ToString(form["PaymentInformation.ExpireYear"].ToString()));

                var paymentMethod = LoadPaymentMethodBySystemName("Payments.AuthorizeNet");
                if (paymentMethod == null)
                    throw new NopException("Payment method couldn't be loaded");
                var paymentControllerType = paymentMethod.GetControllerType();
                var paymentController = DependencyResolver.Current.GetService(paymentControllerType) as BasePaymentController;
                if (paymentController == null) throw new Exception("Payment controller cannot be loaded");
                var warnings = paymentController.ValidatePaymentForm(form1);
                if (warnings.Count > 0)
                {

                    foreach (var warning in warnings)
                        model.warnings.Add(warning);


                    model.Success = false;
                    model = PreparePaymentInformationModel(model);
                    return View(model);
                }
                //if (_workContext.CurrentCustomer.BillingAddress == null) {
                //    model.Result = _localizationService.GetResource("Account.CreditCard.Add.Address.Required");
                //    model.Success = false;
                //    model = PreparePaymentInformationModel(model);
                //    return View(model);
                //}
                Address address = new Address();
                //address.FirstName = Convert.ToString(form["PaymentInformation.FirstName"]);
                //address.LastName = Convert.ToString(form["PaymentInformation.LastName"]);
                //address.Address1 = Convert.ToString(form["PaymentInformation.Address1"]);
                //address.Address2 = Convert.ToString(form["PaymentInformation.Address2"]);
                //address.City = Convert.ToString(form["PaymentInformation.City"]);
                //address.StateProvinceId = Convert.ToInt32(Convert.ToString(form["PaymentInformation.StateId"]));
                //address.StateProvince = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(Convert.ToString(form["PaymentInformation.StateId"])));
                //address.CountryId = Convert.ToInt32(Convert.ToString(form["PaymentInformation.CountryId"]));
                //address.Country = _countryService.GetCountryById(Convert.ToInt32(Convert.ToString(form["PaymentInformation.CountryId"])));
                //address.Email = customer.Email;
                //address.ZipPostalCode = Convert.ToString(form["PaymentInformation.ZipCode"]);
                //address.CreatedOnUtc = DateTime.Now;

                address = _workContext.CurrentCustomer.BillingAddress;
                if (address == null) {
                    address = new Address();
                }

                address.FirstName = Convert.ToString(form["PaymentInformation.FirstName"]);
                address.LastName = Convert.ToString(form["PaymentInformation.LastName"]);
                address.CreatedOnUtc= DateTime.UtcNow;

                CreditCardInfo paymentProfile = new CreditCardInfo();
                paymentProfile.CardNumber = Convert.ToString(form["PaymentInformation.CardNumber"]);
                paymentProfile.CardType = Convert.ToInt32(Convert.ToString(form["PaymentInformation.CartTypeId"]));
                paymentProfile.CVVNumber = Convert.ToString(form["PaymentInformation.CVVNumber"]);
                paymentProfile.ExpireMonth = Convert.ToInt32(Convert.ToString(form["PaymentInformation.ExpireMonth"]));
                paymentProfile.ExpireYear = Convert.ToInt32(Convert.ToString(form["PaymentInformation.ExpireYear"]));
                paymentProfile.PaymentProfileId = 0;
                paymentProfile.Address =address;
                paymentProfile.CustomerId = customer.Id;
                paymentProfile.Customer = customer;
                var creditCardInfoList = _creditCardInfoService.GetCreditCardInfosByCustomerId(_workContext.CurrentCustomer.Id);
                paymentProfile.CreatedDate = DateTime.UtcNow;
                if ((creditCardInfoList == null || creditCardInfoList.Count == 0))
                    paymentProfile.IsDefault = true;
                else
                    paymentProfile.IsDefault = Convert.ToBoolean(IsDefault);

                response = _creditCardInfoService.InsertCreditCardInfo(paymentProfile);
                if (response.Success)
                {
                    if (creditCardInfoList != null && creditCardInfoList.Count > 0)
                    {
                        if (Convert.ToBoolean(IsDefault))
                        {
                            foreach (var creditCard in creditCardInfoList.Where(x => x.Id != paymentProfile.Id).ToList())
                            {
                                creditCard.IsDefault = false;
                                _creditCardInfoService.UpdateCreditCardInfo(creditCard);
                            }
                        }
                    }
                }


                if (!response.Success)
                {
                    model.Result = response.Error;
                    model.Success = false;
                   
                }
                else {

                    model.Result = _localizationService.GetResource("Account.CreditCard.Add.Success");
                    model.Success = true;
                    SuccessNotification(model.Result, true);
                    return RedirectToRoute("PaymentInfo");
                }
                
                
            }
            catch (Exception ex) {
                model.Result = _localizationService.GetResource("Account.CreditCard.Add.Error");
                model.Success = false;
                //ErrorNotification(ex.Message, false);
            }
            model = PreparePaymentInformationModel(model);
            return View(model);
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult EditCreditCard(int id)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var creditCardInfo = new CreditCardInfo();
            creditCardInfo = _creditCardInfoService.GetCreditCardInfoById(id);

            if (creditCardInfo == null)
                //address is not found
                return RedirectToRoute("PaymentInfo");

            var model = new PaymentInformationModel();
            model = GetCreditCardInfo(creditCardInfo, model);
            model = PreparePaymentInformationModel(model);
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        [ValidateInput(false)]
        public virtual ActionResult EditCreditCard(PaymentInformationModel model, int id, FormCollection form,string IsDefault)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var creditCardInfo = new CreditCardInfo();
            creditCardInfo = _creditCardInfoService.GetCreditCardInfoById(id);
            if (creditCardInfo == null)
                //address is not found
                return RedirectToRoute("PaymentInfo");
            Address address = new Address();
            var response = new PaymentInfoResult();
            try {

                address = creditCardInfo.Address;
                address.FirstName = Convert.ToString(form["PaymentInformation.FirstName"]);
                address.LastName = Convert.ToString(form["PaymentInformation.LastName"]);
                address.Address1 = Convert.ToString(form["PaymentInformation.Address1"]);
                address.Address2 = Convert.ToString(form["PaymentInformation.Address2"]);
                address.City = Convert.ToString(form["PaymentInformation.City"]);
                address.StateProvinceId = Convert.ToInt32(Convert.ToString(form["PaymentInformation.StateId"]));
                address.StateProvince = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(Convert.ToString(form["PaymentInformation.StateId"])));
                address.CountryId = Convert.ToInt32(Convert.ToString(form["PaymentInformation.CountryId"]));
                address.Country = _countryService.GetCountryById(Convert.ToInt32(Convert.ToString(form["PaymentInformation.CountryId"])));
                address.Email = _workContext.CurrentCustomer.Email;
                address.ZipPostalCode = Convert.ToString(form["PaymentInformation.ZipCode"]);
                



                CreditCardInfo paymentProfile = new CreditCardInfo();
                paymentProfile = creditCardInfo;
                paymentProfile.IsDefault = Convert.ToBoolean(IsDefault);

                paymentProfile.Address = address;
                paymentProfile.CustomerId = _workContext.CurrentCustomer.Id;
                paymentProfile.Customer = _workContext.CurrentCustomer;
                var creditCardInfoList = _creditCardInfoService.GetCreditCardInfosByCustomerId(_workContext.CurrentCustomer.Id);
                paymentProfile.CreatedDate = DateTime.UtcNow;
                if (!model.deletePaymentProfileFlag && (creditCardInfoList == null || creditCardInfoList.Count == 0))
                    paymentProfile.IsDefault = true;
                else
                    paymentProfile.IsDefault = Convert.ToBoolean(IsDefault);
                paymentProfile.UpdatedDate = DateTime.UtcNow;
                response = _creditCardInfoService.UpdateCreditCardInfo(paymentProfile);
                if (creditCardInfoList != null && creditCardInfoList.Count > 0)
                {
                    if (Convert.ToBoolean(IsDefault))
                    {
                        foreach (var creditCard in creditCardInfoList.Where(x => x.Id != paymentProfile.Id).ToList())
                        {
                            creditCard.IsDefault = false;
                            _creditCardInfoService.UpdateCreditCardInfo(creditCard);
                        }
                    }
                }


                if (!response.Success)
                {
                    model.Result = response.Error;
                    model.Success = false;
                }
                else {
                    _addressService.UpdateAddress(address);
                     model.Result = _localizationService.GetResource("Account.CreditCard.Edit.Success");
                    SuccessNotification(model.Result, true);
                    return RedirectToRoute("PaymentInfo");
                }
            }
            catch (Exception ex){
                ErrorNotification(ex.StackTrace, false);
            }
            model = new PaymentInformationModel();
            model.Result = _localizationService.GetResource("Account.CreditCard.Edit.Error");
            model.Success = false;
            model = GetCreditCardInfo(creditCardInfo, model);
            model = PreparePaymentInformationModel(model);
            return View(model);
            
                 
        }
        #endregion
    }
}
