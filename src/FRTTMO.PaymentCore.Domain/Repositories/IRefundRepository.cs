using FRTTMO.PaymentCore.Entities;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IRefundRepository
    {
        Task<Refund> CreateTransaction(Refund refund);
    }
}
