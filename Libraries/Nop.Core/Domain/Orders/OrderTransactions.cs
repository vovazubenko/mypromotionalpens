using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
    public partial class OrderTransactions : BaseEntity
    {
        public Guid TransferGuid { get; set; }

        public int OrderId { get; set;}

        public decimal TransferAmount { get; set; }

        public bool IsCredit { get; set; }

        public string TransferStatus { get; set; }
        public string TransferNote { get; set; }

        public string TransactionId { get; set; }
        public string TransactionCode { get; set; }
        public string TransactionResult { get; set; }
        public string TransferType { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public int CreatedBy { get; set; }
        public virtual Order Order { get; set; }

        public string CaptureTransactionId { get; set; }
        public string CaptureTransactionResult { get; set; }

    }
}
