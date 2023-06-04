using FRTTMO.PaymentCore.Dto.v2;
using FRTTMO.PaymentCore.Services.v2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Http.Client;
using Volo.Abp.Validation;

namespace FRTTMO.PaymentCore.Controller.v2
{
    [Area(PaymentCoreRemoteServiceConsts.ModuleName)]
    [RemoteService(Name = PaymentCoreRemoteServiceConsts.RemoteServiceName)]
    [Route("api/v{version:apiVersion}/PaymentCore/payment")]
    [ApiController]
    [ApiVersion("2.0")]
    public class PaymentControllerV2 : PaymentCoreController<IPaymentServiceV2>, IPaymentServiceV2
    {
        public PaymentControllerV2(IPaymentServiceV2 paymentService, ILogger<PaymentController> log)
        {
            MainService = paymentService;
            Log = log;
        }

        [HttpGet("collected-info")]
        public async Task<PaymentInfoOutputDtoV2> GetPaymentInfoByPaymentRequest([FromQuery] PaymentInfoInputDtoV2 paymentInfoInput)
        {
            try
            {
                return await _mainService.GetPaymentInfoByPaymentRequest(paymentInfoInput);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetPaymentInfoByPaymentRequest");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Payment request {paymentInfoInput.PaymentRequestCode}: error - " + ex.Message);
            }
        }

        [HttpPost("payment-transaction")]
        public async Task<CreatePaymentTransactionOutputDtoV2> CreatePaymentRequest(CreatePaymentTransactionInputDtoV2 inputDto)
        {
            try
            {
                return await _mainService.CreatePaymentRequest(inputDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CreatePaymentRequest");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inputDto?.OrderCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Payment request {inputDto.PaymentRequestCode}: error - " + ex.Message);
            }
        }

        [HttpGet("check-exists")]
        public async Task<bool> CheckExists(string paymentRequestCode)
        {
            try
            {
                return await _mainService.CheckExists(paymentRequestCode);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CheckExists");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Payment request {paymentRequestCode}: error - " + ex.Message);
            }
        }

        [HttpGet("{paymentRequestCode}")]
        public async Task<PaymentRequestFullOutputDtoV2> GetByPaymentRequestCode(string paymentRequestCode)
        {
            try
            {
                return await _mainService.GetByPaymentRequestCode(paymentRequestCode);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetByPaymentRequestCode");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Payment request {paymentRequestCode}: error - " + ex.Message);
            }
        }

        [HttpGet("history")]
        public async Task<PaymentDepositInfoOutputDtoV2> GetPaymentDepositInfoByPaymentRequest([FromQuery] PaymentDepositInfoInputDtoV2 inputDto)
        {
            try
            {
                return await _mainService.GetPaymentDepositInfoByPaymentRequest(inputDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetPaymentDepositInfoByPaymentRequest");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Payment request {inputDto.PaymentRequestCode}: error - " + ex.Message);
            }
        }

        [HttpPost("pay/cash")]
        public async Task<CreatePaymentTransactionOutputDtoV2> CreateWithdrawDepositCash(CreateWithdrawDepositInputDtoV2 inputDto)
        {
            try
            {
                return await _mainService.CreateWithdrawDepositCash(inputDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CreateWithdrawDepositCash");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inputDto?.OrderCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inputDto.OrderCode}: error - " + ex.Message);
            }
        }

        [HttpPost("pay/transfer")]
        public async Task<CreateWithdrawDepositTransferOutputDtoV2> CreateWithdrawDepositTransfer(CreateWithdrawDepositTransferInputDtoV2 inputDto)
        {
            try
            {
                return await _mainService.CreateWithdrawDepositTransfer(inputDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CreateWithdrawDepositTransfer");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inputDto?.OrderCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inputDto.OrderCode}: error - " + ex.Message);
            }
        }
        [HttpPost("create-payment")]
        public async Task<CreatePaymentOutputDto> CreatePayment(CreatePaymentInputDto inputDto)
        {
            try
            {
                return await _mainService.CreatePayment(inputDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CreatePayment");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inputDto}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inputDto}: error - " + ex.Message);
            }
        }
        [HttpGet("by-paymentcode")]
        public async Task<PaymentRequestFullOutputDtoV2> GetByPaymentCode(string paymentCode)
        {
            try
            {
                return await _mainService.GetByPaymentCode(paymentCode);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetBypaymentCode: {paymentCode}");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData("Message", $"paymentCode {paymentCode}: error - " + ex.Message);
            }
        }
        [HttpGet("pay/transferByPaymentCode")]
        public async Task<TransferFullOutputDtoV2> GetTranferByPaymentCode(string paymentCode)
        {
            return await _mainService.GetTranferByPaymentCode(paymentCode);
        }
        [HttpGet("paymentcode-by-datetime")]
        public async Task<OutPutPaymentDtoV2> GetListPaymentCodeByDateTime([FromQuery] InputPaymentDtoV2 input)
        {
            try
            {
                return await _mainService.GetListPaymentCodeByDateTime(input);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetListPaymentCodeByDateTime: {input}");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData("Message", $"paymentCode {input}: error - " + ex.Message);
            }
        }
        [HttpPost("insert-paymentsource")]
        public async Task<bool> InsertPaymentSource(PaymentSourcDto insertInput)
        {
            try
            {
                return await _mainService.InsertPaymentSource(insertInput);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.InsertPaymentSource: {insertInput}");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData("Message", $"paymentCode {insertInput.PaymentCode}: error - " + ex.Message);
            }
        }
        [HttpPut("pay/confirm-transfer")]
        public async Task<TransferUpdateOutDto> UpdateTransfer([FromBody] TransferUpdateInputDto inputDto) => await _mainService.UpdateTransfer(inputDto);
    }
}
