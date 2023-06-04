using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace FRTTMO.PaymentCore.Services
{
    public interface IGenerateService : IApplicationService
    {
        Task<string> GeneratePaymentRequestCode(string shopCode);
    }
}
