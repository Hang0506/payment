using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface IVendorService : IPaymentCoreAppServiceBase
    {
        Task<VendorDto> GetById(int vendorId);
    }
}
