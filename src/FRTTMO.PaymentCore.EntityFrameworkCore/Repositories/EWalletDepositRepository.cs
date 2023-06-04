using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class EWalletDepositRepository : ITransientDependency, IEWalletDepositRepository
    {
        private readonly IRepository<EWalletDeposit, Guid> _repository;

        public EWalletDepositRepository(IRepository<EWalletDeposit, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<List<EWalletDeposit>> GetByTransactionId(Guid transId)
        {
            return await _repository.GetListAsync(e => e.TransactionId == transId);
        }

        public async Task<List<EWalletDeposit>> GetByTransactionIds(List<Guid> transIds)
        {
            return await _repository.GetListAsync(e => transIds.Contains(e.TransactionId.Value));
        }

        public async Task<EWalletDeposit> InsertEWallet(EWalletDeposit eWalletDeposit)
        {
            eWalletDeposit.CreatedDate = DateTime.Now;
            var result = await _repository.InsertAsync(eWalletDeposit, true);
            return result;
        }
    }
}
