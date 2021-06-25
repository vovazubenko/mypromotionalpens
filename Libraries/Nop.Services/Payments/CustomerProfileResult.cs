using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Payments
{
    public partial class CustomerProfileResult
    {

        public string CustomerProfileId { get; set; }
        public string message { get; set; }
        public string Email { get; set; }
        public int CustomerId { get; set; }
        public int resultCode { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
