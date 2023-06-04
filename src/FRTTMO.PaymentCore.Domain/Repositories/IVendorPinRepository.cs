using FRTTMO.PaymentCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IVendorPinRepository
    {
        Task<VendorPin> GetByVendor(int vendorId, string shopCode);
        Task<List<VendorPin>> GetListAsync();
        Task<VendorPin> InsertAsync(VendorPin input);
        Task<VendorPin> UpdateAsync(VendorPin input);
        Task<VendorPin> DeleteAsync(VendorPin input);
        Task<VendorPin> GetById(int id);
    }
}
