using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class VendorPinRepository : ITransientDependency, IVendorPinRepository
    {
        private readonly IRepository<VendorPin, int> _repository;

        public VendorPinRepository(IRepository<VendorPin, int> repository)
        {
            _repository = repository;
        }

        public async Task<VendorPin> GetByVendor(int vendorId, string shopCode) => await _repository.FirstOrDefaultAsync(c => c.VendorId == vendorId && c.ShopCode == shopCode);

        public async Task<List<VendorPin>> GetListAsync() => await _repository.ToListAsync();

        public async Task<VendorPin> InsertAsync(VendorPin input) => await _repository.InsertAsync(input, true);

        public async Task<VendorPin> UpdateAsync(VendorPin input) => await _repository.UpdateAsync(input, true);

        public async Task<VendorPin> DeleteAsync(VendorPin input)
        {
            await _repository.DeleteAsync(input, true);
            return input;
        }

        public async Task<VendorPin> GetById(int id) => await _repository.FindAsync(id);
    }
}
