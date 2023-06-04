using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System.Threading.Tasks;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services
{
    public interface IVendorDetailService : IPaymentCoreAppServiceBase
    {
        Task<VendorDetailDto> GetByPartnerIdIdAsync(EnmPartnerId? PartnerId);
        Task<VendorDetailDto> SearchAsync(VendorDetailDto item);
    }
}