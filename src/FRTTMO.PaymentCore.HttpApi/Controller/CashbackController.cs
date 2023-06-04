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
    [Route("api/PaymentCore/cashback")]
    [ApiVersion("1.0")]
    public class CashbackController : PaymentCoreController<ICashbackService>, ICashbackService
    {
        public CashbackController(ICashbackService iCashbackService, ILogger<CashbackController> log)
        {
            MainService = iCashbackService;
            Log = log;
        }

        [HttpPost]
        [Route("create-transaction")]
        public async Task<TransactionFullOutputDto> CreateCashbackTransaction([FromBody] CashbackInputBaseDto cashbackDto)
        {
            try
            {
                return await _mainService.CreateCashbackTransaction(cashbackDto);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"{_CoreName}.Cashback");
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData("Message", $"Cashback order {cashbackDto?.OrderCode}: error - " + ex.Message);
            }
        }
    }
}
