using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface IVendorPinService : IPaymentCoreAppServiceBase
    {
        Task<VendorPinDto> GetByVendor(int vendorId, string shopCode);
        Task<List<VendorPinFullOutputDto>> GetListAsync();
        Task<VendorPinFullOutputDto> InsertAsync(InsertVenderInputDto input);
        Task<VendorPinFullOutputDto> UpdateAsync(int id, UpdateVenderInputDto input);
        Task<VendorPinFullOutputDto> DeleteAsync(int id);
    }
}
