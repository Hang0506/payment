using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IDebitRepository
    {
        Task<CreditSales> InsertDebit(CreditSales debit);
        Task<List<CreditSales>> GetByTransactionIds(List<Guid> transIds);
        Task<List<CreditSales>> GetByTransactionId(Guid transId);
    }
}
