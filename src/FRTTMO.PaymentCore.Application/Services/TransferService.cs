using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Services
{
    public class TransferService : PaymentCoreAppService, ITransientDependency, ITransferService
    {
        private readonly ILogger<TransferService> _log;
        private readonly ITransferRepository _repository;

        public TransferService(ITransferRepository repository, ILogger<TransferService> log)
        {
            _repository = repository;
            _log = log;
        }

        public async Task<TransferFullOutputDto> Insert(TransferInputDto dtoInput)
        {
            try
            {
                var entInput = ObjectMapper.Map<TransferInputDto, Transfer>(dtoInput);
                var ent = await _repository.Insert(entInput);
                return ObjectMapper.Map<Transfer, TransferFullOutputDto>(ent);
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.Insert: {ex}| Request body: {JsonConvert.SerializeObject(dtoInput)} ");
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Insert Transfer  {dtoInput.AccountNum}: error - " + ex.Message);
            }
        }

        public async Task<List<TransferFullOutputDto>> GetByTransactionIds(List<Guid> transIds)
        {
            try
            {
                var list = await _repository.GetByTransactionIds(transIds);
                return ObjectMapper.Map<List<Transfer>, List<TransferFullOutputDto>>(list);
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.Insert: {ex}| Request body: {JsonConvert.SerializeObject(transIds)} ");
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"GetByTransactionIds error - " + ex.Message);
            }
        }
        public async Task<bool> CheckTransferNum(string transferNums) => await _repository.CheckTransferNum(transferNums);

        public async Task<bool> HasTransferDepositNotIsConfirmTrans(List<Guid> transactionIds) => await _repository.HasTransferDepositNotIsConfirmTrans(transactionIds);
        public async Task<List<TransferFullOutputDto>> GetByTransactionId(Guid transId)
        {
            try
            {
                var list = await _repository.GetByTransactionId(transId);
                return ObjectMapper.Map<List<Transfer>, List<TransferFullOutputDto>>(list);
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.Insert: {ex}| Request body: {JsonConvert.SerializeObject(transId)} ");
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"GetByTransactionIds error - " + ex.Message);
            }
        }
        public async Task<TransferFullOutputDto> UpdateAsync(TransferFullInputDto transfer)
        {
            var transferUpdated = await _repository.UpdateIsComfirmTranfer(transfer);
            return ObjectMapper.Map<Transfer, TransferFullOutputDto>(transferUpdated);
        }
        public async Task<TransferFullOutputDto> GetByIds(Guid TransferId)
        {
            var transfer = await _repository.GetByIds(TransferId);
            return ObjectMapper.Map<Transfer, TransferFullOutputDto>(transfer);
        }
    }
}
