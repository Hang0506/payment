using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface ITransferRepository
    {
        Task<Transfer> Insert(Transfer entity);
        Task<List<Transfer>> GetByTransactionIds(List<Guid> transIds);
        Task<bool> CheckTransferNum(string transferNums);
        Task<bool> HasTransferDepositNotIsConfirmTrans(List<Guid> transactionIds);
        Task<List<Transfer>> GetByTransactionId(Guid transId);
        Task<Transfer> UpdateIsComfirmTranfer(TransferFullInputDto tranferUpdate);
        Task<Transfer> GetByIds(Guid TransferId);
    }
}
