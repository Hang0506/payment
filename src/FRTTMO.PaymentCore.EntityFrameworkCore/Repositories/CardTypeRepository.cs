using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class CardTypeRepository : ITransientDependency, ICardTypeRepository
    {
        private readonly IRepository<CardType, int> _repository;

        public CardTypeRepository(IRepository<CardType, int> repository)
        {
            _repository = repository;
        }

        public async Task<List<CardType>> GetList()
        {
            return await _repository.ToListAsync();
        }
    }
}
