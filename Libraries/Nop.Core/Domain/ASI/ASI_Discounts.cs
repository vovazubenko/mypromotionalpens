using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.ASI
{
    public partial class ASI_Discounts : BaseEntity
    {
        public DateTime? AddedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
        public string IP { get; set; }

        public int DisCountAutoId { get; set; }

        public string Name { get; set; }

        public long? MinQty { get; set; }

        public long? MaxQty { get; set; }

        public string DisCountType { get; set; }

        public decimal? DisCountValue { get; set; }

        public string LastModified { get; set; }

        public string Span { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public decimal? MinOrderPrice { get; set; }

        public decimal? MaxOrderPrice { get; set; }

        public string Apply_to_All_Orders { get; set; }

        public long? LastModby { get; set; }

        public string CouponCode { get; set; }

        public string OneTimeUse { get; set; }

        public string Cannot_Use_With_Any_Other { get; set; }

        public decimal? Taxable_DiscountAfterTax { get; set; }

        public string CouponUsage { get; set; }
    }
}
