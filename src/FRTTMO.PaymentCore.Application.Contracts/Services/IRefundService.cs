using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface IRefundService: IPaymentCoreAppServiceBase
    {
        Task<RefundFullOutputDto> CreateTransaction(RefundDto refund);
    }
}
