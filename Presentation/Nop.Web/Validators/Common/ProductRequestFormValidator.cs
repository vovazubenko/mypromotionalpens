using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Catalog;
using FluentValidation;



namespace Nop.Web.Validators.Common
{
    public class ProductRequestFormValidator : BaseNopValidator<ProductFormModel>
    {
        public ProductRequestFormValidator(ILocalizationService localizationService)
        {


            RuleFor(x => x.FullName).NotEmpty().WithMessage(localizationService.GetResource("Product.RequestForm.Fields.FullName.Required"));
            RuleFor(x => x.Address1).NotEmpty().WithMessage(localizationService.GetResource("Product.RequestForm.Fields.Address1.Required"));
            RuleFor(x => x.City).NotEmpty().WithMessage(localizationService.GetResource("Product.RequestForm.Fields.City.Required"));
            RuleFor(x => x.State).NotEmpty().WithMessage(localizationService.GetResource("Product.RequestForm.Fields.State.Required"));
            RuleFor(x => x.Zip).NotEmpty().WithMessage(localizationService.GetResource("Product.RequestForm.Fields.Zip.Required"));
            RuleFor(x => x.EmailAddress).NotEmpty().WithMessage(localizationService.GetResource("Product.RequestForm.Fields.EmailAddress.Required"));
            RuleFor(x => x.EmailAddress).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage(localizationService.GetResource("Product.RequestForm.Fields.PhoneNumber.Required"));
            RuleFor(x => x.ProductCode).NotEmpty().WithMessage(localizationService.GetResource("Product.RequestForm.Fields.ProductCode.Required"));
            RuleFor(x => x.ProductColor).NotEmpty().WithMessage(localizationService.GetResource("Product.RequestForm.Fields.ProductColor.Required"));
            RuleFor(x => x.ProductImprintColor).NotEmpty().WithMessage(localizationService.GetResource("Product.RequestForm.Fields.ProductImprintColor.Required"));
            RuleFor(x => x.ProductQty).NotEmpty().WithMessage(localizationService.GetResource("Product.RequestForm.Fields.ProductQty.Required"));
            
        }
    }
}