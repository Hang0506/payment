using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Controller
{
    [Route("api/PaymentCore/bankingonline")]
    [ApiVersion("1.0")]
    public class BankingOnlineController : PaymentCoreController<IBankingOnlineService>, IBankingOnlineService
    {
        private readonly IBankingOnlineService _bankingOnlineService;

        public BankingOnlineController(IBankingOnlineService bankingOnlineService)
        {
            _bankingOnlineService = bankingOnlineService;
        }
        [HttpGet]
        public async Task<List<BankingOnlineOutPutDto>> GetlistAsync()
        {
            return await _bankingOnlineService.GetlistAsync();
        }
    }
}
