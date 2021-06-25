using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Payments
{
    public partial class CreditCardInfo : BaseEntity
    {
        public long PaymentProfileId { get; set; }
        public string CardNumber { get; set; }
        public int CustomerId { get; set; }
        public int? AddressId { get; set; }
        public int CardType { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Address Address { get; set; }
        public int ExpireMonth { get; set; }
        public int ExpireYear { get; set; }
        public string CVVNumber { get; set; }

        public string CustomerProfileId { get; set; }

        public string MaskedCreditCardNumber { get; set; }

        public string CardHolderName { get; set; }
    }
}
