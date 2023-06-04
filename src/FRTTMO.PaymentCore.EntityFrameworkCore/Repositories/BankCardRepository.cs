
using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class BankCardRepository : IBankCardRepository, ITransientDependency
    {
        private readonly IRepository<BankCard, int> _repository;

        public BankCardRepository(IRepository<BankCard, int> repository)
        {
            _repository = repository;
        }
        public async Task<List<BankCard>> GetBankCardListAsync()
        {
            return await _repository.GetListAsync(t => t.IsDeleted == false);
        }
    }
}
