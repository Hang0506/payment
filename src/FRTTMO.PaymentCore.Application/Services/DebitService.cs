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
    public class DebitService : PaymentCoreAppService, ITransientDependency, IDebitService
    {
        private readonly ILogger<DebitService> _log;
        private readonly IDebitRepository _repository;

        public DebitService(ILogger<DebitService> log, IDebitRepository debitRepository)
        {
            _repository = debitRepository;
            _log = log;
        }
        public async Task<DebitFullOutputDto> InsertAsync(DebitDto input)
        {
            try
            {
                var entInput = ObjectMapper.Map<DebitDto, CreditSales>(input);
                var ent = await _repository.InsertDebit(entInput);
                return ObjectMapper.Map<CreditSales, DebitFullOutputDto>(ent);
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.Insert: {ex}| Request body: {JsonConvert.SerializeObject(input)} ");
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Insert Debit Transaction {input.TransactionId}: error - " + ex.Message);
            }
        }
        public async Task<List<DebitFullOutputDto>> GetByTransactionIds(List<Guid> transIds)
        {
            var debit = await _repository.GetByTransactionIds(transIds);
            var debitdto = debit.Select(ObjectMapper.Map<CreditSales, DebitFullOutputDto>).ToList();
            return debitdto;
        }
        public async Task<List<DebitFullOutputDto>> GetByTransactionId(Guid transId)
        {
            var debit = await _repository.GetByTransactionId(transId);
            var debitdto = debit.Select(ObjectMapper.Map<CreditSales, DebitFullOutputDto>).ToList();
            return debitdto;
        }
    }
}
