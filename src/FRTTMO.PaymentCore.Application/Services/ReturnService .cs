using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Exceptions;
using FRTTMO.PaymentCore.Kafka.Eto;
using FRTTMO.PaymentCore.Kafka.Interface;
using FRTTMO.PaymentCore.Repositories;
using FRTTMO.PaymentCore.Services.v2;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Uow;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services
{
    public class ReturnService : PaymentCoreAppService, IReturnService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionService _transactionService;
        private readonly IPublishService<BaseETO> _iPublishService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ITransferService _iTransferService;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IGenerateServiceV2 _generateServiceV2;

        public ReturnService(IAccountRepository accountRepository,
            ITransactionService transactionService, IPublishService<BaseETO> iPublishService,
            IUnitOfWorkManager unitOfWorkManager,
            ITransferService iTransferService,
            IPaymentRequestRepository paymentRequestRepository,
            IGenerateServiceV2 generateServiceV2
        ) : base()
        {
            _accountRepository = accountRepository;
            _transactionService = transactionService;
            _iPublishService = iPublishService;
            _unitOfWorkManager = unitOfWorkManager;
            _iTransferService = iTransferService;
            _paymentRequestRepository = paymentRequestRepository;
            _generateServiceV2 = generateServiceV2;
        }

        /// <summary>
        /// trả hàng chi tiền mặt 
        /// </summary>
        /// <param name="returnRequest"></param>
        /// <returns></returns>
        public async Task<CreatePaymentTransactionOutputDto> ReturnTransactionCash(ReturnCashDto returnRequest)
        {
            //insert transaction
            var paymentRequest = await _paymentRequestRepository.GetByOrderCode(returnRequest.OrderCode, returnRequest.OrderReturnId);
            if (paymentRequest == null)
            {
                //kiểm tra paymentRequest có tồn tại orderCode và OrderReturn không?
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ORDERRETURNID_PAYMENT_NOTFOUND).WithData("OrderReturnId", returnRequest.OrderReturnId);
            }
            else if (paymentRequest.Status.Equals(EnmPaymentRequestStatus.Complete))
            {
                //kiểm tra trạng thái PaymentRequest có completed chưa?
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYEMNT_REQUEST_COMPLETED);
            }

            var account = await _accountRepository.GetByCustomerId(returnRequest.Transaction.CustomerId.Value);
            if (account == null)
            {
                //tài khoản có tồn tại không?
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_NOTFOUND);
            }
            else if (returnRequest.Totalpayment > paymentRequest.TotalPayment)
            {
                //nếu số tiền truyền vào mà lớn hơn số tiền trả hàng thì sẽ không hợp lệ 
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTENOUGH);
            }
            else
            {
                using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                {
                    try
                    {
                        //cashback returnId
                        var transactionInput = returnRequest.Transaction;
                        transactionInput.TransactionTypeId = EnmTransactionType.CashBack;
                        transactionInput.Status = EnmTransactionStatus.Created;
                        transactionInput.Amount = returnRequest.Totalpayment;
                        var requestCashBack = ObjectMapper.Map<TransactionReturnDto, InsertTransactionInputDto>(transactionInput);
                        requestCashBack.AccountId = account.Id;
                        requestCashBack.PaymentRequestId = paymentRequest.Id;
                        requestCashBack.CreatedBy = returnRequest.CreatedBy ?? paymentRequest.CreatedBy;
                        requestCashBack.PaymentMethodId = EnmPaymentMethod.Cash;
                        requestCashBack.PaymentRequestCode = paymentRequest.PaymentRequestCode;
                        requestCashBack.PaymentRequestDate = paymentRequest.PaymentRequestDate;
                        var transactionOutput = await _transactionService.InsertTransaction(requestCashBack);


                        //update account
                        account.CurrentBalance = account.CurrentBalance + returnRequest.Totalpayment;
                        await _accountRepository.Update(account);

                        //Bắn kafka
                        var cashbackEto = new CashbackCreatedEto();
                        var transactionEto = ObjectMapper.Map<TransactionFullOutputDto, TransactionFullOutputEto>(transactionOutput);
                        cashbackEto.Transaction = transactionEto;
                        returnRequest.OrderCode = returnRequest.OrderCode;
                        await _iPublishService.ProduceAsync(cashbackEto);
                        await unitOfWork.SaveChangesAsync();

                        //insert Transaction
                        var insertInput = transactionOutput;
                        insertInput.TransactionTypeId = EnmTransactionType.Refund;
                        insertInput.Status = EnmTransactionStatus.Created;
                        insertInput.PaymentMethodId = EnmPaymentMethod.Cash;
                        var requestRefund = ObjectMapper.Map<TransactionFullOutputDto, InsertTransactionInputDto>(insertInput);
                        requestRefund.AccountId = account.Id;

                        var insertOutput = await _transactionService.InsertTransaction(requestRefund);
                        account.CurrentBalance = account.CurrentBalance - returnRequest.Transaction.Amount;
                        await _accountRepository.Update(account);
                        var createTransactionOutput = new CreatePaymentTransactionOutputDto();
                        await unitOfWork.SaveChangesAsync();
                        createTransactionOutput.Transaction = insertOutput;

                        //publish kafka message
                        var paymentTransEto = ObjectMapper.Map<CreatePaymentTransactionOutputDto, WithdrawReturnCompletedOutputEto>(createTransactionOutput);
                        paymentTransEto.OrderCode = returnRequest.OrderCode;
                        paymentTransEto.OrderReturnId = returnRequest.OrderReturnId != null? returnRequest.OrderReturnId.ToString() :"";
                        //cập nhật trạng thái của paymentRequest
                        var paymentRequestFinish = await _paymentRequestRepository.GetByPaymentRequestCode(paymentRequest.PaymentRequestCode, paymentRequest.PaymentRequestDate);

                        paymentRequestFinish.Status = EnmPaymentRequestStatus.Complete;
                        await _paymentRequestRepository.Update(paymentRequestFinish);
                        await _iPublishService.ProduceAsync(paymentTransEto);
                        return createTransactionOutput;
                    }
                    catch (AbpDbConcurrencyException)
                    {

                        await unitOfWork.RollbackAsync();
                        throw;
                    }
                }
            }
        }
        /// <summary>
        /// trả hàng chi tiền chuyển khoản
        /// </summary>
        /// <param name="returnRequest"></param>
        /// <returns></returns>
        /// <exception cref="CustomBusinessException"></exception>
        public async Task<CreatePaymentTransactionOutputDto> ReturnTransactionTransfer(ReturnTransferDto returnRequest)
        {
            if (returnRequest.Transfers == null)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_METHOD_INVALID);
            var account = await _accountRepository.GetByCustomerId(returnRequest.CustomerId.Value);
            if (account == null)
                //tài khoản có tồn tại không?
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_NOTFOUND);
            var resultPaymentRequest = await _paymentRequestRepository.GetByOrderCode(returnRequest.OrderCode, returnRequest.OrderReturnId);
            if (resultPaymentRequest == null)
                //kiểm tra paymentRequest có tồn tại orderCode và OrderReturn không?
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ORDERRETURNID_PAYMENT_NOTFOUND).WithData("OrderReturnId", returnRequest.OrderReturnId);
            if (resultPaymentRequest.Status == EnmPaymentRequestStatus.Complete)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYEMNT_REQUEST_COMPLETED);
            var createdBy = returnRequest.Transfers.CreatedBy ?? resultPaymentRequest.CreatedBy;
            var requestTransaction = new InsertTransactionInputDto
            {
                CreatedDate = DateTime.Now,
                CreatedBy = createdBy,
                AccountId = account.Id,
                TransactionTypeId = EnmTransactionType.Refund,
                Status = EnmTransactionStatus.Created,
                TransactionTime = returnRequest.TransactionTime,
                Amount = returnRequest.Transfers.Amount,
                ShopCode = returnRequest.ShopCode,
                PaymentRequestId = resultPaymentRequest.Id,
                PaymentMethodId = EnmPaymentMethod.Transfer,
                TransactionFee = 0,
                Note = "",
                AdditionAttributes = "",
                PaymentRequestCode = resultPaymentRequest.PaymentRequestCode,
                PaymentRequestDate = resultPaymentRequest.PaymentRequestDate
            };
            //CashBack tiền về ví cho KH (vì ở đây khi đặt cọc sẽ bị thanh toán đơn cọc để hoàn tất cọc nên cần phải rút tiền về)
            var paymentCashBackRequest = new CashbackDepositRefundBaseDto
            {
                OrderCode = resultPaymentRequest.OrderCode,
                TotalPayment = resultPaymentRequest.TotalPayment,
                Transaction = requestTransaction
            };

            var createTransactionOutput = new CreatePaymentTransactionOutputDto();

            //insert transaction
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    //insert cashback
                    var transactionInput = paymentCashBackRequest.Transaction;
                    transactionInput.TransactionTypeId = EnmTransactionType.CashBack;
                    transactionInput.PaymentMethodId = EnmPaymentMethod.Cash;
                    var transactionOutput = await _transactionService.InsertTransaction(transactionInput);
                    //update account
                    account.CurrentBalance = account.CurrentBalance + paymentCashBackRequest.TotalPayment;
                    await _accountRepository.Update(account);

                    //Bắn kafka
                    var cashbackEto = new CashbackCreatedEto();
                    var transactionEto = ObjectMapper.Map<TransactionFullOutputDto, TransactionFullOutputEto>(transactionOutput);
                    cashbackEto.Transaction = transactionEto;
                    cashbackEto.OrderCode = paymentCashBackRequest.OrderCode;
                    await _iPublishService.ProduceAsync(cashbackEto);
                    await unitOfWork.SaveChangesAsync();

                    //insert cashback Re
                    requestTransaction.TransactionTypeId = EnmTransactionType.Refund;
                    requestTransaction.Status = EnmTransactionStatus.Created;
                    requestTransaction.PaymentMethodId = EnmPaymentMethod.Transfer;
                    var insertOutput = await _transactionService.InsertTransaction(requestTransaction);
                    account.CurrentBalance = account.CurrentBalance - requestTransaction.Amount;
                    await _accountRepository.Update(account);

                    //nếu là loại chuyển khoản thì phải lưu thông tin tài khoản ngân hàng của KH vào table Tranfer
                    var tranfer = ObjectMapper.Map<PaymentTransferInputDto, TransferInputDto>(returnRequest.Transfers);
                    tranfer.TransactionId = insertOutput.Id;
                    tranfer.CreatedBy = createdBy;
                    var tranferEto = await _iTransferService.Insert(tranfer);

                    await unitOfWork.SaveChangesAsync();
                    createTransactionOutput.Transaction = insertOutput;
                    //publish kafka message
                    var paymentTransEto = ObjectMapper.Map<CreatePaymentTransactionOutputDto, WithdrawReturnCompletedOutputEto>(createTransactionOutput);
                    paymentTransEto.OrderCode = resultPaymentRequest.OrderCode;
                    paymentTransEto.OrderReturnId = returnRequest.OrderReturnId != null ? returnRequest.OrderReturnId.ToString() : "";
                    paymentTransEto.Transfers = ObjectMapper.Map<TransferFullOutputDto, TransferFullOutputEto>(tranferEto);

                    //cập nhật trạng thái của paymentRequest
                    var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(resultPaymentRequest.PaymentRequestCode);
                    var paymentRequestFinish = await _paymentRequestRepository.GetByPaymentRequestCode(resultPaymentRequest.PaymentRequestCode, paymentRequestDate);

                    paymentRequestFinish.Status = EnmPaymentRequestStatus.Complete;
                    await _paymentRequestRepository.Update(paymentRequestFinish);
                    await _iPublishService.ProduceAsync(paymentTransEto);
                }
                catch (AbpDbConcurrencyException)
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
            return createTransactionOutput;
        }
    }
}
