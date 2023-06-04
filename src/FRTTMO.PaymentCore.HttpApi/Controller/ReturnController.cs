using System;
using System.Threading.Tasks;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Http.Client;
using Volo.Abp.Validation;

namespace FRTTMO.PaymentCore.Samples
{
    [Route("api/PaymentCore/return")]
    [ApiVersion("1.0")]
    public class ReturnController : PaymentCoreController<IReturnService>, IReturnService
    {
        public ReturnController(IReturnService ireturnService, ILogger<ReturnController> log)
        {
            MainService = ireturnService;
            Log = log;
        }
        [HttpPost]
        [Route("cash/create-transaction")]
        public async Task<CreatePaymentTransactionOutputDto> ReturnTransactionCash([FromBody] ReturnCashDto returnRequest)
        {
            try
            {
                return await _mainService.ReturnTransactionCash(returnRequest);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.ReturnTransactionCash");
                if (ex is AbpDbConcurrencyException)
                {
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                    .WithData("Message", $"PaymentRequestCode {returnRequest?.OrderCode}: error - " + ex.Message);
                }
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

        [HttpPost]
        [Route("transfer/create-transaction")]
        public async Task<CreatePaymentTransactionOutputDto> ReturnTransactionTransfer([FromBody] ReturnTransferDto returnRequest)
        {
            try
            {
                return await _mainService.ReturnTransactionTransfer(returnRequest);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.ReturnTransactionTransfer");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }
    }
}
