using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Payments;
using Nop.Core.Domain.Payments;

namespace Nop.Services.Payments
{
    public partial interface ICreditCardInfoService
    {
        PaymentInfoResult DeleteCreditCardInfo(CreditCardInfo ccInfo);
        CreditCardInfo GetCreditCardInfoById(int ccInfoId);
        IList<CreditCardInfo> GetCreditCardInfosByCustomerId(int customerId);
        PaymentInfoResult InsertCreditCardInfo(CreditCardInfo ccInfo);
        PaymentInfoResult UpdateCreditCardInfo(CreditCardInfo ccInfo);

        void UpdateCreditCardInfoNop(CreditCardInfo ccInfo);
    }
}
