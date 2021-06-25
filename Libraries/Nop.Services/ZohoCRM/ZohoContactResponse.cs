using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Nop.Services.ZohoCRM
{
   
    [XmlRoot(ElementName = "FL")]
    public class ZohoContactFL
    {
        [XmlAttribute(AttributeName = "val")]
        public string Val { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "recorddetail")]
    public class ZohoContactRecorddetail
    {
        [XmlElement(ElementName = "FL")]
        public List<ZohoContactFL> FL { get; set; }
    }

    [XmlRoot(ElementName = "result")]
    public class ZohoContactResult
    {
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }
        [XmlElement(ElementName = "recorddetail")]
        public ZohoContactRecorddetail Recorddetail { get; set; }
    }

    [XmlRoot(ElementName = "error")]
    public class ZohoContactError
    {
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }
    }

    [XmlRoot(ElementName = "response")]
    public partial class ZohoContactResponse
    {
        [XmlElement(ElementName = "result")]
        public ZohoContactResult Result { get; set; }
        [XmlAttribute(AttributeName = "uri")]
        public string Uri { get; set; }

        [XmlElement(ElementName = "error")]
        public List<ZohoContactError> Error { get; set; }
    }
}
