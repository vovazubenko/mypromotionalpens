using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.ASI
{
    public class ASI_SuppliersUpdateStatus : BaseEntity
    {
        public SupplierUpdateStatus Status { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int TotalRecords { get; set; }
        public int SuccessfullyRetrivedRcordCount { get; set; }
        public double RecordsPerPage { get; set; }
        public DateTime? AddedDate { get; set; }
        
        public DateTime? ModifiedDate { get; set; }
        public string IP { get; set; }
    }
    public enum SupplierUpdateStatus
    {
        Started,
        Running,
        WaitingToRun,
        Updating,
        Completed,
        Error
    }
}
