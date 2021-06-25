using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.ZohoDesk
{
    public class ZohoDeskSettings : ISettings
    {
        public string ZohoDeskAuthorizationId { get; set; }
        public string ZohoDeskOrganizationId { get; set; }
        public string ZohodeskDepartmentId { get; set; }
        public string ZohoDeskAgentId { get; set; }
        public string ZohoDeskAccountId { get; set; }
        public string ZohoDeskZohodeskapiurl { get; set; }
        public string ZohoDeskzohoEmailAddress { get; set; }


    }
}
