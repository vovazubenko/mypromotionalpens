using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Payments
{
    public partial class PaymentInfoResult
    {

        public PaymentInfoResult()
        {

        }

        public string Error { get; set; }
        public bool Success { get; set; }

        public string CustomerProfileId { get; set; }
        public string CustomerPaymentProfileId { get; set; }

    }
}
