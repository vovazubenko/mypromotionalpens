using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.ASI;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Tasks;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.ASI.Product;
using Nop.Services.ASI.Suppliers;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

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

        #endregion Prop

        #region Fields

        private static string ApiUri = "";
        private static string ApiApplicationId = "";
        private static string ApiApplicationSecret = "";
        private static string ResultsPerPage = "";
        private static string SearchApiUri = "";
        private static string SuplierSearchURL = "";
        private static string AuthorizationHeader = "";
        #endregion Fields

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

        #endregion ctor

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

                    _logger.Information("Start Supplier Process");

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
                    if (response == null)
                    {
                        return;
                    }
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
                        supplierUpdateStatus.Status = SupplierUpdateStatus.Completed;
                        _dbContext.ExecuteSqlCommand("EXEC [ImportASISupplier]", true, 30000);
                        _logger.Information("End Supplier Process , Total Supplier Found : => '" + supplierUpdateStatus.SuccessfullyRetrivedRcordCount + "'");

                    }
                    else
                        supplierUpdateStatus.Status = SupplierUpdateStatus.WaitingToRun;
                    _ASI_SuppliersUpdateStatusRepository.Update(supplierUpdateStatus);
                }
                else
                {
                    supplierUpdateStatus.Status = SupplierUpdateStatus.Completed;
                    _ASI_SuppliersUpdateStatusRepository.Update(supplierUpdateStatus);
                    _dbContext.ExecuteSqlCommand("EXEC [ImportASISupplier]", true, 30000);
                    _logger.Information("End Supplier Process , Total Supplier Found : => '" + supplierUpdateStatus.SuccessfullyRetrivedRcordCount + "'");

                }
            }
            catch (Exception ex)
            {
                supplierUpdateStatus.Status = SupplierUpdateStatus.WaitingToRun;
                _ASI_SuppliersUpdateStatusRepository.Update(supplierUpdateStatus);
                _logger.Error("Error in GetAllSuppliers", ex);
                _logger.Information("End Supplier Process , Total Supplier Found : => '" + supplierUpdateStatus.SuccessfullyRetrivedRcordCount + "'");
            }
        }

        public XmlDocument GetResponseDocument(string searchUrl)
        {
            var result = new XmlDocument();
            try
            {
                var request = HttpWebRequest.Create(searchUrl);
                request.Headers.Add("Authorization", AuthorizationHeader);
                request.Method = "Get";


                result.Load(request.GetResponse().GetResponseStream());
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetResponseDocument :=> " + searchUrl, ex);
            }
            return result;
        }

        public int SaveSuppliersIndDB(XmlNode node, bool clearDataOfSuccpliers)
        {
            var dataSet = ConverttYourXmlNodeToDataSet(node);
            var settings = new DataSettingsManager();
            var sqlConnectiononn = new SqlConnection(settings.LoadSettings().DataConnectionString.ToString());
            using (SqlBulkCopy copy = new SqlBulkCopy(sqlConnectiononn))
            {
                if (dataSet.Tables["Supplier"] != null)
                {
                    copy.BulkCopyTimeout = 500;
                    copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Id", "SupplierId"));
                    copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Name", "Name"));
                    copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AsiNumber", "ASINumber"));
                    copy.DestinationTableName = "ASI_Suppliers";
                    sqlConnectiononn.Open();
                    if (clearDataOfSuccpliers)
                    {
                        string sqlTrunc = "TRUNCATE TABLE ASI_Suppliers";
                        SqlCommand cmd = new SqlCommand(sqlTrunc, sqlConnectiononn);
                        cmd.ExecuteNonQuery();
                    }
                    copy.WriteToServer(dataSet.Tables["Supplier"]);
                    sqlConnectiononn.Close();
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

        #endregion Supplier

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
                    if (currentRequest != null)
                    {
                        currentRequest.Status = ProductsCSVGenerationStatus.Failed;
                        _asi_ProductsCSVGenerationRequestsRepository.Update(currentRequest);

                    }
                }
                else if (currentRequest.Status == ProductsCSVGenerationStatus.Running || currentRequest.Status == ProductsCSVGenerationStatus.Updating)
                {
                    return;
                }
                else
                {
                    _logger.Information("Start importing ASI product for request :=>" + currentRequest.Id);
                    currentOption.Status = CurrentOptionStatus.Running;
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
                        //currentOption.AddedDate = DateTime.Now;
                        currentOption.ModifiedDate = DateTime.Now;
                        currentOption.Retrivedproducts = 0;
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
                        var result = GetResponseJSON(string.Format(SearchApiUri, currentOption.CurrentPage, searchQuery));
                        if (result != null)
                        {

                            if (currentOption.CurrentPage == 1)
                            {
                                int ResultsTotal = result.ResultsTotal;
                                currentOption.TotalPages = (int)Math.Ceiling((ResultsTotal / currentRequest.RecordsPerPage));
                                currentOption.TotalRecords = ResultsTotal;
                            }
                            currentRequest.Status = ProductsCSVGenerationStatus.Updating;
                            _asi_ProductsCSVGenerationRequestsRepository.Update(currentRequest);
                            if (result != null && result.Results != null && result.Results.Count > 0)
                            {

                                if (result.ResultsTotal > 500000)
                                {
                                    if (currentOption != null && currentOption.SearchOptionType == ProductsSearchOptionType.Supplier)
                                    {
                                        _logger.Warning("Product total exceed for Supplier :=>" + currentOption.SearchValue);
                                    }
                                    else if (currentOption != null && currentOption.SearchOptionType == ProductsSearchOptionType.Category)
                                    {
                                        _logger.Warning("Product total exceed for Category :=>" + currentOption.SearchValue);
                                    }
                                    currentOption.Status = CurrentOptionStatus.Failed;
                                    currentOption.ModifiedDate = DateTime.Now;
                                }
                                else {
                                    if (currentRequest.SuccessfullyRetrivedProductsCount < 0)
                                        currentRequest.SuccessfullyRetrivedProductsCount = 0;
                                    currentOption.Retrivedproducts += ProcessToAddProducts(taskId, currentRequest.Id, result);
                                    currentRequest.SuccessfullyRetrivedProductsCount += currentOption.Retrivedproducts;

                                    //#region remove after demo

                                    //// remove After Demo
                                    //if (currentRequest.SuccessfullyRetrivedProductsCount > 5)
                                    //{
                                    //    currentOption.CurrentPage = currentOption.TotalPages;
                                    //    currentOption.Status = CurrentOptionStatus.Completed;
                                    //    _asi_ProductsCSVGenerationRequestsRepository.Update(currentRequest);

                                    //}
                                    //// remove After Demo

                                    //#endregion remove after demo
                                }
                            }
                        }
                        if (currentOption.TotalPages == currentOption.CurrentPage)
                        {
                            currentOption.Status = CurrentOptionStatus.Completed;
                            currentOption.ModifiedDate = DateTime.Now;

                            if (currentRequest.SearchOptions.Where(x => x.Status != CurrentOptionStatus.Completed).Count() == 0)
                            {
                                //
                                //RemoveJunkImages(currentRequest.Id);
                                _dbContext.ExecuteSqlCommand("EXEC [ImportASIData] '" + currentRequest.Id + "'", true, 30000);

                                currentRequest.Status = ProductsCSVGenerationStatus.Completed;
                                bool fileExists = false;
                                currentRequest.GenerateFolderPath = "";
                                currentRequest.ImageZipFileExists = fileExists;
                                if (_asi_ProductsCSVGenerationRequestsRepository.Table.Count() > 0)
                                    currentRequest.ProductCSVExists = true;
                                else
                                    currentRequest.ProductCSVExists = false;


                                string type = currentRequest.SearchOptions.FirstOrDefault().SearchOptionType.ToString();
                                string typeName = string.Join(",", currentRequest.SearchOptions.Select(x => x.SearchValue).ToList());
                                string Message = @"Successfully retrived produts:=> '" + currentRequest.SuccessfullyRetrivedProductsCount + @"' 
                                    for '" + type + @"' : '" + typeName + "'";

                                _logger.Information(Message);

                                _logger.Information("End importing ASI product for request :=>" + currentRequest.Id);
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

                        currentOption.Status = CurrentOptionStatus.Completed;
                        currentOption.ModifiedDate = DateTime.Now;
                        if (currentRequest.SearchOptions.Where(x => x.Status != CurrentOptionStatus.Completed).Count() == 0)
                        {
                            //RemoveJunkImages(currentRequest.Id);
                            _dbContext.ExecuteSqlCommand("EXEC [ImportASIData] '" + currentRequest.Id + "'", true, 30000);

                            currentRequest.Status = ProductsCSVGenerationStatus.Completed;
                            bool fileExists = false;
                            currentRequest.GenerateFolderPath = "";
                            currentRequest.ImageZipFileExists = fileExists;
                            if (_asi_ProductsCSVGenerationRequestsRepository.Table.Count() > 0)
                                currentRequest.ProductCSVExists = true;
                            else
                                currentRequest.ProductCSVExists = false;

                            string type = currentRequest.SearchOptions.FirstOrDefault().SearchOptionType.ToString();
                            string typeName = string.Join(",", currentRequest.SearchOptions.Select(x => x.SearchValue).ToList());
                            string Message = @"Successfully retrived produts:=> '" + currentRequest.SuccessfullyRetrivedProductsCount + @"' 
                                    for '" + type + @"' : '" + typeName + "'";

                            _logger.Information(Message);

                            _logger.Information("End importing ASI product for request :=>" + currentRequest.Id);
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
                    currentRequest.Status = ProductsCSVGenerationStatus.Failed;
                    _asi_ProductsCSVGenerationRequestsRepository.Update(currentRequest);
                }
                _logger.Error("Error in AddASI_Products for request :=>" + currentRequest.Id, ex);

            }

        }

        public ProductResult GetResponseJSON(string searchUrl)
        {
            ProductResult productsResult = new ProductResult();
            try
            {
                var request = HttpWebRequest.Create(searchUrl);
                request.Headers.Add("Authorization", AuthorizationHeader);
                request.Method = "Get";
                StreamReader r = new StreamReader(request.GetResponse().GetResponseStream());
                string response = r.ReadToEnd();
                productsResult = JsonConvert.DeserializeObject<ProductResult>(response);
            }
            catch (Exception e)
            {
                _logger.Error("Error in GetResponseJson Fro :=> " + searchUrl, e);
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
                    #region remove after demo
                    //if (added > 5)
                    //{
                    //    return added;
                    //}
                    //else {
                    //    if (AddProduct(taskId, requestId, product))
                    //        added++;
                    //}
                    #endregion

                    #region  Uncomment after demo
                    if (AddProduct(taskId, requestId, product))
                        added++;
                    #endregion
                }
            }
            return added;
        }

        public bool AddProduct(long taskId, int requestId, Result record)
        {

            bool added = false;
            try
            {
                if (record.Number != null && (record.Number.Contains("15765") || record.Number.Contains("GC0422") || record.Number.Contains("55155")))
                {
                }

                if (CanAddProduct(record, requestId))
                {
                    string productCode = ValidateProductCode(record.Number, requestId);
                    if (string.IsNullOrWhiteSpace(productCode))
                        return false;
                    var product = new ASI_Product();
                    product.requestId = requestId;
                    product.ProductName = record.Name;
                    product.FullDescription = GetDescription(record, productCode);
                    product.shortDescription = record.Description;
                    product.isProductExist = 0;
                    //product.MetaKeywords = GetProductKeywords(record,productCode);
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
                    product.HasDiscountsApplied = true;
                    product.SetupFee = Convert.ToDecimal(GetSetUpcost(record, productCode));
                    product.Material = GetMaterial(record, productCode);
                    product.Size = GetSize(record, productCode);
                    product.ImprintType = GetImprintType(record, productCode);
                    product.ImprintArea = GetImprintArea(record, productCode);
                    product.InkColor = GetInkColor(record, productCode);
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
                    product.NormalProductionDays = GetNormalProductiondays(record, productCode);
                    product.RushProductionDays = GetRushProductionDays(record, productCode);
                    product.ProductManufacturer = record.Supplier == null ? "" : record.Supplier.Name;
                    product.AddedDate = DateTime.Now;
                    product.ModifiedDate = DateTime.Now;

                    //weight calculation
                    if (record.Shipping != null && record.Shipping.ItemsPerPackage != 0) {
                        product.Weight = (record.Shipping.WeightPerPackage / record.Shipping.ItemsPerPackage);
                    }

                    
                    if (record.Shipping != null && record.Shipping.Dimensions != null) {
                        product.Length =string.IsNullOrEmpty(record.Shipping.Dimensions.Length)?0:Convert.ToDecimal(record.Shipping.Dimensions.Length);
                        product.Width = string.IsNullOrEmpty(record.Shipping.Dimensions.Width) ? 0 : Convert.ToDecimal(record.Shipping.Dimensions.Width);
                        product.Height = string.IsNullOrEmpty(record.Shipping.Dimensions.Height) ? 0 : Convert.ToDecimal(record.Shipping.Dimensions.Height);
                    }


                    ASI_ProductsAddedToCSV csvPRoduct = new ASI_ProductsAddedToCSV();
                    csvPRoduct.ProductCode = productCode;
                    csvPRoduct.RequestId = requestId;
                    csvPRoduct.AddedDate = DateTime.Now;
                    csvPRoduct.ModifiedDate = DateTime.Now;
                    if (record.Supplier != null)
                        csvPRoduct.SupplierName = record.Supplier.Name;
                    _aSI_ProductsAddedToCSV.Insert(csvPRoduct);

                    _asi_Product.Insert(product);
                    added = true;

                    if (added)
                    {
                        AddProductAttributes(record, productCode);
                        GetDiscounts(record, Convert.ToDouble(productPrice), productCode, requestId, product.Id);
                        AddImages(record, productCode);
                        AddCategory(record, productCode, product.Id);
                    }
                }
                else
                {
                    string SupplierName = "";
                    if (record.Supplier != null)
                        SupplierName = record.Supplier.Name;
                    _logger.Information("Skipping Adding Product Code:" + record.Number + " Product Id:" + record.Id +"For Supplier :"+ SupplierName);
                }



            }
            catch (Exception ex)
            {
                _logger.Error("Error in AddProduct :=> " + record.Number, ex);

            }
            return added;
        }

        public void GetDiscounts(Result record, double? basePrice, string productCodeGenerated, int requestId, int discountautoid)
        {
            try
            {
                bool isDiscount = false;

                _dbContext.ExecuteSqlCommand("delete from ASI_DiscountsApplyTo where lower(ProductCode)=lower('" + productCodeGenerated + "')");

                if (record.Prices != null)
                {
                    int discountId = 0;
                    if (_asi_DiscountsApplyToRepository.Table.Count() > 0)
                        discountId = _asi_DiscountsApplyToRepository.Table.Max(x => x.DiscountAutoId);
                    discountId++;

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
                            var discountApplied = _asi_DiscountsApplyToRepository.Table.Where(x => x.MinQty == minQuantity && x.MaxQty == maxQuantity && x.DisCountValue == dicountDecimal && x.ProductCode == productCodeGenerated).FirstOrDefault();
                            if (discountApplied == null)
                            {
                                discountApplied = new ASI_DiscountsApplyTo();
                                discountApplied.MinQty = minQuantity;
                                discountApplied.MaxQty = maxQuantity;
                                discountApplied.DisCountValue = dicountDecimal;
                                discountApplied.ProductCode = productCodeGenerated;
                                discountApplied.DiscountAutoId = discountautoid;
                                _asi_DiscountsApplyToRepository.Insert(discountApplied);
                                isDiscount = true;
                            }
                            if (discountApplied != null)
                            {
                                isDiscount = true;
                            }
                        }
                    }
                }

                if (isDiscount)
                {
                    _dbContext.ExecuteSqlCommand("update asi_product set HasDiscountsApplied='true' where lower(ProductCode)=lower('" + productCodeGenerated + "') and requestid='" + requestId + "'");
                }


            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetDiscounts :=> " + productCodeGenerated, ex);

            }
        }

        public bool CanAddProduct(Result record, int RequestId)
        {
            try
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

                //If product price is not available or blank or null exclude it.
                if (record.Prices == null || record.Prices.Count() == 0)
                    return false;

                // images are not available exclude it.
                if (string.IsNullOrWhiteSpace(record.ImageUrl))
                    return false;

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
                                            ) and RequestId='" + RequestId + "' ";
                    var product = _dbContext.SqlQuery<ASI_ProductsAddedToCSV>(sqlQuery).FirstOrDefault();
                    if (product != null)
                        return false;
                    if (record.Supplier.Name.Trim().ToLower().EndsWith("canada"))
                    {
                        return false;
                    }
                }

                if (record.Name != null)
                {

                    //if product with same code and same name avail then exclude it.
                    string regExp = record.Number + "-[0-9*]%";
                    var productCodeExpression = new Regex(regExp, RegexOptions.IgnoreCase);

                    string sqlQuery = @"select top 1 * from asi_product
                                        where ProductCode is not null and ProductManufacturer = '" + record.Supplier.Name + @"' and
                                        ProductManufacturer is not null and
                                        (ProductCode = '" + record.Number + @"') AND Requestid = '" + RequestId + @"'
                                       AND ProductName  = '" + record.Name + "'";
                    var product = _dbContext.SqlQuery<ASI_Product>(sqlQuery).FirstOrDefault();
                    if (product != null)
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in CanAddProduct :=> " + record.Number, ex);
                return false;
            }
        }

        public string ValidateProductCode(string productCode, int Requestid)
        {
            try
            {
                if (productCode.StartsWith("0"))
                {
                    productCode = "1-" + productCode.Substring(1, productCode.Length - 1);
                }


                productCode = productCode.Replace("/", "-");

                //getting product code 

                string sqlQuery = @"select * from ASI_ProductsAddedToCSV
                                        where  ProductCode is not null and
                                        SupplierName is not null and
                                        (
                                        ProductCode = '" + productCode + @"' or
                                        ProductCode Like '" + productCode + @"-[0-9*]%'
                                        ) and RequestId='" + Requestid + "' ";
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
                            productNumberBegingAppended = temp1 + 1;
                            //if (temp1 > productNumberBegingAppended)
                            //    productNumberBegingAppended = temp1;
                        }
                    }
                    productCode += "-" + productNumberBegingAppended;
                    // check in the csv file that is going to be generate to validate product code -- pending
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in ValidateProductCode :=> " + productCode, ex);
            }
            return productCode;
        }

        public string GetProductKeywords(Result record, string productCodeGenerated)
        {
            string productKeyword = "";
            try
            {
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
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetProductKeywords :=> " + productCodeGenerated, ex);
            }
            return productKeyword.TrimEnd(new char[] { ',', ';' });
        }

        public string GetDescription(Result record, string productCodeGenerated)
        {
            try
            {
                if (record == null || record.Number == null)
                    return "";
                // _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "Started Checking Discription Product Code:" + record.Number, "2");
                string finalDescription = "";
                if (record.Description != null)
                    finalDescription = @"<div class=""desc1"">" + record.Description + "</div>";


                //Get Materail
                var descMaterial = GetMaterial(record, productCodeGenerated);
                if (!string.IsNullOrEmpty(descMaterial))
                {
                    // how does we will know multi color is avaliable?
                    finalDescription += @"<div class=""desc2""><span class=""desc2title""> Material:</span> " + descMaterial + " </div>";
                }

                //Get Size
                var descSize = GetSize(record, productCodeGenerated);
                if (!string.IsNullOrEmpty(descSize))
                {
                    // how does we will know multi color is avaliable?
                    finalDescription += @"<div class=""desc3""><span class=""desc3title""> Size:</span> " + descSize.TrimEnd(new char[] { ',', ';' }) + " </div>";
                }

                //Add Imprint Method
                var imprintMethod = GetImprintType(record, productCodeGenerated);
                if (!string.IsNullOrEmpty(imprintMethod))
                {
                    finalDescription += @"<div class=""desc4""><span class=""desc4title""> Imprint Type: </span>";
                    finalDescription += imprintMethod.TrimEnd(new char[] { ',', ';' });
                    finalDescription += "</div>";

                }

                //Imprint Area
                var imprintArea = GetImprintArea(record, productCodeGenerated);
                if (!string.IsNullOrEmpty(imprintArea))
                {
                    finalDescription += @"<div class=""desc5""><span class=""desc5title""> Imprint Area: </span>";
                    finalDescription += imprintArea.TrimEnd(new char[] { ',', ';' });
                    finalDescription += "</div>";

                }

                //Add ink color
                var inkColor = GetInkColor(record, productCodeGenerated);
                if (!string.IsNullOrEmpty(inkColor))
                {
                    finalDescription += @"<div class=""desc6""><span class=""desc6title""> Ink Color: </span>";
                    finalDescription += inkColor.TrimEnd(new char[] { ',', ';' });
                    finalDescription += "</div>";

                }

                //Multi Color Imprint Available
                if (record.Imprinting != null && record.Imprinting.FullColorProcess)
                {
                    // how does we will know multi color is avaliable?
                    finalDescription += @"<div class=""desc7""><span class=""desc7title""> Multi Color Imprint Available:</span>Yes (call or chat for pricing) </div>";
                }

                //Price Includes
                if (string.IsNullOrEmpty(record.PriceIncludes))
                {
                    finalDescription += @"<div class=""desc8""><span class=""desc8title"">Price Includes:</span>  1 Color Imprint</div>";
                }
                else if (record.Imprinting != null && record.Imprinting.Methods != null && record.Imprinting.Methods.Values != null && record.Imprinting.Methods.Values.Count > 0)
                {
                    finalDescription += @"<div class=""desc8""><span class=""desc8title"">Price Includes:</span> " + record.PriceIncludes + "</div>";
                }

                //Normal Production
                var descNormalProductDays = GetNormalProductiondays(record, productCodeGenerated);
                if (!string.IsNullOrEmpty(descNormalProductDays))
                {
                    finalDescription += @"<div class=""desc9""><span class=""desc9title""> Normal Production: </span>";
                    finalDescription += descNormalProductDays.TrimEnd(new char[] { ',', ';' });
                    finalDescription += "</div>";

                }

                //Rush Production
                var descRushProductDays = GetRushProductionDays(record, productCodeGenerated);
                if (!string.IsNullOrEmpty(descRushProductDays))
                {
                    finalDescription += @"<div class=""desc10""><span class=""desc10title""> Rush Production: </span>";
                    finalDescription += descRushProductDays.TrimEnd(new char[] { ',', ';' });
                    finalDescription += "</div>";

                }

                return finalDescription.TrimEnd(new char[] { ',', ';' });
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetDescription :=> " + productCodeGenerated, ex);
                return "";
            }
        }

        public string GetSetUpcost(Result record, string productCodeGenerated)
        {
            string setupcost = "0";
            bool setUpCostFound = false;
            try
            {
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
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetSetUpcost :=> " + productCodeGenerated, ex);
            }
            return setupcost.TrimEnd(new char[] { ',', ';' });
        }

        public string GetMaterial(Result record, string productCodeGenerated)
        {
            string strMaterial = "";
            try
            {
                if (record.Attributes != null && record.Attributes.Materials != null && record.Attributes.Materials.Values != null)
                {
                    bool entered = false;
                    foreach (var material in record.Attributes.Materials.Values)
                    {
                        if (entered)
                            strMaterial += ";";
                        strMaterial += material.Name == null ? "" : material.Name;
                        entered = true;

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetMaterial :=> " + productCodeGenerated, ex);
            }
            return strMaterial.TrimEnd(new char[] { ',', ';' });
        }

        public string GetSize(Result record, string productCodeGenerated)
        {
            string strSize = "";
            try
            {
                if (record.Attributes != null && record.Attributes.Sizes != null && record.Attributes.Sizes.Values != null)
                {
                    if (record.Attributes.Sizes.Values is List<Value>)
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
                }
                if (string.IsNullOrEmpty(strSize))
                {
                    try
                    {
                        if (record.Imprinting != null && record.Imprinting.Sizes != null && record.Imprinting.Sizes.Values != null)
                        {
                            bool entered = false;
                            foreach (var size in record.Imprinting.Sizes.Values)
                            {
                                if (entered)
                                    strSize += ";";
                                string str = size.Description == null ? "" : size.Description;
                                str += " " + (size.Name == null ? "" : size.Name);
                                strSize += str;
                                entered = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    { _logger.Error("Error in GetSize :=> " + productCodeGenerated, ex); }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetSize :=> " + productCodeGenerated, ex);
            }
            return strSize.TrimEnd(new char[] { ',', ';' });
        }

        public string GetInkColor(Result record, string productCodeGenerated)
        {
            string strColor = "";
            try
            {
                if (record.Options != null)
                {
                    if (record.Options.Any(x => x.Name.ToLower() == "Ink Colors".ToLower()))
                    {
                        var inkColorData = record.Options.FirstOrDefault(x => x.Name.ToLower() == "Ink Colors".ToLower());
                        if (inkColorData != null && inkColorData.Values is object)
                        {

                            JArray jsonResponse = JArray.Parse(inkColorData.Values.ToString());
                            bool entered = false;
                            foreach (var color in jsonResponse)
                            {
                                if (entered)
                                    strColor += ";";
                                strColor += color;
                                entered = true;

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetInkColor :=> " + productCodeGenerated, ex);
            }
            return strColor.TrimEnd(new char[] { ',', ';' });
        }
        public string GetImprintArea(Result record, string productCodeGenerated)
        {
            string strArea = "";
            try
            {
                if (record.Imprinting != null && record.Imprinting.Sizes != null && record.Imprinting.Sizes.Values != null)
                {
                    bool entered = false;
                    foreach (var size in record.Imprinting.Sizes.Values)
                    {
                        if (entered)
                            strArea += ";";
                        string str = size.Description == null ? "" : size.Description;
                        str += " " + (size.Name == null ? "" : size.Name);
                        strArea += str;
                        entered = true;
                    }
                }
            }
            catch (Exception ex)
            { _logger.Error("Error in GetImprintArea :=> " + productCodeGenerated, ex); }
            return strArea.TrimEnd(new char[] { ',', ';' });
        }
        public string GetImprintType(Result record, string productCodeGenerated)
        {
            string strType = "";
            try
            {
                if (record.Imprinting != null && record.Imprinting.Methods != null && record.Imprinting.Methods.Values != null)
                {
                    bool entered = false;
                    foreach (var value in record.Imprinting.Methods.Values)
                    {
                        if (entered)
                            strType += ";";
                        strType += value.Name == null ? "" : value.Name;
                        entered = true;

                    }
                }
            }
            catch (Exception ex)
            { _logger.Error("Error in GetImprintType :=> " + productCodeGenerated, ex); }
            return strType.TrimEnd(new char[] { ',', ';' });
        }

        public string GetNormalProductiondays(Result record, string productCodeGenerated)
        {
            string productionDays = "";
            try
            {
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
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetNormalProductiondays :=> " + productCodeGenerated, ex);
            }
            return productionDays.TrimEnd(new char[] { ',', ';' });
        }

        public string GetRushProductionDays(Result record, string productCodeGenerated)
        {
            string rushProductionDay = "";
            try
            {
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
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetRushProductionDays :=> " + productCodeGenerated, ex);
            }
            return rushProductionDay.TrimEnd(new char[] { ',', ';' });
        }

        public void AddProductAttributes(Result record, string productCodeGenerated)
        {
            _dbContext.ExecuteSqlCommand("delete from ASI_Options where lower(ProductCode)=lower('" + productCodeGenerated + "')");

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
            catch (Exception ex)
            {
                _logger.Error("Error in AddProductAttributes-> AddImprintColor :=> " + productCodeGenerated, ex);
            }

            #endregion AddImprintColor

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
            catch (Exception ex)
            {
                _logger.Error("Error in AddProductAttributes-> AddItemColor :=> " + productCodeGenerated, ex);
            }

            #endregion AddItemColor

            #region AddImprintTYpe
            //Imprint Type|
            try
            {
                if (record.Imprinting != null && record.Imprinting.Methods != null)
                {
                    foreach (var data in record.Imprinting.Methods.Values)
                    {
                        if (data.Name != null)
                        {
                            var option = _asi_optionsRepository.Table.Where(x => x.OptionsDesc != null && x.OptionsDesc.ToLower() == data.Name.ToLower() && x.OptionsName.ToLower() == "Imprint Type".ToLower() && x.ProductCode.ToLower() == productCodeGenerated.ToLower()).FirstOrDefault();
                            if (option == null)
                            {
                                ASI_Options newOption = new ASI_Options();
                                newOption.OptionCatId = 21;
                                newOption.OptionsDesc = data.Name;
                                newOption.ProductCode = productCodeGenerated;
                                newOption.OptionsName = "Imprint Type";
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


        }

        public void AddImages(Result record, string productCodeGenerated)
        {
            try
            {
                _dbContext.ExecuteSqlCommand("delete from ASI_Picture where lower(ProductCode)=lower('" + productCodeGenerated + "')"); //remove picture for productcode

                WebClient webClient = new WebClient();
                ASI_Picture asiPicture = new ASI_Picture();
                string PictureCode = "";
                int displayOrder = 0;
                var mainImageURL = "";
                var PictureData = _asi_PictureRepository.Table.Where(x => x.ProductCode == productCodeGenerated).ToList();
                if (record.ImageUrl != null)
                {
                    /// Insert Main Image
                    PictureCode = new String(record.ImageUrl.Where(x => char.IsDigit(x)).ToArray());

                    if (!PictureData.Any(x => x.PictureCode == PictureCode))
                    {
                        displayOrder++;
                        mainImageURL = ApiUri + record.ImageUrl + "?size=large";
                        asiPicture.MimeType = "image/jpeg";
                        asiPicture.PictureBinary = webClient.DownloadData(mainImageURL);
                        asiPicture.PictureCode = PictureCode;
                        asiPicture.ProductCode = productCodeGenerated;
                        asiPicture.DisplayOrder = displayOrder;
                        _asi_PictureRepository.Insert(asiPicture);
                        PictureData.Add(asiPicture);
                    }
                }

                /// Insert Extra Images
                if (record.Images != null)
                {
                    foreach (var image in record.Images)
                    {
                        PictureCode = new String(image.Where(x => char.IsDigit(x)).ToArray());

                        if (!PictureData.Any(x => x.PictureCode == PictureCode))
                        {
                            displayOrder++;
                            mainImageURL = ApiUri + image + "?size=large";
                            asiPicture.MimeType = "image/jpeg";
                            asiPicture.PictureBinary = webClient.DownloadData(mainImageURL);
                            asiPicture.PictureCode = PictureCode;
                            asiPicture.ProductCode = productCodeGenerated;
                            asiPicture.DisplayOrder = displayOrder;
                            _asi_PictureRepository.Insert(asiPicture);
                            PictureData.Add(asiPicture);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in AddImages :=> " + productCodeGenerated, ex);
            }
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
                        if (!string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.Id))
                        {
                            if (!categoryData.Where(x => x.CategoryId == item.Id).Any())
                            {
                                ASI_Product_Category_Mapping productCategory = new ASI_Product_Category_Mapping();
                                productCategory.ASIProductID = ASIProductID;
                                productCategory.ProductCode = productCodeGenerated;
                                productCategory.CategoryId = item.Id;
                                productCategory.CategoryName = item.Parent != null ? item.Name : string.Empty;
                                productCategory.IsRemoved = false;
                                productCategory.IsParentCategory = false;
                                productCategory.ParentCategoryId = 0;
                                productCategory.ParentCategoryName = item.Parent != null ? item.Parent.Name : item.Name;
                                _asi_ProductCategoryMappingRepository.Insert(productCategory);
                                categoryData.Add(productCategory);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in AddCategory :=> " + productCodeGenerated, ex);
            }
        }

        #endregion Product


        #region Remove junk images
        /// <summary>
        /// Call to remove junk images from folder for duplicate products
        /// </summary>
        /// <param name="requestid"></param>
        public void RemoveJunkImages(int requestid)
        {
            var Query = @"select pictureid from Product_Picture_Mapping pm
                            inner join Product p on p.Id = pm.ProductId
                            inner join ASI_Product ap on ap.ProductCode = p.Sku
                            where ap.requestId = '" + requestid + "'";
            var data = _dbContext.SqlQuery<int>(Query).ToList();


            foreach (var item in data)
            {
                //string filter = string.Format("*{0}.*", item.ToString("0000000"));
                //var thumbDirectoryPath = CommonHelper.MapPath("~/content/images/thumbs");

                //
                ////System.Diagnostics.Process.Start("CMD.exe", "/c del e:\\test\\*.txt");
                //System.Diagnostics.Process.Start("CMD.exe", "/c del '"+ filesToDelete + "'");

                string filter = string.Format("{0}*.*", item.ToString("0000000"));
                var thumbDirectoryPath = CommonHelper.MapPath("~/content/images/thumbs");
                string filesToDelete = thumbDirectoryPath + "\\" + "*" + item + "*";
                string[] currentFiles = System.IO.Directory.GetFiles(thumbDirectoryPath, filter, SearchOption.AllDirectories);
                foreach (string currentFileName in currentFiles)
                {
                    var thumbFilePath = GetThumbLocalPaths(currentFileName);
                    File.Delete(thumbFilePath);
                }

            }


        }

        protected virtual string GetThumbLocalPaths(string thumbFileName)
        {
            var thumbsDirectoryPath = CommonHelper.MapPath("~/content/images/thumbs");
            var _mediaSettings = EngineContext.Current.Resolve<MediaSettings>();
            if (_mediaSettings.MultipleThumbDirectories)
            {
                //get the first two letters of the file name
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(thumbFileName);
                if (fileNameWithoutExtension != null && fileNameWithoutExtension.Length > 3)
                {
                    var subDirectoryName = fileNameWithoutExtension.Substring(0, 3);
                    thumbsDirectoryPath = Path.Combine(thumbsDirectoryPath, subDirectoryName);
                    if (!System.IO.Directory.Exists(thumbsDirectoryPath))
                    {
                        System.IO.Directory.CreateDirectory(thumbsDirectoryPath);
                    }
                }
            }
            var thumbFilePath = Path.Combine(thumbsDirectoryPath, thumbFileName);
            return thumbFilePath;
        }

        #endregion

        #endregion Methods
    }
}