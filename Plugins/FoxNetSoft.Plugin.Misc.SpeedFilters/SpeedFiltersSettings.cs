using Nop.Core.Configuration;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters
{
    public class SpeedFiltersSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a Enabled for plugin
        /// </summary>
        public bool EnableSpeedFilters { get; set; }

        public bool IgnoreDiscountsForCatalog { get; set; }

        //WidgetZone
        public string WidgetZone { get; set; }
        //FiltersUIMode
        public string FiltersType { get; set; }

        //EnableFiltersForCategory
        public bool EnableFiltersForCategory { get; set; }
        //SkipFiltersforCategories
        public string SkipFiltersforCategories { get; set; }

        //EnableFiltersForManufacturer
        public bool EnableFiltersForManufacturer { get; set; }
        public string SkipFiltersforManufacturers { get; set; }

        //EnableFiltersForVendor
        public bool EnableFiltersForVendor { get; set; }
        public string SkipFiltersforVendors { get; set; }

        /// <summary>
        /// Gets or sets a Enable Filters On Search Page
        /// </summary>
        public bool EnableFiltersForSearchPage { get; set; }
        //---------------------------------------
        //EnablePriceRangeFilter
        public bool EnablePriceRangeFilter { get; set; }
        public bool DefaultClosePriceRange { get; set; }
        //EnableSpecificationsFilter
        public bool EnableSpecificationsFilter { get; set; }
        public bool DefaultCloseSpecifications { get; set; }
        //EnableAttributesFilter
        public bool EnableAttributesFilter { get; set; }
        public bool DefaultCloseAttributes { get; set; }
        //EnableManufacturersFilter
        public bool EnableManufacturersFilter { get; set; }
        public bool DefaultCloseManufacturers { get; set; }
        //EnableVendorsFilter
        public bool EnableVendorsFilter { get; set; }
        public bool DefaultCloseVendors { get; set; }

        //---------------------------------------
        //SelectorForListPanel
        public string SelectorForListPanel { get; set; }
        //SelectorForGridPanel
        public string SelectorForGridPanel { get; set; }
        //SelectorForPager
        public string SelectorForPager { get; set; }
        //SelectorForSortOptions
        public string SelectorForSortOptions { get; set; }
        //SelectorForViewOptions
        public string SelectorForViewOptions { get; set; }
        //SelectorForProductPageSize
        public string SelectorForProductPageSize { get; set; }

        public string ProductSelectorsSelector { get; set; }
        
        //ElementToScrollAfterFiltrationSelector
        public string SelectorForScrolling { get; set; }

        //---------------------------------------
        public Nop.Core.Domain.Catalog.ProductSortingEnum DefaultProductSorting { get; set; }
        //ScrollToElementOnThePageAfterFiltration
        public bool ScrollAfterFiltration { get; set; }

        /// <summary>
        /// Gets or sets the showDebugInfo to set plugin
        /// </summary>
        /// <value>
        /// the showDebugInfo property of plugin
        /// </value>
        public bool showDebugInfo { get; set; }

        /// <summary>
        /// Gets or sets a version, for future update
        /// </summary>
        public int Version { get; set; }

        //1.04
        public string FiltersConditionSpecifications { get; set; }
        public string FiltersConditionAttributes { get; set; }

        //1.06
        //Allow select several filters in one block
        public bool AllowSelectFiltersInOneBlock { get; set; }

        //1.11
        public string FiltersConditionBetweenBlocks { get; set; }

        /// <summary>
        /// Gets or sets the prepareSpecificationAttributes
        /// </summary>
        public bool prepareSpecificationAttributes { get; set; }

        public string GlobalCustomKeyword { get; set; }
        public string GlobalMetaTitle { get; set; }
        public string GlobalMetaDescription { get; set; }
        public string GlobalMetaKeyWord { get; set; }
        public string GlobalHTag { get; set; }
        public string GlobalHeaderCopy { get; set; }
        public string SpecificCustomKeyword { get; set; }
        public string SpecificMetaTitle { get; set; }
        public string SpecificMetaDescription { get; set; }
        public string SpecificMetaKeyWord { get; set; }
        public string SpecificHTag { get; set; }
        public string SpecificH2Tag { get; set; }
        public string SpecificHeaderCopy { get; set; }
        public string OptimizedCategory { get; set; }
        public string GlobalHeaderTitle { get; set; }
        public string SpecificHeaderTitle { get; set; }
        public string GlobalFooterContent1 { get; set; }
        public string GlobalFooterContent2 { get; set; }
        public string GlobalFooterContent3 { get; set; }
        public string SpecificFooterContent1 { get; set; }
        public string SpecificFooterContent2 { get; set; }
        public string SpecificFooterContent3 { get; set; }

        public string GlobalFooterTitle1 { get; set; }
        public string GlobalFooterTitle2 { get; set; }
        public string GlobalFooterTitle3 { get; set; }

        public string SpecificFooterTitle1 { get; set; }
        public string SpecificFooterTitle2 { get; set; }
        public string SpecificFooterTitle3 { get; set; }
    }
}
