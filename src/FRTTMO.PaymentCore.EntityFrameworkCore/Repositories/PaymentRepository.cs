using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class PaymentRepository : ITransientDependency, IPaymentRepository
    {
        private readonly IRepository<Payment, Guid> _repository;

        public PaymentRepository(IRepository<Payment, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Payment> Insert(Payment inItem)
        {
            inItem.CreatedDate = DateTime.Now;
            if (inItem.PaymentDate == null)
            {
                inItem.PaymentDate = PaymentCoreUtilities.GetDate(inItem.PaymentCode);
            }
            inItem = await _repository.InsertAsync(inItem, true);
            if (inItem.PaymentVersion == 1)
            {
                inItem.PaymentCode = "PM" + inItem.Id.ToString().Substring(0, 6).ToUpper() + inItem.PaymentCode;
                var outItem = await _repository.UpdateAsync(inItem, true);
                return outItem;
            }
            return inItem;
        }
        public async Task<Payment> Get(string paymentCode, DateTime? paymentDate)
        {
            if (!paymentDate.HasValue)
            {
                paymentDate = PaymentCoreUtilities.GetDate(paymentCode);
            }
            var payment = await _repository.GetListAsync(pay => pay.PaymentDate == paymentDate && pay.PaymentCode == paymentCode );
            return payment.OrderByDescending(pay => pay.PaymentVersion).FirstOrDefault();
        }
        public async Task<Payment> UpdateAsync(Payment payment)
        {
            payment.ModifiedDate = DateTime.Now;
            payment = await _repository.UpdateAsync(payment, true);
            return payment;
        }
        public async Task<List<Payment>> GetListPaymentCodeByDateTime(DateTime? startdate, DateTime? endDate)
        {
            return await _repository.GetListAsync(x => x.CreatedDate >= startdate && x.CreatedDate <= endDate);
        }
    }
}
