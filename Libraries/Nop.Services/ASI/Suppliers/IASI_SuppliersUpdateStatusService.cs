using Nop.Core.Domain.ASI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.ASI.Suppliers
{
    public partial interface IASI_SuppliersUpdateStatusService 
    {
        int GetRunningSupplierTaskCount();
        ASI_SuppliersUpdateStatus GetCurrentlyRunningSupplierStatus();
    }
}
