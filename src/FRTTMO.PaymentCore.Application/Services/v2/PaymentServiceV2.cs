using Amazon.S3;
using Amazon.S3.Model;
using FRTTMO.PaymentCore.Dto.v2;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Exceptions;
using FRTTMO.PaymentCore.Kafka.Eto;
using FRTTMO.PaymentCore.Kafka.Interface;
using FRTTMO.PaymentCore.Options;
using FRTTMO.PaymentCore.Repositories;
using FRTTMO.PaymentIntegration.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Uow;
using static FRTTMO.PaymentCore.Common.EnumType;
using Newtonsoft.Json;
using FRTTMO.PaymentCore.Dto.TransactionDetailDto;
using Microsoft.Extensions.Logging;

namespace FRTTMO.PaymentCore.Services.v2
{
    public class PaymentServiceV2 : PaymentCoreAppService, ITransientDependency, IPaymentServiceV2
    {
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITransactionServiceV2 _transactionServiceV2;
        private readonly IEWalletDepositService _eWalletDepositService;
        private readonly ICardService _cardService;
        private readonly IVoucherService _voucherService;
        private readonly IAccountRepository _accountRepository;
        private readonly IPublishService<BaseETO> _iPublishService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IGenerateServiceV2 _generateServiceV2;
        private readonly AWSOptions _configAws;
        private readonly IAmazonS3 _awsS3;
        private readonly ICODService _codService;
        private readonly ITransferService _transferService;
        private readonly PaymentOptions _paymentOptions;
        private readonly IQRHistoryService _qrHistoryService;
        private readonly IEWalletsAppService _eWalletsAppService;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly FRTTMO.PaymentIntegration.Services.IElasticSearchService _elasticSearchIntergationService;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly ILogger<DepositServiceV2> _log;



        public PaymentServiceV2(
            IPaymentRequestRepository paymentRequestRepository,
            ITransactionServiceV2 transactionServiceV2,
            IEWalletDepositService eWalletDepositService,
            ICardService cardService,
            IVoucherService voucherService,
            IAccountRepository accountRepository,
            IPublishService<BaseETO> iPublishService,
            IUnitOfWorkManager unitOfWorkManager,
            IGenerateServiceV2 generateServiceV2,
            IOptions<AWSOptions> configAws,
            IAmazonS3 awsS3,
            ICODService codService,
            ITransferService transferService,
            IOptions<PaymentOptions> paymentOptions,
            IQRHistoryService qrHistoryService,
            IEWalletsAppService eWalletsAppService,
            IPaymentRepository paymentRepository,
            FRTTMO.PaymentIntegration.Services.IElasticSearchService elasticSearchIntergationService,
            IPaymentTransactionRepository paymentTransactionRepository,
        ILogger<DepositServiceV2> log
,
        ElasticSearchService elasticSearchService
        ) : base()
        {
            _paymentRequestRepository = paymentRequestRepository;
            _transactionServiceV2 = transactionServiceV2;
            _eWalletDepositService = eWalletDepositService;
            _cardService = cardService;
            _voucherService = voucherService;
            _accountRepository = accountRepository;
            _iPublishService = iPublishService;
            _unitOfWorkManager = unitOfWorkManager;
            _generateServiceV2 = generateServiceV2;
            _configAws = configAws.Value;
            _awsS3 = awsS3;
            _codService = codService;
            _transferService = transferService;
            _paymentOptions = paymentOptions.Value;
            _qrHistoryService = qrHistoryService;
            _eWalletsAppService = eWalletsAppService;
            _paymentRepository = paymentRepository;
            _elasticSearchIntergationService = elasticSearchIntergationService;
            _paymentTransactionRepository = paymentTransactionRepository;
            _elasticSearchService = elasticSearchService;
            _log = log;
        }

        public async Task<bool> CheckExists(string paymentRequestCode)
        {
            var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(paymentRequestCode);
            return await _paymentRequestRepository.CheckExists(paymentRequestCode, paymentRequestDate);
        }

        public async Task<CreatePaymentTransactionOutputDtoV2> CreatePaymentRequest(CreatePaymentTransactionInputDtoV2 inputDto)
        {
            var createPaymentRequestOutput = new CreatePaymentTransactionOutputDtoV2();

            var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(inputDto.PaymentRequestCode);
            var paymentRequest = await _paymentRequestRepository.GetByPaymentRequestCode(inputDto.PaymentRequestCode, paymentRequestDate);

            if (paymentRequest == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                    .WithData("Data", "Payment request {0}", inputDto.PaymentRequestCode);
            //
            if (paymentRequest.Status == EnmPaymentRequestStatus.Complete)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYEMNT_REQUEST_COMPLETED)
                    .WithData("PaymentRequestCode", inputDto.PaymentRequestCode);

            if (paymentRequest.Status == EnmPaymentRequestStatus.Cancel)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYEMNT_REQUEST_CANCEL)
                    .WithData("PaymentRequestCode", inputDto.PaymentRequestCode);
            //
            var extAc = await _accountRepository.GetById((Guid)inputDto.AccountId);
            if (extAc == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ACCOUNT_NOTFOUND).WithData("AccountId", inputDto.AccountId);
            //Check SUM(TD)-SUM(TS)
            var sumTrans = await _transactionServiceV2.GetSumAmountOfPaymentRequest(
                paymentRequest.PaymentRequestCode,
                new List<EnmTransactionType>
                {
                        EnmTransactionType.Recharge,
                        EnmTransactionType.FirstDeposit
                },
                paymentRequestDate
            );
            var totalDeviant = sumTrans - paymentRequest.TotalPayment;
            var hasDepositTransfer = await _transactionServiceV2.HasTransferDepositNotIsConfirmTrans(paymentRequest.PaymentRequestCode, paymentRequestDate);
            if (hasDepositTransfer)
            {
                var conditionWrongDeviant = Math.Abs(totalDeviant.Value) > _paymentOptions.TotalPaymentMaxDeviant;
                if (conditionWrongDeviant)
                {
                    var eto = ObjectMapper.Map<CreatePaymentTransactionInputDtoV2, PaymentRequestFailedOutputEto>(inputDto);
                    eto.Transaction.Amount = paymentRequest.TotalPayment;
                    eto.Transaction.TransactionTypeId = EnmTransactionType.CollectMoney;
                    eto.Transaction.Status = EnmTransactionStatus.Created;
                    await _iPublishService.ProduceAsync(eto);
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_TRANSACTIONSELL_MONEY)
                        .WithData("TotalPaymentMaxDeviant", _paymentOptions.TotalPaymentMaxDeviant);
                }
            }
            else
            {
                if (paymentRequest.TotalPayment > extAc.CurrentBalance) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTENOUGH).WithData("AccountId", inputDto.AccountId);

