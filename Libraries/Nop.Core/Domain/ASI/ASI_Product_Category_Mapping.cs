using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.ASI
{
    public partial class ASI_Product_Category_Mapping : BaseEntity
    {
        public int ASIProductID { get; set; }

        public string ProductCode { get; set; }

        public string CategoryName { get; set; }

        public string CategoryId { get; set; }

        public bool IsRemoved { get; set; }

        public int ParentCategoryId { get; set; }

        public bool IsParentCategory { get; set; }

        public string ParentCategoryName { get; set; }




    }
}
