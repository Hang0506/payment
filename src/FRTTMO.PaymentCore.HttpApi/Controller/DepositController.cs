using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Http.Client;
using Volo.Abp.Validation;

namespace FRTTMO.PaymentCore.Controller
{
    [Route("api/PaymentCore/deposit")]
    [ApiVersion("1.0")]
    public class DepositController : PaymentCoreController<IDepositService>, IDepositService
    {
        public DepositController(IDepositService service, ILogger<DepositController> log)
        {
            MainService = service;
            Log = log;
        }

        [HttpPost("card")]
        public async Task<DepositByCardOutputDto> DepositByCard(DepositByCardInputDto inItem)
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
        public async Task<DepositByCODOutputDto> DepositByCOD(DepositByCODInputDto inItem)
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
        public async Task<DepositByTransferOutputDto> DepositByTransfer(DepositByTransferInputDto inItem)
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
        public async Task<DepositByCashOutputDto> DepositByCash(DepositByCashInputDto inItem)
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
        public async Task<DepositByEWalletOutputDto> DepositByEWallet(DepositByEWalletInputDto inItem)
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
        public async Task<DepositByVoucherOutputDto> DepositByVoucher(DepositByVoucherInputDto inItem)
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
        public async Task<DepositByMultipleVoucherOutputDto> DepositByMultipleVoucher(DepositByVoucherInputDto inItem)
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
        public async Task<DepositByEWalletOutputDto> DepositByEWalletOnline(DepositByEWalletOnlineInputDto inItem)
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
        [HttpPost("debt-sale")]
        public async Task<DepositDebtSaleOutputDto> DepositDebtSale(DepositDebtSaleInputDto inItem) => await _mainService.DepositDebtSale(inItem);

    }
}
