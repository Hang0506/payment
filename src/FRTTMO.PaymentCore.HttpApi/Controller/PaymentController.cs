using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Dto.v2;
using FRTTMO.PaymentCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Http.Client;
using Volo.Abp.Validation;

namespace FRTTMO.PaymentCore.Controller
{
    [Route("api/PaymentCore/payment")]
    [ApiVersion("1.0")]
    public class PaymentController : PaymentCoreController<IPaymentService>, IPaymentService
    {
        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> log)
        {
            MainService = paymentService;
            Log = log;
        }

        [HttpGet("by-request-id")]
        public async Task<PaymentInfoOutputDto> GetPaymentInfoByPaymentRequest([FromQuery] PaymentInfoInputDto paymentInfoInput)
        {
            try
            {
                return await _mainService.GetPaymentInfoByPaymentRequest(paymentInfoInput);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetPaymentInfoByPaymentRequest");
                if (ex is AbpDbConcurrencyException)
                {
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                    .WithData("Message", $"Order {paymentInfoInput?.PaymentRequestId}: error - " + ex.Message);
                }
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Payment request {paymentInfoInput.PaymentRequestId}: error - " + ex.Message);
            }
        }

        [NonAction]
        public async Task<PaymentRequestFullOutputDto> CreateRequest([FromBody] PaymentRequestInsertDto paymentRequestInsert)
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
                    .WithData("Message", $"Order {paymentRequestInsert?.OrderCode}: error - " + ex.Message);
                }
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Payment request {paymentRequestInsert.OrderCode}: error - " + ex.Message);
            }
        }
        [HttpPost("cancel-request")]
        public async Task<bool> CancelRequest(PaymentCancelInputDto PaymentRequestDto)
        {
            try
            {
                return await _mainService.CancelRequest(PaymentRequestDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CancelRequest: paymentRequestId: {PaymentRequestDto.PaymentRequestCode}");
                if (ex is AbpDbConcurrencyException)
                {
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                    .WithData("Message", $"PaymentRequestCode {PaymentRequestDto.PaymentRequestCode}: error - " + ex.Message);
                }
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Payment request code {PaymentRequestDto.PaymentRequestCode}: error - " + ex.Message);
            }
        }

        [HttpPost("payment-transaction")]
        public async Task<CreatePaymentTransactionOutputDto> CreatePaymentRequest(CreatePaymentTransactionInputDto inputDto)
        {
            try
            {
                return await _mainService.CreatePaymentRequest(inputDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CreatePaymentRequest");
                if (ex is AbpDbConcurrencyException)
                {
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                    .WithData("Message", $"PaymentRequestCode {inputDto?.PaymentRequestCode}: error - " + ex.Message);
                }
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData("Message", $"Order {inputDto.OrderCode}: error - " + ex.Message);
            }
        }

        [HttpGet("check-exists")]
        public async Task<bool> CheckExists(Guid id)
        {
            try
            {
                return await _mainService.CheckExists(id);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CheckExists {id}");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Id {id}: error - " + ex.Message);
            }
        }

        [HttpGet("check-exists-by-code")]
        public async Task<bool> CheckExistsByCode(string paymentRequestCode)
        {
            try
            {
                return await _mainService.CheckExistsByCode(paymentRequestCode);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CheckExistsByCode: {paymentRequestCode}");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"paymentRequestCode {paymentRequestCode}: error - " + ex.Message);
            }
        }

        [HttpGet()]
        public async Task<PaymentRequestFullOutputDto> GetById(Guid id)
        {
            try
            {
                return await _mainService.GetById(id);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetById: {id}");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData("Message", $"id {id}: error - " + ex.Message);
            }
        }

        [HttpGet("by-payment-request-code")]
        public async Task<PaymentRequestFullOutputDto> GetByPaymentRequestCode(string paymentRequestCode)
        {
            try
            {
                return await _mainService.GetByPaymentRequestCode(paymentRequestCode);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetByPaymentRequestCode: {paymentRequestCode}");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData("Message", $"paymentRequestCode {paymentRequestCode}: error - " + ex.Message);
            }
        }

        [HttpGet("check-payment-request-code-exists")]
        public async Task<bool> CheckPaymentRequestCodeExists(string paymentRequestCode)
        {
            try
            {
                return await _mainService.CheckPaymentRequestCodeExists(paymentRequestCode);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CheckPaymentRequestCodeExists: {paymentRequestCode}");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"paymentRequestCode {paymentRequestCode}: error - " + ex.Message);
            }
        }

        [HttpGet("list-by-order-code")]
        public async Task<List<PaymentRequestFullOutputDto>> GetListByOrderCode(string orderCode)
        {
            try
            {
                return await _mainService.GetListByOrderCode(orderCode);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetListByOrderCode: {orderCode}");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"orderCode {orderCode}: error - " + ex.Message);
            }
        }

        //[HttpPost("upload-file")]
        //public async Task<UploadFileOutputDto> UploadFile(IFormFile file)
        //{
        //    try
        //    {
        //        return await _mainService.UploadFile(file);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.LogError(ex, $"{_CoreName}.UploadFile");
        //        if (ex is AbpDbConcurrencyException)
        //        {
        //            throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
        //            .WithData("Message", ex.Message);
        //        }
        //        if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
        //        throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
        //            .WithData("Message", $"UploadFile: error - " + ex.Message);
        //    }
        //}

        [HttpGet("history")]
        public async Task<PaymentDepositInfoOutputDto> GetPaymentDepositInfoByPaymentRequest([FromQuery] PaymentDepositInfoInputDto inputDto)
        {
            try
            {
                return await _mainService.GetPaymentDepositInfoByPaymentRequest(inputDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CreateWithdrawDepositCash: {inputDto?.PaymentRequestId}");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"PaymentRequestId {inputDto.PaymentRequestId}: error - " + ex.Message);
            }
        }
        [HttpPost("pay/cash")]
        public async Task<CreatePaymentTransactionOutputDto> CreateWithdrawDepositCash(CreateWithdrawDepositInputDto inputDto)
        {
            try
            {
                return await _mainService.CreateWithdrawDepositCash(inputDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CreateWithdrawDepositCash: {inputDto?.OrderCode}");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Order {inputDto.OrderCode}: error - " + ex.Message);
            }
        }
        [HttpPost("pay/transfer")]
        public async Task<CreateWithdrawDepositTransferOutputDto> CreateWithdrawDepositTransfer(CreateWithdrawDepositTransferInputDto inputDto)
        {
            try
            {
                return await _mainService.CreateWithdrawDepositTransfer(inputDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CreateWithdrawDepositTransfer: {inputDto?.OrderCode}");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Order {inputDto.OrderCode}: error - " + ex.Message);
            }
        }

        [HttpGet("presign-upload-s3")]
        public async Task<GetPresignUploadOutputDto> GetPresignUploadS3([FromQuery] GetPresignUploadInputDto input)
        {
            try
            {
                return await _mainService.GetPresignUploadS3(input);
            }
            catch (Exception ex)
            {
                //_log.LogError(string.Format("LoadImage: {0}|", ex));
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData("Message", $"GetPresignUploadS3: error - " + ex.Message);
            }
        }

        [HttpPut("update-company-cod")]
        public async Task<bool> UpdateCompanyCod([FromBody] UpdateCompanyInfoInputDto inItem)
        {
            return await _mainService.UpdateCompanyCod(inItem);
        }

        [HttpGet("history-by-request-id")]
        public async Task<PaymentDepositRequestIdInfoOutputDto> GetPaymentDepositInfoByPaymentRequestId([FromQuery] PaymentDepositInfoInputDto inputDto)
        {
            try
            {
                return await _mainService.GetPaymentDepositInfoByPaymentRequestId(inputDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetPaymentDepositInfoByPaymentRequestId: {inputDto?.PaymentRequestId}");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"PaymentRequestId {inputDto.PaymentRequestId}: error - " + ex.Message);
            }
        }

        [HttpGet("pay/transferByPaymentRequestCode")]
        public async Task<TransferFullOutputDto> GetTranferByPaymentRequestCode(string paymentRequestCode)
        {
            return await _mainService.GetTranferByPaymentRequestCode(paymentRequestCode);
        }

        [HttpPost("pay/search-transfer")]
        public async Task<TransferFullOutputDto> GetTranferByPaymentRequest(SearchTransferCondi item) => await _mainService.GetTranferByPaymentRequest(item);

        [HttpGet("pay/tienvv8-testnuget/{paymentRequestCode}")]
        public async Task<TransferFullOutputDto> Testnuget(string paymentRequestCode) => await _mainService.Testnuget(paymentRequestCode);

        [HttpGet("by-request")]
        public async Task<PaymentAccountingOutputDto> GetPaymentInfoByPaymentRequestId([FromQuery] PaymentInfoInputDto paymentInfoInput)
        {
            try
            {
                return await _mainService.GetPaymentInfoByPaymentRequestId(paymentInfoInput);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetPaymentInfoByPaymentRequest");
                if (ex is AbpDbConcurrencyException)
                {
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                    .WithData("Message", $"Order {paymentInfoInput?.PaymentRequestId}: error - " + ex.Message);
                }
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Payment request {paymentInfoInput.PaymentRequestId}: error - " + ex.Message);
            }
        }
        [HttpGet("pay/transferByPaymentCode")]
        public async Task<TransferFullOutputDto> GetTranferByPaymentCode(string paymentCode)
        {
            return await _mainService.GetTranferByPaymentCode(paymentCode);
        }

        [HttpPost("mapdetail-transactionrecharge")]
        public async Task<List<PaymentInfoDetailDto>> MapDetailTransactionRecharge(List<TransactionFullOutputDto> transRecharge)
        {
            try
            {
                return await _mainService.MapDetailTransactionRecharge(transRecharge);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.MapDetailTransactionRecharge");
                if (ex is AbpDbConcurrencyException)
                {
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                    .WithData("Message", $"TransactionFullOutputDto {transRecharge?.ToString()}: error - " + ex.Message);
                }
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Payment request {transRecharge.ToString()}: error - " + ex.Message);
            }
        }
        [HttpGet("by-payment-code")]
        public async Task<PaymentRequestFullOutputDto> GetByPaymentCode(string paymentCode)
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

        [HttpGet("payment-code")]
        public async Task<PaymentRequestFullOutputDto> GetByPayment(string paymentRequestCode) => await _mainService.GetByPayment(paymentRequestCode);
    }
}
