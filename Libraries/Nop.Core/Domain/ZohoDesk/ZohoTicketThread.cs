using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.ZohoDesk
{
    public partial class ZohoTicketThread : BaseEntity
    {
        public string vTicketNumber { get; set; }

        public string zTicketNumber { get; set; }

        public string vTicketReplyId { get; set; }

        public string zTicketReplyId { get; set; }
    }
}
