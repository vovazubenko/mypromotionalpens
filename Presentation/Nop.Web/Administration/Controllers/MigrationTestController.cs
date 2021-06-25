using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Messages;
using Nop.Web.Framework.Controllers;
using Nop.Core.Data;
using System.Xml.Serialization;
using System.Data;
using System.IO;
using System.Xml;
using System.Text;

namespace Nop.Admin.Controllers
{

    public class MigrationTestController : BaseAdminController
    {
        #region Fields
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<GenericAttribute> _gaRepository;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        #endregion

        #region Constructors

        public MigrationTestController(IRepository<Customer> customerRepository
            , IRepository<GenericAttribute> gaRepository, INewsLetterSubscriptionService newsLetterSubscriptionService)
        {
            this._customerRepository = customerRepository;
            this._gaRepository = gaRepository;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
        }

        #endregion

        // GET: MigrationTest
        #region TestMigration
        [AdminAuthorize(false)]
        public ActionResult CustomerMigration()
        {
            var data = ReadCustomerXml();
            var filtercustomerData = new List<VolusionCustomers>();
            var customerlist = new List<VolusionCustomers>();
            if (data != null)
            {
                filtercustomerData = data.Customers.OrderBy(x => x.Customerid).ToList();
            }
            customerlist = GetAllCustomer(0).OrderBy(x => x.Customerid).ToList();
            var query = (from f in filtercustomerData
                         join c in customerlist on 
                         new {id=f.Customerid==null?"": f.Customerid.ToLower().Trim(), em= f.Emailaddress==null?"":f.Emailaddress.ToLower().Trim()
                             ,fn =f.Firstname==null?"": f.Firstname.ToLower().Trim(), ln= f.Lastname==null?"":f.Lastname.ToLower().Trim()
                             ,com= f.Companyname==null?"":f.Companyname.ToLower().Trim(), ph= f.Phonenumber==null?"":f.Phonenumber.ToLower().Trim()
                             ,zip= f.Postalcode==null?"":f.Postalcode.ToLower().Trim(), a1= f.Billingaddress1==null?"":f.Billingaddress1.ToLower().Trim()
                             ,a2 = f.Billingaddress2==null?"":f.Billingaddress2.ToLower().Trim(),city = f.City==null?"": f.City.ToLower().Trim()
                             ,fax = f.Faxnumber==null?"":f.Faxnumber.ToLower().Trim(),cntry = f.Country==null?"":f.Country.ToLower().Trim()
                            /* ,st= f.State==null?"":f.State.ToLower().Trim()*/} 
                         equals
                         new
                         {
                             id = c.Customerid == null ? "" : c.Customerid.ToLower().Trim(),
                             em = c.Emailaddress == null ? "" : c.Emailaddress.ToLower().Trim()
                             ,
                             fn = c.Firstname == null ? "" : c.Firstname.ToLower().Trim(),
                             ln = c.Lastname == null ? "" : c.Lastname.ToLower().Trim()
                             ,
                             com = c.Companyname == null ? "" : c.Companyname.ToLower().Trim(),
                             ph = c.Phonenumber == null ? "" : c.Phonenumber.ToLower().Trim()
                             ,
                             zip = c.Postalcode == null ? "" : c.Postalcode.ToLower().Trim(),
                             a1 = c.Billingaddress1 == null ? "" : c.Billingaddress1.ToLower().Trim()
                             ,
                             a2 = c.Billingaddress2 == null ? "" : c.Billingaddress2.ToLower().Trim(),
                             city = c.City == null ? "" : c.City.ToLower().Trim()
                             ,
                             fax = c.Faxnumber == null ? "" : c.Faxnumber.ToLower().Trim(),
                             cntry = c.Country == null ? "" : c.Country.ToLower().Trim()
                            //,                             
                            // st = c.State == null ? "" :c.State.ToLower().Trim()
                         }
                        into joinedList
                         from sub in joinedList.DefaultIfEmpty()
                         select new CustomerData
                         {
                             filtercustomerData = f,
                             customerlist = sub,
                             
                         }).Where(x => x.customerlist == null).ToList();
            if (query.Count() > 0)
            {
                query.FirstOrDefault().AllcustData = customerlist;
                query.FirstOrDefault().VolusionTotal = filtercustomerData.Count();
                query.FirstOrDefault().NotFountInNopTotal = query.Count();
                query.FirstOrDefault().TotalcustomerInnop = customerlist.Count();
            }
            else {
                query.Add(new CustomerData
                {
                    VolusionTotal= filtercustomerData.Count(),
                    NotFountInNopTotal= query.Count(),
                    TotalcustomerInnop = customerlist.Count()
                });
            }
            return View(query);
        }

        #endregion

