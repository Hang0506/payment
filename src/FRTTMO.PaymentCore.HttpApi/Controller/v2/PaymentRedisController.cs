using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Services.v2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;

namespace FRTTMO.PaymentCore.Controller.v2
{

    [Area(PaymentCoreRemoteServiceConsts.ModuleName)]
    [RemoteService(Name = PaymentCoreRemoteServiceConsts.RemoteServiceName)]
    [Route("api/v{version:apiVersion}/PaymentCore/redis")]
    [ApiVersion("2.0")]
    public class PaymentRedisController : PaymentCoreController<IPaymentRedisService>, IPaymentRedisService
    {
        public PaymentRedisController(IPaymentRedisService priceRedisAppService, ILogger<PaymentRedisController> log)
        {
            MainService = priceRedisAppService;
            Log = log;
        }

        [HttpPost]
        [Route("item")]
        public async Task<List<PaymentRedisDetailDto>> GetPriceDocumentsAsync([FromBody] List<string> listPaymentCode)
        {
            return await _mainService.GetPriceDocumentsAsync(listPaymentCode);
        }

        [HttpPost]
        [Route("sync-item")]
        public async Task<bool> SyncPaymentCoreToRedis([FromBody] PaymentRedisDto inputDto)
        {
            return await _mainService.SyncPaymentCoreToRedis(inputDto);
        }
    }
}
