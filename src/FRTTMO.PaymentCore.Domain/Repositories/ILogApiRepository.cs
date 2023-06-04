using FRTTMO.PaymentCore.Entities;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface ILogApiRepository
    {
        Task<LogApi> InsertAsync(LogApi logApi);
    }
}
