using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class CustomerRepository : ICustomerRepository, ITransientDependency
    {
        readonly IRepository<Customer, Guid> _repository;

        public CustomerRepository(
            IRepository<Customer, Guid> repository)
        {
            _repository = repository;
        }
        public async Task<Customer> CreateAsync(Customer customer)
        {
            customer = await _repository.InsertAsync(customer, true);
            return customer;
        }

        public async Task<Customer> GetAsync(Guid id)
        {
            var customer = await _repository.GetAsync(id, true);
            return customer;
        }

        public async Task<Customer> UpdateAsync(Customer customer)
        {
            customer = await _repository.UpdateAsync(customer, true);
            return customer;
        }
        public async Task<List<Customer>> GetListAsync(Expression<Func<Customer, bool>> where)
        {
            var data = await _repository.GetListAsync(where);
            return data;
        }
        public async Task<Customer> VerifyCustomerAsync(Expression<Func<Customer, bool>> where)
        {
            Customer customer = null;
            if (where != null)
            {
                customer = await _repository.FirstOrDefaultAsync(where);
            }
            return customer;
        }
    }
}
