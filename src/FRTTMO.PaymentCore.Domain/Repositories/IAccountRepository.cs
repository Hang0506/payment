using FRTTMO.PaymentCore.Entities;
using System;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IAccountRepository
    {
        Task<Account> CreateAsync(Account account);
        Task<bool> CheckExists(Guid AccountId);
        Task<Account> Update(Account account);
        Task<Account> GetById(Guid Id);
        Task<Account> GetByCustomerId(Guid CustomerId);
        Task ChangeAmount(Guid AccountId, decimal Amount);
    }
}
