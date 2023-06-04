using FRTTMO.PaymentCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IBankAccountRepository
    {
        Task<List<BankAccountFull>> GetByBankIdAsync(int BankId);
    }
}
