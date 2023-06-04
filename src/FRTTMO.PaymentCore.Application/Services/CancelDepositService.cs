using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Dto.TransactionDetailDto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Exceptions;
using FRTTMO.PaymentCore.Kafka.Eto;
using FRTTMO.PaymentCore.Kafka.Interface;
using FRTTMO.PaymentCore.Repositories;
using FRTTMO.PaymentCore.Services.v2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Uow;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services
{
    public class CancelDepositService : PaymentCoreAppService, ICancelDepositService
    {
        private readonly IPublishService<BaseETO> _iPublishService;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IPaymentService _paymentService;
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ITransactionService _transactionService;
        private readonly ICancelDepositRepository _iCancelDepositRepository;
        private readonly ITransferService _iTransferService;
        private readonly IElasticSearchService _syncDataESAppService;

        public CancelDepositService(
            IPublishService<BaseETO> iPublishService,
            IPaymentRequestRepository paymentRequestRepository,
            IPaymentService paymentService,
            IAccountRepository accountRepository,
            IUnitOfWorkManager unitOfWorkManager, ITransactionService transactionService,
            ICancelDepositRepository iCancelDepositRepository,
            ITransferService iTransferService,
            IElasticSearchService syncDataESAppService

        ) : base()
        {
            _iPublishService = iPublishService;
            _paymentRequestRepository = paymentRequestRepository;
            _paymentService = paymentService;
            _accountRepository = accountRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _transactionService = transactionService;
            _iCancelDepositRepository = iCancelDepositRepository;
            _iTransferService = iTransferService;
            _syncDataESAppService = syncDataESAppService;
        }
        #region phần hủy cọc chi tiền mặt
        /// <summary>
        /// CreateWithdrawDepositCash
        /// hủy cọc chi tiền mặt 
        /// </summary>
        /// <param name="cancelDepositDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<CreatePaymentTransactionOutputDto> CreateWithdrawDepositCash(CancelDepositDto cancelDepositDto)
        {
            var account = await _accountRepository.GetById(cancelDepositDto.Transaction.AccountId.Value);
            if (account == null)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ACCOUNT_NOTFOUND).WithData("AccountId", cancelDepositDto.Transaction.AccountId.Value);

            if (string.IsNullOrEmpty(cancelDepositDto.OrderCode))
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", "OrderCode");

            if (cancelDepositDto.TypePayment != EmPaymentRequestType.DepositRefund)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_TYPE_INVALID).WithData("TypePayment", cancelDepositDto.TypePayment);
            //kiểm tra đã hủy cọc chưa
            if (await _paymentRequestRepository.CheckExistsComplete(cancelDepositDto.OrderCode, EmPaymentRequestType.DepositRefund))
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_COMPLETE_CANCEL).WithData("OrderCode", cancelDepositDto.OrderCode);

            //Tạo PaymentRequest
            var paymentRequest = ObjectMapper.Map<CancelDepositDto, PaymentRequestInsertDto>(cancelDepositDto);
            var resultPaymentRequest = await _paymentService.CreateRequest(paymentRequest);
            if (resultPaymentRequest != null)
            {
                var transactionDto = ObjectMapper.Map<InsertTransactionCancelInputDto, InsertTransactionInputDto>(cancelDepositDto.Transaction);
                transactionDto.CreatedBy = cancelDepositDto.CreatedBy;
                transactionDto.PaymentRequestId = resultPaymentRequest.Id;
                transactionDto.PaymentRequestCode = resultPaymentRequest.PaymentRequestCode;
                var paymentRequestDate = resultPaymentRequest.PaymentRequestDate;
                transactionDto.PaymentRequestDate = paymentRequestDate;
                //CashBack tiền về ví cho KH (vì ở đây khi đặt cọc sẽ bị thanh toán đơn cọc để hoàn tất cọc nên cần phải rút tiền về)
                var paymentCashBackRequest = new CashbackDepositRefundBaseDto()
                {
                    OrderCode = cancelDepositDto.OrderCode,
                    TotalPayment = cancelDepositDto.TotalPayment,
                    Transaction = transactionDto,
                };
                var extAc = await CreateCashbackTransaction(paymentCashBackRequest, account);
                //Tiếp tục Tạo PaymentTransaction
                //insert transaction
                if (cancelDepositDto.Transaction.Amount > extAc.CurrentBalance.Value)
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTENOUGH).WithData("AccountId", extAc.Id);

                //Nếu là loại chuyên khoản hoặc loại tiền mặt
                var createTransactionOutput = new CreatePaymentTransactionOutputDto();

                //insert transaction
                using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                {
                    try
                    {
                        var insertInput = transactionDto;
                        insertInput.AccountId = extAc.Id;
                        insertInput.TransactionTypeId = EnmTransactionType.WithdrawDeposit;
                        insertInput.Status = EnmTransactionStatus.Created;
                        insertInput.PaymentMethodId = cancelDepositDto.Transaction.PaymentMethodId;
                        var insertOutput = await _transactionService.InsertTransaction(insertInput);
                        extAc.CurrentBalance = extAc.CurrentBalance - cancelDepositDto.Transaction.Amount;
                        await _accountRepository.Update(extAc);

                        await unitOfWork.SaveChangesAsync();
                        createTransactionOutput.Transaction = insertOutput;
                        //publish kafka message
                        var paymentTransEto = ObjectMapper.Map<CreatePaymentTransactionOutputDto, WithdrawDepositCompletedOutputEto>(createTransactionOutput);
                        paymentTransEto.OrderCode = cancelDepositDto.OrderCode;
                        paymentTransEto.CreatedByName = cancelDepositDto.CreatedByName;
                        //cập nhật trạng thái của paymentRequest
                        var paymentRequestFinish = await _paymentRequestRepository.GetByPaymentRequestCode(resultPaymentRequest.PaymentRequestCode, paymentRequestDate);
                        //
                        paymentRequestFinish.Status = EnmPaymentRequestStatus.Complete;
                        paymentRequestFinish.CreatedByName = cancelDepositDto.CreatedByName;
                        await _paymentRequestRepository.Update(paymentRequestFinish);
                        await _iPublishService.ProduceAsync(paymentTransEto);
                        ///

                        //// Sync ES
                        //var result = new Dto.TransactionDetailDto.TransactionDetailTransferOutputDto();
                        //var paymentMethod = paymentTransEto.Transaction.PaymentMethodId;
                        //var listTransactionID = new List<Guid>() { paymentTransEto.Transaction.Id };

                        //result.OrderCode = cancelDepositDto.OrderCode;
                        //result.PaymentCode = paymentRequestFinish.PaymentCode;
                        //result.Amount = cancelDepositDto.Transaction.Amount;
                        //result.CreatedBy = cancelDepositDto.CreatedBy;
                        //result.TypePayment = (byte?)paymentRequestFinish.TypePayment;
                        //result.CreatedDate = DateTime.Now;
                        //result.PaymentMethodId = (int)EnmPaymentMethod.Cash;
                        //result.CreatedByName = cancelDepositDto.CreatedByName;
                        //result.PaymentRequest = ObjectMapper.Map<PaymentRequest, PaymentRequestSyncOutPutDto>(paymentRequestFinish);
                        //await _syncDataESAppService.SyncDataESTransfer(cancelDepositDto.OrderCode, result);
                    }
                    catch (Exception)
                    {
                        await unitOfWork.RollbackAsync();
                        throw;
                    }
                }
                return createTransactionOutput;
            }
            throw new Exception("Can't Cancel Deposit");
        }
        #endregion
        public async Task<Account> CreateCashbackTransaction(CashbackDepositRefundBaseDto cashbackDto, Account account)
        {
            if (cashbackDto.Transaction.TransactionTypeId != EnmTransactionType.WithdrawDeposit)
            {
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_TRANSACTION_TYPE_INVALID).WithData("TransactionTypeName", EnmTransactionType.WithdrawDeposit);
            }
            else
            {
                using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                {
                    //insert transaction
                    try
                    {
                        var transactionInput = cashbackDto.Transaction;
                        transactionInput.TransactionTypeId = EnmTransactionType.CashBack;
                        transactionInput.PaymentMethodId = EnmPaymentMethod.Cash;
                        transactionInput.Status = EnmTransactionStatus.Created;
                        transactionInput.Amount = cashbackDto.TotalPayment;
                        var transactionOutput = await _transactionService.InsertTransaction(transactionInput);
                        //update account
                        account.CurrentBalance = account.CurrentBalance + cashbackDto.TotalPayment;
                        await _accountRepository.Update(account);

                        //Bắn kafka
                        var cashbackEto = new CashbackCreatedEto();
                        var transactionEto = ObjectMapper.Map<TransactionFullOutputDto, TransactionFullOutputEto>(transactionOutput);
                        cashbackEto.Transaction = transactionEto;
                        cashbackDto.OrderCode = cashbackDto.OrderCode;
                        await _iPublishService.ProduceAsync(cashbackEto);
                        await unitOfWork.SaveChangesAsync();
                        return account;
                    }
                    catch (Exception)
                    {
                        await unitOfWork.RollbackAsync();
                        throw;
                    }
                }
            }
        }
        public async Task<List<PaymentPayMethodDto>> Getlistpaymentpaymethod()
        {
            var response = await _iCancelDepositRepository.GetlistpaymentpaymethodAsync();
            var listMap = ObjectMapper.Map<List<PaymentMethod>, List<PaymentPayMethodDto>>(response);
            return listMap;
        }
        #region phần hủy cọc chi tiền chuyển khoản 
        /// <summary>
        /// Create paymentRequest hình thức chuyển khoản
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PaymentRequestFullOutputDto> CreatePaymentRequesttransfer(PaymentRequestTransferDto request)
        {
            if (string.IsNullOrEmpty(request.OrderCode))
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", "OrderCode");
            if (await _paymentRequestRepository.CheckExistsComplete(request.OrderCode, EmPaymentRequestType.DepositRefund))
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_COMPLETE_CANCEL).WithData("OrderCode", request.OrderCode);
            var paymentRequest = ObjectMapper.Map<PaymentRequestTransferDto, PaymentRequestInsertDto>(request);
            paymentRequest.TypePayment = EmPaymentRequestType.DepositRefund;
            //Tạo PaymentRequest
            var result = await _paymentService.CreateRequest(paymentRequest);
            //// Sync ES
            //var resultES = new Dto.TransactionDetailDto.TransactionDetailOutputDto();

            ////result.TransactionID = paymentTransEto.Transaction.Id.ToString();
            //resultES.OrderCode = request.OrderCode;
            ////resultES.Amount = request.Amount;
            //resultES.CreatedBy = request.CreatedBy;
            //resultES.CreatedDate = DateTime.Now;
            //resultES.PaymentMethodId = (int)EnmPaymentMethod.Transfer;
            //resultES.CreatedByName = request.CreatedByName;

            //await _syncDataESAppService.SyncDataESTransfer(resultES.OrderCode, resultES);
            return result;

        }
        /// <summary>
        /// CreateTransactiontransfer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CreatePaymentTransactionOutputDto> CreateTransactiontransfer(TransactionCancelDepositTransfer request)
        {
            if (request.Transfers == null)
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_METHOD_INVALID);
            var account = await _accountRepository.GetByCustomerId(request.CustomerId.Value);
            if (account == null)
                //tài khoản có tồn tại không?
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_NOTFOUND);
            var resultPaymentRequest = await _paymentRequestRepository.GetById(request.PaymentRequestId.Value);

            if (resultPaymentRequest == null)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_NOTFOUND).WithData("PaymentRequestId", request.PaymentRequestId.Value);
            if (resultPaymentRequest.Status == EnmPaymentRequestStatus.Complete)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_COMPLETE_CANCEL).WithData("OrderCode", resultPaymentRequest.OrderCode);

            var requestTransaction = new InsertTransactionInputDto
            {
                CreatedDate = DateTime.Now,
                CreatedBy = resultPaymentRequest.CreatedBy,
                AccountId = account.Id,
                TransactionTypeId = EnmTransactionType.WithdrawDeposit,
                Status = EnmTransactionStatus.Created,
                TransactionTime = request.TransactionTime,
                Amount = resultPaymentRequest.TotalPayment,
                ShopCode = request.ShopCode,
                PaymentRequestId = resultPaymentRequest.Id,
                PaymentMethodId = EnmPaymentMethod.Transfer,
                TransactionFee = 0,
                Note = "",
                AdditionAttributes = "",
                PaymentRequestDate = resultPaymentRequest.PaymentRequestDate,
                PaymentRequestCode = resultPaymentRequest.PaymentRequestCode
            };
            //CashBack tiền về ví cho KH (vì ở đây khi đặt cọc sẽ bị thanh toán đơn cọc để hoàn tất cọc nên cần phải rút tiền về)
            var paymentCashBackRequest = new CashbackDepositRefundBaseDto
            {
                OrderCode = resultPaymentRequest.OrderCode,
                TotalPayment = resultPaymentRequest.TotalPayment,
                Transaction = requestTransaction
            };
            //insert transaction
            var createTransactionOutput = new CreatePaymentTransactionOutputDto();

            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    var extAc = await CreateCashbackTransaction(paymentCashBackRequest, account);

                    requestTransaction.PaymentMethodId = EnmPaymentMethod.Transfer;
                    requestTransaction.TransactionTypeId = EnmTransactionType.WithdrawDeposit;
                    var insertOutput = await _transactionService.InsertTransaction(requestTransaction);
                    extAc.CurrentBalance = extAc.CurrentBalance - requestTransaction.Amount;
                    await _accountRepository.Update(extAc);

                    //nếu là loại chuyển khoản thì phải lưu thông tin tài khoản ngân hàng của KH vào table Tranfer
                    var tranfer = ObjectMapper.Map<PaymentTransferInputDto, TransferInputDto>(request.Transfers);
                    tranfer.TransactionId = insertOutput.Id;
                    tranfer.CreatedBy = resultPaymentRequest.CreatedBy;
                    var tranferEto = await _iTransferService.Insert(tranfer);

                    await unitOfWork.SaveChangesAsync();
                    createTransactionOutput.Transaction = insertOutput;

                    //publish kafka message
                    var paymentTransEto = ObjectMapper.Map<CreatePaymentTransactionOutputDto, WithdrawDepositCompletedOutputEto>(createTransactionOutput);
                    paymentTransEto.OrderCode = resultPaymentRequest.OrderCode;
                    paymentTransEto.CreatedByName = request.Transfers.CreatedByName;
                    paymentTransEto.Transfers = ObjectMapper.Map<TransferFullOutputDto, TransferFullOutputEto>(tranferEto);
                    //cập nhật trạng thái của paymentRequest
                    var paymentRequestFinish = await _paymentRequestRepository.GetByPaymentRequestCode(resultPaymentRequest.PaymentRequestCode, resultPaymentRequest.PaymentRequestDate);
                    //
                    paymentRequestFinish.Status = EnmPaymentRequestStatus.Complete;
                    paymentRequestFinish.CreatedByName = request.Transfers.CreatedByName;
                    await _paymentRequestRepository.Update(paymentRequestFinish);
                    await _iPublishService.ProduceAsync(paymentTransEto);

                    //// Sync ES
                    //var resultES = new Dto.TransactionDetailDto.TransactionDetailOutputDto();

                    ////result.TransactionID = paymentTransEto.Transaction.Id.ToString();
                    //resultES.OrderCode = resultPaymentRequest.OrderCode;
                    //resultES.Amount = request.Transfers.Amount;
                    //resultES.CreatedBy = request.Transfers.CreatedBy;
                    //resultES.CreatedDate = DateTime.Now;
                    //resultES.PaymentMethodId = (int)EnmPaymentMethod.Transfer;
                    //resultES.CreatedByName = request.Transfers.CreatedByName;

                    //await _syncDataESAppService.SyncDataESTransfer(resultES.OrderCode, resultES);
                }
                catch (Exception)
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
            return createTransactionOutput;
        }
        #endregion
    }
}

