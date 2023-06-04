using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Repositories
{
    public class TransferRepository : ITransientDependency, ITransferRepository
    {
        private readonly IRepository<Transfer, Guid> _repository;

        public TransferRepository(IRepository<Transfer, Guid> repository) => _repository = repository;

        public async Task<Transfer> Insert(Transfer entity)
        {
            entity.CreatedDate = DateTime.Now;
            entity = await _repository.InsertAsync(entity, true);
            return entity;
        }

        public async Task<List<Transfer>> GetByTransactionIds(List<Guid> transIds) => await _repository.GetListAsync(tr => transIds.Contains(tr.TransactionId.Value));

        public async Task<bool> CheckTransferNum(string transferNums) => await _repository.AnyAsync(c => transferNums == (c.TransferNum));

        public async Task<bool> HasTransferDepositNotIsConfirmTrans(List<Guid> transactionIds) => await _repository.AnyAsync(trs => transactionIds.Contains(trs.TransactionId.Value) && trs.IsConfirm.HasValue && trs.IsConfirm == EnmTransferIsConfirm.AdvanceTransfer);
        public async Task<List<Transfer>> GetByTransactionId(Guid transId) => await _repository.GetListAsync(tr => tr.TransactionId == transId);
        public async Task<Transfer> UpdateIsComfirmTranfer(TransferFullInputDto tranferUpdate)
        {
            var itemtransfer = await _repository.GetAsync(tr => tr.Id == tranferUpdate.Id);
            itemtransfer.UserConfirm = tranferUpdate.UserConfirm;
            itemtransfer.IsConfirm = EnmTransferIsConfirm.Confirm;
            var entity = await _repository.UpdateAsync(itemtransfer);
            return entity;
        }
        public async Task<Transfer> GetByIds(Guid TransferId) => await _repository.GetAsync(tr => TransferId == tr.Id);
    }
}
