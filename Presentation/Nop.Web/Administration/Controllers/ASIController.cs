using Nop.Admin.Extensions;
using Nop.Admin.Models.ASI;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.ASI;
using Nop.Core.Domain.Tasks;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.ASI.Product;
using Nop.Services.ASI.Suppliers;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Tasks;
using Nop.Web.Framework;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Nop.Admin.Controllers
{
    [Web.Framework.Security.AdminAntiForgery(true)]
    public class ASIController : BaseAdminController
    {
        // GET: ASI

        #region Fields

        private readonly IASI_ProductsCSVGenerationRequestService _asi_ProductsCSVGenerationRequestService;
        private readonly IRepository<ASI_ProductsCSVGenerationRequests> _asi_ProductsCSVGenerationRequestsRepository;
        private readonly IRepository<ASI_Suppliers> _asi_SuppliersRespository;
        private readonly IRepository<ASI_SuppliersUpdateStatus> _ASI_SuppliersUpdateStatusRepository;
        private readonly ICategoryService _categoryService;
        private readonly IDbContext _dbContext;
        private readonly IRepository<ScheduleTask> _scheduleTaskRepository;
        private readonly IScheduleTaskService _ScheduleTaskService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ILogger _logger;
        private readonly IPermissionService _permissionService;
        private readonly IRepository<ASI_ProductsSearchOptions> _asi_ProductsSearchOptionsRepository;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion Fields

        #region Constructors

        public ASIController(IScheduleTaskService ScheduleTaskService
            , IRepository<ASI_SuppliersUpdateStatus> ASI_SuppliersUpdateStatusRepository
            , IRepository<ScheduleTask> scheduleTaskRepository
            , IASI_ProductsCSVGenerationRequestService asi_ProductsCSVGenerationRequestService
            , IRepository<ASI_Suppliers> asi_SuppliersRespository
            , ICategoryService categoryService
            , IDbContext dbContext, IRepository<ASI_ProductsCSVGenerationRequests> asi_ProductsCSVGenerationRequestsRepository
            , IManufacturerService manufacturerService
            ,ILogger logger,
            IPermissionService permissionService,
            IRepository<ASI_ProductsSearchOptions> ASI_ProductsSearchOptionsRepository,
            ILocalizationService localizationService, IDateTimeHelper dateTimeHelper)
        {
            this._ScheduleTaskService = ScheduleTaskService;
            this._ASI_SuppliersUpdateStatusRepository = ASI_SuppliersUpdateStatusRepository;
            this._scheduleTaskRepository = scheduleTaskRepository;
            this._asi_ProductsCSVGenerationRequestService = asi_ProductsCSVGenerationRequestService;
            this._asi_SuppliersRespository = asi_SuppliersRespository;
            this._categoryService = categoryService;
            this._dbContext = dbContext;
            this._asi_ProductsCSVGenerationRequestsRepository = asi_ProductsCSVGenerationRequestsRepository;
            this._manufacturerService = manufacturerService;
            this._logger = logger;
            this._permissionService = permissionService;
            this._asi_ProductsSearchOptionsRepository = ASI_ProductsSearchOptionsRepository;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion Constructors

        public ActionResult Index()
        {
            //return View();
            return RedirectToAction("ASIProductProdcess");
        }

        #region Product

        public ActionResult ASIProduct(int? id)
        {
            if (_asi_ProductsCSVGenerationRequestService.IsCSVGenerationRequestRunning())
                return RedirectToAction("ASIProductProdcess");
            //var categories = false;
            //if (id != null && id.Value == (int)ProductsSearchOptionType.Category)
            //{
            //    categories = true;
            //}
            var model = new SearchCategorySupplier();
            model.AvailAbleSearchOptionType.Add(new SelectListItem { Text = "Suppliers", Value = "0" });
            model.AvailAbleSearchOptionType.Add(new SelectListItem { Text = "Categories", Value = "1" });
            return View(model);
            //return View(categories);
        }

        [HttpPost]
        public ActionResult ASIProduct(List<ASI_ProductsSearchOptions> options)
        {
            var runningRequest = _asi_ProductsCSVGenerationRequestService.GetCurrentRunningRequest();
            if (runningRequest == null && options != null)
            {
                ASI_ProductsCSVGenerationRequests request = new ASI_ProductsCSVGenerationRequests();
                request.Status = ProductsCSVGenerationStatus.Started;
                request.SuccessfullyRetrivedProductsCount = -1;
                request.SearchOptions = new List<ASI_ProductsSearchOptions>();
                request.AddedDate = DateTime.Now;
                request.ModifiedDate = DateTime.Now;

                _dbContext.ExecuteSqlCommand("truncate table [dbo].[ASI_ProductsAddedToCSV]");
                foreach (var option in options)
                {
                    option.TotalRecords = -1;
                    option.AddedDate = DateTime.Now;
                    if (option.SearchOptionType == ProductsSearchOptionType.Category)
                    {
                        long categoryId = 0;
                        long.TryParse(option.SearchValue, out categoryId);
                        var category = _categoryService.GetAllASICategories().Where(x => x.Id == categoryId).FirstOrDefault();
                        if (category != null)
                        {
                            option.SearchValue = category.Name;
                            request.SearchOptions.Add(option);
                        }
                    }
                    else if (option.SearchOptionType == ProductsSearchOptionType.Supplier)
                    {
                        long supplierId = 0;
                        long.TryParse(option.SearchValue, out supplierId);
                        var supplier = _manufacturerService.GetAllASIManufacturers().Where(x => x.Id == supplierId).FirstOrDefault();
                        if (supplier != null)
                        {
                            option.SearchValue = supplier.Name;
                            request.SearchOptions.Add(option);
                        }
                    }
                }

                _asi_ProductsCSVGenerationRequestsRepository.Insert(request);

                var scheduleTask = _ScheduleTaskService.GetTaskByType(typeof(ProductTask).FullName);
                if (scheduleTask == null)
                {
                    scheduleTask = new ScheduleTask();
                    scheduleTask.Name = "Products";
                    scheduleTask.Seconds = 0;
                    scheduleTask.Type = typeof(ProductTask).FullName;
                    scheduleTask.Enabled = true;
                    scheduleTask.StopOnError = false;
                    scheduleTask.LastStartUtc = DateTime.UtcNow;
                    scheduleTask.LastEndUtc = DateTime.UtcNow;
                    scheduleTask.LastSuccessUtc = DateTime.UtcNow;
                    _scheduleTaskRepository.Insert(scheduleTask);
                }
                else
                {
                    scheduleTask.Enabled = true;
                    _scheduleTaskRepository.Update(scheduleTask);
                }
            }
            return RedirectToAction("ASIProduct");
        }

        public ActionResult ASIProductProdcess()
        {
            ProductsTaskStatusModel model = new ProductsTaskStatusModel();
            model.TaskRunning = false;
            model.SearchOptions = new List<ProductsSearchOptions>();
            var csvGenerationRequest = _asi_ProductsCSVGenerationRequestsRepository.Table.OrderByDescending(x => x.Id).FirstOrDefault();
            if (csvGenerationRequest != null)
            {
                if (csvGenerationRequest.Status != ProductsCSVGenerationStatus.Completed && csvGenerationRequest.Status != ProductsCSVGenerationStatus.Failed)
                {
                    model.TaskRunning = true;
                }
                model.SearchOptionCount = csvGenerationRequest.SearchOptions.Count();
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult GetProductTaskStatus(DataSourceRequest command)
        {
            var SearchOptions = new List<ProductsSearchOptions>();
            var csvGenerationRequest = _asi_ProductsCSVGenerationRequestsRepository.Table.OrderByDescending(x => x.Id).FirstOrDefault();

            var searchOptionList = _asi_ProductsSearchOptionsRepository.Table.OrderByDescending(x => x.Id).ToList();
            if (csvGenerationRequest != null)
            {
                foreach (var option in csvGenerationRequest.SearchOptions)
                //foreach (var option in searchOptionList)
                {
                    ProductsSearchOptions searchoption = new ProductsSearchOptions();
                    searchoption.Keyword = option.SearchValue;
                    searchoption.Type = option.SearchOptionType.ToString();
                    searchoption.Status = option.Status.ToString();
                    // if search is done in current option then
                    if (option.Status == CurrentOptionStatus.Completed)
                    {
                        searchoption.TotalRecordsFound = option.TotalRecords;
                        //searchoption.TotalRecordsRetrived = option.TotalRecords;
                        searchoption.TotalRecordsRetrived = option.Retrivedproducts;
                    }
                    //if current option is started and atleast we have retrived one option then
                    else if (option.CurrentPage - 1 > 0)
                    {
                        searchoption.TotalRecordsFound = option.TotalRecords;
                        //searchoption.TotalRecordsRetrived = (option.CurrentPage - 1) * csvGenerationRequest.RecordsPerPage;
                        searchoption.TotalRecordsRetrived = option.Retrivedproducts;
                    }
                    //if we didnt retrived atleast one page records
                    else
                    {
                        // searchoption.Status = CurrentOptionStatus.WaitingToRun.ToString();
                        searchoption.TotalRecordsFound = option.TotalRecords;
                        searchoption.TotalRecordsRetrived = option.Retrivedproducts;
                    }
                    if (option.AddedDate != null && option.ModifiedDate!=null)
                    {
                        var diff = (option.ModifiedDate - option.AddedDate);
                        searchoption.TimeTaken += (diff.Days > 0 ? "" + diff.Days + " day(s)" : "");
                        searchoption.TimeTaken += (diff.Hours > 0 ? "" + diff.Hours + " hour(s)" : "");
                        searchoption.TimeTaken += (diff.Minutes > 0 ? "" + diff.Minutes + " minute(s)" : "");
                        searchoption.TimeTaken += (diff.Seconds > 0 ? "" + diff.Seconds + " second(s)" : "");
                    }
                    searchoption.AddedDate = (DateTime)_dateTimeHelper.ConvertToUserTime(option.AddedDate, _dateTimeHelper.CurrentTimeZone);
                    searchoption.Id = option.Id;
                    SearchOptions.Add(searchoption);
                }
            }

            var resources = SearchOptions;
            var records = resources.PagedForCommand(command).ToList();
            var gridModel = new DataSourceResult
            {
                Data = records,
                Total = resources.Count()
            };
            return Json(gridModel);
        }

        #endregion Product

        #region Supplier

        [HttpPost]
        public virtual ActionResult GetSuppliers(DataSourceRequest command)
        {
            var resources = _asi_SuppliersRespository.Table.ToList();
            var records = resources.PagedForCommand(command).ToList();
            var gridModel = new DataSourceResult
            {
                Data = records,
                Total = resources.Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public virtual ActionResult GetManuFacturer(DataSourceRequest command,SearchCategorySupplier model)
        {

            var manufacturers = _manufacturerService.GetAllASIManufacturers(model.SearchName, 0, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = manufacturers.Select(x => x.ToModel()),
                Total = manufacturers.TotalCount
            };
            return Json(gridModel);
        }

        public ActionResult Suppliers()
        {
            var currentStatus = _ASI_SuppliersUpdateStatusRepository.Table.OrderByDescending(x => x.Id).FirstOrDefault();
            return View(currentStatus);
        }

        [HttpPost]
        public ActionResult Suppliers(string data)
        {
            var scheduleTask = _ScheduleTaskService.GetTaskByType(typeof(SupplierTask).FullName);
            ASI_SuppliersUpdateStatus supplierUpdateStatus = new ASI_SuppliersUpdateStatus();
            supplierUpdateStatus.Status = SupplierUpdateStatus.Started;
            supplierUpdateStatus.SuccessfullyRetrivedRcordCount = -1;
            supplierUpdateStatus.TotalPages = -1;
            supplierUpdateStatus.CurrentPage = -1;
            supplierUpdateStatus.RecordsPerPage = -1;
            _ASI_SuppliersUpdateStatusRepository.Insert(supplierUpdateStatus);

            if (scheduleTask == null)
            {
                scheduleTask = new ScheduleTask();
                scheduleTask.Name = "Suppliers";
                scheduleTask.Seconds = 0;
                scheduleTask.Type = typeof(SupplierTask).FullName;
                scheduleTask.Enabled = true;
                scheduleTask.StopOnError = false;
                scheduleTask.LastStartUtc = DateTime.UtcNow;
                scheduleTask.LastEndUtc = DateTime.UtcNow;
                scheduleTask.LastSuccessUtc = DateTime.UtcNow;
                _scheduleTaskRepository.Insert(scheduleTask);
            }
            else if (!scheduleTask.Enabled)
            {
                scheduleTask.Enabled = true;
                scheduleTask.LastStartUtc = DateTime.UtcNow;
                _scheduleTaskRepository.Update(scheduleTask);
            }
            return Suppliers();
        }

        #endregion Supplier

        #region Category

        [HttpPost]
        public virtual ActionResult GetAllCategories(DataSourceRequest command,SearchCategorySupplier model)
        {
            var categories = _categoryService.GetAllASICategories(model.SearchName, 0, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = categories.Select(x =>
                {
                    var categoryModel = x.ToModel();
                    categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                    return categoryModel;
                }),
                Total = categories.TotalCount
            };
            return Json(gridModel);
        }

        #endregion Category

        #region Remove Searchoption
        
        public virtual ActionResult DeleteSearchOption(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();
            try
            {
                var searchOption = _asi_ProductsSearchOptionsRepository.Table.Where(x => x.Id == id).FirstOrDefault(); ;
                if (searchOption == null)
                    //No log found with the specified id
                    return RedirectToAction("ASIProductProdcess");
                _asi_ProductsSearchOptionsRepository.Delete(searchOption);

                if (!_asi_ProductsSearchOptionsRepository.Table.Where(x => x.RequestId == searchOption.RequestId).ToList().Any()) {
                    var searchRequest = _asi_ProductsCSVGenerationRequestsRepository.Table.Where(x => x.Id == searchOption.RequestId).FirstOrDefault();
                    if (searchRequest != null) {
                        searchRequest.Status = ProductsCSVGenerationStatus.Completed;
                        _asi_ProductsCSVGenerationRequestsRepository.Update(searchRequest);
                        //_asi_ProductsCSVGenerationRequestsRepository.Delete(searchRequest);
                        //_dbContext.ExecuteSqlCommand("EXEC [ImportASIData] '" + searchRequest.Id + "'", true, 30000);
                    }
                }

                SuccessNotification(_localizationService.GetResource("Admin.ASI.Searchoption.Deleted"));
            }
            catch (Exception ex) {
                ErrorNotification(ex.Message, false);
                SuccessNotification(_localizationService.GetResource("Admin.ASI.Searchoption.NotDeleted"),true);
            }
            return RedirectToAction("ASIProductProdcess");
        }
        #endregion

        #region ASI Request List
        public ActionResult ASIList() {

            ASI_ProductsCSVGenerationRequestsModel model = new ASI_ProductsCSVGenerationRequestsModel();
            model.TaskRunning = false;
            model.SearchRequests = new List<ASI_ProductsCSVGenerationRequests>();
            var csvGenerationRequest = _asi_ProductsCSVGenerationRequestsRepository.Table.OrderByDescending(x => x.Id).ToList();
            if (csvGenerationRequest != null && csvGenerationRequest.Any())
            {
                if (csvGenerationRequest.Where(x=>x.Status != ProductsCSVGenerationStatus.Completed).Any() && csvGenerationRequest.Where(x => x.Status != ProductsCSVGenerationStatus.Failed).Any())
                {
                    model.TaskRunning = true;
                }
                model.SearchRequestsCount = csvGenerationRequest.Count();
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult GetASIList(DataSourceRequest command)
        {
            var SearchRequests = new List<ASI_ProductsCSVGenerationRequests>();
            SearchRequests = _asi_ProductsCSVGenerationRequestsRepository.Table.OrderByDescending(x => x.Id).ToList();

            var resources = SearchRequests;
            var records = resources.PagedForCommand(command).ToList();
            var gridModel = new DataSourceResult
            {
                Data = records,
                Total = resources.Count()
            };
            return Json(gridModel);
        }

        #endregion

        #region List Exist Product
        public ActionResult ExistProductList()
        {

            ASIExistProductModel model = new ASIExistProductModel();
            return View(model);
        }
        public ActionResult GetExistProductList(DataSourceRequest command, ASIExistProductModel model)
        {
            if (string.IsNullOrEmpty(model.SearchName))
                model.SearchName = "";
            if (string.IsNullOrEmpty(model.Productcode))
                model.Productcode = "";
            var products = GetExistProductsList(
               RequestId: 0,
               Manufacturer: model.SearchName,
               Productcode:model.Productcode,
               PageIndex: command.Page - 1,
               PageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = products,
                Total = products.Count()
            };
            gridModel.Total = products.TotalCount;
             return Json(gridModel);
        }

        [HttpPost]
        public virtual ActionResult overrideselected(ICollection<string> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                _dbContext.ExecuteSqlCommand("EXEC [UpdateDuplicateAsiProduct] '" + string.Join(",", selectedIds.ToList()) + "'", true, 30000);
            }

            return Json(new { Result = true });
        }

        
        public virtual ActionResult OverrideExistingProduct(int ASIProductId,int requestid,int productid) {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            try {
                
                _dbContext.ExecuteSqlCommand("EXEC [UpdateDuplicateAsiProduct] '" + ASIProductId.ToString() + "','0','" + requestid + "'", true, 30000);
                SuccessNotification(_localizationService.GetResource("Admin.ASI.Products.Existing.Edit.Success"), true);
            }
            catch (Exception ex) {
                ErrorNotification(ex, false);
                ErrorNotification(_localizationService.GetResource("Admin.ASI.Products.Existing.Edit.Error"), true);
            }
            return RedirectToAction("ExistProductList");
        }
        #endregion


        [HttpPost]
        public virtual ActionResult ignoreselected(ICollection<string> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                _dbContext.ExecuteSqlCommand("EXEC [UpdateDuplicateAsiProduct] '" + string.Join(",", selectedIds.ToList()) + "','1'", true, 30000);
            }

            return Json(new { Result = true });
        }


        public virtual ActionResult ignoreExistingProduct(int ASIProductId, int requestid, int productid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            try
            {

                _dbContext.ExecuteSqlCommand("EXEC [UpdateDuplicateAsiProduct] '" + ASIProductId.ToString() + "','1','" + requestid + "'", true, 30000);
                SuccessNotification(_localizationService.GetResource("Admin.ASI.Products.Existing.Edit.Success"), true);
            }
            catch (Exception ex)
            {
                ErrorNotification(ex, false);
                ErrorNotification(_localizationService.GetResource("Admin.ASI.Products.Existing.Edit.Error"), true);
            }
            return RedirectToAction("ExistProductList");
        }


        #region Method
        public IPagedList<ASIExistProductListModel> GetExistProductsList(int RequestId,string Manufacturer,string Productcode, int PageIndex,int PageSize) {
            
            var pRequestId1 = new SqlParameter("@RequestId", SqlDbType.Int);
            pRequestId1.Value = RequestId;
            
            var pManufacturer1 = new SqlParameter("@Manufacturer", SqlDbType.NVarChar);
            pManufacturer1.Value = Manufacturer;

            var pPageIndex1 = new SqlParameter("@PageIndex", SqlDbType.Int);
            pPageIndex1.Value = PageIndex;

            var pPageSize1 = new SqlParameter("@PageSize", SqlDbType.Int);
            pPageSize1.Value = PageSize;


            var pProductcode = new SqlParameter("@Productcode", SqlDbType.NVarChar);
            pProductcode.Value = Productcode;

            var products = _dbContext.SqlQuery<ASIExistProductListModel>("GetASIExistProduct @RequestId,@Manufacturer,@Productcode,@PageIndex,@PageSize"
                , pRequestId1
                , pManufacturer1
                , pProductcode
                , pPageIndex1
                , pPageSize1
               
                ).ToList();

            int totalRecords = products.Any() ? products.FirstOrDefault().TotalRecords : 0;
            return new PagedList<ASIExistProductListModel>(products, PageIndex, PageSize,totalRecords);
        }
        #endregion
    }
}