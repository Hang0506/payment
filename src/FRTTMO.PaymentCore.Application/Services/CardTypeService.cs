using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Services
{
    public class CardTypeService : PaymentCoreAppService, ITransientDependency, ICardTypeService
    {
        private readonly ILogger<CardService> _log;
        private readonly ICardTypeRepository _cardTypeRepository;

        public CardTypeService(ICardTypeRepository cardTypeRepository, ILogger<CardService> log)
        {
            _cardTypeRepository = cardTypeRepository;
            _log = log;
        }

        public async Task<List<CardTypeFullOutputDto>> GetList()
        {
            var list = await _cardTypeRepository.GetList();
            return ObjectMapper.Map<List<CardType>, List<CardTypeFullOutputDto>>(list);
        }
    }
}
