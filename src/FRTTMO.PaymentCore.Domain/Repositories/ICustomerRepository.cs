using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer> CreateAsync(Customer customer);
        Task<Customer> UpdateAsync(Customer customer);
        Task<Customer> GetAsync(Guid id);
        Task<List<Customer>> GetListAsync(Expression<Func<Customer, bool>> where);
        Task<Customer> VerifyCustomerAsync(Expression<Func<Customer, bool>> where);
    }
}
