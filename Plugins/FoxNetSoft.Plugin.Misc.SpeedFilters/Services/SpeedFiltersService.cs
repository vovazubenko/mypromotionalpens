using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Data;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Logger;
using Nop.Services.Configuration;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Core.Domain.Common;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Domain;
using System.Data.Entity.Infrastructure;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Data;
using System.Text;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Models;
using Nop.Services.Catalog;
using Nop.Services.Vendors;
using Nop.Core.Domain.Vendors;
using System.Globalization;
using System.Data.SqlClient;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Services
{
    public class SpeedFiltersService : ISpeedFiltersService
    {
        #region Constants
        //SpeedFilters

        //SpeedFilters by categoryId
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : categoryId
        /// {1} : _storeContext.CurrentStore.Id
        /// {2} : ACL comma separeted roles
        /// </remarks>
        private const string SPEEDFILTER_BYCATEGORYID = "FoxNetSoft.SpeedFilters.categoryId-{0}-{1}-{2}";

        //SpeedFilters by manufacturerId
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : manufacturerId
        /// {1} : _storeContext.CurrentStore.Id
        /// {2} : ACL comma separeted roles
        /// </remarks>
        private const string SPEEDFILTER_BYMANUFACTURERID = "FoxNetSoft.SpeedFilters.manufacturerId-{0}-{1}-{2}";

        //SpeedFilters by vendorId
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : vendorId
        /// {1} : _storeContext.CurrentStore.Id
        /// {2} : ACL comma separeted roles
        /// </remarks>
        private const string SPEEDFILTER_BYVENDORID = "FoxNetSoft.SpeedFilters.vendorId-{0}-{1}-{2}";

        //Authorize by categoryId,manufacturerId,vendorId

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : categoryId
        /// {1} : _storeContext.CurrentStore.Id
        /// {2} : ACL comma separeted roles
        /// </remarks>
        private const string SPEEDFILTER_AUTHORIZE_BYCATEGORYID = "FoxNetSoft.SpeedFilters.Authorize.categoryId-{0}-{1}-{2}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : manufacturerId
        /// {1} : _storeContext.CurrentStore.Id
        /// {2} : ACL comma separeted roles
        /// </remarks>
        private const string SPEEDFILTER_AUTHORIZE_BYMANUFACTURERID = "FoxNetSoft.SpeedFilters.Authorize.manufacturerId-{0}-{1}-{2}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : vendorId
        /// {1} : _storeContext.CurrentStore.Id
        /// {2} : ACL comma separeted roles
        /// </remarks>
        private const string SPEEDFILTER_AUTHORIZE_BYVENDORID = "FoxNetSoft.SpeedFilters.Authorize.vendorId-{0}-{1}-{2}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string SPEEDFILTERS_PATTERN_KEY = "FoxNetSoft.SpeedFilters.";

        #endregion

        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        private readonly ICacheManager _cacheManagerStatic;

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<SS_Specific_Category_Setting> _specificRepository;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly ISettingService _settingService;
        private readonly ILanguageService _languageService;
        private readonly CommonSettings _commonSettings;
        private readonly CatalogSettings _catalogSettings;

        //private readonly IAclService2 _aclservice;
        //private readonly IStoreMappingService2 _storeMappingservice;

        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IVendorService _vendorService;
        private readonly VendorSettings _vendorSettings;

        private bool showDebugInfo;
        private readonly FNSLogger _fnsLogger;

        private bool IgnoreDiscountsForCatalog;

        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        private readonly IRepository<SpecificationAttributeOption> _specificationAttributeOptionRepository;
        #endregion

        #region Ctor
        public SpeedFiltersService(
                IEventPublisher eventPublisher,
                ICacheManager _cacheManager,
                IRepository<Product> productRepository,
                IRepository<Category> categoryRepository,
                ILogger logger,
                IWorkContext workContext,
                IStoreContext storeContext,
                IDataProvider dataProvider,
                IDbContext dbContext,
                ISettingService settingService,
                ILanguageService languageService,
                CommonSettings commonSettings,
                CatalogSettings catalogSettings,
                //IAclService2 aclservice,
                //IStoreMappingService2 storeMappingservice,
                ICategoryService categoryService,
                IManufacturerService manufacturerService,
                IVendorService vendorService,
                VendorSettings vendorSettings,
                IRepository<SpecificationAttribute> specificationAttributeRepository,
                IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository,
                IRepository<SS_Specific_Category_Setting> specificRepository
            )
        {
            this._eventPublisher = eventPublisher;
            this._cacheManager = _cacheManager;
            this._productRepository = productRepository;
            this._categoryRepository = categoryRepository;
            this._logger = logger;
            this._workContext = workContext;
            this._storeContext = storeContext;

            this._dataProvider = dataProvider;
            this._dbContext = dbContext;
            //this._dbContext = EngineContext.Current.ContainerManager.Resolve<IDbContext>("nop_object_context_foxnetsoft_speedfilters");

            this._settingService = settingService;
            this._languageService = languageService;
            this._commonSettings = commonSettings;
            this._catalogSettings = catalogSettings;
            //this._aclservice = aclservice;
            // this._storeMappingservice = storeMappingservice;

            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._vendorService = vendorService;

            this._vendorSettings = vendorSettings;

            this._cacheManagerStatic = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");

            var speedFiltersSettings = _settingService.LoadSetting<SpeedFiltersSettings>();
            this.showDebugInfo = speedFiltersSettings.showDebugInfo;
            this._fnsLogger = new FNSLogger(this.showDebugInfo);
            //this.IgnoreDiscountsForCatalog = settingService.GetSettingByKey<bool>("MSSQLProviderSettings.IgnoreDiscountsForCatalog", true);
            this.IgnoreDiscountsForCatalog = settingService.GetSettingByKey<bool>("SpeedFiltersSettings.IgnoreDiscountsForCatalog", true);
            this._specificationAttributeRepository = specificationAttributeRepository;
            this._specificationAttributeOptionRepository = specificationAttributeOptionRepository;
            this._specificRepository = specificRepository;

        }
        #endregion

        #region Utils
        private void LogMessage(string message)
        {
            if (this.showDebugInfo)
            {
                this._fnsLogger.LogMessage(message);
            }
        }

        private void AttachTierPrice(Product product, IList<TierPrice> tierPrices)
        {
            if (product.HasTierPrices)
            {
                var tierPrices2 = tierPrices.Where(t => t.ProductId == product.Id).OrderBy(o => o.Quantity).ToList();
                if (tierPrices2.Count > 0)
                {
                    foreach (var tierPrice in tierPrices2)
                    {
                        product.TierPrices.Add(tierPrice);
                        tierPrice.Product = product;
                    }
                }
            }
        }

        #endregion

        #region Implementation of ISpeedFiltersService

        #region GetFilteredProducts (products)

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
        public virtual void GetFilteredProducts(
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
            SpeedFiltersSettings speedFiltersSettings = null
            )
        {
            DateTime datetime = DateTime.Now;
            int totalRecords = 0;
            string filterableSpecificationAttributeOptionIdsStr = "";
            string filterableProductAttributeOptionIdsStr = "";
            string filterableManufacturerIdsStr = "";
            string filterableVendorIdsStr = "";

            productsSEOSlug = new List<ProductSEOModel>();
            pagedProducts = new PagedList<Product>(new List<Product>(), pageIndex, pageSize, 0);
            subProducts = new List<Product>();
            filterableSpecificationAttributeOptionIds = new List<int>();
            filterableProductAttributeOptionIds = new List<int>();
            filterableManufacturerIds = new List<int>();
            filterableVendorIds = new List<int>();
            hasError = false;

            IList<Product> products = new List<Product>();
            IList<TierPrice> tierPrices = new List<TierPrice>();

            //stored procedures are enabled and supported by the database. 
            //It's much faster than the LINQ implementation below 

            #region Use stored procedure

            //pass manufactirer identifiers as comma-delimited string
            string commaSeparatedManufacturers = "";
            for (int i = 0; i < manufacturerIds.Count; i++)
            {
                commaSeparatedManufacturers += manufacturerIds[i].ToString();
                if (i != manufacturerIds.Count - 1)
                {
                    commaSeparatedManufacturers += ",";
                }
            }

            string commaSeparatedVendors = "";
            for (int i = 0; i < vendorIds.Count; i++)
            {
                commaSeparatedVendors += vendorIds[i].ToString();
                if (i != vendorIds.Count - 1)
                {
                    commaSeparatedVendors += ",";
                }
            }

            //pass customer role identifiers as comma-delimited string
            string commaSeparatedAllowedCustomerRoleIds = "";
            for (int i = 0; i < allowedCustomerRolesIds.Count; i++)
            {
                commaSeparatedAllowedCustomerRoleIds += allowedCustomerRolesIds[i].ToString();
                if (i != allowedCustomerRolesIds.Count - 1)
                {
                    commaSeparatedAllowedCustomerRoleIds += ",";
                }
            }

            //pass specification identifiers as comma-delimited string
            //1,2,3,4,56,58,47
            string commaSeparatedSpecIds = "";
            if (filteredSpecs != null)
            {
                //((List<int>)filteredSpecs).Sort();
                for (int i = 0; i < filteredSpecs.Count; i++)
                {
                    //                    commaSeparatedSpecIds += filteredSpecs[i].Id.ToString();
                    commaSeparatedSpecIds += filteredSpecs[i].BlockId.ToString() + '>' + filteredSpecs[i].Id.ToString();
                    if (i != filteredSpecs.Count - 1)
                    {
                        commaSeparatedSpecIds += ",";
                    }
                }
            }

            //pass product atributes identifiers as comma-delimited string
            //1>47,1>58,3>45,3>4,3>56,1>58,1>47
            string commaSeparatedAtrIds = "";
            if (filteredAtrs != null)
            {
                //((List<int>)filteredAtrs).Sort();
                for (int i = 0; i < filteredAtrs.Count; i++)
                {
                    commaSeparatedAtrIds += filteredAtrs[i].BlockId.ToString() + '>' + filteredAtrs[i].Id.ToString();
                    if (i != filteredAtrs.Count - 1)
                    {
                        commaSeparatedAtrIds += ",";
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            if (this.showDebugInfo)
            {
                sb.AppendLine(String.Format("SpeedFiltersService. GetFilteredProducts. categoryId={0}, manufacturerIds={1}, vendorIds={2}, pageIndex={3}, pageSize={4}, orderBy={5}, commaSeparatedSpecIds={6}, commaSeparatedAtrIds={7}",
                    categoryId, string.Join(", ", manufacturerIds), string.Join(", ", vendorIds), pageIndex, pageSize, orderBy, commaSeparatedSpecIds, commaSeparatedAtrIds));
            }

            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            //prepare parameters
            var pCategoryId = _dataProvider.GetParameter();
            pCategoryId.ParameterName = "CategoryId";
            pCategoryId.Value = categoryId;
            pCategoryId.DbType = DbType.String;

            var pManufacturerIds = _dataProvider.GetParameter();
            pManufacturerIds.ParameterName = "ManufacturerIds";
            pManufacturerIds.Value = commaSeparatedManufacturers;
            pManufacturerIds.DbType = DbType.String;

            var pVendorIds = _dataProvider.GetParameter();
            pVendorIds.ParameterName = "VendorIds";
            pVendorIds.Value = commaSeparatedVendors;
            pVendorIds.DbType = DbType.String;

            var pStoreId = _dataProvider.GetParameter();
            pStoreId.ParameterName = "StoreId";
            pStoreId.Value = storeId;
            pStoreId.DbType = DbType.Int32;

            var pLanguageId = _dataProvider.GetParameter();
            pLanguageId.ParameterName = "LanguageId";
            pLanguageId.Value = languageId;
            pLanguageId.DbType = DbType.Int32;

            var pAllowedCustomerRoleIds = _dataProvider.GetParameter();
            pAllowedCustomerRoleIds.ParameterName = "AllowedCustomerRoleIds";
            pAllowedCustomerRoleIds.Value = commaSeparatedAllowedCustomerRoleIds;
            pAllowedCustomerRoleIds.DbType = DbType.String;

            var pShowProductsFromSubcategories = _dataProvider.GetParameter();
            pShowProductsFromSubcategories.ParameterName = "ShowProductsFromSubcategories";
            pShowProductsFromSubcategories.Value = ShowProductsFromSubcategories;
            pShowProductsFromSubcategories.DbType = DbType.Boolean;

            var pIgnoreDiscounts = _dataProvider.GetParameter();
            pIgnoreDiscounts.ParameterName = "IgnoreDiscounts";
            pIgnoreDiscounts.Value = this.IgnoreDiscountsForCatalog || _catalogSettings.IgnoreDiscounts;
            pIgnoreDiscounts.DbType = DbType.Boolean;

            var pPageIndex = _dataProvider.GetParameter();
            pPageIndex.ParameterName = "PageIndex";
            pPageIndex.Value = pageIndex;
            pPageIndex.DbType = DbType.Int32;

            var pPageSize = _dataProvider.GetParameter();
            pPageSize.ParameterName = "PageSize";
            pPageSize.Value = pageSize;
            pPageSize.DbType = DbType.Int32;

            var pFeaturedProducts = _dataProvider.GetParameter();
            pFeaturedProducts.ParameterName = "FeaturedProducts";
            pFeaturedProducts.Value = featuredProducts.HasValue ? (object)featuredProducts.Value : DBNull.Value;
            pFeaturedProducts.DbType = DbType.Boolean;

            var pKeywords = _dataProvider.GetParameter();
            pKeywords.ParameterName = "Keywords";
            pKeywords.Value = keywords != null ? (object)keywords : DBNull.Value;
            pKeywords.DbType = DbType.String;

            var pSearchDescriptions = _dataProvider.GetParameter();
            pSearchDescriptions.ParameterName = "SearchDescriptions";
            pSearchDescriptions.Value = searchDescriptions;
            pSearchDescriptions.DbType = DbType.Boolean;

            var pUseFullTextSearch = _dataProvider.GetParameter();
            pUseFullTextSearch.ParameterName = "UseFullTextSearch";
            pUseFullTextSearch.Value = _commonSettings.UseFullTextSearch;
            pUseFullTextSearch.DbType = DbType.Boolean;

            var pFullTextMode = _dataProvider.GetParameter();
            pFullTextMode.ParameterName = "FullTextMode";
            pFullTextMode.Value = (int)_commonSettings.FullTextMode;
            pFullTextMode.DbType = DbType.Int32;

            var pPriceMin = _dataProvider.GetParameter();
            pPriceMin.ParameterName = "PriceMin";
            pPriceMin.Value = priceMin.HasValue ? (object)priceMin.Value : DBNull.Value;
            pPriceMin.DbType = DbType.Decimal;

            var pPriceMax = _dataProvider.GetParameter();
            pPriceMax.ParameterName = "PriceMax";
            pPriceMax.Value = priceMax.HasValue ? (object)priceMax.Value : DBNull.Value;
            pPriceMax.DbType = DbType.Decimal;

            var pOrderBy = _dataProvider.GetParameter();
            pOrderBy.ParameterName = "OrderBy";
            pOrderBy.Value = (int)orderBy;
            pOrderBy.DbType = DbType.Int32;

            var pshowOnSaldo = _dataProvider.GetParameter();
            pshowOnSaldo.ParameterName = "showOnSaldo";
            pshowOnSaldo.Value = showOnSaldo;
            pshowOnSaldo.DbType = DbType.String;

            var pFilteredSpecs = _dataProvider.GetParameter();
            pFilteredSpecs.ParameterName = "FilteredSpecs";
            pFilteredSpecs.Value = commaSeparatedSpecIds != null ? (object)commaSeparatedSpecIds : DBNull.Value;
            pFilteredSpecs.DbType = DbType.String;

            var pFilteredAtrs = _dataProvider.GetParameter();
            pFilteredAtrs.ParameterName = "filteredAtrs";
            pFilteredAtrs.Value = commaSeparatedAtrIds != null ? (object)commaSeparatedAtrIds : DBNull.Value;
            pFilteredAtrs.DbType = DbType.String;

            var pEnablePriceRangeFilter = _dataProvider.GetParameter();
            pEnablePriceRangeFilter.ParameterName = "enablePriceRangeFilter";
            pEnablePriceRangeFilter.Value = speedFiltersSettings.EnablePriceRangeFilter;
            pEnablePriceRangeFilter.DbType = DbType.Boolean;

            var pEnableSpecificationsFilter = _dataProvider.GetParameter();
            pEnableSpecificationsFilter.ParameterName = "enableSpecificationsFilter";
            pEnableSpecificationsFilter.Value = speedFiltersSettings.EnableSpecificationsFilter;
            pEnableSpecificationsFilter.DbType = DbType.Boolean;

            var pEnableAttributesFilter = _dataProvider.GetParameter();
            pEnableAttributesFilter.ParameterName = "enableAttributesFilter";
            pEnableAttributesFilter.Value = speedFiltersSettings.EnableAttributesFilter;
            pEnableAttributesFilter.DbType = DbType.Boolean;

            var pEnableManufacturersFilter = _dataProvider.GetParameter();
            pEnableManufacturersFilter.ParameterName = "enableManufacturersFilter";
            pEnableManufacturersFilter.Value = speedFiltersSettings.EnableManufacturersFilter;
            pEnableManufacturersFilter.DbType = DbType.Boolean;

            var pEnableVendorsFilter = _dataProvider.GetParameter();
            pEnableVendorsFilter.ParameterName = "enableVendorsFilter";
            pEnableVendorsFilter.Value = speedFiltersSettings.EnableVendorsFilter;
            pEnableVendorsFilter.DbType = DbType.Boolean;

            var pFiltersConditionSpecifications = _dataProvider.GetParameter();
            pFiltersConditionSpecifications.ParameterName = "filtersConditionSpecifications";
            pFiltersConditionSpecifications.Value = speedFiltersSettings.FiltersConditionSpecifications.Equals("AND", StringComparison.CurrentCultureIgnoreCase) ? true : false;
            pFiltersConditionSpecifications.DbType = DbType.Boolean;

            var pFiltersConditionAttributes = _dataProvider.GetParameter();
            pFiltersConditionAttributes.ParameterName = "filtersConditionAttributes";
            pFiltersConditionAttributes.Value = speedFiltersSettings.FiltersConditionAttributes.Equals("AND", StringComparison.CurrentCultureIgnoreCase) ? true : false;
            pFiltersConditionAttributes.DbType = DbType.Boolean;

            var pFiltersConditionBetweenBlocks = _dataProvider.GetParameter();
            pFiltersConditionBetweenBlocks.ParameterName = "filtersConditionBetweenBlocks";
            pFiltersConditionBetweenBlocks.Value = speedFiltersSettings.FiltersConditionBetweenBlocks.Equals("AND", StringComparison.CurrentCultureIgnoreCase) ? true : false;
            pFiltersConditionBetweenBlocks.DbType = DbType.Boolean;

            //invoke stored procedure
            DbContext db = (DbContext)_dbContext;
            NopObjectContext nopDbContext = (NopObjectContext)_dbContext;
            bool openingConnection = db.Database.Connection.State == ConnectionState.Closed;
            //var culture = System.Globalization.CultureInfo.InvariantCulture;
            try
            {
                //System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
                if (openingConnection) { db.Database.Connection.Open(); }
                //nopDbContext.Configuration.AutoDetectChangesEnabled=false;
                //    using(var connectionScope = cmd.Connection.CreateConnectionScope())

                // Create a SQL command to execute the sproc
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "FNS_SpeedFilter_ProductLoadAllPaged";
                /*+
                     "@CategoryId, @ManufacturerId, @VendorId, @StoreId, @LanguageId," +
                     "@AllowedCustomerRoleIds, " +
                     "@getCategorybreadcrumb, @getSubCategories, @getFeaturedProducts, @getProducts,@IgnoreDiscounts," +
                     "@PageIndex, @PageSize, @FeaturedProduct, @PriceMin, @PriceMax," +
                     "@OrderBy, @FilteredSpecs, @LoadFilterableSpecificationAttributeOptionIds OUTPUT, @TotalRecords OUTPUT"
                     ;*/
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(pCategoryId);
                cmd.Parameters.Add(pManufacturerIds);
                cmd.Parameters.Add(pVendorIds);
                cmd.Parameters.Add(pStoreId);
                cmd.Parameters.Add(pLanguageId);
                cmd.Parameters.Add(pAllowedCustomerRoleIds);
                cmd.Parameters.Add(pShowProductsFromSubcategories);
                cmd.Parameters.Add(pIgnoreDiscounts);
                cmd.Parameters.Add(pPageIndex);
                cmd.Parameters.Add(pPageSize);
                cmd.Parameters.Add(pFeaturedProducts);
                cmd.Parameters.Add(pKeywords);
                cmd.Parameters.Add(pSearchDescriptions);
                cmd.Parameters.Add(pUseFullTextSearch);
                cmd.Parameters.Add(pFullTextMode);
                cmd.Parameters.Add(pPriceMin);
                cmd.Parameters.Add(pPriceMax);
                cmd.Parameters.Add(pOrderBy);
                cmd.Parameters.Add(pshowOnSaldo);
                cmd.Parameters.Add(pFilteredSpecs);
                cmd.Parameters.Add(pFilteredAtrs);
                cmd.Parameters.Add(pEnablePriceRangeFilter);
                cmd.Parameters.Add(pEnableSpecificationsFilter);
                cmd.Parameters.Add(pEnableAttributesFilter);
                cmd.Parameters.Add(pEnableManufacturersFilter);
                cmd.Parameters.Add(pEnableVendorsFilter);
                cmd.Parameters.Add(pFiltersConditionSpecifications);
                cmd.Parameters.Add(pFiltersConditionAttributes);
                cmd.Parameters.Add(pFiltersConditionBetweenBlocks);

                using (var reader = cmd.ExecuteReader())
                {
                    sb.AppendLine("               getProducts. start.");

                    // Read Featured Product from the first result set
                    products = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<Product>(reader, nopDbContext.GetEntitySetName<Product>(), System.Data.Entity.Core.Objects.MergeOption.AppendOnly).ToList();
                    reader.NextResult();

                    //Read featured Sub Products
                    if (products.Count > 0)
                    {
                        subProducts = ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<Product>(reader, nopDbContext.GetEntitySetName<Product>(), System.Data.Entity.Core.Objects.MergeOption.AppendOnly).ToList();
                    }
                    reader.NextResult();
                    sb.AppendLine("               Total+FilterableSpec");

                    //Total+FilterableSpec
                    while (reader.Read())
                    {
                        totalRecords = Convert.ToInt32(reader["Total"]);
                        filterableSpecificationAttributeOptionIdsStr = reader["FilterableSpecificationAttributeOptionIds"].ToString();
                        if (!string.IsNullOrWhiteSpace(filterableSpecificationAttributeOptionIdsStr))
                        {
                            filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIdsStr
                               .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(x => Convert.ToInt32(x.Trim()))
                               .ToList();
                        }
                        filterableProductAttributeOptionIdsStr = reader["FilterableProductAttributeOptionIds"].ToString();
                        if (!string.IsNullOrWhiteSpace(filterableProductAttributeOptionIdsStr))
                        {
                            filterableProductAttributeOptionIds = filterableProductAttributeOptionIdsStr
                               .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(x => Convert.ToInt32(x.Trim()))
                               .ToList();
                        }
                        filterableManufacturerIdsStr = reader["FilterableManufacturerIds"].ToString();
                        if (!string.IsNullOrWhiteSpace(filterableManufacturerIdsStr))
                        {
                            filterableManufacturerIds = filterableManufacturerIdsStr
                               .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(x => Convert.ToInt32(x.Trim()))
                               .ToList();
                        }
                        filterableVendorIdsStr = reader["FilterableVendorIds"].ToString();
                        if (!string.IsNullOrWhiteSpace(filterableVendorIdsStr))
                        {
                            filterableVendorIds = filterableVendorIdsStr
                               .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(x => Convert.ToInt32(x.Trim()))
                               .ToList();
                        }
                    }
                    reader.NextResult();
                    sb.AppendLine("               getProducts. end.");

                    //Read SEO Slug

                    sb.AppendLine("               Read SEO Slug. start.");
                    while (reader.Read())
                    {
                        productsSEOSlug.Add(new ProductSEOModel()
                        {
                            ProductId = Convert.ToInt32(reader["ProductId"]),
                            SeName = reader["SeName"].ToString()
                        });
                    }
                    reader.NextResult();
                    sb.AppendLine("               Read SEO Slug. end.");

                    //Read TierPrice

                    sb.AppendLine("              TierPrice. start.");
                    tierPrices = ((IObjectContextAdapter)db)
                         .ObjectContext
                         .Translate<TierPrice>(reader, nopDbContext.GetEntitySetName<TierPrice>(), System.Data.Entity.Core.Objects.MergeOption.AppendOnly).ToList();
                    //                        reader.NextResult();
                    sb.AppendLine("              TierPrice. end.");

                }
            }
            catch (Exception e)
            {

                //_logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Information,"Error "+e.ToString(),fullMessage: e.Message);
                if (this.showDebugInfo)
                    sb.AppendLine("-----------------> Error " + e.Message);
                totalRecords = 0;

                productsSEOSlug = new List<ProductSEOModel>();
                subProducts = new List<Product>();
                products = new List<Product>();
                tierPrices = new List<TierPrice>();
                filterableSpecificationAttributeOptionIds = new List<int>();
                filterableProductAttributeOptionIds = new List<int>();
                filterableManufacturerIds = new List<int>();
                filterableVendorIds = new List<int>();
                hasError = true;
            }
            finally
            {
                //db.Database.Connection.Close();
                if (openingConnection && db.Database.Connection.State == ConnectionState.Open) { db.Database.Connection.Close(); }
                //System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            }
            #endregion

            #region Attach

            var oldAutoDetectChangesEnabled = nopDbContext.Configuration.AutoDetectChangesEnabled;
            nopDbContext.Configuration.AutoDetectChangesEnabled = false;

            if (products.Count > 0)
            {
                foreach (var product in products)
                {
                    AttachTierPrice(product, tierPrices);
                    this._dbContext.Set<Product>().Attach(product);
                }
                foreach (var product in subProducts)
                {
                    AttachTierPrice(product, tierPrices);
                    this._dbContext.Set<Product>().Attach(product);
                }
            }
            if (tierPrices.Count > 0)
            {
                foreach (var tierPrice in tierPrices)
                    this._dbContext.Set<TierPrice>().Attach(tierPrice);
            }

            nopDbContext.Configuration.AutoDetectChangesEnabled = oldAutoDetectChangesEnabled;

            pagedProducts = new PagedList<Product>(products, pageIndex, pageSize, totalRecords);

            #endregion

            #region Debug
            if (this.showDebugInfo)
            {
                sb.AppendLine("        Filtered Products");
                foreach (var smcat in pagedProducts)
                    sb.AppendLine(String.Format("              Id={0},Name={1}", smcat.Id, smcat.Name));

                sb.AppendLine(String.Format(" totalRecords={0} ", totalRecords));
                sb.AppendLine(String.Format(" filterableSpecificationAttributeOptionIdsStr={0} ", filterableSpecificationAttributeOptionIdsStr));
                sb.AppendLine(String.Format(" filterableProductAttributeOptionIdsStr={0} ", filterableProductAttributeOptionIdsStr));
                sb.AppendLine(String.Format(" filterableManufacturerIdsStr={0} ", filterableManufacturerIdsStr));
                sb.AppendLine(String.Format(" filterableVendorIdsStr={0} ", filterableVendorIdsStr));

                TimeSpan sp = DateTime.Now - datetime;
                sb.AppendLine();
                sb.AppendLine(String.Format(" duration={0} ", sp.ToString()));

                LogMessage(sb.ToString());
            }
            #endregion
        }
        #endregion

        #region GetSpeedFilters
        /// <summary>
        /// Get SpeedFilter by Category identifier
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="searchModel">Search Model</param>
        /// <param name="speedFiltersSettings">SpeedFiltersSettings</param>
        /// <returns>SpeedFilter</returns>
        public virtual SpeedFilter GetSpeedFilters(int categoryId, int manufacturerId, int vendorId, SearchModel searchModel, SpeedFiltersSettings speedFiltersSettings)
        {
            DateTime datetime = DateTime.Now;

            //            string key = string.Format(CATEGORIES_BY_PARENT_CATEGORY_ID_KEY, parentCategoryId,  _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            //Access control list. Allowed customer roles
            var allowedCustomerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                .Where(cr => cr.Active).Select(cr => cr.Id).ToList();

            //pass customer role identifiers as comma-delimited string
            string commaSeparatedAllowedCustomerRoleIds = "";
            for (int i = 0; i < allowedCustomerRolesIds.Count; i++)
            {
                commaSeparatedAllowedCustomerRoleIds += allowedCustomerRolesIds[i].ToString();
                if (i != allowedCustomerRolesIds.Count - 1)
                {
                    commaSeparatedAllowedCustomerRoleIds += ",";
                }
            }
            string key = string.Format(SPEEDFILTER_BYCATEGORYID, categoryId, _storeContext.CurrentStore.Id, commaSeparatedAllowedCustomerRoleIds);
            if (manufacturerId > 0)
                key = string.Format(SPEEDFILTER_BYMANUFACTURERID, manufacturerId, _storeContext.CurrentStore.Id, commaSeparatedAllowedCustomerRoleIds);
            if (vendorId > 0)
                key = string.Format(SPEEDFILTER_BYVENDORID, vendorId, _storeContext.CurrentStore.Id, commaSeparatedAllowedCustomerRoleIds);
            if (searchModel.Enabled)
                key = string.Format(SPEEDFILTER_BYVENDORID, searchModel.RawUrl, _storeContext.CurrentStore.Id, commaSeparatedAllowedCustomerRoleIds);

            SpeedFilter speedFilter = _cacheManager.Get(key, () =>
            {
                #region store procedure
                //prepare parameters
                var pcategoryId = _dataProvider.GetParameter();
                pcategoryId.ParameterName = "categoryId";
                pcategoryId.Value = searchModel.Enabled ? searchModel.CategoryId : categoryId;
                pcategoryId.DbType = DbType.Int32;

                var pmanufacturerId = _dataProvider.GetParameter();
                pmanufacturerId.ParameterName = "manufacturerId";
                pmanufacturerId.Value = searchModel.Enabled ? searchModel.ManufacturerId : manufacturerId;
                pmanufacturerId.DbType = DbType.Int32;

                var pvendorId = _dataProvider.GetParameter();
                pvendorId.ParameterName = "vendorId";
                pvendorId.Value = vendorId;
                pvendorId.DbType = DbType.Int32;

                //Store mapping
                var pstoreId = _dataProvider.GetParameter();
                pstoreId.ParameterName = "storeId";
                pstoreId.Value = !_catalogSettings.IgnoreStoreLimitations ? _storeContext.CurrentStore.Id : 0; //_storeContext.CurrentStore.Id;
                pstoreId.DbType = DbType.Int32;

                var planguageId = _dataProvider.GetParameter();
                planguageId.ParameterName = "LanguageId";
                planguageId.Value = _workContext.WorkingLanguage.Id;
                planguageId.DbType = DbType.Int32;

                var pShowProductsFromSubcategories = _dataProvider.GetParameter();
                pShowProductsFromSubcategories.ParameterName = "ShowProductsFromSubcategories";
                pShowProductsFromSubcategories.Value = searchModel.Enabled ? searchModel.IncludeSubCategories : _catalogSettings.ShowProductsFromSubcategories;
                pShowProductsFromSubcategories.DbType = DbType.Boolean;

                var pAllowedCustomerRoleIds = _dataProvider.GetParameter();
                pAllowedCustomerRoleIds.ParameterName = "AllowedCustomerRoleIds";
                pAllowedCustomerRoleIds.Value = commaSeparatedAllowedCustomerRoleIds;
                pAllowedCustomerRoleIds.DbType = DbType.String;

                var pEnablePriceRangeFilter = _dataProvider.GetParameter();
                pEnablePriceRangeFilter.ParameterName = "enablePriceRangeFilter";
                pEnablePriceRangeFilter.Value = speedFiltersSettings.EnablePriceRangeFilter;
                pEnablePriceRangeFilter.DbType = DbType.Boolean;

                var pEnableSpecificationsFilter = _dataProvider.GetParameter();
                pEnableSpecificationsFilter.ParameterName = "enableSpecificationsFilter";
                pEnableSpecificationsFilter.Value = speedFiltersSettings.EnableSpecificationsFilter;
                pEnableSpecificationsFilter.DbType = DbType.Boolean;

                var pEnableAttributesFilter = _dataProvider.GetParameter();
                pEnableAttributesFilter.ParameterName = "enableAttributesFilter";
                pEnableAttributesFilter.Value = speedFiltersSettings.EnableAttributesFilter;
                pEnableAttributesFilter.DbType = DbType.Boolean;

                var pEnableManufacturersFilter = _dataProvider.GetParameter();
                pEnableManufacturersFilter.ParameterName = "enableManufacturersFilter";
                pEnableManufacturersFilter.Value = speedFiltersSettings.EnableManufacturersFilter;
                pEnableManufacturersFilter.DbType = DbType.Boolean;

                var pEnableVendorsFilter = _dataProvider.GetParameter();
                pEnableVendorsFilter.ParameterName = "enableVendorsFilter";
                pEnableVendorsFilter.Value = speedFiltersSettings.EnableVendorsFilter;
                pEnableVendorsFilter.DbType = DbType.Boolean;

                //Search Page
                var pKeywords = _dataProvider.GetParameter();
                pKeywords.ParameterName = "Keywords";
                pKeywords.Value = searchModel.QueryStringForSeacrh != null ? (object)searchModel.QueryStringForSeacrh : DBNull.Value;
                pKeywords.DbType = DbType.String;

                var pSearchDescriptions = _dataProvider.GetParameter();
                pSearchDescriptions.ParameterName = "SearchDescriptions";
                pSearchDescriptions.Value = searchModel.SearchInDescriptions;
                pSearchDescriptions.DbType = DbType.Boolean;

                var pUseFullTextSearch = _dataProvider.GetParameter();
                pUseFullTextSearch.ParameterName = "UseFullTextSearch";
                pUseFullTextSearch.Value = _commonSettings.UseFullTextSearch;
                pUseFullTextSearch.DbType = DbType.Boolean;

                var pFullTextMode = _dataProvider.GetParameter();
                pFullTextMode.ParameterName = "FullTextMode";
                pFullTextMode.Value = (int)_commonSettings.FullTextMode;
                pFullTextMode.DbType = DbType.Int32;

                var pPriceMin = _dataProvider.GetParameter();
                pPriceMin.ParameterName = "PriceMin";
                pPriceMin.Value = searchModel.PriceMin.HasValue ? (object)searchModel.PriceMin.Value : DBNull.Value;
                pPriceMin.DbType = DbType.Decimal;

                var pPriceMax = _dataProvider.GetParameter();
                pPriceMax.ParameterName = "PriceMax";
                pPriceMax.Value = searchModel.PriceMax.HasValue ? (object)searchModel.PriceMax.Value : DBNull.Value;
                pPriceMax.DbType = DbType.Decimal;

                SpeedFilter querySpeedFilter = new SpeedFilter()
                {
                    hasError = false,
                    categoryId = categoryId,
                    manufacturerId = manufacturerId,
                    vendorId = vendorId,
                    speedFiltersSettings = speedFiltersSettings,
                    isSearchPage = searchModel.Enabled
                };

                //invoke stored procedure
                DbContext db = (DbContext)_dbContext;

                bool openingConnection = db.Database.Connection.State == ConnectionState.Closed;
                //var culture=System.Globalization.CultureInfo.InvariantCulture;
                try
                {
                    //System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
                    //System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                    if (openingConnection) { db.Database.Connection.Open(); }
                    //    using(var connectionScope = cmd.Connection.CreateConnectionScope())

                    // Create a SQL command to execute the sproc
                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "FNS_SpeedFilter_GetSpeedFilters";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(pcategoryId);
                    cmd.Parameters.Add(pmanufacturerId);
                    cmd.Parameters.Add(pvendorId);
                    cmd.Parameters.Add(pstoreId);
                    cmd.Parameters.Add(planguageId);
                    cmd.Parameters.Add(pShowProductsFromSubcategories);
                    cmd.Parameters.Add(pAllowedCustomerRoleIds);
                    cmd.Parameters.Add(pEnablePriceRangeFilter);
                    cmd.Parameters.Add(pEnableSpecificationsFilter);
                    cmd.Parameters.Add(pEnableAttributesFilter);
                    cmd.Parameters.Add(pEnableManufacturersFilter);
                    cmd.Parameters.Add(pEnableVendorsFilter);

                    cmd.Parameters.Add(pKeywords);
                    cmd.Parameters.Add(pSearchDescriptions);
                    cmd.Parameters.Add(pUseFullTextSearch);
                    cmd.Parameters.Add(pFullTextMode);
                    cmd.Parameters.Add(pPriceMin);
                    cmd.Parameters.Add(pPriceMax);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (querySpeedFilter.speedFiltersSettings.EnablePriceRangeFilter)
                        {
                            reader.Read();
                            if (reader.HasRows)
                            {
                                /*Convert.ToDecimal(record[prop.Name], CultureInfo.InvariantCulture), null);

                                if (double.TryParse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out number))
                                decimal.TryParse("4.0", NumberStyles.Number, CultureInfo.InvariantCulture, out rating); 

                                querySpeedFilter.PriceMin = Convert.ToInt32(reader["PriceMin"]);
                                querySpeedFilter.PriceMax = Convert.ToInt32(reader["PriceMax"]);

                                це не працює , бо може бути крапка або кома.  
                                querySpeedFilter.PriceMin =(int)reader.GetDecimal(reader.GetOrdinal("PriceMin"));
                                querySpeedFilter.PriceMax =(int)reader.GetDecimal(reader.GetOrdinal("PriceMax"));
                                */

                                querySpeedFilter.PriceMinBase = Convert.ToDecimal(reader["PriceMin"], CultureInfo.InvariantCulture);
                                querySpeedFilter.PriceMaxBase = Convert.ToDecimal(reader["PriceMax"], CultureInfo.InvariantCulture);

                                //querySpeedFilter.PriceMin = (int)Convert.ToDecimal(reader["PriceMin"], CultureInfo.InvariantCulture);
                                //querySpeedFilter.PriceMax = (int)Convert.ToDecimal(reader["PriceMax"], CultureInfo.InvariantCulture);
                                //1.08
                            }
                            reader.NextResult();
                        }
                        if (querySpeedFilter.speedFiltersSettings.EnableSpecificationsFilter)
                        {
                            while (reader.Read())
                            {
                                querySpeedFilter.specificationAttributes.Add(new SpeedFilter.sfSpecificationAttribute()
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"].ToString(),
                                    DisplayOrder = Convert.ToInt32(reader["DisplayOrder"])
                                });
                            }
                            /*
                            // Read SpecificationAttribute from the first result set
                            querySpeedFilter.specificationAttributes = ((IObjectContextAdapter)db)
                                .ObjectContext
                                .Translate<SpeedFilter.sfSpecificationAttribute>(reader, nopDbContext.GetEntitySetName<SpeedFilter.sfSpecificationAttribute>(), System.Data.Entity.Core.Objects.MergeOption.AppendOnly).ToList();
                            */

                            // Move to second result set and read DiscountRequirements
                            reader.NextResult();

                            while (reader.Read())
                            {
                                querySpeedFilter.specificationAttributeOptions.Add(new SpeedFilter.sfSpecificationAttributeOption()
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    SpecificationAttributeId = Convert.ToInt32(reader["SpecificationAttributeId"]),
                                    Name = reader["Name"].ToString(),
                                    DisplayOrder = Convert.ToInt32(reader["DisplayOrder"])
                                });
                            }
                            /*
                            // Read SpecificationAttributeOption from the first result set
                            querySpeedFilter.specificationAttributeOptions = ((IObjectContextAdapter)db)
                                .ObjectContext
                                .Translate<SpeedFilter.sfSpecificationAttributeOption>(reader, nopDbContext.GetEntitySetName<SpeedFilter.sfSpecificationAttributeOption>(), MergeOption.AppendOnly).ToList();
                            */
                            // Move to second result set and read DiscountRequirements
                            reader.NextResult();
                        }
                        if (querySpeedFilter.speedFiltersSettings.EnableAttributesFilter)
                        {
                            while (reader.Read())
                            {
                                querySpeedFilter.productAttributes.Add(new SpeedFilter.sfProductAttribute()
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"].ToString()
                                });
                            }
                            /*
                            // Read SpecificationAttribute from the first result set
                            querySpeedFilter.productAttributes = ((IObjectContextAdapter)db)
                                .ObjectContext
                                .Translate<SpeedFilter.sfProductAttribute>(reader, nopDbContext.GetEntitySetName<SpeedFilter.sfProductAttribute>(), MergeOption.AppendOnly).ToList();
                            */
                            // Move to second result set and read DiscountRequirements
                            reader.NextResult();

                            while (reader.Read())
                            {
                                querySpeedFilter.productAttributesOptions.Add(new SpeedFilter.sfProductAttributeOption()
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    ProductAttributeId = Convert.ToInt32(reader["ProductAttributeId"]),
                                    Name = reader["Name"].ToString()
                                });
                            }
                            /*
                            // Read SpecificationAttribute from the first result set
                            querySpeedFilter.productAttributesOptions = ((IObjectContextAdapter)db)
                                .ObjectContext
                                .Translate<SpeedFilter.sfProductAttributeOption>(reader, nopDbContext.GetEntitySetName<SpeedFilter.sfProductAttributeOption>(), MergeOption.AppendOnly).ToList();
                            */
                            // Move to second result set and read DiscountRequirements
                            reader.NextResult();
                        }
                        if (querySpeedFilter.speedFiltersSettings.EnableManufacturersFilter)
                        {
                            while (reader.Read())
                            {
                                querySpeedFilter.sfManufacturers.Add(new SpeedFilter.sfManufacturer()
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"].ToString(),
                                    DisplayOrder = Convert.ToInt32(reader["DisplayOrder"])
                                });
                            }
                            /*
                                                            // Read sfManufacturers from the first result set
                                                            querySpeedFilter.sfManufacturers = ((IObjectContextAdapter)db)
                                                                .ObjectContext
                                                                .Translate<SpeedFilter.sfManufacturer>(reader, nopDbContext.GetEntitySetName<SpeedFilter.sfManufacturer>(), MergeOption.AppendOnly).ToList();
                            */
                            // Move to second result set and read DiscountRequirements
                            reader.NextResult();
                        }
                        if (querySpeedFilter.speedFiltersSettings.EnableVendorsFilter)
                        {
                            while (reader.Read())
                            {
                                querySpeedFilter.sfVendors.Add(new SpeedFilter.sfVendor()
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"].ToString()
                                });
                            }
                            /*
                            // Read sfVendors from the first result set
                            querySpeedFilter.sfVendors = ((IObjectContextAdapter)db)
                                .ObjectContext
                                .Translate<SpeedFilter.sfVendor>(reader, nopDbContext.GetEntitySetName<SpeedFilter.sfVendor>(), MergeOption.AppendOnly).ToList();
                            */
                            // Move to second result set and read DiscountRequirements
                            //reader.NextResult();
                        }
                    }
                }
                catch (Exception e)
                {
                    LogMessage("-----------------> Error " + e.Message);
                    querySpeedFilter.ErrorMessage = e.Message;
                    querySpeedFilter.hasError = true;
                }
                finally
                {
                    //db.Database.Connection.Close();
                    if (openingConnection && db.Database.Connection.State == ConnectionState.Open) { db.Database.Connection.Close(); }
                    //System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                }

                return querySpeedFilter;

                #endregion
            });

            speedFilter.speedFiltersSettings = speedFiltersSettings;

            speedFilter.DefaultViewMode = _catalogSettings.DefaultViewMode;
            speedFilter.DefaultSortOption = 0;
            speedFilter.DefaultPageSize = 0;

            //pagesize
            if (categoryId != 0)
            {
                var category = _categoryService.GetCategoryById(categoryId);
                if (category != null)
                {
                    if (category.AllowCustomersToSelectPageSize && category.PageSizeOptions != null)
                    {
                        var pageSizes = category.PageSizeOptions.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (pageSizes.Any())
                        {
                            // get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
                            int temp = 0;
                            if (int.TryParse(pageSizes.FirstOrDefault(), out temp))
                            {
                                if (temp > 0)
                                {
                                    speedFilter.DefaultPageSize = temp;
                                }
                            }

                        }
                    }
                    else
                    {
                        //customer is not allowed to select a page size
                        speedFilter.DefaultPageSize = category.PageSize;
                    }
                }
            }
            if (manufacturerId != 0)
            {
                var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
                if (manufacturer != null)
                {
                    if (manufacturer.AllowCustomersToSelectPageSize && manufacturer.PageSizeOptions != null)
                    {
                        var pageSizes = manufacturer.PageSizeOptions.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (pageSizes.Any())
                        {
                            // get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
                            int temp = 0;
                            if (int.TryParse(pageSizes.FirstOrDefault(), out temp))
                            {
                                if (temp > 0)
                                {
                                    speedFilter.DefaultPageSize = temp;
                                }
                            }

                        }
                    }
                    else
                    {
                        //customer is not allowed to select a page size
                        speedFilter.DefaultPageSize = manufacturer.PageSize;
                    }
                }
            }
            if (vendorId != 0)
            {
                var vendor = _vendorService.GetVendorById(vendorId);
                if (vendor.AllowCustomersToSelectPageSize && vendor.PageSizeOptions != null)
                {
                    var pageSizes = vendor.PageSizeOptions.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (pageSizes.Any())
                    {
                        // get the first page size entry to use as the default (manufacturer page load) or if customer enters invalid value via query string

                        int temp = 0;

                        if (int.TryParse(pageSizes.FirstOrDefault(), out temp))
                        {
                            if (temp > 0)
                            {
                                speedFilter.DefaultPageSize = temp;
                            }
                        }
                    }
                }
                else
                {
                    //customer is not allowed to select a page size
                    speedFilter.DefaultPageSize = vendor.PageSize;
                }
            }

            if (speedFilter.hasError)
                _cacheManager.Remove(key);

            if (this.showDebugInfo)
            {
                TimeSpan sp = DateTime.Now - datetime;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("FoxNetSoft :SpeedFiltersService:GetSpeedFilters. duration={0},categoryId={1},manufacturerId={2},vendorId={3}," +
                        "storeId={4},languageId={5}," +
                        "ShowProductsFromSubcategories={6}," +
                        "AllowedCustomerRoleIds={7}," +
                        "enablePriceRangeFilter={8}," +
                        "enableSpecificationsFilter={9}," +
                        "enableAttributesFilter={10}," +
                        "enableManufacturersFilter={11}," +
                        "enableVendorsFilter={12}"
                        ,
                    sp.ToString(), categoryId, manufacturerId, vendorId,
                    _storeContext.CurrentStore.Id,
                    _workContext.WorkingLanguage.Id,
                    _catalogSettings.ShowProductsFromSubcategories,
                    commaSeparatedAllowedCustomerRoleIds,
                    speedFiltersSettings.EnablePriceRangeFilter,
                    speedFiltersSettings.EnableSpecificationsFilter,
                    speedFiltersSettings.EnableAttributesFilter,
                    speedFiltersSettings.EnableManufacturersFilter,
                    speedFiltersSettings.EnableVendorsFilter
                    ));

                if (searchModel.Enabled)
                {
                    sb.AppendLine(string.Format("                SearchPage"));
                    sb.AppendLine(string.Format("                QueryStringForSeacrh={0}", searchModel.QueryStringForSeacrh));
                    sb.AppendLine(string.Format("                AdvancedSearch={0}", searchModel.AdvancedSearch));
                    sb.AppendLine(string.Format("                CategoryId={0}", searchModel.CategoryId));
                    sb.AppendLine(string.Format("                IncludeSubCategories={0}", searchModel.IncludeSubCategories));
                    sb.AppendLine(string.Format("                ManufacturerId={0}", searchModel.ManufacturerId));
                    sb.AppendLine(string.Format("                SearchInDescriptions={0}", searchModel.SearchInDescriptions));
                    sb.AppendLine(string.Format("                PriceMin={0}", searchModel.PriceMin));
                    sb.AppendLine(string.Format("                PriceMax={0}", searchModel.PriceMax));
                    sb.AppendLine(string.Format("                RawUrl={0}", searchModel.RawUrl));
                }

                if (speedFilter.hasError)
                    sb.AppendLine("     Error. " + speedFilter.ErrorMessage);
                if (speedFilter.speedFiltersSettings.EnablePriceRangeFilter)
                    sb.AppendLine(string.Format("      PriceRangeFilter. PriceMin={0},PriceMax={1}", speedFilter.PriceMin, speedFilter.PriceMax));

                if (speedFilter.speedFiltersSettings.EnableSpecificationsFilter)
                {
                    sb.AppendLine("      SpecificationsFilter.");
                    foreach (var specificationAttribute in speedFilter.specificationAttributes)
                    {
                        sb.AppendLine(string.Format("      specificationAttribute => Id={0},Name={1}", specificationAttribute.Id, specificationAttribute.Name));
                        foreach (var specificationOption in
                            speedFilter.specificationAttributeOptions
                                .Where(s => s.SpecificationAttributeId == specificationAttribute.Id)
                                .OrderBy(o => o.DisplayOrder))
                        {
                            sb.AppendLine(string.Format("         option => Id={0},Name={1}", specificationOption.Id, specificationOption.Name));
                        }

                    }
                }

                if (speedFilter.speedFiltersSettings.EnableAttributesFilter)
                {
                    sb.AppendLine("      AttributesFilter.");
                    foreach (var productAttribute in speedFilter.productAttributes)
                    {
                        sb.AppendLine(string.Format("      productAttribute => Id={0},Name={1}", productAttribute.Id, productAttribute.Name));
                        foreach (var productOption in
                            speedFilter.productAttributesOptions
                                .Where(s => s.ProductAttributeId == productAttribute.Id)
                                .OrderBy(o => o.Name))
                        {
                            sb.AppendLine(string.Format("         option => Id={0},Name={1}", productOption.Id, productOption.Name));
                        }

                    }
                }
                if (speedFilter.speedFiltersSettings.EnableManufacturersFilter)
                {
                    sb.AppendLine("      ManufacturersFilter.");
                    foreach (var sfManufacturer in speedFilter.sfManufacturers)
                        sb.AppendLine(string.Format("         Id={0},Name={1}", sfManufacturer.Id, sfManufacturer.Name));
                }
                if (speedFilter.speedFiltersSettings.EnableVendorsFilter)
                {
                    sb.AppendLine("      VendorsFilter.");
                    foreach (var sfvendor in speedFilter.sfVendors)
                        sb.AppendLine(string.Format("         Id={0},Name={1}", sfvendor.Id, sfvendor.Name));
                }

                LogMessage(sb.ToString());
            }

            speedFilter.SubCategories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId:categoryId,showHidden:false,includeAllLevels:false);
            return speedFilter;
        }
        #endregion

        #region Authorize
        /// <summary>
        /// Authorize by Category (Manufacturer, Vendor) identifier
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="allowedCustomerRolesIds">customer Roles Ids</param>
        /// <returns>bool</returns>
        public virtual bool Authorize(int categoryId, int manufacturerId, int vendorId, IList<int> allowedCustomerRolesIds = null)
        {
            //for search page
            if (categoryId == 0 && manufacturerId == 0 && vendorId == 0)
                return true;

            DateTime datetime = DateTime.Now;

            //pass customer role identifiers as comma-delimited string
            string commaSeparatedAllowedCustomerRoleIds = "";
            for (int i = 0; i < allowedCustomerRolesIds.Count; i++)
            {
                commaSeparatedAllowedCustomerRoleIds += allowedCustomerRolesIds[i].ToString();
                if (i != allowedCustomerRolesIds.Count - 1)
                {
                    commaSeparatedAllowedCustomerRoleIds += ",";
                }
            }
            string cacheKey = string.Format(SPEEDFILTER_AUTHORIZE_BYCATEGORYID, categoryId, _storeContext.CurrentStore.Id, commaSeparatedAllowedCustomerRoleIds);
            if (manufacturerId > 0)
                cacheKey = string.Format(SPEEDFILTER_AUTHORIZE_BYMANUFACTURERID, manufacturerId, _storeContext.CurrentStore.Id, commaSeparatedAllowedCustomerRoleIds);
            if (vendorId > 0)
                cacheKey = string.Format(SPEEDFILTER_AUTHORIZE_BYVENDORID, vendorId, _storeContext.CurrentStore.Id, commaSeparatedAllowedCustomerRoleIds);

            
            var isAuthorize = _cacheManager.Get<bool?>(cacheKey);
            //_logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Information, isAuthorize.ToString());
            if (!isAuthorize.HasValue)
            {
                #region store procedure
                //prepare parameters
                var pcategoryId = _dataProvider.GetParameter();
                pcategoryId.ParameterName = "categoryId";
                pcategoryId.Value = categoryId;
                pcategoryId.DbType = DbType.Int32;

                var pmanufacturerId = _dataProvider.GetParameter();
                pmanufacturerId.ParameterName = "manufacturerId";
                pmanufacturerId.Value = manufacturerId;
                pmanufacturerId.DbType = DbType.Int32;

                var pvendorId = _dataProvider.GetParameter();
                pvendorId.ParameterName = "vendorId";
                pvendorId.Value = vendorId;
                pvendorId.DbType = DbType.Int32;

                //Store mapping
                var pstoreId = _dataProvider.GetParameter();
                pstoreId.ParameterName = "storeId";
                pstoreId.Value = !_catalogSettings.IgnoreStoreLimitations ? _storeContext.CurrentStore.Id : 0; //_storeContext.CurrentStore.Id;
                pstoreId.DbType = DbType.Int32;

                var pAllowedCustomerRoleIds = _dataProvider.GetParameter();
                pAllowedCustomerRoleIds.ParameterName = "AllowedCustomerRoleIds";
                pAllowedCustomerRoleIds.Value = commaSeparatedAllowedCustomerRoleIds;
                pAllowedCustomerRoleIds.DbType = DbType.String;


                //invoke stored procedure
                DbContext db = (DbContext)_dbContext;
                bool openingConnection = db.Database.Connection.State == ConnectionState.Closed;
                //bool isAuthorize2 = false;
                isAuthorize = false;
                try
                {
                    if (openingConnection) { db.Database.Connection.Open(); }
                    //    using(var connectionScope = cmd.Connection.CreateConnectionScope())

                    // Create a SQL command to execute the sproc
                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "FNS_SpeedFilter_Authorize_ById";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(pcategoryId);
                    cmd.Parameters.Add(pmanufacturerId);
                    cmd.Parameters.Add(pvendorId);
                    cmd.Parameters.Add(pstoreId);
                    cmd.Parameters.Add(pAllowedCustomerRoleIds);

                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        if (reader.HasRows)
                            isAuthorize = reader.GetBoolean(reader.GetOrdinal("IsAuthorize"));
                        //                            isAuthorize = reader.GetBoolean(0);
                        //isAuthorize = Convert.ToBoolean(reader["IsAuthorize"]);
                        /*{
                            var i = reader.GetOrdinal("IsAuthorize"); // Get the field position
                            isAuthorize = reader.GetBoolean(i);
                        }
                        */
                        //isAuthorize = true;
                    }
                }
                catch (SqlException e)
                {
                    //_logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Information, isAuthorize.ToString());
                    LogMessage("-----------------> SQLError " + e.Message);
                    isAuthorize = false;
                }
                catch (Exception e)
                {
                    //_logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Information, isAuthorize.ToString());
                    LogMessage("-----------------> Error " + e.Message);
                    isAuthorize = false;
                }
                finally
                {
                    //_logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Information, isAuthorize.ToString());
                    //db.Database.Connection.Close();
                    if (openingConnection && db.Database.Connection.State == ConnectionState.Open) { db.Database.Connection.Close(); }
                }

                #endregion

                _cacheManager.Set(cacheKey, isAuthorize, 60);
            }

            if (this.showDebugInfo)
            {
                TimeSpan sp = DateTime.Now - datetime;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("FoxNetSoft :SpeedFiltersService:Authorize. duration={0},categoryId={1},manufacturerId={2},vendorId={3}," +
                        "storeId={4},AllowedCustomerRoleIds={5}, isAuthorize={6}",
                    sp.ToString(), categoryId, manufacturerId, vendorId,
                    _storeContext.CurrentStore.Id,
                    commaSeparatedAllowedCustomerRoleIds, isAuthorize));

                _logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Information, string.Format("FoxNetSoft :SpeedFiltersService:Authorize. duration={0},categoryId={1},manufacturerId={2},vendorId={3}," +
                        "storeId={4},AllowedCustomerRoleIds={5}, isAuthorize={6}",
                    sp.ToString(), categoryId, manufacturerId, vendorId,
                    _storeContext.CurrentStore.Id,
                    commaSeparatedAllowedCustomerRoleIds, isAuthorize));

                LogMessage(sb.ToString());
            }
            return isAuthorize.Value;
        }
        #endregion

        #region Specific Category CRUD

        public void InsertSpecificCategory(SS_Specific_Category_Setting model)
        {
            if(model == null)
                throw new ArgumentNullException("model");

            _specificRepository.Insert(model);
            _eventPublisher.EntityInserted(model);
        }

        public void UpdateSpecificCategory(SS_Specific_Category_Setting model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            _specificRepository.Update(model);
            _eventPublisher.EntityUpdated(model);
        }

        public SS_Specific_Category_Setting GetSpecificCategorySettingById(int id)
        {
            if (id == 0)
                return null;

            return _specificRepository.GetById(id);
        }

        public SS_Specific_Category_Setting GetSpecificCategorySettingByCategoryId(int categoryId)
        {
            if (categoryId == 0)
                return null;

            return _specificRepository.Table.Where(x=>x.CategoryId  == categoryId).FirstOrDefault();
        }

        #endregion

        #endregion


        #region GenerateUrl
        public RetrunUrlResult GenerateSpecificationUrl(string FilterUrl) {
            string sFilterUrl = "";
            RetrunUrlResult result = new RetrunUrlResult();
            if (string.IsNullOrEmpty(FilterUrl))
                return result;

            var psFilter = _dataProvider.GetParameter();
            psFilter.ParameterName = "sFilter";
            psFilter.Value = FilterUrl;
            psFilter.DbType = DbType.String;

            //invoke stored procedure
            DbContext db = (DbContext)_dbContext;
            db.Database.Connection.Open();
            try
            {
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "GenerateFilterUrlFromSeoUrl";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(psFilter);

                using (var reader = cmd.ExecuteReader())
                {
                    var filterData = ((IObjectContextAdapter)db).ObjectContext.Translate<RetrunUrlResult>(reader).FirstOrDefault();
                    //sFilterUrl = filterData.sFilter.ToString();
                    result = filterData;

                }
            }
            catch (Exception ex) {
                sFilterUrl ="";
            }
            finally
            {
                db.Database.Connection.Close();
            }
            return result;
        }
        #endregion
    }

    public class RetrunUrlResult {
        public string sFilter { get; set; }
        public string Name { get; set; }
    }
}
/*

exec [dbo].[FNS_SpeedFilter_ProductLoadAllPaged]
	@CategoryId	=3,	
	@ManufacturerIds  = null,	--a list of manufacturer IDs (comma-separated list) for which a product should be shown
	@VendorIds	= null,	--a list of vendor IDs (comma-separated list) for which a product should be shown
	@StoreId	= 0,
	@LanguageId	= 0,
	@AllowedCustomerRoleIds	='1,2,3',	--a list of customer role IDs (comma-separated list) for which a product should be shown (if a subjet to ACL)
	@ShowProductsFromSubcategories =1, --Get Products From Subcategories
	@IgnoreDiscounts =1, --Ignore Discounts 
	@PageIndex			= 0, 
	@PageSize			= 2147483644,
	@FeaturedProducts	= null,	--0 featured only , 1 not featured only, null - load all products
	@PriceMin			= null,
	@PriceMax			= null,
	@OrderBy			= 0, --0 - position, 5 - Name: A to Z, 6 - Name: Z to A, 10 - Price: Low to High, 11 - Price: High to Low, 15 - creation date
	@showOnSaldo        ='all', -- all - all products, 'zal' Saldo, 'nozal' - nonSaldo
	@FilteredSpecs		= null,	--filter by specifucation attributes (comma-separated list). e.g. 14,15,16
	@FilteredAtrs		 = null,	--filter by product attributes (comma-separated list). e.g. 14,15,16
	@enablePriceRangeFilter =1,  -- enable Price Range Filter
	@enableSpecificationsFilter =1, -- enable Specifications Filter
	@enableAttributesFilter =1, -- enable Attributes Filter
	@enableManufacturersFilter =1, -- enable Manufacturers Filter
	@enableVendorsFilter =1,
    @filtersConditionSpecifications=1,
    @filtersConditionAttributes=1,
    @filtersConditionBetweenBlocks bit=1 --  filters Condition between blocks 1=AND, 0=OR
 
 
 exec [FNS_SpeedFilter_Authorize_ById]
	@CategoryId		= 3,
	@ManufacturerId	 = 0,	--a manufacturer ID for which a product should be shown
	@VendorId		= 0,	--a vendor ID for which a product should be shown
	@StoreId		= 0,
	@AllowedCustomerRoleIds	='1,2,3'
  
  
 */
