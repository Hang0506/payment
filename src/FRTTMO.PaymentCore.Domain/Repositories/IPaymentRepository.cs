using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> Insert(Payment inItem);
        Task<Payment> Get(string paymentCode, DateTime? paymentDate);
        Task<Payment> UpdateAsync(Payment payment);
        Task<List<Payment>> GetListPaymentCodeByDateTime(DateTime? startdate, DateTime? endDate);
    }
}
