using Nop.Services.Logging;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Core.Data;
using Nop.Services.Configuration;
using Nop.Services.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Linq;
using Nop.Core.Domain.ZohoDesk;

namespace Nop.Services.ZohoDesk
{
    public partial class ZohoDeskApi : IZohoDeskApi
    {
        #region Prop
        private readonly ZohoDeskSettings _zohoDeskSettings;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<StateProvince> _stateProvinceRepository;
        private readonly IRepository<ZohoTicket> _zohoTicketRepository;
        private readonly IRepository<ZohoContact> _zohoContactRepository;
        #endregion

        #region Fields
        private static string ZohoDeskAuthorizationId = "";
        public static string ZohoDeskOrganizationId = "";
        public static string ZohodeskDepartmentId = "";
        public static string ZohoDeskAgentId = "";
        public static string ZohoDeskAccountId = "";
        public static string ZohoDeskZohodeskapiurl = "";
        public static string ZohoDeskzohoEmailAddress = "";
        #endregion

        #region ctor
        public ZohoDeskApi(ZohoDeskSettings zohoDeskSettings, ILogger logger, ILocalizationService localizationService,
            IWorkContext workContext, IRepository<Country> countryRepository, IRepository<StateProvince> stateProvinceRepository
            , IRepository<ZohoTicket> zohoTicketRepository, IRepository<ZohoContact> zohoContactRepository
            , IRepository<Customer> customerRepository
            )
        {
            this._zohoDeskSettings = zohoDeskSettings;
            this._logger = logger;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._countryRepository = countryRepository;
            this._stateProvinceRepository = stateProvinceRepository;
            this._zohoTicketRepository = zohoTicketRepository;
            this._zohoContactRepository = zohoContactRepository;
            this._customerRepository = customerRepository;
            ZohoDeskAuthorizationId = zohoDeskSettings.ZohoDeskAuthorizationId;
            ZohoDeskOrganizationId = zohoDeskSettings.ZohoDeskOrganizationId;
            ZohodeskDepartmentId = zohoDeskSettings.ZohodeskDepartmentId;
            ZohoDeskAccountId = zohoDeskSettings.ZohoDeskAccountId;
            ZohoDeskZohodeskapiurl = zohoDeskSettings.ZohoDeskZohodeskapiurl;



        }
        #endregion

        #region Methods
        #region Contact
        
        public ResponseZohoContact CreateNewContact(string email, string name)
        {
            ResponseZohoContact contactResponse = new ResponseZohoContact();
            var contact = GetNewContactdata(email, name);
            try
            {
                contact.accountId = ZohoDeskAccountId;
                var data = JsonConvert.SerializeObject(contact);
                var request = HttpWebRequest.Create(ZohoDeskZohodeskapiurl + "contacts");
                request.Headers.Add("Authorization", "Zoho-authtoken " + ZohoDeskAuthorizationId);
                request.Headers.Add("orgId:" + ZohoDeskOrganizationId);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(Encoding.ASCII.GetBytes(data), 0, data.Length);
                }
                StreamReader r = new StreamReader(request.GetResponse().GetResponseStream());
                string response = r.ReadToEnd();
                contactResponse = JsonConvert.DeserializeObject<ResponseZohoContact>(response);

                _logger.Information("New zoho contact created:" + contactResponse.id);

            }
            catch (WebException ex)
            {
                StreamReader r = new StreamReader(ex.Response.GetResponseStream());
                string response = r.ReadToEnd();

                _logger.Error("Create new zoho contact error response :" + response, ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Create new zoho contact", ex);
            }
            return contactResponse;
        }

        public CreateZohoContact GetNewContactdata(string email, string name)
        {

            //var customer = _workContext.CurrentCustomer;
            var customer = _customerRepository.Table.Where(x=>x.Email.ToLower()==email.ToLower()).FirstOrDefault();
            var contact = new CreateZohoContact();
            if (customer != null)
            {
                try
                {

                    int countryId = 0;
                    int StateProvinceId = 0;
                    int.TryParse(customer.GetAttribute<string>(SystemCustomerAttributeNames.CountryId), out countryId);
                    int.TryParse(customer.GetAttribute<string>(SystemCustomerAttributeNames.StateProvinceId), out StateProvinceId);
                    var country = _countryRepository.GetById(countryId);
                    var state = _stateProvinceRepository.GetById(StateProvinceId);

                    contact.country = country == null ? " " : country.Name;

                    contact.description = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company) == null ? " " : customer.GetAttribute<string>(SystemCustomerAttributeNames.Company);

                    contact.email = email;

                    contact.firstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName) == null ? " " : customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);

