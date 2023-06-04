using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface ICardRepository
    {
        Task<List<Card>> GetByTransactionId(Guid transId);
        Task<List<Card>> GetByTransactionIds(List<Guid> transIds);
        Task<Card> InsertCard(Card card);
    }
}
