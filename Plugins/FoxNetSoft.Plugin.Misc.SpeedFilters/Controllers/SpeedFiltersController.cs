using System;
using System.Linq;
using System.Web.Mvc;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Controllers;
using Nop.Services.Configuration;
using Nop.Web.Models.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Common;
using Nop.Services.Tax;
using Nop.Services.Directory;
using Nop.Core.Domain.Media;
using Nop.Web.Models.Media;
using Nop.Web.Infrastructure.Cache;
using Nop.Services.Vendors;
using Nop.Core.Plugins;
using Nop.Core.Domain.Vendors;
using Nop.Services.Seo;
using Nop.Web.Extensions;
using Nop.Web.Framework.Security;
using Nop.Services.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Services;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Logger;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Domain;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Models;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Themes;
using Nop.Data;
using Nop.Core.Domain.Seo;
using Nop.Services.Filter;
using Nop.Services.Logging;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Controllers
{
    public partial class SpeedFiltersController : BasePublicController
    {
        #region Constants

        private const string THEMES_PATH = "FoxNetSoft.Themes_Path-{0}-{1}";

        #endregion

        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IVendorService _vendorService;

        private readonly ISpeedFiltersService _speedFiltersService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ISettingService _settingService;
        private readonly ICacheManager _cacheManager;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPermissionService _permissionService;

        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IMeasureService _measureService;
        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly ILogger _logger;

        private bool showDebugInfo;
        private readonly FNSLogger _fnsLogger;

        private bool IgnoreDiscountsForCatalog;
        private readonly SpeedFiltersSettings _speedFiltersSettings;

        private bool SkipFirstLoadingForFilters = false;
        private readonly SeoSettings _seoSettings;
        #endregion

        #region Constructors

        public SpeedFiltersController(
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IVendorService vendorService,
            ISpeedFiltersService speedFiltersService,
            IWorkContext workContext, IStoreContext storeContext,
            IPictureService pictureService, ILocalizationService localizationService,
            IWebHelper webHelper,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            ICacheManager cacheManager,
            IPriceFormatter priceFormatter,
            IPermissionService permissionService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPriceCalculationService priceCalculationService,
            ISpecificationAttributeService specificationAttributeService,
            IPluginFinder pluginFinder,
            IMeasureService measureService,
            MediaSettings mediaSettings,
            CatalogSettings catalogSettings,
            VendorSettings vendorSettings,
            ISettingService settingService,
            ILogger logger)
        {
            _logger = logger;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._vendorService = vendorService;


            this._speedFiltersService = speedFiltersService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._settingService = settingService;
            this._cacheManager = cacheManager;
            this._priceFormatter = priceFormatter;
            this._permissionService = permissionService;

            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceCalculationService = priceCalculationService;
            this._specificationAttributeService = specificationAttributeService;
            this._pluginFinder = pluginFinder;
            this._measureService = measureService;

            this._mediaSettings = mediaSettings;
            this._catalogSettings = catalogSettings;
            this._vendorSettings = vendorSettings;

            //треба підняти швидкість!!!!!
            this._speedFiltersSettings = settingService.LoadSetting<SpeedFiltersSettings>(_storeContext.CurrentStore.Id);

            //foxnetsoft
            this.showDebugInfo = _speedFiltersSettings.showDebugInfo;
            this.IgnoreDiscountsForCatalog = _speedFiltersSettings.IgnoreDiscountsForCatalog;

            this._fnsLogger = new FNSLogger(this.showDebugInfo);

            this._speedFiltersService = speedFiltersService;

            VerifySkipFirstLoadingForFilters();
        }

        #endregion

        #region Utilities

        [NonAction]
        private void LogMessage(string message)
        {
            if (this.showDebugInfo)
            {
                this._fnsLogger.LogMessage(message);
            }
        }

        [NonAction]
        private void VerifySkipFirstLoadingForFilters()
        {
            //ProductSortingEnum.Position  = 0
            if (this._speedFiltersSettings.DefaultProductSorting != 0)
            {
                this.SkipFirstLoadingForFilters = true;
                return;
            }
            SkipFirstLoadingForFilters = false;

            if (!this._settingService.GetSettingByKey<bool>("MSSQLProviderSettings.Enable", false))
                return;

            if (this._settingService.GetSettingByKey<bool>("MSSQLProviderSettings.AlwaysLoadProductsByDefault", false))
                return;

            //is plugin installed?
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("FoxNetSoft.Plugin.Misc.MSSQLProvider");
            if (pluginDescriptor == null)
                return;
            if (!pluginDescriptor.Installed)
                return;
            if (!_pluginFinder.AuthenticateStore(pluginDescriptor, _storeContext.CurrentStore.Id))
                return;
            SkipFirstLoadingForFilters = true;
        }

        [NonAction]
        private string FormatPrice(int price)
        {
            /*
                    .00
                    ,00
           */
            //1250.13
            //1250.13
            //$139.900,00
            //
            string strPrice = _priceFormatter.FormatPrice(price).Trim();
            if (strPrice.Length > 3)
                if (strPrice.LastIndexOf('.') == strPrice.Length - 3 || strPrice.LastIndexOf(',') == strPrice.Length - 3)
                    strPrice = strPrice.Substring(0, strPrice.Length - 3);
            return strPrice;
        }

        [NonAction]
        private bool IsIdInSkippedListIds(int id, string stringIds)
        {
            if (String.IsNullOrEmpty(stringIds))
                return false;
            if (id == 0)
                return false;
            //            var ids = new List<int>();
            foreach (var idStr in stringIds
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()))
            {
                int id1 = 0;
                if (int.TryParse(idStr, out id1))
                {
                    //                    ids.Add(id1);
                    if (id1 == id)
                        return true;
                }
            }
            //            ids.ToArray();
            return false;
        }

        [NonAction]
        protected virtual void PrepareSortingOptions(CatalogPagingFilteringModel pagingFilteringModel, SelectedSpeedFilter model)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException("pagingFilteringModel");

            if (model == null)
                throw new ArgumentNullException("model");

            pagingFilteringModel.AllowProductSorting = _catalogSettings.AllowProductSorting;
            if (pagingFilteringModel.AllowProductSorting)
            {
                foreach (ProductSortingEnum enumValue in Enum.GetValues(typeof(ProductSortingEnum)))
                {
                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.ModifyQueryString(currentPageUrl, "orderby=" + ((int)enumValue).ToString(), null);

                    var sortValue = enumValue.GetLocalizedEnum(_localizationService, _workContext);
                    pagingFilteringModel.AvailableSortOptions.Add(new SelectListItem()
                    {
                        Text = sortValue,
                        Value = sortUrl,
                        Selected = enumValue == (ProductSortingEnum)model.OrderBy
                    });
                }
            }
        }

        [NonAction]
        protected virtual void PrepareViewModes(CatalogPagingFilteringModel pagingFilteringModel, SelectedSpeedFilter model)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException("pagingFilteringModel");

            if (model == null)
                throw new ArgumentNullException("model");

            pagingFilteringModel.AllowProductViewModeChanging = _catalogSettings.AllowProductViewModeChanging;

            var viewMode = !string.IsNullOrEmpty(model.ViewMode)
                ? model.ViewMode
                : _catalogSettings.DefaultViewMode;

            pagingFilteringModel.ViewMode = viewMode;
            if (pagingFilteringModel.AllowProductViewModeChanging)
            {
                var currentPageUrl = _webHelper.GetThisPageUrl(true);
                //grid
                pagingFilteringModel.AvailableViewModes.Add(new SelectListItem()
                {
                    Text = _localizationService.GetResource("Catalog.ViewMode.Grid"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode=grid", null),
                    Selected = viewMode == "grid"
                });
                //list
                pagingFilteringModel.AvailableViewModes.Add(new SelectListItem()
                {
                    Text = _localizationService.GetResource("Catalog.ViewMode.List"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode=list", null),
                    Selected = viewMode == "list"
                });
            }
        }

        [NonAction]
        protected virtual void PreparePageSizeOptions(CatalogPagingFilteringModel pagingFilteringModel, SelectedSpeedFilter model,
            bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException("pagingFilteringModel");

            if (model == null)
                throw new ArgumentNullException("model");

            if (model.PageNumber <= 0)
            {
                model.PageNumber = 1;
            }

            //ensure pge size is specified
            if (model.PageSize <= 0)
            {
                model.PageSize = fixedPageSize;
            }

            pagingFilteringModel.AllowCustomersToSelectPageSize = false;
            if (allowCustomersToSelectPageSize && pageSizeOptions != null)
            {
                var pageSizes = pageSizeOptions.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (pageSizes.Any())
                {
                    // get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
                    if (model.PageSize <= 0 || !pageSizes.Contains(model.PageSize.ToString()))
                    {
                        int temp = 0;

                        if (int.TryParse(pageSizes.FirstOrDefault(), out temp))
                        {
                            if (temp > 0)
                            {
                                model.PageSize = temp;
                            }
                        }
                    }

                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.ModifyQueryString(currentPageUrl, "pagesize={0}", null);
                    sortUrl = _webHelper.RemoveQueryString(sortUrl, "pagenumber");

                    foreach (var pageSize in pageSizes)
                    {
                        int temp = 0;
                        if (!int.TryParse(pageSize, out temp))
                        {
                            continue;
                        }
                        if (temp <= 0)
                        {
                            continue;
                        }

                        pagingFilteringModel.PageSizeOptions.Add(new SelectListItem()
                        {
                            Text = pageSize,
                            Value = String.Format(sortUrl, pageSize),
                            Selected = pageSize.Equals(model.PageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        });
                    }

                    if (pagingFilteringModel.PageSizeOptions.Any())
                    {
                        pagingFilteringModel.PageSizeOptions = pagingFilteringModel.PageSizeOptions.OrderBy(x => int.Parse(x.Text)).ToList();
                        pagingFilteringModel.AllowCustomersToSelectPageSize = true;
                    }
                }
            }

            //ensure pge size is specified
            if (model.PageSize <= 0)
            {
                model.PageSize = fixedPageSize;
            }
        }

        [NonAction]
        protected virtual void PrepareViewZal(CatalogPagingFilteringModel pagingFilteringModel, SelectedSpeedFilter model)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException("pagingFilteringModel");

            if (model == null)
                throw new ArgumentNullException("model");
        }

        [NonAction]
        protected string GetViewname(string viewname)
        {
            return GetThemesPath("SpeedFilters/" + viewname + ".cshtml");
        }

        [NonAction]
        protected string GetPluginFolder()
        {
            return "~/Plugins/FoxNetSoft.SpeedFilters/";
        }

        [NonAction]
        private string GetThemesPath(string file)
        {
            //file Catalog/_ProductBox.cshtml
            var currentThemeName = EngineContext.Current.Resolve<IThemeContext>().WorkingThemeName;
            var cashKey = string.Format(THEMES_PATH, currentThemeName, file);
            return _cacheManager.Get(cashKey, () =>
            {
                // nopConfig.ThemeBasePath; ~/Themes/
                //var nopConfig = System.Configuration.ConfigurationManager.GetSection("NopConfig") as NopConfig;
                //var pathtoview = nopConfig.ThemeBasePath + currentThemeName + "/Views/" + file;
                var pathtoview = "~/Themes/" + currentThemeName + "/Views/" + file;
                if (!System.IO.File.Exists(CommonHelper.MapPath(pathtoview)))
                {
                    pathtoview = "~/Views/" + file;
                    if (!System.IO.File.Exists(CommonHelper.MapPath(pathtoview)))
                    {
                        pathtoview = this.GetPluginFolder() + "Themes/" + currentThemeName + "/" + file;
                        if (!System.IO.File.Exists(CommonHelper.MapPath(pathtoview)))
                        {
                            pathtoview = this.GetPluginFolder() + "Views/" + file;
                        }
                    }
                }
                return pathtoview;
            });
        }

        #endregion

        #region Widget

        [ChildActionOnly]
        public ActionResult Content_Widget(string widgetZone)
        {
            LogMessage("SpeedFiltersController. Content_Widget. widgetZone=" + widgetZone);

            //треба підняти швидкість!!!!!
            if (!this._speedFiltersSettings.EnableSpeedFilters)
                return new EmptyResult();

            string controller = "";
            string action = "";
            var routeData = ((System.Web.UI.Page)this.HttpContext.CurrentHandler).RouteData;
            try
            {
                controller = (string)routeData.Values["controller"];
                action = (string)routeData.Values["action"];
            }
            catch
            {
                controller = "";
                action = "";
            }

            this.LogMessage("SpeedFiltersController. Content_Widget. controller=" + controller + ", action=" + action);
            if (string.IsNullOrEmpty(controller) || string.IsNullOrEmpty(action))
                return new EmptyResult();
            /*
            string controller = "";
            if (Request.RequestContext.RouteData.Values.ContainsKey("controller"))
            {
                controller = Request.RequestContext.RouteData.Values["controller"].ToString();
            }
            string action = "";
            if (Request.RequestContext.RouteData.Values.ContainsKey("action"))
            {
                action = Request.RequestContext.RouteData.Values["action"].ToString();
            }

            this.LogMessage("SpeedFiltersController. Content_Widget. controller=" + controller + " action=" + action);

            if (string.IsNullOrEmpty(controller) || string.IsNullOrEmpty(action))
                return new EmptyResult();
            */
            int categoryId = 0;
            int manufacturerId = 0;
            int vendorId = 0;
            var searchModel = new FoxNetSoft.Plugin.Misc.SpeedFilters.Domain.SearchModel();
            string filterSeoUrl = "";
            string generatedUrl = "";
            try
            {
                filterSeoUrl = (string)routeData.Values["FilterUrl"];
            }
            catch
            {
                filterSeoUrl = "";
            }
            string paramUrl = "";
            try
            {
                paramUrl = (string)routeData.Values["paramUrl"];
            }
            catch
            {
                paramUrl = "";
            }

            var sFiltersData = _speedFiltersService.GenerateSpecificationUrl(filterSeoUrl);
            var sFiltersUrl = "";
            if (sFiltersData != null && !string.IsNullOrEmpty(sFiltersData.sFilter))
                sFiltersUrl = sFiltersData.sFilter;
            if (!string.IsNullOrEmpty(sFiltersUrl))
            {
                generatedUrl = sFiltersUrl + paramUrl;
            }



            #region "Catalog-Category"

            if (controller == "Catalog" && action == "Category")
            {
                if (!this._speedFiltersSettings.EnableFiltersForCategory)
                    return new EmptyResult();
                try
                {
                    categoryId = (int)routeData.Values["categoryId"];
                }
                catch
                {
                    categoryId = 0;
                }
                if (categoryId == 0)
                    return new EmptyResult();
                if (!String.IsNullOrWhiteSpace(this._speedFiltersSettings.SkipFiltersforCategories))
                {
                    if (IsIdInSkippedListIds(categoryId, this._speedFiltersSettings.SkipFiltersforCategories))
                        return new EmptyResult();
                }
            }

            #endregion

            #region "Catalog-Manufacturer"

            if (controller == "Catalog" && action == "Manufacturer")
            {
                if (!this._speedFiltersSettings.EnableFiltersForManufacturer)
                    return new EmptyResult();
                //                int manufacturerId = 0;
                try
                {
                    manufacturerId = (int)routeData.Values["manufacturerId"];
                }
                catch
                {
                    manufacturerId = 0;
                }
                if (manufacturerId == 0)
                    return new EmptyResult();
                if (!String.IsNullOrWhiteSpace(this._speedFiltersSettings.SkipFiltersforManufacturers))
                {
                    if (IsIdInSkippedListIds(manufacturerId, this._speedFiltersSettings.SkipFiltersforManufacturers))
                        return new EmptyResult();
                }
            }

            #endregion

            #region "Catalog-Vendor"

            if (controller == "Catalog" && action == "Vendor")
            {
                if (!this._speedFiltersSettings.EnableFiltersForVendor)
                    return new EmptyResult();
                //                int vendorId = 0;
                try
                {
                    vendorId = (int)routeData.Values["vendorId"];
                }
                catch
                {
                    vendorId = 0;
                }
                if (vendorId == 0)
                    return new EmptyResult();
                if (!String.IsNullOrWhiteSpace(this._speedFiltersSettings.SkipFiltersforVendors))
                {
                    if (IsIdInSkippedListIds(vendorId, this._speedFiltersSettings.SkipFiltersforVendors))
                        return new EmptyResult();
                }
            }

            #endregion

            #region "Catalog-Search"

            if (controller == "Catalog" && action == "Search")
            {
                if (!this._speedFiltersSettings.EnableFiltersForSearchPage)
                    return new EmptyResult();
                if (Request.Params["q"] == null)
                    return new EmptyResult();

                var searchTerms = Request.QueryString["q"];
                if (searchTerms == null)
                    searchTerms = "";
                searchTerms = searchTerms.Trim();

                if (searchTerms.Length < _catalogSettings.ProductSearchTermMinimumLength)
                    return new EmptyResult();
                searchModel.Enabled = true;
                searchModel.QueryStringForSeacrh = searchTerms;
                //RawUrl=q=with&adv=true&adv=false&cid=0&isc=false&mid=0&pf=&pt=&sid=true&sid=false

                if (Request.Params["adv"] != null)
                {
                    //logical value has different string values
                    //"true,false" or "false"

                    if (Request.QueryString["adv"] != null &&
                        Request.QueryString["adv"].Equals("true,false", StringComparison.InvariantCultureIgnoreCase))
                        searchModel.AdvancedSearch = true;

                    if (Request.QueryString["isc"] != null &&
                        Request.QueryString["isc"].Equals("true,false", StringComparison.InvariantCultureIgnoreCase))
                        searchModel.IncludeSubCategories = true;

                    if (Request.QueryString["sid"] != null &&
                        Request.QueryString["sid"].Equals("true,false", StringComparison.InvariantCultureIgnoreCase))
                        searchModel.SearchInDescriptions = true;

                    if (Request.Params["cid"] != null)
                    {
                        int cid = 0;
                        if (int.TryParse(Request.Params["cid"], out cid))
                            searchModel.CategoryId = cid;
                    }
                    if (Request.Params["mid"] != null)
                    {
                        int mid = 0;
                        if (int.TryParse(Request.Params["mid"], out mid))
                            searchModel.ManufacturerId = mid;
                    }
                    if (Request.Params["pf"] != null)
                    {
                        decimal pf = 0;
                        if (decimal.TryParse(Request.Params["pf"], out pf))
                            searchModel.PriceMin = pf;
                    }
                    if (Request.Params["pt"] != null)
                    {
                        decimal pt = 0;
                        if (decimal.TryParse(Request.Params["pt"], out pt))
                            searchModel.PriceMax = pt;
                    }
                }
                searchModel.RawUrl = Request.RawUrl;
                if (string.IsNullOrWhiteSpace(searchModel.RawUrl))
                    searchModel.RawUrl = "";
                if (searchModel.RawUrl.IndexOf('?') > 0)
                {
                    var i = searchModel.RawUrl.IndexOf('?');
                    searchModel.RawUrl = searchModel.RawUrl.Substring(i + 1);
                }
            }

            #endregion

            if (categoryId == 0 && manufacturerId == 0 && vendorId == 0 && !searchModel.Enabled)
                return new EmptyResult();

            SpeedFilter model = this._speedFiltersService.GetSpeedFilters(categoryId, manufacturerId, vendorId, searchModel, _speedFiltersSettings);
            if (model == null || model.hasError)
                return new EmptyResult();

            model.PluginPath = this.GetPluginFolder();
            model.SkipFirstLoadingForFilters = this.SkipFirstLoadingForFilters;

            if (model.specificationAttributes.Count == 0 &&
                model.specificationAttributeOptions.Count == 0 &&
                model.productAttributes.Count == 0 &&
                model.productAttributesOptions.Count == 0 &&
                model.sfManufacturers.Count == 0 &&
                model.sfVendors.Count == 0 &&
                (int)model.PriceMinBase == (int)model.PriceMaxBase &&
                model.SkipFirstLoadingForFilters == false)
                return new EmptyResult();

            #region PrepareModel

            if (model.speedFiltersSettings.EnablePriceRangeFilter)
            {
                model.PriceMin = (int)_currencyService.ConvertFromPrimaryStoreCurrency(model.PriceMinBase, _workContext.WorkingCurrency);
                model.PriceMax = (int)_currencyService.ConvertFromPrimaryStoreCurrency(model.PriceMaxBase, _workContext.WorkingCurrency);

                if (model.PriceMin < model.PriceMax)
                {
                    model.PriceMinStr = FormatPrice((int)model.PriceMin);
                    model.PriceMaxStr = FormatPrice((int)model.PriceMax);
                }
                else
                {
                    model.speedFiltersSettings.EnablePriceRangeFilter = false;
                }
            }
            //model.speedFiltersSettings = speedFiltersSettings;

            this.LogMessage(String.Format("SpeedFiltersController. Content_Widget. SkipFirstLoadingForFilters={0}", SkipFirstLoadingForFilters));

            #endregion
            model.generatedUrl = generatedUrl;

            #region catgory
            var category = _categoryService.GetCategoryById(categoryId);
            #endregion

            #region MetaTags
            SpeedFilterSeoModel metaModel = new SpeedFilterSeoModel();
            if (category != null)
            {
                Dictionary<string, string> dictSpecificationAttr = new Dictionary<string, string>();
                if (sFiltersData != null && !string.IsNullOrEmpty(sFiltersData.Name))
                {
                    var mainAttr = sFiltersData.Name.Split(';').ToList();
                    foreach (var item in mainAttr)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            var subAttr = item.Split(':');
                            if (subAttr.Length > 1)
                            {
                                dictSpecificationAttr.Add(subAttr[0], subAttr[1]);
                            }

                        }
                    }
                }
                SelectedSpeedFilter metamodel = new SelectedSpeedFilter(this._catalogSettings, this._speedFiltersSettings, this._fnsLogger);
                SpeedFilterSelectedAttr selectedAttrData = new SpeedFilterSelectedAttr();
                selectedAttrData.CompositeUrl = "";
                selectedAttrData.dictSpecAttr = dictSpecificationAttr;
                metaModel = metamodel.PrepareMetaTags(selectedAttrData, category);
            }
            #endregion
            model.seoModel = metaModel;



            return View(GetViewname("FilterBlocks"), model);
        }

        #endregion

        #region PrepareMSQLProductOverviewModels

        [NonAction]
        protected string GetSEOName(int productId,
            IList<ProductSEOModel> productsSEOSlug)
        {
            if (productId == 0 || productsSEOSlug == null || productsSEOSlug.Count() == 0)
                return "";
            var productSEO = productsSEOSlug.Where(l => l.ProductId == productId).FirstOrDefault();
            if (productSEO != null && !String.IsNullOrWhiteSpace(productSEO.SeName))
                return productSEO.SeName;
            return "";
        }
        [NonAction]
        protected IEnumerable<ProductOverviewModel> PrepareMSQLProductOverviewModels(IEnumerable<Product> products,
            IList<Product> subProducts,
            IList<ProductSEOModel> productsSEOSlug,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null,
            bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false)
        {
            if (products == null)
                throw new ArgumentNullException("products");

            if (this._speedFiltersSettings.prepareSpecificationAttributes)
                prepareSpecificationAttributes = true;

            var models = new List<ProductOverviewModel>();
            foreach (var product in products)
            {
                var model = new ProductOverviewModel()
                {
                    Id = product.Id,
                    //Name = product.GetLocalized(x => x.Name),
                    //ShortDescription = product.GetLocalized(x => x.ShortDescription),
                    //FullDescription = product.GetLocalized(x => x.FullDescription),
                    Name = product.Name,
                    ShortDescription = product.ShortDescription,
                    Sku=product.Sku,
                    OrderMinimumQuantity=product.OrderMinimumQuantity,

                    FullDescription = product.FullDescription,
                    SeName = GetSEOName(product.Id, productsSEOSlug),
                    ProductType = product.ProductType,
                    MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow)

                };
                var appliedAmt = product.AppliedDiscounts.Where(x => x.DiscountType == Nop.Core.Domain.Discounts.DiscountType.AssignedToSkus && x.UsePercentage == false);

                var DiscountRanges = (from a in appliedAmt
                                      select new ProductDetailsModel.DiscountRange
                                      {

                                          Amount = product.Price - a.DiscountAmount,


                                      }).OrderByDescending(x => x.Amount).ToList();

                if(DiscountRanges.Count>0)
                {
                    model.AmountMin = DiscountRanges.Last().Amount;
                    model.AmountMax = DiscountRanges.First().Amount;
                }
                
                //for Michelle Selnick <mselnick@webfeatcomplete.com>
                //model.Sku=product.Sku;

                //price
                if (preparePriceModel)
                {
                    #region Prepare product price

                    var priceModel = new ProductOverviewModel.ProductPriceModel()
                    {
                        ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart
                    };

                    switch (product.ProductType)
                    {
                        case ProductType.GroupedProduct:
                            {
                                #region Grouped product

                                var associatedProducts = subProducts.Where(p => p.ParentGroupedProductId == product.Id).ToList();

                                //add to cart button (ignore "DisableBuyButton" property for grouped products)
                                priceModel.DisableBuyButton = !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart) ||
                                    !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

                                //add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
                                priceModel.DisableWishlistButton = !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) ||
                                    !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

                                //compare products
                                priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

                                switch (associatedProducts.Count)
                                {
                                    case 0:
                                        {
                                            //no associated products
                                            /*priceModel.OldPrice = null;
                                            priceModel.Price = null;
                                            priceModel.DisableBuyButton = true;
                                            priceModel.DisableWishlistButton = true;
                                            priceModel.AvailableForPreOrder = false;
                                            priceModel.DisableAddToCompareListButton = true;*/
                                        }
                                        break;
                                    default:
                                        {
                                            //compare products
                                            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
                                            //priceModel.AvailableForPreOrder = false;


                                            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                                            {
                                                //find a minimum possible price
                                                decimal? minPossiblePrice = null;
                                                Product minPriceProduct = null;
                                                foreach (var associatedProduct in associatedProducts)
                                                {
                                                    //calculate for the maximum quantity (in case if we have tier prices)
                                                    /*var tmpPrice = _priceCalculationService.GetFinalPrice(associatedProduct,
                                                        _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);*/

                                                    decimal tmpPrice = decimal.Zero;

                                                    if (this.IgnoreDiscountsForCatalog || _catalogSettings.IgnoreDiscounts)
                                                        tmpPrice = _priceCalculationService.GetFinalPrice(associatedProduct,
                                                            _workContext.CurrentCustomer, decimal.Zero, false, int.MaxValue);
                                                    else
                                                        tmpPrice = _priceCalculationService.GetFinalPrice(associatedProduct,
                                                            _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);



                                                    if (!minPossiblePrice.HasValue || tmpPrice < minPossiblePrice.Value)
                                                    {
                                                        minPriceProduct = associatedProduct;
                                                        minPossiblePrice = tmpPrice;
                                                    }
                                                }
                                                if (minPriceProduct != null && !minPriceProduct.CustomerEntersPrice)
                                                {
                                                    if (minPriceProduct.CallForPrice)
                                                    {
                                                        priceModel.OldPrice = null;
                                                        priceModel.Price = _localizationService.GetResource("Products.CallForPrice");
                                                    }
                                                    else if (minPossiblePrice.HasValue)
                                                    {
                                                        //calculate prices
                                                        decimal taxRate = decimal.Zero;
                                                        decimal finalPriceBase = _taxService.GetProductPrice(minPriceProduct, minPossiblePrice.Value, out taxRate);
                                                        decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);

                                                        priceModel.OldPrice = null;
                                                        priceModel.Price = String.Format(_localizationService.GetResource("Products.PriceRangeFrom"), _priceFormatter.FormatPrice(finalPrice));
                                                        priceModel.PriceValue = finalPrice;

                                                        //PAngV baseprice (used in Germany)
                                                        priceModel.BasePricePAngV = product.FormatBasePrice(finalPrice,
                                                            _localizationService, _measureService, _currencyService, _workContext, _priceFormatter);

                                                    }
                                                    else
                                                    {
                                                        //Actually it's not possible (we presume that minimalPrice always has a value)
                                                        //We never should get here
                                                        Debug.WriteLine(string.Format("Cannot calculate minPrice for product #{0}", product.Id));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //hide prices
                                                priceModel.OldPrice = null;
                                                priceModel.Price = null;
                                            }
                                        }
                                        break;
                                }

                                #endregion
                            }
                            break;
                        case ProductType.SimpleProduct:
                        default:
                            {
                                #region Simple product

                                //add to cart button
                                priceModel.DisableBuyButton = product.DisableBuyButton ||
                                    !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart) ||
                                    !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

                                //add to wishlist button
                                priceModel.DisableWishlistButton = product.DisableWishlistButton ||
                                    !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) ||
                                    !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

                                //compare products
                                priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

                                //rental
                                priceModel.IsRental = product.IsRental;

                                //pre-order
                                if (product.AvailableForPreOrder)
                                {
                                    priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                        product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
                                    priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
                                }

                                //prices
                                if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                                {
                                    if (!product.CustomerEntersPrice)
                                    {
                                        if (product.CallForPrice)
                                        {
                                            //call for price
                                            priceModel.OldPrice = null;
                                            priceModel.Price = _localizationService.GetResource("Products.CallForPrice");
                                        }
                                        else
                                        {
                                            decimal minPossiblePrice = decimal.Zero;

                                            if (this.IgnoreDiscountsForCatalog || _catalogSettings.IgnoreDiscounts)
                                                minPossiblePrice = _priceCalculationService.GetFinalPrice(product,
                                                    _workContext.CurrentCustomer, decimal.Zero, false, int.MaxValue);
                                            else
                                                minPossiblePrice = _priceCalculationService.GetFinalPrice(product,
                                                    _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);

                                            //calculate prices
                                            decimal taxRate = decimal.Zero;
                                            decimal oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, out taxRate);
                                            decimal finalPriceBase = _taxService.GetProductPrice(product, minPossiblePrice, out taxRate);

                                            decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                                            decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);

                                            //do we have tier prices configured?
                                            var tierPrices = new List<TierPrice>();
                                            if (product.HasTierPrices)
                                            {
                                                tierPrices.AddRange(product.TierPrices
                                                    .OrderBy(tp => tp.Quantity)
                                                    .ToList()
                                                    .FilterByStore(_storeContext.CurrentStore.Id)
                                                    .FilterForCustomer(_workContext.CurrentCustomer)
                                                    .RemoveDuplicatedQuantities());
                                            }
                                            //When there is just one tier (with  qty 1), 
                                            //there are no actual savings in the list.
                                            bool displayFromMessage = tierPrices.Any() &&
                                                !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
                                            if (displayFromMessage)
                                            {
                                                priceModel.OldPrice = null;
                                                priceModel.Price = String.Format(_localizationService.GetResource("Products.PriceRangeFrom"), _priceFormatter.FormatPrice(finalPrice));
                                                priceModel.PriceValue = finalPrice;
                                            }
                                            else
                                            {
                                                if (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero)
                                                {
                                                    priceModel.OldPrice = _priceFormatter.FormatPrice(oldPrice);
                                                    priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
                                                    priceModel.PriceValue = finalPrice;
                                                }
                                                else
                                                {
                                                    priceModel.OldPrice = null;
                                                    priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
                                                    priceModel.PriceValue = finalPrice;
                                                }
                                            }
                                            if (product.IsRental)
                                            {
                                                //rental product
                                                priceModel.OldPrice = _priceFormatter.FormatRentalProductPeriod(product, priceModel.OldPrice);
                                                priceModel.Price = _priceFormatter.FormatRentalProductPeriod(product, priceModel.Price);
                                            }


                                            //property for German market
                                            //we display tax/shipping info only with "shipping enabled" for this product
                                            //we also ensure this it's not free shipping
                                            priceModel.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductBoxes
                                                && product.IsShipEnabled &&
                                                !product.IsFreeShipping;


                                            //PAngV baseprice (used in Germany)
                                            priceModel.BasePricePAngV = product.FormatBasePrice(finalPrice,
                                                _localizationService, _measureService, _currencyService, _workContext, _priceFormatter);

                                        }
                                    }
                                }
                                else
                                {
                                    //hide prices
                                    priceModel.OldPrice = null;
                                    priceModel.Price = null;
                                }

                                #endregion
                            }
                            break;
                    }

                    var applied = product.AppliedDiscounts.OrderByDescending(x => x.MinimumDiscountedQuantity).Take(1).FirstOrDefault();
                    if (applied != null)
                    {
                        priceModel.LowestPrice = priceModel.PriceValue - applied.DiscountAmount;

                        #region custom Sp
                        if (priceModel.LowestPrice < 0)
                        {
                            priceModel.LowestPrice = 0;
                        }
                        priceModel.LowestPriceValue = _priceFormatter.FormatPrice(priceModel.LowestPrice);
                        #endregion

                    }
                    priceModel.DiscountRanges = (from a in product.AppliedDiscounts
                                                 select new ProductDetailsModel.DiscountRange
                                                 {
                                                     Discount = a.Name,
                                                     DiscountID = a.Id,
                                                     Amount = product.Price - a.DiscountAmount,
                                                     MaxMiniQty = a.MaximumDiscountedQuantity,
                                                     MinQty = a.MinimumDiscountedQuantity

                                                 }).OrderByDescending(x => x.Amount).ToList();

                    model.ProductPrice = priceModel;

                    #endregion
                }

                //picture
                if (preparePictureModel)
                {
                    #region Prepare product picture

                    //If a size has been set in the view, we use it in priority
                    int pictureSize = productThumbPictureSize.HasValue ? productThumbPictureSize.Value : _mediaSettings.ProductThumbPictureSize;
                    //prepare picture model
                    var defaultProductPictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DEFAULTPICTURE_MODEL_KEY, product.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    model.DefaultPictureModel = _cacheManager.Get(defaultProductPictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                        var pictureModel = new PictureModel()
                        {
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            /*Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name)*/
                        };
                        //"title" attribute
                        pictureModel.Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute)) ?
                            picture.TitleAttribute :
                            string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name);
                        //"alt" attribute
                        pictureModel.AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute)) ?
                            picture.AltAttribute :
                            string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name);


                        return pictureModel;
                    });

                    #endregion
                }

                //specs
                if (prepareSpecificationAttributes)
                {
                    model.SpecificationAttributeModels = this.PrepareProductSpecificationModel(_workContext,
                         _specificationAttributeService, _cacheManager, product);
                }

                //reviews
                model.ReviewOverviewModel = this.PrepareProductReviewOverviewModel(_storeContext, _catalogSettings, _cacheManager, product);

                models.Add(model);
            }
            return models;
        }

        #endregion

        #region Categories

        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult CategoryPage(int id, int page)
        {
            var category = this._categoryService.GetCategoryById(id);
            if (category == null)
                return InvokeHttp404();

            return RedirectToRoute("Category", new { SeName = category.GetSeName(), pagenumber = page });
        }

        [NopHttpsRequirement(SslRequirement.No)]
        [HttpPost]
        public ActionResult FNSCategory(int page_id, string filterstring, string urlparam)
        {
            DateTime datetime = DateTime.Now;
            LogMessage(String.Format("SpeedFiltersController. FNSCategory. page_id={0}, filterstring={1}, urlparam={2}", page_id, filterstring, urlparam));
            string categorySlug = "";
            string queryStrUrl = "";
            StringBuilder sb = new StringBuilder();
            //sb.AppendLine(String.Format("SpeedFiltersController. FNSCategory. paramId={0},newurl={1}", paramId, newurl));

            //http://mebli.ars.ua/dom-mahkaja-mebel-komplekty#/specFilters=25!#-!83&pageSize=45&viewMode=grid&orderBy=0&pageNumber=1
            //http://demo320.foxnetsoft.com/desktops?pagesize=12&orderby=6&viewmode=list
            //http://mebli.ars.ua/dom-mahkaja-mebel-komplekty?pagesize=12&orderby=6&viewmode=list#/specFilters=25!#-!83&pageSize=45&viewMode=grid&orderBy=0&pageNumber=1

            //_catalogSettings
            SelectedSpeedFilter model = new SelectedSpeedFilter(this._catalogSettings, this._speedFiltersSettings, this._fnsLogger);
            var selectedAttrData = model.GetParameters(filterstring);
            var filterUrl = selectedAttrData.CompositeUrl;
            // sort , order , viewmode url
            var orderby = model.QueryString<string>(urlparam, "orderby");
            if (!String.IsNullOrWhiteSpace(orderby))
            {
                queryStrUrl = string.IsNullOrEmpty(queryStrUrl) ? "orderby=" + orderby : queryStrUrl + "&orderby=" + orderby;
            }
            var ViewMode = model.QueryString<string>(urlparam, "viewmode");
            if (!String.IsNullOrWhiteSpace(ViewMode))
            {
                queryStrUrl = string.IsNullOrEmpty(queryStrUrl) ? "viewmode=" + ViewMode : queryStrUrl + "&viewmode=" + ViewMode;
            }
            if (!String.IsNullOrWhiteSpace(model.QueryString<string>(urlparam, "pagesize")))
            {
                string pagesize = model.QueryString<string>(urlparam, "pagesize");
                queryStrUrl = string.IsNullOrEmpty(queryStrUrl) ? "pagesize=" + pagesize : queryStrUrl + "&pagesize=" + pagesize;
            }
            string pageNumber = "";
            if (!String.IsNullOrWhiteSpace(model.QueryString<string>(urlparam, "pageNumber")))
            {
                pageNumber = model.QueryString<string>(urlparam, "pageNumber");
                queryStrUrl = string.IsNullOrEmpty(queryStrUrl) ? "pageNumber=" + pageNumber : queryStrUrl + "&pageNumber=" + pageNumber;
            }
            if (!String.IsNullOrWhiteSpace(model.QueryString<string>(urlparam, "viewzal")))
            {
                string viewzal = model.QueryString<string>(urlparam, "viewzal");
                queryStrUrl = string.IsNullOrEmpty(queryStrUrl) ? "viewzal=" + viewzal : queryStrUrl + "&viewzal=" + viewzal;
            }

            //
            model.categoryId = page_id;

            int categoryId = page_id;

            #region Save Debug

            if (this.showDebugInfo)
            {
                sb.AppendLine("SpeedFiltersController.FNSCategory. Step 2");
                sb.AppendLine(String.Format("       model.PageNumber={0},model.OrderBy={1},model.ViewMode={2},model.PageSize={3}", model.PageNumber, model.OrderBy, model.ViewMode, model.PageSize));
                sb.AppendLine(String.Format("       model.priceRange.From={0},model.priceRange.To={1}", model.priceRange.From, model.priceRange.To));

                sb.Append("       model.Manufacturers Ids=");
                foreach (var strid in model.manufacturerIds)
                    sb.Append(String.Format("{0},", strid));
                sb.AppendLine("");

                sb.Append("       model.Vendors Ids=");
                foreach (var strid in model.vendorIds)
                    sb.Append(String.Format("{0},", strid));
                sb.AppendLine("");

                sb.AppendLine("       model.specFilters Ids=");
                foreach (var strid in model.specFilterIds)
                    sb.AppendLine(String.Format("                BlockId={0}, Id={1}", strid.BlockId, strid.Id));

                sb.AppendLine("       model.attrFilters Ids=");
                foreach (var strid in model.attrFilterIds)
                    sb.AppendLine(String.Format("                    BlockId={0}, Id={1}", strid.BlockId, strid.Id));
            }
            #endregion

            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                .Where(cr => cr.Active).Select(cr => cr.Id).ToList();

            if (!this._speedFiltersService.Authorize(
                categoryId: categoryId,
                manufacturerId: 0,
                vendorId: 0,
                allowedCustomerRolesIds: customerRolesIds))
            {
                return Json(new
                {
                    success = false,
                    message = _localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.ErrorMessage")
                });
            }

            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null || category.Deleted)
                return InvokeHttp404();
            /*
            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a category before publishing
            if (!category.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(category))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(category))
                return InvokeHttp404();
            */
            /*
             можна зберегти і там де була людина з фільрами 
            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);
            */


            if (model.PageNumber <= 0) model.PageNumber = 1;
            if (model.PageSize <= 0) model.PageSize = category.PageSize;

            //price ranges
            var selectedPriceRange = model.priceRange;
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _workContext.WorkingCurrency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _workContext.WorkingCurrency);
            }

            #region  PagingFilteringContext

            if (this.SkipFirstLoadingForFilters)
            {
                //sorting
                PrepareSortingOptions(model.PagingFilteringContext, model);

                //view mode
                PrepareViewModes(model.PagingFilteringContext, model);

                //page size
                PreparePageSizeOptions(model.PagingFilteringContext, model,
                    category.AllowCustomersToSelectPageSize,
                    category.PageSizeOptions,
                    category.PageSize);

                //view na saldo
                PrepareViewZal(model.PagingFilteringContext, model);
            }
            #endregion

            //дістати швидко список груп через нову процедуру.
            IPagedList<Product> products;
            IList<Product> subProducts;
            IList<ProductSEOModel> productsSEOSlug;
            IList<int> filterableSpecificationAttributeOptionIds = null;
            IList<int> filterableProductAttributeOptionIds = null;
            IList<int> filterableManufacturerIds = null;
            IList<int> filterableVendorIds = null;
            bool hasError = false;

            this._speedFiltersService.GetFilteredProducts(
                out products,
                out subProducts,
                out productsSEOSlug,
                out filterableSpecificationAttributeOptionIds,
                out filterableProductAttributeOptionIds,
                out filterableManufacturerIds,
                out filterableVendorIds,
                out hasError,
                categoryId: model.categoryId,
                manufacturerIds: model.manufacturerIds,
                vendorIds: model.vendorIds,
                storeId: !_catalogSettings.IgnoreStoreLimitations ? _storeContext.CurrentStore.Id : 0,//_storeContext.CurrentStore.Id,
                languageId: _workContext.WorkingLanguage.Id,
                allowedCustomerRolesIds: customerRolesIds,
                ShowProductsFromSubcategories: _catalogSettings.ShowProductsFromSubcategories,
                pageIndex: model.PageNumber - 1, //command.PageNumber - 1,
                pageSize: model.PageSize, //command.PageSize,
                featuredProducts: _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
                priceMin: minPriceConverted, priceMax: maxPriceConverted,
                filteredSpecs: model.specFilterIds,
                filteredAtrs: model.attrFilterIds,
                orderBy: (ProductSortingEnum)model.OrderBy, //command.OrderBy,
                showOnSaldo: model.ShowOnSaldo,
                speedFiltersSettings: _speedFiltersSettings
                );

            //_logger.InsertLog(Nop.Core.Domain.Logging.LogLevel.Information, hasError.ToString());
            model.Products = PrepareMSQLProductOverviewModels(products, subProducts, productsSEOSlug).ToList();

            model.filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
            model.filterableProductAttributeOptionIds = filterableProductAttributeOptionIds;
            model.filterableManufacturerIds = filterableManufacturerIds;
            model.filterableVendorIds = filterableVendorIds;

            //model.PagingFilteringContext.LoadPagedList(products);

            model.pagerModel = new PagerModel()
            {
                PageSize = products.PageSize,
                TotalRecords = products.TotalCount,
                PageIndex = products.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "SpeedFilterCategoryPage",
                UseRouteLinks = true,
                RouteValues = new RouteValues { id = category.Id, page = products.PageIndex }
            };

            if (this.showDebugInfo)
            {
                TimeSpan sp = DateTime.Now - datetime;
                sb.AppendLine();
                sb.AppendLine(String.Format(" duration={0} ", sp.ToString()));
                LogMessage(sb.ToString());
            }
            if (!hasError)
            {
                //display notification message and update appropriate blocks
                var updateproductsectionhtml = this.RenderPartialViewToString(GetViewname("ProductsInGridOrLines"), model);

                var updatepagerhtml = this.RenderPartialViewToString(GetViewname("Pager"), model);

                string updateproductselectorshtml = "";
                if (this.SkipFirstLoadingForFilters && model.Products.Count > 0)
                    updateproductselectorshtml = this.RenderPartialViewToString(GetViewname("ProductSelectorsCategory"), model);

                if (this.showDebugInfo)
                {
                    TimeSpan sp = DateTime.Now - datetime;
                    LogMessage(String.Format("SpeedFiltersController. FNSCategory. total duration={0} ", sp.ToString()));

                    /*LogMessage(updateproductsectionhtml);
                    LogMessage(updatepagerhtml);*/
                }
                SpeedFilterSeoModel metaModel = model.PrepareMetaTags(selectedAttrData, category);
                var mainFilterUrl = filterUrl;

                var specCategorySetting = _speedFiltersService.GetSpecificCategorySettingByCategoryId(category.Id);
               
                string customKeyword = "";
                if (specCategorySetting != null)
                    customKeyword = specCategorySetting.CustomKeyword;
                else
                    customKeyword = _speedFiltersSettings.GlobalCustomKeyword;


                mainFilterUrl = string.IsNullOrEmpty(filterUrl) ? "" : filterUrl + "_" + model.GetSeName(customKeyword, true) + "_" + category.GetSeName();
                filterUrl = string.IsNullOrEmpty(filterUrl) ? "" : filterUrl + "_" + model.GetSeName(customKeyword, true) + "_" + category.GetSeName()
                    + (string.IsNullOrEmpty(queryStrUrl) ? "" : "?" + queryStrUrl);
                categorySlug = category.GetSeName();

                /// bind meta tags

                ///
                var canonicalUrl = "";
                if (string.IsNullOrEmpty(pageNumber) || pageNumber == "1")
                {
                    if(!string.IsNullOrEmpty(customKeyword))
                        canonicalUrl = customKeyword.Replace(" ","-") + "_" + categorySlug;
                }
                else {
                    canonicalUrl = mainFilterUrl;
                }
                return Json(new
                {
                    success = true,
                    updateproductsectionhtml = updateproductsectionhtml,
                    updatepagerhtml = updatepagerhtml,
                    updateproductselectorshtml = updateproductselectorshtml,
                    urlparam = urlparam,
                    filterstring = filterstring,
                    filterableSpecification = model.filterableSpecificationAttributeOptionIds,
                    filterableAttribute = model.filterableProductAttributeOptionIds,
                    filterableManufacturer = model.filterableManufacturerIds,
                    filterableVendor = model.filterableVendorIds,
                    filterUrl = filterUrl,
                    categorySlug = categorySlug,
                    metaModel = metaModel,
                    customKeyword = customKeyword,
                    canonicalUrl = canonicalUrl
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = _localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.ErrorMessage")
                });

            }
        }
        #endregion

        #region Manufacturers

        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult ManufacturerPage(int id, int page)
        {
            var manufacturer = this._manufacturerService.GetManufacturerById(id);
            if (manufacturer == null)
                return InvokeHttp404();

            return RedirectToRoute("Manufacturer", new { SeName = manufacturer.GetSeName(), pagenumber = page });
        }

        [NopHttpsRequirement(SslRequirement.No)]
        [HttpPost]
        public ActionResult FNSManufacturer(int page_id, string filterstring, string urlparam)
        {
            DateTime datetime = DateTime.Now;
            LogMessage(String.Format("SpeedFiltersController. FNSManufacturer. page_id={0}, filterstring={1}, urlparam={2}", page_id, filterstring, urlparam));

            StringBuilder sb = new StringBuilder();
            //sb.AppendLine(String.Format("SpeedFiltersController. FNSCategory. paramId={0},newurl={1}", paramId, newurl));

            //http://mebli.ars.ua/dom-mahkaja-mebel-komplekty#/specFilters=25!#-!83&pageSize=45&viewMode=grid&orderBy=0&pageNumber=1
            //http://demo320.foxnetsoft.com/desktops?pagesize=12&orderby=6&viewmode=list
            //http://mebli.ars.ua/dom-mahkaja-mebel-komplekty?pagesize=12&orderby=6&viewmode=list#/specFilters=25!#-!83&pageSize=45&viewMode=grid&orderBy=0&pageNumber=1

            SelectedSpeedFilter model = new SelectedSpeedFilter(this._catalogSettings, this._speedFiltersSettings, this._fnsLogger);
            model.GetParameters(filterstring);
            model.manufacturerIds.Add(page_id);

            int manufacturerId = page_id;

            #region Save Debug
            if (this.showDebugInfo)
            {
                sb.AppendLine("SpeedFiltersController.FNSManufacturer. Step 2");
                sb.AppendLine(String.Format("       model.PageNumber={0},model.OrderBy={1},model.ViewMode={2},model.PageSize={3}", model.PageNumber, model.OrderBy, model.ViewMode, model.PageSize));
                sb.AppendLine(String.Format("       model.priceRange.From={0},model.priceRange.To={1}", model.priceRange.From, model.priceRange.To));

                sb.Append("       model.Manufacturers Ids=");
                foreach (var strid in model.manufacturerIds)
                    sb.Append(String.Format("{0},", strid));
                sb.AppendLine("");

                sb.Append("       model.Vendors Ids=");
                foreach (var strid in model.vendorIds)
                    sb.Append(String.Format("{0},", strid));
                sb.AppendLine("");

                sb.AppendLine("       model.specFilters Ids=");
                foreach (var strid in model.specFilterIds)
                    sb.AppendLine(String.Format("                BlockId={0}, Id={1}", strid.BlockId, strid.Id));

                sb.AppendLine("       model.attrFilters Ids=");
                foreach (var strid in model.attrFilterIds)
                    sb.AppendLine(String.Format("                    BlockId={0}, Id={1}", strid.BlockId, strid.Id));
            }
            #endregion


            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                .Where(cr => cr.Active).Select(cr => cr.Id).ToList();

            if (!this._speedFiltersService.Authorize(
                categoryId: 0,
                manufacturerId: manufacturerId,
                vendorId: 0,
                allowedCustomerRolesIds: customerRolesIds))
            {
                return Json(new
                {
                    success = false,
                    message = _localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.ErrorMessage")
                });
            }

            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null || manufacturer.Deleted)
                return InvokeHttp404();

            if (model.PageNumber <= 0) model.PageNumber = 1;
            if (model.PageSize <= 0) model.PageSize = manufacturer.PageSize;

            //price ranges
            var selectedPriceRange = model.priceRange;
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _workContext.WorkingCurrency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _workContext.WorkingCurrency);
            }

            #region  PagingFilteringContext
            if (this.SkipFirstLoadingForFilters)
            {
                //sorting
                PrepareSortingOptions(model.PagingFilteringContext, model);

                //view mode
                PrepareViewModes(model.PagingFilteringContext, model);

                //page size
                PreparePageSizeOptions(model.PagingFilteringContext, model,
                    manufacturer.AllowCustomersToSelectPageSize,
                    manufacturer.PageSizeOptions,
                    manufacturer.PageSize);
            }
            #endregion

            //дістати швидко список груп через нову процедуру.
            IPagedList<Product> products;
            IList<Product> subProducts;
            IList<ProductSEOModel> productsSEOSlug;
            IList<int> filterableSpecificationAttributeOptionIds = null;
            IList<int> filterableProductAttributeOptionIds = null;
            IList<int> filterableManufacturerIds = null;
            IList<int> filterableVendorIds = null;
            bool hasError = false;

            this._speedFiltersService.GetFilteredProducts(
                out products,
                out subProducts,
                out productsSEOSlug,
                out filterableSpecificationAttributeOptionIds,
                out filterableProductAttributeOptionIds,
                out filterableManufacturerIds,
                out filterableVendorIds,
                out hasError,
                categoryId: model.categoryId,
                manufacturerIds: model.manufacturerIds,
                vendorIds: model.vendorIds,
                storeId: !_catalogSettings.IgnoreStoreLimitations ? _storeContext.CurrentStore.Id : 0,// _storeContext.CurrentStore.Id,
                languageId: _workContext.WorkingLanguage.Id,
                allowedCustomerRolesIds: customerRolesIds,
                ShowProductsFromSubcategories: _catalogSettings.ShowProductsFromSubcategories,
                pageIndex: model.PageNumber - 1, //command.PageNumber - 1,
                pageSize: model.PageSize, //command.PageSize,
                featuredProducts: _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
                priceMin: minPriceConverted, priceMax: maxPriceConverted,
                filteredSpecs: model.specFilterIds,
                filteredAtrs: model.attrFilterIds,
                orderBy: (ProductSortingEnum)model.OrderBy, //command.OrderBy,
                showOnSaldo: model.ShowOnSaldo,
                speedFiltersSettings: _speedFiltersSettings
                );

            model.Products = PrepareMSQLProductOverviewModels(products, subProducts, productsSEOSlug).ToList();

            model.filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
            model.filterableProductAttributeOptionIds = filterableProductAttributeOptionIds;
            model.filterableManufacturerIds = filterableManufacturerIds;
            model.filterableVendorIds = filterableVendorIds;

            model.pagerModel = new PagerModel()
            {
                PageSize = products.PageSize,
                TotalRecords = products.TotalCount,
                PageIndex = products.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "SpeedFilterManufacturerPage",
                UseRouteLinks = true,
                RouteValues = new RouteValues { id = manufacturer.Id, page = products.PageIndex }
            };

            if (this.showDebugInfo)
            {
                TimeSpan sp = DateTime.Now - datetime;
                sb.AppendLine();
                sb.AppendLine(String.Format(" duration={0} ", sp.ToString()));

                LogMessage(sb.ToString());
            }
            if (!hasError)
            {
                //display notification message and update appropriate blocks
                var updateproductsectionhtml = this.RenderPartialViewToString(GetViewname("ProductsInGridOrLines"), model);
                var updatepagerhtml = this.RenderPartialViewToString(GetViewname("Pager"), model);

                string updateproductselectorshtml = "";
                if (this.SkipFirstLoadingForFilters && model.Products.Count > 0)
                    updateproductselectorshtml = this.RenderPartialViewToString(GetViewname("ProductSelectorsManufacturer"), model);

                if (this.showDebugInfo)
                {
                    TimeSpan sp = DateTime.Now - datetime;
                    LogMessage(String.Format("SpeedFiltersController. FNSManufacturer. total duration={0} ", sp.ToString()));

                    /*   LogMessage(updateproductsectionhtml);
                       LogMessage(updatepagerhtml);*/
                }
                return Json(new
                {
                    success = true,
                    updateproductsectionhtml = updateproductsectionhtml,
                    updatepagerhtml = updatepagerhtml,
                    updateproductselectorshtml = updateproductselectorshtml,
                    urlparam = urlparam,
                    filterstring = filterstring,
                    filterableSpecification = model.filterableSpecificationAttributeOptionIds,
                    filterableAttribute = model.filterableProductAttributeOptionIds,
                    filterableManufacturer = model.filterableManufacturerIds,
                    filterableVendor = model.filterableVendorIds
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = _localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.ErrorMessage")
                });

            }
        }
        #endregion

        #region Vendors

        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult VendorPage(int id, int page)
        {
            var vendor = this._vendorService.GetVendorById(id);
            if (vendor == null)
                return InvokeHttp404();

            return RedirectToRoute("Vendor", new { SeName = vendor.GetSeName(), pagenumber = page });
        }

        [NopHttpsRequirement(SslRequirement.No)]
        [HttpPost]
        public ActionResult FNSVendor(int page_id, string filterstring, string urlparam)
        {
            DateTime datetime = DateTime.Now;
            LogMessage(String.Format("SpeedFiltersController. FNSVendor. page_id={0}, filterstring={1}, urlparam={2}", page_id, filterstring, urlparam));

            StringBuilder sb = new StringBuilder();
            //sb.AppendLine(String.Format("SpeedFiltersController. FNSCategory. paramId={0},newurl={1}", paramId, newurl));

            //http://mebli.ars.ua/dom-mahkaja-mebel-komplekty#/specFilters=25!#-!83&pageSize=45&viewMode=grid&orderBy=0&pageNumber=1
            //http://demo320.foxnetsoft.com/desktops?pagesize=12&orderby=6&viewmode=list
            //http://mebli.ars.ua/dom-mahkaja-mebel-komplekty?pagesize=12&orderby=6&viewmode=list#/specFilters=25!#-!83&pageSize=45&viewMode=grid&orderBy=0&pageNumber=1

            SelectedSpeedFilter model = new SelectedSpeedFilter(this._catalogSettings, this._speedFiltersSettings, this._fnsLogger);
            model.GetParameters(filterstring);
            model.vendorIds.Add(page_id);

            int vendorId = page_id;

            #region Save Debug
            if (this.showDebugInfo)
            {
                sb.AppendLine("SpeedFiltersController.FNSVendor. Step 2");
                sb.AppendLine(String.Format("       model.PageNumber={0},model.OrderBy={1},model.ViewMode={2},model.PageSize={3}", model.PageNumber, model.OrderBy, model.ViewMode, model.PageSize));
                sb.AppendLine(String.Format("       model.priceRange.From={0},model.priceRange.To={1}", model.priceRange.From, model.priceRange.To));

                sb.Append("       model.Manufacturers Ids=");
                foreach (var strid in model.manufacturerIds)
                    sb.Append(String.Format("{0},", strid));
                sb.AppendLine("");

                sb.Append("       model.Vendors Ids=");
                foreach (var strid in model.vendorIds)
                    sb.Append(String.Format("{0},", strid));
                sb.AppendLine("");

                sb.AppendLine("       model.specFilters Ids=");
                foreach (var strid in model.specFilterIds)
                    sb.AppendLine(String.Format("                BlockId={0}, Id={1}", strid.BlockId, strid.Id));

                sb.AppendLine("       model.attrFilters Ids=");
                foreach (var strid in model.attrFilterIds)
                    sb.AppendLine(String.Format("                    BlockId={0}, Id={1}", strid.BlockId, strid.Id));
            }
            #endregion


            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                .Where(cr => cr.Active).Select(cr => cr.Id).ToList();

            if (!this._speedFiltersService.Authorize(
                categoryId: 0,
                manufacturerId: 0,
                vendorId: vendorId,
                allowedCustomerRolesIds: customerRolesIds))
            {
                return Json(new
                {
                    success = false,
                    message = _localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.ErrorMessage")
                });
            }

            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null || vendor.Deleted)
                return InvokeHttp404();

            if (model.PageNumber <= 0) model.PageNumber = 1;
            if (model.PageSize <= 0) model.PageSize = vendor.PageSize;

            //price ranges
            var selectedPriceRange = model.priceRange;
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _workContext.WorkingCurrency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _workContext.WorkingCurrency);
            }

            #region  PagingFilteringContext
            if (this.SkipFirstLoadingForFilters)
            {
                //sorting
                PrepareSortingOptions(model.PagingFilteringContext, model);

                //view mode
                PrepareViewModes(model.PagingFilteringContext, model);

                //page size
                PreparePageSizeOptions(model.PagingFilteringContext, model,
                    vendor.AllowCustomersToSelectPageSize,
                    vendor.PageSizeOptions,
                    vendor.PageSize);
            }
            #endregion

            //дістати швидко список груп через нову процедуру.
            IPagedList<Product> products;
            IList<Product> subProducts;
            IList<ProductSEOModel> productsSEOSlug;
            IList<int> filterableSpecificationAttributeOptionIds = null;
            IList<int> filterableProductAttributeOptionIds = null;
            IList<int> filterableManufacturerIds = null;
            IList<int> filterableVendorIds = null;
            bool hasError = false;

            this._speedFiltersService.GetFilteredProducts(
                out products,
                out subProducts,
                out productsSEOSlug,
                out filterableSpecificationAttributeOptionIds,
                out filterableProductAttributeOptionIds,
                out filterableManufacturerIds,
                out filterableVendorIds,
                out hasError,
                categoryId: model.categoryId,
                manufacturerIds: model.manufacturerIds,
                vendorIds: model.vendorIds,
                storeId: !_catalogSettings.IgnoreStoreLimitations ? _storeContext.CurrentStore.Id : 0,// _storeContext.CurrentStore.Id,
                languageId: _workContext.WorkingLanguage.Id,
                allowedCustomerRolesIds: customerRolesIds,
                ShowProductsFromSubcategories: _catalogSettings.ShowProductsFromSubcategories,
                pageIndex: model.PageNumber - 1, //command.PageNumber - 1,
                pageSize: model.PageSize, //command.PageSize,
                featuredProducts: _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
                priceMin: minPriceConverted, priceMax: maxPriceConverted,
                filteredSpecs: model.specFilterIds,
                filteredAtrs: model.attrFilterIds,
                orderBy: (ProductSortingEnum)model.OrderBy, //command.OrderBy,
                showOnSaldo: model.ShowOnSaldo,
                speedFiltersSettings: _speedFiltersSettings
                );

            model.Products = PrepareMSQLProductOverviewModels(products, subProducts, productsSEOSlug).ToList();

            model.filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
            model.filterableProductAttributeOptionIds = filterableProductAttributeOptionIds;
            model.filterableManufacturerIds = filterableManufacturerIds;
            model.filterableVendorIds = filterableVendorIds;

            model.pagerModel = new PagerModel()
            {
                PageSize = products.PageSize,
                TotalRecords = products.TotalCount,
                PageIndex = products.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "SpeedFilterVendorPage",
                UseRouteLinks = true,
                RouteValues = new RouteValues { id = vendor.Id, page = products.PageIndex }
            };

            if (this.showDebugInfo)
            {
                TimeSpan sp = DateTime.Now - datetime;
                sb.AppendLine();
                sb.AppendLine(String.Format(" duration={0} ", sp.ToString()));

                LogMessage(sb.ToString());
            }
            if (!hasError)
            {
                //display notification message and update appropriate blocks
                var updateproductsectionhtml = this.RenderPartialViewToString(GetViewname("ProductsInGridOrLines"), model);
                var updatepagerhtml = this.RenderPartialViewToString(GetViewname("Pager"), model);

                string updateproductselectorshtml = "";
                if (this.SkipFirstLoadingForFilters && model.Products.Count > 0)
                    updateproductselectorshtml = this.RenderPartialViewToString(GetViewname("ProductSelectorsVendor"), model);

                if (this.showDebugInfo)
                {
                    TimeSpan sp = DateTime.Now - datetime;
                    LogMessage(String.Format("SpeedFiltersController. FNSVendor. total duration={0} ", sp.ToString()));

                    /*LogMessage(updateproductsectionhtml);
                    LogMessage(updatepagerhtml);*/
                }
                return Json(new
                {
                    success = true,
                    updateproductsectionhtml = updateproductsectionhtml,
                    updatepagerhtml = updatepagerhtml,
                    updateproductselectorshtml = updateproductselectorshtml,
                    urlparam = urlparam,
                    filterstring = filterstring,
                    filterableSpecification = model.filterableSpecificationAttributeOptionIds,
                    filterableAttribute = model.filterableProductAttributeOptionIds,
                    filterableManufacturer = model.filterableManufacturerIds,
                    filterableVendor = model.filterableVendorIds
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = _localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.ErrorMessage")
                });

            }
        }
        #endregion

        #region Searching

        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult SearchPage()
        {
            return RedirectToAction("Search", "Catalog");
        }

        [NopHttpsRequirement(SslRequirement.No)]
        [HttpPost]
        public ActionResult FNSSearch(int page_id, string filterstring, string urlparam)
        {
            DateTime datetime = DateTime.Now;
            LogMessage(String.Format("SpeedFiltersController. FNSSearch. page_id={0}, filterstring={1}, urlparam={2}", page_id, filterstring, urlparam));

            StringBuilder sb = new StringBuilder();

            //http://mebli.ars.ua/dom-mahkaja-mebel-komplekty#/specFilters=25!#-!83&pageSize=45&viewMode=grid&orderBy=0&pageNumber=1
            //http://demo320.foxnetsoft.com/desktops?pagesize=12&orderby=6&viewmode=list
            //http://mebli.ars.ua/dom-mahkaja-mebel-komplekty?pagesize=12&orderby=6&viewmode=list#/specFilters=25!#-!83&pageSize=45&viewMode=grid&orderBy=0&pageNumber=1

            //_catalogSettings
            SelectedSpeedFilter model = new SelectedSpeedFilter(this._catalogSettings, this._speedFiltersSettings, this._fnsLogger);
            model.GetParameters(filterstring);

            #region Save Debug

            if (this.showDebugInfo)
            {
                sb.AppendLine("SpeedFiltersController.FNSSearch. Step 2");
                sb.AppendLine(String.Format("       model.PageNumber={0},model.OrderBy={1},model.ViewMode={2},model.PageSize={3}", model.PageNumber, model.OrderBy, model.ViewMode, model.PageSize));
                sb.AppendLine(String.Format("       model.priceRange.From={0},model.priceRange.To={1}", model.priceRange.From, model.priceRange.To));

                sb.Append("       model.Manufacturers Ids=");
                foreach (var strid in model.manufacturerIds)
                    sb.Append(String.Format("{0},", strid));
                sb.AppendLine("");

                sb.Append("       model.Vendors Ids=");
                foreach (var strid in model.vendorIds)
                    sb.Append(String.Format("{0},", strid));
                sb.AppendLine("");

                sb.AppendLine("       model.specFilters Ids=");
                foreach (var strid in model.specFilterIds)
                    sb.AppendLine(String.Format("                BlockId={0}, Id={1}", strid.BlockId, strid.Id));

                sb.AppendLine("       model.attrFilters Ids=");
                foreach (var strid in model.attrFilterIds)
                    sb.AppendLine(String.Format("                    BlockId={0}, Id={1}", strid.BlockId, strid.Id));

                if (model.searchModel.Enabled)
                {
                    sb.AppendLine("       SearchPage");
                    sb.AppendLine(String.Format("       model.searchModel.QueryStringForSeacrh={0}", model.searchModel.QueryStringForSeacrh));
                    sb.AppendLine(String.Format("       model.searchModel.AdvancedSearch={0}", model.searchModel.AdvancedSearch));
                    sb.AppendLine(String.Format("       model.searchModel.IncludeSubCategories={0}", model.searchModel.IncludeSubCategories));
                    sb.AppendLine(String.Format("       model.searchModel.SearchInDescriptions={0}", model.searchModel.SearchInDescriptions));
                }
            }
            #endregion

            if (model.searchModel.QueryStringForSeacrh.Length < _catalogSettings.ProductSearchTermMinimumLength)
            {
                return Json(new
                {
                    success = false,
                    message = string.Format(_localizationService.GetResource("Search.SearchTermMinimumLengthIsNCharacters"), _catalogSettings.ProductSearchTermMinimumLength)
                });
            }
            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                .Where(cr => cr.Active).Select(cr => cr.Id).ToList();

            if (model.PageNumber <= 0) model.PageNumber = 1;
            if (model.PageSize <= 0) model.PageSize = _catalogSettings.SearchPageProductsPerPage;

            //price ranges
            var selectedPriceRange = model.priceRange;
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _workContext.WorkingCurrency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _workContext.WorkingCurrency);
            }

            #region  PagingFilteringContext

            if (this.SkipFirstLoadingForFilters)
            {
                //sorting
                PrepareSortingOptions(model.PagingFilteringContext, model);

                //view mode
                PrepareViewModes(model.PagingFilteringContext, model);

                //page size
                PreparePageSizeOptions(model.PagingFilteringContext, model,
                    _catalogSettings.SearchPageAllowCustomersToSelectPageSize,
                    _catalogSettings.SearchPagePageSizeOptions,
                    _catalogSettings.SearchPageProductsPerPage);

                //view na saldo
                PrepareViewZal(model.PagingFilteringContext, model);
            }
            #endregion

            //дістати швидко список груп через нову процедуру.
            IPagedList<Product> products;
            IList<Product> subProducts;
            IList<ProductSEOModel> productsSEOSlug;
            IList<int> filterableSpecificationAttributeOptionIds = null;
            IList<int> filterableProductAttributeOptionIds = null;
            IList<int> filterableManufacturerIds = null;
            IList<int> filterableVendorIds = null;
            bool hasError = false;

            this._speedFiltersService.GetFilteredProducts(
                out products,
                out subProducts,
                out productsSEOSlug,
                out filterableSpecificationAttributeOptionIds,
                out filterableProductAttributeOptionIds,
                out filterableManufacturerIds,
                out filterableVendorIds,
                out hasError,
                categoryId: model.categoryId,
                manufacturerIds: model.manufacturerIds,
                vendorIds: model.vendorIds,
                storeId: !_catalogSettings.IgnoreStoreLimitations ? _storeContext.CurrentStore.Id : 0,//_storeContext.CurrentStore.Id ,
                languageId: _workContext.WorkingLanguage.Id,
                allowedCustomerRolesIds: customerRolesIds,
                ShowProductsFromSubcategories: (model.searchModel.Enabled && model.searchModel.AdvancedSearch)
                    ? model.searchModel.IncludeSubCategories : _catalogSettings.ShowProductsFromSubcategories,
                pageIndex: model.PageNumber - 1, //command.PageNumber - 1,
                pageSize: model.PageSize, //command.PageSize,
                featuredProducts: _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
                keywords: model.searchModel.Enabled ? model.searchModel.QueryStringForSeacrh : null,
                searchDescriptions: model.searchModel.Enabled ? model.searchModel.SearchInDescriptions : false,
                priceMin: minPriceConverted, priceMax: maxPriceConverted,
                filteredSpecs: model.specFilterIds,
                filteredAtrs: model.attrFilterIds,
                orderBy: (ProductSortingEnum)model.OrderBy, //command.OrderBy,
                showOnSaldo: model.ShowOnSaldo,
                speedFiltersSettings: _speedFiltersSettings
                );

            model.Products = PrepareMSQLProductOverviewModels(products, subProducts, productsSEOSlug).ToList();

            model.filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
            model.filterableProductAttributeOptionIds = filterableProductAttributeOptionIds;
            model.filterableManufacturerIds = filterableManufacturerIds;
            model.filterableVendorIds = filterableVendorIds;

            //model.PagingFilteringContext.LoadPagedList(products);

            model.pagerModel = new PagerModel()
            {
                PageSize = products.PageSize,
                TotalRecords = products.TotalCount,
                PageIndex = products.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "SpeedFilterSearchPage",
                UseRouteLinks = true,
                RouteValues = new RouteValues { page = products.PageIndex }
            };

            if (this.showDebugInfo)
            {
                TimeSpan sp = DateTime.Now - datetime;
                sb.AppendLine();
                sb.AppendLine(String.Format(" duration={0} ", sp.ToString()));
                LogMessage(sb.ToString());
            }
            if (!hasError)
            {
                //display notification message and update appropriate blocks
                var updateproductsectionhtml = this.RenderPartialViewToString(GetViewname("ProductsInGridOrLines"), model);

                var updatepagerhtml = this.RenderPartialViewToString(GetViewname("Pager"), model);

                string updateproductselectorshtml = "";
                if (this.SkipFirstLoadingForFilters && model.Products.Count > 0)
                    updateproductselectorshtml = this.RenderPartialViewToString(GetViewname("ProductSelectorsCategory"), model);

                if (this.showDebugInfo)
                {
                    TimeSpan sp = DateTime.Now - datetime;
                    LogMessage(String.Format("SpeedFiltersController. FNSSearch. total duration={0} ", sp.ToString()));

                    /*LogMessage(updateproductsectionhtml);
                    LogMessage(updatepagerhtml);*/
                }
                return Json(new
                {
                    success = true,
                    updateproductsectionhtml = updateproductsectionhtml,
                    updatepagerhtml = updatepagerhtml,
                    updateproductselectorshtml = updateproductselectorshtml,
                    urlparam = urlparam,
                    filterstring = filterstring,
                    filterableSpecification = model.filterableSpecificationAttributeOptionIds,
                    filterableAttribute = model.filterableProductAttributeOptionIds,
                    filterableManufacturer = model.filterableManufacturerIds,
                    filterableVendor = model.filterableVendorIds
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = _localizationService.GetResource("FoxNetSoft.Plugin.Misc.SpeedFilters.ErrorMessage")
                });

            }
        }

        #endregion
    }
}

//http://demo320.foxnetsoft.com/notebooks#/specFilters=2!#-!5!##!6!-#!3m!#-!7
//http://demo320.foxnetsoft.com/notebooks#/manFilters=1!##!2&prFilter=From-1415
//http://demo320.foxnetsoft.com/notebooks#/specFilters=2!#-!5!##!6!-#!3!#-!7&manFilters=1&prFilter=From-1501
//http://demo320.foxnetsoft.com/notebooks#/prFilter=From-1433!-#!To-2503