using FRTTMO.PaymentCore.Entities;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class RefundRepository : IRefundRepository, ITransientDependency
    {
        private readonly IRepository<Refund, Guid> _repository;
        private readonly IRepository<Transaction, Guid> _repositoryTransaction;
        private readonly IRepository<PaymentRequest, Guid> _repositoryPaymentRequest;

        public RefundRepository(
            IRepository<Refund, Guid> repository,
            IRepository<Transaction, Guid> repositoryTransaction,
            IRepository<PaymentRequest, Guid> repositoryPaymentRequest
        ) {
            _repository = repository;
            _repositoryTransaction = repositoryTransaction;
            _repositoryPaymentRequest = repositoryPaymentRequest;
        }

        public async Task<Refund> CreateTransaction(Refund refund)
        {
            refund.CreatedDate = DateTime.Now;
            refund = await _repository.InsertAsync(refund, true);
            return refund;
        }
    }
}
