using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.ASI
{
    public class ASI_ProductsAddedToCSV:BaseEntity
    {
        public string ProductCode { get; set; }
        public string SupplierName { get; set; }

        public int RequestId { get; set; }
        public DateTime? AddedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