                if (totalDeviant < 0)
                {
                    var eto = ObjectMapper.Map<CreatePaymentTransactionInputDtoV2, PaymentRequestFailedOutputEto>(inputDto);
                    eto.Transaction.TransactionTypeId = EnmTransactionType.CollectMoney;
                    eto.Transaction.Status = EnmTransactionStatus.Created;
                    await _iPublishService.ProduceAsync(eto);
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_CHECKSUM_TD_TS_NOTMATCH).WithData("PaymentRequest", paymentRequest.PaymentRequestCode);
                }
            }
            var depositTrans = await _transactionServiceV2.GetByPaymentRequestInfo(new GetByPaymentRequestInfoInputV2
            {
                PaymentRequestCode = paymentRequest.PaymentRequestCode,
                TransactionTypeIds = new List<EnmTransactionType> {
                            EnmTransactionType.Recharge,
                            EnmTransactionType.FirstDeposit,
                    },
                PaymentRequestDate = paymentRequest.PaymentRequestDate
            });
            var paymentRequestCompletedOutputDto = new PaymentRequestCompletedOutputDtoV2
            {
                OrderCode = inputDto.OrderCode,
                PaymentCode = inputDto.PaymentCode,
                PaymentRequestCode = inputDto.PaymentRequestCode,
                DepositTransactions = depositTrans
            };
            //insert transaction
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    var insertInput = ObjectMapper.Map<PaymentTransactionDtoV2, InsertTransactionInputDtoV2>(inputDto.Transaction);
                    insertInput.PaymentRequestId = paymentRequest.Id;
                    insertInput.PaymentRequestDate = paymentRequest.PaymentRequestDate;
                    insertInput.PaymentRequestCode = paymentRequest.PaymentRequestCode;
                    insertInput.Amount = paymentRequest.TotalPayment;
                    insertInput.AccountId = inputDto.AccountId;
                    //insertInput.PaymentRequestId = inputDto.PaymentRequestId;
                    insertInput.TransactionTypeId = EnmTransactionType.CollectMoney;
                    insertInput.Status = EnmTransactionStatus.Created;
                    var insertOutput = await _transactionServiceV2.InsertTransaction(insertInput);
                    createPaymentRequestOutput.Transaction = insertOutput;
                    //
                    extAc.CurrentBalance = extAc.CurrentBalance - sumTrans;
                    await _accountRepository.Update(extAc);
                    //
                    paymentRequest.Status = EnmPaymentRequestStatus.Complete;
                    await _paymentRequestRepository.Update(paymentRequest);
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
            paymentRequestCompletedOutputDto.Transaction = createPaymentRequestOutput.Transaction;
            //publish kafka message
            var paymentTransEto = ObjectMapper.Map<CreatePaymentTransactionOutputDtoV2, PaymentTransactionCompletedOutputEto>(createPaymentRequestOutput);
            paymentTransEto.OrderCode = inputDto.OrderCode;
            await _iPublishService.ProduceAsync(paymentTransEto);
            //
            if (paymentRequest.TypePayment == EmPaymentRequestType.PaymentDebitRequest)
            {
                var orderCode = await _paymentTransactionRepository.Get(paymentRequest.PaymentCode);
                var sourceCode = orderCode != null ? orderCode.SourceCode : "";
                var payRQDebbitEto = ObjectMapper.Map<PaymentRequestCompletedOutputDtoV2, PaymentRequestCompletedDebbitOutputEto>(paymentRequestCompletedOutputDto);
                payRQDebbitEto.OrderCode = sourceCode;
                await _iPublishService.ProduceAsync(payRQDebbitEto);

                var paymentRequestArEto = ObjectMapper.Map<PaymentRequestCompletedOutputDtoV2, PaymentRequestCompletedAROutputEto>(paymentRequestCompletedOutputDto);
                paymentRequestArEto.OrderCode = sourceCode;
                var paymentRequestArDetail = await MapDetailTransactionRecharge(paymentRequestCompletedOutputDto.DepositTransactions);
                paymentRequestArEto.DepositTransactions = ObjectMapper.Map<List<PaymentInfoDetailDtoV2>, List<PaymentInfoDetailEto>>(paymentRequestArDetail);
                await _iPublishService.ProduceAsync(paymentRequestArEto);
            }
            //else
            //{
            //    var paymentRequestEto = ObjectMapper.Map<PaymentRequestCompletedOutputDtoV2, PaymentRequestCompletedOutputEto>(paymentRequestCompletedOutputDto);
            //    await _iPublishService.ProduceAsync(paymentRequestEto);
            //}
            return createPaymentRequestOutput;
        }

        public async Task<CreatePaymentTransactionOutputDtoV2> CreateWithdrawDepositCash(CreateWithdrawDepositInputDtoV2 inputDto)
        {
            var createPaymentRequestOutput = new CreatePaymentTransactionOutputDtoV2();
            if (string.IsNullOrEmpty(inputDto.OrderCode))
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", "OrderCode");

            var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(inputDto.Transaction.PaymentRequestCode);
            var paymentRequest = await _paymentRequestRepository.GetByPaymentRequestCode(inputDto.Transaction.PaymentRequestCode, paymentRequestDate);
            if (paymentRequest == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                    .WithData("Data", "Payment request {0}", inputDto.Transaction.PaymentRequestCode);
            //
            if (paymentRequest.Status == EnmPaymentRequestStatus.Complete)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYEMNT_REQUEST_COMPLETED)
                    .WithData("PaymentRequestCode", paymentRequest.PaymentRequestCode);

            if (paymentRequest.Status == EnmPaymentRequestStatus.Cancel)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYEMNT_REQUEST_CANCEL)
                    .WithData("PaymentRequestCode", paymentRequest.PaymentRequestCode);
            //
            var extAc = await _accountRepository.GetById(inputDto.AccountId.Value);
            if (extAc == null)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ACCOUNT_NOTFOUND).WithData("AccountId", inputDto.AccountId.Value);
            //Check SUM(TD)-SUM(TS)

            if (inputDto.Transaction.Amount > extAc.CurrentBalance.Value)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTENOUGH).WithData("AccountId", inputDto.AccountId.Value);

            var lstType = new List<EnmTransactionType>
                {
                    EnmTransactionType.Recharge,
                    EnmTransactionType.FirstDeposit
                };
            //paymentRequest.Id, lstType, paymentRequest.PaymentRequestDate
            var depositTransactions = await _transactionServiceV2.GetByPaymentRequestInfo(new GetByPaymentRequestInfoInputV2
            {
                PaymentRequestCode = paymentRequest.PaymentRequestCode,
                TransactionTypeIds = lstType,
                PaymentRequestDate = paymentRequest.PaymentRequestDate
            });
            var paymentRequestCompletedOutputDto = new PaymentRequestCompletedOutputDtoV2
            {
                OrderCode = inputDto.OrderCode,
                PaymentRequestCode = paymentRequest.PaymentRequestCode,
                DepositTransactions = depositTransactions
            };
            //insert transaction
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    var insertInput = ObjectMapper.Map<PaymentTransactionInputDtoV2, InsertTransactionInputDtoV2>(inputDto.Transaction);
                    insertInput.PaymentRequestId = paymentRequest.Id;
                    insertInput.PaymentRequestDate = paymentRequest.PaymentRequestDate;
                    insertInput.AccountId = inputDto.AccountId;
                    insertInput.TransactionTypeId = EnmTransactionType.WithdrawDeposit;
                    insertInput.Status = EnmTransactionStatus.Created;
                    insertInput.PaymentMethodId = EnmPaymentMethod.Cash;
                    var insertOutput = await _transactionServiceV2.InsertTransaction(insertInput);
                    createPaymentRequestOutput.Transaction = insertOutput;
                    //
                    extAc.CurrentBalance = extAc.CurrentBalance - inputDto.Transaction.Amount;
                    await _accountRepository.Update(extAc);
                    //
                    //paymentRequest.Status = EnmPaymentRequestStatus.Complete;
                    //await _paymentRequestRepository.Update(paymentRequest);
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
            paymentRequestCompletedOutputDto.Transaction = createPaymentRequestOutput.Transaction;
            //publish kafka message
            var paymentTransEto = ObjectMapper.Map<CreatePaymentTransactionOutputDtoV2, WithdrawDepositCompletedOutputEto>(createPaymentRequestOutput);
            paymentTransEto.OrderCode = inputDto.OrderCode;
            await _iPublishService.ProduceAsync(paymentTransEto);
            return createPaymentRequestOutput;
        }

        public async Task<CreateWithdrawDepositTransferOutputDtoV2> CreateWithdrawDepositTransfer(CreateWithdrawDepositTransferInputDtoV2 inputDto)
        {
            var createPaymentRequestOutput = new CreateWithdrawDepositTransferOutputDtoV2();
            if (string.IsNullOrEmpty(inputDto.OrderCode))
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", "OrderCode");

            var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(inputDto.Transaction.PaymentRequestCode);
            var paymentRequest = await _paymentRequestRepository.GetByPaymentRequestCode(inputDto.Transaction.PaymentRequestCode, paymentRequestDate);
            if (paymentRequest == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                    .WithData("Data", "Payment request {0}", inputDto.Transaction.PaymentRequestCode);
            //
            if (paymentRequest.Status == EnmPaymentRequestStatus.Complete)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYEMNT_REQUEST_COMPLETED)
                    .WithData("PaymentRequestCode", paymentRequest.PaymentRequestCode);

            if (paymentRequest.Status == EnmPaymentRequestStatus.Cancel)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYEMNT_REQUEST_CANCEL)
                    .WithData("PaymentRequestCode", paymentRequest.PaymentRequestCode);
            //
            var extAc = await _accountRepository.GetById(inputDto.AccountId.Value);
            if (extAc == null)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ACCOUNT_NOTFOUND).WithData("AccountId", inputDto.AccountId.Value);
            //Check SUM(TD)-SUM(TS)


            if (inputDto.Transaction.Amount > extAc.CurrentBalance.Value)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTENOUGH).WithData("AccountId", inputDto.AccountId.Value);

            List<EnmTransactionType> lstType = new List<EnmTransactionType>();
            lstType.Add(EnmTransactionType.Recharge);
            lstType.Add(EnmTransactionType.FirstDeposit);
            //paymentRequest.Id, lstType, paymentRequest.PaymentRequestDate
            var depositTransactions = await _transactionServiceV2.GetByPaymentRequestInfo(new GetByPaymentRequestInfoInputV2
            {
                PaymentRequestCode = paymentRequest.PaymentRequestCode,
                PaymentRequestDate = paymentRequest.PaymentRequestDate,
                TransactionTypeIds = lstType
            });
            var paymentRequestCompletedOutputDto = new PaymentRequestCompletedOutputDtoV2
            {
                OrderCode = inputDto.OrderCode,
                PaymentRequestCode = paymentRequest.PaymentRequestCode,
                DepositTransactions = depositTransactions
            };
            //insert transaction
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    var insertInput = ObjectMapper.Map<PaymentTransactionInputDtoV2, InsertTransactionInputDtoV2>(inputDto.Transaction);
                    insertInput.PaymentRequestId = paymentRequest.Id;
                    insertInput.PaymentRequestDate = paymentRequest.PaymentRequestDate;
                    insertInput.AccountId = inputDto.AccountId;
                    //insertInput.PaymentRequestId = inputDto.PaymentRequestId;
                    insertInput.TransactionTypeId = EnmTransactionType.WithdrawDeposit;
                    insertInput.Status = EnmTransactionStatus.Created;
                    insertInput.PaymentMethodId = EnmPaymentMethod.Transfer;
                    var transactionWithDetail = new DepositCoresInputDtoV2
                    {
                        OrderCode = inputDto.OrderCode,
                        Transaction = insertInput,
                        Transfers = ObjectMapper.Map<List<PaymentTransferInputDto>, List<TransferInputDto>>(inputDto.Transfers)
                    };
                    var insertOutput = await _transactionServiceV2.InsertTransactionWithDetail(transactionWithDetail, false);
                    //CreateWithdrawDepositTransferOutputDto
                    createPaymentRequestOutput.Transaction = insertOutput.Transaction;
                    createPaymentRequestOutput.Transfers = insertOutput.Transfers;
                    //
                    extAc.CurrentBalance = extAc.CurrentBalance - inputDto.Transaction.Amount;
                    await _accountRepository.Update(extAc);
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
            paymentRequestCompletedOutputDto.Transaction = createPaymentRequestOutput.Transaction;
            //publish kafka message
            var paymentTransEto = ObjectMapper.Map<CreateWithdrawDepositTransferOutputDtoV2, WithdrawDepositCompletedOutputEto>(createPaymentRequestOutput);
            paymentTransEto.OrderCode = inputDto.OrderCode;
            await _iPublishService.ProduceAsync(paymentTransEto);

            return createPaymentRequestOutput;
        }

        public async Task<PaymentRequestFullOutputDtoV2> GetByPaymentRequestCode(string paymentRequestCode)
        {
            var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(paymentRequestCode);
            var item = await _paymentRequestRepository.GetByPaymentRequestCode(paymentRequestCode, paymentRequestDate);
            return ObjectMapper.Map<PaymentRequest, PaymentRequestFullOutputDtoV2>(item);
        }

        private static Dictionary<PaymentIntegration.Common.EnumType.EnmPartnerId, PaymentIntegration.Common.EnumType.EnmWalletProvider> MapEWalletProvider = new()
        {
            { PaymentIntegration.Common.EnumType.EnmPartnerId.VNpay, PaymentIntegration.Common.EnumType.EnmWalletProvider.VNPAY },
            { PaymentIntegration.Common.EnumType.EnmPartnerId.Smartpay, PaymentIntegration.Common.EnumType.EnmWalletProvider.Smartpay },
            { PaymentIntegration.Common.EnumType.EnmPartnerId.ShopeePay, PaymentIntegration.Common.EnumType.EnmWalletProvider.ShopeePay },
            { PaymentIntegration.Common.EnumType.EnmPartnerId.Alepay, PaymentIntegration.Common.EnumType.EnmWalletProvider.Alepay },
            { PaymentIntegration.Common.EnumType.EnmPartnerId.Zalopay, PaymentIntegration.Common.EnumType.EnmWalletProvider.Zalopay },
            { PaymentIntegration.Common.EnumType.EnmPartnerId.VPB, PaymentIntegration.Common.EnumType.EnmWalletProvider.VPB },
            { PaymentIntegration.Common.EnumType.EnmPartnerId.Foxpay, PaymentIntegration.Common.EnumType.EnmWalletProvider.Foxpay },
        };
        public async Task<PaymentDepositInfoOutputDtoV2> GetPaymentDepositInfoByPaymentRequest(PaymentDepositInfoInputDtoV2 inputDto)
        {
            if (inputDto.PaymentRequestCode == null)
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_ID_INVALID);
            var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(inputDto.PaymentRequestCode);
            var paymentRq = await _paymentRequestRepository.GetByPaymentRequestCode(inputDto.PaymentRequestCode, paymentRequestDate);
            if (paymentRq == null)
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_ID_INVALID);
            var outputDto = ObjectMapper.Map<PaymentRequest, PaymentDepositInfoOutputDtoV2>(paymentRq);

            var trans = await _transactionServiceV2.GetByPaymentRequestCode(inputDto.PaymentRequestCode, paymentRq.PaymentRequestDate);
            var paymentTrans = trans.Where(x => x.TransactionTypeId == EnmTransactionType.CollectMoney).FirstOrDefault();
            var rechargeTrans = trans.Where(x =>
                x.TransactionTypeId == EnmTransactionType.Recharge
                || x.TransactionTypeId == EnmTransactionType.FirstDeposit
            ).ToList();
            var sumRechargeAmount = rechargeTrans.Sum(x => x.Amount);
            outputDto.RemainingAmount = (sumRechargeAmount <= paymentRq.TotalPayment) ? (paymentRq.TotalPayment - sumRechargeAmount) : 0;
            outputDto.Detail.PaymentTransaction = paymentTrans;
            outputDto.Detail.DepositTransactions = await MapDetailTransactionRecharge(rechargeTrans);

            var listQrHistoryInterg = await _qrHistoryService.GetByPaymentRequestCode(paymentRq.PaymentRequestCode);
            outputDto.Detail.QrHistory = new List<QRHistoryDetailDto>();
            foreach (var item in listQrHistoryInterg)
            {
                var detail = new QRHistoryDetailDto
                {
                    CreatedDate = item.CreatedDate,
                    PartnerId = (EnmPartnerId)item.PartnerId,
                    PartnerName = item.MethodName,
                    Response = item.Response,
                    QrCode = item.QrCode,
                    OrgAmount = item.OrgAmount,
                    TxnCode = item.TxnCode,
                };

                var inputrq = new PaymentIntegration.Dto.CheckTransactionEwalletInputDto
                {
                    PayDate = item.CreatedDate.Value,
                    PaymentRequestCode = item.PaymentRequestCode,
                    ShopCode = item.ShopCode,
                    Amount = Convert.ToInt64(item.OrgAmount ?? 0),
                    TransactionType = item.PartnerId == PaymentIntegration.Common.EnumType.EnmPartnerId.ShopeePay ? (uint)PaymentIntegration.Dto.ShopeePay.ShopeePayTransactionType.Payment : default,
                    VirtualAccNo = item.PartnerId == PaymentIntegration.Common.EnumType.EnmPartnerId.VPB ? item.TxnCode : null,
                    ProviderId = (item.PartnerId.HasValue && MapEWalletProvider.ContainsKey(item.PartnerId.Value)) ? MapEWalletProvider[item.PartnerId.Value] : default,
                };

                try
                {
                    var res = await _eWalletsAppService.CheckTransactionPayed(inputrq);
                    detail.IsPayed = res.IsPayed;
                    detail.DebitAmount = res.DebitAmount;
                    detail.RealAmount = res.RealAmount;
                }
                catch (AbpRemoteCallException ex)
                {
                    detail.IsPayed = false;
                    detail.Message = ex.Message;
                }
                outputDto.Detail.QrHistory.Add(detail);
            }
            return outputDto;
        }

        public async Task<PaymentInfoOutputDtoV2> GetPaymentInfoByPaymentRequest(PaymentInfoInputDtoV2 paymentInfoInput)
        {
            if (paymentInfoInput.TransactionTypeId != EnmTransactionType.CollectMoney
                && paymentInfoInput.TransactionTypeId != EnmTransactionType.Pay)
            {
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYEMNT_METHOD_NOT_COLLECT_MONNEY)
                    .WithData("TransactionTypeId", paymentInfoInput.TransactionTypeId);
            }
            var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(paymentInfoInput.PaymentRequestCode);
            var paymentRq = await _paymentRequestRepository.GetByPaymentRequestCode(paymentInfoInput.PaymentRequestCode, paymentRequestDate);
            if (paymentRq == null)
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_ID_INVALID);

            var transPayment = await _transactionServiceV2.GetByPaymentRequestInfo(
                new GetByPaymentRequestInfoInputV2
                {
                    TransactionTypeIds = new List<EnmTransactionType> { paymentInfoInput.TransactionTypeId },
                    PaymentRequestCode = paymentRq.PaymentRequestCode,
                    PaymentRequestDate = paymentRq.PaymentRequestDate
                }
            );
            if (transPayment == null || transPayment.Count == 0)
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYMENTREQUEST_DONT_SELL).WithData("PaymentRequestId", $"PaymentRequest {paymentInfoInput.PaymentRequestCode}");

            var transRecharge = await _transactionServiceV2.GetByPaymentRequestInfo(new GetByPaymentRequestInfoInputV2
            {
                PaymentRequestCode = paymentRq.PaymentRequestCode,
                TransactionTypeIds = new List<EnmTransactionType>
                    {
                        EnmTransactionType.Recharge,
                        EnmTransactionType.FirstDeposit
                    },
                PaymentRequestDate = paymentRq.PaymentRequestDate
            });
            var transRechargeOutputDetail = await MapDetailTransactionRecharge(transRecharge);

            return new PaymentInfoOutputDtoV2
            {
                AccountId = transPayment[0].AccountId,
                PaymentRequestId = transPayment[0].PaymentRequestId,
                TransactionTypeId = transPayment[0].TransactionTypeId,
                Amount = transPayment[0].Amount,
                ShopCode = transPayment[0].ShopCode,
                PaymentRequestStatus = paymentRq.Status,
                Detail = transRechargeOutputDetail
            };
        }

        private async Task<List<PaymentInfoDetailDtoV2>> MapDetailTransactionRecharge(List<TransactionFullOutputDtoV2> transRecharge)
        {
            var transRechargeOutputDetail = ObjectMapper.Map<List<TransactionFullOutputDtoV2>, List<PaymentInfoDetailDtoV2>>(transRecharge);

            var transRechargeListIds = transRechargeOutputDetail.Select(tr => tr.Id).ToList();

            var eWalletDtos = await _eWalletDepositService.GetByTransactionIds(transRechargeListIds);
            var cardDtos = await _cardService.GetByTransactionIds(transRechargeListIds);
            var voucherDtos = await _voucherService.GetByTransactionIds(transRechargeListIds);
            var codDtos = await _codService.GetByTransactionIds(transRechargeListIds);
            var transferDtos = await _transferService.GetByTransactionIds(transRechargeListIds);

            foreach (var item in transRechargeOutputDetail)
            {
                var detail = new TransactionFullOutputDetailDto();

                if (
                    item.PaymentMethodId == EnmPaymentMethod.Wallet
                    || item.PaymentMethodId == EnmPaymentMethod.VNPayGateway
                    || item.PaymentMethodId == EnmPaymentMethod.AlepayGateway
                    || item.PaymentMethodId == EnmPaymentMethod.MocaEWallet
                )
                {
                    detail.EWallets = eWalletDtos.Where(e => e.TransactionId == item.Id).ToList();
                }
                else if (item.PaymentMethodId == EnmPaymentMethod.Card)
                {
                    detail.Cards = cardDtos.Where(e => e.TransactionId == item.Id).ToList();
                }
                else if (item.PaymentMethodId == EnmPaymentMethod.Voucher)
                {
                    detail.Vouchers = voucherDtos.Where(e => e.TransactionId == item.Id).ToList();
                }
                else if (item.PaymentMethodId == EnmPaymentMethod.COD)
                {
                    detail.CODs = codDtos.Where(e => e.TransactionId == item.Id).ToList();
                }
                else if (item.PaymentMethodId == EnmPaymentMethod.Transfer)
                {
                    var transfers = transferDtos.Where(e => e.TransactionId == item.Id).ToList();
                    foreach (var transfer in transfers)
                    {
                        if (!string.IsNullOrEmpty(transfer.Image))
                        {
                            var privateUrl = transfer.Image.Split('/');
                            if (privateUrl.Length == 2 || privateUrl[0] == _configAws.BucketName)
                            {
                                var imgUrlPublic = GetPublicUrlImageS3(privateUrl[1]);
                                transfer.Image = imgUrlPublic;
                            }
                            else
                            {
                                transfer.Image = null;
                            }
                        }
                    }
                    detail.Transfers = transfers;
                }
                item.Detail = detail;
            }
            return transRechargeOutputDetail;
        }
        private string GetPublicUrlImageS3(string keyName)
        {
            return _awsS3.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _configAws.BucketName,
                Key = keyName,
                Expires = DateTime.UtcNow.AddMinutes(_configAws.ExpiresMinutes)
            });
        }

        public async Task<CreatePaymentOutputDto> CreatePayment(CreatePaymentInputDto inputDto)
        {
            //bằng 0 vẫn cho tạo đơn 0 đồng
            var paymentVersion = 1;
            var inputPayment = new Payment
            {
                PaymentCode = inputDto.PaymentCode,
                Total = inputDto.Amount,
                Type = inputDto.Type,
                PaymentVersion = (byte)paymentVersion,
                Status = EnmPaymentStatus.Created,
                CreatedBy = inputDto.CreatedBy,
            };
            if (!string.IsNullOrEmpty(inputDto.PaymentCode))
            {
                //adjust payment


                var paymentDate = _generateServiceV2.GetPaymentRequestDate(inputDto.PaymentCode);
                var payment = await _paymentRepository.Get(inputDto.PaymentCode, paymentDate);
                if (payment != null)
                {
                    if (payment.Status == EnmPaymentStatus.Complete)
                        throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_FINISHED).WithData("PaymentCode", inputDto.PaymentCode);


                    if ((double)payment.Total > (double)inputDto.Amount)
                    {// nếu adjust ít tiền hơn bàn đâu
                     //nếu chưa deposit và chưa tồn tại qr history thì sẽ cho thay đổi 
                     //chưa deposit 
                        var isDeposit = await _paymentRequestRepository.GetListByPaymentCode(payment.PaymentCode);
                        // nếu trong những paymentRequest có tồn tại 1 cái hoàn tất thì k cho adjust
                        if (isDeposit.Any(x => x.Status == EnmPaymentRequestStatus.Complete))
                        {
                            throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_NOT_COLLECT_MONNEY);
                        }

                        //nếu số tiền cần adjust nhỏ hơn tiền đã thanh toán và nhỏ hơn số tiền qrHistory mới không cho adjust
                        var totalDeposit = 0;
                        foreach (var item in isDeposit)
                        {
                            var isTransaction = await _transactionServiceV2.GetByPaymentRequestCode(item.PaymentRequestCode, item.PaymentRequestDate);
                            if (isTransaction != null && isTransaction.Count > 0)
                            {
                                totalDeposit += (int)isTransaction.Sum(x => x.Amount);
                            }
                        }
                        var qrhisoty = await _elasticSearchIntergationService.SearchESHistory(payment.PaymentCode);

                        if ((double)inputDto.Amount < totalDeposit)
                        {
                            throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_NOT_COLLECT_MONNEY);
                        }

                        if ((qrhisoty != null && qrhisoty.QrDetail != null && qrhisoty.QrDetail.Count > 0 && qrhisoty.PaymentCode != null) &&
                            ((double)inputDto.Amount < (double)qrhisoty.QrDetail.Sum(x => x.OrgAmount))
                            )
                        {
                            throw new BusinessException(PaymentCoreErrorCodes.ERROR_QR_EXITS);
                        }
                    }

                    payment.Status = EnmPaymentStatus.Cancel;
                    payment.ModifiedBy = inputDto.CreatedBy;
                    await _paymentRepository.UpdateAsync(payment);

                    inputPayment.PaymentCode = payment.PaymentCode;
                    inputPayment.PaymentDate = payment.PaymentDate;
                    inputPayment.PaymentVersion = (byte)(payment.PaymentVersion + 1);
                };
            }
            else
            {
                var paymentRqGen = _generateServiceV2.GeneratePaymentRequestCode("");
                inputPayment.PaymentCode = paymentRqGen.PaymentRequestCode;
                inputPayment.PaymentDate = paymentRqGen.PaymentRequestDate;
            }


            var outPutPayment = await _paymentRepository.Insert(inputPayment);
            var paymentRequestEto = ObjectMapper.Map<Payment, PaymentCreatedEto>(outPutPayment);

            //bắn kafka
            await _iPublishService.ProduceAsync(paymentRequestEto);
            // insert PaymentSource 
            if (inputDto.PaymentSourceType != null && !string.IsNullOrEmpty(inputDto.SourceCode))
            {
                if (inputDto.Type == EnmPaymentType.Chi && outPutPayment != null &&
                    (inputDto.PaymentSourceType == EnmPaymentSourceCode.RT || inputDto.PaymentSourceType == EnmPaymentSourceCode.ReturnCancelDeposit))
                {
                    var sourceCode = inputDto.SourceCode;
                    var insertInput = new PaymentSource();
                    insertInput.Type = (EnmPaymentSourceCode)inputDto.PaymentSourceType;
                    insertInput.SourceCode = inputDto.SourceCode;
                    insertInput.Amount = inputDto.Amount;
                    insertInput.PaymentId = outPutPayment.Id;
                    insertInput.PaymentVersion = 1;
                    insertInput.Status = EnmPaymentTransactionStatus.Complete;
                    insertInput.PaymentCode = outPutPayment.PaymentCode;
                    var isExists = await _paymentTransactionRepository.IsCheckInfor(insertInput);
                    if (!isExists)
                    {
                        var checksourceCode = (await _paymentTransactionRepository.Insert(insertInput));
                    }
                    try
                    {
                        // sync transfer to ES 
                        var result = new TransactionDetailTransferOutputDto();
                        result.TypePayment = (byte?)outPutPayment.Type;
                        result.PaymentCode = outPutPayment.PaymentCode;
                        result.IsConfirmTransfer = (byte?)EnmTransferIsConfirm.AdvanceTransfer;
                        result.CreatedDate = DateTime.Now;
                        result.StatusFill = (byte?)StatusFill.NotFilled;
                        result.PaymentMethodId = (byte?)EnmPaymentMethod.Transfer;
                        result.PaymentSoureType = (byte?)inputDto.PaymentSourceType;
                        result.AmountPayment = inputDto.Amount;
                        result.CreateDatePayment = DateTime.Now;
                        result.SourceCode = new List<SourceCodeSyncES>() { new SourceCodeSyncES() { SourceCode = sourceCode } };
                        var syncES = await _elasticSearchService.SyncDataESTransfer(outPutPayment.PaymentCode, result);

                        if (syncES == true)
                        {
                            _log.LogInformation(string.Format("SyncTrue: {0}| Request body: {1} ", null, JsonConvert.SerializeObject(result)));
                        }
                        else
                        {
                            _log.LogInformation(string.Format("SyncFail: {0}| Request body: {1} ", null, JsonConvert.SerializeObject(result)));
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"{_CoreName}.CreatePayment: {ex}| Request body: {JsonConvert.SerializeObject(inputDto)} ");
                    }
                }
            }
            return ObjectMapper.Map<Payment, CreatePaymentOutputDto>(outPutPayment);
        }
        public async Task<PaymentRequestFullOutputDtoV2> GetByPaymentCode(string paymentCode)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true, requiresNew: true, isolationLevel: System.Data.IsolationLevel.ReadUncommitted))
            {
                var paymentDate = _generateServiceV2.GetPaymentRequestDate(paymentCode);
                var item = await _paymentRepository.Get(paymentCode, paymentDate);
                return ObjectMapper.Map<Payment, PaymentRequestFullOutputDtoV2>(item);
            }
        }
        public async Task<TransferFullOutputDtoV2> GetTranferByPaymentCode(string paymentCode)
        {
            try
            {
                var paymentDate = _generateServiceV2.GetPaymentRequestDate(paymentCode);
                var paymentRequest = await _paymentRepository.Get(paymentCode, paymentDate);
                if (paymentRequest == null)
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_CODE_NOTFOUND)
                    .WithData("Data", "Payment Request Code {0} ", paymentCode);

                var transactions = await _transactionServiceV2.GetByPaymentRequestCode(paymentRequest.PaymentCode, null);
                if (transactions.Count == 0)
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_NOT_TRANSACTION)
                    .WithData("Data", "Payment Request Code {0}", paymentCode);

                var transaction = transactions.FirstOrDefault(x => x.PaymentMethodId == EnmPaymentMethod.Transfer);
                if (transaction == null)
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_NOT_TRANFER_ACCOUNT_NOTFOUND)
                    .WithData("Data", "Payment Request Code {0}", paymentCode);

                var tranfers = await _transferService.GetByTransactionIds(new List<Guid>() { transaction.Id });
                var resultTranfer = ObjectMapper.Map<List<TransferFullOutputDto>, List<TransferFullOutputDtoV2>>(tranfers);
                return resultTranfer.FirstOrDefault();
            }
            catch (CustomBusinessException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }
        public async Task<OutPutPaymentDtoV2> GetListPaymentCodeByDateTime(InputPaymentDtoV2 input)
        {
            var listPayment = await _paymentRepository.GetListPaymentCodeByDateTime(input.StartDate, input.EndDate);
            var resulListPaymentCode = new OutPutPaymentDtoV2() { PaymentCode = listPayment.Select(x => x.PaymentCode).ToList() };
            return resulListPaymentCode;
        }
        public async Task<bool> InsertPaymentSource(PaymentSourcDto insertInput)
        {
            var payment = await _paymentRepository.Get(insertInput.PaymentCode, _generateServiceV2.GetPaymentRequestDate(insertInput.PaymentCode));
            var dataInsertInput = new PaymentSource();
            bool isExists = true;
            if (payment != null)
            {
                dataInsertInput = ObjectMapper.Map<PaymentSourcDto, PaymentSource>(insertInput);
                dataInsertInput.PaymentId = payment.Id;
                isExists = await _paymentTransactionRepository.IsCheckInfor(dataInsertInput);
                if (!isExists)
                {
                    await _paymentTransactionRepository.Insert(dataInsertInput);
                }
            }
            try
            {
                // sync ES
                var payRQ = await _paymentRequestRepository.GetByPaymentCode(insertInput.PaymentCode);
                if (payment != null && payRQ != null)
                {
                    if (payment != null && payment.Type == EnmPaymentType.Chi &&
                        (payRQ.TypePayment == EmPaymentRequestType.PaymentCoreRequest
                        || payRQ.TypePayment == EmPaymentRequestType.DepositRefund))
                    {
                        var resultSyncES = new TransactionDetailTransferOutputDto();
                        resultSyncES.PaymentCode = insertInput.PaymentCode;
                        resultSyncES.SourceCode = new List<SourceCodeSyncES>()
                        { new SourceCodeSyncES() { SourceCode = dataInsertInput.SourceCode } };
                        var syncES = await _elasticSearchService.SyncDataESTransfer(insertInput.PaymentCode, resultSyncES);

                        if (syncES == true)
                        {
                            _log.LogInformation(string.Format("SyncTrue: {0}| Request body: {1} ", null, JsonConvert.SerializeObject(resultSyncES)));
                        }
                        else
                        {
                            _log.LogInformation(string.Format("SyncFail: {0}| Request body: {1} ", null, JsonConvert.SerializeObject(resultSyncES)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"{InsertPaymentSource}.DepositTransferAll: {ex}| Request body: {JsonConvert.SerializeObject(insertInput)} ");
            }
            if (!isExists)
            {
                return true;
            }
            return false;
        }
        public async Task<TransferUpdateOutDto> UpdateTransfer(TransferUpdateInputDto inputDto)
        {
            try
            {
                var rt = new TransferUpdateOutDto();
                var listSuccess = new List<Success>();
                var listFail = new List<Failed>();
                var FailInfo = new Failed();
                if (inputDto.InPutUpdateTransfer.Count != 0)
                {
                    foreach (var itemTransfer in inputDto.InPutUpdateTransfer)
                    {
                        try
                        {
                            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                            {
                                // get Transfer by transferID
                                var tranferInfo = await _transferService.GetByIds(itemTransfer.TransferId);
                                if (tranferInfo.IsConfirm == EnmTransferIsConfirm.Confirm)
                                {
                                    FailInfo.StatusFail = false;
                                    FailInfo.MessageFail = "PaymentCode" + itemTransfer.PaymentCode + "," + "TransferID" + itemTransfer.TransferId
                                        + "update Chuyển Khoảng không thành công , đơn này đã được chuyển khoản";
                                    listFail.Add(FailInfo);

                                    break;
                                }
                                tranferInfo.IsConfirm = EnmTransferIsConfirm.Confirm;
                                tranferInfo.UserConfirm = inputDto.UserConfirm;
                                // Updated Transfer 
                                var entity = await _transferService.UpdateAsync(ObjectMapper.Map<TransferFullOutputDto, TransferFullInputDto>(tranferInfo));
                                await unitOfWork.SaveChangesAsync();
                                //sync Es
                                var requestES = new TransactionDetailTransferOutputDto();
                                requestES.IsConfirmTransfer = (byte)EnmTransferIsConfirm.Confirm;
                                var transferAll = await _elasticSearchService.GetDocTransferES(itemTransfer.PaymentCode);
                                if (transferAll != null && transferAll != null)
                                {
                                    var traferInfo = new TransferSyncAll();
                                    requestES.TransferAll = transferAll.TransferAll;
                                    requestES.TransferAll.FirstOrDefault(x => x.Id == itemTransfer.TransferId).IsConfirm = (byte)EnmTransferIsConfirm.Confirm;
                                }
                                await _elasticSearchService.SyncDataESTransfer(itemTransfer.PaymentCode, requestES);
                                listSuccess.Add(new Success() { Successful = true, TransferIdSuccessful = itemTransfer.TransferId.ToString() });
                            }
                        }
                        catch (Exception e)
                        {
                            listFail.Add(new Failed()
                            {
                                StatusFail = false,
                                MessageFail = e.Message + "ByTransferId"
                                + itemTransfer.TransferId + "ByOrderCode" + itemTransfer.PaymentCode
                            });
                        }
                    }
                    rt.Success = listSuccess;
                    rt.Failed = listFail;
                }
                return rt;
            }
            catch (Exception ex)
            {
                _log.LogError(string.Format("UpdateTransfer :{0}| listTranferId :{1}| Error :{2}  ", inputDto.InPutUpdateTransfer, ex.Message));
                if (ex is BusinessException || ex is CustomBusinessException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData("Message", $"UpdateTransfer: error - " + ex.Message);
            }
        }
    }
}
