using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Security.Captcha;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using System.Dynamic;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using Nop.Core.Infrastructure;
using Nop.Core.Domain.Common;
using Nop.Services.Directory;
using System.IO;
using System.Web;
using Nop.Core.Domain.Media;
using Nop.Services.Media;

namespace Nop.Web.Controllers
{
    public partial class ProductController : BasePublicController
    {
        #region Fields

        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly ICompareProductsService _compareProductsService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IOrderReportService _orderReportService;
        private readonly IOrderService _orderService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly ICacheManager _cacheManager;
        private readonly IDownloadService _downloadService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly MediaSettings _mediaSettings;

        //private readonly IEmailSender _EmailSender;
        //private readonly IEmailAccountService _EmailAccountService;


        #endregion

        #region Constructors

        public ProductController(IProductModelFactory productModelFactory,
            IProductService productService,
            IPictureService pictureService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            ICompareProductsService compareProductsService,
            IWorkflowMessageService workflowMessageService,
            IOrderReportService orderReportService,
            IOrderService orderService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            ICustomerActivityService customerActivityService,
            IEventPublisher eventPublisher,
            CatalogSettings catalogSettings,
            ShoppingCartSettings shoppingCartSettings,
            LocalizationSettings localizationSettings,
            CaptchaSettings captchaSettings,
            ICacheManager cacheManager,
            MediaSettings mediaSettings 
            //IEmailSender emailSender=null,
            //IEmailAccountService  EmailAccountService=null
            )
        {
            this._productModelFactory = productModelFactory;
            this._productService = productService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._recentlyViewedProductsService = recentlyViewedProductsService;
            this._compareProductsService = compareProductsService;
            this._workflowMessageService = workflowMessageService;
            this._orderReportService = orderReportService;
            this._orderService = orderService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;
            this._customerActivityService = customerActivityService;
            this._eventPublisher = eventPublisher;
            this._catalogSettings = catalogSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._localizationSettings = localizationSettings;
            this._captchaSettings = captchaSettings;
            this._cacheManager = cacheManager;
            this._pictureService = pictureService;
            this._mediaSettings = mediaSettings;
            //this._EmailSender = emailSender;
            //this._EmailAccountService = EmailAccountService;

            _downloadService = EngineContext.Current.Resolve<IDownloadService>();
            this._stateProvinceService = EngineContext.Current.Resolve<IStateProvinceService>();
        }

        #endregion

        #region Product details page

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult ProductDetails(int productId, int updatecartitemid = 0, bool PrintMode = false)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted)
                return InvokeHttp404();

            var notAvailable =
                //published?
                (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
                //ACL (access control list) 
                !_aclService.Authorize(product) ||
                //Store mapping
                !_storeMappingService.Authorize(product) ||
                //availability dates
                !product.IsAvailable();
            //Check whether the current user has a "Manage products" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            if (notAvailable && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return InvokeHttp404();

            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return RedirectToRoute("HomePage");

                return RedirectToRoute("Product", new { SeName = parentGroupedProduct.GetSeName() });
            }

