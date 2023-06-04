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
    [Route("api/v{version:apiVersion}/paymentcore/deposit")]
    [ApiVersion("2.0")]
    public class DepositController : PaymentCoreController<IDepositServiceV2>, IDepositServiceV2
    {
        public DepositController(IDepositServiceV2 service, ILogger<DepositController> log)
        {
            MainService = service;
            Log = log;
        }

        [HttpPost("card")]
        public async Task<DepositByCardOutputDtoV2> DepositByCard(MaskDepositByCardInputDtoV2 inItem)
        {
            try
            {
                return await _mainService.DepositByCard(inItem);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.DepositByCard OrderCode {inItem?.OrderCode}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inItem?.OrderCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem?.OrderCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }

        [HttpPost("cod")]
        public async Task<DepositByCODOutputDtoV2> DepositByCOD(MaskDepositByCODInputDtoV2 inItem)
        {
            try
            {
                return await _mainService.DepositByCOD(inItem);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.DepositByCOD OrderCode {inItem?.OrderCode}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inItem?.OrderCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem?.OrderCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }

        [HttpPost("transfer")]
        public async Task<DepositByTransferOutputDtoV2> DepositByTransfer(MaskDepositByTransferInputDtoV2 inItem)
        {
            try
            {
                return await _mainService.DepositByTransfer(inItem);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.DepositByTransfer OrderCode {inItem?.OrderCode}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inItem?.OrderCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem?.OrderCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }

        [HttpPost("cash")]
        public async Task<DepositByCashOutputDtoV2> DepositByCash(MaskDepositByCashInputDtoV2 inItem)
        {
            try
            {
                return await _mainService.DepositByCash(inItem);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.DepositByCash OrderCode {inItem?.OrderCode}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inItem?.OrderCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem?.OrderCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }

        [HttpPost("ewallet")]
        public async Task<DepositByEWalletOutputDtoV2> DepositByEWallet(MaskDepositByEWalletInputDtoV2 inItem)
        {
            try
            {
                return await _mainService.DepositByEWallet(inItem);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.DepositByEWallet OrderCode {inItem?.OrderCode}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inItem?.OrderCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem?.OrderCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }

        [HttpPost("voucher")]
        public async Task<DepositByVoucherOutputDtoV2> DepositByVoucher(MaskDepositByVoucherInputDtoV2 inItem)
        {
            try
            {
                return await _mainService.DepositByVoucher(inItem);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.DepositByVoucher OrderCode {inItem?.OrderCode}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inItem?.OrderCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem?.OrderCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }

        [HttpPost("multiple-voucher")]
        public async Task<DepositByMultipleVoucherOutputDtoV2> DepositByMultipleVoucher(MaskDepositByVoucherInputDtoV2 inItem)
        {
            try
            {
                return await _mainService.DepositByMultipleVoucher(inItem);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.DepositByMultipleVoucher{inItem?.OrderCode}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inItem?.OrderCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem?.OrderCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }

        [HttpPost("online")]
        public async Task<DepositByEWalletOutputDtoV2> DepositByEWalletOnline(MaskDepositByEWalletOnlineInputDtoV2 inItem)
        {
            try
            {
                return await _mainService.DepositByEWalletOnline(inItem);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.DepositByEWalletOnline OrderCode {inItem?.OrderCode}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inItem?.OrderCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem?.OrderCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }
        [HttpPost("debt-sale-all")]

        public async Task<DebtSaleFullOutputV2Dto> DepositDebtSaleAll(DepositAllInputDto inItem)
        {
            try
            {
                return await _mainService.DepositDebtSaleAll(inItem);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.DepositDebtSaleAll PaymentCode {inItem?.PaymentCode}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inItem?.PaymentCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.PaymentCode, inItem?.PaymentCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }
        [HttpPost("finishPayment-all")]

        public async Task<VerifyTDTOutputDto> FinishTSTD(VerifyTDTSInputDto inItem)
        {
            try
            {
                return await _mainService.FinishTSTD(inItem);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.finishPayment PaymentCode {inItem?.PaymentCode}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"PaymentCode {inItem?.PaymentCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.PaymentCode, inItem?.PaymentCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }
        [HttpPost("createRequest-all")]

        public async Task<CreateRequestDepositAllOutputDto> CreateRequestDepositAll(CreateRequestDepositAllInputDto inItem)
        {
            try
            {
                return await _mainService.CreateRequestDepositAll(inItem);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CreateRequestDepositAll PaymentCode {inItem?.PaymentCode}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Order {inItem?.PaymentCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.PaymentCode, inItem?.PaymentCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }
        [HttpPost("Cod-all")]

        public async Task<CodFullOutputDtoV2Dto> DepositCodsAll([FromBody] CodRequestDto requestDto)
        {
            try
            {
                return await _mainService.DepositCodsAll(requestDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.DepositCodsAll PaymentRequestCode {requestDto?.PaymentRequestCode}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"PaymentRequestCode {requestDto?.PaymentRequestCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.PaymentRequestCode, requestDto?.PaymentRequestCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }
        [HttpPost("deposit-all-cash")]
        public async Task<CashFullOutputV2Dto> DepositCashAll([FromBody] DepositAllInputDto inItem)
        {
            try
            {
                return await _mainService.DepositCashAll(inItem);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.DepositByEWalletOnline PaymentCode {inItem?.PaymentCode}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData(PaymentCoreErrorMessageKey.MessageDetail, $"PaymentCode {inItem?.PaymentCode}: error - " + ex.Message);
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.PaymentCode, inItem?.PaymentCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }

        [HttpPost("voucher-all")]
        public async Task<VoucherFullOutputV2Dto> DepositVoucherAll([FromBody] VoucherRequestDto vouchers) => await _mainService.DepositVoucherAll(vouchers);

        [HttpPost("ewallet-all")]
        public async Task<EWalletDepositFullOutputV2Dto> DepositEWalletAll([FromBody] eWalletRequestDto eWallet) => await _mainService.DepositEWalletAll(eWallet);

        [HttpPost("online-all")]
        public Task<EWalletDepositFullOutputV2Dto> DepositEWalletOnlineAll([FromBody] eWalletRequestDto eWallet) => _mainService.DepositEWalletOnlineAll(eWallet);

        [HttpPost("transfer-all")]
        public async Task<TransferFullOutputV2Dto> DepositTransferAll([FromBody] TransferRequestDto tranfer) => await _mainService.DepositTransferAll(tranfer);

        [HttpPost("card-all")]
        public async Task<CardFullOutputV2Dto> DepositCardsAll([FromBody] CardRequestDto cards) => await _mainService.DepositCardsAll(cards);
        [HttpPost("deposit-all")]
        public async Task<DepositAllOutDto> DepositSyntheticAll([FromBody] DepositAllInputDto inItem) => await _mainService.DepositSyntheticAll(inItem);
        [HttpPost("sync-es-deposit-all")]
        public async Task<bool> MapDataSyncES(MapESDepositDto response) => await _mainService.MapDataSyncES(response);
        [HttpGet("MigrationPaymentRequestCode")]
        public async Task<MigrationPaymentrequestCodeOutnputDto> MigrationPaymentRequestCode([FromQuery] MigrationPaymentrequestCodeInnputDto inputMigration)
        {
            try
            {
                return await _mainService.MigrationPaymentRequestCode(inputMigration);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.MigrationPaymentRequestCode");
                if (ex is AbpDbConcurrencyException)
                {
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                    .WithData("Message", $"MigrationPaymentRequestCode {inputMigration?.ToString()}: error - " + ex.Message);
                }
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Payment request {inputMigration.ToString()}: error - " + ex.Message);
            }
        }
        [HttpPost("verify-vc")]
        public async Task<VoucherFullOutputV2Dto> VerifyVoucher(VoucherRequestDto request) => await _mainService.VerifyVoucher(request);
    }
}


