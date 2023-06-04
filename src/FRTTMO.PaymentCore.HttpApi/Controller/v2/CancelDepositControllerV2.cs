using FRTTMO.PaymentCore.Dto.v2;
using FRTTMO.PaymentCore.Services.v2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Controller.v2
{
    [Route("api/v{version:apiVersion}/paymentcore/cancel-deposit")]
    [ApiVersion("2.0")]
    public class CancelDepositControllerV2 : PaymentCoreController<ICancelDepositServiceV2>, ICancelDepositServiceV2
    {
        public CancelDepositControllerV2(ICancelDepositServiceV2 service, ILogger<DepositController> log)
        {
            MainService = service;
            Log = log;
        }

        [HttpPost("create-transaction-transfer")]
        public async Task<CreatePaymentTransactionOutputDtoV2> CreateTransactiontransfer(TransactionCancelDepositTransferV2 request) => await _mainService.CreateTransactiontransfer(request);
    }
}
