using System.Linq;
using System.Collections.Generic;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Core.Domain.Cms;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Cms;
using Nop.Web.Framework.Menu;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Logger;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Data;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Services;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Models;
using Nop.Core.Domain.Catalog;
using Nop.Services.Filter;
using System;
using Nop.Core.Data;
using Nop.Data;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Domain;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters
{
    public partial class SpeedFiltersPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin, IAdminMenuPlugin, IFilterMethods
    {
        #region Constants

        //SpeedFilters

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store Id
        /// </remarks>
        private const string SPEEDFILTERS_BY_STORE_ID_KEY = "FoxNetSoft.SpeedFilters.storeid-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string SPEEDFILTERS_PATTERN_KEY = "FoxNetSoft.SpeedFilters.";

        #endregion

        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ICacheManager _cacheManager;
        private readonly ISpeedFiltersService _speedFiltersService;
        private readonly CatalogSettings _catalogSettings;
        private readonly SpeedFiltersSettings _speedFiltersSettings;
        private readonly FNSLogger _fnsLogger;
        private bool showDebugInfo;
        #endregion

        #region Ctor
        public SpeedFiltersPlugin(
            IStoreContext storeContext,
            ISpeedFiltersService speedFiltersService,
            CatalogSettings catalogSettings
            , SpeedFiltersSettings speedFiltersSettings
            )
        {
            this._storeContext = storeContext;

            //TODO inject static cache manager using constructor
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
            this._speedFiltersService = speedFiltersService;
            this._catalogSettings = catalogSettings;
            this._speedFiltersSettings = speedFiltersSettings;
            this.showDebugInfo = _speedFiltersSettings.showDebugInfo;
            this._fnsLogger = new FNSLogger(this.showDebugInfo);
        }
        #endregion

        #region Utils
        /*public void LogMessage(string message)
        {
            if (this.showDebugInfo)
            {
                this._fnsLogger.LogMessage(message);
            }
        }
        */
        #endregion

        #region Implementation of IMiscPlugin
        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "SpeedFiltersSettins";
            routeValues = new RouteValueDictionary { { "Namespaces", "FoxNetSoft.Plugin.Misc.SpeedFilters.Controllers" }, { "area", null } };
        }
        #endregion

        #region Implementation of IWidgetPlugin

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            string key = string.Format(SPEEDFILTERS_BY_STORE_ID_KEY, _storeContext.CurrentStore.Id);
            //ICacheManager _cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
            return _cacheManager.Get(key, () =>
            {
                var settingService = EngineContext.Current.ContainerManager.Resolve<ISettingService>();
                var speedFiltersSettings = settingService.LoadSetting<SpeedFiltersSettings>(_storeContext.CurrentStore.Id);

                IList<string> widgetZones = new List<string>();

                if (speedFiltersSettings.EnableSpeedFilters)
                {
                    var widgetZone = speedFiltersSettings.WidgetZone;
                    if (!string.IsNullOrWhiteSpace(widgetZone))
                        widgetZones.Add(widgetZone);
                }
                return widgetZones;
            });
        }

        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
/*            if (this.enablePlugin)
            {*/
                actionName = "Content_Widget";
                controllerName = "SpeedFilters";
                routeValues = new RouteValueDictionary()
                                    {
                                        {"Namespaces", "FoxNetSoft.Plugin.Misc.SpeedFilters.Controllers"},
                                        {"area", null},
                                        {"widgetZone", widgetZone}
                                    };