                    contact.lastName =string.IsNullOrEmpty(Convert.ToString(customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName)))? email : customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);

                    contact.mobile = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone) == null ? customer.BillingAddress == null ? " " : customer.BillingAddress.PhoneNumber : customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                    
                    contact.ownerId = "";

                    contact.phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone) == null ? customer.BillingAddress == null ? " " : customer.BillingAddress.PhoneNumber : customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);

                    contact.state = state == null ? " " : state.Name;

                    contact.street = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress) == null ? " " : customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress);

                    contact.street = contact.street + " " + customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2) == null ? " " : customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2);

                    contact.title = "";

                    contact.zip = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode) == null ? customer.BillingAddress == null ? " " : customer.BillingAddress.ZipPostalCode : customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex);
                }
            }
            else {
                contact.city = " ";
                contact.country = " ";
                contact.description = " ";
                contact.email = email;
                contact.firstName = " ";
                contact.lastName = string.IsNullOrEmpty(name) ? email : name; ;
                contact.mobile = " ";
                contact.ownerId = " ";
                contact.state = " ";
                contact.street = " ";
                contact.title = " ";
                contact.zip = " ";
            }
            return contact;
        }
        public ZohoContact GetZohoContactByEmail(string email) {
            ZohoContact contact = _zohoContactRepository.Table.Where(x => x.zohoContactEmail == email).FirstOrDefault();
            return contact;
        }

        public ResponseZohoContact UpdateZohoContact(string email, string name,string zohoContactId) {
            ResponseZohoContact contactResponse = new ResponseZohoContact();
            var contact = GetNewContactdata(email, name);
            try
            {
                
                var data = JsonConvert.SerializeObject(contact);
                var request = HttpWebRequest.Create(ZohoDeskZohodeskapiurl + "contacts/"+zohoContactId);
                request.Headers.Add("Authorization", "Zoho-authtoken " + ZohoDeskAuthorizationId);
                request.Headers.Add("orgId:" + ZohoDeskOrganizationId);
                request.Method = "PUT";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(Encoding.ASCII.GetBytes(data), 0, data.Length);
                }
                StreamReader r = new StreamReader(request.GetResponse().GetResponseStream());
                string response = r.ReadToEnd();
                contactResponse = JsonConvert.DeserializeObject<ResponseZohoContact>(response);

                _logger.Information("zoho contact updated: " + zohoContactId);

            }
            catch (WebException ex)
            {
                StreamReader r = new StreamReader(ex.Response.GetResponseStream());
                string response = r.ReadToEnd();

                _logger.Error("Update zoho contact error response :" + response, ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Update zoho contact", ex);
            }
            return contactResponse;
        }
        #endregion

        #region Ticket
        public string CreateTickets(string name, string email, string subject, string description,string phone)
        {
            var webUrl ="";
            var contact = GetZohoContactByEmail(email);
            try
            {
                
                if (contact == null)
                {
                    //create new contact
                    var zohocontact = CreateNewContact(email, name);
                    if (string.IsNullOrEmpty(zohocontact.id))
                        return "";
                    //insert data to zoho contact
                    contact = new ZohoContact();
                        contact.zohoContactEmail = zohocontact.email;
                        contact.zohoContactId = zohocontact.id;
                        contact.CreateDate = DateTime.Now;
                        _zohoContactRepository.Insert(contact);
                }
                var ticketData = GetCreateTicketData(contact.zohoContactId, description, email, subject, phone);
                if (!string.IsNullOrEmpty(ticketData))
                {
                    var request = HttpWebRequest.Create(ZohoDeskZohodeskapiurl + "tickets");
                    request.Headers.Add("Authorization", "Zoho-authtoken " + ZohoDeskAuthorizationId);
                    request.Headers.Add("orgId:" + ZohoDeskOrganizationId);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.ContentLength = ticketData.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(Encoding.ASCII.GetBytes(ticketData), 0, ticketData.Length);
                    }
                    StreamReader r = new StreamReader(request.GetResponse().GetResponseStream());
                    string response = r.ReadToEnd();
                    var ticketRespose = JsonConvert.DeserializeObject<ZohoTicketRespose>(response);
                    if (!string.IsNullOrEmpty(ticketRespose.id))
                    {
                        ZohoTicket zohoTicket = new ZohoTicket();
                        zohoTicket.CreateDate = DateTime.Now;
                        zohoTicket.IsImported = false;
                        zohoTicket.VolusionTicketNumber = " ";
                        zohoTicket.Zohocontactemail = ticketRespose.email;
                        zohoTicket.ZohocontactId = ticketRespose.contactId;
                        zohoTicket.ZohoTicketNumber = ticketRespose.id;
                        _zohoTicketRepository.Insert(zohoTicket);
                        _logger.Information("New zoho ticket created:" + ticketRespose.subject);
                        webUrl = ticketRespose.webUrl;
                    }


                }

            }
            catch (WebException ex)
            {
                StreamReader r = new StreamReader(ex.Response.GetResponseStream());
                string response = r.ReadToEnd();


                _logger.Error("Create new zoho ticket error response :" + response, ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Create new zoho ticket :", ex);
            }
            return webUrl;
        }
        public string GetCreateTicketData(string contactId, string description, string email, string subject,string phone)
        {
            CreateTicket ticket = new CreateTicket();
            ticket.subCategory = "";
            ticket.productId = "";
            ticket.assigneeId = "";
            ticket.category = "";
            ticket.channel = "Email";
            ticket.classification = "-None-";
            ticket.contactId = contactId;
            ticket.departmentId = ZohodeskDepartmentId;
            ticket.description = description;
            ticket.dueDate = "";
            ticket.email = email;
            ticket.phone = phone;
            ticket.priority = "-None-";
            ticket.status = "Open";
            ticket.subject = subject;
            return JsonConvert.SerializeObject(ticket);
        }
        #endregion

        #endregion

    }
}
