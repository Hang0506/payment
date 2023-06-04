using FRTTMO.PaymentCore.Dto;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace FRTTMO.PaymentCore.Services
{
    public interface ILogApiService : IApplicationService
    {
        Task<bool> WriteLogApi(LogApiDto logApi);        
    }
}
