using FRTTMO.PaymentCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface ICardTypeRepository
    {
        Task<List<CardType>> GetList();
    }
}
