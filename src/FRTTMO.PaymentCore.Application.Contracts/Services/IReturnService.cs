using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface IReturnService : IPaymentCoreAppServiceBase
    {
        Task<CreatePaymentTransactionOutputDto> ReturnTransactionCash(ReturnCashDto returnRequest);
        Task<CreatePaymentTransactionOutputDto> ReturnTransactionTransfer(ReturnTransferDto returnRequest);
    }
}
