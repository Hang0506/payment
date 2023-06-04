using FRTTMO.PaymentCore.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface IEWalletDepositService
    {
        Task<List<EWalletDepositFullOutputDto>> GetByTransactionId(Guid transId);
        Task<List<EWalletDepositFullOutputDto>> GetByTransactionIds(List<Guid> transIds);
        Task<EWalletDepositFullOutputDto> InsertEWalletDeposit(EWalletDepositInputDto eWalletDeposit);
    }
}
