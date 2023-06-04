using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class CODRepository : ITransientDependency, ICODRepository
    {
        private readonly IRepository<COD, Guid> _repository;

        public CODRepository(IRepository<COD, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<COD> Insert(COD cod)
        {
            cod.CreatedDate = DateTime.Now;
            var output = await _repository.InsertAsync(cod, true);
            return output;
        }

        public async Task<List<COD>> GetByTransactionIds(List<Guid> transIds)
        {
            return await _repository.GetListAsync(card => transIds.Contains(card.TransactionId.Value));
        }
        public async Task<COD> GetByTransactionId(Guid transId)
        {
            return await _repository.FirstOrDefaultAsync(cod => cod.TransactionId.Equals(transId));
        }
        public async Task<COD> UpdateAsync(COD cod)
        {
            cod = await _repository.UpdateAsync(cod, true);
            return cod;
        }
        public async Task<List<COD>> GetListByTransactionId(Guid transIds)
        {
            return await _repository.GetListAsync(card => card.TransactionId == transIds);
        }
    }
}
