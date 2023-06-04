using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Services
{
    public class BankingOnlineService : PaymentCoreAppService, ITransientDependency, IBankingOnlineService
    {
        private readonly ILogger<BankingOnlineService> _log;
        private readonly IBankingOnlineRepository _bankingOnlineRepository;

        public BankingOnlineService(ILogger<BankingOnlineService> log, IBankingOnlineRepository bankingOnlineRepository)
        {
            _log = log;
            _bankingOnlineRepository = bankingOnlineRepository;
        }

        public async Task<List<BankingOnlineOutPutDto>> GetlistAsync()
        {
            var bankingOnlineList = await _bankingOnlineRepository.GetAllAsync();

            return ObjectMapper.Map<List<BankingOnline>, List<BankingOnlineOutPutDto>>(bankingOnlineList);
        }
    }
}