        #region Methods
        [NonAction]
        public IEnumerable<VolusionCustomers> GetAllCustomer(int id = 0, string email = null,
        string firstName = null, string lastName = null, string company = null, string phone = null, string zipPostalCode = null)
        {
            var query = _customerRepository.Table;
            if (id != 0)
                query = query.Where(c => c.Id == id);
            if (!String.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));
            if (!String.IsNullOrWhiteSpace(firstName))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.FirstName &&
                        z.Attribute.Value.Contains(firstName)))
                    .Select(z => z.Customer);
            }
            if (!String.IsNullOrWhiteSpace(lastName))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.LastName &&
                        z.Attribute.Value.Contains(lastName)))
                    .Select(z => z.Customer);
            }

            //search by company
            if (!String.IsNullOrWhiteSpace(company))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.Company &&
                        z.Attribute.Value.Contains(company)))
                    .Select(z => z.Customer);
            }
            //search by phone
            if (!String.IsNullOrWhiteSpace(phone))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.Phone &&
                        z.Attribute.Value.Contains(phone)))
                    .Select(z => z.Customer);
            }
            //search by zip
            if (!String.IsNullOrWhiteSpace(zipPostalCode))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.ZipPostalCode &&
                        z.Attribute.Value.Contains(zipPostalCode)))
                    .Select(z => z.Customer);
            }

            //query = query.OrderByDescending(c => c.CreatedOnUtc);
            var customers = query.ToList();

            return customers.Select(PrepareCustomerModelForList);
        }
        [NonAction]
        public CustomerExport ReadCustomerXml()
        {

            string xmlData = HttpContext.Server.MapPath("~/Customers_ZXQ4XRZ7VV.xml");//Path of the xml script  
            string xmlInputData = string.Empty;
            xmlInputData = System.IO.File.ReadAllText(xmlData, Encoding.GetEncoding("iso-8859-1"));


            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(CustomerExport));

            using (StringReader sr = new StringReader(xmlInputData))
            {
                return (CustomerExport)ser.Deserialize(sr);
            }


            //List<VolusionCustomers> list = (List<VolusionCustomers>)serializer.Deserialize(reader);
            return new CustomerExport();
        }

        [NonAction]
        protected virtual VolusionCustomers PrepareCustomerModelForList(Customer customer)
        {
            try
            {
                return new VolusionCustomers
                {
                    Customerid = Convert.ToString(customer.Id),
                    Emailaddress = customer.Email == null ? string.Empty : customer.Email,
                    Firstname = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName) == null ? string.Empty : customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                    Lastname = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName) == null ? string.Empty : customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                    Companyname = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company) == null ? string.Empty : customer.GetAttribute<string>(SystemCustomerAttributeNames.Company),
                    Phonenumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone) == null ? string.Empty : customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                    Postalcode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode) == null ? string.Empty : customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode),
                    Billingaddress1 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress) == null ? string.Empty : customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress),
                    Billingaddress2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2) == null ? string.Empty : customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2),
                    City = customer.GetAttribute<string>(SystemCustomerAttributeNames.City) == null ? string.Empty : customer.GetAttribute<string>(SystemCustomerAttributeNames.City),
                    Faxnumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax) == null ? string.Empty : customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax),
                    Country = customer.BillingAddress == null ? "" : customer.BillingAddress.Country == null ? "" : customer.BillingAddress.Country.Name,
                    State = customer.BillingAddress == null ? "" : customer.BillingAddress.StateProvince == null ? "" : customer.BillingAddress.StateProvince.Abbreviation,
                    stateAbbr = customer.BillingAddress == null ? "" : customer.BillingAddress.StateProvince == null ? "" : customer.BillingAddress.StateProvince.Name,
                };
            }
            catch (Exception ex)
            {
                return new VolusionCustomers();
            }
        }
        #endregion
    }

    [XmlRoot(ElementName = "Customers")]
    public class VolusionCustomers
    {
        [XmlElement(ElementName = "customerid")]
        public string Customerid { get; set; }
        [XmlElement(ElementName = "accesskey")]
        public string Accesskey { get; set; }
        [XmlElement(ElementName = "firstname")]
        public string Firstname { get; set; }
        [XmlElement(ElementName = "lastname")]
        public string Lastname { get; set; }
        [XmlElement(ElementName = "companyname")]
        public string Companyname { get; set; }
        [XmlElement(ElementName = "billingaddress1")]
        public string Billingaddress1 { get; set; }
        [XmlElement(ElementName = "billingaddress2")]
        public string Billingaddress2 { get; set; }
        [XmlElement(ElementName = "city")]
        public string City { get; set; }
        [XmlElement(ElementName = "state")]
        public string State { get; set; }
        [XmlElement(ElementName = "postalcode")]
        public string Postalcode { get; set; }
        [XmlElement(ElementName = "country")]
        public string Country { get; set; }
        [XmlElement(ElementName = "phonenumber")]
        public string Phonenumber { get; set; }
        [XmlElement(ElementName = "faxnumber")]
        public string Faxnumber { get; set; }
        [XmlElement(ElementName = "emailaddress")]
        public string Emailaddress { get; set; }
        [XmlElement(ElementName = "paysstatetax")]
        public string Paysstatetax { get; set; }
        [XmlElement(ElementName = "taxid")]
        public string Taxid { get; set; }
        [XmlElement(ElementName = "emailsubscriber")]
        public string Emailsubscriber { get; set; }
        [XmlElement(ElementName = "catalogsubscriber")]
        public string Catalogsubscriber { get; set; }
        [XmlElement(ElementName = "lastmodified")]
        public string Lastmodified { get; set; }
        [XmlElement(ElementName = "percentdiscount")]
        public string Percentdiscount { get; set; }
        [XmlElement(ElementName = "websiteaddress")]
        public string Websiteaddress { get; set; }
        [XmlElement(ElementName = "discountlevel")]
        public string Discountlevel { get; set; }
        [XmlElement(ElementName = "customertype")]
        public string Customertype { get; set; }
        [XmlElement(ElementName = "lastmodby")]
        public string Lastmodby { get; set; }
        [XmlElement(ElementName = "customer_isanonymous")]
        public string Customer_isanonymous { get; set; }
        [XmlElement(ElementName = "issuperadmin")]
        public string Issuperadmin { get; set; }
        [XmlElement(ElementName = "news1")]
        public string News1 { get; set; }
        [XmlElement(ElementName = "news2")]
        public string News2 { get; set; }
        [XmlElement(ElementName = "news3")]
        public string News3 { get; set; }
        [XmlElement(ElementName = "news4")]
        public string News4 { get; set; }
        [XmlElement(ElementName = "news5")]
        public string News5 { get; set; }
        [XmlElement(ElementName = "news6")]
        public string News6 { get; set; }
        [XmlElement(ElementName = "news7")]
        public string News7 { get; set; }
        [XmlElement(ElementName = "news8")]
        public string News8 { get; set; }
        [XmlElement(ElementName = "news9")]
        public string News9 { get; set; }
        [XmlElement(ElementName = "news10")]
        public string News10 { get; set; }
        [XmlElement(ElementName = "news11")]
        public string News11 { get; set; }
        [XmlElement(ElementName = "news12")]
        public string News12 { get; set; }
        [XmlElement(ElementName = "news13")]
        public string News13 { get; set; }
        [XmlElement(ElementName = "news14")]
        public string News14 { get; set; }
        [XmlElement(ElementName = "news15")]
        public string News15 { get; set; }
        [XmlElement(ElementName = "news16")]
        public string News16 { get; set; }
        [XmlElement(ElementName = "news17")]
        public string News17 { get; set; }
        [XmlElement(ElementName = "news18")]
        public string News18 { get; set; }
        [XmlElement(ElementName = "news19")]
        public string News19 { get; set; }
        [XmlElement(ElementName = "news20")]
        public string News20 { get; set; }
        [XmlElement(ElementName = "allow_access_to_private_sections")]
        public string Allow_access_to_private_sections { get; set; }
        [XmlElement(ElementName = "customer_notes")]
        public string Customer_notes { get; set; }
        [XmlElement(ElementName = "salesrep_customerid")]
        public string Salesrep_customerid { get; set; }
        [XmlElement(ElementName = "id_customers_groups")]
        public string Id_customers_groups { get; set; }
        [XmlElement(ElementName = "custom_field_custom2")]
        public string Custom_field_custom2 { get; set; }
        [XmlElement(ElementName = "custom_field_custom3")]
        public string Custom_field_custom3 { get; set; }
        [XmlElement(ElementName = "custom_field_custom4")]
        public string Custom_field_custom4 { get; set; }
        [XmlElement(ElementName = "custom_field_custom5")]
        public string Custom_field_custom5 { get; set; }
        [XmlElement(ElementName = "removed_from_rewards")]
        public string Removed_from_rewards { get; set; }
        [XmlElement(ElementName = "custom_field_industry")]
        public string Custom_field_industry { get; set; }

        
        [XmlIgnore]
        public string stateAbbr { get; set; }
    }

    [XmlRoot(ElementName = "Export")]
    public class CustomerExport
    {
        [XmlElement(ElementName = "Customers")]
        public List<VolusionCustomers> Customers { get; set; }
    }

    public class CustomerData
    {
        public VolusionCustomers filtercustomerData { get; set; }
        public VolusionCustomers customerlist { get; set; }

        public List<VolusionCustomers> AllcustData { get; set; }

        public int VolusionTotal { get; set; }
        public int NotFountInNopTotal { get; set; }
        public int TotalcustomerInnop { get; set; }
    }

    


}