using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Repositories
{
    public class PaymentTransactionRepository : ITransientDependency, IPaymentTransactionRepository
    {
        private readonly IRepository<PaymentSource, Guid> _repository;

        public PaymentTransactionRepository(IRepository<PaymentSource, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<PaymentSource> Insert(PaymentSource inItem)
        {
            inItem.CreatedDate = DateTime.Now;
            inItem.PaymentDate = PaymentCoreUtilities.GetDate(inItem.PaymentCode);
            inItem = await _repository.InsertAsync(inItem, true);
            return inItem;
        }
        public async Task<PaymentSource> Get(string paymentCode)
        {
            DateTime paymentDate = PaymentCoreUtilities.GetDate(paymentCode);
            return await _repository.FirstOrDefaultAsync(x => x.PaymentDate == paymentDate && x.PaymentCode == paymentCode);
        }
        public async Task<List<PaymentSource>> GetListByPaymentCode(string paymentCode)
        {
            DateTime paymentDate = PaymentCoreUtilities.GetDate(paymentCode);
            return await _repository.GetListAsync(x => x.PaymentDate == paymentDate && x.PaymentCode == paymentCode);
        }
        public async Task<bool> IsCheckInfor(PaymentSource inItem)
        {
            DateTime paymentDate = PaymentCoreUtilities.GetDate(inItem.PaymentCode);
            var checkAny = await _repository.AnyAsync(x => x.PaymentDate == paymentDate 
            && x.PaymentCode == inItem.PaymentCode 
            && x.SourceCode == inItem.SourceCode 
            && x.Type == inItem.Type);
            return checkAny;
        }
    }
}
