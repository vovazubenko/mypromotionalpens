using Nop.Core.Domain.Payments;

namespace Nop.Services.Payments
{
    public class CustomerPaymentProfileResponse
    {
        public CustomerPaymentProfileResponse()
        {
            PaymentProfile = new CreditCardInfo();
        }

        public string CustomerProfileId { get; set; }
        public long PaymentProfileId { get; set; }
        public string Message { get; set; }
        public int resultCode { get; set; }
        public bool Success { get; set; }
        public CreditCardInfo PaymentProfile { get; set; }
    }

    public partial class CustomerPaymentProfileResult
    {

        public string CustomerProfileId { get; set; }
        public string PaymentProfileId { get; set; }
        public string CardNumber { get; set; }
        public string ExpireDate { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }

    }
}