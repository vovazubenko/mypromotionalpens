using FluentValidation;
using Nop.Services.Localization;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Models;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Validators
{
    public class SpeedFiltersSettingsValidator : AbstractValidator<SpeedFiltersSettingsModel>
    {
        public SpeedFiltersSettingsValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.WidgetZone).NotEmpty().WithMessage(localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.WidgetZone.Required"));

            RuleFor(x => x.SelectorForListPanel).NotEmpty().WithMessage(localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.HTMLSelector.Required"));
            RuleFor(x => x.SelectorForGridPanel).NotEmpty().WithMessage(localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.HTMLSelector.Required"));
            RuleFor(x => x.SelectorForPager).NotEmpty().WithMessage(localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.HTMLSelector.Required"));
            RuleFor(x => x.SelectorForSortOptions).NotEmpty().WithMessage(localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.HTMLSelector.Required"));
            RuleFor(x => x.SelectorForViewOptions).NotEmpty().WithMessage(localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.HTMLSelector.Required"));
            RuleFor(x => x.SelectorForProductPageSize).NotEmpty().WithMessage(localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.HTMLSelector.Required"));
           // RuleFor(x => x.SelectorForScrolling).NotEmpty().WithMessage(localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.HTMLSelector.Required"));
        }
    }
}