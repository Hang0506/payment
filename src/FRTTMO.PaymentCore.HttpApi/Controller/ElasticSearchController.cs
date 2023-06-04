using FRTTMO.PaymentCore.Dto.TransactionDetailDto;
using FRTTMO.PaymentCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Controller
{
    [Route("api/PaymentCore/es")]
    public class ElasticSearchController : PaymentCoreController<IElasticSearchService>, IElasticSearchService
    {
        public ElasticSearchController(IElasticSearchService elasticsearchService, ILogger<DepositController> log)
        {
            Log = log;
            MainService = elasticsearchService;
        }
        [HttpGet]
        [Route("get-by-paymentcode")]
        public async Task<List<TransactionDetailTransferOutputDto>> SearchESByPaymentCode([FromQuery] SearchESByPaymentCodeRequestDto request)
        {
            return await _mainService.SearchESByPaymentCode(request);
        }
        [HttpGet]
        [Route("tranferinfo-by-paymentcode")]
        public async Task<TransferInfoDetailOutputDto> SearchESTransferInfoByPaymentCode([FromQuery] TransferInfoInputDto request)
        {
            return await _mainService.SearchESTransferInfoByPaymentCode(request);
        }
        [HttpPatch]
        [Route("sync-common")]
        public async Task<bool> SyncDataESTransfer(string paymentCode, TransactionDetailTransferOutputDto data)
        {
            return await _mainService.SyncDataESTransfer(paymentCode, data);
        }
        [HttpGet]
        [Route("getdoc-paymentcode")]
        public async Task<TransactionDetailTransferOutputDto> GetDocTransferES([FromQuery] string paymentCode)
        {
            return await _mainService.GetDocTransferES(paymentCode);
        }
        [HttpPost]
        [Route("delete-paymentcode")]
        public async Task<bool> DeleteDataESTransfer(string paymentCode)
        {
            return await _mainService.DeleteDataESTransfer(paymentCode);
        }
    }
}
