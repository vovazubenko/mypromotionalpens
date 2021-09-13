using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Security;
using Nop.Web.Models.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Filter;
using Nop.Services.Seo;

namespace Nop.Web.Controllers
{
    public partial class CatalogController : BasePublicController
    {
        #region Fields

        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IProductModelFactory _productModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IProductTagService _productTagService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Constructors

        public CatalogController(ICatalogModelFactory catalogModelFactory,
            IProductModelFactory productModelFactory,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductService productService,
            IVendorService vendorService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IProductTagService productTagService,
            IGenericAttributeService genericAttributeService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            ICustomerActivityService customerActivityService,
            MediaSettings mediaSettings,
            CatalogSettings catalogSettings,
            VendorSettings vendorSettings)
        {
            this._catalogModelFactory = catalogModelFactory;
            this._productModelFactory = productModelFactory;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._vendorService = vendorService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._productTagService = productTagService;
            this._genericAttributeService = genericAttributeService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;
            this._customerActivityService = customerActivityService;
            this._mediaSettings = mediaSettings;
            this._catalogSettings = catalogSettings;
            this._vendorSettings = vendorSettings;
        }

        #endregion

        #region Categories

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult Category(int categoryId, CatalogPagingFilteringModel command)
        {
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null || category.Deleted)
                return InvokeHttp404();

            var notAvailable =
                //published?
                !category.Published ||
                //ACL (access control list) 
                !_aclService.Authorize(category) ||
                //Store mapping
                !_storeMappingService.Authorize(category);
            //Check whether the current user has a "Manage categories" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            if (notAvailable && !_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                DisplayEditLink(Url.Action("Edit", "Category", new { id = category.Id, area = "Admin" }));

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewCategory", _localizationService.GetResource("ActivityLog.PublicStore.ViewCategory"), category.Name);

            //model
            var model = _catalogModelFactory.PrepareCategoryModel(category, command);
            var data = new SpeedFilterSeoModel();
            //filter
            string filterSeoUrl = "";
            var routeData = System.Web.HttpContext.Current.Request.RequestContext.RouteData;//  ((System.Web.UI.Page)HttpContext.CurrentHandler).RouteData;
            try
            {
                filterSeoUrl = (string)routeData.Values["FilterUrl"];
            }
            catch
            {
                filterSeoUrl = "";
            }
            var _filterService = EngineContext.Current.Resolve<IFilterService>();
            var filterMethod = _filterService.LoadFilterMethodBySystemName("FoxNetSoft.Plugin.Misc.SpeedFilters");
            if (filterMethod != null)
            {
                if (filterMethod.CategoryFilterEnabled())
                {
                    string paramUrl = "";
                    try
                    {
                        paramUrl = (string)routeData.Values["paramUrl"];
                    }
                    catch
                    {
                        paramUrl = "";
                    }
                    if (!string.IsNullOrEmpty(filterSeoUrl))
                    {
                        string generatedUrl = _filterService.GenerateSpecificationUrl(filterSeoUrl, paramUrl);
                        if (!string.IsNullOrEmpty(generatedUrl))
                        {
                            bool isRedirect = _filterService.IsPermanentRedirect(generatedUrl, categoryId);
                            if (isRedirect)
                                return RedirectPermanent("/");
                        }
                    }
                    data = _filterService.GetMetaOptions(filterSeoUrl, category);
                }
            }
            //template

