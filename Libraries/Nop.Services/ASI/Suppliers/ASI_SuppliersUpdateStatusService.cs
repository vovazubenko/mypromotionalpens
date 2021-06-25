using Nop.Core.Data;
using Nop.Core.Domain.ASI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.ASI.Suppliers
{
   public partial class ASI_SuppliersUpdateStatusService :IASI_SuppliersUpdateStatusService
    {
        #region Fields
        private readonly IRepository<ASI_SuppliersUpdateStatus> _asi_SuppliersUpdateStatus;
        #endregion

        #region Ctor
        public ASI_SuppliersUpdateStatusService(IRepository<ASI_SuppliersUpdateStatus> asi_SuppliersUpdateStatus)
            
        {
            this._asi_SuppliersUpdateStatus = asi_SuppliersUpdateStatus;
        }
        #endregion

        #region Method
        public int GetRunningSupplierTaskCount()
        {
            return _asi_SuppliersUpdateStatus.Table.Where(x => x.Status == SupplierUpdateStatus.Running || x.Status == SupplierUpdateStatus.Updating).Count();
        }
        public ASI_SuppliersUpdateStatus GetCurrentlyRunningSupplierStatus()
        {
            return _asi_SuppliersUpdateStatus.Table
                .Where(x => x.Status != SupplierUpdateStatus.Completed && x.Status == SupplierUpdateStatus.Error)
                .FirstOrDefault();
        }
        #endregion
    }
}
