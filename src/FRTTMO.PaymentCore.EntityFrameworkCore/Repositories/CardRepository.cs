using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class CardRepository : ITransientDependency, ICardRepository
    {
        readonly IRepository<Card, Guid> _repository;

        public CardRepository(IRepository<Card, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<List<Card>> GetByTransactionId(Guid transId)
        {
            return await _repository.GetListAsync(card => card.TransactionId == transId);
        }
        public async Task<List<Card>> GetByTransactionIds(List<Guid> transIds)
        {
            return await _repository.GetListAsync(card => transIds.Contains(card.TransactionId.Value));
        }

        public async Task<Card> InsertCard(Card card)
        {
            card.CreatedDate = DateTime.Now;
            var output = await _repository.InsertAsync(card, true);
            return output;
        }
    }
}
