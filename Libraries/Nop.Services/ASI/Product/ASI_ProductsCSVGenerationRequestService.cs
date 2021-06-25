using Nop.Core.Data;
using Nop.Core.Domain.ASI;
using System.Linq;

namespace Nop.Services.ASI.Product
{
    public partial class ASI_ProductsCSVGenerationRequestService :IASI_ProductsCSVGenerationRequestService
    {

        #region Fields
        private readonly IRepository<ASI_ProductsCSVGenerationRequests> _asi_ProductsCSVGenerationRequests;
        #endregion

        #region Ctor
        public ASI_ProductsCSVGenerationRequestService(IRepository<ASI_ProductsCSVGenerationRequests> asi_ProductsCSVGenerationRequests)

        {
            this._asi_ProductsCSVGenerationRequests = asi_ProductsCSVGenerationRequests;
        }
        #endregion


        #region Methods
        public ASI_ProductsCSVGenerationRequests GetCurrentRunningRequest()
        {
            
            return _asi_ProductsCSVGenerationRequests.Table.Where(x => x.Status != ProductsCSVGenerationStatus.Completed &&
                x.Status != ProductsCSVGenerationStatus.Failed).FirstOrDefault();
        }
        public bool IsCSVGenerationRequestRunning()
        {
            var record = _asi_ProductsCSVGenerationRequests.Table.Where(x => x.Status == ProductsCSVGenerationStatus.Running ||
              x.Status == ProductsCSVGenerationStatus.Started ||
              x.Status == ProductsCSVGenerationStatus.Updating ||
              x.Status == ProductsCSVGenerationStatus.WaitingToRun);
            return record.Count() > 0;
        }
        #endregion
    }
}
