using System;
using System.Text;
using System.Web.Mvc;
using Nop.Plugin.Shipping.USPS.Domain;
using Nop.Plugin.Shipping.USPS.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Controllers;
using Nop.Core.Infrastructure;
using Nop.Services.Shipping;
using System.Collections.Generic;
using Nop.Core.Domain.Shipping;
using System.Linq;

namespace Nop.Plugin.Shipping.USPS.Controllers
{
    [AdminAuthorize]
    public class ShippingUSPSController : BasePluginController
    {
        private readonly USPSSettings _uspsSettings;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public ShippingUSPSController(USPSSettings uspsSettings,
            ISettingService settingService,
            ILocalizationService localizationService)
        {
            this._uspsSettings = uspsSettings;
            this._settingService = settingService;
            this._localizationService = localizationService;
        }

        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new USPSShippingModel();
            model.Url = _uspsSettings.Url;
            model.Username = _uspsSettings.Username;
            model.Password = _uspsSettings.Password;
            model.AdditionalHandlingCharge = _uspsSettings.AdditionalHandlingCharge;
            model.IsLiveRates = _uspsSettings.IsLiveRates;
            model.isusafreeshipping = _uspsSettings.isusafreeshipping;

            var _shippingSerive = EngineContext.Current.Resolve<IShippingService>();
            // Load Domestic service names
            string carrierServicesOfferedDomestic = _uspsSettings.CarrierServicesOfferedDomestic;
            foreach (string service in USPSServices.DomesticServices)
                model.AvailableCarrierServicesDomestic.Add(service);

            if (!String.IsNullOrEmpty(carrierServicesOfferedDomestic))
                foreach (string service in USPSServices.DomesticServices)
                {
                    string serviceId = USPSServices.GetServiceIdDomestic(service);
                    if (!String.IsNullOrEmpty(serviceId))
                    {
                        // Add delimiters [] so that single digit IDs aren't found in multi-digit IDs
                        if (carrierServicesOfferedDomestic.Contains(String.Format("[{0}]", serviceId)))
                            model.CarrierServicesOfferedDomestic.Add(service);
                    }
                }
            var domesticRates = _shippingSerive.GetShippingRates(true, "USPS").ToList();
            foreach (string service in USPSServices.DomesticServices)
            {
                string serviceId = USPSServices.GetServiceIdDomestic(service);
                if (!String.IsNullOrEmpty(serviceId))
                {
                    model.domesticShippingRate.Add(domesticRates.Where(x=>x.CarrierId==serviceId).FirstOrDefault()==null?"": domesticRates.Where(x => x.CarrierId == serviceId).FirstOrDefault().Rate.ToString());
                }
                else {
                    model.domesticShippingRate.Add("");
                }
            }
                // Load Internation service names
                string carrierServicesOfferedInternational = _uspsSettings.CarrierServicesOfferedInternational;
            foreach (string service in USPSServices.InternationalServices)
                model.AvailableCarrierServicesInternational.Add(service);

