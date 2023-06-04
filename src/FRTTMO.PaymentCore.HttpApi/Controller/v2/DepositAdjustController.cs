using FRTTMO.PaymentCore.Dto.v2;
using FRTTMO.PaymentCore.Services.v2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Http.Client;
using Volo.Abp.Validation;

namespace FRTTMO.PaymentCore.Controller.v2
{
    [Route("api/v{version:apiVersion}/PaymentCore/depositAdjust")]
    [ApiVersion("2.0")]
    public class DepositAdjustController : PaymentCoreController<IDepositAdjustService>, IDepositAdjustService
    {
        public DepositAdjustController(IDepositAdjustService service, ILogger<DepositController> log)
        {
            MainService = service;
            Log = log;
        }

        [HttpPost("create-request")]
        public async Task<PaymentRequestOutputDto> CreateRequest([FromBody] PaymentRequestInputDto paymentRequestInsert)
        {
            try
            {
                return await _mainService.CreateRequest(paymentRequestInsert);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CreateRequest");
                if (ex is AbpDbConcurrencyException)
                {
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                    .WithData("Message", $"PaymentCode {paymentRequestInsert?.PaymentCode}: error - " + ex.Message);
                }
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Payment request {paymentRequestInsert.PaymentCode}: error - " + ex.Message);
            }
        }

        [HttpPut("cancel-request/{paymentCode}")]
        public async Task<bool> CancelReuqest(string paymentCode) => await _mainService.CancelReuqest(paymentCode);

        [HttpPut("cancel-payment-request/{paymentRequestCode}")]
        public async Task<bool> CancelRequestByPaymentRequestCode(string paymentRequestCode) => await _mainService.CancelRequestByPaymentRequestCode(paymentRequestCode);
    }
}


