using Nop.Services.Localization;
using Nop.Services.Logging;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace Nop.Services.ZohoCRM
{
    public class ZohoCRMAPI
    {
        #region Prop
        private readonly ZohoCRMSetting _zohoCRMSetting;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Fields
        private static string zohocrmurl = "";
        public static string zohoAuthToken = "";

        #endregion

        #region ctor
        public ZohoCRMAPI(ZohoCRMSetting zohoCRMSetting, ILogger logger, ILocalizationService localizationService) {
            this._zohoCRMSetting = zohoCRMSetting;
            this._logger = logger;
            this._localizationService = localizationService;
            zohocrmurl = zohoCRMSetting.zohocrmurl;
            zohoAuthToken = zohoCRMSetting.zohoAuthToken;

        }
        #endregion

        #region  General Methods
        public String APIMethod(string modulename, string methodname, string recordId, string xmldata)
        {
            string uri = zohocrmurl + modulename + "/" + methodname + "?";
            /* Append your parameters here */
            string postContent = "scope=crmapi";
            postContent = postContent + "&authtoken=" + zohoAuthToken;//Give your authtoken
            if (methodname.Equals("insertRecords") || methodname.Equals("updateRecords"))
            {
                postContent = postContent + "&xmlData=" + xmldata;
            }
            if (methodname.Equals("updateRecords") || methodname.Equals("deleteRecords") || methodname.Equals("getRecordById"))
            {
                postContent = postContent + "&id=" + recordId;
            }
            string result = AccessCRM(uri, postContent);
            return result;
        }
        public  string AccessCRM(string url, string postcontent)
        {
            string responseFromServer = "";
            try {
                
                //request
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                byte[] byteArray = Encoding.UTF8.GetBytes(postcontent);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                //response
                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            catch (Exception ex) {
                _logger.Error(ex.Message,ex);
            }
                return responseFromServer;
            
        }
        #endregion

        #region Insert contact into zoho crm
        public string ZohoCrmInsertContact(string xmlData) {
            var result = APIMethod("Contacts", "insertRecords", string.Empty, xmlData);
            if (!string.IsNullOrWhiteSpace(result)) {
                var responseData= ZohoCrmXml.DeserializeXml<ZohoContactResponse>(result);
                if (responseData != null) {
                    if (responseData.Error.Any()) {
                        foreach (var item in responseData.Error) {
                            _logger.Error(_localizationService.GetResource("ContactUs.EnquiryErrorMessage")+item.Message);
                        }
                        result = "";
                    }
                    else if (!string.IsNullOrEmpty(Convert.ToString(responseData.Result.Recorddetail.FL[0].Val)))
                    {
                        string message = _localizationService.GetResource("ContactUs.EnquiryMessage");
                        _logger.Information(message+" "+Convert.ToString(responseData.Result.Recorddetail.FL[0].Text));
                    }
                }
            }
            return result;
        }
        #endregion

       
    }

    public static class ZohoCrmXml
    {
        #region Deserialize XML
        public static T DeserializeXml<T>(this string xmlString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString));
            var data = (T)serializer.Deserialize(memStream);
            memStream.Close();
            return data;
            //return new XmlSerializer(typeof(T)).Deserialize(memStream) as T;
        }
        #endregion
    }
}
