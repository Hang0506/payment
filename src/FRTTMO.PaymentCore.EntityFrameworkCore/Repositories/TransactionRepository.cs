using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Repositories
{
    public class TransactionRepository : ITransientDependency, ITransactionRepository
    {
        private readonly IRepository<Transaction, Guid> _repository;
        private readonly ITransferRepository _transferRepository;

        public TransactionRepository(IRepository<Transaction, Guid> repository, ITransferRepository transferRepository)
        {
            _repository = repository;
            _transferRepository = transferRepository;
        }

        public async Task<List<Transaction>> GetByPaymentRequestInfo(Guid paymentRequestId, EnmTransactionType transactionTypeId, DateTime? paymentRequestDate)
        {
            if (!paymentRequestDate.HasValue)
            {
                return await _repository
                    .GetListAsync(
                        tran => tran.PaymentRequestId == paymentRequestId
                        && tran.TransactionTypeId == transactionTypeId
                    );
            }
            return await _repository
                .GetListAsync(
                    tran =>tran.PaymentRequestDate == paymentRequestDate &&
                    tran.PaymentRequestId == paymentRequestId
                    && tran.TransactionTypeId == transactionTypeId
                );
        }
        public async Task<List<Transaction>> GetByPaymentRequestInfo(Guid paymentRequestId, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate)
        {
            if (!paymentRequestDate.HasValue)
            {
                return await _repository
                    .GetListAsync(
                        tran => tran.PaymentRequestId == paymentRequestId
                        && transactionTypes.Contains(tran.TransactionTypeId.Value)
                    );
            }
            return await _repository
                .GetListAsync(
                    tran => tran.PaymentRequestId == paymentRequestId
                    && transactionTypes.Contains(tran.TransactionTypeId.Value)
                    && tran.PaymentRequestDate == paymentRequestDate
                );
        }
        public async Task<List<Transaction>> GetByPaymentRequestInfo(Guid paymentRequestId, List<EnmTransactionType> transactionTypes)
        {
            var list = await _repository
                .GetListAsync(
                    tran => tran.PaymentRequestId == paymentRequestId
                    && transactionTypes.Contains(tran.TransactionTypeId.Value)
                );
            return list;
        }

        public async Task<Transaction> InsertTransaction(Transaction transaction)
        {
            transaction.CreatedDate = DateTime.Now;
            transaction = await _repository.InsertAsync(transaction, true);
            return transaction;
        }

        public async Task<decimal?> GetSumAmountOfPaymentRequest(Guid paymentRequestId, EnmTransactionType transactionTypeId, DateTime? paymentRequestDate)
        {

            var list = await _repository.GetListAsync
                (
                    tran =>
                    tran.PaymentRequestDate == paymentRequestDate &&
                    tran.PaymentRequestId == paymentRequestId
                    && tran.TransactionTypeId == transactionTypeId
                );
            return list.Sum(x => x.Amount);
        }
        public async Task<decimal?> GetSumAmountOfPaymentRequest(Guid paymentRequestId, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate)
        {
            var list = await _repository
                .GetListAsync(
                    tran => tran.PaymentRequestDate == paymentRequestDate &&
                    tran.PaymentRequestId == paymentRequestId
                    && transactionTypes.Contains(tran.TransactionTypeId.Value)
                );
            return list.Sum(x => x.Amount);
        }
        public async Task<decimal?> GetSumAmountOfPaymentRequest(Guid paymentRequestId, List<EnmTransactionType> transactionTypes)
        {
            //không sử dụng nữa v1
            var list = await _repository
                .GetListAsync(
                    tran => tran.PaymentRequestId == paymentRequestId
                    && transactionTypes.Contains(tran.TransactionTypeId.Value)
                );
            return list.Sum(x => x.Amount);
        }

        public async Task<List<Transaction>> GetByListPaymentRequestIds(List<Guid> listIds)
        {
            return await _repository.GetListAsync
                (trans => listIds.Contains(trans.PaymentRequestId.Value) &&
                trans.TransactionTypeId == EnmTransactionType.Recharge);
        }

        public async Task<List<Transaction>> GetByPaymentRequestId(Guid paymentRequestId, DateTime? paymentRequestDate)
        {
            if (paymentRequestDate == null)
                return await _repository.GetListAsync(tran => tran.PaymentRequestId == paymentRequestId);
            return await _repository.GetListAsync(tran => tran.PaymentRequestId == paymentRequestId && tran.PaymentRequestDate == paymentRequestDate);
        }
        public async Task<decimal?> GetTotalDeposited(Guid paymentRequestId, DateTime? paymentRequestDate)
        {
            var rt = await GetByPaymentRequestId(paymentRequestId, paymentRequestDate);
            if (rt == null)
                return null;
            else
                return rt.WhereIf(paymentRequestDate.HasValue, tran => tran.PaymentRequestDate == paymentRequestDate)
                    .Where(c => c.TransactionTypeId == EnmTransactionType.Recharge)
                    .Sum(c => c.Amount ?? 0m);
        }

        public async Task<bool> HasTransferDepositNotIsConfirmTrans(Guid paymentRequestId, DateTime? paymentRequestDate)
        {
            var listTrans = await _repository.GetListAsync(
                    tran => tran.PaymentRequestDate == paymentRequestDate &&
                    tran.PaymentRequestId == paymentRequestId
                    && tran.TransactionTypeId == EnmTransactionType.Recharge
                    && tran.PaymentMethodId == EnmPaymentMethod.Transfer
                );
            var flag = await _transferRepository.HasTransferDepositNotIsConfirmTrans(listTrans.Select(x => x.Id).ToList());
            return flag;
        }
        public async Task<bool> HasTransferDepositNotIsConfirmTrans(Guid paymentRequestId, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate)
        {
            var listTrans = await _repository.GetListAsync
                (tran => tran.PaymentRequestDate == paymentRequestDate &&
                     tran.PaymentRequestId == paymentRequestId
                    && transactionTypes.Contains(tran.TransactionTypeId.Value)
                    && tran.PaymentMethodId == EnmPaymentMethod.Transfer
                );
            var flag = await _transferRepository.HasTransferDepositNotIsConfirmTrans(listTrans.Select(x => x.Id).ToList());
            return flag;
        }
        public async Task<Transaction> FindTransaction(Guid? transactionId, EnmTransactionType? transactionType)
        {
            var result = await _repository
               .FirstOrDefaultAsync(tran => tran.Id.Equals(transactionId) && tran.TransactionTypeId.Equals(transactionType));
            return result;
        }

        public async Task<List<Transaction>> GetByPaymentRequestInfo(string paymentRequestCode, EnmTransactionType transactionTypeId, DateTime? paymentRequestDate)
        {
            if (!paymentRequestDate.HasValue)
            {
                paymentRequestDate = PaymentCoreUtilities.GetDate(paymentRequestCode);
            }
            return await _repository
                .GetListAsync(
                    tran => tran.PaymentRequestDate == paymentRequestDate &&
                    tran.PaymentRequestCode == paymentRequestCode
                    && tran.TransactionTypeId == transactionTypeId
                );
        }

        public async Task<List<Transaction>> GetByPaymentRequestInfo(string paymentRequestCode, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate)
        {
            if (!paymentRequestDate.HasValue)
            {
                paymentRequestDate = PaymentCoreUtilities.GetDate(paymentRequestCode);
            }
            return await _repository
                .GetListAsync(
                    tran => tran.PaymentRequestDate == paymentRequestDate &&
                    tran.PaymentRequestCode == paymentRequestCode
                    && transactionTypes.Contains(tran.TransactionTypeId.Value)
                );
        }

        public async Task<List<Transaction>> GetByPaymentRequestCode(string paymentRequestCode, DateTime? paymentRequestDate)
        {
            if (paymentRequestDate == null)
                paymentRequestDate = PaymentCoreUtilities.GetDate(paymentRequestCode);
            return await _repository.GetListAsync(tran => tran.PaymentRequestDate == paymentRequestDate && tran.PaymentRequestCode == paymentRequestCode );
        }

        public async Task<decimal?> GetSumAmountOfPaymentRequest(string paymentRequestCode, EnmTransactionType transactionTypeId, DateTime? paymentRequestDate)
        {
            if (!paymentRequestDate.HasValue)
            {
                paymentRequestDate = PaymentCoreUtilities.GetDate(paymentRequestCode);
            }
            var list = await _repository.GetListAsync(
                tran =>
                       tran.PaymentRequestDate == paymentRequestDate &&
                     tran.PaymentRequestCode == paymentRequestCode
                    && tran.TransactionTypeId == transactionTypeId
                );
            return list.Sum(x => x.Amount);
        }

        public async Task<decimal?> GetSumAmountOfPaymentRequest(string paymentRequestCode, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate)
        {
            if (!paymentRequestDate.HasValue)
            {
                paymentRequestDate = PaymentCoreUtilities.GetDate(paymentRequestCode);
            }
            var list = await _repository.GetListAsync
                (
                    tran => tran.PaymentRequestDate == paymentRequestDate &&
                    tran.PaymentRequestCode == paymentRequestCode
                    && transactionTypes.Contains(tran.TransactionTypeId.Value)
                );
            return list.Sum(x => x.Amount);
        }

        public async Task<decimal?> GetTotalDeposited(string paymentRequestCode, DateTime? paymentRequestDate)
        {
            var rt = await GetByPaymentRequestCode(paymentRequestCode, paymentRequestDate);
            if (rt == null)
                return null;
            else
                return rt.WhereIf(paymentRequestDate.HasValue, tran => tran.PaymentRequestDate == paymentRequestDate)
                    .Where(c => c.TransactionTypeId == EnmTransactionType.Recharge)
                    .Sum(c => c.Amount ?? 0m);
        }

        public async Task<bool> HasTransferDepositNotIsConfirmTrans(string paymentRequestCode, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate)
        {
            if (!paymentRequestDate.HasValue)
            {
                paymentRequestDate = PaymentCoreUtilities.GetDate(paymentRequestCode);
            }
            var listTrans = await _repository.GetListAsync
                (
                    tran => tran.PaymentRequestDate == paymentRequestDate &&
                    tran.PaymentRequestCode == paymentRequestCode
                    && transactionTypes.Contains(tran.TransactionTypeId.Value)
                    && tran.PaymentMethodId == EnmPaymentMethod.Transfer
                );
            var flag = await _transferRepository.HasTransferDepositNotIsConfirmTrans(listTrans.Select(x => x.Id).ToList());
            return flag;
        }

        public async Task<bool> HasTransferDepositNotIsConfirmTrans(string paymentRequestCode, DateTime? paymentRequestDate)
        {
            if (!paymentRequestDate.HasValue)
            {
                paymentRequestDate = PaymentCoreUtilities.GetDate(paymentRequestCode);
            }
            var listTrans = await _repository
                .GetListAsync(
                    tran => tran.PaymentRequestDate == paymentRequestDate &&
                    tran.PaymentRequestCode == paymentRequestCode
                    && (tran.TransactionTypeId == EnmTransactionType.Recharge
                    || tran.TransactionTypeId == EnmTransactionType.FirstDeposit)
                    && tran.PaymentMethodId == EnmPaymentMethod.Transfer
                );
            var flag = await _transferRepository.HasTransferDepositNotIsConfirmTrans(listTrans.Select(x => x.Id).ToList());
            return flag;
        }
        public async Task<bool> HasTransferDepositNotIsConfirmTrans(Guid paymentRequestId, List<EnmTransactionType> transactionTypes)
        {
            // không dùng nữa v1
            var listTrans = await _repository.GetListAsync
                (
                    tran => tran.PaymentRequestId == paymentRequestId
                    && transactionTypes.Contains(tran.TransactionTypeId.Value)
                    && tran.PaymentMethodId == EnmPaymentMethod.Transfer
                );
            var flag = await _transferRepository.HasTransferDepositNotIsConfirmTrans(listTrans.Select(x => x.Id).ToList());
            return flag;
        }
        public async Task<Transaction> GetTransactionMethod(Guid? paymentRequestId, EnmPaymentMethod? paymentMethod)
        {
            var result = await _repository
               .FirstOrDefaultAsync(tran => tran.PaymentRequestId.Equals(paymentRequestId) && tran.PaymentMethodId.Equals(paymentMethod));
            return result;
        }
        public async Task<bool> HasTransferDepositNotIsConfirmTransPayment(List<string> paymentRequestCodes, DateTime? paymentRequestDate)
        {
            var listTrans = await _repository
                .GetListAsync
                (
                    tran => tran.PaymentRequestDate == paymentRequestDate &&
                    paymentRequestCodes.Contains(tran.PaymentRequestCode)
                    && tran.TransactionTypeId == EnmTransactionType.Recharge
                    && tran.PaymentMethodId == EnmPaymentMethod.Transfer
                );
            var flag = await _transferRepository.HasTransferDepositNotIsConfirmTrans(listTrans.Select(x => x.Id).ToList());
            return flag;
        }
    }
}
