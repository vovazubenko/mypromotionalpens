using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Validators;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Models
{
    [Validator(typeof(SpeedFiltersSettingsValidator))]
    public class SpeedFiltersSettingsModel : BaseNopModel
    {
        public SpeedFiltersSettingsModel()
        {
            AvailableWidgets = new List<SelectListItem>();
            AvailableFiltersTypes = new List<SelectListItem>();
            DefaultProductSortingValues = new List<SelectListItem>();

            AvailableFiltersConditionSpecifications = new List<SelectListItem>();
            AvailableFiltersConditionAttributes = new List<SelectListItem>();
            AvailableFiltersConditionBetweenBlocks = new List<SelectListItem>();
            AvailableCategories = new List<SelectListItem>();
        }

        public IList<SelectListItem> AvailableCategories { get; set; }
        public int ActiveStoreScopeConfiguration { get; set; }
        public int SpecificCategorySettingId { get; set; }
        /// <summary>
        /// Gets or sets a Enabled for plugin
        /// </summary>
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.EnableSpeedFilters")]
        public bool EnableSpeedFilters { get; set; }
        public bool EnableSpeedFilters_OverrideForStore { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.IgnoreDiscountsForCatalog")]
        public bool IgnoreDiscountsForCatalog { get; set; }
        public bool IgnoreDiscountsForCatalog_OverrideForStore { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.WidgetZone")]
        public string WidgetZone { get; set; }
        public bool WidgetZone_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableWidgets { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.FiltersType")]
        public string FiltersType { get; set; }
        public bool FiltersType_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableFiltersTypes { get; set; }


        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.EnableFiltersForCategory")]
        public bool EnableFiltersForCategory { get; set; }
        public bool EnableFiltersForCategory_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SkipFiltersforCategories")]
        public string SkipFiltersforCategories { get; set; }
        public bool SkipFiltersforCategory_OverrideForStore { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.EnableFiltersForManufacturer")]
        public bool EnableFiltersForManufacturer { get; set; }
        public bool EnableFiltersForManufacturer_OverrideForStore { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SkipFiltersforManufacturers")]
        public string SkipFiltersforManufacturers { get; set; }
        public bool SkipFiltersforManufacturer_OverrideForStore { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.EnableFiltersForVendor")]
        public bool EnableFiltersForVendor { get; set; }
        public bool EnableFiltersForVendor_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SkipFiltersforVendors")]
        public string SkipFiltersforVendors { get; set; }
        public bool SkipFiltersforVendors_OverrideForStore { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.EnableFiltersForSearchPage")]
        public bool EnableFiltersForSearchPage { get; set; }
        public bool EnableFiltersForSearchPage_OverrideForStore { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.EnablePriceRangeFilter")]
        public bool EnablePriceRangeFilter { get; set; }
        public bool EnablePriceRangeFilter_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.EnableSpecificationsFilter")]
        public bool EnableSpecificationsFilter { get; set; }
        public bool EnableSpecificationsFilter_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.EnableAttributesFilter")]
        public bool EnableAttributesFilter { get; set; }
        public bool EnableAttributesFilter_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.EnableManufacturersFilter")]
        public bool EnableManufacturersFilter { get; set; }
        public bool EnableManufacturersFilter_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.EnableVendorsFilter")]
        public bool EnableVendorsFilter { get; set; }
        public bool EnableVendorsFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.DefaultClosePriceRange")]
        public bool DefaultClosePriceRange { get; set; }
        public bool DefaultClosePriceRange_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.DefaultCloseSpecifications")]
        public bool DefaultCloseSpecifications { get; set; }
        public bool DefaultCloseSpecifications_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.DefaultCloseAttributes")]
        public bool DefaultCloseAttributes { get; set; }
        public bool DefaultCloseAttributes_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.DefaultCloseManufacturers")]
        public bool DefaultCloseManufacturers { get; set; }
        public bool DefaultCloseManufacturers_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.DefaultCloseVendors")]
        public bool DefaultCloseVendors { get; set; }
        public bool DefaultCloseVendors_OverrideForStore { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SelectorForListPanel")]
        public string SelectorForListPanel { get; set; }
        public bool SelectorForListPanel_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SelectorForGridPanel")]
        public string SelectorForGridPanel { get; set; }
        public bool SelectorForGridPanel_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SelectorForPager")]
        public string SelectorForPager { get; set; }
        public bool SelectorForPager_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SelectorForSortOptions")]
        public string SelectorForSortOptions { get; set; }
        public bool SelectorForSortOptions_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SelectorForViewOptions")]
        public string SelectorForViewOptions { get; set; }
        public bool SelectorForViewOptions_OverrideForStore { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SelectorForProductPageSize")]
        public string SelectorForProductPageSize { get; set; }
        public bool SelectorForProductPageSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.ProductSelectorsSelector")]
        public string ProductSelectorsSelector { get; set; }
        public bool ProductSelectorsSelector_OverrideForStore { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SelectorForScrolling")]
        public string SelectorForScrolling { get; set; }
        public bool SelectorForScrolling_OverrideForStore { get; set; }


        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.DefaultProductSorting")]
        public string DefaultProductSorting { get; set; }
        public IList<SelectListItem> DefaultProductSortingValues { get; set; }
        public bool DefaultProductSorting_OverrideForStore { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.ScrollAfterFiltration")]
        public bool ScrollAfterFiltration { get; set; }
        public bool ScrollAfterFiltration_OverrideForStore { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.showDebugInfo")]
        public bool showDebugInfo { get; set; }
 

        //1.04
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.FiltersConditionSpecifications")]
        public string FiltersConditionSpecifications { get; set; }
        public bool FiltersConditionSpecifications_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableFiltersConditionSpecifications { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.FiltersConditionAttributes")]
        public string FiltersConditionAttributes { get; set; }
        public bool FiltersConditionAttributes_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableFiltersConditionAttributes { get; set; }

        //1.06
        //Allow select several filters in one block
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.AllowSelectFiltersInOneBlock")]
        public bool AllowSelectFiltersInOneBlock { get; set; }
        public bool AllowSelectFiltersInOneBlock_OverrideForStore { get; set; }

        //1.11
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.FiltersConditionBetweenBlocks")]
        public string FiltersConditionBetweenBlocks { get; set; }
        public bool FiltersConditionBetweenBlocks_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableFiltersConditionBetweenBlocks { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.CustomKeyword")]
        public string GlobalCustomKeyword { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.MetaTitle")]
        public string GlobalMetaTitle { get; set; }


        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.MetaDescription")]
        public string GlobalMetaDescription { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.MetaKeyWord")]
        public string GlobalMetaKeyWord { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.HTag")]
        public string GlobalHTag { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.HeaderCopy")]
        public string GlobalHeaderCopy { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.CustomKeyword")]
        public string SpecificCustomKeyword { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.MetaTitle")]
        public string SpecificMetaTitle { get; set; }


        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.MetaDescription")]
        public string SpecificMetaDescription { get; set; }
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.MetaKeyWord")]
        public string SpecificMetaKeyWord { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.HTag")]
        public string SpecificHTag { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.H2Tag")]
        public string SpecificH2Tag { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.HeaderCopy")]
        public string SpecificHeaderCopy { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.Category")]
        public int OptimizedCategory { get; set; }

        public string OptimizedCategoryString { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.HeaderTitle")]
        public string GlobalHeaderTitle { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.HeaderTitle")]
        public string SpecificHeaderTitle { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.GlobalFooterContent1")]
        public string GlobalFooterContent1 { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.GlobalFooterContent2")]
        public string GlobalFooterContent2 { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.GlobalFooterContent3")]
        public string GlobalFooterContent3 { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SpecificFooterContent1")]
        public string SpecificFooterContent1 { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SpecificFooterContent2")]
        public string SpecificFooterContent2 { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SpecificFooterContent3")]
        public string SpecificFooterContent3 { get; set; }
                
        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.GlobalFooterTitle1")]
        public string GlobalFooterTitle1 { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.GlobalFooterTitle2")]
        public string GlobalFooterTitle2 { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.GlobalFooterTitle3")]
        public string GlobalFooterTitle3 { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SpecificFooterTitle1")]
        public string SpecificFooterTitle1 { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SpecificFooterTitle2")]
        public string SpecificFooterTitle2 { get; set; }

        [NopResourceDisplayName("FoxNetSoft.Plugin.Misc.SpeedFilters.SpecificFooterTitle3")]
        public string SpecificFooterTitle3 { get; set; }

    }
}
