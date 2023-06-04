using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Repositories;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Services
{
    public class VendorService : PaymentCoreAppService, ITransientDependency, IVendorService
    {
        private readonly IVendorRepository _repository;

        public VendorService(
            IVendorRepository repository
        ) : base()
        {
            _repository = repository;
        }

        public async Task<VendorDto> GetById(int vendorId)
        {
            var rt = await _repository.GetById(vendorId);
            if (rt == null) throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", "Vendor");
            return ObjectMapper.Map<Vendor, VendorDto>(rt);
        }
    }
}
