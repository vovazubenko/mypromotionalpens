using Nop.Core.Data;
using Nop.Core.Domain.ASI;
using Nop.Services.ASI.Suppliers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Configuration;
using Nop.Data;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using Nop.Core.Domain.Tasks;
using Nop.Services.ASI.Product;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;

namespace Nop.Services.ASI
{
    public class ASI_API
    {

        #region Prop
        private readonly IASI_SuppliersUpdateStatusService _supplierUpdateService;
        private readonly IRepository<ASI_SuppliersUpdateStatus> _ASI_SuppliersUpdateStatusRepository;
        private readonly IDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly ASISetting _asiSetting;
        private readonly IScheduleTaskService _ScheduleTaskService;
        private readonly IRepository<ScheduleTask> _scheduleTaskRepository;
        private readonly IASI_ProductsCSVGenerationRequestService _asi_ProductsCSVGenerationRequestService;

        private readonly IRepository<ASI_Options> _asi_optionsRepository;
        private readonly IRepository<ASI_ProductsAddedToCSV> _aSI_ProductsAddedToCSV;
        private readonly IRepository<ASI_Product> _asi_Product;
        private readonly IRepository<ASI_ProductsCSVGenerationRequests> _asi_ProductsCSVGenerationRequestsRepository;
        private readonly IRepository<ASI_Discounts> _asi_DiscountsRepository;
        private readonly IRepository<ASI_DiscountsApplyTo> _asi_DiscountsApplyToRepository;

        private readonly IRepository<ASI_Picture> _asi_PictureRepository;
        private readonly IRepository<ASI_Product_Category_Mapping> _asi_ProductCategoryMappingRepository;

        #endregion

        #region Fields
        static string ApiUri = "";
        static string ApiApplicationId = "";
        static string ApiApplicationSecret = "";
        static string ResultsPerPage = "";
        static string SearchApiUri = "";
        static string SuplierSearchURL = "";
        static string AuthorizationHeader = "";
        static int ImprtingColorCategoriyId = 4;
        static int ItemColor = 10;
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DataConnectionString"].ConnectionString);
        #endregion

        #region ctor
        public ASI_API(IASI_SuppliersUpdateStatusService supplierUpdateService
            , IRepository<ASI_SuppliersUpdateStatus> ASI_SuppliersUpdateStatusRepository
            , IDbContext dbContext
            , ILogger logger
            , ASISetting asiSetting
            , IScheduleTaskService ScheduleTaskService
            , IRepository<ScheduleTask> scheduleTaskRepository, IASI_ProductsCSVGenerationRequestService asi_ProductsCSVGenerationRequestService
            , IRepository<ASI_Options> asi_optionsRepository
            , IRepository<ASI_ProductsAddedToCSV> aSI_ProductsAddedToCSV
            , IRepository<ASI_Product> asi_Product
            , IRepository<ASI_ProductsCSVGenerationRequests> asi_ProductsCSVGenerationRequestsRepository
            , IRepository<ASI_Discounts> asi_DiscountsRepository
            , IRepository<ASI_DiscountsApplyTo> asi_DiscountsApplyToRepository
            , IRepository<ASI_Picture> asi_PictureRepository
            , IRepository<ASI_Product_Category_Mapping> asi_ProductCategoryMappingRepository)
        {
            this._supplierUpdateService = supplierUpdateService;
            this._ASI_SuppliersUpdateStatusRepository = ASI_SuppliersUpdateStatusRepository;
            this._dbContext = dbContext;
            this._logger = logger;
            this._asiSetting = asiSetting;
            this._ScheduleTaskService = ScheduleTaskService;
            this._scheduleTaskRepository = scheduleTaskRepository;
            this._asi_ProductsCSVGenerationRequestService = asi_ProductsCSVGenerationRequestService;
            this._asi_optionsRepository = asi_optionsRepository;
            this._aSI_ProductsAddedToCSV = aSI_ProductsAddedToCSV;
            this._asi_Product = asi_Product;
            this._asi_ProductsCSVGenerationRequestsRepository = asi_ProductsCSVGenerationRequestsRepository;
            this._asi_DiscountsRepository = asi_DiscountsRepository;
            this._asi_DiscountsApplyToRepository = asi_DiscountsApplyToRepository;
            this._asi_PictureRepository = asi_PictureRepository;
            this._asi_ProductCategoryMappingRepository = asi_ProductCategoryMappingRepository;

            ApiUri = asiSetting.ASIAPIURL;
            ApiApplicationId = asiSetting.ASIApplicationId;
            ApiApplicationSecret = asiSetting.ASIApplicationSecret;
            ResultsPerPage = asiSetting.ResultsPerPage.ToString();

            SearchApiUri = ApiUri + "products/search.json?rpp=" + ResultsPerPage + "&page={0}&include_detail=1&{1}";
            SuplierSearchURL = ApiUri + "suppliers/search.xml?rpp=" + ResultsPerPage + "&page={0}";
            AuthorizationHeader = "AsiMemberAuth client_id=" + ApiApplicationId + "&client_secret=" + ApiApplicationSecret;

        }
        #endregion

        #region Methods

        #region Supplier
        public void GetAllSuppliers(IASI_SuppliersUpdateStatusService supplierUpdateStatusService)
        {
            ASI_SuppliersUpdateStatus supplierUpdateStatus = new ASI_SuppliersUpdateStatus();
            if (supplierUpdateStatusService.GetRunningSupplierTaskCount() > 0)
                return;
            supplierUpdateStatus = _ASI_SuppliersUpdateStatusRepository.Table.Where(x => x.Status == SupplierUpdateStatus.WaitingToRun || x.Status == SupplierUpdateStatus.Started).FirstOrDefault();
            try
            {
                bool clearData = false;
                if (supplierUpdateStatus == null || supplierUpdateStatus.TotalPages == -1)
                {
                    if (supplierUpdateStatus == null)
                    {
                        supplierUpdateStatus = new ASI_SuppliersUpdateStatus();
                        return;
                    }
                    supplierUpdateStatus.Status = SupplierUpdateStatus.Started;
                    supplierUpdateStatus.SuccessfullyRetrivedRcordCount = -1;
                    supplierUpdateStatus.TotalPages = 1;
                    supplierUpdateStatus.CurrentPage = 1;
                    supplierUpdateStatus.RecordsPerPage = double.Parse(ResultsPerPage);
                    if (supplierUpdateStatus.Id == 0)
                        _ASI_SuppliersUpdateStatusRepository.Insert(supplierUpdateStatus);
                    else
                        _ASI_SuppliersUpdateStatusRepository.Update(supplierUpdateStatus);
                    clearData = true;
                }
                else
                    supplierUpdateStatus.CurrentPage++;
                int PagesTotal = 1;
                if (supplierUpdateStatus.CurrentPage <= supplierUpdateStatus.TotalPages)
                {
                    var xmlOutput = new XmlDocument();
                    xmlOutput.LoadXml(@"<Results></Results>");
                    supplierUpdateStatus.Status = SupplierUpdateStatus.Running;
                    _ASI_SuppliersUpdateStatusRepository.Update(supplierUpdateStatus);
                    var response = GetResponseDocument(string.Format(SuplierSearchURL, supplierUpdateStatus.CurrentPage));
                    if (supplierUpdateStatus.CurrentPage == 1)
                    {
                        int ResultsTotal = int.Parse(response.GetElementsByTagName("ResultsTotal").Item(0).InnerText);
                        PagesTotal = (int)Math.Ceiling((ResultsTotal / supplierUpdateStatus.RecordsPerPage));
                        supplierUpdateStatus.TotalPages = PagesTotal;
                        supplierUpdateStatus.TotalRecords = ResultsTotal;
                    }
                    supplierUpdateStatus.Status = SupplierUpdateStatus.Updating;
                    _ASI_SuppliersUpdateStatusRepository.Update(supplierUpdateStatus);
                    var nodeResults = response.SelectSingleNode("/SearchResult/Results");
                    if (nodeResults.HasChildNodes)
                    {
                        if (supplierUpdateStatus.SuccessfullyRetrivedRcordCount < 0)
                            supplierUpdateStatus.SuccessfullyRetrivedRcordCount = 0;
                        supplierUpdateStatus.SuccessfullyRetrivedRcordCount += SaveSuppliersIndDB(nodeResults, clearData);
                        _ASI_SuppliersUpdateStatusRepository.Update(supplierUpdateStatus);
                    }
                    if (supplierUpdateStatus.TotalPages == supplierUpdateStatus.CurrentPage)
                    {
                        //var scheduleTask = _ScheduleTaskService.GetTaskByType(typeof(SupplierTask).FullName);
                        //if (scheduleTask!=null)
                        //{
                        //    scheduleTask.Enabled = true;
                        //    scheduleTask.Seconds = 0;
                        //    scheduleTask.LastEndUtc = DateTime.UtcNow;
                        //    scheduleTask.LastSuccessUtc = DateTime.UtcNow;
                        //    _scheduleTaskRepository.Update(scheduleTask);
                        //}
                        supplierUpdateStatus.Status = SupplierUpdateStatus.Completed;
                    }
                    else
                        supplierUpdateStatus.Status = SupplierUpdateStatus.WaitingToRun;
                    _ASI_SuppliersUpdateStatusRepository.Update(supplierUpdateStatus);
                }
                else
                {

                    supplierUpdateStatus.Status = SupplierUpdateStatus.Completed;
                    _ASI_SuppliersUpdateStatusRepository.Update(supplierUpdateStatus);
                }

            }
            catch (Exception ex)
            {
                supplierUpdateStatus.Status = SupplierUpdateStatus.WaitingToRun;
                _ASI_SuppliersUpdateStatusRepository.Update(supplierUpdateStatus);
                _logger.Warning("--------Get Suppliers ------:" + ex.Message, ex);

            }
        }

