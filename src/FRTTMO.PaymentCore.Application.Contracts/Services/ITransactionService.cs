using FRTTMO.PaymentCore.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services
{
    public interface ITransactionService
    {
        Task<List<TransactionFullOutputDto>> GetByPaymentRequestInfo(GetByPaymentRequestInfoInput infoInput);
        Task<TransactionFullOutputDto> InsertTransaction(InsertTransactionInputDto transactionInputDto);
        Task<DepositWalletOutputDto> InsertTransactionWithDetail(DepositCoresInputDto rechargeWalletInput, bool UpdateAccountAmount = false);

        Task<List<TransactionFullOutputDto>> GetByListPaymentRequestIds(List<Guid> listIds);
        Task<List<TransactionFullOutputDto>> GetByPaymentRequestId(Guid paymentRequestId, DateTime? paymentRequestDate);
        Task<bool> HasTransferDepositNotIsConfirmTrans(Guid paymentRequestId, DateTime? paymentRequestDate);
        Task<bool> HasTransferDepositNotIsConfirmTrans(Guid paymentRequestId, DateTime? paymentRequestDate, List<EnmTransactionType> transactionTypes);
        Task<decimal?> GetTotalDeposited(Guid paymentRequestId, DateTime? paymentRequestDate);
        Task<decimal?> GetSumAmountOfPaymentRequest(Guid paymentRequestId, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate);
    }
}
