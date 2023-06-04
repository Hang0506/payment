using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Http.Client;
using Volo.Abp.Validation;

namespace FRTTMO.PaymentCore.Controller
{
    [Route("api/PaymentCore/refund")]
    [ApiVersion("1.0")]
    public class RefundController : PaymentCoreController<IRefundService>, IRefundService
    {
        public RefundController(IRefundService refundService, ILogger<AccountController> log)
        {
            MainService = refundService;
            Log = log;
        }

        [HttpPost("create-transaction")]
        public async Task<RefundFullOutputDto> CreateTransaction([FromQuery] RefundDto refundDto)
        {
            try
            {
                return await _mainService.CreateTransaction(refundDto);
            }
            catch (Exception ex)
            {
                if (ex is BusinessException || ex is AbpValidationException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }
    }
}
