using FRTTMO.PaymentCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IPaymentMethodRepository
    {
        Task<List<PaymentMethod>> GetListAsync();
        Task<List<PaymentMethod>> GetListByIdsAsync(List<int> listId);
    }
}
