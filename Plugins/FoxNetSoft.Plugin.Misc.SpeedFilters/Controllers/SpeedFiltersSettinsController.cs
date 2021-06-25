using System;
using System.IO;
using System.Web.Hosting;
using System.Collections.Generic;
using System.Xml;
using System.Web.Mvc;
using Nop.Services.Configuration;
using Nop.Admin.Controllers;
using Nop.Web.Framework.Controllers;
using Nop.Core;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Logging;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Logger;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Models;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Services;
using Nop.Admin.Helpers;
using Nop.Services.Catalog;
using System.Linq;
namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Controllers
{
    [AdminAuthorize]
    public class SpeedFiltersSettinsController : BaseAdminController
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

        private readonly ISettingService _settingService;
        private readonly ISpeedFiltersService _speedFiltersService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly ICacheManager _cacheManager;

        private bool showDebugInfo;
        private readonly FNSLogger _fnsLogger;
        private readonly ICategoryService _categoryService;

        #endregion

        #region Ctor
        public SpeedFiltersSettinsController(ISettingService settingService,
                ISpeedFiltersService speedFiltersService,
                IPermissionService permissionService,
                IStoreService storeService,
                IWorkContext workContext,
                ILogger logger, ICategoryService categoryService
            )
        {
            this._settingService = settingService;
            this._speedFiltersService = speedFiltersService;
            this._permissionService = permissionService;
            this._storeService = storeService;
            this._workContext = workContext;
            this._logger = logger;
            this._categoryService = categoryService;
            var speedFiltersSettings = _settingService.LoadSetting<SpeedFiltersSettings>();

            this.showDebugInfo = speedFiltersSettings.showDebugInfo;
            this._fnsLogger = new FNSLogger(this.showDebugInfo);

            //TODO inject static cache manager using constructor
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
        }
        #endregion

        #region Utils
        [NonAction]
        private void LogMessage(string message)
        {
            if (this.showDebugInfo)
            {
                this._fnsLogger.LogMessage(message);
            }
        }

        [NonAction]
        private void PrepareAvailableWidgetsModel(SpeedFiltersSettingsModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var widgetnames = GetWidgetZones();
            foreach (var widgetname in widgetnames)
            {
                model.AvailableWidgets.Add(new SelectListItem()
                {
                    /*Text = articletype.Name,
                    Value = articletype.Id.ToString()*/
                    Text = widgetname,
                    Value = widgetname
                });
            }
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        [NonAction]
        protected IList<string> GetWidgetZones()
        {
            /*string key = string.Format(ARTICLE_WIDGET_PATTERN_KEY);
            return _cacheManager.Get(key, () =>
            {*/
            IList<string> widgetZones = new List<string>();
            try
            {
                string widgetPath;
                widgetPath = HostingEnvironment.MapPath("~/Plugins/FoxNetSoft.SpeedFilters/WidgetZones.xml");

                using (XmlTextReader reader = new XmlTextReader(widgetPath))
                {
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element: // Узел является элементом.
                                break;
                            case XmlNodeType.Text: // Вывести текст в каждом элементе.
                                widgetZones.Add(reader.Value);
                                //_logger.Information("333", new NopException(reader.Value));
                                break;
                            case XmlNodeType.EndElement: // Вывести конец элемента.
                                break;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc.Message, exc);
            }
            return widgetZones;
        }

        [NonAction]
        protected string GetViewname(string viewname)
        {
            return "~/Plugins/FoxNetSoft.SpeedFilters/Views/SpeedFiltersSettins/" + viewname + ".cshtml";
        }

        #endregion

        #region Configure
        [HttpGet]
        public ActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            #region LocaleResource

            string path;
            path = "~/Plugins/FoxNetSoft.SpeedFilters/Resources/";

            var il = new InstallLocaleResources(path);
            il.Update();
            #endregion

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

            var speedFiltersSettings = _settingService.LoadSetting<SpeedFiltersSettings>(storeScope);
            var model = new SpeedFiltersSettingsModel();

            //meta change

            int categoryId = 0;
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
            {
                model.AvailableCategories.Add(c);
            }
            categoryId = model.AvailableCategories != null && model.AvailableCategories.Count > 0? Convert.ToInt32(model.AvailableCategories.Select(x => x.Value).FirstOrDefault()):0;
            model.GlobalCustomKeyword = speedFiltersSettings.GlobalCustomKeyword;
            model.GlobalMetaTitle = speedFiltersSettings.GlobalMetaTitle;
            model.GlobalMetaDescription = speedFiltersSettings.GlobalMetaDescription;
            model.GlobalMetaKeyWord = speedFiltersSettings.GlobalMetaKeyWord;
            model.GlobalHTag = speedFiltersSettings.GlobalHTag;
            model.GlobalHeaderCopy = speedFiltersSettings.GlobalHeaderCopy;
            model.GlobalHeaderTitle = speedFiltersSettings.GlobalHeaderTitle;

            model.GlobalFooterContent1 = speedFiltersSettings.GlobalFooterContent1;
            model.GlobalFooterContent2 = speedFiltersSettings.GlobalFooterContent2;
            model.GlobalFooterContent3= speedFiltersSettings.GlobalFooterContent3;

            //model.SpecificFooterContent1 = speedFiltersSettings.SpecificFooterContent1;
            //model.SpecificFooterContent2 = speedFiltersSettings.SpecificFooterContent2;
            //model.SpecificFooterContent3= speedFiltersSettings.SpecificFooterContent3;
            
            model.GlobalFooterTitle1 = speedFiltersSettings.GlobalFooterTitle1;
            model.GlobalFooterTitle2 = speedFiltersSettings.GlobalFooterTitle2;
            model.GlobalFooterTitle3 = speedFiltersSettings.GlobalFooterTitle3;
            
            //model.SpecificFooterTitle1 = speedFiltersSettings.SpecificFooterTitle1;
            //model.SpecificFooterTitle2 = speedFiltersSettings.SpecificFooterTitle2;
            //model.SpecificFooterTitle3 = speedFiltersSettings.SpecificFooterTitle3;

            //model.SpecificCustomKeyword = speedFiltersSettings.SpecificCustomKeyword;
            //model.SpecificMetaTitle = speedFiltersSettings.SpecificMetaTitle;
            //model.SpecificMetaDescription = speedFiltersSettings.SpecificMetaDescription;
            //model.SpecificMetaKeyWord = speedFiltersSettings.SpecificMetaKeyWord;
            //model.SpecificHTag = speedFiltersSettings.SpecificHTag;
            //model.SpecificHeaderCopy = speedFiltersSettings.SpecificHeaderCopy;
            //model.SpecificHeaderTitle = speedFiltersSettings.SpecificHeaderTitle;
            //model.OptimizedCategoryString = speedFiltersSettings.OptimizedCategory;
            //if (!string.IsNullOrEmpty(speedFiltersSettings.OptimizedCategory)) {
            //    model.OptimizedCategory = speedFiltersSettings.OptimizedCategory.Split(',');
            //}
            var specificCategory = _speedFiltersService.GetSpecificCategorySettingByCategoryId(categoryId);
            if(specificCategory == null)
            {
                specificCategory = new Domain.SS_Specific_Category_Setting();
            }
            model.OptimizedCategory = Convert.ToInt32(specificCategory.CategoryId);
            model.SpecificCustomKeyword = specificCategory.CustomKeyword;
            model.SpecificMetaTitle = specificCategory.MetaTitle;
            model.SpecificMetaDescription = specificCategory.MetaDescription;
            model.SpecificMetaKeyWord = specificCategory.MetaKeyword;
            model.SpecificHTag = specificCategory.H1Tag;
            model.SpecificHeaderCopy = specificCategory.HeaderCopy;
            model.SpecificHeaderTitle = specificCategory.HeaderTitle;
            model.SpecificFooterTitle1 = specificCategory.FooterTitle1;
            model.SpecificFooterTitle2 = specificCategory.FooterTitle2;
            model.SpecificFooterTitle3 = specificCategory.FooterTitle3;
            model.SpecificFooterContent1 = specificCategory.FooterContent1;
            model.SpecificFooterContent2 = specificCategory.FooterContent2;
            model.SpecificFooterContent3 = specificCategory.FooterContent3;
            model.SpecificCategorySettingId = specificCategory.Id;
            //////

            model.ActiveStoreScopeConfiguration = storeScope;

            model.showDebugInfo = speedFiltersSettings.showDebugInfo;
            
            model.EnableSpeedFilters = speedFiltersSettings.EnableSpeedFilters;
            model.IgnoreDiscountsForCatalog = speedFiltersSettings.IgnoreDiscountsForCatalog;
            model.WidgetZone = speedFiltersSettings.WidgetZone;

            model.FiltersType = speedFiltersSettings.FiltersType;
            model.EnableFiltersForCategory = speedFiltersSettings.EnableFiltersForCategory;
            model.SkipFiltersforCategories = speedFiltersSettings.SkipFiltersforCategories;
            model.EnableFiltersForManufacturer = speedFiltersSettings.EnableFiltersForManufacturer;
            model.SkipFiltersforManufacturers = speedFiltersSettings.SkipFiltersforManufacturers;
            model.EnableFiltersForVendor = speedFiltersSettings.EnableFiltersForVendor;
            model.SkipFiltersforVendors = speedFiltersSettings.SkipFiltersforVendors;
            model.EnableFiltersForSearchPage = speedFiltersSettings.EnableFiltersForSearchPage;

            model.EnablePriceRangeFilter = speedFiltersSettings.EnablePriceRangeFilter;
            model.DefaultClosePriceRange = speedFiltersSettings.DefaultClosePriceRange;
            model.EnableSpecificationsFilter = speedFiltersSettings.EnableSpecificationsFilter;
            model.DefaultCloseSpecifications = speedFiltersSettings.DefaultCloseSpecifications;
            model.EnableAttributesFilter = speedFiltersSettings.EnableAttributesFilter;
            model.DefaultCloseAttributes = speedFiltersSettings.DefaultCloseAttributes;
            model.EnableManufacturersFilter = speedFiltersSettings.EnableManufacturersFilter;
            model.DefaultCloseManufacturers = speedFiltersSettings.DefaultCloseManufacturers;
            model.EnableVendorsFilter = speedFiltersSettings.EnableVendorsFilter;
            model.DefaultCloseVendors = speedFiltersSettings.DefaultCloseVendors;

            model.SelectorForListPanel = speedFiltersSettings.SelectorForListPanel;
            model.SelectorForGridPanel = speedFiltersSettings.SelectorForGridPanel;
            model.SelectorForPager = speedFiltersSettings.SelectorForPager;
            model.SelectorForSortOptions = speedFiltersSettings.SelectorForSortOptions;
            model.SelectorForViewOptions = speedFiltersSettings.SelectorForViewOptions;
            model.SelectorForProductPageSize = speedFiltersSettings.SelectorForProductPageSize;
            model.ProductSelectorsSelector = speedFiltersSettings.ProductSelectorsSelector;

            model.SelectorForScrolling = speedFiltersSettings.SelectorForScrolling;

            model.FiltersConditionSpecifications = speedFiltersSettings.FiltersConditionSpecifications;
            model.FiltersConditionAttributes = speedFiltersSettings.FiltersConditionAttributes;
            model.AllowSelectFiltersInOneBlock = speedFiltersSettings.AllowSelectFiltersInOneBlock;
            model.FiltersConditionBetweenBlocks = speedFiltersSettings.FiltersConditionBetweenBlocks;

            //model.DefaultProductSorting = speedFiltersSettings.DefaultProductSorting;

            //DefaultProductSorting
            foreach (Nop.Core.Domain.Catalog.ProductSortingEnum productSortingEnum in Enum.GetValues(typeof(Nop.Core.Domain.Catalog.ProductSortingEnum)))
            {
                model.DefaultProductSortingValues.Add(new SelectListItem()
                {
                    Text = CommonHelper.ConvertEnum(productSortingEnum.ToString()),
                    Value = productSortingEnum.ToString(),
                    Selected = productSortingEnum == speedFiltersSettings.DefaultProductSorting
                });
            }

            model.ScrollAfterFiltration = speedFiltersSettings.ScrollAfterFiltration;

            //AvailableWidgets
            PrepareAvailableWidgetsModel(model);
            //AvailableFiltersTypes
            model.AvailableFiltersTypes.Add(new SelectListItem()
            {
                Text = "checkbox",
                Value = "checkbox"
            });
            model.AvailableFiltersTypes.Add(new SelectListItem()
            {
                Text = "dropdown",
                Value = "dropdown"
            });

            //AvailableFiltersConditionSpecifications
            model.AvailableFiltersConditionSpecifications.Add(new SelectListItem()
            {
                Text = "AND",
                Value = "AND"
            });
            model.AvailableFiltersConditionSpecifications.Add(new SelectListItem()
            {
                Text = "OR",
                Value = "OR"
            });
            //AvailableFiltersConditionAttributes
            model.AvailableFiltersConditionAttributes.Add(new SelectListItem()
            {
                Text = "AND",
                Value = "AND"
            });
            model.AvailableFiltersConditionAttributes.Add(new SelectListItem()
            {
                Text = "OR",
                Value = "OR"
            });
            //AvailableFiltersConditionBetweenBlocks
            model.AvailableFiltersConditionBetweenBlocks.Add(new SelectListItem()
            {
                Text = "AND",
                Value = "AND"
            });
            model.AvailableFiltersConditionBetweenBlocks.Add(new SelectListItem()
            {
                Text = "OR",
                Value = "OR"
            });

            if (storeScope > 0)
            {
                model.EnableSpeedFilters_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.EnableSpeedFilters, storeScope);
                model.IgnoreDiscountsForCatalog_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.IgnoreDiscountsForCatalog, storeScope);
                model.WidgetZone_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.WidgetZone, storeScope);
                model.FiltersType_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.FiltersType, storeScope);
                model.EnableFiltersForCategory_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.EnableFiltersForCategory, storeScope);
                model.SkipFiltersforCategory_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.SkipFiltersforCategories, storeScope);
                model.EnableFiltersForManufacturer_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.EnableFiltersForManufacturer, storeScope);
                model.SkipFiltersforManufacturer_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.SkipFiltersforManufacturers, storeScope);
                model.EnableFiltersForVendor_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.EnableFiltersForVendor, storeScope);
                model.SkipFiltersforVendors_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.SkipFiltersforVendors, storeScope);
                model.EnableFiltersForSearchPage_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.EnableFiltersForSearchPage, storeScope);

                model.EnablePriceRangeFilter_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.EnablePriceRangeFilter, storeScope);
                model.DefaultClosePriceRange_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.DefaultClosePriceRange, storeScope);
                model.EnableSpecificationsFilter_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.EnableSpecificationsFilter, storeScope);
                model.DefaultCloseSpecifications_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.DefaultCloseSpecifications, storeScope);
                model.EnableAttributesFilter_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.EnableAttributesFilter, storeScope);
                model.DefaultCloseAttributes_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.DefaultCloseAttributes, storeScope);
                model.EnableManufacturersFilter_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.EnableManufacturersFilter, storeScope);
                model.DefaultCloseManufacturers_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.DefaultCloseManufacturers, storeScope);
                model.EnableVendorsFilter_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.EnableVendorsFilter, storeScope);
                model.DefaultCloseVendors_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.DefaultCloseVendors, storeScope);

                model.SelectorForListPanel_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.SelectorForListPanel, storeScope);
                model.SelectorForGridPanel_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.SelectorForGridPanel, storeScope);
                model.SelectorForPager_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.SelectorForPager, storeScope);
                model.SelectorForSortOptions_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.SelectorForSortOptions, storeScope);
                model.SelectorForViewOptions_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.SelectorForViewOptions, storeScope);
                model.SelectorForProductPageSize_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.SelectorForProductPageSize, storeScope);
                model.ProductSelectorsSelector_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.ProductSelectorsSelector, storeScope);

                model.SelectorForScrolling_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.SelectorForScrolling, storeScope);

                model.DefaultProductSorting_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.DefaultProductSorting, storeScope);
                model.ScrollAfterFiltration_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.ScrollAfterFiltration, storeScope);

                model.FiltersConditionSpecifications_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.FiltersConditionSpecifications, storeScope);
                model.FiltersConditionAttributes_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.FiltersConditionAttributes, storeScope);
                model.AllowSelectFiltersInOneBlock_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.AllowSelectFiltersInOneBlock, storeScope);
                model.FiltersConditionBetweenBlocks_OverrideForStore = _settingService.SettingExists(speedFiltersSettings, x => x.FiltersConditionBetweenBlocks, storeScope);
            }
            return View(GetViewname("Configure"), model);
        }

        [ChildActionOnly]
        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult Configure(SpeedFiltersSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var speedFiltersSettings = _settingService.LoadSetting<SpeedFiltersSettings>(storeScope);

            //speedFiltersSettings = model.ToEntity(speedFiltersSettings);

            /// meta change
            speedFiltersSettings.GlobalCustomKeyword = model.GlobalCustomKeyword;
            speedFiltersSettings.GlobalMetaTitle = model.GlobalMetaTitle;
            speedFiltersSettings.GlobalMetaDescription = model.GlobalMetaDescription;
            speedFiltersSettings.GlobalMetaKeyWord = model.GlobalMetaKeyWord;
            speedFiltersSettings.GlobalHTag = model.GlobalHTag;
            speedFiltersSettings.GlobalHeaderCopy = model.GlobalHeaderCopy;
            speedFiltersSettings.GlobalHeaderTitle = model.GlobalHeaderTitle;
            //speedFiltersSettings.SpecificCustomKeyword = model.SpecificCustomKeyword;
            //speedFiltersSettings.SpecificMetaTitle = model.SpecificMetaTitle;
            //speedFiltersSettings.SpecificMetaDescription = model.SpecificMetaDescription;
            //speedFiltersSettings.SpecificMetaKeyWord = model.SpecificMetaKeyWord;
            //speedFiltersSettings.SpecificHTag = model.SpecificHTag;
            //speedFiltersSettings.SpecificHeaderCopy = model.SpecificHeaderCopy;
            //speedFiltersSettings.SpecificHeaderTitle = model.SpecificHeaderTitle;
            //speedFiltersSettings.OptimizedCategory =string.Join(",",model.OptimizedCategory);

            speedFiltersSettings.GlobalFooterContent1 = model.GlobalFooterContent1;
            speedFiltersSettings.GlobalFooterContent2 = model.GlobalFooterContent2;
            speedFiltersSettings.GlobalFooterContent3 = model.GlobalFooterContent3;

            //speedFiltersSettings.SpecificFooterContent1 = model.SpecificFooterContent1;
            //speedFiltersSettings.SpecificFooterContent2 = model.SpecificFooterContent2;
            //speedFiltersSettings.SpecificFooterContent3 = model.SpecificFooterContent3;

            speedFiltersSettings.GlobalFooterTitle1 = model.GlobalFooterTitle1;
            speedFiltersSettings.GlobalFooterTitle2 = model.GlobalFooterTitle2;
            speedFiltersSettings.GlobalFooterTitle3 = model.GlobalFooterTitle3;

            //speedFiltersSettings.SpecificFooterTitle1 = model.SpecificFooterTitle1;
            //speedFiltersSettings.SpecificFooterTitle2 = model.SpecificFooterTitle2;
            //speedFiltersSettings.SpecificFooterTitle3 = model.SpecificFooterTitle3;

            ////

            speedFiltersSettings.showDebugInfo = model.showDebugInfo;
            
            speedFiltersSettings.EnableSpeedFilters = model.EnableSpeedFilters;
            speedFiltersSettings.IgnoreDiscountsForCatalog = model.IgnoreDiscountsForCatalog;
            speedFiltersSettings.WidgetZone = model.WidgetZone;
            speedFiltersSettings.FiltersType = model.FiltersType;
            speedFiltersSettings.EnableFiltersForCategory = model.EnableFiltersForCategory;
            speedFiltersSettings.SkipFiltersforCategories = model.SkipFiltersforCategories;
            speedFiltersSettings.EnableFiltersForManufacturer = model.EnableFiltersForManufacturer;
            speedFiltersSettings.SkipFiltersforManufacturers = model.SkipFiltersforManufacturers;
            speedFiltersSettings.EnableFiltersForVendor = model.EnableFiltersForVendor;
            speedFiltersSettings.SkipFiltersforVendors = model.SkipFiltersforVendors;
            speedFiltersSettings.EnableFiltersForSearchPage = model.EnableFiltersForSearchPage;

            speedFiltersSettings.EnablePriceRangeFilter = model.EnablePriceRangeFilter;
            speedFiltersSettings.DefaultClosePriceRange = model.DefaultClosePriceRange;
            speedFiltersSettings.EnableSpecificationsFilter = model.EnableSpecificationsFilter;
            speedFiltersSettings.DefaultCloseSpecifications = model.DefaultCloseSpecifications;
            speedFiltersSettings.EnableAttributesFilter = model.EnableAttributesFilter;
            speedFiltersSettings.DefaultCloseAttributes = model.DefaultCloseAttributes;
            speedFiltersSettings.EnableManufacturersFilter = model.EnableManufacturersFilter;
            speedFiltersSettings.DefaultCloseManufacturers = model.DefaultCloseManufacturers;
            speedFiltersSettings.EnableVendorsFilter = model.EnableVendorsFilter;
            speedFiltersSettings.DefaultCloseVendors = model.DefaultCloseVendors;

            speedFiltersSettings.SelectorForListPanel = model.SelectorForListPanel;
            speedFiltersSettings.SelectorForGridPanel = model.SelectorForGridPanel;
            speedFiltersSettings.SelectorForPager = model.SelectorForPager;
            speedFiltersSettings.SelectorForSortOptions = model.SelectorForSortOptions;
            speedFiltersSettings.SelectorForViewOptions = model.SelectorForViewOptions;
            speedFiltersSettings.SelectorForProductPageSize = model.SelectorForProductPageSize;
            speedFiltersSettings.ProductSelectorsSelector = model.ProductSelectorsSelector;
            speedFiltersSettings.SelectorForScrolling = model.SelectorForScrolling;

            //speedFiltersSettings.DefaultProductSorting = model.DefaultProductSorting;
            if (!String.IsNullOrEmpty(model.DefaultProductSorting))
                speedFiltersSettings.DefaultProductSorting = (Nop.Core.Domain.Catalog.ProductSortingEnum)Enum.Parse(typeof(Nop.Core.Domain.Catalog.ProductSortingEnum), model.DefaultProductSorting);


            speedFiltersSettings.ScrollAfterFiltration = model.ScrollAfterFiltration;

            speedFiltersSettings.FiltersConditionSpecifications = model.FiltersConditionSpecifications;
            speedFiltersSettings.FiltersConditionAttributes = model.FiltersConditionAttributes;
            speedFiltersSettings.AllowSelectFiltersInOneBlock = model.AllowSelectFiltersInOneBlock;
            speedFiltersSettings.FiltersConditionBetweenBlocks = model.FiltersConditionBetweenBlocks;

            //_settingService.SaveSetting(speedFiltersSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSetting(speedFiltersSettings, x => x.showDebugInfo, 0, false);

            _settingService.SaveSetting(speedFiltersSettings, x => x.GlobalCustomKeyword, storeScope, false);
            _settingService.SaveSetting(speedFiltersSettings, x => x.GlobalMetaTitle, storeScope, false);
            _settingService.SaveSetting(speedFiltersSettings, x => x.GlobalMetaDescription, storeScope, false);
            _settingService.SaveSetting(speedFiltersSettings, x => x.GlobalMetaKeyWord, storeScope, false);
            _settingService.SaveSetting(speedFiltersSettings, x => x.GlobalHTag, storeScope, false);
            _settingService.SaveSetting(speedFiltersSettings, x => x.GlobalHeaderCopy, storeScope, false);
            _settingService.SaveSetting(speedFiltersSettings, x => x.GlobalHeaderTitle, storeScope, false);
            //_settingService.SaveSetting(speedFiltersSettings, x => x.SpecificCustomKeyword, storeScope, false);
            //_settingService.SaveSetting(speedFiltersSettings, x => x.SpecificMetaTitle, storeScope, false);
            //_settingService.SaveSetting(speedFiltersSettings, x => x.SpecificMetaDescription, storeScope, false);
            //_settingService.SaveSetting(speedFiltersSettings, x => x.SpecificMetaKeyWord, storeScope, false);
            //_settingService.SaveSetting(speedFiltersSettings, x => x.SpecificHTag, storeScope, false);
            //_settingService.SaveSetting(speedFiltersSettings, x => x.SpecificHeaderCopy, storeScope, false);
            //_settingService.SaveSetting(speedFiltersSettings, x => x.SpecificHeaderTitle, storeScope, false);
            //_settingService.SaveSetting(speedFiltersSettings, x => x.OptimizedCategory, storeScope, false);

            _settingService.SaveSetting(speedFiltersSettings, x => x.GlobalFooterContent1, storeScope, false);
            _settingService.SaveSetting(speedFiltersSettings, x => x.GlobalFooterContent2, storeScope, false);
            _settingService.SaveSetting(speedFiltersSettings, x => x.GlobalFooterContent3, storeScope, false);

            //_settingService.SaveSetting(speedFiltersSettings, x => x.SpecificFooterContent1, storeScope, false);
            //_settingService.SaveSetting(speedFiltersSettings, x => x.SpecificFooterContent2, storeScope, false);
            //_settingService.SaveSetting(speedFiltersSettings, x => x.SpecificFooterContent3, storeScope, false);


            _settingService.SaveSetting(speedFiltersSettings, x => x.GlobalFooterTitle1, storeScope, false);
            _settingService.SaveSetting(speedFiltersSettings, x => x.GlobalFooterTitle2, storeScope, false);
            _settingService.SaveSetting(speedFiltersSettings, x => x.GlobalFooterTitle3, storeScope, false);

            //_settingService.SaveSetting(speedFiltersSettings, x => x.SpecificFooterTitle1, storeScope, false);
            //_settingService.SaveSetting(speedFiltersSettings, x => x.SpecificFooterTitle2, storeScope, false);
            //_settingService.SaveSetting(speedFiltersSettings, x => x.SpecificFooterTitle3, storeScope, false);
            //--------------------------------------------------
            if (model.EnableSpeedFilters_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.EnableSpeedFilters, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.EnableSpeedFilters, storeScope);

            if (model.IgnoreDiscountsForCatalog_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.IgnoreDiscountsForCatalog, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.IgnoreDiscountsForCatalog, storeScope);


            if (model.WidgetZone_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.WidgetZone, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.WidgetZone, storeScope);

            if (model.FiltersType_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.FiltersType, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.FiltersType, storeScope);

            if (model.EnableFiltersForCategory_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.EnableFiltersForCategory, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.EnableFiltersForCategory, storeScope);

            if (model.SkipFiltersforCategory_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.SkipFiltersforCategories, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.SkipFiltersforCategories, storeScope);

            if (model.EnableFiltersForManufacturer_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.EnableFiltersForManufacturer, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.EnableFiltersForManufacturer, storeScope);

            if (model.SkipFiltersforManufacturer_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.SkipFiltersforManufacturers, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.SkipFiltersforManufacturers, storeScope);

            if (model.EnableFiltersForVendor_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.EnableFiltersForVendor, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.EnableFiltersForVendor, storeScope);

            if (model.SkipFiltersforVendors_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.SkipFiltersforVendors, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.SkipFiltersforVendors, storeScope);

            if (model.EnableFiltersForSearchPage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.EnableFiltersForSearchPage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.EnableFiltersForSearchPage, storeScope);
            //----------------------------

            if (model.EnablePriceRangeFilter_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.EnablePriceRangeFilter, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.EnablePriceRangeFilter, storeScope);

            if (model.DefaultClosePriceRange_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.DefaultClosePriceRange, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.DefaultClosePriceRange, storeScope);

            if (model.EnableSpecificationsFilter_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.EnableSpecificationsFilter, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.EnableSpecificationsFilter, storeScope);

            if (model.DefaultCloseSpecifications_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.DefaultCloseSpecifications, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.DefaultCloseSpecifications, storeScope);

            if (model.EnableAttributesFilter_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.EnableAttributesFilter, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.EnableAttributesFilter, storeScope);

            if (model.DefaultCloseAttributes_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.DefaultCloseAttributes, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.DefaultCloseAttributes, storeScope);

            if (model.EnableManufacturersFilter_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.EnableManufacturersFilter, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.EnableManufacturersFilter, storeScope);

            if (model.DefaultCloseManufacturers_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.DefaultCloseManufacturers, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.DefaultCloseManufacturers, storeScope);

            if (model.EnableVendorsFilter_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.EnableVendorsFilter, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.EnableVendorsFilter, storeScope);

            if (model.DefaultCloseVendors_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.DefaultCloseVendors, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.DefaultCloseVendors, storeScope);
            //---------------

            if (model.SelectorForListPanel_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.SelectorForListPanel, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.SelectorForListPanel, storeScope);
            if (model.SelectorForGridPanel_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.SelectorForGridPanel, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.SelectorForGridPanel, storeScope);
            if (model.SelectorForPager_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.SelectorForPager, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.SelectorForPager, storeScope);
            if (model.SelectorForSortOptions_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.SelectorForSortOptions, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.SelectorForSortOptions, storeScope);
            if (model.SelectorForViewOptions_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.SelectorForViewOptions, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.SelectorForViewOptions, storeScope);
            if (model.SelectorForProductPageSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.SelectorForProductPageSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.SelectorForProductPageSize, storeScope);

            if (model.ProductSelectorsSelector_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.ProductSelectorsSelector, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.ProductSelectorsSelector, storeScope);

            if (model.SelectorForScrolling_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.SelectorForScrolling, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.SelectorForScrolling, storeScope);

            if (model.DefaultProductSorting_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.DefaultProductSorting, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.DefaultProductSorting, storeScope);
            if (model.ScrollAfterFiltration_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.ScrollAfterFiltration, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.ScrollAfterFiltration, storeScope);

            if (model.FiltersConditionSpecifications_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.FiltersConditionSpecifications, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.FiltersConditionSpecifications, storeScope);

            if (model.FiltersConditionAttributes_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.FiltersConditionAttributes, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.FiltersConditionAttributes, storeScope);

            if (model.AllowSelectFiltersInOneBlock_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.AllowSelectFiltersInOneBlock, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.AllowSelectFiltersInOneBlock, storeScope);

            if (model.FiltersConditionBetweenBlocks_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(speedFiltersSettings, x => x.FiltersConditionBetweenBlocks, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(speedFiltersSettings, x => x.FiltersConditionBetweenBlocks, storeScope);

            #region Specific Category Setting

            var specificCategory = _speedFiltersService.GetSpecificCategorySettingById(model.SpecificCategorySettingId);
            if(specificCategory != null)
            {
                specificCategory.CategoryId = model.OptimizedCategory;
                specificCategory.CustomKeyword = model.SpecificCustomKeyword;
                specificCategory.MetaTitle = model.SpecificMetaTitle;
                specificCategory.MetaDescription = model.SpecificMetaDescription;
                specificCategory.MetaKeyword = model.SpecificMetaKeyWord;
                specificCategory.H1Tag = model.SpecificHTag;
                specificCategory.HeaderCopy = model.SpecificHeaderCopy;
                specificCategory.HeaderTitle = model.SpecificHeaderTitle;
                specificCategory.FooterTitle1 = model.SpecificFooterTitle1;
                specificCategory.FooterContent1 = model.SpecificFooterContent1;
                specificCategory.FooterTitle2 = model.SpecificFooterTitle2;
                specificCategory.FooterContent2 = model.SpecificFooterContent2;
                specificCategory.FooterTitle3 = model.SpecificFooterTitle3;
                specificCategory.FooterContent3 = model.SpecificFooterContent3;
                specificCategory.UpdatedOn = DateTime.UtcNow;
                _speedFiltersService.UpdateSpecificCategory(specificCategory);
            }
            else
            {
                specificCategory = new Domain.SS_Specific_Category_Setting()
                {
                    CategoryId = model.OptimizedCategory,
                    CustomKeyword=model.SpecificCustomKeyword,
                    MetaTitle = model.SpecificMetaTitle,
                    MetaDescription = model.SpecificMetaDescription,
                    MetaKeyword = model.SpecificMetaKeyWord,
                    H1Tag = model.SpecificHTag,
                    HeaderCopy = model.SpecificHeaderCopy,
                    HeaderTitle = model.SpecificHeaderTitle,
                    FooterTitle1 = model.SpecificFooterTitle1,
                    FooterContent1 = model.SpecificFooterContent1,
                    FooterTitle2 = model.SpecificFooterTitle2,
                    FooterContent2 = model.SpecificFooterContent2,
                    FooterTitle3 = model.SpecificFooterTitle3,
                    FooterContent3 = model.SpecificFooterContent3,
                    CreatedOn = DateTime.UtcNow
                };
                _speedFiltersService.InsertSpecificCategory(specificCategory);
            }

            #endregion

            //now clear settings cache
            _settingService.ClearCache();

            //cache
            _cacheManager.RemoveByPattern(SPEEDFILTERS_PATTERN_KEY);

            return Configure();
        }

        [HttpPost]
        public ActionResult GetSpecificCategorySettingByCatId(int categoryId)
        {
            return Json(_speedFiltersService.GetSpecificCategorySettingByCategoryId(categoryId));
        }

        #endregion

        #region Debug Data

        public ActionResult ClearLogFile()
        {
            this._fnsLogger.ClearLogFile();
            return RedirectToAction("ConfigureWidget", "Widget", new { systemName = "FoxNetSoft.Plugin.Misc.SpeedFilters", area = "Admin" });
        }

        public ActionResult GetLogFile()
        {
            string virtualFilePath = this._fnsLogger.GetLogFilePath();
            if (System.IO.File.Exists(virtualFilePath))
            {
                return File(virtualFilePath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(virtualFilePath));
            }
            return RedirectToAction("ConfigureWidget", "Widget", new { systemName = "FoxNetSoft.Plugin.Misc.SpeedFilters", area = "Admin" });
        }

        public static List<SelectListItem> GetCategoryList(ICategoryService categoryService, bool showHidden = false)
        {
            if (categoryService == null)
                throw new ArgumentNullException("categoryService");

            var result = new List<SelectListItem>();
            var categories = categoryService.GetAllCategories(showHidden: showHidden);
            foreach (var item in categories)
            {
                result.Add(new SelectListItem
                {
                    Text = item.GetFormattedBreadCrumb(categories),
                    Value = item.Id.ToString()
                });
            }

            return result;
        }
        #endregion
    }
}