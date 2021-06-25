using FluentValidation;
using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Customer;

namespace Nop.Web.Validators.Customer
{
    public partial class PaymentInformationValidator : BaseNopValidator<PaymentInformationModel>
    {
        public PaymentInformationValidator(ILocalizationService localizationService, CustomerSettings customerSettings)
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.FirstName.Required"));
            RuleFor(x => x.LastName).NotEmpty().WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.LastName.Required"));
            RuleFor(x => x.CartTypeId).NotEqual(0).WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.CartTypeId.Required"));

            RuleFor(x => x.CardNumber).NotEmpty().WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.CardNumber.Required"));
            RuleFor(x => x.CardNumber).IsCreditCard().WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.CardNumber.Wrong"));


            RuleFor(x => x.ExpireMonth).NotEqual(0).WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.ExpireMonth.Required"));
            RuleFor(x => x.ExpireYear).NotEqual(0).WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.ExpireYear.Required"));

            RuleFor(x => x.CVVNumber).NotEmpty().WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.CVVNumber.Required"));
            RuleFor(x => x.CVVNumber).Matches(@"^[0-9]{3,4}$").WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.CVVNumber.Wrong"));

            RuleFor(x => x.Address1).NotEmpty().WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.Address1.Required"));
            
            RuleFor(x => x.City).NotEmpty().WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.City.Required"));
            RuleFor(x => x.StateId).NotEmpty().WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.StateId.Required"));
            RuleFor(x => x.CountryId).NotEmpty().WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.CountryId.Required"));
            RuleFor(x => x.ZipCode).NotEmpty().WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.ZipCode.Required"));
            RuleFor(x => x.CountryId).NotEqual(0).WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.Country.Required"));
            RuleFor(x => x.StateId).NotEqual(0).WithMessage(localizationService.GetResource("Account.PaymentInformation.Fields.State.Required"));
            

        }
    }
}