using FoxNetSoft.Plugin.Misc.SpeedFilters.Models;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Filter;
using System.Collections.Generic;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Domain
{
    /// <summary>
    /// Represents a SpeedFilter
    /// </summary>
    public class SpeedFilter : BaseEntity
    {
        #region Constructors

        public SpeedFilter()
        {
            specificationAttributes = new List<sfSpecificationAttribute>();
            specificationAttributeOptions = new List<sfSpecificationAttributeOption>();
            productAttributes = new List<SpeedFilter.sfProductAttribute>();
            productAttributesOptions = new List<SpeedFilter.sfProductAttributeOption>();

            sfManufacturers = new List<SpeedFilter.sfManufacturer>();
            sfVendors = new List<SpeedFilter.sfVendor>();
            this.seoModel = new SpeedFilterSeoModel();
            SubCategories = new List<Category>();
        }
        #endregion

        #region Properties

        public SpeedFilterSeoModel seoModel { get; set; }

        public string generatedUrl { get; set; }
        public int categoryId { get; set; }
        public int manufacturerId { get; set; }
        public int vendorId { get; set; }
        public bool isSearchPage { get; set; }
        /*
        public string action { get; set; }
        */
        public bool hasError { get; set; }
        public string ErrorMessage { get; set; }

        //setting
        public SpeedFiltersSettings speedFiltersSettings { get; set; }

        public bool SkipFirstLoadingForFilters { get; set; }


        /// <summary>
        /// Gets or sets the price min
        /// </summary>
        public int PriceMin { get; set; }
        public string PriceMinStr { get; set; }
        public decimal PriceMinBase { get; set; }

        /// <summary>
        /// Gets or sets the price max
        /// </summary>
        public int PriceMax { get; set; }
        public string PriceMaxStr { get; set; }
        public decimal PriceMaxBase { get; set; }

        /// SpecificationsFilter  
        public List<sfSpecificationAttribute> specificationAttributes { get; set; }
        public List<sfSpecificationAttributeOption> specificationAttributeOptions { get; set; }

        /// AttributesFilter  
        public List<SpeedFilter.sfProductAttribute> productAttributes { get; set; }
        public List<SpeedFilter.sfProductAttributeOption> productAttributesOptions { get; set; }

        /// ManufacturerFilter  
        public List<sfManufacturer> sfManufacturers { get; set; }

        /// VendorFilter  
        public List<sfVendor> sfVendors { get; set; }

        //_catalogSettings.DefaultViewMode
        public string DefaultViewMode { get; set; }
        public int DefaultSortOption { get; set; }
        public int DefaultPageSize { get; set; }
        public string PluginPath { get; set; }

        #endregion

        #region Nested classes sfSpecificationAttribute

        public partial class sfSpecificationAttribute
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int DisplayOrder { get; set; }
        }

        #endregion

        #region Nested classes sfSpecificationAttributeOption

        public partial class sfSpecificationAttributeOption
        {
            public int Id { get; set; }
            public int SpecificationAttributeId { get; set; }
            public string Name { get; set; }
            public int DisplayOrder { get; set; }
        }

        #endregion

        #region Nested classes sfProductAttribute

        public partial class sfProductAttribute
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region Nested classes sfProductAttributeOption

        public partial class sfProductAttributeOption
        {
            public int Id { get; set; }
            public int ProductAttributeId { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region Nested classes sfManufacturer
        public partial class sfManufacturer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int DisplayOrder { get; set; }
        }
        #endregion

        #region Nested classes sfVendor
        public partial class sfVendor
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        #endregion

        public IList<Category> SubCategories { get; set; }
    }
}
