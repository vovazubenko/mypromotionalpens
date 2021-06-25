using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.ASI
{
    public  class ASISetting : ISettings
    {
        public string ASIAPIURL { get; set; }
        public string ASIApplicationId { get; set; }
        public string ASIApplicationSecret { get; set; }
        public int ResultsPerPage { get; set; }
    }
}
