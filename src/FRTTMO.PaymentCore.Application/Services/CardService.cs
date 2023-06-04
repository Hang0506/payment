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
    public class CardService : PaymentCoreAppService, ITransientDependency, ICardService
    {
        private readonly ILogger<CardService> _log;
        private readonly ICardRepository _cardRepository;

        public CardService(ICardRepository cardRepository, ILogger<CardService> log)
        {
            _cardRepository = cardRepository;
            _log = log;
        }

        public async Task<List<CardFullOutputDto>> GetByTransactionId(Guid transId)
        {
            var cards = await _cardRepository.GetByTransactionId(transId);
            var cardDtos = cards.Select(ObjectMapper.Map<Card, CardFullOutputDto>).ToList();
            return cardDtos;
        }

        public async Task<List<CardFullOutputDto>> GetByTransactionIds(List<Guid> transIds)
        {
            var cards = await _cardRepository.GetByTransactionIds(transIds);
            var cardDtos = cards.Select(ObjectMapper.Map<Card, CardFullOutputDto>).ToList();
            return cardDtos;
        }

        public async Task<CardFullOutputDto> InsertCard(CardInputDto cardDto)
        {
            try
            {
                var cardInput = ObjectMapper.Map<CardInputDto, Card>(cardDto);
                var card = await _cardRepository.InsertCard(cardInput);
                var cardOutput = ObjectMapper.Map<Card, CardFullOutputDto>(card);
                return cardOutput;
            }
            catch (Exception ex)
            {
                _log.LogError(string.Format("InsertCard: {0}| Request body: {1} ", ex, JsonConvert.SerializeObject(cardDto)));
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Insert Card {cardDto.CardNumber}: error - " + ex.Message);
            }
        }
    }
}
