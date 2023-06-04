using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IPaymentTransactionRepository
    {
        Task<PaymentSource> Insert(PaymentSource inItem);
        Task<PaymentSource> Get(string paymentCode);
        Task<List<PaymentSource>> GetListByPaymentCode(string paymentCode);
        Task<bool> IsCheckInfor(PaymentSource inItem);
    }
}
