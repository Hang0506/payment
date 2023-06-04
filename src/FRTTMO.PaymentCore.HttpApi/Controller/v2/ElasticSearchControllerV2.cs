using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Dto.v2;
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
    [Route("api/v{version:apiVersion}/PaymentCore/es")]
    [ApiVersion("2.0")]
    public class ElasticSearchController : PaymentCoreController<IElasticSearchServiceV2>, IElasticSearchServiceV2
    {
        public ElasticSearchController(IElasticSearchServiceV2 service, ILogger<ElasticSearchController> log)
        {
            MainService = service;
            Log = log;
        }

        [HttpPost("sycn-depositall")]
        public async Task<bool> SyncESDepositAllAsync(string paymentCode, DepositAllDto data, string insertFrom = "") => await _mainService.SyncESDepositAllAsync(paymentCode, data, "");

        [HttpGet]
        public async Task<DepositAllDto> SearchESHistory(string paymentCode) => await _mainService.SearchESHistory(paymentCode);
        [HttpGet("historydatabase")]
        public async Task<DepositAllDto> GetHistoryAll(string paymentCode) => await _mainService.GetHistoryAll(paymentCode);
        [HttpGet("payment-history")]
        public async Task<DepositAllDto> GetdocESByPaymentCode(string paymentCode) => await _mainService.GetdocESByPaymentCode(paymentCode);


        [HttpPost]
        [Route("infor-paymentMethod")]
        public async Task<List<ResponseMethodEs>> SearchESByOrdercode(RequestListPaymentMethodEs request)
        {
            return await _mainService.SearchESByOrdercode(request);
        }
    }
}