            //update existing shopping cart or wishlist  item?
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found?
                if (updatecartitem == null)
                {
                    return RedirectToRoute("Product", new { SeName = product.GetSeName() });
                }
                //is it this product?
                if (product.Id != updatecartitem.ProductId)
                {
                    return RedirectToRoute("Product", new { SeName = product.GetSeName() });
                }
            }

            //save as recently viewed
            _recentlyViewedProductsService.AddProductToRecentlyViewedList(product.Id);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) &&
                _permissionService.Authorize(StandardPermissionProvider.ManageProducts))
            {
                //a vendor should have access only to his products
                if (_workContext.CurrentVendor == null || _workContext.CurrentVendor.Id == product.VendorId)
                {
                    DisplayEditLink(Url.Action("Edit", "Product", new { id = product.Id, area = "Admin" }));
                }
            }

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewProduct", _localizationService.GetResource("ActivityLog.PublicStore.ViewProduct"), product.Name);

            //model
            var model = _productModelFactory.PrepareProductDetailsModel(product, updatecartitem, false);

            //template
            var productTemplateViewPath = _productModelFactory.PrepareProductTemplateViewPath(product);
            model.PrintMode = PrintMode;
            var Address = new Core.Domain.Common.Address();
            model.FillShippingInfo = new AddressModel();

            if (_workContext.CurrentCustomer != null && !_workContext.CurrentCustomer.IsGuest())
            {
                Address = _workContext.CurrentCustomer.ShippingAddress != null ?
                    _workContext.CurrentCustomer.ShippingAddress : _workContext.CurrentCustomer.BillingAddress;

                if (Address == null && _workContext.CurrentCustomer.Addresses.Any())
                    Address = _workContext.CurrentCustomer.Addresses.FirstOrDefault();
            }



            var _addressModelFactory = EngineContext.Current.Resolve<IAddressModelFactory>();
            var _addressSettings = EngineContext.Current.Resolve<AddressSettings>();
            var _countryService = EngineContext.Current.Resolve<ICountryService>();
            _addressModelFactory.PrepareAddressModel(model.FillShippingInfo,
                address: Address,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));
            model.FillShippingInfoVirtual = model.FillShippingInfo;
            model.FillShippingInfoProof = model.FillShippingInfo;
            model.updatecartitemid = updatecartitemid;
            if (PrintMode)
                return View("_PrintProductDetail", model);
            else
                return View(productTemplateViewPath, model);
        }

        public virtual ActionResult ProofRequestForm()
        {

            return View();
        }


        public virtual ActionResult ProofRequestFormSubmit(ProductFormModel Model)
        {
            //var emailaccount = _EmailAccountService.GetEmailAccountById(1);
            //_EmailSender.SendEmail(emailaccount, "Inquiry from Proof Request Form", messageBody, emailaddress, "PromotionalPens", "smpatel@sigmasolve.net", "PromotionalPens Admin");

            dynamic model = new ExpandoObject();
            model = Model;

            if (!string.IsNullOrEmpty(Model.EmailAddress))
                _workflowMessageService.SendProductProofRequestFormMessage(_workContext.WorkingLanguage.Id, Model.EmailAddress.Trim(), Model.FullName.Trim(), "Inquiry from Proof Request Form", "", model);

            return View();
        }
        public virtual ActionResult QuoteRequestForm()
        {
            return View();
        }


        public virtual ActionResult QuoteRequestFormSubmit(ProductFormModel Model)
        {
            var productcode = Model.ProductCode;
            var productcolor = Model.ProductColor;
            var productimprintcolor = Model.ProductImprintColor;
            var productqty = Model.ProductQty;

            var productcode2 = Model.ProductCode2;
            var productcolor2 = Model.ProductColor2;
            var productimprintcolor2 = Model.ProductImprintColor2;
            var productqty2 = Model.ProductQty2;

            var productcode3 = Model.ProductCode3;
            var productcolor3 = Model.ProductColor3;
            var productimprintcolor3 = Model.ProductImprintColor3;
            var productqty3 = Model.ProductQty3;

            var productcode4 = Model.ProductCode4;
            var productcolor4 = Model.ProductColor4;
            var productimprintcolor4 = Model.ProductImprintColor4;
            var productqty4 = Model.ProductQty4;

            var messagedetails = Model.ProductInfo;

            var messageBody = "";

            if (!string.IsNullOrEmpty(productcode))
            {
                if (!string.IsNullOrEmpty(productcode))
                    messageBody += "<p>Product Code: " + productcode + "</p>";
                if (!string.IsNullOrEmpty(productcolor))
                    messageBody += "<p>Product Color: " + productcolor + "</p>";
                if (!string.IsNullOrEmpty(productimprintcolor))
                    messageBody += "<p>Product Imprint Color: " + productimprintcolor + "</p>";
                if (!string.IsNullOrEmpty(productqty))
                    messageBody += "<p>Product Qty: " + productqty + "</p>";
            }

            if (!string.IsNullOrEmpty(productcode2))
            {
                if (!string.IsNullOrEmpty(productcode2))
                    messageBody += "<p>Product Code: " + productcode2 + "</p>";
                if (!string.IsNullOrEmpty(productcolor2))
                    messageBody += "<p>Product Color: " + productcolor2 + "</p>";
                if (!string.IsNullOrEmpty(productimprintcolor2))
                    messageBody += "<p>Product Imprint Color: " + productimprintcolor2 + "</p>";
                if (!string.IsNullOrEmpty(productqty))
                    messageBody += "<p>Product Qty: " + productqty2 + "</p>";
            }

            if (!string.IsNullOrEmpty(productcode3))
            {
                if (!string.IsNullOrEmpty(productcode3))
                    messageBody += "<p>Product Code: " + productcode3 + "</p>";
                if (!string.IsNullOrEmpty(productcolor3))
                    messageBody += "<p>Product Color: " + productcolor3 + "</p>";
                if (!string.IsNullOrEmpty(productimprintcolor3))
                    messageBody += "<p>Product Imprint Color: " + productimprintcolor3 + "</p>";
                if (!string.IsNullOrEmpty(productqty3))
                    messageBody += "<p>Product Qty: " + productqty3 + "</p>";
            }

            if (!string.IsNullOrEmpty(productcode4))
            {
                if (!string.IsNullOrEmpty(productcode4))
                    messageBody += "<p>Product Code: " + productcode4 + "</p>";
                if (!string.IsNullOrEmpty(productcolor4))
                    messageBody += "<p>Product Color: " + productcolor4 + "</p>";
                if (!string.IsNullOrEmpty(productimprintcolor4))
                    messageBody += "<p>Product Imprint Color: " + productimprintcolor4 + "</p>";
                if (!string.IsNullOrEmpty(productqty4))
                    messageBody += "<p>Product Qty: " + productqty4 + "</p>";
            }

            //var emailaccount = _EmailAccountService.GetEmailAccountById(1);
            //_EmailSender.SendEmail(emailaccount, "Inquiry from Quote Request Form", messageBody, emailaddress, "PromotionalPens", emailaccount.Email, "PromotionalPens Admin");

            dynamic model = new ExpandoObject();
            model = Model;

            if (!string.IsNullOrEmpty(Model.EmailAddress))
                _workflowMessageService.SendProductQuoteRequestFormMessage(_workContext.WorkingLanguage.Id, Model.EmailAddress.Trim(), Model.FullName.Trim(), "Inquiry from Quote Request Form", messageBody, model);

            return View();
        }


        public virtual ActionResult SampleRequestForm()
        {
            return View();
        }

        public virtual ActionResult SampleRequestFormSubmit(ProductFormModel Model)
        {
            bool IsValid = true;
            if (string.IsNullOrEmpty(Model.FullName))
            {
                ModelState.AddModelError("", "Provide name.");
                IsValid = false;
            }

            if (string.IsNullOrEmpty(Model.EmailAddress))
            {
                ModelState.AddModelError("", "Provide e-mail address.");
                IsValid = false;
            }
            if (!IsValid)
            {
                return View("SampleRequestForm", Model);
            }
            var messageBody = "";

            if (!string.IsNullOrEmpty(Model.ProductCode))
            {
                if (!string.IsNullOrEmpty(Model.ProductCode))
                    messageBody += "<p>Product Code: " + Model.ProductCode + "</p>";
                if (!string.IsNullOrEmpty(Model.ProductColor))
                    messageBody += "<p>Product Color: " + Model.ProductColor + "</p>";
            }
            if (!string.IsNullOrEmpty(Model.ProductCode1))
            {
                if (!string.IsNullOrEmpty(Model.ProductCode1))
                    messageBody += "<p>Product Code: " + Model.ProductCode1 + "</p>";
                if (!string.IsNullOrEmpty(Model.ProductColor1))
                    messageBody += "<p>Product Color: " + Model.ProductColor1 + "</p>";
            }
            if (!string.IsNullOrEmpty(Model.ProductCode2))
            {
                if (!string.IsNullOrEmpty(Model.ProductCode2))
                    messageBody += "<p>Product Code: " + Model.ProductCode2 + "</p>";
                if (!string.IsNullOrEmpty(Model.ProductColor2))
                    messageBody += "<p>Product Color: " + Model.ProductColor2 + "</p>";
            }
            if (!string.IsNullOrEmpty(Model.ProductCode3))
            {
                if (!string.IsNullOrEmpty(Model.ProductCode3))
                    messageBody += "<p>Product Code: " + Model.ProductCode3 + "</p>";
                if (!string.IsNullOrEmpty(Model.ProductColor3))
                    messageBody += "<p>Product Color: " + Model.ProductColor3 + "</p>";
            }
            if (!string.IsNullOrEmpty(Model.ProductCode4))
            {
                if (!string.IsNullOrEmpty(Model.ProductCode4))
                    messageBody += "<p>Product Code: " + Model.ProductCode4 + "</p>";
                if (!string.IsNullOrEmpty(Model.ProductColor4))
                    messageBody += "<p>Product Color: " + Model.ProductColor4 + "</p>";
            }
            if (!string.IsNullOrEmpty(Model.ProductCode5))
            {
                if (!string.IsNullOrEmpty(Model.ProductCode5))
                    messageBody += "<p>Product Code: " + Model.ProductCode5 + "</p>";
                if (!string.IsNullOrEmpty(Model.ProductColor5))
                    messageBody += "<p>Product Color: " + Model.ProductColor5 + "</p>";
            }

            dynamic model = new ExpandoObject();
            model = Model;

            //var emailaccount = _EmailAccountService.GetEmailAccountById(1);
            //_EmailSender.SendEmail(emailaccount, "Inquiry from Sample Request Form", messageBody, emailaddress, "PromotionalPens", "smpatel@sigmasolve.net", "PromotionalPens Admin");
            if (!string.IsNullOrEmpty(Model.EmailAddress))
                _workflowMessageService.SendProductSampleRequestFormMessage(_workContext.WorkingLanguage.Id, Model.EmailAddress.Trim(), Model.FullName.Trim(), "Inquiry from Sample Request Form", messageBody, model);
            return View();
        }

        [ChildActionOnly]
        public virtual ActionResult RelatedProducts(int productId, int? productThumbPictureSize)
        {
            //load and cache report
            var productIds = _cacheManager.Get(string.Format(ModelCacheEventConsumer.PRODUCTS_RELATED_IDS_KEY, productId, _storeContext.CurrentStore.Id),
                () =>
                    _productService.GetRelatedProductsByProductId1(productId).Select(x => x.ProductId2).ToArray()
                    );

            //load products
            var products = _productService.GetProductsByIds(productIds);
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return Content("");

            var model = _productModelFactory.PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList();
            return PartialView(model);
        }

        [ChildActionOnly]
        public virtual ActionResult ProductsAlsoPurchased(int productId, int? productThumbPictureSize)
        {
            if (!_catalogSettings.ProductsAlsoPurchasedEnabled)
                return Content("");

            //load and cache report
            var productIds = _cacheManager.Get(string.Format(ModelCacheEventConsumer.PRODUCTS_ALSO_PURCHASED_IDS_KEY, productId, _storeContext.CurrentStore.Id),
                () =>
                    _orderReportService
                    .GetAlsoPurchasedProductsIds(_storeContext.CurrentStore.Id, productId, _catalogSettings.ProductsAlsoPurchasedNumber)
                    );

            //load products
            var products = _productService.GetProductsByIds(productIds);
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return Content("");

            var model = _productModelFactory.PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList();
            return PartialView(model);
        }

        [ChildActionOnly]
        public virtual ActionResult CrossSellProducts(int? productThumbPictureSize)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var products = _productService.GetCrosssellProductsByShoppingCart(cart, _shoppingCartSettings.CrossSellsNumber);
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return Content("");


            //Cross-sell products are dispalyed on the shopping cart page.
            //We know that the entire shopping cart page is not refresh
            //even if "ShoppingCartSettings.DisplayCartAfterAddingProduct" setting  is enabled.
            //That's why we force page refresh (redirect) in this case
            var model = _productModelFactory.PrepareProductOverviewModels(products,
                productThumbPictureSize: productThumbPictureSize, forceRedirectionAfterAddingToCart: true)
                .ToList();

            return PartialView(model);
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult PrintProductDetils(int Id)
        {
            return RedirectToAction("ProductDetails", new { productId = Id, updatecartitemid = 0, PrintMode = true });
        }

        #endregion

        #region Recently viewed products

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult RecentlyViewedProducts()
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return Content("");

            var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_catalogSettings.RecentlyViewedProductsNumber);

            var model = new List<ProductOverviewModel>();
            model.AddRange(_productModelFactory.PrepareProductOverviewModels(products));

            return View(model);
        }

        [ChildActionOnly]
        public virtual ActionResult RecentlyViewedProductsBlock(int? productThumbPictureSize, bool? preparePriceModel)
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return Content("");

            var preparePictureModel = productThumbPictureSize.HasValue;
            var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_catalogSettings.RecentlyViewedProductsNumber);

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return Content("");

            //prepare model
            var model = new List<ProductOverviewModel>();
            model.AddRange(_productModelFactory.PrepareProductOverviewModels(products,
                preparePriceModel.GetValueOrDefault(),
                preparePictureModel,
                productThumbPictureSize));

            return PartialView(model);
        }

        #endregion

        #region New (recently added) products page

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult NewProducts()
        {
            if (!_catalogSettings.NewProductsEnabled)
                return Content("");

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                markedAsNewOnly: true,
                orderBy: ProductSortingEnum.CreatedOn,
                pageSize: _catalogSettings.NewProductsNumber);

            var model = new List<ProductOverviewModel>();
            model.AddRange(_productModelFactory.PrepareProductOverviewModels(products));

            return View(model);
        }

        public virtual ActionResult NewProductsRss()
        {
            var feed = new SyndicationFeed(
                                    string.Format("{0}: New products", _storeContext.CurrentStore.GetLocalized(x => x.Name)),
                                    "Information about products",
                                    new Uri(_webHelper.GetStoreLocation(false)),
                                    string.Format("urn:store:{0}:newProducts", _storeContext.CurrentStore.Id),
                                    DateTime.UtcNow);

            if (!_catalogSettings.NewProductsEnabled)
                return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));

            var items = new List<SyndicationItem>();

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                markedAsNewOnly: true,
                orderBy: ProductSortingEnum.CreatedOn,
                pageSize: _catalogSettings.NewProductsNumber);
            foreach (var product in products)
            {
                string productUrl = Url.RouteUrl("Product", new { SeName = product.GetSeName() }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");
                string productName = product.GetLocalized(x => x.Name);
                string productDescription = product.GetLocalized(x => x.ShortDescription);
                var item = new SyndicationItem(productName, productDescription, new Uri(productUrl), String.Format("urn:store:{0}:newProducts:product:{1}", _storeContext.CurrentStore.Id, product.Id), product.CreatedOnUtc);
                items.Add(item);
                //uncomment below if you want to add RSS enclosure for pictures
                //var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                //if (picture != null)
                //{
                //    var imageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductDetailsPictureSize);
                //    item.ElementExtensions.Add(new XElement("enclosure", new XAttribute("type", "image/jpeg"), new XAttribute("url", imageUrl)).CreateReader());
                //}

            }
            feed.Items = items;
            return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));
        }

        #endregion

        #region Home page latest reviews
        [ChildActionOnly]
        public virtual ActionResult HomepageLatestReview(int? productThumbPictureSize)
        {
            if (!_catalogSettings.ShowBestsellersOnHomepage || _catalogSettings.NumberOfBestsellersOnHomepage == 0)
                return Content("");

            var reviews = _productService.GetLatestReviews();


            if (!reviews.Any())
                return Content("");

            dynamic dmodel = new List<ExpandoObject>();
            foreach (var item in reviews)
            {
                dynamic dm = new ExpandoObject();
                dm.Products = item.Product;
                dm.SeName = item.Product.GetSeName();
                dm.DefaultPictureModel = _productModelFactory.PrepareProductOverviewPictureModel(item.Product);
                dm.ProductReview = item;
                dmodel.Add(dm);
            }
            //prepare model
            //var model = _productModelFactory.PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList();
            return PartialView("HomepageLatestReview", dmodel);
        }
        #endregion

        #region Home page bestsellers and products

        [ChildActionOnly]
        public virtual ActionResult HomepageBestSellers(int? productThumbPictureSize)
        {
            if (!_catalogSettings.ShowBestsellersOnHomepage || _catalogSettings.NumberOfBestsellersOnHomepage == 0)
                return Content("");

            //load and cache report
            var report = _cacheManager.Get(string.Format(ModelCacheEventConsumer.HOMEPAGE_BESTSELLERS_IDS_KEY, _storeContext.CurrentStore.Id),
                () => _orderReportService.BestSellersReport(
                    storeId: _storeContext.CurrentStore.Id,
                    pageSize: _catalogSettings.NumberOfBestsellersOnHomepage)
                    .ToList());


            //load products
            var products = _productService.GetProductsByIds(report.Select(x => x.ProductId).ToArray());
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return Content("");

            //prepare model
            var model = _productModelFactory.PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList();
            return PartialView(model);
        }




        [ChildActionOnly]
        public virtual ActionResult HomepageProducts(int? productThumbPictureSize)
        {
            var products = _productService.GetAllProductsDisplayedOnHomePage();
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return Content("");

            var model = _productModelFactory.PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList();
            return PartialView(model);
        }

        #endregion

        #region Product reviews

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult ProductReviews(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            var model = new ProductReviewsModel();
            model = _productModelFactory.PrepareProductReviewsModel(model, product);
            //only registered users can leave reviews
            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                ModelState.AddModelError("", _localizationService.GetResource("Reviews.OnlyRegisteredUsersCanWriteReviews"));

            if (_catalogSettings.ProductReviewPossibleOnlyAfterPurchasing &&
                !_orderService.SearchOrders(customerId: _workContext.CurrentCustomer.Id, productId: productId, osIds: new List<int> { (int)OrderStatus.Complete }).Any())
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));

            //default value
            model.AddProductReview.Rating = _catalogSettings.DefaultProductRatingValue;
            return View(model);
        }

        [HttpPost, ActionName("ProductReviews")]
        [PublicAntiForgery]
        [FormValueRequired("add-review")]
        [CaptchaValidator]
        public virtual ActionResult ProductReviewsAdd(int productId, ProductReviewsModel model, bool captchaValid)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Reviews.OnlyRegisteredUsersCanWriteReviews"));
            }

            if (_catalogSettings.ProductReviewPossibleOnlyAfterPurchasing &&
                !_orderService.SearchOrders(customerId: _workContext.CurrentCustomer.Id, productId: productId, osIds: new List<int> { (int)OrderStatus.Complete }).Any())
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));

            if (ModelState.IsValid)
            {
                //save review
                int rating = model.AddProductReview.Rating;
                if (rating < 1 || rating > 5)
                    rating = _catalogSettings.DefaultProductRatingValue;
                bool isApproved = !_catalogSettings.ProductReviewsMustBeApproved;

                var productReview = new ProductReview
                {
                    ProductId = product.Id,
                    CustomerId = _workContext.CurrentCustomer.Id,
                    Title = model.AddProductReview.Title,
                    ReviewText = model.AddProductReview.ReviewText,
                    Rating = rating,
                    HelpfulYesTotal = 0,
                    HelpfulNoTotal = 0,
                    IsApproved = isApproved,
                    CreatedOnUtc = DateTime.UtcNow,
                    StoreId = _storeContext.CurrentStore.Id,
                };
                product.ProductReviews.Add(productReview);
                _productService.UpdateProduct(product);

                //update product totals
                _productService.UpdateProductReviewTotals(product);

                //notify store owner
                if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                    _workflowMessageService.SendProductReviewNotificationMessage(productReview, _localizationSettings.DefaultAdminLanguageId);

                //activity log
                _customerActivityService.InsertActivity("PublicStore.AddProductReview", _localizationService.GetResource("ActivityLog.PublicStore.AddProductReview"), product.Name);

                //raise event
                if (productReview.IsApproved)
                    _eventPublisher.Publish(new ProductReviewApprovedEvent(productReview));

                model = _productModelFactory.PrepareProductReviewsModel(model, product);
                model.AddProductReview.Title = null;
                model.AddProductReview.ReviewText = null;

                model.AddProductReview.SuccessfullyAdded = true;
                if (!isApproved)
                    model.AddProductReview.Result = _localizationService.GetResource("Reviews.SeeAfterApproving");
                else
                    model.AddProductReview.Result = _localizationService.GetResource("Reviews.SuccessfullyAdded");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model = _productModelFactory.PrepareProductReviewsModel(model, product);
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult SetProductReviewHelpfulness(int productReviewId, bool washelpful)
        {
            var productReview = _productService.GetProductReviewById(productReviewId);
            if (productReview == null)
                throw new ArgumentException("No product review found with the specified id");

            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
                return Json(new
                {
                    Result = _localizationService.GetResource("Reviews.Helpfulness.OnlyRegistered"),
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });
            }

            //customers aren't allowed to vote for their own reviews
            if (productReview.CustomerId == _workContext.CurrentCustomer.Id)
            {
                return Json(new
                {
                    Result = _localizationService.GetResource("Reviews.Helpfulness.YourOwnReview"),
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });
            }

            //delete previous helpfulness
            var prh = productReview.ProductReviewHelpfulnessEntries
                .FirstOrDefault(x => x.CustomerId == _workContext.CurrentCustomer.Id);
            if (prh != null)
            {
                //existing one
                prh.WasHelpful = washelpful;
            }
            else
            {
                //insert new helpfulness
                prh = new ProductReviewHelpfulness
                {
                    ProductReviewId = productReview.Id,
                    CustomerId = _workContext.CurrentCustomer.Id,
                    WasHelpful = washelpful,
                };
                productReview.ProductReviewHelpfulnessEntries.Add(prh);
            }
            _productService.UpdateProduct(productReview.Product);

            //new totals
            productReview.HelpfulYesTotal = productReview.ProductReviewHelpfulnessEntries.Count(x => x.WasHelpful);
            productReview.HelpfulNoTotal = productReview.ProductReviewHelpfulnessEntries.Count(x => !x.WasHelpful);
            _productService.UpdateProduct(productReview.Product);

            return Json(new
            {
                Result = _localizationService.GetResource("Reviews.Helpfulness.SuccessfullyVoted"),
                TotalYes = productReview.HelpfulYesTotal,
                TotalNo = productReview.HelpfulNoTotal
            });
        }

        public virtual ActionResult CustomerProductReviews(int? page)
        {
            if (_workContext.CurrentCustomer.IsGuest())
                return new HttpUnauthorizedResult();

            if (!_catalogSettings.ShowProductReviewsTabOnAccountPage)
            {
                return RedirectToRoute("CustomerInfo");
            }

            var model = _productModelFactory.PrepareCustomerProductReviewsModel(page);
            return View(model);
        }

        #endregion

        #region Email a friend

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult ProductEmailAFriend(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return RedirectToRoute("HomePage");

            var model = new ProductEmailAFriendModel();
            model = _productModelFactory.PrepareProductEmailAFriendModel(model, product, false);
            return View(model);
        }

        [HttpPost, ActionName("ProductEmailAFriend")]
        [PublicAntiForgery]
        [FormValueRequired("send-email")]
        [CaptchaValidator]
        public virtual ActionResult ProductEmailAFriendSend(ProductEmailAFriendModel model, bool captchaValid)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            //check whether the current customer is guest and ia allowed to email a friend
            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToEmailAFriend)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Products.EmailAFriend.OnlyRegisteredUsers"));
            }

            if (ModelState.IsValid)
            {
                //email
                _workflowMessageService.SendProductEmailAFriendMessage(_workContext.CurrentCustomer,
                        _workContext.WorkingLanguage.Id, product,
                        model.YourEmailAddress, model.FriendEmail,
                        Core.Html.HtmlHelper.FormatText(model.PersonalMessage, false, true, false, false, false, false));

                model = _productModelFactory.PrepareProductEmailAFriendModel(model, product, true);
                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("Products.EmailAFriend.SuccessfullySent");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model = _productModelFactory.PrepareProductEmailAFriendModel(model, product, true);
            return View(model);
        }

        #endregion

        #region Comparing products

        [HttpPost]
        public virtual ActionResult AddProductToCompareList(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published)
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            if (!_catalogSettings.CompareProductsEnabled)
                return Json(new
                {
                    success = false,
                    message = "Product comparison is disabled"
                });

            _compareProductsService.AddProductToCompareList(productId);

            //activity log
            _customerActivityService.InsertActivity("PublicStore.AddToCompareList", _localizationService.GetResource("ActivityLog.PublicStore.AddToCompareList"), product.Name);

            return Json(new
            {
                success = true,
                message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToCompareList.Link"), Url.RouteUrl("CompareProducts"))
                //use the code below (commented) if you want a customer to be automatically redirected to the compare products page
                //redirect = Url.RouteUrl("CompareProducts"),
            });
        }

        public virtual ActionResult RemoveProductFromCompareList(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return RedirectToRoute("HomePage");

            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            _compareProductsService.RemoveProductFromCompareList(productId);

            return RedirectToRoute("CompareProducts");
        }

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult CompareProducts()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            var model = new CompareProductsModel
            {
                IncludeShortDescriptionInCompareProducts = _catalogSettings.IncludeShortDescriptionInCompareProducts,
                IncludeFullDescriptionInCompareProducts = _catalogSettings.IncludeFullDescriptionInCompareProducts,
            };

            var products = _compareProductsService.GetComparedProducts();

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            //prepare model
            _productModelFactory.PrepareProductOverviewModels(products, prepareSpecificationAttributes: true)
                .ToList()
                .ForEach(model.Products.Add);
            return View(model);
        }

        public virtual ActionResult ClearCompareList()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            _compareProductsService.ClearCompareProducts();

            return RedirectToRoute("CompareProducts");
        }

        #endregion



        [FormValueRequired("add-sample-cart")]
        [HttpPost]
        public virtual ActionResult ProductDetails(ProductDetailsModel model, FormCollection form)
        {
            if (_workContext.CurrentCustomer == null)
                return RedirectToRoute("login");

            if (_workContext.CurrentCustomer != null && _workContext.CurrentCustomer.IsGuest())
                return RedirectToRoute("login");

            ProductFormModel data = new ProductFormModel();
            if (model == null || form == null)
            {
                ErrorNotification(_localizationService.GetResource("Product.Details.SmapleCart.Error"));
                return RedirectToAction("ProductDetails", new { productId = model.Id });
            }
            if (model.FillShippingInfo == null)
            {
                ErrorNotification(_localizationService.GetResource("Product.Details.SmapleCart.Error"));
                return RedirectToAction("ProductDetails", new { productId = model.Id });
            }
            data.FullName = model.FillShippingInfo.FirstName + " " + model.FillShippingInfo.LastName;
            data.Company = model.FillShippingInfo.Company;
            data.Address1 = model.FillShippingInfo.Address1;
            data.Address2 = model.FillShippingInfo.Address2;
            data.City = model.FillShippingInfo.City;
            data.Country = model.FillShippingInfo.CountryName;
            var state = _stateProvinceService.GetStateProvinceById(model.FillShippingInfo.StateProvinceId == null ? 0 : Convert.ToInt32(model.FillShippingInfo.StateProvinceId));
            if (state != null)
                data.State = state.Abbreviation;
            else
                data.State = model.FillShippingInfo.StateProvinceName;
            data.Zip = model.FillShippingInfo.ZipPostalCode;
            data.EmailAddress = model.FillShippingInfo.Email;
            data.PhoneNumber = model.FillShippingInfo.PhoneNumber;
            data.ProductCode = form["samplecart-sku"] == null ? "" : Convert.ToString(form["samplecart-sku"]);
            data.ProductName = form["samplecart-product-name"] == null ? "" : Convert.ToString(form["samplecart-product-name"]);
            data.ProductColor = form["SampleCart-ItemColor"] == null ? "" : Convert.ToString(form["SampleCart-ItemColor"]);

            var product = _productService.GetProductById(model.Id);
            if (product != null)
            {
                var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();

                //let's check whether this product has some parent "grouped" product
                if (picture != null)
                {
                    string imageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.CartThumbPictureSize);
                    data.ProductPictureUrl = imageUrl;
                }

           }

            dynamic productFormModel = new ExpandoObject();
            productFormModel = data;

            //var emailaccount = _EmailAccountService.GetEmailAccountById(1);
            //_EmailSender.SendEmail(emailaccount, "Inquiry from Sample Request Form", messageBody, emailaddress, "PromotionalPens", "smpatel@sigmasolve.net", "PromotionalPens Admin");
            if (!string.IsNullOrEmpty(data.EmailAddress))
                _workflowMessageService.SendProductSampleRequestFormMessage(_workContext.WorkingLanguage.Id, data.EmailAddress.Trim(), data.FullName.Trim(), "Inquiry from Sample Request Form", "1", data);

            SuccessNotification(_localizationService.GetResource("Product.Details.SmapleCart.Success"));

            return RedirectToAction("ProductDetails", new { productId = model.Id });
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("ProductDetails")]
        [FormValueRequired("ProofsRequestForm")]
        public virtual ActionResult ProofsRequestForm(ProductDetailsModel model, FormCollection form)
        {
            if (_workContext.CurrentCustomer == null)
                return RedirectToRoute("login");

            if (_workContext.CurrentCustomer != null && _workContext.CurrentCustomer.IsGuest())
                return RedirectToRoute("login");

            ProductFormModel data = new ProductFormModel();
            if (model == null || form == null)
            {
                ErrorNotification(_localizationService.GetResource("Product.Details.ProofRequest.Error"));
                return RedirectToAction("ProductDetails", new { productId = model.Id });
            }
            if (model.FillShippingInfoProof == null)
            {
                ErrorNotification(_localizationService.GetResource("Product.Details.ProofRequest.Error"));
                return RedirectToAction("ProductDetails", new { productId = model.Id });
            }
            data.FullName = model.FillShippingInfoProof.FirstName + " " + model.FillShippingInfoProof.LastName;
            data.Company = model.FillShippingInfoProof.Company;
            data.Address1 = model.FillShippingInfoProof.Address1;
            data.Address2 = model.FillShippingInfoProof.Address2;
            data.City = model.FillShippingInfoProof.City;
            data.Country = model.FillShippingInfoProof.CountryName;
            var state = _stateProvinceService.GetStateProvinceById(model.FillShippingInfoProof.StateProvinceId == null ? 0 : Convert.ToInt32(model.FillShippingInfoProof.StateProvinceId));
            if (state != null)
                data.State = state.Abbreviation;
            else
                data.State = model.FillShippingInfoProof.StateProvinceName;
            data.Zip = model.FillShippingInfoProof.ZipPostalCode;
            data.EmailAddress = model.FillShippingInfoProof.Email;
            data.PhoneNumber = model.FillShippingInfoProof.PhoneNumber;
            data.ProductCode = form["proofrequest-sku"] == null ? "" : Convert.ToString(form["proofrequest-sku"]);
            data.ProductName = form["proofrequest-product-name"] == null ? "" : Convert.ToString(form["proofrequest-product-name"]);
            data.ItemColor = form["Proof-ItemColor"] == null ? "" : Convert.ToString(form["Proof-ItemColor"]);
            data.ImprintColor = form["Proof-ImprintColor"] == null ? "" : Convert.ToString(form["Proof-ImprintColor"]);
            string imprintLines = "";
            if (form["Proof-ImprintText"] != null && !string.IsNullOrEmpty(Convert.ToString(form["Proof-ImprintText"])))
            {
                imprintLines = string.Join(",", Convert.ToString(form["Proof-ImprintText"]).Split(',').Where(x => !string.IsNullOrEmpty(x)));
            }
            data.ImprintText = form["Proof-ImprintText"] == null ? "" : imprintLines;
            data.Comment = form["Proof-Comment"] == null ? "" : Convert.ToString(form["Proof-Comment"]);
            data.ArtUpload = form["Proof-artUpload"] == null ? "" : Convert.ToString(form["Proof-artUpload"]);

            var product = _productService.GetProductById(model.Id);
            if (product != null)
            {
                var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();

                //let's check whether this product has some parent "grouped" product
                if (picture != null)
                {
                    string imageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.CartThumbPictureSize);
                    data.ProductPictureUrl = imageUrl; 
                }

            }

            dynamic productFormModel = new ExpandoObject();
            productFormModel = data;



            if (!string.IsNullOrEmpty(data.EmailAddress))
                _workflowMessageService.SendProductProofRequestFormMessage(_workContext.WorkingLanguage.Id, data.EmailAddress.Trim(), data.FullName.Trim(), "Inquiry from Proof Request Form", "1", data);

            SuccessNotification(_localizationService.GetResource("Product.Details.ProofRequest.Success"));

            return RedirectToAction("ProductDetails", new { productId = model.Id });
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("ProductDetails")]
        [FormValueRequired("QuotesRequestForm")]
        public virtual ActionResult QuotesRequestForm(ProductDetailsModel model, FormCollection form)
        {
            if (_workContext.CurrentCustomer == null)
                return RedirectToRoute("login");

            if (_workContext.CurrentCustomer != null && _workContext.CurrentCustomer.IsGuest())
                return RedirectToRoute("login");

            ProductFormModel data = new ProductFormModel();
            if (model == null || form == null)
            {
                ErrorNotification(_localizationService.GetResource("Product.Details.QuoteRequest.Error"));
                return RedirectToAction("ProductDetails", new { productId = model.Id });
            }
            if (model.FillShippingInfoVirtual == null)
            {
                ErrorNotification(_localizationService.GetResource("Product.Details.QuoteRequest.Error"));
                return RedirectToAction("ProductDetails", new { productId = model.Id });
            }
            data.FullName = model.FillShippingInfoVirtual.FirstName + " " + model.FillShippingInfoVirtual.LastName;
            data.Company = model.FillShippingInfoVirtual.Company;
            data.Address1 = model.FillShippingInfoVirtual.Address1;
            data.Address2 = model.FillShippingInfoVirtual.Address2;
            data.City = model.FillShippingInfoVirtual.City;
            data.Country = model.FillShippingInfoVirtual.CountryName;

            var state = _stateProvinceService.GetStateProvinceById(model.FillShippingInfoVirtual.StateProvinceId == null ? 0 : Convert.ToInt32(model.FillShippingInfoVirtual.StateProvinceId));
            if (state != null)
                data.State = state.Abbreviation;
            else
                data.State = model.FillShippingInfoVirtual.StateProvinceName;

            data.Zip = model.FillShippingInfoVirtual.ZipPostalCode;
            data.EmailAddress = model.FillShippingInfoVirtual.Email;
            data.PhoneNumber = model.FillShippingInfoVirtual.PhoneNumber;
            data.ProductCode = form["Quoterequest-sku"] == null ? "" : Convert.ToString(form["Quoterequest-sku"]);
            data.ProductName = form["Quoterequest-product-name"] == null ? "" : Convert.ToString(form["Quoterequest-product-name"]);
            data.ItemColor = form["Quote-ItemColor"] == null ? "" : Convert.ToString(form["Quote-ItemColor"]);
            data.ImprintColor = form["Quote-ImprintColor"] == null ? "" : Convert.ToString(form["Quote-ImprintColor"]);
            string imprintLines = "";
            if (form["Quote-ImprintText"] != null && !string.IsNullOrEmpty(Convert.ToString(form["Quote-ImprintText"])))
            {
                imprintLines = string.Join(",", Convert.ToString(form["Quote-ImprintText"]).Split(',').Where(x => !string.IsNullOrEmpty(x)));
            }
            data.ImprintText = form["Quote-ImprintText"] == null ? "" : imprintLines;
            data.Comment = form["Quote-Comment"] == null ? "" : Convert.ToString(form["Quote-Comment"]);
            data.ArtUpload = form["Quote-artUpload"] == null ? "" : Convert.ToString(form["Quote-artUpload"]);
            data.Quantity = form["Quote-Quantity"] == null ? "" : Convert.ToString(form["Quote-Quantity"]);

            var product = _productService.GetProductById(model.Id);
            if (product != null)
            {
                var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();

                //let's check whether this product has some parent "grouped" product
                if (picture != null)
                {
                    string imageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.CartThumbPictureSize);
                    data.ProductPictureUrl = imageUrl;
                }

            }

            dynamic productFormModel = new ExpandoObject();
            productFormModel = data;


            if (!string.IsNullOrEmpty(data.EmailAddress))
                _workflowMessageService.SendProductQuoteRequestFormMessage(_workContext.WorkingLanguage.Id, data.EmailAddress.Trim(), data.FullName.Trim(), "Inquiry from Quote Request Form", "1", data);

            SuccessNotification(_localizationService.GetResource("Product.Details.QuoteRequest.Success"));

            return RedirectToAction("ProductDetails", new { productId = model.Id });
        }

        [HttpPost]
        public virtual ActionResult UploadArtWork()
        {
            Stream stream = null;
            var fileName = "";
            var contentType = "";
            if (String.IsNullOrEmpty(Request["qqfile"]))
            {
                // IE
                HttpPostedFileBase httpPostedFile = Request.Files[0];
                if (httpPostedFile == null)
                    throw new ArgumentException("No file uploaded");
                stream = httpPostedFile.InputStream;
                fileName = Path.GetFileName(httpPostedFile.FileName);
                contentType = httpPostedFile.ContentType;
            }
            else
            {
                //Webkit, Mozilla
                stream = Request.InputStream;
                fileName = Request["qqfile"];
            }

            var fileBinary = new byte[stream.Length];
            stream.Read(fileBinary, 0, fileBinary.Length);

            var fileExtension = Path.GetExtension(fileName);
            if (!String.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = Path.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            _downloadService.InsertDownload(download);


            //var picture = _pictureService.InsertPicture(fileBinary, contentType, null);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                downloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                downloadGuid = download.DownloadGuid,

            }, MimeTypes.TextPlain);
        }

    }


}
