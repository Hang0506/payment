using FRTTMO.PaymentCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IVendorRepository
    {
        Task<List<Vendor>> GetListAsync();
        Task<Vendor> GetById(int vendorId);
    }
}
