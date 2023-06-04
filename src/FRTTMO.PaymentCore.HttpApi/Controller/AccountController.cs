using System;
using System.Threading.Tasks;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Http.Client;
using Volo.Abp.Validation;

namespace FRTTMO.PaymentCore.Controller
{
    [Route("api/PaymentCore/Account")]
    [ApiVersion("1.0")]
    public class AccountController : PaymentCoreController<IAccountService>, IAccountService
    {
        public AccountController(IAccountService accountService, ILogger<AccountController> log)
        {
            MainService = accountService;
            Log = log;
        }

        [HttpGet("get-balance/{customerId:Guid}")]
        public async Task<AccountFullOutputDto> GetBalanceByCustomerId(Guid customerId)
        {
            try
            {
                return await _mainService.GetBalanceByCustomerId(customerId);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.BankGetListAsync");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

        [HttpPost]
        public async Task<AccountFullOutputDto> CreateAsync([FromBody] AccountInsertInputDto inputDto)
        {
            try
            {
                return await _mainService.CreateAsync(inputDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.CreateAsync");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }
    }
}
