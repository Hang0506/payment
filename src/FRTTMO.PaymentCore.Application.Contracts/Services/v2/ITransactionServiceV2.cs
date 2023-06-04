using FRTTMO.PaymentCore.Dto.v2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services.v2
{
    public interface ITransactionServiceV2 : IApplicationService
    {
        Task<List<TransactionFullOutputDtoV2>> GetByPaymentRequestInfo(GetByPaymentRequestInfoInputV2 infoInput);
        Task<TransactionFullOutputDtoV2> InsertTransaction(InsertTransactionInputDtoV2 transactionInputDto);
        Task<DepositCoresOutputDtoV2> InsertTransactionWithDetail(DepositCoresInputDtoV2 rechargeWalletInput, bool UpdateAccountAmount = false);

        Task<decimal?> GetSumAmountOfPaymentRequest(GetByPaymentRequestInfoInputV2 infoInput);
        Task<List<TransactionFullOutputDtoV2>> GetByPaymentRequestCode(string paymentRequestCode, DateTime? paymentRequestDate);
        Task<bool> HasTransferDepositNotIsConfirmTrans(string paymentRequestCode, DateTime? paymentRequestDate);
        Task<decimal?> GetSumAmountOfPaymentRequest(string paymentRequestCode, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate);
        Task<List<TransactionFullOutputDtoV2>> GetByPaymentRequestId(Guid paymentRequestId, DateTime? paymentRequestDate);
    }
}
