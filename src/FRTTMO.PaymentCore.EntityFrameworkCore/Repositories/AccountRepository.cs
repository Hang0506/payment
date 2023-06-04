using FRTTMO.PaymentCore.Entities;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class AccountRepository : IAccountRepository, ITransientDependency
    {
        private readonly IRepository<Account, Guid> _repository;

        public AccountRepository(
            IRepository<Account, Guid> repository)
        {
            _repository = repository;
        }
        public async Task<Account> CreateAsync(Account account)
        {
            account = await _repository.InsertAsync(account, true);
            return account;
        }
        public async Task<bool> CheckExists(Guid AccountId) => await _repository.AnyAsync(c => c.Id == AccountId);

        public async Task<Account> Update(Account account)
        {
            account.ModifiedDate = DateTime.Now;
            account = await _repository.UpdateAsync(account, true);
            return account;
        }
        public async Task<Account> GetById(Guid Id) => await _repository.FindAsync(c => c.Id == Id);
        public async Task<Account> GetByCustomerId(Guid CustomerId) => await _repository.FindAsync(c => c.CustomerId == CustomerId);

        /// <param name="Amount"> >0 => cộng. <0 => trừ</param>
        public async Task ChangeAmount(Guid AccountId, decimal Amount)
        {
            var acc = await GetById(AccountId);
            acc.CurrentBalance = (acc.CurrentBalance ?? 0m) + Amount;
            await Update(acc);
        }
    }
}
