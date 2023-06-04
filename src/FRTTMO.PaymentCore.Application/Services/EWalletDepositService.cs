using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Services
{
    public class EWalletDepositService : PaymentCoreAppService, ITransientDependency, IEWalletDepositService
    {
        private readonly ILogger<EWalletDepositService> _log;
        private readonly IEWalletDepositRepository _eWalletDepositRepository;

        public EWalletDepositService(IEWalletDepositRepository voucherRepository, ILogger<EWalletDepositService> log)
        {
            _eWalletDepositRepository = voucherRepository;
            _log = log;
        }

        public async Task<List<EWalletDepositFullOutputDto>> GetByTransactionId(Guid transId)
        {
            var eWallets = await _eWalletDepositRepository.GetByTransactionId(transId);
            var eWalletDtos = eWallets.Select(ObjectMapper.Map<EWalletDeposit, EWalletDepositFullOutputDto>).ToList();
            return eWalletDtos;
        }

        public async Task<List<EWalletDepositFullOutputDto>> GetByTransactionIds(List<Guid> transIds)
        {
            var eWallets = await _eWalletDepositRepository.GetByTransactionIds(transIds);
            var eWalletDtos = eWallets.Select(ObjectMapper.Map<EWalletDeposit, EWalletDepositFullOutputDto>).ToList();
            return eWalletDtos;
        }

        public async Task<EWalletDepositFullOutputDto> InsertEWalletDeposit(EWalletDepositInputDto eWalletDeposit)
        {
            try
            {
                var input = ObjectMapper.Map<EWalletDepositInputDto, EWalletDeposit>(eWalletDeposit);
                var data = await _eWalletDepositRepository.InsertEWallet(input);
                var output = ObjectMapper.Map<EWalletDeposit, EWalletDepositFullOutputDto>(data);
                return output;
            }
            catch (Exception ex)
            {
                _log.LogError(string.Format("InsertEWalletDeposit: {0}| Request body: {1} ", ex, JsonConvert.SerializeObject(eWalletDeposit)));
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Insert EWallet Deposit {eWalletDeposit.TypeWalletId}: error - " + ex.Message);
            }
        }
    }
}
