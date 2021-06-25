using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.ASI
{
    public partial class ASI_DiscountsApplyTo : BaseEntity
    {
        public long SyncId { get; set; }
        public int DiscountAutoId { get; set; }
        public string ProductCode { get; set; }
        public long? CategoryId { get; set; }
        public long? MinQty { get; set; }
        public long? MaxQty { get; set; }
        public decimal? DisCountValue { get; set; }

        public DateTime? AddedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
        public string IP { get; set; }

    }
}
