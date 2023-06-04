using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Dto.v2;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Exceptions;
using FRTTMO.PaymentCore.Kafka.Eto;
using FRTTMO.PaymentCore.Kafka.Interface;
using FRTTMO.PaymentCore.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services.v2
{
    public class CancelDepositServiceV2 : PaymentCoreAppService, ITransientDependency, ICancelDepositServiceV2
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IGenerateServiceV2 _generateServiceV2;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ITransactionServiceV2 _transactionServiceV2;
        private readonly ITransferService _iTransferService;
        private readonly IPublishService<BaseETO> _iPublishService;
        private readonly IElasticSearchService _syncDataESAppService;
        private readonly IDepositServiceV2 _depositServiceV2;
        public CancelDepositServiceV2(
            IAccountRepository accountRepository,
            IPaymentRequestRepository paymentRequestRepository,
            IGenerateServiceV2 generateServiceV2,
            IUnitOfWorkManager unitOfWorkManager,
            ITransactionServiceV2 transactionServiceV2,
            ITransferService iTransferService,
            IPublishService<BaseETO> iPublishService,
            IElasticSearchService syncDataESAppService,
            IDepositServiceV2 depositServiceV2
            )
        {
            _accountRepository = accountRepository;
            _paymentRequestRepository = paymentRequestRepository;
            _generateServiceV2 = generateServiceV2;
            _unitOfWorkManager = unitOfWorkManager;
            _transactionServiceV2 = transactionServiceV2;
            _iTransferService = iTransferService;
            _iPublishService = iPublishService;
            _syncDataESAppService = syncDataESAppService;
            _depositServiceV2 = depositServiceV2;
        }
        public async Task<CreatePaymentTransactionOutputDtoV2> CreateTransactiontransfer(TransactionCancelDepositTransferV2 request)
        {
            if (request.Transfers == null)
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_METHOD_INVALID);
            var account = await _accountRepository.GetByCustomerId(request.CustomerId.Value);
            if (account == null)
                //tài khoản có tồn tại không?
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_NOTFOUND);

            var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(request.PaymentRequestCode);
            var resultPaymentRequest = await _paymentRequestRepository.GetByPaymentRequestCode(request.PaymentRequestCode, paymentRequestDate);

            if (resultPaymentRequest == null)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_NOTFOUND).WithData("PaymentRequestCode", request.PaymentRequestCode);
            if (resultPaymentRequest.Status == EnmPaymentRequestStatus.Complete)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_COMPLETE_CANCEL).WithData("PaymentRequestCode", request.PaymentRequestCode);

            var requestTransaction = new InsertTransactionInputDtoV2
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
            var paymentCashBackRequest = new CashbackDepositRefundBaseDtoV2
            {
                PaymentRequestCode = resultPaymentRequest.PaymentRequestCode,
                TotalPayment = resultPaymentRequest.TotalPayment,
                Transaction = requestTransaction
            };
            //insert transaction
            var createTransactionOutput = new CreatePaymentTransactionOutputDtoV2();

            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    var extAc = await CreateCashbackTransaction(paymentCashBackRequest, account);

                    requestTransaction.PaymentMethodId = EnmPaymentMethod.Transfer;
                    requestTransaction.TransactionTypeId = EnmTransactionType.WithdrawDeposit;
                    var insertOutput = await _transactionServiceV2.InsertTransaction(requestTransaction);
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
                    var paymentTransEto = ObjectMapper.Map<CreatePaymentTransactionOutputDtoV2, WithdrawDepositCompletedOutputEto>(createTransactionOutput);
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

                    //cap nhat trang thai cua Payment
                    var inputFinish = new VerifyTDTSInputDto
                    {
                        PaymentRequestCode = request.PaymentRequestCode,
                        Total = request.Transfers.Amount,
                        PaymentCode = resultPaymentRequest.PaymentCode,

                        Transaction = new CreatePaymentTransactionInputDtoV2
                        {
                            AccountId = (Guid)account.Id,
                            PaymentRequestCode = request.PaymentRequestCode,
                            Transaction = new PaymentTransactionDtoV2
                            {
                                ShopCode = request.ShopCode,
                                TransactionFee = 0,
                                TransactionTime = DateTime.Now,
                                Note = Decription.PaymentTransaction,
                                CreatedBy = request.Transfers.CreatedBy,
                            }
                        }
                    };
                    var result = await _depositServiceV2.FinishTSTD(inputFinish);

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

        public async Task<Account> CreateCashbackTransaction(CashbackDepositRefundBaseDtoV2 cashbackDto, Account account)
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
                        var transactionOutput = await _transactionServiceV2.InsertTransaction(transactionInput);
                        //update account
                        account.CurrentBalance = account.CurrentBalance + cashbackDto.TotalPayment;
                        await _accountRepository.Update(account);

                        //Bắn kafka
                        var cashbackEto = new CashbackCreatedEto();
                        var transactionEto = ObjectMapper.Map<TransactionFullOutputDtoV2, TransactionFullOutputEto>(transactionOutput);
                        cashbackEto.Transaction = transactionEto;
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
    }
}
