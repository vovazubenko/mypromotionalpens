using FluentValidation;
using Nop.Admin.Models.Settings;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Admin.Models.Security;

namespace Nop.Admin.Validators.Settings
{
    public partial class SettingValidator : BaseNopValidator<SettingModel>
    {
        public SettingValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Settings.AllSettings.Fields.Name.Required"));
        }
    }

    public partial class RestrictDomainValidator : BaseNopValidator<DomainRestrictionModel>
    {
        public RestrictDomainValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.DomainURL).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Settings.AllSettings.Fields.Name.Required"));
            
            
        }
    }
}