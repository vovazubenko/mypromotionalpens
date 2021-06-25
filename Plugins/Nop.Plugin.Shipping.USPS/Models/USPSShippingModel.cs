using System.Collections.Generic;
using Nop.Web.Framework;
using Nop.Core.Domain.Shipping;

namespace Nop.Plugin.Shipping.USPS.Models
{
    public class USPSShippingModel
    {
        public USPSShippingModel()
        {
            CarrierServicesOfferedDomestic = new List<string>();
            AvailableCarrierServicesDomestic = new List<string>();
            CarrierServicesOfferedInternational = new List<string>();
            AvailableCarrierServicesInternational = new List<string>();
            domesticShippingRate = new List<string>();
            internationalShippingRate = new List<string>();
        }

        [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.Url")]
        public string Url { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.Username")]
        public string Username { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.Password")]
        public string Password { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.AdditionalHandlingCharge")]
        public decimal AdditionalHandlingCharge { get; set; }
        
        public IList<string> CarrierServicesOfferedDomestic { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.AvailableCarrierServicesDomestic")]
        public IList<string> AvailableCarrierServicesDomestic { get; set; }
        public string[] CheckedCarrierServicesDomestic { get; set; }

        public IList<string> CarrierServicesOfferedInternational { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.AvailableCarrierServicesInternational")]
        public IList<string> AvailableCarrierServicesInternational { get; set; }
        public string[] CheckedCarrierServicesInternational { get; set; }

        public List<string> domesticShippingRate { get; set; }
        public List<string> internationalShippingRate { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.IsLiveRates")]
        public bool IsLiveRates { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.IsUsaFreeShipping")]
        public bool isusafreeshipping { get; set; }
    }
}