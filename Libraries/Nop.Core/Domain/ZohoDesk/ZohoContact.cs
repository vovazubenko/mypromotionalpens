using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.ZohoDesk
{
    public partial class ZohoContact : BaseEntity
    {
        public string zohoContactEmail { get; set; }
        public string zohoContactId { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
