using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface ICancelDepositService : IPaymentCoreAppServiceBase
    {
        Task<CreatePaymentTransactionOutputDto> CreateWithdrawDepositCash(CancelDepositDto cancelDepositDto);
        Task<List<PaymentPayMethodDto>> Getlistpaymentpaymethod();
        Task<PaymentRequestFullOutputDto> CreatePaymentRequesttransfer(PaymentRequestTransferDto request);
        Task<CreatePaymentTransactionOutputDto> CreateTransactiontransfer(TransactionCancelDepositTransfer request);
    }
}