            if (data != null)
            {
                model.MetaTitle = !string.IsNullOrEmpty(data.MetaTitle) ? data.MetaTitle : model.MetaTitle;
                model.MetaKeywords = !string.IsNullOrEmpty(data.MetaKeyWord) ? data.MetaKeyWord : model.MetaKeywords;
                model.MetaDescription = !string.IsNullOrEmpty(data.MetaDescription) ? data.MetaDescription : model.MetaDescription;
                model.HeaderCopy = !string.IsNullOrEmpty(data.HeaderCopy) ? data.HeaderCopy : "";
                model.HeaderTitle = !string.IsNullOrEmpty(data.HeaderTitle) ? data.HeaderTitle : "";
                model.H1Tag = !string.IsNullOrEmpty(data.HTag) ? data.HTag : "";
                model.H2Tag = !string.IsNullOrEmpty(data.H2Tag) ? data.H2Tag : "";
                model.CustomKeyword = !string.IsNullOrEmpty(data.KeyWord) ? data.KeyWord : "";

                model.FooterContent1 = !string.IsNullOrEmpty(data.FooterContent1) ? data.FooterContent1 : "";
                model.FooterContent2 = !string.IsNullOrEmpty(data.FooterContent2) ? data.FooterContent2 : "";
                model.FooterContent3 = !string.IsNullOrEmpty(data.FooterContent3) ? data.FooterContent3 : "";

                model.FooterTitle1 = !string.IsNullOrEmpty(data.FooterTitle1) ? data.FooterTitle1 : "";
                model.FooterTitle2 = !string.IsNullOrEmpty(data.FooterTitle2) ? data.FooterTitle2 : "";
                model.FooterTitle3 = !string.IsNullOrEmpty(data.FooterTitle3) ? data.FooterTitle3 : "";

                if (!string.IsNullOrEmpty(model.FooterTitle1) || !string.IsNullOrEmpty(model.FooterContent1))
                    model.FooterContent.Add(new Tuple<string, string>(model.FooterTitle1, model.FooterContent1));
                if (!string.IsNullOrEmpty(model.FooterTitle2) || !string.IsNullOrEmpty(model.FooterContent2))
                    model.FooterContent.Add(new Tuple<string, string>(model.FooterTitle2, model.FooterContent2));
                if (!string.IsNullOrEmpty(model.FooterTitle3) || !string.IsNullOrEmpty(model.FooterContent3))
                    model.FooterContent.Add(new Tuple<string, string>(model.FooterTitle3, model.FooterContent3));
            }
            string pageNumber = System.Web.HttpContext.Current.Request.QueryString["pageNumber"];
            string canonicalUrl = "";
            //if (string.IsNullOrEmpty(Convert.ToString(pageNumber)) || Convert.ToString(pageNumber) == "1")
            //{
            //    canonicalUrl = model.CustomKeyword + "_" + category.GetSeName();
            //}
            //else
            //{
            //    canonicalUrl = filterSeoUrl;
            //}
            if (!string.IsNullOrEmpty(model.CustomKeyword))
                model.CustomKeyword = model.CustomKeyword.Replace(" ", "-");

            if (string.IsNullOrEmpty(filterSeoUrl)) 
                canonicalUrl = model.CustomKeyword + "_" + category.GetSeName();
            else
                canonicalUrl = filterSeoUrl;


            model.CanonicalUrl = canonicalUrl;
            //model
           

            var templateViewPath = "CategoryTemplate.ProductsInGridOrLines";// _catalogModelFactory.PrepareCategoryTemplateViewPath(category.CategoryTemplateId);
            return View(templateViewPath, model);
        }

        [ChildActionOnly]
        public virtual ActionResult CategoryNavigation(int currentCategoryId, int currentProductId)
        {
            var model = _catalogModelFactory.PrepareCategoryNavigationModel(currentCategoryId, currentProductId);
            return PartialView(model);
        }

        [ChildActionOnly]
        public virtual ActionResult TopMenu()
        {
            var model = _catalogModelFactory.PrepareTopMenuModel();
            return PartialView(model);
        }

        [ChildActionOnly]
        public virtual ActionResult HomepageCategories()
        {
            var model = _catalogModelFactory.PrepareHomepageCategoryModels();
            if (!model.Any())
                return Content("");

            return PartialView(model);
        }

        #endregion

