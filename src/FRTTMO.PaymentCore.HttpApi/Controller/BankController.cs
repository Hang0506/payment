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
    [Route("api/PaymentCore/bank")]
    [ApiVersion("1.0")]
    public class BankController : PaymentCoreController<IBankService>, IBankService
    {

        public BankController(IBankService bankService, ILogger<BankController> log)
        {
            MainService = bankService;
            Log = log;
        }
        [HttpGet]
        public async Task<List<BankFullOutputDto>> GetListAsync()
        {
            try
            {
                return await _mainService.GetListAsync();
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.BankGetListAsync");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

        [HttpGet]
        [Route("card")]
        public async Task<List<BankCardFullOutputDto>> GetBankCardListAsync()
        {
            try
            {
                return await _mainService.GetBankCardListAsync();
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.BankGetCardListAsync");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

        [HttpGet]
        [Route("get-account-by-id")]
        public async Task<List<BankAccountFullOutputDto>> GetAccountByBankIdAsync(int BankId)
        {
            try
            {
                return await _mainService.GetAccountByBankIdAsync(BankId);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.GetAccountByBankIdAsync: {BankId}");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }
    }
}
