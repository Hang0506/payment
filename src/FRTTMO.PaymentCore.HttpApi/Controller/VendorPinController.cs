using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Http.Client;
using Volo.Abp.Validation;

namespace FRTTMO.PaymentCore.Controller
{
    [Route("api/PaymentCore/vendorpin")]
    [ApiVersion("1.0")]
    public class VendorPinController : PaymentCoreController<IVendorPinService>, IVendorPinService
    {
        public VendorPinController(IVendorPinService appService, ILogger<VendorPinController> log)
        {
            MainService = appService;
            Log = log;
        }

        [HttpGet("get-by-vendor")]
        public async Task<VendorPinDto> GetByVendor(int vendorId, string shopCode)
        {
            try
            {
                return await _mainService.GetByVendor(vendorId, shopCode);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetByVendor");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

        [HttpGet]
        public async Task<List<VendorPinFullOutputDto>> GetListAsync()
        {
            try
            {
                return await _mainService.GetListAsync();
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetListAsync");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

        [HttpPost]
        public async Task<VendorPinFullOutputDto> InsertAsync(InsertVenderInputDto input)
        {
            try
            {
                return await _mainService.InsertAsync(input);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.InsertAsync");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<VendorPinFullOutputDto> UpdateAsync(int id, UpdateVenderInputDto input)
        {
            try
            {
                return await _mainService.UpdateAsync(id, input);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.UpdateAsync");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<VendorPinFullOutputDto> DeleteAsync(int id)
        {
            try
            {
                return await _mainService.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.DeleteAsync");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }
    }
}
