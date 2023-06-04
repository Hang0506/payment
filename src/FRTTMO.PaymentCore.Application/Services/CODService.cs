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
    public class CODService : PaymentCoreAppService, ITransientDependency, ICODService
    {
        private readonly ILogger<CODService> _log;
        private readonly ICODRepository _repository;

        public CODService(ICODRepository repository, ILogger<CODService> log)
        {
            _repository = repository;
            _log = log;
        }

        public async Task<CODFullOutputDto> Insert(CODInputDto dtoInput)
        {
            try
            {
                var entInput = ObjectMapper.Map<CODInputDto, COD>(dtoInput);
                var ent = await _repository.Insert(entInput);
                return ObjectMapper.Map<COD, CODFullOutputDto>(ent);
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.Insert: {ex}| Request body: {JsonConvert.SerializeObject(dtoInput)} ");
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Insert COD  {dtoInput.waybillnumber}: error - " + ex.Message);
            }
        }

        public async Task<List<CODFullOutputDto>> GetByTransactionIds(List<Guid> transIds)
        {
            try
            {
                var list = await _repository.GetByTransactionIds(transIds);
                return ObjectMapper.Map<List<COD>, List<CODFullOutputDto>>(list);
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.Insert: {ex}| Request body: {JsonConvert.SerializeObject(transIds)} ");
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"GetByTransactionIds error - " + ex.Message);
            }
        }
        public async Task<List<CODFullOutputDto>> GetByTransactionId(Guid transId)
        {
            try
            {
                var list = await _repository.GetListByTransactionId(transId);
                return ObjectMapper.Map<List<COD>, List<CODFullOutputDto>>(list);
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.Insert: {ex}| Request body: {JsonConvert.SerializeObject(transId)} ");
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"GetByTransactionIds error - " + ex.Message);
            }
        }
    }
}
