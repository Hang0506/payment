using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IEWalletDepositRepository
    {
        Task<List<EWalletDeposit>> GetByTransactionId(Guid transId);
        Task<List<EWalletDeposit>> GetByTransactionIds(List<Guid> transIds);
        Task<EWalletDeposit> InsertEWallet(EWalletDeposit eWalletDeposit);
    }
}
