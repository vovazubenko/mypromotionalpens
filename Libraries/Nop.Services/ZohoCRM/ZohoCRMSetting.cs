using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.ZohoCRM
{
    public partial class ZohoCRMSetting : ISettings
    {
        public string zohocrmurl { get; set; }
        public string zohoAuthToken { get; set; }
    }
}
