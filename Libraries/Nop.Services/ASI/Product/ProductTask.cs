using System;
using Nop.Services.Tasks;
using Nop.Core.Data;
using Nop.Data;
using Nop.Services.Logging;
using Nop.Core.Domain.ASI;
using Nop.Core.Domain.Tasks;
using Nop.Services.ASI.Suppliers;

namespace Nop.Services.ASI.Product
{
    public partial class ProductTask : ITask
    {
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
        public ProductTask(IASI_SuppliersUpdateStatusService supplierUpdateService
             , IRepository<ASI_SuppliersUpdateStatus> ASI_SuppliersUpdateStatusRepository
            , IDbContext dbContext
            , ILogger logger
            , ASISetting asiSetting
             , IScheduleTaskService ScheduleTaskService
            , IRepository<ScheduleTask> scheduleTaskRepository
            , IASI_ProductsCSVGenerationRequestService asi_ProductsCSVGenerationRequestService
            ,IRepository<ASI_Options> asi_optionsRepository
            , IRepository<ASI_ProductsAddedToCSV> aSI_ProductsAddedToCSV
            , IRepository<ASI_Product> asi_Product
            , IRepository<ASI_ProductsCSVGenerationRequests> asi_ProductsCSVGenerationRequestsRepository
            , IRepository<ASI_Discounts> asi_DiscountsRepository
            , IRepository<ASI_DiscountsApplyTo> asi_DiscountsApplyToRepository
            , IRepository<ASI_Picture> asi_PictureRepository
            , IRepository<ASI_Product_Category_Mapping> asi_ProductCategoryMappingRepository
            )
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
        }

        public void Execute()
        {
            ASI_API asi_api = new ASI_API(_supplierUpdateService, _ASI_SuppliersUpdateStatusRepository, _dbContext,
                   _logger, _asiSetting, _ScheduleTaskService, _scheduleTaskRepository, _asi_ProductsCSVGenerationRequestService
                   , _asi_optionsRepository, _aSI_ProductsAddedToCSV, _asi_Product, _asi_ProductsCSVGenerationRequestsRepository
                   , _asi_DiscountsRepository, _asi_DiscountsApplyToRepository, _asi_PictureRepository, _asi_ProductCategoryMappingRepository);
            try
            {
                asi_api.AddASI_Products(_asi_ProductsCSVGenerationRequestService);
            }
            catch (Exception e)
            {

            }
        }
    }
}
