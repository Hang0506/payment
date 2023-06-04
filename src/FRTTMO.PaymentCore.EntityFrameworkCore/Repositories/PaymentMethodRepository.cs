
using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class PaymentMethodRepository : IPaymentMethodRepository, ITransientDependency
    {
        private readonly IRepository<PaymentMethod, int> _repository;

        public PaymentMethodRepository(IRepository<PaymentMethod, int> repository)
        {
            _repository = repository;
        }

        public async Task<List<PaymentMethod>> GetListAsync()
        {
            var list = await _repository.ToListAsync();
            return list;
        }
        public async Task<List<PaymentMethod>> GetListByIdsAsync(List<int> listId)
        {
            return await _repository.GetListAsync(method => listId.Contains(method.Id));
        }
    }
}
