using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Models;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Domain;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Services
{
    /// <summary>
    /// SpeedFilters service
    /// </summary>
    public interface ISpeedFiltersService
    {
        /// <summary>
        /// GetFilteredProducts
        /// </summary>
        /// <param name="pagedProducts">Products</param>
        /// <param name="subProducts">Products subProducs Group</param>
        /// <param name="productsSEOSlug">Products SEO Slug</param>
        /// <param name="filterableSpecificationAttributeOptionIds">The specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="filterableProductttributeOptionIds">The Product attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="filterableManufacturerIds">The Manufacturer identifiers applied to loaded products (all pages)</param>
        /// <param name="filterableVendorIds">The Vendor identifiers applied to loaded products (all pages)</param>
        /// <param name="hasError">Logical return Error, when Error have heppend</param>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="manufacturerIds">Manufacturers list of the identifier; empty to load all records</param>
        /// <param name="vendorIds">Vendors list of the identifier; empty to load all records</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="allowedCustomerRolesIds">customer Roles Ids</param>
        /// <param name="ShowProductsFromSubcategories">Show Products From Subcategories</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="filteredSpecs">Filtered product specification identifiers</param>
        /// <param name="filteredAtrs">Filtered product atribute identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showOnSaldo">view saldo</param>
        /// <param name="speedFiltersSettings">SpeedFilters Settings</param>
        void GetFilteredProducts(
            out IPagedList<Product> pagedProducts,
            out IList<Product> subProducts,
            out IList<ProductSEOModel> productsSEOSlug,
            out IList<int> filterableSpecificationAttributeOptionIds,
            out IList<int> filterableProductAttributeOptionIds,
            out IList<int> filterableManufacturerIds,
            out IList<int> filterableVendorIds,
            out bool hasError,
            int categoryId = 0,
            IList<int> manufacturerIds = null,
            IList<int> vendorIds = null,
            int storeId = 0,
            int languageId = 0,
            IList<int> allowedCustomerRolesIds = null,
            bool ShowProductsFromSubcategories = false,
            int pageIndex = 0,
            int pageSize = 2147483647,  //Int32.MaxValue
            bool? featuredProducts = null,
            string keywords = null,
            bool searchDescriptions = false,
            decimal? priceMin = null,
            decimal? priceMax = null,
            IList<SelectedSpeedFilter.FilterElement> filteredSpecs = null,
            IList<SelectedSpeedFilter.FilterElement> filteredAtrs = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            string showOnSaldo = "all",
            SpeedFiltersSettings speedFiltersSettings =null
            );

        /// <summary>
        /// Get SpeedFilter by Category identifier
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="searchModel">Search Model</param>
        /// <param name="speedFiltersSettings">SpeedFiltersSettings</param>
        /// <returns>SpeedFilter</returns>
        SpeedFilter GetSpeedFilters(int categoryId, int manufacturerId, int vendorId, SearchModel searchModel, SpeedFiltersSettings speedFiltersSettings);

        /// <summary>
        /// Authorize by Category (Manufacturer, Vendor) identifier
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="allowedCustomerRolesIds">customer Roles Ids</param>
        /// <returns>bool</returns>
        bool Authorize(int categoryId, int manufacturerId, int vendorId, IList<int> allowedCustomerRolesIds = null);

        RetrunUrlResult GenerateSpecificationUrl(string FilterUrl);

        void InsertSpecificCategory(SS_Specific_Category_Setting model);

        void UpdateSpecificCategory(SS_Specific_Category_Setting model);

        SS_Specific_Category_Setting GetSpecificCategorySettingById(int id);

        SS_Specific_Category_Setting GetSpecificCategorySettingByCategoryId(int categoryId);

    }
}
