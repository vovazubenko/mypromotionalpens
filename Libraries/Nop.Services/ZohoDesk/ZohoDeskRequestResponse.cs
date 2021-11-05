using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.ZohoDesk
{
    class ZohoDeskRequestResponse
    {
    }
    public class CreateTicket
    {
        public string subCategory { get; set; }
        public string productId { get; set; }
        public string contactId { get; set; }
        public string subject { get; set; }
        public string dueDate { get; set; }
        public string departmentId { get; set; }
        public string channel { get; set; }
        public string description { get; set; }
        public string priority { get; set; }
        public string classification { get; set; }
        public string assigneeId { get; set; }
        public string phone { get; set; }
        public string category { get; set; }
        public string email { get; set; }
        public string status { get; set; }
    }
    public class CreateTicketReply
    {
        public string channel { get; set; }
        public string to { get; set; }
        public string fromEmailAddress { get; set; }
        public string contentType { get; set; }
        public string content { get; set; }
        public string isForward { get; set; }
        public string uploads { get; set; }
    }

    public class TicketReplyResponse
    {
        public string summary { get; set; }
        public string cc { get; set; }
        public string bcc { get; set; }
        public string channel { get; set; }
        public bool isPrivate { get; set; }
        public string content { get; set; }
        public bool isForward { get; set; }
        public bool hasAttach { get; set; }
        public string responderId { get; set; }
        public DateTime createdTime { get; set; }
        public string id { get; set; }
        public string to { get; set; }
        public string fromEmailAddress { get; set; }
        public string status { get; set; }
        public string direction { get; set; }
    }

    public class GetTicketReplies
    {

        public string zThreadId { get; set; }
        public string ZohoTicketNumber { get; set; }
        public string VolusionTicketNumber { get; set; }
        public string toemail { get; set; }
        public string fromEmailAddress { get; set; }
        public string content { get; set; }
        public string vReplyId { get; set; }
        public string lastmodified { get; set; }

    }

    public class TicketComments
    {
        public string isPublic { get; set; }
        public string content { get; set; }
    }
    public class TicketCommentResponse
    {
        public string modifiedTime { get; set; }
        public string commentedTime { get; set; }
        public string isPublic { get; set; }
        public string id { get; set; }
        public string content { get; set; }
        public string commenterId { get; set; }
    }

    public class CreateZohoContact
    {
        public string zip { get; set; }
        public string lastName { get; set; }
        public string country { get; set; }
        public string secondaryEmail { get; set; }
        public string city { get; set; }
        public string facebook { get; set; }
        public string mobile { get; set; }
        public string description { get; set; }
        public string ownerId { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string accountId { get; set; }
        public string firstName { get; set; }
        public string twitter { get; set; }
        public string phone { get; set; }
        public string street { get; set; }
        public string state { get; set; }
        public string email { get; set; }
    }
    public class ResponseZohoContact
    {
        public object firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public object secondaryEmail { get; set; }
        public string phone { get; set; }
        public object type { get; set; }
        public string ownerId { get; set; }
        public string accountId { get; set; }
        public object zohoCRMContact { get; set; }
        public string id { get; set; }
    }
    public class ResponseZohoContactData
    {
        public List<ResponseZohoContact> data { get; set; }
    }
    public class ZohoTicketRespose
    {
        public DateTime modifiedTime { get; set; }
        public string ticketNumber { get; set; }
        public string subCategory { get; set; }
        public string subject { get; set; }
        //public DateTime dueDate { get; set; }
        public string departmentId { get; set; }
        public string channel { get; set; }
        public string description { get; set; }
        public object resolution { get; set; }
        public object closedTime { get; set; }
        public string approvalCount { get; set; }
        public string timeEntryCount { get; set; }
        public DateTime createdTime { get; set; }
        public string id { get; set; }
        public string email { get; set; }
        public DateTime customerResponseTime { get; set; }
        public object productId { get; set; }
        public string contactId { get; set; }
        public string threadCount { get; set; }
        public string priority { get; set; }
        public object classification { get; set; }
        public string assigneeId { get; set; }
        public string commentCount { get; set; }
        public string taskCount { get; set; }
        public string phone { get; set; }
        public string webUrl { get; set; }
        public string attachmentCount { get; set; }
        public string category { get; set; }
        public string status { get; set; }
    }

    public class GenerateAccessTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