        #region Manufacturers

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult Manufacturer(int manufacturerId, CatalogPagingFilteringModel command)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null || manufacturer.Deleted)
                return InvokeHttp404();

            var notAvailable =
                //published?
                !manufacturer.Published ||
                //ACL (access control list) 
                !_aclService.Authorize(manufacturer) ||
                //Store mapping
                !_storeMappingService.Authorize(manufacturer);
            //Check whether the current user has a "Manage categories" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            if (notAvailable && !_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                DisplayEditLink(Url.Action("Edit", "Manufacturer", new { id = manufacturer.Id, area = "Admin" }));

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewManufacturer", _localizationService.GetResource("ActivityLog.PublicStore.ViewManufacturer"), manufacturer.Name);

            //model
            var model = _catalogModelFactory.PrepareManufacturerModel(manufacturer, command);

            //template
            var templateViewPath = _catalogModelFactory.PrepareManufacturerTemplateViewPath(manufacturer.ManufacturerTemplateId);
            return View(templateViewPath, model);
        }

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult ManufacturerAll()
        {
            var model = _catalogModelFactory.PrepareManufacturerAllModels();
            return View(model);
        }

        [ChildActionOnly]
        public virtual ActionResult ManufacturerNavigation(int currentManufacturerId)
        {
            if (_catalogSettings.ManufacturersBlockItemsToDisplay == 0)
                return Content("");

            var model = _catalogModelFactory.PrepareManufacturerNavigationModel(currentManufacturerId);

            if (!model.Manufacturers.Any())
                return Content("");

            return PartialView(model);
        }

        #endregion

        #region Vendors

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult Vendor(int vendorId, CatalogPagingFilteringModel command)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null || vendor.Deleted || !vendor.Active)
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                DisplayEditLink(Url.Action("Edit", "Vendor", new { id = vendor.Id, area = "Admin" }));

            //model
            var model = _catalogModelFactory.PrepareVendorModel(vendor, command);

            return View(model);
        }

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult VendorAll()
        {
            //we don't allow viewing of vendors if "vendors" block is hidden
            if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
                return RedirectToRoute("HomePage");

            var model = _catalogModelFactory.PrepareVendorAllModels();
            return View(model);
        }

        [ChildActionOnly]
        public virtual ActionResult VendorNavigation()
        {
            if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
                return Content("");

            var model = _catalogModelFactory.PrepareVendorNavigationModel();
            if (!model.Vendors.Any())
                return Content("");

            return PartialView(model);
        }

        #endregion

        #region Product tags

        [ChildActionOnly]
        public virtual ActionResult PopularProductTags()
        {
            var model = _catalogModelFactory.PreparePopularProductTagsModel();

            if (!model.Tags.Any())
                return Content("");

            return PartialView(model);
        }

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult ProductsByTag(int productTagId, CatalogPagingFilteringModel command)
        {
            var productTag = _productTagService.GetProductTagById(productTagId);
            if (productTag == null)
                return InvokeHttp404();

            var model = _catalogModelFactory.PrepareProductsByTagModel(productTag, command);
            return View(model);
        }

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult ProductTagsAll()
        {
            var model = _catalogModelFactory.PrepareProductTagsAllModel();
            return View(model);
        }

        #endregion

        #region Searching

        [NopHttpsRequirement(SslRequirement.No)]
        [ValidateInput(false)]
        public virtual ActionResult Search(SearchModel model, CatalogPagingFilteringModel command)
        {
            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            if (model == null)
                model = new SearchModel();

            model = _catalogModelFactory.PrepareSearchModel(model, command);
            return View(model);
        }

        [ChildActionOnly]
        public virtual ActionResult SearchBox()
        {
            var model = _catalogModelFactory.PrepareSearchBoxModel();
            return PartialView(model);
        }

        [ValidateInput(false)]
        public virtual ActionResult SearchTermAutoComplete(string term)
        {
            if (String.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return Content("");

            //products
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                keywords: term,
                languageId: _workContext.WorkingLanguage.Id,
                visibleIndividuallyOnly: true,
                pageSize: productNumber);

            var models = _productModelFactory.PrepareProductOverviewModels(products, false, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize).ToList();
            var result = (from p in models
                          select new
                          {
                              label = p.Name,
                              producturl = Url.RouteUrl("Product", new { SeName = p.SeName }),
                              productpictureurl = p.DefaultPictureModel.ImageUrl
                          })
                          .ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
