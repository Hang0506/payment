using Amazon.S3;
using Amazon.S3.Model;
using FRTTMO.DebitService.Services;
using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Exceptions;
using FRTTMO.PaymentCore.Kafka.Eto;
using FRTTMO.PaymentCore.Kafka.Interface;
using FRTTMO.PaymentCore.Options;
using FRTTMO.PaymentCore.RemoteAPIs;
using FRTTMO.PaymentCore.Repositories;
using FRTTMO.PaymentCore.Services.v2;
using FRTTMO.PaymentIntegration.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Uow;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services
{
    public class PaymentService : PaymentCoreAppService, ITransientDependency, IPaymentService
    {
        private readonly ILogger<PaymentService> _log;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly ITransactionService _transactionService;
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
        private readonly ITransactionRepository _transactionRepository;
        private readonly IInternalAppServiceCore _internalAppService;
        private readonly ICODRepository _iCODRepository;
        private readonly IAccountingService _accountingService;
        private readonly ITransactionServiceV2 _transactionServiceV2;
        private readonly IPaymentRepository _paymentRepository;
        private readonly FRTTMO.PaymentIntegration.Services.IElasticSearchService _elasticSearchIntergationService;
        public PaymentService(
            ILogger<PaymentService> log,
            IPaymentRequestRepository paymentRequestRepository,
            ITransactionService transactionService,
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
            ITransactionRepository transactionRepository,
            IInternalAppServiceCore internalAppServiceCore,
            ICODRepository cODRepository,
            IAccountingService accountingService,
            ITransactionServiceV2 transactionServiceV2,
            IPaymentRepository paymentRepository,
            FRTTMO.PaymentIntegration.Services.IElasticSearchService elasticSearchIntergationService
        ) : base()
        {
            _log = log;
            _paymentRequestRepository = paymentRequestRepository;
            _transactionService = transactionService;
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
            _transactionRepository = transactionRepository;
            _internalAppService = internalAppServiceCore;
            _iCODRepository = cODRepository;
            _accountingService = accountingService;
            _transactionServiceV2 = transactionServiceV2;
            _paymentRepository = paymentRepository;
            _elasticSearchIntergationService = elasticSearchIntergationService;
        }

        public async Task<PaymentInfoOutputDto> GetPaymentInfoByPaymentRequest(PaymentInfoInputDto paymentInfoInput)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true, requiresNew: true, isolationLevel: System.Data.IsolationLevel.ReadUncommitted))
            {
                if (paymentInfoInput.TransactionTypeId != EnmTransactionType.CollectMoney
                        && paymentInfoInput.TransactionTypeId != EnmTransactionType.Pay)
                {
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYEMNT_METHOD_NOT_COLLECT_MONNEY)
                        .WithData("TransactionTypeId", paymentInfoInput.TransactionTypeId);
                }
                var paymentRq = await _paymentRequestRepository.GetById(paymentInfoInput.PaymentRequestId);
                if (paymentRq == null)
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_ID_INVALID);

                var transPayment = await _transactionService.GetByPaymentRequestInfo(
                    new GetByPaymentRequestInfoInput
                    {
                        transactionTypeIds = new List<EnmTransactionType> { paymentInfoInput.TransactionTypeId },
                        paymentRequestId = paymentRq.Id,
                        paymentRequestDate = paymentRq.PaymentRequestDate
                    }
                );
                if (transPayment == null || transPayment.Count == 0)
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYMENTREQUEST_DONT_SELL).WithData("PaymentRequestId", $"PaymentRequest {paymentInfoInput.PaymentRequestId}");

                var transRecharge = await _transactionRepository.GetByPaymentRequestInfo(
                    paymentRq.Id,
                    new List<EnmTransactionType>
                        {
                        EnmTransactionType.Recharge,
                        EnmTransactionType.FirstDeposit
                        },
                        paymentRq.PaymentRequestDate
                    );
                var transRechargeOutputDetail = await MapDetailTransactionRecharge(ObjectMapper.Map<List<Transaction>, List<TransactionFullOutputDto>>(transRecharge));

                return new PaymentInfoOutputDto
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
        }

        /// <summary>
        /// Tạo payment request
        /// </summary>
        public async Task<PaymentRequestFullOutputDto> CreateRequest(PaymentRequestInsertDto paymentRequestInsert)
        {
            if (string.IsNullOrEmpty(paymentRequestInsert.OrderCode))
            {
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_ORDER_CODE_NOT_EMPTY).WithData("OrderCode", paymentRequestInsert.OrderCode);
            }
            else
            {
                var request = ObjectMapper.Map<PaymentRequestInsertDto, PaymentRequest>(paymentRequestInsert);
                //
                var paymentRqGen = _generateServiceV2.GeneratePaymentRequestCode("");
                request.PaymentRequestCode = paymentRqGen.PaymentRequestCode;
                request.PaymentRequestDate = paymentRqGen.PaymentRequestDate;
                //
                request.Status = EnmPaymentRequestStatus.Confirm;
                var response = await _paymentRequestRepository.InsertObj(request);
                var returnData = ObjectMapper.Map<PaymentRequest, PaymentRequestFullOutputDto>(response);
                var paymentRequestEto = ObjectMapper.Map<PaymentRequest, PaymentRequestCreatedETO>(response);
                await _iPublishService.ProduceAsync(paymentRequestEto);
                return returnData;
            }
        }

        public async Task<bool> CancelRequest(PaymentCancelInputDto PaymentRequestDto)
        {
            var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(PaymentRequestDto.PaymentRequestCode);
            var paymentRequest = await _paymentRequestRepository.GetByPaymentRequestCode(PaymentRequestDto.PaymentRequestCode, paymentRequestDate);
            if (paymentRequest == null || paymentRequest.Status != EnmPaymentRequestStatus.Confirm)
            {
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYMENTREQUEST_CANCELFAILD);
            }
            else
            {
                paymentRequest.Status = EnmPaymentRequestStatus.Cancel;
                await _paymentRequestRepository.UpdateObj(paymentRequest);
                await _elasticSearchIntergationService.QRHistoryRemoveAsync(paymentRequest.PaymentCode, PaymentRequestDto.PaymentRequestCode);
            }
            return true;
        }
        public async Task<CreatePaymentTransactionOutputDto> CreatePaymentRequest(CreatePaymentTransactionInputDto inputDto)
        {
            var createPaymentRequestOutput = new CreatePaymentTransactionOutputDto();
            if (string.IsNullOrEmpty(inputDto.OrderCode)) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", "OrderCode");

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
            var extAc = await _accountRepository.GetById(inputDto.AccountId);
            if (extAc == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ACCOUNT_NOTFOUND).WithData("AccountId", inputDto.AccountId);
            //Check SUM(TD)-SUM(TS)
            var sumTrans = await _transactionService.GetSumAmountOfPaymentRequest(
                paymentRequest.Id,
                new List<EnmTransactionType>
                    {
                        EnmTransactionType.Recharge,
                        EnmTransactionType.FirstDeposit
                    },
                    paymentRequestDate
                );
            var totalDeviant = sumTrans - paymentRequest.TotalPayment;
            var hasDepositTransfer = await _transactionService.HasTransferDepositNotIsConfirmTrans(
                paymentRequest.Id,
                paymentRequestDate,
                new List<EnmTransactionType>
                {
                    EnmTransactionType.Recharge,
                    EnmTransactionType.FirstDeposit
                }
            );
            if (hasDepositTransfer)
            {
                var conditionWrongDeviant = Math.Abs(totalDeviant.Value) > _paymentOptions.TotalPaymentMaxDeviant;
                if (conditionWrongDeviant)
                {
                    var eto = ObjectMapper.Map<CreatePaymentTransactionInputDto, PaymentRequestFailedOutputEto>(inputDto);
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

                if (totalDeviant != 0)
                {
                    var eto = ObjectMapper.Map<CreatePaymentTransactionInputDto, PaymentRequestFailedOutputEto>(inputDto);
                    eto.Transaction.TransactionTypeId = EnmTransactionType.CollectMoney;
                    eto.Transaction.Status = EnmTransactionStatus.Created;
                    await _iPublishService.ProduceAsync(eto);
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_CHECKSUM_TD_TS_NOTMATCH).WithData("PaymentRequest", paymentRequest.Id);
                }
            }
            var paymentRequestCompletedOutputDto = new PaymentRequestCompletedOutputDto
            {
                OrderCode = inputDto.OrderCode,
                PaymentRequestCode = inputDto.PaymentRequestCode,
                OrderReturnId = paymentRequest.OrderReturnId,
                TypePayment = paymentRequest.TypePayment,
                DepositTransactions = await _transactionService.GetByPaymentRequestInfo(
                    new GetByPaymentRequestInfoInput
                    {
                        paymentRequestId = paymentRequest.Id,
                        transactionTypeIds = new List<EnmTransactionType> { EnmTransactionType.Recharge, EnmTransactionType.FirstDeposit },
                        paymentRequestDate = paymentRequest.PaymentRequestDate
                    }
                )
            };
            //insert transaction
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    var insertInput = ObjectMapper.Map<PaymentTransactionInputDto, InsertTransactionInputDto>(inputDto.Transaction);
                    insertInput.PaymentRequestCode = paymentRequest.PaymentRequestCode;
                    insertInput.PaymentRequestDate = paymentRequest.PaymentRequestDate;
                    insertInput.AccountId = inputDto.AccountId;
                    insertInput.TransactionTypeId = EnmTransactionType.CollectMoney;
                    insertInput.Status = EnmTransactionStatus.Created;
                    var insertOutput = await _transactionService.InsertTransaction(insertInput);
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
            var paymentTransEto = ObjectMapper.Map<CreatePaymentTransactionOutputDto, PaymentTransactionCompletedOutputEto>(createPaymentRequestOutput);
            paymentTransEto.OrderCode = inputDto.OrderCode;
            await _iPublishService.ProduceAsync(paymentTransEto);
            //
            if (paymentRequest.TypePayment == EmPaymentRequestType.PaymentDebitRequest)
            {
                var payRQDebbitEto = ObjectMapper.Map<PaymentRequestCompletedOutputDto, PaymentRequestCompletedDebbitOutputEto>(paymentRequestCompletedOutputDto);
                await _iPublishService.ProduceAsync(payRQDebbitEto);

                var paymentRequestArEto = ObjectMapper.Map<PaymentRequestCompletedOutputDto, PaymentRequestCompletedAROutputEto>(paymentRequestCompletedOutputDto);
                var paymentRequestArDetail = await MapDetailTransactionRecharge(paymentRequestCompletedOutputDto.DepositTransactions);
                paymentRequestArEto.DepositTransactions = ObjectMapper.Map<List<PaymentInfoDetailDto>, List<PaymentInfoDetailEto>>(paymentRequestArDetail);
                await _iPublishService.ProduceAsync(paymentRequestArEto);
            }
            else
            {
                var paymentRequestEto = ObjectMapper.Map<PaymentRequestCompletedOutputDto, PaymentRequestCompletedOutputEto>(paymentRequestCompletedOutputDto);
                await _iPublishService.ProduceAsync(paymentRequestEto);
            }
            return createPaymentRequestOutput;
        }

        public async Task<bool> CheckExists(Guid id)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true, requiresNew: true, isolationLevel: System.Data.IsolationLevel.ReadUncommitted))
            {
                return await _paymentRequestRepository.CheckExists(id);
            }
        }

        public async Task<bool> CheckExistsByCode(string paymentRequestCode)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true, requiresNew: true, isolationLevel: System.Data.IsolationLevel.ReadUncommitted))
            {
                var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(paymentRequestCode);
                return await _paymentRequestRepository.CheckExists(paymentRequestCode, paymentRequestDate);
            }
        }

        public async Task<PaymentRequestFullOutputDto> GetById(Guid id)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true, requiresNew: true, isolationLevel: System.Data.IsolationLevel.ReadUncommitted))
            {
                var item = await _paymentRequestRepository.GetById(id);
                return ObjectMapper.Map<PaymentRequest, PaymentRequestFullOutputDto>(item);
            }
        }

        public async Task<PaymentRequestFullOutputDto> GetByPaymentRequestCode(string paymentRequestCode)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true, requiresNew: true, isolationLevel: System.Data.IsolationLevel.ReadUncommitted))
            {
                var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(paymentRequestCode);
                var item = await _paymentRequestRepository.GetByPaymentRequestCode(paymentRequestCode, paymentRequestDate);
                return ObjectMapper.Map<PaymentRequest, PaymentRequestFullOutputDto>(item);
            }
        }

        public async Task<bool> CheckPaymentRequestCodeExists(string paymentRequestCode)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true, requiresNew: true, isolationLevel: System.Data.IsolationLevel.ReadUncommitted))
            {
                var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(paymentRequestCode);
                return await _paymentRequestRepository.CheckExists(paymentRequestCode, paymentRequestDate);
            }
        }

        public async Task<List<PaymentRequestFullOutputDto>> GetListByOrderCode(string orderCode)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true, requiresNew: true, isolationLevel: System.Data.IsolationLevel.ReadUncommitted))
            {
                var list = await _paymentRequestRepository.GetListByOrderCode(orderCode);
                return ObjectMapper.Map<List<PaymentRequest>, List<PaymentRequestFullOutputDto>>(list);
            }
        }

        //public async Task<UploadFileOutputDto> UploadFile(IFormFile file)
        //{
        //    AddLogElapsed("StartFuncUploadFile");
        //    if (!file.FileName.HasExtensionFileAppAccept())
        //        throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_FILE_TYPE_INVALID)
        //            .WithData("FileName", file.FileName);
        //    //10MB
        //    if (file.Length > (_configAws.FileMaxLength * 1024 * 1024))
        //        throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_FILE_LENGTH_INVALID)
        //            .WithData("FileLength", file.Length)
        //            .WithData("MaxLength", _configAws.FileMaxLength);

        //    var newKeyName = $"{Guid.NewGuid()}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        //    var request = new UploadPartRequest
        //    {
        //        BucketName = _configAws.BucketName,
        //        Key = newKeyName,
        //        InputStream = file.OpenReadStream()
        //    };
        //    AddLogElapsed("AWS3 start");
        //    var response = await _awsS3.UploadPartAsync(request);
        //    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        //        throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
        //            .WithData("Message", $"KeyName {newKeyName}: error - AWS UploadPartAsync falied");

        //    var imgUrlPublic = GetPublicUrlImageS3(newKeyName);
        //    AddLogElapsed("AWS3 finish");
        //    return new UploadFileOutputDto
        //    {
        //        KeyName = newKeyName,
        //        PrivateUrl = $"{_configAws.BucketName}/{newKeyName}",
        //        PublicUrl = imgUrlPublic
        //    };
        //}

        private string GetPublicUrlImageS3(string keyName)
        {
            return _awsS3.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _configAws.BucketName,
                Key = keyName,
                Expires = DateTime.UtcNow.AddMinutes(_configAws.ExpiresMinutes)
            });
        }

        public async Task<List<PaymentInfoDetailDto>> MapDetailTransactionRecharge(List<TransactionFullOutputDto> transRecharge)
        {
            var transRechargeOutputDetail = ObjectMapper.Map<List<TransactionFullOutputDto>, List<PaymentInfoDetailDto>>(transRecharge);

            var transRechargeListIds = transRechargeOutputDetail.Select(tr => tr.Id).ToList();

            var eWalletDtos = await _eWalletDepositService.GetByTransactionIds(transRechargeListIds);
            var cardDtos = await _cardService.GetByTransactionIds(transRechargeListIds);
            var voucherDtos = await _voucherService.GetByTransactionIds(transRechargeListIds);
            var codDtos = await _codService.GetByTransactionIds(transRechargeListIds);
            var transferDtos = await _transferService.GetByTransactionIds(transRechargeListIds);

            foreach (var item in transRechargeOutputDetail)
            {
                if (
                    item.PaymentMethodId == EnmPaymentMethod.Wallet
                    || item.PaymentMethodId == EnmPaymentMethod.VNPayGateway
                    || item.PaymentMethodId == EnmPaymentMethod.AlepayGateway
                    || item.PaymentMethodId == EnmPaymentMethod.ZaloPayGateway
                    || item.PaymentMethodId == EnmPaymentMethod.MocaEWallet
                )
                {
                    item.Detail.EWallets = eWalletDtos.Where(e => e.TransactionId == item.Id).ToList();
                }
                else if (item.PaymentMethodId == EnmPaymentMethod.Card)
                {
                    item.Detail.Cards = cardDtos.Where(e => e.TransactionId == item.Id).ToList();
                }
                else if (item.PaymentMethodId == EnmPaymentMethod.Voucher)
                {
                    item.Detail.Vouchers = voucherDtos.Where(e => e.TransactionId == item.Id).ToList();
                }
                else if (item.PaymentMethodId == EnmPaymentMethod.COD)
                {
                    item.Detail.CODs = codDtos.Where(e => e.TransactionId == item.Id).ToList();
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
                    item.Detail.Transfers = transfers;

                }
            }
            return transRechargeOutputDetail;
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
        public async Task<PaymentDepositInfoOutputDto> GetPaymentDepositInfoByPaymentRequest(PaymentDepositInfoInputDto inputDto)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true, requiresNew: true, isolationLevel: System.Data.IsolationLevel.ReadUncommitted))
            {
                if (inputDto.PaymentRequestId == null)
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_ID_INVALID);
                var paymentRq = await _paymentRequestRepository.GetById(inputDto.PaymentRequestId.Value);
                if (paymentRq == null)
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_ID_INVALID);
                var outputDto = ObjectMapper.Map<PaymentRequest, PaymentDepositInfoOutputDto>(paymentRq);

                var trans = await _transactionService.GetByPaymentRequestId(inputDto.PaymentRequestId.Value, paymentRq.PaymentRequestDate);
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
                        TransactionType = (item.PartnerId == PaymentIntegration.Common.EnumType.EnmPartnerId.ShopeePay) ? (uint)PaymentIntegration.Dto.ShopeePay.ShopeePayTransactionType.Payment : default,
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
        }

        /// <summary>
        /// Hàm chi trả tiền đặt cọc - tiền mặt
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        public async Task<CreatePaymentTransactionOutputDto> CreateWithdrawDepositCash(CreateWithdrawDepositInputDto inputDto)
        {

            var createPaymentRequestOutput = new CreatePaymentTransactionOutputDto();
            if (string.IsNullOrEmpty(inputDto.OrderCode))
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", "OrderCode");

            var paymentRequest = await _paymentRequestRepository.GetById(inputDto.Transaction.PaymentRequestId.Value);
            if (paymentRequest == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                    .WithData("Data", "Payment request {0}", inputDto.Transaction.PaymentRequestId.Value);
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
            var depositTransactions = await _transactionRepository.GetByPaymentRequestInfo(paymentRequest.Id, lstType, paymentRequest.PaymentRequestDate);
            var paymentRequestCompletedOutputDto = new PaymentRequestCompletedOutputDto
            {
                OrderCode = inputDto.OrderCode,
                PaymentRequestCode = paymentRequest.PaymentRequestCode,
                DepositTransactions = ObjectMapper.Map<List<Transaction>, List<TransactionFullOutputDto>>(depositTransactions)
            };
            //insert transaction
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    var insertInput = ObjectMapper.Map<PaymentTransactionInputDto, InsertTransactionInputDto>(inputDto.Transaction);
                    insertInput.AccountId = inputDto.AccountId;
                    insertInput.TransactionTypeId = EnmTransactionType.WithdrawDeposit;
                    insertInput.Status = EnmTransactionStatus.Created;
                    insertInput.PaymentMethodId = EnmPaymentMethod.Cash;
                    var insertOutput = await _transactionService.InsertTransaction(insertInput);
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
            var paymentTransEto = ObjectMapper.Map<CreatePaymentTransactionOutputDto, WithdrawDepositCompletedOutputEto>(createPaymentRequestOutput);
            paymentTransEto.OrderCode = inputDto.OrderCode;
            await _iPublishService.ProduceAsync(paymentTransEto);
            return createPaymentRequestOutput;
        }
        /// <summary>
        /// Hàm chi trả tiền đặt cọc - chuyển khoản
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        public async Task<CreateWithdrawDepositTransferOutputDto> CreateWithdrawDepositTransfer(CreateWithdrawDepositTransferInputDto inputDto)
        {

            var createPaymentRequestOutput = new CreateWithdrawDepositTransferOutputDto();
            if (string.IsNullOrEmpty(inputDto.OrderCode))
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", "OrderCode");

            var paymentRequest = await _paymentRequestRepository.GetById(inputDto.Transaction.PaymentRequestId.Value);
            if (paymentRequest == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                    .WithData("Data", "Payment request {0}", inputDto.Transaction.PaymentRequestId.Value);
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
            var depositTransactions = await _transactionRepository.GetByPaymentRequestInfo(paymentRequest.Id, lstType, paymentRequest.PaymentRequestDate);
            var paymentRequestCompletedOutputDto = new PaymentRequestCompletedOutputDto
            {
                OrderCode = inputDto.OrderCode,
                PaymentRequestCode = paymentRequest.PaymentRequestCode,
                DepositTransactions = ObjectMapper.Map<List<Transaction>, List<TransactionFullOutputDto>>(depositTransactions)
            };
            //insert transaction
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    var insertInput = ObjectMapper.Map<PaymentTransactionInputDto, InsertTransactionInputDto>(inputDto.Transaction);
                    insertInput.AccountId = inputDto.AccountId;
                    //insertInput.PaymentRequestId = inputDto.PaymentRequestId;
                    insertInput.TransactionTypeId = EnmTransactionType.WithdrawDeposit;
                    insertInput.Status = EnmTransactionStatus.Created;
                    insertInput.PaymentMethodId = EnmPaymentMethod.Transfer;
                    var transactionWithDetail = new DepositCoresInputDto
                    {
                        OrderCode = inputDto.OrderCode,
                        Transaction = insertInput,
                        Transfers = ObjectMapper.Map<List<PaymentTransferInputDto>, List<TransferInputDto>>(inputDto.Transfers)
                    };
                    var insertOutput = await _transactionService.InsertTransactionWithDetail(transactionWithDetail, false);
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
            var paymentTransEto = ObjectMapper.Map<CreateWithdrawDepositTransferOutputDto, WithdrawDepositCompletedOutputEto>(createPaymentRequestOutput);
            paymentTransEto.OrderCode = inputDto.OrderCode;
            await _iPublishService.ProduceAsync(paymentTransEto);

            return createPaymentRequestOutput;

        }
        public async Task<bool> UpdateCompanyCod(UpdateCompanyInfoInputDto inItem)
        {
            try
            {
                var PaymentRequest = await _paymentRequestRepository.GetToTalBill(inItem.OrderCode, EnmPaymentRequestStatus.Complete);
                if (PaymentRequest == null)
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_ID_INVALID);

                var paymentRequestId = PaymentRequest.Id;
                var transaction = await _transactionRepository.GetTransactionMethod(paymentRequestId, EnmPaymentMethod.COD);
                if (transaction == null) throw new BusinessException(PaymentCoreErrorCodes.ERROR_ORDERCODE_NOTFOUND).WithData("OrderCode", inItem.OrderCode);

                var Cod = await _iCODRepository.GetByTransactionId(transaction.Id);
                if (Cod == null) throw new BusinessException(PaymentCoreErrorCodes.ERROR_ORDERCODE_NOTFOUND).WithData("OrderCode", inItem.OrderCode);

                var requestApi = new UpdateCompanyDebitInputDto()
                {
                    CompanyID = (int)inItem.TransporterCode,
                    OrderCode = inItem.OrderCode
                };
                // call api  updateCompany "​/api/DebitService/debit/compay-info"
                await _internalAppService.InvokeApi<object>(
                     EnvironmentSetting.RemoteDebitService,
                     InternalApiUrl.updateCompany,
                     RestSharp.Method.PUT,
                     JsonConvert.SerializeObject(requestApi)
                 );


                Cod.TransporterID = inItem.TransporterID;
                Cod.TransporterName = inItem.TransporterName;
                Cod.TransporterCode = inItem.TransporterCode;
                await _iCODRepository.UpdateAsync(Cod);
                //Sync to Kafka lc.payment.COD.updated
                UpdateCodCompanyEto updateCod = ObjectMapper.Map<COD, UpdateCodCompanyEto>(Cod);
                updateCod.OrderCode = inItem.OrderCode;
                await _iPublishService.ProduceAsync(updateCod);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(string.Format("UpdateCompanyCod By OrderCode :{0}| TransporterCode :{1}| Error :{2}  ", inItem.OrderCode, inItem.TransporterCode, ex.Message));
                if (ex is BusinessException || ex is CustomBusinessException || ex is AbpRemoteCallException) throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData("Message", $"UpdateCompanyCod: error - " + ex.Message);
            }
        }


        //[CapSubscribe(KafkaTopics.PAYMENT_COMPANY_COD_UPDATED)]
        //public async Task<bool> UpdateCodTransporterCode(UpdateCodCompanyEto inItem)
        //{
        //    using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: false, requiresNew: true, isolationLevel: System.Data.IsolationLevel.ReadUncommitted))
        //    {
        //        try
        //        {

        //            _log.LogInformation(string.Format("UpdateCodTransporterCode By PaymentCode :{0}| TransactionId :{1}| Error :{2}  ", inItem.PaymentCode, inItem.TransporterCode, inItem.TransactionId));

        //            if (inItem.TransactionId == null || inItem.TransactionId == new Guid())
        //                return false;
        //            var Cod = await _iCODRepository.GetByTransactionId(inItem.TransactionId);
        //            if (Cod == null) return false;
        //            Cod.TransporterID = inItem.TransporterID;
        //            Cod.TransporterName = inItem.TransporterName;
        //            Cod.TransporterCode = inItem.TransporterCode;
        //            await _iCODRepository.UpdateAsync(Cod);
        //            await unitOfWork.SaveChangesAsync();
        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            await unitOfWork.RollbackAsync();
        //            _log.LogError(string.Format("UpdateCompanyCod By PaymentCode :{0}| TransporterCode :{1}| Error :{2}  ", inItem.PaymentCode, inItem.TransporterCode, ex.Message));
        //            if (ex is BusinessException || ex is CustomBusinessException || ex is AbpRemoteCallException) throw;
        //            throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
        //                .WithData("Message", $"UpdateCodTransporterCode: error - " + ex.Message);
        //        }
        //    }

        //}



        #region Get PresignUpload s3
        public async Task<GetPresignUploadOutputDto> GetPresignUploadS3(GetPresignUploadInputDto input)
        {
            var keyName = input.KeyName;
            var contentType = input.ContentType;
            if (keyName.IsNullOrEmpty())
            {
                keyName = $"{Guid.NewGuid()}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            }
            if (contentType.IsNullOrEmpty())
            {
                contentType = "image/jpeg";
            }
            var presignUrl = _awsS3.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _configAws.BucketName,
                Key = keyName,
                Expires = DateTime.UtcNow.AddMinutes(_configAws.ExpiresMinutesPresignUrlUpload),
                Verb = HttpVerb.PUT,
                ContentType = contentType
            });
            return new GetPresignUploadOutputDto
            {
                KeyName = keyName,
                ExpiresMinutes = _configAws.ExpiresMinutesPresignUrlUpload,
                PresignUrl = presignUrl,
                PrivateUrl = $"{_configAws.BucketName}/{keyName}"
            };
        }
        #endregion Get PresignUpload s3
        public async Task<TransferFullOutputDto> GetTranferByPaymentRequestCode(string paymentRequestCode)
        {
            try
            {
                var paymentRequest = await _paymentRequestRepository.GetByPaymentRequestCode(paymentRequestCode, null);
                if (paymentRequest == null)
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_CODE_NOTFOUND)
                    .WithData("Data", "Payment Request Code {0} ", paymentRequestCode);

                var transactions = await _transactionService.GetByPaymentRequestId(paymentRequest.Id, null);
                if (transactions.Count == 0)
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_NOT_TRANSACTION)
                    .WithData("Data", "Payment Request Code {0}", paymentRequestCode);

                var transaction = transactions.FirstOrDefault(x => x.PaymentMethodId == EnmPaymentMethod.Transfer);
                if (transaction == null)
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_NOT_TRANFER_ACCOUNT_NOTFOUND)
                    .WithData("Data", "Payment Request Code {0}", paymentRequestCode);

                var tranfers = await _transferService.GetByTransactionIds(new List<Guid>() { transaction.Id });
                return tranfers.FirstOrDefault();

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

        public async Task<TransferFullOutputDto> GetTranferByPaymentRequest(SearchTransferCondi item) => await GetTranferByPaymentRequestCode(item.PaymentRequestCode);
        public async Task<PaymentDepositRequestIdInfoOutputDto> GetPaymentDepositInfoByPaymentRequestId(PaymentDepositInfoInputDto inputDto)
        {
            if (inputDto.PaymentRequestId == null)
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_ID_INVALID);
            var paymentRq = await _paymentRequestRepository.GetById(inputDto.PaymentRequestId.Value);
            if (paymentRq == null)
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_ID_INVALID);
            var outputDto = ObjectMapper.Map<PaymentRequest, PaymentDepositRequestIdInfoOutputDto>(paymentRq);

            var trans = await _transactionService.GetByPaymentRequestId(inputDto.PaymentRequestId.Value, paymentRq.PaymentRequestDate);
            var paymentTrans = trans.Where(x => x.TransactionTypeId == EnmTransactionType.CollectMoney).FirstOrDefault();//paymentTransaction 
            var rechargeTrans = trans.Where(x =>
                x.TransactionTypeId == EnmTransactionType.Recharge
                || x.TransactionTypeId == EnmTransactionType.FirstDeposit
            ).ToList(); //depositTransactions
            var sumRechargeAmount = rechargeTrans.Sum(x => x.Amount);
            outputDto.RemainingAmount = (sumRechargeAmount <= paymentRq.TotalPayment) ? (paymentRq.TotalPayment - sumRechargeAmount) : 0;
            outputDto.Detail.PaymentTransaction = paymentTrans;
            outputDto.Detail.DepositTransactions = await MapDetailTransactionRecharge(rechargeTrans);

            var cashBasks = trans.Where(t => t.TransactionTypeId == EnmTransactionType.Refund ||
                                             t.TransactionTypeId == EnmTransactionType.Pay ||
                                             t.TransactionTypeId == EnmTransactionType.CashBack ||
                                             t.TransactionTypeId == EnmTransactionType.WithdrawDeposit).ToList();

            outputDto.Detail.CashBacks = await MapDetailTransactionRecharge(cashBasks);



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
                    OrgAmount = item.OrgAmount
                };

                var inputrq = new PaymentIntegration.Dto.CheckTransactionEwalletInputDto
                {
                    PayDate = item.CreatedDate.Value,
                    PaymentRequestCode = item.PaymentRequestCode,
                    ShopCode = item.ShopCode,
                    Amount = Convert.ToInt64(item.OrgAmount ?? 0),
                    TransactionType = item.PartnerId == PaymentIntegration.Common.EnumType.EnmPartnerId.ShopeePay ? (uint)PaymentIntegration.Dto.ShopeePay.ShopeePayTransactionType.Payment : default,
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

        public async Task<TransferFullOutputDto> Testnuget(string paymentRequestCode) => await GetTranferByPaymentRequestCode(paymentRequestCode);

        public async Task<PaymentAccountingOutputDto> GetPaymentInfoByPaymentRequestId(PaymentInfoInputDto paymentInfoInput)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true, requiresNew: true, isolationLevel: System.Data.IsolationLevel.ReadUncommitted))
            {
                if (paymentInfoInput.TransactionTypeId != EnmTransactionType.CollectMoney
                        && paymentInfoInput.TransactionTypeId != EnmTransactionType.Pay)
                {
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYEMNT_METHOD_NOT_COLLECT_MONNEY)
                        .WithData("TransactionTypeId", paymentInfoInput.TransactionTypeId);
                }
                var paymentRq = await _paymentRequestRepository.GetById(paymentInfoInput.PaymentRequestId);
                if (paymentRq == null)
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_ID_INVALID);

                var transPayment = await _transactionService.GetByPaymentRequestInfo(
                    new GetByPaymentRequestInfoInput
                    {
                        transactionTypeIds = new List<EnmTransactionType> { paymentInfoInput.TransactionTypeId },
                        paymentRequestId = paymentRq.Id,
                        paymentRequestDate = paymentRq.PaymentRequestDate
                    }
                );
                if (transPayment == null || transPayment.Count == 0)
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYMENTREQUEST_DONT_SELL).WithData("PaymentRequestId", $"PaymentRequest {paymentInfoInput.PaymentRequestId}");

                var transRecharge = await _transactionRepository.GetByPaymentRequestInfo(
                    paymentRq.Id,
                    new List<EnmTransactionType>
                        {
                        EnmTransactionType.Recharge,
                        EnmTransactionType.FirstDeposit
                        },
                        paymentRq.PaymentRequestDate
                    );
                var transRechargeOutputDetail = await MapDetailTransactionRecharge(ObjectMapper.Map<List<Transaction>, List<TransactionFullOutputDto>>(transRecharge));
                //check NVC LCD 
                var result = new PaymentAccountingOutputDto
                {
                    AccountId = transPayment[0].AccountId,
                    PaymentRequestId = transPayment[0].PaymentRequestId,
                    TransactionTypeId = transPayment[0].TransactionTypeId,
                    Amount = transPayment[0].Amount,
                    ShopCode = transPayment[0].ShopCode,
                    PaymentRequestStatus = paymentRq.Status,
                    Detail = transRechargeOutputDetail,
                };
                if (transRechargeOutputDetail.Count > 0)
                {
                    var firstData = transRechargeOutputDetail.Where(x => x.PaymentMethodId == EnmPaymentMethod.COD).FirstOrDefault();
                    if (firstData != null && firstData.Detail != null && firstData.Detail.CODs != null)
                    {
                        var isLCD = firstData.Detail.CODs.Where(x => x.TransporterCode == (int)(EnmCompanyType.LCD) || x.TransporterName == "Long Châu Delivery").ToArray();
                        if (isLCD.Length > 0)
                        {
                            var accountingDetail = await _accountingService.AccountingByOrderCode(paymentRq.OrderCode);
                            result.AccountingDetail = ObjectMapper.Map<FRTMO.DebitService.Dtos.AccountingHistoryDetailOutputDto, AccountingHistoryDetailOutputDto>(accountingDetail);
                        }
                    }
                }

                return result;
            }
        }
        public async Task<TransferFullOutputDto> GetTranferByPaymentCode(string paymentCode)
        {
            try
            {
                var paymentRequest = await _paymentRequestRepository.GetByPaymentCode(paymentCode);
                if (paymentRequest == null)
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_CODE_NOTFOUND)
                    .WithData("Data", "Payment Request Code {0} ", paymentCode);

                var transactions = await _transactionServiceV2.GetByPaymentRequestCode(paymentRequest.PaymentRequestCode, null);
                if (transactions.Count == 0)
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_NOT_TRANSACTION)
                    .WithData("Data", "Payment Request Code {0}", paymentCode);

                var transaction = transactions.FirstOrDefault(x => x.PaymentMethodId == EnmPaymentMethod.Transfer);
                if (transaction == null)
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_NOT_TRANFER_ACCOUNT_NOTFOUND)
                    .WithData("Data", "Payment Request Code {0}", paymentCode);

                var tranfers = await _transferService.GetByTransactionIds(new List<Guid>() { transaction.Id });
                return tranfers.FirstOrDefault();

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
        public async Task<PaymentRequestFullOutputDto> GetByPaymentCode(string paymentCode)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true, requiresNew: true, isolationLevel: System.Data.IsolationLevel.ReadUncommitted))
            {
                if (!string.IsNullOrEmpty(paymentCode))
                {
                    var paymentDate = _generateServiceV2.GetPaymentRequestDate(paymentCode);
                    var checkPayPM = await _paymentRepository.Get(paymentCode, paymentDate);
                    if (checkPayPM != null)
                    {
                        var item = await _paymentRequestRepository.GetByPaymentCode(paymentCode);
                        if (item == null)
                            throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_CODE_NOT_VALID).WithData("Data", "Payment Code {0} ", paymentCode);

                        var result = ObjectMapper.Map<PaymentRequest, PaymentRequestFullOutputDto>(item);
                        result.TotalPayment = (decimal)checkPayPM.Total;
                        result.Status = (byte)checkPayPM.Status;
                        return result;
                    }
                }
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_CODE_NOT_VALID)
                    .WithData("Data", "Payment Code {0} ", paymentCode);
            }
        }

        public async Task<PaymentRequestFullOutputDto> GetByPayment(string paymentRequestCode)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true, requiresNew: true, isolationLevel: System.Data.IsolationLevel.ReadUncommitted))
            {
                var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(paymentRequestCode);
                var item = await _paymentRequestRepository.GetByPaymentRequestCode(paymentRequestCode, paymentRequestDate);
                var result = ObjectMapper.Map<PaymentRequest, PaymentRequestFullOutputDto>(item);
                if (item != null)
                {
                    //get paymentCode
                    var paymentDate = _generateServiceV2.GetPaymentRequestDate(item.PaymentCode);
                    var checkPayPM = await _paymentRepository.Get(item.PaymentCode, paymentDate);
                    result.TotalPayment = (decimal)checkPayPM.Total;
                    result.Status = (byte)checkPayPM.Status;
                    return result;
                }
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_CODE_NOTFOUND)
                    .WithData("Data", "Payment Request Code {0} ", paymentRequestCode);
            }
        }
    }
}