            if (!String.IsNullOrEmpty(carrierServicesOfferedInternational))
                foreach (string service in USPSServices.InternationalServices)
                {
                    string serviceId = USPSServices.GetServiceIdInternational(service);
                    if (!String.IsNullOrEmpty(serviceId))
                    {
                        // Add delimiters [] so that single digit IDs aren't found in multi-digit IDs
                        if (carrierServicesOfferedInternational.Contains(String.Format("[{0}]", serviceId)))
                            model.CarrierServicesOfferedInternational.Add(service);
                    }
                }
            var internationalRates = _shippingSerive.GetShippingRates(false, "USPS").ToList();
            foreach (string service in USPSServices.InternationalServices)
            {
                string serviceId = USPSServices.GetServiceIdInternational(service);
                if (!String.IsNullOrEmpty(serviceId))
                {
                    model.internationalShippingRate.Add(internationalRates.Where(x => x.CarrierId == serviceId).FirstOrDefault() == null ? "" : internationalRates.Where(x => x.CarrierId == serviceId).FirstOrDefault().Rate.ToString());
                }
                else {
                    model.internationalShippingRate.Add("");
                }
            }
            return View("~/Plugins/Shipping.USPS/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [ChildActionOnly]
        public ActionResult Configure(USPSShippingModel model,FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }
            
            //save settings
            _uspsSettings.Url = model.Url;  
            _uspsSettings.Username = model.Username;
            _uspsSettings.Password = model.Password;
            _uspsSettings.AdditionalHandlingCharge = model.AdditionalHandlingCharge;
            _uspsSettings.IsLiveRates = model.IsLiveRates;
            _uspsSettings.isusafreeshipping = model.isusafreeshipping;
            var _shippingService = EngineContext.Current.Resolve<IShippingService>();
            Dictionary<string, decimal> dictDomestic = new Dictionary<string, decimal>();
            Dictionary<string, string> dictDomesticCarrierName = new Dictionary<string, string>();
            // Save selected Domestic services
            var carrierServicesOfferedDomestic = new StringBuilder();
            int carrierServicesDomesticSelectedCount = 0;
            if (model.CheckedCarrierServicesDomestic != null)
            {
                foreach (var cs in model.CheckedCarrierServicesDomestic)
                {
                    carrierServicesDomesticSelectedCount++;

                    string serviceId = USPSServices.GetServiceIdDomestic(cs);
                    //unselect any other services if NONE is selected
                    if (!String.IsNullOrEmpty(serviceId) && serviceId.Equals("NONE"))
                    {
                        carrierServicesOfferedDomestic.Clear();
                        carrierServicesOfferedDomestic.AppendFormat("[{0}]:", serviceId);
                        break;
                    }

                    if (!String.IsNullOrEmpty(serviceId))
                    {
                        // Add delimiters [] so that single digit IDs aren't found in multi-digit IDs
                        carrierServicesOfferedDomestic.AppendFormat("[{0}]:", serviceId);
                    }
                    decimal rates = 0;
                    decimal.TryParse(Convert.ToString(form["txtdome_" + cs]),out rates);
                    dictDomestic.Add(serviceId, rates);
                    dictDomesticCarrierName.Add(serviceId, cs);
                }
                _shippingService.DeleteAllShippingRates(true, "USPS");
                _shippingService.InsertAllShippingRates(dictDomestic, true, dictDomesticCarrierName, "USPS");
            }
            // Add default options if no services were selected
            if (carrierServicesDomesticSelectedCount == 0)
                _uspsSettings.CarrierServicesOfferedDomestic = "[1]:[3]:[4]:";
            else
                _uspsSettings.CarrierServicesOfferedDomestic = carrierServicesOfferedDomestic.ToString();



            // Save selected International services
            var carrierServicesOfferedInternational = new StringBuilder();
            int carrierServicesInternationalSelectedCount = 0;
            Dictionary<string, decimal> dictInternational = new Dictionary<string, decimal>();
            Dictionary<string, string> dictInternationalCarrierName = new Dictionary<string, string>();
            if (model.CheckedCarrierServicesInternational != null)
            {
                foreach (var cs in model.CheckedCarrierServicesInternational)
                {
                    carrierServicesInternationalSelectedCount++;
                    string serviceId = USPSServices.GetServiceIdInternational(cs);
                    // unselect other services if NONE is selected
                    if (!String.IsNullOrEmpty(serviceId) && serviceId.Equals("NONE"))
                    {
                        carrierServicesOfferedInternational.Clear();
                        carrierServicesOfferedInternational.AppendFormat("[{0}]:", serviceId);
                        break;
                    }
                    if (!String.IsNullOrEmpty(serviceId))
                    {
                        // Add delimiters [] so that single digit IDs aren't found in multi-digit IDs
                        carrierServicesOfferedInternational.AppendFormat("[{0}]:", serviceId);
                    }
                    decimal rates = 0;
                    decimal.TryParse(Convert.ToString(form["txtinter_" + cs]), out rates);
                    dictInternational.Add(serviceId, rates);
                    dictInternationalCarrierName.Add(serviceId, cs);
                }
                _shippingService.DeleteAllShippingRates(false, "USPS");
                _shippingService.InsertAllShippingRates(dictInternational, false, dictInternationalCarrierName, "USPS");
            }
            
            // Add default options if no services were selected
            if (carrierServicesInternationalSelectedCount == 0)
                _uspsSettings.CarrierServicesOfferedInternational = "[2]:[15]:[1]:";
            else
                _uspsSettings.CarrierServicesOfferedInternational = carrierServicesOfferedInternational.ToString();
            

            _settingService.SaveSetting(_uspsSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

    }
}