/*            }
            else
            {
                actionName = null;
                controllerName = null;
                routeValues = null;
            }*/
        }
        #endregion

        #region Implementation of IAdminMenuPlugin

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var rootPluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins");
            if (rootPluginNode == null)
                return;

            var pluginNode = rootPluginNode.ChildNodes.FirstOrDefault(x => x.SystemName == "FoxNetSoft");
            if (pluginNode == null)
            {
                pluginNode = new SiteMapNode()
                {
                    Title = "FoxNetSoft",
                    ControllerName = "",
                    ActionName = "",
                    Visible = true,
                    RouteValues = new RouteValueDictionary() { { "area", null } },
                    SystemName = "FoxNetSoft",
                    IconClass = "fa-dot-circle-o"
                };
                rootPluginNode.ChildNodes.Add(pluginNode);

                var SubMenuItem3 = new SiteMapNode()
                {
                    Title = "Help on Youtube channel",
                    ControllerName = "",
                    ActionName = "",
                    Url = "http://www.youtube.com/foxnetsoft",
                    Visible = true,
                    RouteValues = new RouteValueDictionary() { { "area", null } },
                    IconClass = "fa-dot-circle-o"
                };
                pluginNode.ChildNodes.Add(SubMenuItem3);

                var SubMenuItem4 = new SiteMapNode()
                {
                    Title = "Help on foxnetsoft.com Forum",
                    ControllerName = "",
                    ActionName = "",
                    Url = "http://www.foxnetsoft.com/boards",
                    Visible = true,
                    RouteValues = new RouteValueDictionary() { { "area", null } },
                    IconClass = "fa-dot-circle-o"
                };
                pluginNode.ChildNodes.Add(SubMenuItem4);
            }

            var menuItem = new SiteMapNode()
            {
                Title = "SpeedFilters",
                ControllerName = "",
                ActionName = "",
                Url = "~/Admin/Plugin/ConfigureMiscPlugin?systemName=" + this.PluginDescriptor.SystemName,
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "area", null } },
                SystemName = "FoxNetSoft.SpeedFilters.Configure",
                IconClass = "fa-dot-circle-o"
            };
            pluginNode.ChildNodes.Insert(0, menuItem);
        }

        #endregion

        #region Implementation of Install - UnInstall

        public override void Install()
        {
            /*save default settings*/
            SpeedFiltersSettings _settings = new SpeedFiltersSettings()
            {
                EnableSpeedFilters = true,
                IgnoreDiscountsForCatalog = true,
                WidgetZone = "left_side_column_before",
//                WidgetZone = "left_side_column_after_category_navigation",
                FiltersType = "checkbox",

                EnableFiltersForCategory = true,
                SkipFiltersforCategories = "",
                EnableFiltersForManufacturer = false,
                SkipFiltersforManufacturers = "",
                EnableFiltersForVendor = false,
                SkipFiltersforVendors = "",
                EnableFiltersForSearchPage = false,

                EnablePriceRangeFilter=false,
                EnableSpecificationsFilter=true,
                EnableAttributesFilter= false,
                EnableManufacturersFilter= false,
                EnableVendorsFilter= false,
                DefaultClosePriceRange=false,
                DefaultCloseSpecifications = true,
                DefaultCloseAttributes = false,
                DefaultCloseManufacturers = false,
                DefaultCloseVendors = false,

                SelectorForListPanel =".page .product-list",
                SelectorForGridPanel = ".page .product-grid",
                SelectorForPager =".pager",
                SelectorForSortOptions ="#products-orderby",
                SelectorForViewOptions ="#products-viewmode",
                SelectorForProductPageSize ="#products-pagesize",
                ProductSelectorsSelector = ".page .product-selectors",
                SelectorForScrolling = ".page .product-selectors",

                DefaultProductSorting = Nop.Core.Domain.Catalog.ProductSortingEnum.PriceAsc,
                ScrollAfterFiltration =false,
                FiltersConditionSpecifications = "OR",
                FiltersConditionAttributes = "OR",
                FiltersConditionBetweenBlocks = "OR",
                prepareSpecificationAttributes = false,
                AllowSelectFiltersInOneBlock=true,
                showDebugInfo = false,
                Version = 114,
                GlobalCustomKeyword=""
            };
            var settingService = EngineContext.Current.ContainerManager.Resolve<ISettingService>();
            settingService.SaveSetting(_settings);

            //mark _widget as active
            WidgetSettings widgetSettings = EngineContext.Current.ContainerManager.Resolve<WidgetSettings>();
            widgetSettings.ActiveWidgetSystemNames.Add(this.PluginDescriptor.SystemName);
            settingService.SaveSetting(widgetSettings);

            SpeedFiltersObjectContext _context = EngineContext.Current.ContainerManager.Resolve<SpeedFiltersObjectContext>();
            _context.Install();

            #region LocaleResource

            string path;
            path = "~/Plugins/FoxNetSoft.SpeedFilters/Resources/";
            var il = new InstallLocaleResources(path);
            il.Install();

            #endregion

            base.Install();
        }

        public override void Uninstall()
        {
            #region LocaleResource
            string path;
            path = "~/Plugins/FoxNetSoft.SpeedFilters/Resources/";

            var il = new InstallLocaleResources(path);
            il.UnInstall("FoxNetSoft.Plugin.Misc.SpeedFilters.");
            #endregion

            var settingService = EngineContext.Current.ContainerManager.Resolve<ISettingService>();
            //mark _widget as inactive
            WidgetSettings widgetSettings = EngineContext.Current.ContainerManager.Resolve<WidgetSettings>();
            widgetSettings.ActiveWidgetSystemNames.Remove(this.PluginDescriptor.SystemName);
            settingService.SaveSetting(widgetSettings);
            //settings
            settingService.DeleteSetting<SpeedFiltersSettings>();

            SpeedFiltersObjectContext _context = EngineContext.Current.ContainerManager.Resolve<SpeedFiltersObjectContext>();
            _context.Uninstall();
            base.Uninstall();
        }
        #endregion

        public SpeedFilterSeoModel GetMetaOptions(string filterSeoUrl,Category category) {
            var sFiltersData = _speedFiltersService.GenerateSpecificationUrl(filterSeoUrl);
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
            return metaModel;
        }

        public string GetCustomKeyWord(Category category)
        {
            var KeyWord = "";
            List<int> speciFiedCategories = new List<int>();
            if (!string.IsNullOrEmpty(_speedFiltersSettings.OptimizedCategory))
                speciFiedCategories = _speedFiltersSettings.OptimizedCategory.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            if (speciFiedCategories.Contains(category.Id))
            {
                KeyWord= _speedFiltersSettings.SpecificCustomKeyword;
            }
            else
            {
                KeyWord= _speedFiltersSettings.GlobalCustomKeyword;
            }
           
           
            return KeyWord;
        }

        public bool CategoryFilterEnabled() {
            if (_speedFiltersSettings.EnableSpeedFilters == false)
                return false;
            else if (_speedFiltersSettings.EnableFiltersForCategory == false)
                return false;
            else
                return true;
        }
        public bool IsPermanentRedirect(string FilterUrl,int categoryId)
        {
            var _workContext = EngineContext.Current.Resolve<IWorkContext>();
            #region 301 redirect
            IPagedList<Product> products;
            IList<Product> subProducts;
            IList<ProductSEOModel> productsSEOSlug;
            IList<int> filterableSpecificationAttributeOptionIds = null;
            IList<int> filterableProductAttributeOptionIds = null;
            IList<int> filterableManufacturerIds = null;
            IList<int> filterableVendorIds = null;
            bool hasError = false;

            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles
               .Where(cr => cr.Active).Select(cr => cr.Id).ToList();

            SelectedSpeedFilter model1 = new SelectedSpeedFilter(this._catalogSettings, this._speedFiltersSettings, this._fnsLogger);
            var selectedAttrData1 = model1.GetParameters(FilterUrl);

            this._speedFiltersService.GetFilteredProducts(
            out products,
            out subProducts,
            out productsSEOSlug,
            out filterableSpecificationAttributeOptionIds,
            out filterableProductAttributeOptionIds,
            out filterableManufacturerIds,
            out filterableVendorIds,
            out hasError,
            categoryId: categoryId,
            manufacturerIds: new List<int>(),
            vendorIds: new List<int>(),
            storeId: !_catalogSettings.IgnoreStoreLimitations ? _storeContext.CurrentStore.Id : 0,//_storeContext.CurrentStore.Id,
            languageId: _workContext.WorkingLanguage.Id,
            allowedCustomerRolesIds: customerRolesIds,
            ShowProductsFromSubcategories: _catalogSettings.ShowProductsFromSubcategories,
            featuredProducts: _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
            filteredSpecs: model1.specFilterIds,
            filteredAtrs: model1.attrFilterIds,
            orderBy: (ProductSortingEnum)model1.OrderBy, //command.OrderBy,
            showOnSaldo: model1.ShowOnSaldo,
            speedFiltersSettings: _speedFiltersSettings
            );
            #endregion
            if (!products.Any())
                return true;
            else
                return false;
        }

        public string GenerateSpecificationUrl(string FilterUrl,string paramUrl) {
            var sFiltersData = _speedFiltersService.GenerateSpecificationUrl(FilterUrl);
            var sFiltersUrl = "";
            string generatedUrl = "";
            if (sFiltersData != null && !string.IsNullOrEmpty(sFiltersData.sFilter))
                sFiltersUrl = sFiltersData.sFilter;
            if (!string.IsNullOrEmpty(sFiltersUrl))
            {
                generatedUrl = sFiltersUrl + paramUrl;
            }
            return generatedUrl;
        }
    }
}