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
    [Route("api/PaymentCore/payment-method")]
    [ApiVersion("1.0")]
    public class PaymentMethodController : PaymentCoreController<IPaymentMethodService>, IPaymentMethodService
    {
        public PaymentMethodController(IPaymentMethodService paymentMethodService, ILogger<PaymentMethodController> log)
        {
            MainService = paymentMethodService;
            Log = log;
        }

        [HttpGet("list")]
        public async Task<List<PaymentMethodDto>> GetListAsync()
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

        [HttpGet("by-order")]
        public async Task<List<PaymentMethodDto>> GetListPaymentMethodByOrderCode([FromQuery] GetListPaymentMethodByOrderCodeInputDto inputDto)
        {
            try
            {
                return await _mainService.GetListPaymentMethodByOrderCode(inputDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetListPaymentMethodByOrderCode");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"OrderCode {inputDto.OrderCode}: error - " + ex.Message);
            }
        }

    }
}
