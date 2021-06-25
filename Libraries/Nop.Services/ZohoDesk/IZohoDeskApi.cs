using Nop.Core.Domain.ZohoDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.ZohoDesk
{
    public partial interface IZohoDeskApi
    {
        #region Contact 
        ResponseZohoContact CreateNewContact(string email, string name);
        CreateZohoContact GetNewContactdata(string email, string name);
        ZohoContact GetZohoContactByEmail(string email);
        ResponseZohoContact UpdateZohoContact(string email, string name, string zohoContactId);
        #endregion

        #region Ticket
        string CreateTickets(string name, string email, string subject, string description, string phone);
        string GetCreateTicketData(string contactId, string description, string email, string subject, string phone);
        #endregion
    }
}
