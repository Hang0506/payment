using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class VoucherRepository : ITransientDependency, IVoucherRepository
    {
        private readonly IRepository<Voucher, Guid> _repository;

        public VoucherRepository(IRepository<Voucher, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<List<Voucher>> GetByTransactionId(Guid transId)
        {
            return await _repository.GetListAsync(voucher => voucher.TransactionId == transId);
        }

        public async Task<Voucher> InsertVoucher(Voucher voucher)
        {
            voucher.CreatedDate = DateTime.Now;
            voucher = await _repository.InsertAsync(voucher, true);
            return voucher;
        }

        public async Task<List<Voucher>> GetByTransactionIds(List<Guid> transIds)
        {
            return await _repository.GetListAsync(voucher => transIds.Contains(voucher.TransactionId.Value));
        }
    }
}
