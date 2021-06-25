using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Payments
{
    public partial class CustomerProfileResponse
    {
        public string customerProfileId { get; set; }
        public string customerPaymentProfileId { get; set; }
        public string message { get; set; }
        public string refId { get; set; }
        public string sessionToken { get; set; }
        public string validationDirectResponse { get; set; }
        public int resultCode { get; set; }
        public bool Success { get; set; }
    }
}
