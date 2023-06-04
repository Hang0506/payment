using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class DebitRepository : ITransientDependency, IDebitRepository
    {
        readonly IRepository<CreditSales, Guid> _repository;

        public DebitRepository(IRepository<CreditSales, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<CreditSales> InsertDebit(CreditSales debit)
        {
            debit.CreatedDate = DateTime.Now;
            debit = await _repository.InsertAsync(debit, true);
            return debit;
        }
        public async Task<List<CreditSales>> GetByTransactionIds(List<Guid> transIds)
        {
            return await _repository.GetListAsync(e => transIds.Contains(e.TransactionId.Value));
        } 
        public async Task<List<CreditSales>> GetByTransactionId(Guid transId)
        {
            return await _repository.GetListAsync(e => e.TransactionId == transId);
        }
    }
}
