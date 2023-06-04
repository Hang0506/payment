using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Controller
{
    [Route("api/PaymentCore/supporting")]
    [ApiVersion("1.0")]
    public class SupportingController : PaymentCoreController<ISupportingService>, ISupportingService
    {
        public SupportingController(ISupportingService  supportingService, ILogger<SupportingController> log)
        {
            MainService = supportingService;
            Log = log;
        }
        [HttpGet("get-orders")]
        public async Task<List<OrderOutputSupporting>> GetOrders(string orderCode)
        {
            return await _mainService.GetOrders(orderCode);
        }

        [HttpPut("update-orders")]
        public  async Task<bool> UpdateOrderOMS(UpdateOMSDto input)
        {
            return await _mainService.UpdateOrderOMS(input);
        }
    }
}
