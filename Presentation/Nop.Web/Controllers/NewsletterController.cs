using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Extensions;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Security.Captcha;

namespace Nop.Web.Controllers
{
    public partial class NewsletterController : BasePublicController
    {
        private readonly INewsletterModelFactory _newsletterModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IStoreContext _storeContext;

        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;

        public NewsletterController(INewsletterModelFactory newsletterModelFactory,
            ILocalizationService localizationService,
            IWorkContext workContext,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IWorkflowMessageService workflowMessageService,
            IStoreContext storeContext,
            CustomerSettings customerSettings,
            CaptchaSettings captchaSettings)
        {
            this._newsletterModelFactory = newsletterModelFactory;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._workflowMessageService = workflowMessageService;
            this._storeContext = storeContext;
            this._customerSettings = customerSettings;
            this._captchaSettings = captchaSettings;
        }

        [ChildActionOnly]
        public virtual ActionResult NewsletterBox()
        {
            if (_customerSettings.HideNewsletterBlock)
                return Content("");

            var model = _newsletterModelFactory.PrepareNewsletterBoxModel();
            return PartialView(model);
        }

        //available even when a store is closed
        [StoreClosed(true)]
        [HttpPost]
        [ValidateInput(false)]
        public virtual ActionResult SubscribeNewsletter(string email, bool subscribe, string token)
        {
            string result;
            bool success = false;

            var recaptchaResult = Task.Run(() => IsCaptchaValid(token)).Result;

            if (!recaptchaResult)
            {
                result = _localizationService.GetResource("Common.WrongCaptchaV2");
            }
            if (ValidationRulesForNewCustomer.ValidationRulesForNewCustomerByEmail(email))
            {
                result = _localizationService.GetResource("Newsletter.Email.Wrong");
            }
            else if (!CommonHelper.IsValidEmail(email))
            {
                result = _localizationService.GetResource("Newsletter.Email.Wrong");
            }
            else
            {
                email = email.Trim();

                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(email, _storeContext.CurrentStore.Id);
                if (subscription != null)
                {
                    if (subscribe)
                    {
                        if (!subscription.Active)
                        {
                            _workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);
                        }
                        result = _localizationService.GetResource("Newsletter.SubscribeEmailSent");
                    }
                    else
                    {
                        if (subscription.Active)
                        {
                            _workflowMessageService.SendNewsLetterSubscriptionDeactivationMessage(subscription, _workContext.WorkingLanguage.Id);
                        }
                        result = _localizationService.GetResource("Newsletter.UnsubscribeEmailSent");
                    }
                }
                else if (subscribe)
                {
                    subscription = new NewsLetterSubscription
                    {
                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                        Email = email,
                        Active = false,
                        StoreId = _storeContext.CurrentStore.Id,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
                    _workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);

                    result = _localizationService.GetResource("Newsletter.SubscribeEmailSent");
                }
                else
                {
                    result = _localizationService.GetResource("Newsletter.UnsubscribeEmailSent");
                }
                success = true;
            }

            return Json(new
            {
                Success = success,
                Result = result,
            });
        }

        //available even when a store is closed
        [StoreClosed(true)]
        public virtual ActionResult SubscriptionActivation(Guid token, bool active)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByGuid(token);
            if (subscription == null)
                return RedirectToRoute("HomePage");

            if (active)
            {
                subscription.Active = true;
                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
            }
            else
                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);

            var model = _newsletterModelFactory.PrepareSubscriptionActivationModel(active);
            return View(model);
        }

        public class CaptchaResponseViewModel
        {
            public bool Success { get; set; }

            [JsonProperty(PropertyName = "error-codes")]
            public IEnumerable<string> ErrorCodes { get; set; }

            [JsonProperty(PropertyName = "challenge_ts")]
            public DateTime ChallengeTime { get; set; }

            public string HostName { get; set; }
            public double Score { get; set; }
            public string Action { get; set; }
        }

        private async Task<bool> IsCaptchaValid(string response)
        {
            try
            {
                var secret = _captchaSettings.ReCaptchaPrivateKey;

                using (var client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                    {
                        {"secret", secret},
                        {"response", response},
                        {"remoteip", Request.UserHostAddress}
                    };

                    var content = new FormUrlEncodedContent(values);
                    var verify = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                    var captchaResponseJson = await verify.Content.ReadAsStringAsync();
                    var captchaResult = JsonConvert.DeserializeObject<CaptchaResponseViewModel>(captchaResponseJson);

                    return captchaResult.Success;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
