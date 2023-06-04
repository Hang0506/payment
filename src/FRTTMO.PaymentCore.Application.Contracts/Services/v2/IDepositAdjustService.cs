using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto.v2;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services.v2
{
    public interface IDepositAdjustService : IPaymentCoreAppServiceBase
    {
        Task<PaymentRequestOutputDto> CreateRequest(PaymentRequestInputDto paymentRequestInsert);
        Task<bool> CancelReuqest(string paymentCode);
        Task<bool> CancelRequestByPaymentRequestCode(string paymentRequestCode);
    }
}
