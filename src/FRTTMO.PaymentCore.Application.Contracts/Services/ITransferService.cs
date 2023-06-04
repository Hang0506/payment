using FRTTMO.PaymentCore.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface ITransferService
    {
        Task<TransferFullOutputDto> Insert(TransferInputDto dtoInput);
        Task<List<TransferFullOutputDto>> GetByTransactionIds(List<Guid> transIds);
        Task<bool> CheckTransferNum(string transferNums);
        Task<bool> HasTransferDepositNotIsConfirmTrans(List<Guid> transactionIds);
        Task<List<TransferFullOutputDto>> GetByTransactionId(Guid transId);
        Task<TransferFullOutputDto> UpdateAsync(TransferFullInputDto transfer);
        Task<TransferFullOutputDto> GetByIds(Guid TransferId);
    }
}
