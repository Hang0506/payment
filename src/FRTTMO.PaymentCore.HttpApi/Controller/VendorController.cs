using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Http.Client;
using Volo.Abp.Validation;

namespace FRTTMO.PaymentCore.Controller
{
    [Route("api/PaymentCore/vendor")]
    [ApiVersion("1.0")]
    public class VendorController : PaymentCoreController<IVendorService>, IVendorService
    {
        public VendorController(IVendorService appService, ILogger<VendorController> log)
        {
            MainService = appService;
            Log = log;
        }

        [HttpGet("get-by-id")]
        public async Task<VendorDto> GetById(int vendorId)
        {
            try
            {
                return await _mainService.GetById(vendorId);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetById");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }
    }
}
