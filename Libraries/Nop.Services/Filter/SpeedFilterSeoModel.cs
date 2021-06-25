using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Filter
{
    public partial class SpeedFilterSeoModel
    {
        public string Slug { get; set; }
        public string KeyWord { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeyWord { get; set; }
        public string HTag { get; set; }
        public string HeaderCopy { get; set; }
        public string HeaderTitle { get; set; }
        public string FooterContent1 { get; set; }
        public string FooterContent2 { get; set; }
        public string FooterContent3 { get; set; }
        public string FooterTitle1 { get; set; }
        public string FooterTitle2 { get; set; }
        public string FooterTitle3 { get; set; }
    }
}