        public XmlDocument GetResponseDocument(string searchUrl)
        {
            var request = HttpWebRequest.Create(searchUrl);
            request.Headers.Add("Authorization", AuthorizationHeader);
            request.Method = "Get";

            var result = new XmlDocument();
            result.Load(request.GetResponse().GetResponseStream());

            return result;
        }

        public int SaveSuppliersIndDB(XmlNode node, bool clearDataOfSuccpliers)
        {
            var dataSet = ConverttYourXmlNodeToDataSet(node);
            using (SqlBulkCopy copy = new SqlBulkCopy(con))
            {
                if (dataSet.Tables["Supplier"] != null)
                {
                    copy.BulkCopyTimeout = 500;
                    copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Id", "SupplierId"));
                    copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Name", "Name"));
                    copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AsiNumber", "ASINumber"));
                    copy.DestinationTableName = "ASI_Suppliers";
                    con.Open();
                    if (clearDataOfSuccpliers)
                    {
                        string sqlTrunc = "TRUNCATE TABLE ASI_Suppliers";
                        SqlCommand cmd = new SqlCommand(sqlTrunc, con);
                        cmd.ExecuteNonQuery();
                    }
                    copy.WriteToServer(dataSet.Tables["Supplier"]);
                    con.Close();
                    copy.Close();
                    int insertedRecordsCount = dataSet.Tables["Supplier"].Rows.Count;
                    /*for (int i = 0; i < dataSet.Tables.Count; i++)
                    {
                        insertedRecordsCount += dataSet.Tables[i].Rows.Count;
                    }*/
                    dataSet.Dispose();
                    return insertedRecordsCount;
                }

            }
            return 0;
        }

        public static DataSet ConverttYourXmlNodeToDataSet(XmlNode xmlnodeinput)
        {
            //declaring data set object
            DataSet dataset = null;
            if (xmlnodeinput != null)
            {
                XmlTextReader xtr = new XmlTextReader(xmlnodeinput.OuterXml, XmlNodeType.Element, null);
                dataset = new DataSet();
                dataset.ReadXml(xtr);
                xtr.Dispose();
            }

            return dataset;
        }
        #endregion


        #region Product

