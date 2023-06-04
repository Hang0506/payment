using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Http.Client;
using Volo.Abp.Validation;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Controller
{
    [Route("api/PaymentCore/vendor-detail")]
    [ApiVersion("1.0")]
    public class VendorDetailController : PaymentCoreController<IVendorDetailService>, IVendorDetailService
    {
        public VendorDetailController(IVendorDetailService appService, ILogger<VendorDetailController> log)
        {
            MainService = appService;
            Log = log;
        }

        [HttpGet("get-by-PartnerId")]
        public async Task<VendorDetailDto> GetByPartnerIdIdAsync(EnmPartnerId? PartnerId)
        {
            try
            {
                return await _mainService.GetByPartnerIdIdAsync(PartnerId);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetByPartnerId");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

        [HttpPost("search")]
        public async Task<VendorDetailDto> SearchAsync(VendorDetailDto item)
        {
            try
            {
                return await _mainService.SearchAsync(item);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.SearchAsync");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }
    }
}
