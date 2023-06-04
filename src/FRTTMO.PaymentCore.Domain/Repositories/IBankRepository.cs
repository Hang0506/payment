using FRTTMO.PaymentCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IBankRepository
    {
        //Task<List<Bank>> GetListAsync(byte? Type);
        Task<List<Bank>> GetListAsync();

    }
}
