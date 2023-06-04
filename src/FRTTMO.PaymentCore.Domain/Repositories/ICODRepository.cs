using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface ICODRepository
    {
        Task<COD> Insert(COD cod);
        Task<List<COD>> GetByTransactionIds(List<Guid> transIds);
        Task<COD> GetByTransactionId(Guid transId);
        Task<COD> UpdateAsync(COD cod);
        Task<List<COD>> GetListByTransactionId(Guid transIds);
    }
}
