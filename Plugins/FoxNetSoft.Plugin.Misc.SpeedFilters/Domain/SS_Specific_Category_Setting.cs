using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Domain
{
    public partial class SS_Specific_Category_Setting: BaseEntity
    {
        public int? CategoryId { get; set; }
        public string CustomKeyword { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeyword { get; set; }
        public string H1Tag { get; set; }
        public string HeaderCopy { get; set; }
        public string HeaderTitle { get; set; }
        public string FooterTitle1 { get; set; }
        public string FooterContent1 { get; set; }
        public string FooterTitle2 { get; set; }
        public string FooterContent2 { get; set; }
        public string FooterTitle3 { get; set; }
        public string FooterContent3 { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
