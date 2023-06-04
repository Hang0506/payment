using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Http.Client;
using Volo.Abp.Validation;

namespace FRTTMO.PaymentCore.Samples
{
    [Route("api/PaymentCore/canceldeposit")]
    [ApiVersion("1.0")]
    public class CancelDepositController : PaymentCoreController<ICancelDepositService>, ICancelDepositService
    {
        public CancelDepositController(ICancelDepositService icancelDepositService, ILogger<CancelDepositController> log)
        {
            MainService = icancelDepositService;
            Log = log;
        }
        [HttpPost("cash")]
        public async Task<CreatePaymentTransactionOutputDto> CreateWithdrawDepositCash([FromBody] CancelDepositDto cancelDepositDto)
        {
            try
            {
                return await _mainService.CreateWithdrawDepositCash(cancelDepositDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CreateWithdrawDepositCash");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

        [HttpGet("get-list-payment-pay")]
        public async Task<List<PaymentPayMethodDto>> Getlistpaymentpaymethod()
        {
            try
            {
                return await _mainService.Getlistpaymentpaymethod();
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.Getlistpaymentpaymethod");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

        [HttpPost("create-request-transfer")]
        public async Task<PaymentRequestFullOutputDto> CreatePaymentRequesttransfer(PaymentRequestTransferDto request)
        {
            try
            {
                return await _mainService.CreatePaymentRequesttransfer(request);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CreatePaymentRequesttransfer");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

        [HttpPost("create-transaction-transfer")]
        public async Task<CreatePaymentTransactionOutputDto> CreateTransactiontransfer(TransactionCancelDepositTransfer request)
        {
            try
            {
                return await _mainService.CreateTransactiontransfer(request);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CreateTransactiontransfer");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }
    }
}
