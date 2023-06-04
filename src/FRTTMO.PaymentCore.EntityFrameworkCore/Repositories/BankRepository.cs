
using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class BankRepository : IBankRepository, ITransientDependency
    {
        private readonly IRepository<Bank, int> _repository;

        public BankRepository(IRepository<Bank, int> repository)
        {
            _repository = repository;
        }
        public async Task<List<Bank>> GetListAsync()
        {
            return await _repository.GetListAsync(t => t.IsDeleted == false);
        }

    }
}
