using FRTTMO.PaymentCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface ICancelDepositRepository
    {
        Task<List<PaymentMethod>> GetlistpaymentpaymethodAsync();
    }
}
