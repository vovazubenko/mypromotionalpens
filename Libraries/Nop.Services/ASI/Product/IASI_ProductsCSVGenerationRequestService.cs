using Nop.Core.Domain.ASI;

namespace Nop.Services.ASI.Product
{
    public partial interface IASI_ProductsCSVGenerationRequestService
    {
        ASI_ProductsCSVGenerationRequests GetCurrentRunningRequest();
        bool IsCSVGenerationRequestRunning();
    }
}