        public void AddASI_Products(IASI_ProductsCSVGenerationRequestService productCSVGenerationRequestService, long taskId = 0)
        {
            var currentRequest = productCSVGenerationRequestService.GetCurrentRunningRequest();
            try
            {
                ASI_ProductsSearchOptions currentOption = new ASI_ProductsSearchOptions();
                if (currentRequest != null)
                {
                    currentOption = currentRequest.SearchOptions.Where(x => x.Status == CurrentOptionStatus.Running).FirstOrDefault();
                    if (currentOption == null)
                    {
                        currentOption = currentRequest.SearchOptions.Where(x => x.Status == CurrentOptionStatus.WaitingToRun).FirstOrDefault();
                    }
                }
                if (currentRequest == null || currentOption == null)
                {

                }
                else if (currentRequest.Status == ProductsCSVGenerationStatus.Running || currentRequest.Status == ProductsCSVGenerationStatus.Updating)
                {
                    return;
                }
                else
                {
                    currentOption.Status = CurrentOptionStatus.Running;
                    TaskStatus status = TaskStatus.WaitingToRun;
                    currentRequest.Status = ProductsCSVGenerationStatus.Running;
                    if (currentOption.TotalRecords == -1)
                    {
                        if (currentRequest.SuccessfullyRetrivedProductsCount < 1)
                            currentRequest.SuccessfullyRetrivedProductsCount = -1;
                        currentOption.TotalPages = 1;
                        currentOption.CurrentPage = 1;
                        currentRequest.RecordsPerPage = 0;
                        double recordsPerPage = 0;
                        double.TryParse(ResultsPerPage, out recordsPerPage);
                        currentRequest.RecordsPerPage = recordsPerPage;
                        currentOption.AddedDate = DateTime.Now;
                        currentOption.ModifiedDate = DateTime.Now;
                    }
                    else
                        currentOption.CurrentPage++;
                    _asi_ProductsCSVGenerationRequestsRepository.Update(currentRequest);
                    if (currentOption.CurrentPage <= currentOption.TotalPages)
                    {
                        string searchQuery = "";
                        if (currentOption != null && currentOption.SearchOptionType == ProductsSearchOptionType.Supplier)
                        {
                            searchQuery += "q=supplier:" + currentOption.SearchValue;
                        }
                        else if (currentOption != null && currentOption.SearchOptionType == ProductsSearchOptionType.Category)
                        {
                            searchQuery += "q=category:" + currentOption.SearchValue;
                        }
                        /*  if (currentOption.CurrentPage == 1)
                          {
                              searchQuery = "q=Id:5549338";
                          }
                          if (currentOption.CurrentPage == 2)
                          {
                              searchQuery = "q=Id:6808556";
                          }
                          if (currentOption.CurrentPage == 3)
                          {
                              searchQuery = "q=Id:6812942";
                          }
                          if (currentOption.CurrentPage == 4)
                          {
                              searchQuery = "q=Id:4994505";
                          }*/
                        var result = GetResponseJSON(string.Format(SearchApiUri, currentOption.CurrentPage, searchQuery));
                        if (result != null)
                        {
                            if (currentOption.CurrentPage == 1)
                            {
                                int ResultsTotal = result.ResultsTotal; //int.Parse(response.GetElementsByTagName("ResultsTotal").Item(0).InnerText);
                                currentOption.TotalPages = (int)Math.Ceiling((ResultsTotal / currentRequest.RecordsPerPage));
                                currentOption.TotalRecords = ResultsTotal;
                            }
                            currentRequest.Status = ProductsCSVGenerationStatus.Updating;
                            _asi_ProductsCSVGenerationRequestsRepository.Update(currentRequest);
                            if (result != null && result.Results != null && result.Results.Count > 0)
                            {
                                if (currentRequest.SuccessfullyRetrivedProductsCount < 0)
                                    currentRequest.SuccessfullyRetrivedProductsCount = 0;
                                currentRequest.SuccessfullyRetrivedProductsCount += ProcessToAddProducts(taskId, currentRequest.Id, result);

                                #region remove after demo
                                // remove After Demo
                                if (currentRequest.SuccessfullyRetrivedProductsCount > 2)
                                {
                                    currentOption.Status = CurrentOptionStatus.Completed;
                                    currentOption.ModifiedDate = DateTime.Now;
                                    currentOption.CurrentPage = currentOption.TotalPages;
                                    status = TaskStatus.RanToCompletion;
                                    currentRequest.Status = ProductsCSVGenerationStatus.Completed;
                                    bool fileExists = false;
                                    currentRequest.GenerateFolderPath = "";
                                    currentRequest.ImageZipFileExists = fileExists;
                                    if (_asi_ProductsCSVGenerationRequestsRepository.Table.Count() > 0)
                                        currentRequest.ProductCSVExists = true;
                                    else
                                        currentRequest.ProductCSVExists = false;
                                }
                                // remove After Demo
                                #endregion
                            }
                        }
                        if (currentOption.TotalPages == currentOption.CurrentPage)
                        {
                            //System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Factory.StartNew(() => _dbContext.SqlQuery<int>("EXEC [ImportASIData] '" + currentRequest.Id + "'"));
                            //var exeresult = await System.Threading.Tasks.Task.Run(()=> _dbContext.SqlQuery<int>("EXEC [ImportASIData] '" + currentRequest.Id + "'"));
                            //await System.Threading.Tasks.Task.Factory.StartNew(() => _dbContext.SqlQuery<int>("EXEC [ImportASIData] '" + currentRequest.Id + "'"));

                            //task.Wait();

                            _dbContext.ExecuteSqlCommand("EXEC [ImportASIData] '" + currentRequest.Id + "'", true, 30000);
                            
                            currentOption.Status = CurrentOptionStatus.Completed;
                            currentOption.ModifiedDate = DateTime.Now;
                            if (currentRequest.SearchOptions.Where(x => x.Status != CurrentOptionStatus.Completed).Count() == 0)
                            {
                                status = TaskStatus.RanToCompletion;
                                currentRequest.Status = ProductsCSVGenerationStatus.Completed;
                                bool fileExists = false;
                                currentRequest.GenerateFolderPath = "";
                                currentRequest.ImageZipFileExists = fileExists;
                                if (_asi_ProductsCSVGenerationRequestsRepository.Table.Count() > 0)
                                    currentRequest.ProductCSVExists = true;
                                else
                                    currentRequest.ProductCSVExists = false;

                            }
                            else
                            {
                                currentRequest.Status = ProductsCSVGenerationStatus.WaitingToRun;
                            }
                        }
                        else
                            currentRequest.Status = ProductsCSVGenerationStatus.WaitingToRun;
                        _asi_ProductsCSVGenerationRequestsRepository.Update(currentRequest);
                    }
                    else
                    {
                        System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Factory.StartNew(() => _dbContext.SqlQuery<int>("EXEC [ImportASIData] '" + currentRequest.Id + "'"));
                        //var exeresult = await System.Threading.Tasks.Task.Run(()=> _dbContext.SqlQuery<int>("EXEC [ImportASIData] '" + currentRequest.Id + "'"));

                        task.Wait();

                        currentOption.Status = CurrentOptionStatus.Completed;
                        currentOption.ModifiedDate = DateTime.Now;
                        if (currentRequest.SearchOptions.Where(x => x.Status != CurrentOptionStatus.Completed).Count() == 0)
                        {
                            status = TaskStatus.RanToCompletion;
                            currentRequest.Status = ProductsCSVGenerationStatus.Completed;
                            bool fileExists = false;
                            currentRequest.GenerateFolderPath = "";
                            currentRequest.ImageZipFileExists = fileExists;
                            if (_asi_ProductsCSVGenerationRequestsRepository.Table.Count() > 0)
                                currentRequest.ProductCSVExists = true;
                            else
                                currentRequest.ProductCSVExists = false;
                        }
                        else
                        {
                            currentRequest.Status = ProductsCSVGenerationStatus.WaitingToRun;
                        }
                        _asi_ProductsCSVGenerationRequestsRepository.Update(currentRequest);
                    }

                }
            }
            catch (Exception ex)
            {
                if (currentRequest != null)
                {
                    currentRequest.Status = ProductsCSVGenerationStatus.WaitingToRun;
                    _asi_ProductsCSVGenerationRequestsRepository.Update(currentRequest);
                }

                _logger.Error("ADD asi Products : AddASI_Products : ASI_API", ex);
            }
        }
        public ProductResult GetResponseJSON(string searchUrl)
        {
            _logger.Information("Getting Products URL: " + searchUrl);
            var request = HttpWebRequest.Create(searchUrl);
            request.Headers.Add("Authorization", AuthorizationHeader);
            request.Method = "Get";
            StreamReader r = new StreamReader(request.GetResponse().GetResponseStream());
            string response = r.ReadToEnd();
            ProductResult productsResult = new ProductResult();
            try
            {
                productsResult = JsonConvert.DeserializeObject<ProductResult>(response);
            }
            catch (Exception e)
            {
                _logger.Error("Json Parsrig Products : GetResponseJson : ASI_API", e);
            }
            return productsResult;
        }
        public int ProcessToAddProducts(long taskId, int requestId, ProductResult result)
        {
            int added = 0;
            if (result != null)
            {
                foreach (var product in result.Results)
                {
                    if (added > 10)
                    {
                        return added;
                    }

                    else {
                        if (AddProduct(taskId, requestId, product))
                            added++;
                    }
                }
            }
            return added;
        }

        public bool AddProduct(long taskId, int requestId, Result record)
        {
            _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "Adding Product to asi_products Product Code:" + record.Number + " Product ID :" + record.Id);
            bool added = false;
            if (record.Number != null && (record.Number.Contains("15765") || record.Number.Contains("GC0422") || record.Number.Contains("55155")))
            {

            }

