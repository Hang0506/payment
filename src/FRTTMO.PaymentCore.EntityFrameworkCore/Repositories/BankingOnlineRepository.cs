using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;


namespace FRTTMO.PaymentCore.Repositories
{
    public class BankingOnlineRepository : IBankingOnlineRepository, ITransientDependency
    {
        private readonly IRepository<BankingOnline, int> _repository;

        public BankingOnlineRepository(IRepository<BankingOnline, int> repository)
        {
            _repository = repository;
        }

        public async Task<List<BankingOnline>> GetAllAsync()
        {
            return await _repository.GetListAsync(t => t.Status == true);
        }
    }
}
