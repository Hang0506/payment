using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface ITransactionRepository
    {
        Task<List<Transaction>> GetByPaymentRequestInfo(Guid paymentRequestId, EnmTransactionType transactionTypeId, DateTime? paymentRequestDate);
        Task<List<Transaction>> GetByPaymentRequestInfo(Guid paymentRequestId, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate);
        Task<List<Transaction>> GetByPaymentRequestInfo(string paymentRequestCode, EnmTransactionType transactionTypeId, DateTime? paymentRequestDate);
        Task<List<Transaction>> GetByPaymentRequestInfo(string paymentRequestCode, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate);
        Task<Transaction> InsertTransaction(Transaction transaction);
        Task<decimal?> GetSumAmountOfPaymentRequest(Guid paymentRequestId, EnmTransactionType transactionTypeId, DateTime? paymentRequestDate);
        Task<decimal?> GetSumAmountOfPaymentRequest(Guid paymentRequestId, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate);
        Task<decimal?> GetSumAmountOfPaymentRequest(string paymentRequestCode, EnmTransactionType transactionTypeId, DateTime? paymentRequestDate);
        Task<decimal?> GetSumAmountOfPaymentRequest(string paymentRequestCode, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate);
        Task<List<Transaction>> GetByListPaymentRequestIds(List<Guid> listIds);
        Task<List<Transaction>> GetByPaymentRequestId(Guid paymentRequestId, DateTime? paymentRequestDate);
        Task<List<Transaction>> GetByPaymentRequestCode(string paymentRequestCode, DateTime? paymentRequestDate);
        Task<bool> HasTransferDepositNotIsConfirmTrans(Guid paymentRequestId, DateTime? paymentRequestDate);
        Task<bool> HasTransferDepositNotIsConfirmTrans(string paymentRequestCode, DateTime? paymentRequestDate);
        Task<decimal?> GetTotalDeposited(Guid paymentRequestId, DateTime? paymentRequestDate);
        Task<decimal?> GetTotalDeposited(string paymentRequestCode, DateTime? paymentRequestDate);
        Task<bool> HasTransferDepositNotIsConfirmTrans(Guid paymentRequestId, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate);
        Task<bool> HasTransferDepositNotIsConfirmTrans(string paymentRequestCode, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate);
        Task<Transaction> FindTransaction(Guid? transactionId, EnmTransactionType? transactionType);
        Task<Transaction> GetTransactionMethod(Guid? paymentRequestId, EnmPaymentMethod? paymentMethod);
    }
}
