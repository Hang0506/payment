using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface ICashbackService : IPaymentCoreAppServiceBase
    {
        Task<TransactionFullOutputDto> CreateCashbackTransaction(CashbackInputBaseDto cashbackDto);
    }
}
