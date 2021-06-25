using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Customer;
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace Nop.Web.Models.Customer
{
    [Validator(typeof(PaymentInformationValidator))]
    public partial class PaymentInformationModel : BaseNopEntityModel
    {
        public PaymentInformationModel()
        {
            this.ExistingPaymentProfiles = new List<SelectListItem>();
            this.ExpireMonths = new List<SelectListItem>();
            this.ExpireYears = new List<SelectListItem>();
            this.CardTypes = new List<SelectListItem>();
            this.AvailableStates = new List<SelectListItem>();
            this.AvailableCountries = new List<SelectListItem>();
            this.warnings = new List<string>();
            this.CreditCardInfos = new List<CreditCardInfoModel>();
        }

        [NopResourceDisplayName("Account.PaymentInformation.Fields.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Account.PaymentInformation.Fields.LastName")]
        public string LastName { get; set; }

        [NopResourceDisplayName("Account.PaymentInformation.Fields.Address1")]
        public string Address1 { get; set; }

        [NopResourceDisplayName("Account.PaymentInformation.Fields.Address2")]
        public string Address2 { get; set; }

        [NopResourceDisplayName("Account.PaymentInformation.Fields.City")]
        public string City { get; set; }

        [NopResourceDisplayName("Account.PaymentInformation.Fields.StateId")]
        public int StateId { get; set; }

        [NopResourceDisplayName("Account.PaymentInformation.Fields.CountryId")]
        public int CountryId { get; set; }

        [NopResourceDisplayName("Account.PaymentInformation.Fields.ZipCode")]
        public string ZipCode { get; set; }

        [NopResourceDisplayName("Account.PaymentInformation.Fields.CardNumber")]
        public string CardNumber { get; set; }

        [NopResourceDisplayName("Account.PaymentInformation.Fields.CardTypeId")]
        public int CartTypeId { get; set; }

        [NopResourceDisplayName("Account.PaymentInformation.Fields.ExpireMonth")]
        public int ExpireMonth { get; set; }

        [NopResourceDisplayName("Account.PaymentInformation.Fields.ExpireYear")]
        public int ExpireYear { get; set; }
        [NopResourceDisplayName("Account.PaymentInformation.Fields.CVV")]
        public string CVVNumber { get; set; }
        [NopResourceDisplayName("Account.PaymentInformation.Fields.IsDefault")]
        public bool IsDefault { get; set; }

        [NopResourceDisplayName("Account.PaymentInformation.Fields.ExistingPaymentProfileId")]
        public int ExistingPaymentProfileId { get; set; }

        public long PaymentProfileId { get; set; }
        public string CustomerProfileId { get; set; }

        public List<SelectListItem> ExistingPaymentProfiles { get; set; }
        public IList<SelectListItem> ExpireMonths { get; set; }
        public IList<SelectListItem> ExpireYears { get; set; }
        public IList<SelectListItem> CardTypes { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        public string Result { get; set; }

        public bool Success { get; set; }

        public int addressId { get; set; }

        public bool deletePaymentProfileFlag { get; set; }
        public IList<string> warnings { get; set; }

        public IList<CreditCardInfoModel> CreditCardInfos { get; set; }


    }
    public enum EnumCardType
    {
        [Description("American Express")]
        AmericanExpress = 1,
        [Description("Discover")]
        Discover = 2,
        [Description("Master Card")]
        MasterCard = 3,
        [Description("Visa")]
        Visa = 4,
        [Description("JCB")]
        JCB = 5,
    }

    public class CreditCardInfoModel:BaseNopEntityModel {
        public long PaymentProfileId { get; set; }
        [NopResourceDisplayName("Account.PaymentInformation.Fields.CardTypeId")]
        public string CardType { get; set; }
        [NopResourceDisplayName("Account.PaymentInformation.Fields.IsDefault")]
        public bool IsDefault { get; set; }
       
        public int ExpireMonth { get; set; }
        public int ExpireYear { get; set; }
        [NopResourceDisplayName("Account.PaymentInformation.Fields.CardNumber")]
        public string MaskedCreditCardNumber { get; set; }
        [NopResourceDisplayName("Account.PaymentInformation.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
        [NopResourceDisplayName("Account.PaymentInformation.Fields.CardHolderName")]
        public string CardHolderName { get; set; }
    }

}