            if (CanAddProductToCSV(record))
            {


                string productCode = ValidateProductCode(record.Number);
                if (string.IsNullOrWhiteSpace(productCode))
                    return false;
                var product = new ASI_Product();
                product.requestId = requestId;
                product.ProductName = record.Name;
                product.FullDescription = GetDescription(record);
                //product.MetaKeywords = GetProductKeywords(record);
                product.MetaKeywords = ("Promotional " + record.Name + ", Personalized " + record.Name + ", Custom " + record.Name + ", Printed " + record.Name);
                product.MetaDescription = "Call 866-856-7063 to order " + record.Name + ".  Add your logo free. We'll beat any price.  Free proofs.";
                product.MetaTitle = "Custom Promotional " + record.Name + " Personalized with your Logo by My Promotional Pens";
                product.ProductCode = productCode;
                product.MinStockQuantity = 0;
                decimal productPrice = 0;
                int MinStockQuantity = 0;
                int MaxQuantity = 0;
                if (record.Prices != null && record.Prices.Count > 0)
                {
                    int? minQuantity = null;
                    int? maxQuantity = null;
                    foreach (var price in record.Prices)
                    {
                        if (price.Quantity != null && (!minQuantity.HasValue || price.Quantity.From < minQuantity.Value))
                        {
                            minQuantity = price.Quantity.From;
                            productPrice = Convert.ToDecimal(price.Price);
                        }
                        if (price.Quantity != null && (!maxQuantity.HasValue || price.Quantity.To > maxQuantity.Value))
                        {
                            maxQuantity = Convert.ToInt32(price.Quantity.To);

                        }
                    }
                    if (minQuantity.HasValue)
                    {
                        MinStockQuantity = minQuantity.Value;
                    }
                    if (maxQuantity.HasValue)
                    {
                        MaxQuantity = maxQuantity.Value;
                    }
                }
                product.MinStockQuantity = MinStockQuantity;
                product.OrderMinimumQuantity = MinStockQuantity;
                product.OrderMaximumQuantity = MaxQuantity;
                product.Price = productPrice;
                product.HasDiscountsApplied = HasDiscount(record, Convert.ToDouble(productPrice), productCode);
                product.SetupFee = Convert.ToDecimal(GetSetUpcost(record));
                product.Material = GetMaterial(record);
                product.Size = GetSize(record);
                product.ImprintType = "";
                product.ImprintArea = GetImprintArea(record);
                product.InkColor = "";
                if (record.Imprinting != null && record.Imprinting.FullColorProcess)
                    product.MultiColorImprintAvailable = "Yes (call or chat for pricing)";
                else
                    product.MultiColorImprintAvailable = "No";
                if (string.IsNullOrEmpty(record.PriceIncludes))
                {
                    product.PriceIncludes = "1 Color Imprint";
                }
                else if (record.Imprinting != null && record.Imprinting.Methods != null && record.Imprinting.Methods.Values != null && record.Imprinting.Methods.Values.Count > 0)
                {
                    product.PriceIncludes = record.PriceIncludes;
                }
                product.NormalProductionDays = GetNormalProductiondays(record);
                product.RushProductionDays = GetRushProductionDays(record);
                product.ProductManufacturer = record.Supplier == null ? "" : record.Supplier.Name;
                product.AddedDate = DateTime.UtcNow;
                product.ModifiedDate = DateTime.UtcNow;
                added = true;
                ASI_ProductsAddedToCSV csvPRoduct = new ASI_ProductsAddedToCSV();
                csvPRoduct.ProductCode = productCode;
                if (record.Supplier != null)
                    csvPRoduct.SupplierName = record.Supplier.Name;
                _aSI_ProductsAddedToCSV.Insert(csvPRoduct);


                _asi_Product.Insert(product);

                try
                {
                    AddProductAttributes(record, productCode);
                }
                catch { }
                try
                {
                    GetDiscounts(record, Convert.ToDouble(productPrice), productCode, requestId);
                }
                catch { }
                try
                {
                    AddImages(record, productCode);
                }
                catch { }

                AddCategory(record, productCode, product.Id);
            }
            else
            {
                _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "Skipping Adding Product to ASI_Products Product Code:" + record.Number + " Product Id:" + record.Id, "1");
            }

