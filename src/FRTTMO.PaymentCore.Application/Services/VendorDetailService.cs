using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Repositories;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services
{
    public class VendorDetailService : PaymentCoreAppService, ITransientDependency, IVendorDetailService
    {
        private readonly IVendorDetailRepository _repository;

        public VendorDetailService(
            IVendorDetailRepository repository
        ) : base()
        {
            _repository = repository;
        }

        public async Task<VendorDetailDto> GetByPartnerIdIdAsync(EnmPartnerId? PartnerId)
        {
            var result = await _repository.GetByPartnerIdIdAsync(PartnerId);
            return ObjectMapper.Map<VendorDetail, VendorDetailDto>(result);
        }

        public async Task<VendorDetailDto> SearchAsync(VendorDetailDto item) => await GetByPartnerIdIdAsync(item.PartnerId);
    }
}