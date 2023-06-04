using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class VendorRepository : ITransientDependency, IVendorRepository
    {
        private readonly IRepository<Vendor, int> _repository;

        public VendorRepository(IRepository<Vendor, int> repository)
        {
            _repository = repository;
        }

        public async Task<List<Vendor>> GetListAsync() => await _repository.ToListAsync();
        public async Task<Vendor> GetById(int vendorId) => await _repository.FirstOrDefaultAsync(c => c.Id == vendorId);
    }
}
