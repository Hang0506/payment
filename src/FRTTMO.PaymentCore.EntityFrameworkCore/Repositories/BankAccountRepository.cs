
using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class BankAccountRepository : IBankAccountRepository, ITransientDependency
    {
        private readonly IRepository<BankAccount, int> _repository;
        private readonly IRepository<Bank, int> _repositoryBank;

        public BankAccountRepository(IRepository<BankAccount, int> repository, IRepository<Bank, int> repositoryBank)
        {
            _repository = repository;
            _repositoryBank = repositoryBank;
        }

        public async Task<List<BankAccountFull>> GetByBankIdAsync(int BankId)
        {
            var queryable = await _repository.GetListAsync(x => x.BankId == BankId);
            var queryableBank = await _repositoryBank.GetListAsync(x => x.Id == BankId);
            var data = queryable.Join(queryableBank,
                act => act.BankId,
                deb => deb.Id,
                (act, deb) => new
                {
                    deb.BankName,
                    deb.BankCode,
                    act.Id,
                    act.BankId,
                    act.AccountNum,
                    act.AccountName,
                    act.BranchName,
                    act.IsDefault,
                    act.CreatedBy,
                    act.CreatedDate,
                    act.ModifiedBy,
                    act.ModifiedDate
                })
                .Where(t => t.BankId == BankId)

                .Select(x => new BankAccountFull
                {
                    Id = x.Id,
                    AccountNum = x.AccountNum,
                    AccountName = x.AccountName,
                    BranchName = x.BranchName,
                    IsDefault = x.IsDefault,
                    BankId = x.BankId,
                    BankCode = x.BankCode,
                    BankName = x.BankName,
                    CreatedDate = x.CreatedDate,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy,
                    ModifiedDate = x.ModifiedDate
                });

            return data.ToList();
        }
    }
}
