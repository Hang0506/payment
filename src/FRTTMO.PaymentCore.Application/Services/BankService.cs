using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Services
{
    public class BankService : PaymentCoreAppService, ITransientDependency, IBankService
    {
        private readonly ILogger<BankService> _log;
        private readonly IBankRepository _bankRepository;
        private readonly IBankCardRepository _bankCardRepository;
        private readonly IBankAccountRepository _bankAccountRepository;

        public BankService(
            IBankCardRepository bankCardRepository,
            IBankRepository bankRepository,
            IBankAccountRepository bankAccountRepository,
            ILogger<BankService> log
        ) : base()
        {
            _bankCardRepository = bankCardRepository;
            _bankRepository = bankRepository;
            _bankAccountRepository = bankAccountRepository;
            _log = log;
        }
        public async Task<List<BankCardFullOutputDto>> GetBankCardListAsync()
        {
            var list = await _bankCardRepository.GetBankCardListAsync();
            return ObjectMapper.Map<List<BankCard>, List<BankCardFullOutputDto>>(list);
        }

        public async Task<List<BankFullOutputDto>> GetListAsync()
        {
            var list = await _bankRepository.GetListAsync();
            return ObjectMapper.Map<List<Bank>, List<BankFullOutputDto>>(list);
        }

        public async Task<List<BankAccountFullOutputDto>> GetAccountByBankIdAsync(int BankId)
        {
            var list = await _bankAccountRepository.GetByBankIdAsync(BankId);
            return ObjectMapper.Map<List<BankAccountFull>, List<BankAccountFullOutputDto>>(list);
        }
    }
}
