using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.ZohoDesk
{
    public partial class ZohoTicket : BaseEntity
    {
        public string ZohoTicketNumber { get; set; }

        public string VolusionTicketNumber { get; set; }

        public string Zohocontactemail { get; set; }

        public string ZohocontactId { get; set; }

        public bool? IsImported { get; set; }

        public DateTime CreateDate { get; set; }

    }
}