            _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "Done Adding Products to ASI_Products Product Product Code:" + record.Number, "1");
            return added;
        }

        public string GetDiscounts(Result record, double? basePrice, string productCodeGenerated, int requestId)
        {
            try
            {
                bool isDiscount = false;

                _dbContext.ExecuteSqlCommand("delete from ASI_DiscountsApplyTo where lower(ProductCode)=lower('" + productCodeGenerated + "')");

                _logger.Information("Started Checking Discounts Product Code:" + productCodeGenerated);
                if (record.Prices != null)
                {
                    int discountId = 0;
                    if (_asi_DiscountsRepository.Table.Count() > 0)
                        discountId = _asi_DiscountsRepository.Table.Max(x => x.DisCountAutoId);
                    discountId++;
                    var resource = _asi_DiscountsRepository.Table;
                    foreach (var data in record.Prices)
                    {
                        if (data != null && data.Quantity != null)
                        {
                            if (record.Prices.IndexOf(data) == record.Prices.Count() - 1)
                            {
                                data.Quantity.To = null;
                            }
                            long minQuantity = data.Quantity.From;
                            long? maxQuantity = data.Quantity.To;
                            if (minQuantity == maxQuantity)
                                maxQuantity = null;
                            double baseP = basePrice.HasValue ? basePrice.Value : 0;
                            double disount = (baseP - data.Price) + 0.01;
                            decimal? dicountDecimal = Convert.ToDecimal(disount);
                            if (dicountDecimal.HasValue)
                            {
                                dicountDecimal = Math.Round(dicountDecimal.Value, 2);
                            }
                            var existingDicount = _asi_DiscountsRepository.Table.Where(x => x.MinQty == minQuantity && x.MaxQty == maxQuantity && x.DisCountValue == dicountDecimal).FirstOrDefault();
                            bool inserted = false;
                            if (existingDicount == null)
                            {
                                existingDicount = new ASI_Discounts();
                                existingDicount.Name = "Discount";
                                existingDicount.MinQty = minQuantity;
                                if (minQuantity != maxQuantity)
                                    existingDicount.MaxQty = maxQuantity;
                                //existingDicount.DisCountType = "Dollar amount off Product";
                                existingDicount.DisCountType = "Per Unit";
                                existingDicount.DisCountValue = dicountDecimal;
                                existingDicount.Span = "Y";
                                existingDicount.Taxable_DiscountAfterTax = 0;
                                existingDicount.CouponUsage = "Unlimited";
                                existingDicount.DisCountAutoId = discountId++;
                                _asi_DiscountsRepository.Insert(existingDicount);
                                inserted = true;
                            }
                            ASI_DiscountsApplyTo discountApplied = null;
                            if (!inserted)
                            {
                                discountApplied = _asi_DiscountsApplyToRepository.Table.Where(x =>
                                  x.DiscountAutoId == existingDicount.DisCountAutoId &&
                                  x.ProductCode == productCodeGenerated
                                  ).FirstOrDefault();
                            }
                            if (discountApplied == null)
                            {
                                discountApplied = new ASI_DiscountsApplyTo();
                                discountApplied.MinQty = minQuantity;
                                discountApplied.MaxQty = maxQuantity;
                                discountApplied.DisCountValue = dicountDecimal;
                                discountApplied.ProductCode = productCodeGenerated;
                                discountApplied.DiscountAutoId = existingDicount.DisCountAutoId;
                                _asi_DiscountsApplyToRepository.Insert(discountApplied);
                                isDiscount = true;
                            }
                        }
                    }
                }

                if (isDiscount)
                {
                    _dbContext.ExecuteSqlCommand("update asi_product set HasDiscountsApplied='true' where lower(ProductCode)=lower('" + productCodeGenerated + "') and requestid='" + requestId + "'");
                }

                _logger.Information("Done Checking Discounts Product Code:" + productCodeGenerated);
            }
            catch (Exception ex) {
                _logger.Error("Error Import Discount For Productcode:" + productCodeGenerated, ex);
            }
            return "";

        }

        public bool CanAddProductToCSV(Result record)
        {
            if (string.IsNullOrWhiteSpace(record.Number))
            {
                return false;
            }
            record.Number = record.Number.Trim();
            record.Number = record.Number.Replace(' ', '-');
            //If product id begins with a symbol (*), exclude it. 
            if (record.Number.StartsWith("*"))
            {
                return false;
            }
            //If a product id begins with 0, change to a '1-'
            if (record.Number.StartsWith("0"))
            {
                record.Number = "1-" + record.Number.Substring(1, record.Number.Length - 1);
            }
            //Excluded Category: Apparel
            if (record.Categories != null)
            {
                foreach (var category in record.Categories)
                {
                    if (category.Name != null && category.Name.ToLower() == "apparel")
                    {
                        return false;
                    }
                }
            }
            //Exclude Canadian Products -- Not sure how to identify canadian products

            //If product price is not available or blank or null or we will not include in CSV files.
            if (record.Prices == null || record.Prices.Count() == 0)
                return false;
            //If product price is not available or blank or null or we will not include in CSV files.
            //if (record.Prices.Any(x => x.Price != null))
            //    return true;
            //else
            //    return false;
            // image are not available in ASI Central, we will not include in CSV files.
            if (string.IsNullOrWhiteSpace(record.ImageUrl))
                return false;
            //if there is a product already added in to the volusion with same supplier then we will ignore that product

            //check in the csv to validate whether the product code can be added or not  -- pending 
            if (record.Supplier != null)
            {
                string regExp = record.Number + "-[0-9*]%";
                var productCodeExpression = new Regex(regExp, RegexOptions.IgnoreCase);

                string sqlQuery = @"select top 1 * from ASI_ProductsAddedToCSV
                                        where ProductCode is not null and SupplierName = '" + record.Supplier.Name + @"' and
                                        SupplierName is not null and
                                        (
                                        ProductCode = '" + record.Number + @"' or 
                                        ProductCode Like '" + record.Number + @"-[0-9*]%' 
                                        )";
                var product = _dbContext.SqlQuery<ASI_ProductsAddedToCSV>(sqlQuery).FirstOrDefault();
                if (product != null)
                    return false;
                if (record.Supplier.Name.Trim().ToLower().EndsWith("canada"))
                {
                    return false;
                }
            }
            return true;
        }
        public string ValidateProductCode(string productCode)
        {
            if (productCode.StartsWith("0"))
            {
                productCode = "1-" + productCode.Substring(1, productCode.Length - 1);
            }

            //getting product code from volusion database
            productCode = productCode.Replace("/", "-");

            //getting product code from generate csv file

            string sqlQuery = @"select * from ASI_ProductsAddedToCSV
                                        where ProductCode is not null and
                                        SupplierName is not null and
                                        (
                                        ProductCode = '" + productCode + @"' or 
                                        ProductCode Like '" + productCode + @"-[0-9*]%' 
                                        )";
            var products1 = _dbContext.SqlQuery<ASI_ProductsAddedToCSV>(sqlQuery).ToList();
            List<string> productCodes = new List<string>();
            if (products1 != null && products1.Count > 0)
                productCodes.AddRange(products1.Select(x => x.ProductCode).ToList());


            //if product code already exists then we have to append number to the product code based on the previous product codes
            if (productCodes != null && productCodes.Count() > 0)
            {
                int productNumberBegingAppended = 1;
                foreach (var product in productCodes)
                {
                    var temp = product.Split('-');
                    if (temp != null && temp.Count() > 1)
                    {
                        var numberString = temp.Last();
                        int temp1 = 0;
                        int.TryParse(numberString, out temp1);
                        if (temp1 > productNumberBegingAppended)
                            productNumberBegingAppended = temp1;
                    }

                }
                productCode += "-" + productNumberBegingAppended;
                // check in the csv file that is going to be generate to validate product code -- pending
            }
            return productCode;
        }

        public string GetProductKeywords(Result record)
        {
            _logger.Information("Started Getting Product Keywords for " + record.Number);
            string productKeyword = "";
            if (record.Themes != null)
            {
                foreach (var theme in record.Themes)
                {
                    if (theme != null)
                    {
                        if (productKeyword != "")
                            productKeyword += ",";
                        productKeyword += theme.Replace(" ", ",");
                    }
                }
            }
            if (record.Imprinting != null)
            {
                if (record.Imprinting.Colors != null)
                {
                    foreach (var color in record.Imprinting.Colors.Values)
                    {
                        if (color != null && color.Name != null)
                        {
                            if (productKeyword != "")
                                productKeyword += ",";
                            productKeyword += color.Name.Replace(" ", ",");
                        }

                    }
                }
            }
            _logger.Information("Done Getting Product Keywords for " + record.Number);

            return productKeyword;
        }
        public string GetDescription(Result record)
        {
            try
            {
                if (record == null || record.Number == null)
                    return "";
                _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "Started Checking Discription Product Code:" + record.Number, "2");
                string finalDescription = "";
                if (record.Description != null)
                    finalDescription = @"<div class=""desc1"">" + record.Description + "</div>";
                if (string.IsNullOrEmpty(record.PriceIncludes))
                {
                    finalDescription += @"<div class=""desc2""><span class=""desc2title"">Price Includes:</span>  1 Color Imprint</div>";
                }
                else if (record.Imprinting != null && record.Imprinting.Methods != null && record.Imprinting.Methods.Values != null && record.Imprinting.Methods.Values.Count > 0)
                {
                    finalDescription += @"<div class=""desc2""><span class=""desc2title"">Price Includes:</span> " + record.PriceIncludes + "</div>";
                }
                if (record.ProductionTime != null)
                {
                    bool canAdd = record.ProductionTime.Where(x => x.Days != null && x.Days.ToString().Trim() != "0").Count() > 0;
                    if (canAdd)
                    {
                        finalDescription += @"<div class=""desc3""><span class=""desc3title"">Normal Production:</span>";
                        bool entered = false;
                        foreach (var productionTime in record.ProductionTime)
                        {
                            if (entered)
                                finalDescription += ";";
                            if (productionTime.Days is int || productionTime.Days is Int64)
                            {
                                if (productionTime.Days != null && productionTime.Days.ToString() != "0")
                                {
                                    entered = true;
                                    finalDescription += productionTime.Days.ToString() + " Days";
                                }
                            }
                            else if (productionTime.Days is ProductionTimeDays)
                            {
                                entered = true;
                                finalDescription += (productionTime.Days as ProductionTimeDays).From + " to " + (productionTime.Days as ProductionTimeDays).To + " Days";
                            }
                            else
                            {
                                entered = true;
                                finalDescription += productionTime.Name.ToString();
                            }
                        }
                        finalDescription += @"</div>";
                    }
                }
                if (record.RushTime != null)
                {
                    bool canAdd = record.RushTime.Where(x => x.Days != null && x.Days.ToString().Trim() != "0").Count() > 0;
                    if (canAdd)
                    {
                        finalDescription += @"<div class=""desc4""><span class=""desc4title"">Rush Production:</span>";
                        bool entered = false;

                        foreach (var rushTime in record.RushTime)
                        {
                            if (entered)
                                finalDescription += ";";
                            if (rushTime.Days is int || rushTime.Days is Int64)
                            {
                                if (rushTime.Days != null && rushTime.Days.ToString() != "0")
                                {
                                    finalDescription += rushTime.Days.ToString() + " Days";
                                    entered = true;
                                }
                            }
                            else if (rushTime.Days is ProductionTimeDays)
                            {
                                entered = true;
                                finalDescription += (rushTime.Days as ProductionTimeDays).From + " to " + (rushTime.Days as ProductionTimeDays).To + " Days";
                            }
                            else
                            {
                                entered = true;
                                finalDescription += rushTime.Name.ToString();
                            }
                        }
                        finalDescription += @"</div>";
                    }
                }
                if (record.Imprinting != null && record.Imprinting.Sizes != null && record.Imprinting.Sizes.Values != null)
                {
                    finalDescription += @"<div class=""desc5"">Imprint Area:</span>";
                    bool entered = false;
                    foreach (var size in record.Imprinting.Sizes.Values)
                    {
                        if (entered)
                            finalDescription += ";";
                        finalDescription += size.Description == null ? "" : size.Description.ToString() + " " + size.Name == null ? "" : size.Name.ToString();
                        entered = true;
                    }
                    finalDescription += "</div>";
                    //  Barrel: " + record.Imprinting.Sizes.Values[0] + " H Clip: " + record.Imprinting.Sizes.Values[1] + "</div>";
                }
                if (record.Imprinting != null && record.Imprinting.FullColorProcess)
                {
                    // how does we will know multi color is avaliable?
                    finalDescription += @"<div class=""desc6""><span class=""desc6title""> Multi Color Imprint Available:</span>Yes (call or chat for pricing) </div>";
                }
                /*
                if (record.Imprinting != null && record.Imprinting.Sizes != null && record.Imprinting.Sizes.Values != null && record.Imprinting.Sizes.Values.Count > 2)
                {
                    // how does we know which color ? and which point its has
                    finalDescription += @"<div class=""desc7""> <span class=""desc7title"">Ink Color:</span>  Black - Medium Point</div>";
                }*/

                if (record.Attributes != null && record.Attributes.Sizes != null && record.Attributes.Sizes.Values != null)
                {
                    if (record.Attributes.Sizes.Values is List<Value>)
                    {
                        try
                        {
                            var values = record.Attributes.Sizes.Values as List<Value>;
                            if (values != null && values.Count() > 0)
                            {
                                finalDescription += @"<div class=""desc8""><span class=""desc8title""> Size: </span>";
                                bool entered = false;
                                foreach (var size in values)
                                {
                                    if (entered)
                                        finalDescription += ";";
                                    finalDescription += size.Name == null ? "" : size.Name;
                                    entered = true;
                                }
                                finalDescription += "</div>";
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error("GetDescription: ASI_API", e);

                        }
                    }
                }
                _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "Done Checking Discription Product Code:" + record.Number, "2");
                return finalDescription;
            }
            catch (Exception e)
            {
                _logger.Error("GetDescription: ASI_API", e);
                return "";
            }
        }

        public bool HasDiscount(Result record, double? basePrice, string productCodeGenerated)
        {
            return false;

        }
        public string GetSetUpcost(Result record)
        {
            string setupcost = "0";
            bool setUpCostFound = false;
            if (record.Imprinting != null && record.Imprinting.Sizes != null && record.Imprinting.Sizes.Values != null)
            {
                foreach (var value in record.Imprinting.Sizes.Values)
                {
                    if (value.Options != null)
                    {
                        foreach (var option in value.Options)
                        {
                            if (option.Groups != null)
                            {
                                foreach (var group in option.Groups)
                                {
                                    if (group.Charges != null)
                                    {
                                        foreach (var charge in group.Charges)
                                        {
                                            //getting setup charge 
                                            if (charge.TypeCode != null && charge.Prices != null && charge.TypeCode.ToLower() == "stch" && charge.Prices.Count() > 0)
                                            {
                                                setupcost = charge.Prices[0].Price == null ? "0" : charge.Prices[0].Price.ToString();
                                                setUpCostFound = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (setUpCostFound)
                                        break;
                                }
                            }
                            if (setUpCostFound)
                                break;
                        }
                    }
                    if (setUpCostFound)
                        break;
                }
            }
            if (record.Imprinting != null && record.Imprinting.Methods != null && record.Imprinting.Methods.Values != null && !setUpCostFound)
            {
                foreach (var value in record.Imprinting.Methods.Values)
                {
                    if (value != null && value.Charges != null)
                    {
                        foreach (var charge in value.Charges)
                        {
                            if (!string.IsNullOrWhiteSpace(charge.Type) && charge.TypeCode.ToLower() == "stch")
                            {
                                setupcost = charge.Prices[0].Price.ToString() == null ? "" : charge.Prices[0].Price.ToString();
                                setUpCostFound = true;
                                break;
                            }
                        }
                    }
                    if (setUpCostFound)
                        break;
                }
            }
            return setupcost;
        }
        public string GetMaterial(Result record)
        {
            string strMaterial = "";
            if (record.Attributes != null && record.Attributes.Materials != null && record.Attributes.Materials.Values != null)
            {

                foreach (var material in record.Attributes.Materials.Values)
                {


                    strMaterial += material.Name == null ? "" : material.Name + ",";

                }
            }
            return strMaterial;
        }
        public string GetSize(Result record)
        {
            string strSize = "";
            if (record.Attributes != null && record.Attributes.Sizes != null && record.Attributes.Sizes.Values != null)
            {
                if (record.Attributes.Sizes.Values is List<Value>)
                {
                    try
                    {
                        var values = record.Attributes.Sizes.Values as List<Value>;
                        if (values != null && values.Count() > 0)
                        {
                            bool entered = false;
                            foreach (var size in values)
                            {
                                if (entered)
                                    strSize += ";";
                                strSize += size.Name == null ? "" : size.Name;
                                entered = true;
                            }

                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            return strSize;
        }

        public string GetImprintArea(Result record)
        {
            string strArea = "";
            if (record.Imprinting != null && record.Imprinting.Sizes != null && record.Imprinting.Sizes.Values != null)
            {

                bool entered = false;
                foreach (var size in record.Imprinting.Sizes.Values)
                {
                    if (entered)
                        strArea += ";";
                    strArea += size.Description == null ? "" : size.Description + " " + size.Name == null ? "" : size.Name;
                    entered = true;
                }


            }
            return strArea;
        }

        public string GetNormalProductiondays(Result record)
        {
            string productionDays = "";
            if (record.ProductionTime != null)
            {
                bool canAdd = record.ProductionTime.Where(x => x.Days != null && x.Days.ToString().Trim() != "0").Count() > 0;
                if (canAdd)
                {

                    bool entered = false;
                    foreach (var productionTime in record.ProductionTime)
                    {
                        if (entered)
                            productionDays += ";";
                        if (productionTime.Days is int || productionTime.Days is Int64)
                        {
                            if (productionTime.Days != null && productionTime.Days.ToString() != "0")
                            {
                                entered = true;
                                productionDays += productionTime.Days.ToString() + " Days";
                            }
                        }
                        else if (productionTime.Days is ProductionTimeDays)
                        {
                            entered = true;
                            productionDays += (productionTime.Days as ProductionTimeDays).From + " to " + (productionTime.Days as ProductionTimeDays).To + " Days";
                        }
                        else
                        {
                            entered = true;
                            productionDays += productionTime.Name.ToString();
                        }
                    }

                }
            }
            return productionDays;
        }

        public string GetRushProductionDays(Result record)
        {
            string rushProductionDay = "";
            if (record.RushTime != null)
            {
                bool canAdd = record.RushTime.Where(x => x.Days != null && x.Days.ToString().Trim() != "0").Count() > 0;
                if (canAdd)
                {

                    bool entered = false;

                    foreach (var rushTime in record.RushTime)
                    {
                        if (entered)
                            rushProductionDay += ";";
                        if (rushTime.Days is int || rushTime.Days is Int64)
                        {
                            if (rushTime.Days != null && rushTime.Days.ToString() != "0")
                            {
                                rushProductionDay += rushTime.Days.ToString() + " Days";
                                entered = true;
                            }
                        }
                        else if (rushTime.Days is ProductionTimeDays)
                        {
                            entered = true;
                            rushProductionDay += (rushTime.Days as ProductionTimeDays).From + " to " + (rushTime.Days as ProductionTimeDays).To + " Days";
                        }
                        else
                        {
                            entered = true;
                            rushProductionDay += rushTime.Name.ToString();
                        }
                    }

                }
            }
            return rushProductionDay;
        }

        public string AddProductAttributes(Result record, string productCodeGenerated)
        {

            _dbContext.ExecuteSqlCommand("delete from ASI_Options where lower(ProductCode)=lower('" + productCodeGenerated + "')");

            string str = "";

            #region AddImprintColor
            try
            {
                if (record.Imprinting != null && record.Imprinting.Colors != null)
                {
                    foreach (var data in record.Imprinting.Colors.Values)
                    {
                        if (data.Name != null)
                        {
                            var option = _asi_optionsRepository.Table.Where(x => x.OptionsDesc != null && x.OptionsDesc.ToLower() == data.Name.ToLower() && x.OptionsName.ToLower() == "Imprint Color".ToLower() && x.ProductCode.ToLower() == productCodeGenerated.ToLower()).FirstOrDefault();
                            if (option == null)
                            {
                                ASI_Options newOption = new ASI_Options();
                                newOption.OptionCatId = 4;
                                newOption.OptionsDesc = data.Name;
                                newOption.ProductCode = productCodeGenerated;
                                newOption.OptionsName = "Imprint Color";
                                newOption.AddedDate = null;
                                newOption.ModifiedDate = null;
                                newOption.IP = null;
                                _asi_optionsRepository.Insert(newOption);
                            }

                        }

                    }
                }
            }
            catch { }
            #endregion

            #region AddItemColor
            try
            {
                if (record.Attributes != null && record.Attributes.Colors != null)
                {
                    foreach (var data in record.Attributes.Colors.Values)
                    {
                        if (data.Name != null)
                        {
                            var option = _asi_optionsRepository.Table.Where(x => x.OptionsDesc != null && x.OptionsDesc.ToLower() == data.Name.ToLower() && x.OptionsName.ToLower() == "Item Color".ToLower() && x.ProductCode.ToLower() == productCodeGenerated.ToLower()).FirstOrDefault();
                            if (option == null)
                            {
                                ASI_Options newOption = new ASI_Options();
                                newOption.OptionCatId = 10;
                                newOption.OptionsDesc = data.Name;
                                newOption.ProductCode = productCodeGenerated;
                                newOption.OptionsName = "Item Color";
                                newOption.AddedDate = null;
                                newOption.ModifiedDate = null;
                                newOption.IP = null;
                                _asi_optionsRepository.Insert(newOption);
                            }
                        }

                    }
                }
            }
            catch { }
            #endregion

            //#region AddSizes

            //if (record.Attributes != null && record.Attributes.Sizes != null && record.Attributes.Sizes.Values != null)
            //{
            //    if (record.Attributes.Sizes.Values is List<Value>)
            //    {
            //        try
            //        {
            //            var values = record.Attributes.Sizes.Values as List<Value>;
            //            if (values != null && values.Count() > 0)
            //            {
            //                foreach (var size in values)
            //                {

            //                    if (size.Name != null)
            //                    {
            //                        var option = _asi_optionsRepository.Table.Where(x => x.OptionsDesc != null && x.OptionsDesc.ToLower() == size.Name.ToLower() && x.OptionsName.ToLower() == "Size".ToLower() && x.ProductCode.ToLower() == productCodeGenerated.ToLower()).FirstOrDefault();
            //                        if (option == null)
            //                        {
            //                            ASI_Options newOption = new ASI_Options();
            //                            newOption.OptionCatId = 0;
            //                            newOption.OptionsDesc = size.Name == null ? "" : size.Name;
            //                            newOption.ProductCode = productCodeGenerated;
            //                            newOption.OptionsName = "Size";
            //                            newOption.AddedDate = null;
            //                            newOption.ModifiedDate = null;
            //                            newOption.IP = null;
            //                            _asi_optionsRepository.Insert(newOption);
            //                        }
            //                    }



            //                }

            //            }
            //        }
            //        catch (Exception e)
            //        {

            //        }
            //    }
            //}
            //#endregion

            //#region AddPackaing 
            //if (record.Packaging.Any())
            //{

            //    foreach (var item in record.Packaging)
            //    {
            //        if (item.Values is List<Value>)
            //        {
            //            try
            //            {
            //                var values = item.Values as List<Value>;
            //                if (values != null && values.Count() > 0)
            //                {
            //                    foreach (var package in values)
            //                    {

            //                        if (package.Name != null)
            //                        {
            //                            var option = _asi_optionsRepository.Table.Where(x => x.OptionsDesc != null && x.OptionsDesc.ToLower() == package.Name.ToLower() && x.OptionsName.ToLower() == "Packaging Options".ToLower() && x.ProductCode.ToLower() == productCodeGenerated.ToLower()).FirstOrDefault();
            //                            if (option == null)
            //                            {
            //                                ASI_Options newOption = new ASI_Options();
            //                                newOption.OptionCatId = 0;
            //                                newOption.OptionsDesc = package.Name == null ? "" : package.Name;
            //                                newOption.ProductCode = productCodeGenerated;
            //                                newOption.OptionsName = "Packaging Options";
            //                                newOption.AddedDate = null;
            //                                newOption.ModifiedDate = null;
            //                                newOption.IP = null;
            //                                _asi_optionsRepository.Insert(newOption);
            //                            }
            //                        }



            //                    }

            //                }
            //            }
            //            catch (Exception e)
            //            {

            //            }
            //        }
            //    }


            //}
            //#endregion

            //#region AddImprintTYpe
            ////Imprint Type|
            //try
            //{
            //    if (record.Imprinting != null && record.Imprinting.Methods != null)
            //    {
            //        foreach (var data in record.Imprinting.Methods.Values)
            //        {
            //            if (data.Name != null)
            //            {
            //                var option = _asi_optionsRepository.Table.Where(x => x.OptionsDesc != null && x.OptionsDesc.ToLower() == data.Name.ToLower() && x.OptionsName.ToLower() == "Imprint Type".ToLower() && x.ProductCode.ToLower() == productCodeGenerated.ToLower()).FirstOrDefault();
            //                if (option == null)
            //                {
            //                    ASI_Options newOption = new ASI_Options();
            //                    newOption.OptionCatId = 0;
            //                    newOption.OptionsDesc = data.Name;
            //                    newOption.ProductCode = productCodeGenerated;
            //                    newOption.OptionsName = "Imprint Type";
            //                    newOption.AddedDate = null;
            //                    newOption.ModifiedDate = null;
            //                    newOption.IP = null;
            //                    _asi_optionsRepository.Insert(newOption);
            //                }

            //            }

            //        }
            //    }
            //}
            //catch { }

            //#endregion

            //#region AddService
            //try
            //{
            //    if (record.Imprinting != null && record.Imprinting.Services != null)
            //    {
            //        foreach (var data in record.Imprinting.Services.Values)
            //        {
            //            if (data.Name != null)
            //            {
            //                var option = _asi_optionsRepository.Table.Where(x => x.OptionsDesc != null && x.OptionsDesc.ToLower() == data.Name.ToLower() && x.OptionsName.ToLower() == "Service".ToLower() && x.ProductCode.ToLower() == productCodeGenerated.ToLower()).FirstOrDefault();
            //                if (option == null)
            //                {
            //                    ASI_Options newOption = new ASI_Options();
            //                    newOption.OptionCatId = 0;
            //                    newOption.OptionsDesc = data.Name;
            //                    newOption.ProductCode = productCodeGenerated;
            //                    newOption.OptionsName = "Service";
            //                    newOption.AddedDate = null;
            //                    newOption.ModifiedDate = null;
            //                    newOption.IP = null;
            //                    _asi_optionsRepository.Insert(newOption);
            //                }

            //            }

            //        }
            //    }
            //}
            //catch { }
            //#endregion

            //#region AddOptions
            //try
            //{
            //    if (record.Options != null && record.Options.Any())
            //    {

            //        foreach (var data in record.Options)
            //        {
            //            if (data.Name != null)
            //            {
            //                var option = _asi_optionsRepository.Table.Where(x => x.OptionsDesc != null && x.OptionsDesc.ToLower() == data.Name.ToLower() && x.OptionsName.ToLower() == "Options".ToLower() && x.ProductCode.ToLower() == productCodeGenerated.ToLower()).FirstOrDefault();
            //                if (option == null)
            //                {
            //                    ASI_Options newOption = new ASI_Options();
            //                    newOption.OptionCatId = 0;
            //                    newOption.OptionsDesc = data.Name;
            //                    newOption.ProductCode = productCodeGenerated;
            //                    newOption.OptionsName = "Options";
            //                    newOption.AddedDate = null;
            //                    newOption.ModifiedDate = null;
            //                    newOption.IP = null;
            //                    _asi_optionsRepository.Insert(newOption);
            //                }
            //            }
            //        }
            //    }
            //}
            //catch { }
            //#endregion

            //#region AddShape
            //try
            //{
            //    if (record.Attributes != null && record.Attributes.Shapes != null)
            //    {
            //        foreach (var data in record.Attributes.Shapes.Values)
            //        {
            //            if (data.Name != null)
            //            {
            //                var option = _asi_optionsRepository.Table.Where(x => x.OptionsDesc != null && x.OptionsDesc.ToLower() == data.Name.ToLower() && x.OptionsName.ToLower() == "Shape".ToLower() && x.ProductCode.ToLower() == productCodeGenerated.ToLower()).FirstOrDefault();
            //                if (option == null)
            //                {
            //                    ASI_Options newOption = new ASI_Options();
            //                    newOption.OptionCatId = 0;
            //                    newOption.OptionsDesc = data.Name;
            //                    newOption.ProductCode = productCodeGenerated;
            //                    newOption.OptionsName = "Shape";
            //                    newOption.AddedDate = null;
            //                    newOption.ModifiedDate = null;
            //                    newOption.IP = null;
            //                    _asi_optionsRepository.Insert(newOption);
            //                }
            //            }

            //        }
            //    }
            //}
            //catch { }
            //#endregion

            //#region AddImprintLocation
            //try
            //{
            //    if (record.Imprinting != null && record.Imprinting.Locations != null)
            //    {
            //        foreach (var data in record.Imprinting.Locations.Values)
            //        {
            //            if (data != null)
            //            {
            //                var option = _asi_optionsRepository.Table.Where(x => x.OptionsDesc != null && x.OptionsDesc.ToLower() == data.ToString().ToLower() && x.OptionsName.ToLower() == "Imprint Location(s)".ToLower() && x.ProductCode.ToLower() == productCodeGenerated.ToLower()).FirstOrDefault();
            //                if (option == null)
            //                {
            //                    ASI_Options newOption = new ASI_Options();
            //                    newOption.OptionCatId = 0;
            //                    newOption.OptionsDesc = data.ToString();
            //                    newOption.ProductCode = productCodeGenerated;
            //                    newOption.OptionsName = "Imprint Location(s)";
            //                    newOption.AddedDate = null;
            //                    newOption.ModifiedDate = null;
            //                    newOption.IP = null;
            //                    _asi_optionsRepository.Insert(newOption);
            //                }

            //            }

            //        }
            //    }
            //}
            //catch { }
            //#endregion


            return str;

        }

        public void AddImages(Result record, string productCodeGenerated)
        {
            _dbContext.ExecuteSqlCommand("delete from ASI_Picture where lower(ProductCode)=lower('" + productCodeGenerated + "')");
            string PictureCode = "";
            int displayOrder = 0;
            var PictureData = _asi_PictureRepository.Table.Where(x => x.ProductCode == productCodeGenerated).ToList();
            if (record.ImageUrl != null)
            {
                PictureCode = new String(record.ImageUrl.Where(x => char.IsDigit(x)).ToArray());
                try
                {
                    if (!PictureData.Any(x => x.PictureCode == PictureCode))
                    {
                        displayOrder++;
                        var mainImageURL = ApiUri + record.ImageUrl + "?size=large";
                        WebClient webClient = new WebClient();

                        ASI_Picture asiPicture = new ASI_Picture();
                        asiPicture.MimeType = "image/jpeg";
                        asiPicture.PictureBinary = webClient.DownloadData(mainImageURL);
                        asiPicture.PictureCode = PictureCode;
                        asiPicture.ProductCode = productCodeGenerated;
                        asiPicture.DisplayOrder = displayOrder;
                        _asi_PictureRepository.Insert(asiPicture);
                        PictureData.Add(asiPicture);
                    }
                }
                catch (Exception ex) { }

            }
            if (record.Images != null)
            {

                foreach (var image in record.Images)
                {

                    PictureCode = new String(image.Where(x => char.IsDigit(x)).ToArray());
                    try
                    {
                        if (!PictureData.Any(x => x.PictureCode == PictureCode))
                        {
                            displayOrder++;
                            WebClient webClient = new WebClient();
                            var mainImageURL = ApiUri + image + "?size=large";
                            ASI_Picture asiPicture = new ASI_Picture();
                            asiPicture.MimeType = "image/jpeg";
                            asiPicture.PictureBinary = webClient.DownloadData(mainImageURL);
                            asiPicture.PictureCode = PictureCode;
                            asiPicture.ProductCode = productCodeGenerated;
                            asiPicture.DisplayOrder = displayOrder;
                            _asi_PictureRepository.Insert(asiPicture);
                            PictureData.Add(asiPicture);
                        }
                    }
                    catch (Exception ex) { }
                }
            }

        }

        public void GenerateThumbNails(Bitmap bitmap, int width, int height, string fileName, string filePath)
        {
            using (Bitmap newBitmap = new Bitmap(bitmap))
            {
                newBitmap.SetResolution(width, height);
                using (var newImage = ScaleImage(newBitmap, width, height))
                {
                    newImage.Save(filePath + "\\" + fileName, ImageFormat.Jpeg);
                }
            }
        }

        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }

        public void AddCategory(Result record, string productCodeGenerated, int ASIProductID = 0)
        {
            try
            {
                _dbContext.ExecuteSqlCommand("delete from ASI_Product_Category_Mapping where lower(ProductCode)=lower('" + productCodeGenerated + "')");

                var categoryData = new List<ASI_Product_Category_Mapping>();

                if (record.Categories.Count() > 0)
                {
                    foreach (var item in record.Categories)
                    {
                        int ParentCategory = 0;
                        //if (item.Parent != null)
                        //{
                        //    if (!string.IsNullOrEmpty(item.Parent.Name))
                        //    {
                                

                        //        // Insert Parent Category
                        //        //if (!categoryData.Where(x => x.CategoryId == item.Parent.Id).Any())
                        //        //{
                        //        //    ASI_Product_Category_Mapping productCategory = new ASI_Product_Category_Mapping();
                        //        //    productCategory.ASIProductID = ASIProductID;
                        //        //    productCategory.ProductCode = productCodeGenerated;
                        //        //    productCategory.CategoryId = item.Parent.Id;
                        //        //    productCategory.CategoryName = string.Empty;
                        //        //    productCategory.IsRemoved = false;
                        //        //    productCategory.ParentCategoryId = 0;
                        //        //    productCategory.IsParentCategory = true;
                        //        //    productCategory.ParentCategoryName = item.Parent.Name;
                        //        //    _asi_ProductCategoryMappingRepository.Insert(productCategory);
                        //        //    categoryData.Add(productCategory);
                        //        //    ParentCategory = productCategory.Id;
                        //        //    ParentCategoryName = productCategory.ParentCategoryName;
                        //        //}
                        //        //else {
                        //        //    if (categoryData.Where(x => x.CategoryId == item.Parent.Id).FirstOrDefault() != null)
                        //        //    {
                        //        //        ParentCategory = categoryData.Where(x => x.CategoryId == item.Parent.Id).FirstOrDefault().Id;
                        //        //        ParentCategoryName =categoryData.Where(x => x.CategoryId == item.Parent.Id).FirstOrDefault().ParentCategoryName;
                        //        //    }
                        //        //}
                        //    }
                        //}

                        if (!string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.Id))
                        {
                            if (!categoryData.Where(x => x.CategoryId == item.Id).Any())
                            {
                                ASI_Product_Category_Mapping productCategory = new ASI_Product_Category_Mapping();
                                productCategory.ASIProductID = ASIProductID;
                                productCategory.ProductCode = productCodeGenerated;
                                productCategory.CategoryId = item.Id;
                                productCategory.CategoryName = item.Parent != null?item.Name:string.Empty;
                                productCategory.IsRemoved = false;
                                productCategory.IsParentCategory = false;
                                productCategory.ParentCategoryId = ParentCategory;
                                productCategory.ParentCategoryName= item.Parent != null?item.Parent.Name:item.Name;
                                _asi_ProductCategoryMappingRepository.Insert(productCategory);
                                categoryData.Add(productCategory);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {


            }
        }

        #endregion

        #endregion

    }
}
