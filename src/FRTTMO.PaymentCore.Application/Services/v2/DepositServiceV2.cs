using FRTTMO.DebitService.Services;
using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Dto.TransactionDetailDto;
using FRTTMO.PaymentCore.Dto.v2;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Exceptions;
using FRTTMO.PaymentCore.Kafka.Eto;
using FRTTMO.PaymentCore.Kafka.Interface;
using FRTTMO.PaymentCore.Options;
using FRTTMO.PaymentCore.RemoteAPIs;
using FRTTMO.PaymentCore.Repositories;
using FRTTMO.PaymentIntegration.Dto;
using FRTTMO.PaymentIntegration.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Http.Client;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services.v2
{
    public class DepositServiceV2 : PaymentCoreAppService, ITransientDependency, IDepositServiceV2
    {
        private readonly ILogger<DepositServiceV2> _log;
        private readonly IPaymentServiceV2 _paymentServiceV2;
        private readonly ITransactionServiceV2 _transactionServiceV2;
        private readonly IAccountRepository _accountRepository;
        private readonly IPublishService<BaseETO> _iPublishService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IVendorPinService _vendorPinService;
        private readonly IUTopAppService _uTopAppService;
        private readonly ITransferService _transferService;
        private readonly ITaptapAppService _taptapAppService;
        private readonly IGotITAppService _gotITAppService;
        private readonly ILongChauService _longChauService;
        private readonly IBankTransferService _bankTransferService;
        private readonly IEWalletsAppService _eWalletsAppService;
        private readonly IInternalAppServiceCore _internalAppService;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IVPBAppService _vpbAppService;
        private readonly IGenerateServiceV2 _generateServiceV2;
        private readonly IDepositAdjustService _idepositAdjustService;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly PaymentOptions _paymentOptions;
        private readonly IDebitAppService _debitAppService;
        private readonly ElasticSearchServiceV2 _elasticSearchServiceV2;
        private readonly IQRHistoryService _qhHistoryService;
        private readonly FRTTMO.PaymentIntegration.Services.IElasticSearchService _elasticSearchIntergationService;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly IPayooAppService _integrationService;

        protected AbpExceptionLocalizationOptions LocalizationOptions { get; }

        public DepositServiceV2(
            ILogger<DepositServiceV2> log,
            IPaymentServiceV2 paymentServiceV2,
            ITransactionServiceV2 transactionServiceV2,
            IAccountRepository accountRepository,
            IPublishService<BaseETO> iPublishService,
            IUnitOfWorkManager unitOfWorkManager,
            IVendorPinService vendorPinService,
            IUTopAppService uTopAppService,
            ITransferService transferService,
            ITaptapAppService taptapAppService,
            IGotITAppService gotITAppService,
            IEWalletsAppService eWalletsAppService,
            IBankTransferService bankTransferService,
            //IElasticDepositAllSearchService syncDataESAppService,
            IPaymentService paymentService,
            IPaymentRequestRepository paymentRequestRepository,
            IInternalAppServiceCore internalAppServiceCore,
            IVPBAppService vPBAppService,
            ILongChauService longChauService,
            IGenerateServiceV2 generateServiceV2,
            IDepositAdjustService depositAdjustService,
            IPaymentTransactionRepository paymentTransactionRepository,
            IPaymentRepository paymentRepository,
            IOptions<PaymentOptions> paymentOptions,
            IDebitAppService debitAppService,
            ElasticSearchServiceV2 elasticSearchServiceV2,
            IOptions<AbpExceptionLocalizationOptions> localizationOptions,
            IQRHistoryService qhHistoryService,
            FRTTMO.PaymentIntegration.Services.IElasticSearchService elasticSearchIntergationService,
            ElasticSearchService elasticSearchService,
            IPayooAppService payooAppService
        ) : base()
        {
            _log = log;
            _paymentRequestRepository = paymentRequestRepository;
            _paymentServiceV2 = paymentServiceV2;
            _transactionServiceV2 = transactionServiceV2;
            _accountRepository = accountRepository;
            _iPublishService = iPublishService;
            _unitOfWorkManager = unitOfWorkManager;
            _vendorPinService = vendorPinService;
            _uTopAppService = uTopAppService;
            _transferService = transferService;
            _taptapAppService = taptapAppService;
            _gotITAppService = gotITAppService;
            _eWalletsAppService = eWalletsAppService;
            _bankTransferService = bankTransferService;
            _internalAppService = internalAppServiceCore;
            _vpbAppService = vPBAppService;
            _longChauService = longChauService;
            //_syncDataESAppService = syncDataESAppService;
            _generateServiceV2 = generateServiceV2;
            _idepositAdjustService = depositAdjustService;
            _paymentTransactionRepository = paymentTransactionRepository;
            _paymentRepository = paymentRepository;
            _paymentOptions = paymentOptions.Value;
            _debitAppService = debitAppService;
            _elasticSearchServiceV2 = elasticSearchServiceV2;
            LocalizationOptions = localizationOptions.Value;
            _qhHistoryService = qhHistoryService;
            _elasticSearchIntergationService = elasticSearchIntergationService;
            _elasticSearchService = elasticSearchService;
            _integrationService = payooAppService;
        }
        private static List<EnmVoucherProvider> VoucherAvailable = new() { EnmVoucherProvider.GotIT, EnmVoucherProvider.LC, EnmVoucherProvider.Taptap, EnmVoucherProvider.UTop, EnmVoucherProvider.Vani };
        private static Dictionary<EnmWalletProvider, PaymentIntegration.Common.EnumType.EnmWalletProvider> EwalletAvailabel = new()
        {
            { EnmWalletProvider.VNPAY, PaymentIntegration.Common.EnumType.EnmWalletProvider.VNPAY },
            { EnmWalletProvider.Smartpay, PaymentIntegration.Common.EnumType.EnmWalletProvider.Smartpay },
            { EnmWalletProvider.ShopeePay, PaymentIntegration.Common.EnumType.EnmWalletProvider.ShopeePay },
            { EnmWalletProvider.Zalopay, PaymentIntegration.Common.EnumType.EnmWalletProvider.Zalopay },
            { EnmWalletProvider.Foxpay, PaymentIntegration.Common.EnumType.EnmWalletProvider.Foxpay },
        };
        private async Task<bool> InitBeforDeposit(EnmPaymentMethod paymentMethod, DepositCoresInputDtoV2 inItem)
        {
            Stopwatch_StartNew();
            //chung
            if (inItem.Transaction == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                        .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                        .WithData("Detail", ParamTypes.JustData, "Transactions bị null")
                                                        .WithData("Data", "Transactions");
            //chung
            if (inItem.Transaction.PaymentMethodId != paymentMethod) throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_METHOD_INVALID);
            //chung
            if ((inItem.Transaction.Amount ?? 0m) < 0m) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                             .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                             .WithData("TransactionAmount", ParamTypes.JustData, inItem.Transaction.Amount)
                                                             .WithData("Entity", "Transaction Amount");

            //Step1: dùng chung 
            if (string.IsNullOrEmpty(inItem.Transaction.PaymentRequestCode)) throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_NOTFOUND).WithData("PaymentRequestID", inItem.Transaction.PaymentRequestId);


            AddLogElapsed("CheckAccount");
            //Step2:  Check account ID in table account
            if (!inItem.Transaction.AccountId.HasValue) throw new BusinessException(PaymentCoreErrorCodes.ERROR_ACCOUNT_NOTFOUND);
            var extAc = await _accountRepository.GetById(inItem.Transaction.AccountId.Value);
            if (extAc == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ACCOUNT_NOTFOUND).WithData(PaymentCoreErrorMessageKey.AccountId, inItem.Transaction.AccountId);
            if (inItem.Transaction.TransactionTypeId != EnmTransactionType.FirstDeposit && inItem.Transaction.TransactionTypeId != EnmTransactionType.WithdrawDeposit)
            {
                inItem.Transaction.TransactionTypeId = EnmTransactionType.Recharge;
            }
            return true;
        }
        /// <summary>
        /// Nạp tiền vào tài khoản ví khách hàng bằng tiền mặt
        /// </summary>
        public async Task<DepositByCashOutputDtoV2> DepositByCash(MaskDepositByCashInputDtoV2 inItem)
        {
            try
            {
                var obj = ObjectMapper.Map<MaskDepositByCashInputDtoV2, DepositCoresInputDtoV2>(inItem);
                //Insert  transaction
                DepositByCashOutputDtoV2 rt;
                using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                {
                    try
                    {
                        AddLogElapsed("InsertTransDetail");
                        var trans = await _transactionServiceV2.InsertTransactionWithDetail(obj, true);
                        AddLogElapsed("MapReturn");
                        rt = ObjectMapper.Map<DepositCoresOutputDtoV2, DepositByCashOutputDtoV2>(trans);
                        await unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        await unitOfWork.RollbackAsync();
                        throw;
                    }
                }
                await _iPublishService.ProduceAsync(new TransactionCreatedETO
                {
                    PaymentCode = inItem.PaymentCode,
                    Data = rt
                });
                if (obj.Transaction.TransactionTypeId == EnmTransactionType.FirstDeposit)
                {
                    await _iPublishService.ProduceAsync(new CollectDepositTransactionCreatedETO
                    {
                        PaymentCode = inItem.PaymentCode,
                        Data = rt
                    });
                }
                return rt;
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.DepositByCash OrderCode {inItem?.OrderCode}: {ex}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData("Message", $"Order {inItem?.OrderCode}: error - " + ex.Message);
                if ((ex is AbpRemoteCallException) || (ex is BusinessException) || (ex is UserFriendlyException))
                    throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem?.OrderCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }

        public async Task<DepositByCODOutputDtoV2> DepositByCOD(MaskDepositByCODInputDtoV2 inItem)
        {
            try
            {
                var obj = ObjectMapper.Map<MaskDepositByCODInputDtoV2, DepositCoresInputDtoV2>(inItem);
                //Insert  transaction
                if (inItem.COD == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                       .WithData(PaymentCoreErrorMessageKey.PaymentRequestCode, ParamTypes.JustData, inItem.Transaction.PaymentRequestCode)
                                                       .WithData("Detail", ParamTypes.JustData, "Không thấy thông tin COD!")
                                                       .WithData("Data", "Thông tin COD");
                if (inItem.COD.Amount != inItem.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("COD", inItem.COD);
                if ((inItem.COD.Amount ?? 0m) < 0m) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                       .WithData(PaymentCoreErrorMessageKey.PaymentRequestCode, ParamTypes.JustData, inItem.Transaction.PaymentRequestCode)
                                                                .WithData("COD", ParamTypes.JustData, inItem.COD)
                                                                 .WithData("Entity", "COD Amount");
                DepositByCODOutputDtoV2 rt;
                using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                {
                    try
                    {
                        AddLogElapsed("InsertTransDetail");
                        var trans = await _transactionServiceV2.InsertTransactionWithDetail(obj, true);
                        AddLogElapsed("MapReturn");
                        rt = ObjectMapper.Map<DepositCoresOutputDtoV2, DepositByCODOutputDtoV2>(trans);
                        await unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        await unitOfWork.RollbackAsync();
                        throw;
                    }
                }
                await _iPublishService.ProduceAsync(new TransactionCreatedETO
                {
                    PaymentCode = inItem.PaymentCode,
                    Data = rt
                });
                if (obj.Transaction.TransactionTypeId == EnmTransactionType.FirstDeposit)
                {
                    await _iPublishService.ProduceAsync(new CollectDepositTransactionCreatedETO
                    {
                        PaymentCode = inItem.PaymentCode,
                        Data = rt
                    });
                }
                return rt;
            }
            catch (Exception ex)
            {
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData("Message", $"Order {inItem?.OrderCode}: error - " + ex.Message);
                if ((ex is AbpRemoteCallException) || (ex is BusinessException) || (ex is UserFriendlyException))
                    throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.PaymentRequestCode, inItem?.Transaction.PaymentRequestCode)
                    .WithData(PaymentCoreErrorMessageKey.Message, ex.Message);
            }
        }

        public async Task<CashFullOutputV2Dto> DepositCashAll(DepositAllInputDto inItem)
        {
            var rt = new CashFullOutputV2Dto
            {
                DataSucceeded = new List<SucceededCash>(),
                DataFailed = new List<FailedCash>(),
            };
            foreach (var item in inItem.Cash)
            {
                var DataFailed = new FailedCash();
                item.CreatedDate = DateTime.Now;
                var dataMapFailed = ObjectMapper.Map<DepositCashInputDto, DepositCashOutPutDto>(item);
                try
                {

                    if (item.TransactionId != null)
                    {
                        var itemDataSucceeded = new SucceededCash();
                        itemDataSucceeded.Succeeded = dataMapFailed;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                    else
                    {
                        if (inItem.PaymentSource != null && inItem.PaymentSource.TypePayment == EnmPaymentType.Chi)
                        {
                            //lay thong tin account
                            var account = (!string.IsNullOrEmpty(inItem.CustCode)) ? await _accountRepository.GetByCustomerId(Guid.Parse(inItem.CustCode))
                                : await _accountRepository.GetById((Guid)inItem.AccountId.Value)
                                ;
                            if (account == null)
                                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ACCOUNT_NOTFOUND).WithData("AccountId", inItem.AccountId.Value);
                            inItem.AccountId = account.Id;
                            var requestTransaction = new InsertTransactionInputDtoV2
                            {
                                CreatedDate = DateTime.Now,
                                CreatedBy = inItem.PaymentSource.CreatedBy,
                                AccountId = account.Id,
                                TransactionTypeId = item.TransactionTypeId,
                                Status = EnmTransactionStatus.Created,
                                TransactionTime = item.TransactionTime,
                                Amount = item.Amount,
                                ShopCode = inItem.ShopCode,
                                PaymentMethodId = item.PaymentMethodId,
                                TransactionFee = 0,
                                Note = "",
                                AdditionAttributes = "",
                                PaymentRequestDate = item.TransactionTime,
                                PaymentRequestCode = inItem.PaymentRequestCode,
                            };
                            //tao cashback
                            var paymentCashBackRequest = new CashbackDepositRefundBaseDtoV2()
                            {
                                PaymentRequestCode = inItem.PaymentRequestCode,
                                TotalPayment = (decimal)inItem.TotalPayment,
                                Transaction = requestTransaction,
                            };
                            var extAc = await CreateCashbackTransaction(paymentCashBackRequest, account);
                        }

                        //create transaction
                        var requestCash = ObjectMapper.Map<DepositAllInputDto, MaskDepositByCashInputDtoV2>(inItem);
                        requestCash.Transaction = ObjectMapper.Map<DepositCashInputDto, DepositTransactionInputDtoV2>(item);
                        requestCash.Transaction.ShopCode = inItem.ShopCode;
                        requestCash.Transaction.PaymentRequestCode = inItem.PaymentRequestCode;
                        requestCash.Transaction.AccountId = inItem.AccountId;
                        var resultCash = await DepositByCash(requestCash);

                        var dataMap = ObjectMapper.Map<TransactionFullOutputDtoV2, DepositCashOutPutDto>(resultCash.Transaction);
                        dataMap.TransactionId = resultCash.Transaction.Id;
                        var itemDataSucceeded = new SucceededCash();
                        itemDataSucceeded.Succeeded = dataMap;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                }
                catch (Exception ex)
                {
                    DataFailed.Failed = dataMapFailed;
                    if (ex is BusinessException)
                    {
                        DataFailed.Message = CollectMessage((BusinessException)ex);
                    }
                    if (ex is AbpRemoteCallException)
                    {
                        DataFailed.Message = (ex as AbpRemoteCallException).Message;
                    }
                }
                finally
                {
                    if (DataFailed.Failed != null)
                    {
                        DataFailed.Failed.TransactionId = null;
                        rt.DataFailed.Add(DataFailed);
                    }
                }
            };
            try 
            {
                // sync Transfer to ES
                var result = new TransactionDetailTransferOutputDto();
                result.PaymentMethodId = (byte?)EnmPaymentMethod.Cash;
                result.PaymentCode = inItem.PaymentCode;
                var syncES = await _elasticSearchService.SyncDataESTransfer(inItem.PaymentCode, result); 
            }
            catch(Exception ex)
            {
                _log.LogError($"{_CoreName}.DepositCashAll: {ex}| Request body: {JsonConvert.SerializeObject(inItem)} ");
            }
            return rt;
        }

        public async Task<CodFullOutputDtoV2Dto> DepositCodsAll(CodRequestDto requestDto)
        {
            var rt = new CodFullOutputDtoV2Dto
            {
                DataSucceeded = new List<SucceededCod>(),
                DataFailed = new List<FailedCOD>(),
            };
            var DataFailed = new FailedCOD();

            foreach (var item in requestDto.cods)
            {
                try
                {
                    item.CODetail.CreatedDate = DateTime.Now;
                    if (!string.IsNullOrEmpty(item.CODetail.TransactionId.ToString()))
                    {
                        var itemDataSucceeded = new SucceededCod();
                        itemDataSucceeded.Succeeded = item;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                    else
                    {
                        var request = new MaskDepositByCODInputDtoV2();
                        request.COD = ObjectMapper.Map<CODInputDto, PaymentCODInputDtoV2>(item.CODetail);
                        request.Transaction = ObjectMapper.Map<DepositTransactionDto, DepositTransactionInputDtoV2>(item.Transaction);
                        request.Transaction.ShopCode = requestDto.ShopCode;
                        request.Transaction.PaymentRequestCode = requestDto.PaymentRequestCode;
                        request.Transaction.AccountId = requestDto.AccountId;
                        var resultdepositEWallet = await DepositByCOD(request);
                        item.CODetail.TransactionId = resultdepositEWallet.Transaction.Id;
                        var itemDataSucceeded = new SucceededCod();
                        itemDataSucceeded.Succeeded = item;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                }
                catch (Exception ex)
                {
                    DataFailed.Failed = item;
                    if (ex is BusinessException)
                    {
                        DataFailed.Message = CollectMessage((BusinessException)ex);
                    }
                    else if (ex is AbpRemoteCallException)
                    {
                        DataFailed.Message = ex.Message;
                    }
                    else
                    {
                        DataFailed.Message = ex.Message;
                    }
                }
                finally
                {
                    if (DataFailed.Failed != null)
                    {
                        DataFailed.Failed.CODetail.TransactionId = null;
                        rt.DataFailed.Add(DataFailed);
                    }
                }
            };
            return rt;
        }

        public async Task<DebtSaleOutputDto> DepositDebtSale(DebtSaleInputDto inItem)
        {
            try
            {
                var obj = ObjectMapper.Map<DebtSaleInputDto, DepositCoresInputDtoV2>(inItem);
                if (inItem.Debit == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                       .WithData(PaymentCoreErrorMessageKey.PaymentCode, ParamTypes.JustData, inItem.PaymentCode)
                                                        .WithData("Detail", ParamTypes.JustData, "Không thấy thông tin Debit!")
                                                        .WithData("Data", "Thông tin Debit");
                if (inItem.Debit.Amount != inItem.Transaction.Amount) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("Debit", inItem.Debit);
                if ((inItem.Debit.Amount ?? 0m) < 0m) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                       .WithData(PaymentCoreErrorMessageKey.PaymentCode, ParamTypes.JustData, inItem.PaymentCode)
                                                                .WithData("Debit", ParamTypes.JustData, inItem.Debit)
                                                                 .WithData("Entity", "Debit Amount");
                if (inItem.Debit.TaxCode == null && inItem.Debit.Phone == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                       .WithData(PaymentCoreErrorMessageKey.PaymentCode, ParamTypes.JustData, inItem.PaymentCode)
                                                                .WithData("TaxCode and Phone", ParamTypes.JustData, "không hợp lệ")
                                                                 .WithData("Entity", "Debit TaxCode,Phone");                //Insert  transaction
                DebtSaleOutputDto rt;
                using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                {
                    try
                    {
                        AddLogElapsed("InsertTransDetail");

                        var trans = await _transactionServiceV2.InsertTransactionWithDetail(obj, true);
                        AddLogElapsed("MapReturn");
                        rt = ObjectMapper.Map<DepositCoresOutputDtoV2, DebtSaleOutputDto>(trans);

                        await unitOfWork.SaveChangesAsync();
                        //// gọi debit tạm thời gọi api 
                        //var totalAmount = await _paymentRequestRepository.GetToTalBill(inItem.PaymentCode, EnmPaymentRequestStatus.Confirm);

                        //DebitCreateInputDto inputDto = new DebitCreateInputDto()
                        //{
                        //    PaymentCode = inItem.PaymentCode,
                        //    CustomerID = inItem.CustCode,
                        //    CustomerName = inItem.CustName,
                        //    TotalAmount = totalAmount != null ? totalAmount.TotalPayment : inItem.Debit.Amount,
                        //    TotalDebitAmount = inItem.Debit.Amount,
                        //    TotalPayment = inItem.Debit.Amount,
                        //    CreatedBy = inItem.Debit.CreatedBy,
                        //    ShopCode = inItem.Transaction.ShopCode,
                        //    CompanyID = EnmCompanyType.CusDebit,
                        //    Phone = inItem.Phone,
                        //    TaxCode = inItem.Debit.TaxCode
                        //};
                        //var requestCreateDebit = ObjectMapper.Map<DebitCreateInputDto, FRTTMO.DebitService.Dto.DebitCreateInputDto>(inputDto);
                        //await _debitAppService.Create(requestCreateDebit);


                        await _iPublishService.ProduceAsync(new TransactionCreatedETO
                        {
                            PaymentCode = inItem.PaymentCode,
                            Data = rt
                        });
                        if (inItem.Transaction.TransactionTypeId == EnmTransactionType.Recharge)
                        {
                            await _iPublishService.ProduceAsync(new CollectDepositTransactionCreatedETO
                            {
                                PaymentCode = inItem.PaymentCode,
                                Data = rt
                            });
                        }
                    }
                    catch (Exception)
                    {
                        await unitOfWork.RollbackAsync();
                        throw;
                    }
                }
                return rt;
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.DepositDebtSale PaymentCode {inItem?.PaymentCode}: {ex}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData("Message", $"PaymentCode {inItem?.PaymentCode}: error - " + ex.Message);
                if ((ex is AbpRemoteCallException) || (ex is BusinessException) || (ex is UserFriendlyException) || (ex is CustomBusinessException))
                    throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.PaymentCode, inItem?.PaymentCode)
                    .WithData(PaymentCoreErrorMessageKey.Message, ex.Message);
            }
        }

        public async Task<DebtSaleFullOutputV2Dto> DepositDebtSaleAll(DepositAllInputDto inItem)
        {
            var rt = new DebtSaleFullOutputV2Dto
            {
                DataSucceeded = new List<SucceededDebtSale>(),
                DataFailed = new List<FailedDebtSale>(),
            };

            foreach (var item in inItem.DebtSaleAll)
            {
                var DataFailed = new FailedDebtSale();
                var dataMapFailed = ObjectMapper.Map<DebtSaleInputV2Dto, DepositDebtSaleOutPutDto>(item);
                try
                {
                    item.Debit.CreatedDate = DateTime.Now;
                    if (!string.IsNullOrEmpty(item.Debit.TransactionId.ToString()))
                    {
                        var itemDataSucceeded = new SucceededDebtSale();
                        itemDataSucceeded.Succeeded = dataMapFailed;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                    else
                    {
                        var requestDebtSale = ObjectMapper.Map<DebtSaleInputV2Dto, DebtSaleInputDto>(item);
                        requestDebtSale.PaymentCode = inItem.PaymentCode;
                        requestDebtSale.Debit.CustName = inItem.CustName;
                        requestDebtSale.Debit.CustCode = Guid.Parse(inItem.CustCode);
                        requestDebtSale.Transaction.ShopCode = inItem.ShopCode;
                        requestDebtSale.Transaction.PaymentRequestCode = inItem.PaymentRequestCode;
                        requestDebtSale.Transaction.AccountId = inItem.AccountId;

                        var resultDebtSale = await DepositDebtSale(requestDebtSale);
                        var dataMap = ObjectMapper.Map<DebtSaleOutputDto, DepositDebtSaleOutPutDto>(resultDebtSale);
                        dataMap.Debit = resultDebtSale.Debit;

                        var itemDataSucceeded = new SucceededDebtSale();
                        itemDataSucceeded.Succeeded = dataMap;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                }
                catch (Exception ex)
                {
                    DataFailed.Failed = dataMapFailed;
                    if (ex is BusinessException)
                    {
                        DataFailed.Message = CollectMessage((BusinessException)ex);
                    }
                    else if (ex is AbpRemoteCallException)
                    {
                        DataFailed.Message = ex.Message;
                    }
                    else
                    {
                        DataFailed.Message = ex.Message;
                    }
                }
                finally
                {
                    if (DataFailed.Failed != null)
                    {
                        DataFailed.Failed.Debit.TransactionId = null;
                        rt.DataFailed.Add(DataFailed);
                    }
                }
            };
            return rt;
        }

        public async Task<CreateRequestDepositAllOutputDto> CreateRequestDepositAll(CreateRequestDepositAllInputDto inItem)
        {
            try
            {

                var result = new CreateRequestDepositAllOutputDto();
                var paymentDate = _generateServiceV2.GetPaymentRequestDate(inItem.PaymentCode);
                var payment = await _paymentRepository.Get(inItem.PaymentCode, paymentDate);
                if (payment == null)
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_CODE_NOT_VALID);

                if (payment.Status == EnmPaymentStatus.Complete)
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_FINISHED).WithData("PaymentCode", inItem.PaymentCode);

                if (inItem.PaymentRequestType == null)
                    inItem.PaymentRequestType = EmPaymentRequestType.PaymentCoreRequest;

                var inputPaymentRequest = new PaymentRequestInputDto
                {
                    TotalPayment = inItem.TotalPaymentRequest,
                    PaymentCode = inItem.PaymentCode,
                    CreatedBy = inItem.PaymentSource != null ? inItem.PaymentSource.CreatedBy : "",
                    TypePaymentValue = inItem.PaymentRequestType,
                };
                if (inItem.isVoucher && inItem.TotalPaymentRequest > payment.Total)
                {
                    inputPaymentRequest.TotalPayment = (decimal)payment.Total;
                }
                //bool isTransaction = false;
                var payRQtoES = new PaymentRequestOutputDto();
                if (string.IsNullOrEmpty(inItem.PaymentRequestCode))
                {
                    var payRQ = await _paymentRequestRepository.GetListOfCode(inItem.PaymentCode);
                    var totalCreate = payRQ.Where(x => x.Status == EnmPaymentRequestStatus.Complete || x.Status == EnmPaymentRequestStatus.Confirm);
                    var sumtotalCreate = totalCreate.Sum(x => x.TotalPayment);
                    if (!inItem.isVoucher && ((sumtotalCreate + inItem.TotalPaymentRequest) > payment.Total))
                    {
                        if ((payment.Total - sumtotalCreate) == 0)
                        {
                            throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_NOT_FINISH).WithData("PaymentRequestCode", totalCreate.FirstOrDefault() != null ? totalCreate.FirstOrDefault().PaymentRequestCode : "");
                        }
                        else if (!inItem.isTransfer)
                        {
                            throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_DEPOSIT_NOTENOUGH).WithData("Monney", payment.Total - sumtotalCreate);
                        }
                    }
                    bool isFinish = payRQ.Any(x => x.Status == EnmPaymentRequestStatus.Confirm && x.TypePayment == EmPaymentRequestType.PaymentCoreRequest);
                    if (isFinish)
                        throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_NOT_FINISH);
                    // call hàm create paymentRequest
                    //Tạo PaymentRequest
                    //var amountRequest = inItem.PaymentSource.Detail.Sum(x => x.Amount);
                    //if (amountRequest == 0)
                    //    throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_NOT_COLLECT_MONNEY);
                    var paymentRequest = await _idepositAdjustService.CreateRequest(inputPaymentRequest);
                    inItem.PaymentRequestCode = paymentRequest.PaymentRequestCode;
                    result.PaymentRequestCode = paymentRequest.PaymentRequestCode;
                    result.PaymentRequestId = paymentRequest.Id.ToString();
                    payRQtoES = paymentRequest;
                    //isTransaction = true;
                }
                else
                {
                    var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(inItem.PaymentRequestCode);
                    var payRQ = await _paymentRequestRepository.GetByPaymentRequestCode(inItem.PaymentRequestCode, paymentRequestDate);
                    //nếu tồn tại ví điện tử mà số tiền còn thiếu thì tạo thêm paymentRequestCode cho loại Voucher
                    if (inItem.isCreate && payRQ.TotalPayment < payment.Total)
                    {
                        if ((payRQ.TotalPayment + inputPaymentRequest.TotalPayment) > payment.Total)
                        {
                            inputPaymentRequest.TotalPayment = (decimal)payment.Total - (decimal)payRQ.TotalPayment;
                        }
                        var paymentRequest = await _idepositAdjustService.CreateRequest(inputPaymentRequest);
                        result.PaymentRequestCodeV2 = paymentRequest.PaymentRequestCode;
                        result.PaymentRequestIdV2 = paymentRequest.Id.ToString();
                        payRQtoES = paymentRequest;
                    }
                    //var paymentSource = await _paymentTransactionRepository.Get(inItem.PaymentCode);
                    //if (paymentSource == null)
                    //{
                    //    isTransaction = true;
                    //}
                    result.PaymentRequestCode = inItem.PaymentRequestCode;
                    result.PaymentRequestId = payRQ.Id.ToString();
                }
                if (inItem.PaymentSource != null)
                {
                    // call hàm create PaymentTransaction
                    var PaymentSourceIds = new List<PaymentSourceOutPutDto>();
                    foreach (var item in inItem.PaymentSource.Detail)
                    {
                        var insertInput = ObjectMapper.Map<PaymentTransactionBaseDto, PaymentSource>(item);
                        insertInput.PaymentId = payment.Id;
                        insertInput.PaymentVersion = 1;
                        insertInput.Status = EnmPaymentTransactionStatus.Complete;
                        insertInput.PaymentCode = inItem.PaymentCode;
                        var outPutPaymentSource = new PaymentSource();
                        var isExists = await _paymentTransactionRepository.IsCheckInfor(insertInput);
                        if (!isExists)
                        {
                            outPutPaymentSource = await _paymentTransactionRepository.Insert(insertInput);
                        }
                        else
                        {
                            outPutPaymentSource = await _paymentTransactionRepository.Get(inItem.PaymentCode);
                        }
                        var addEto = ObjectMapper.Map<PaymentSource, PaymentTransactionCreatedEto>(outPutPaymentSource);
                        await _iPublishService.ProduceAsync(addEto);
                        var outputTransaction = ObjectMapper.Map<PaymentSource, PaymentSourceOutPutDto>(outPutPaymentSource);

                        PaymentSourceIds.Add(outputTransaction);
                    }
                    result.PaymentSourceId = PaymentSourceIds;
                }

                result.CreatedBy = payment.CreatedBy;
                result.PaymentDate = payment.PaymentDate;
                result.UpdatedBy = payment.ModifiedBy;
                result.CreatedDate = payment.CreatedDate;
                result.TypePayment = payment.Type;
                try 
                {
                    // sync ES
                    if (result != null && payRQtoES != null)
                    {
                        if (result != null && result.TypePayment == EnmPaymentType.Chi &&
                            (payRQtoES.TypePayment == EmPaymentRequestType.PaymentCoreRequest || payRQtoES.TypePayment == EmPaymentRequestType.DepositRefund))
                        {
                            var resultSyncES = new TransactionDetailTransferOutputDto();
                            resultSyncES.PaymentCode = inItem.PaymentCode;
                            resultSyncES.SourceCode = ObjectMapper.Map<List<PaymentSourceOutPutDto>, List<SourceCodeSyncES>>(result.PaymentSourceId);
                            var syncES = await _elasticSearchService.SyncDataESTransfer(inItem.PaymentCode, resultSyncES);

                            if (syncES == true)
                            {
                                _log.LogInformation(string.Format("SyncTrue: {0}| Request body: {1} ", null, JsonConvert.SerializeObject(result)));
                            }
                            else
                            {
                                _log.LogInformation(string.Format("SyncFail: {0}| Request body: {1} ", null, JsonConvert.SerializeObject(result)));
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    _log.LogError($"{_CoreName}.CreateRequestDepositAll: {ex}| Request body: {JsonConvert.SerializeObject(inItem)} ");
                }
                return result;
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.CreateRequestDepositAll PaymentCode {inItem?.PaymentCode}: {ex}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData("Message", $"PaymentCode {inItem?.PaymentCode}: error - " + ex.Message);
                if ((ex is AbpRemoteCallException) || (ex is BusinessException) || (ex is UserFriendlyException))
                    throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.PaymentCode, inItem?.PaymentCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }


        public async Task<VerifyTDTOutputDto> FinishTSTD(VerifyTDTSInputDto inItem)
        {
            // Payment.Total và PaymentRequest.where(x=>x.paymentCode).sum(x=>x.amount)
            //cập nhật trạng thái của PaymentRequest
            //nếu PaymentRequest.amount và transaction.sum(x=>x.amount) thì sẽ update status paymentRequest=4 và Tạo Transaction Thanh toán
            //Nếu Total = Sum(PaymentRequest có PaymentCode) cập nhật Status = 4 


            var result = new VerifyTDTOutputDto();
            if (!string.IsNullOrEmpty(inItem.PaymentRequestCode) && !string.IsNullOrEmpty(inItem.PaymentCode))
            {
                var TypePaymentRequest = inItem.TypePaymentRequest != null ? inItem.TypePaymentRequest : EmPaymentRequestType.PaymentCoreRequest;
                var paymentDate = _generateServiceV2.GetPaymentRequestDate(inItem.PaymentCode);
                var checkPayPM = await _paymentRepository.Get(inItem.PaymentCode, paymentDate);
                result.TotalFinal = checkPayPM.Total;
                try
                {
                    try
                    {
                        // hoàn tất paymentRequest 2 loại theo xử ý khác nhau
                        if (checkPayPM.Type == EnmPaymentType.Thu)
                        {
                            var paymentRequestQRs = await _paymentRequestRepository.GetListByPaymentCode(inItem.PaymentCode);
                            var dataFinish = paymentRequestQRs.Where(x => x.Status != EnmPaymentRequestStatus.Cancel && x.Status != EnmPaymentRequestStatus.Complete);
                            foreach (var item in dataFinish)
                            {
                                result.IsPaymentRequest = true;
                                try
                                {
                                    inItem.Transaction.PaymentCode = inItem.PaymentCode;
                                    inItem.Transaction.PaymentRequestCode = item.PaymentRequestCode;
                                    inItem.Transaction.Total = (decimal)item.TotalPayment;
                                    var paymentTrans = await _paymentServiceV2.CreatePaymentRequest(inItem.Transaction);
                                }
                                catch (Exception)
                                {
                                    result.IsPaymentRequest = false;
                                }
                            }
                        }
                        else
                        {
                            var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(inItem.PaymentRequestCode);
                            var paymentRequestQR = await _paymentRequestRepository.GetByPaymentRequestCode(inItem.PaymentRequestCode, paymentRequestDate);
                            //Check SUM(TD)-SUM(TS)
                            var sumTrans = await _transactionServiceV2.GetSumAmountOfPaymentRequest(
                                inItem.PaymentRequestCode,
                                new List<EnmTransactionType>
                                {
                                EnmTransactionType.Refund,
                                EnmTransactionType.WithdrawDeposit
                                },
                                paymentRequestDate
                            );
                            if (sumTrans == paymentRequestQR.TotalPayment && sumTrans == checkPayPM.Total)
                            {
                                _log.LogInformation(string.Format("Finish chi = số tiền: {0}| sumTrans: {1} ", checkPayPM.PaymentCode, JsonConvert.SerializeObject(sumTrans)));

                                //cập nhật trạng thái của paymentRequest
                                paymentRequestQR.Status = EnmPaymentRequestStatus.Complete;
                                paymentRequestQR.ModifiedBy = inItem.Transaction.Transaction.CreatedBy;
                                var resultUpdatePR = await _paymentRequestRepository.Update(paymentRequestQR);
                                result.IsPaymentRequest = true;
                                var transactionOutput = await _transactionServiceV2.GetByPaymentRequestCode(paymentRequestQR.PaymentRequestCode, paymentRequestDate);
                                var dataKafka = transactionOutput.Where(x => x.TransactionTypeId == EnmTransactionType.WithdrawDeposit || x.TransactionTypeId == EnmTransactionType.Refund).FirstOrDefault();

                                _log.LogInformation(string.Format("dataKafka chi : {0}| dataKafka: {1} ", checkPayPM.PaymentCode, JsonConvert.SerializeObject(dataKafka)));

                                //publish kafka message
                                if (dataKafka != null)
                                {
                                    var dataTranfer = new TransferFullOutputEto();
                                    if (dataKafka.PaymentMethodId == EnmPaymentMethod.Transfer)
                                    {
                                        var tranfer = await _transferService.GetByTransactionId(dataKafka.Id);
                                        if (tranfer != null && tranfer.Count > 0 && tranfer.FirstOrDefault() != null)
                                        {
                                            var getFirst = tranfer.FirstOrDefault();
                                            dataTranfer = ObjectMapper.Map<TransferFullOutputDto, TransferFullOutputEto>(getFirst);
                                        }
                                    }
                                    var paymentTransEto = ObjectMapper.Map<TransactionFullOutputDtoV2, TransactionFullOutputEto>(dataKafka);

                                    var orderCode = await _paymentTransactionRepository.Get(checkPayPM.PaymentCode);
                                    if (inItem.TypePaymentRequest == EmPaymentRequestType.PaymentCoreRequest)
                                    {
                                        var data = new WithdrawReturnCompletedOutputEto();
                                        data.PaymentCode = checkPayPM.PaymentCode;
                                        data.OrderReturnId = orderCode != null ? orderCode.SourceCode : "";
                                        data.Transfers = dataTranfer;
                                        data.Note = "FinishTSTD";
                                        data.Transaction = paymentTransEto;
                                        _log.LogInformation(string.Format("bắn RT chi : {0}| dataKafka: {1} ", checkPayPM.PaymentCode, JsonConvert.SerializeObject(data)));
                                        await _iPublishService.ProduceAsync(data);
                                        _log.LogInformation(string.Format("bắn RT chi : {0}| 0: {1} ", checkPayPM.PaymentCode, "Success"));
                                    }
                                    else
                                    {
                                        //publish kafka message
                                        var data = new WithdrawDepositCompletedOutputEto();
                                        data.OrderCode = orderCode != null ? orderCode.SourceCode : "";
                                        data.PaymentCode = checkPayPM.PaymentCode;
                                        data.Transfers = dataTranfer;
                                        data.Transaction = paymentTransEto;
                                        _log.LogInformation(string.Format("bắn RD chi : {0}| dataKafka: {1} ", checkPayPM.PaymentCode, JsonConvert.SerializeObject(data)));
                                        await _iPublishService.ProduceAsync(data);
                                        _log.LogInformation(string.Format("bắn RD chi : {0}| 0: {1} ", checkPayPM.PaymentCode, "Success"));
                                    }
                                    checkPayPM.Status = EnmPaymentStatus.Complete;
                                    checkPayPM.ModifiedBy = inItem.Transaction.Transaction.CreatedBy;
                                    await _paymentRepository.UpdateAsync(checkPayPM);
                                    result.IsPayment = true;
                                    result.RemainingAmount = paymentRequestQR.TotalPayment - sumTrans;
                                    return result;
                                }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"{_CoreName}.CreatePaymentRequest FinishTSTD PaymentCode {inItem?.PaymentCode}: {ex}");
                    }
                    if (checkPayPM != null && checkPayPM.Status != null && (checkPayPM.Status == EnmPaymentStatus.Created))
                    {
                        var payRQ = await _paymentRequestRepository.GetListOfPaymentCode(inItem.PaymentCode, (EmPaymentRequestType)TypePaymentRequest);
                        var totalPayRQ = payRQ.Sum(x => x.TotalPayment);
                        var totalDeviant = checkPayPM.Total - totalPayRQ;
                        result.RemainingAmount = totalDeviant;
                        if (totalDeviant != 0)
                        {

                            bool isTranfer = false;
                            foreach (var item in payRQ)
                            {
                                var hasDepositTransfer = await _transactionServiceV2.HasTransferDepositNotIsConfirmTrans(item.PaymentRequestCode, item.PaymentRequestDate);
                                if (hasDepositTransfer)
                                {
                                    isTranfer = true;
                                    break;
                                }
                            }
                            decimal totalTransaction = 0;

                            if (totalPayRQ == 0)
                            {
                                var payRQNotFinish = await _paymentRequestRepository.GetListOfCode(inItem.PaymentCode);
                                foreach (var item in payRQNotFinish)
                                {
                                    var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(item.PaymentRequestCode);

                                    var sumTrans = await _transactionServiceV2.GetSumAmountOfPaymentRequest(
                                                    item.PaymentRequestCode,
                                                    new List<EnmTransactionType>
                                                    {
                                                         EnmTransactionType.Recharge,
                                                         EnmTransactionType.FirstDeposit,
                                                         EnmTransactionType.Refund,
                                                         EnmTransactionType.WithdrawDeposit
                                                    },
                                                    paymentRequestDate
                                                );
                                    totalTransaction = totalTransaction + (decimal)sumTrans;
                                }
                                result.RemainingAmount = (decimal)checkPayPM.Total - (decimal)totalTransaction;
                            }
                            if (isTranfer)
                            {
                                var conditionWrongDeviant = Math.Abs(totalDeviant.Value) > _paymentOptions.TotalPaymentMaxDeviant;
                                if (!conditionWrongDeviant)
                                {
                                    result.IsPayment = true;
                                    checkPayPM.Status = EnmPaymentStatus.Complete;
                                    checkPayPM.ModifiedBy = inItem.Transaction.Transaction.CreatedBy;
                                    await _paymentRepository.UpdateAsync(checkPayPM);
                                }
                            }
                        }
                        else
                        {
                            checkPayPM.Status = EnmPaymentStatus.Complete;
                            checkPayPM.ModifiedBy = inItem.Transaction.Transaction.CreatedBy;
                            await _paymentRepository.UpdateAsync(checkPayPM);
                            result.IsPayment = true;
                        }
                    }
                    if (result.RemainingAmount < 0)
                    {
                        result.RemainingAmount = 0;
                        result.IsPayment = true;
                        checkPayPM.Status = EnmPaymentStatus.Complete;
                        checkPayPM.ModifiedBy = inItem.Transaction.Transaction.CreatedBy;
                        await _paymentRepository.UpdateAsync(checkPayPM);
                    }
                }
                catch (Exception)
                {

                    if (result.RemainingAmount == null)
                    {
                        var payRQ = await _paymentRequestRepository.GetListOfPaymentCode(inItem.PaymentCode, (EmPaymentRequestType)TypePaymentRequest);
                        var totalPayRQ = payRQ.Sum(x => x.TotalPayment);
                        var totalDeviant = inItem.Total - totalPayRQ;
                        result.RemainingAmount = totalDeviant;
                    }
                    result.IsPaymentRequest = false;
                    result.IsPayment = false;
                }
            }



            return result;
        }

        public async Task<DepositByCardOutputDtoV2> DepositByCard(MaskDepositByCardInputDtoV2 inItem)
        {

            if (inItem.Cards == null || !inItem.Cards.Any()) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                    .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                    .WithData("Detail", ParamTypes.JustData, "Không thấy thông tin Card!")
                                                    .WithData("Data", "Thông tin Card");
            if (inItem.Cards.Sum(c => c.Amount ?? 0m) != inItem.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("Card", inItem.Cards);
            if (inItem.Cards.Any(c => c.Amount.HasValue && c.Amount < 0m)) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                            .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                            .WithData("Cards", ParamTypes.JustData, inItem.Cards)
                                                            .WithData("Entity", "Cards Amount");
            if (inItem.Cards.FirstOrDefault().Paymethod == 2)
            {
                //check trans đối với hình thức trả góp 
                var response = await _integrationService.CheckTransPayoo(inItem.PaymentCode);
                if (string.IsNullOrEmpty(response.OrderCode))
                {
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_CHECK_TRANS_FAIL);
                }
            }
            try
            {
                var obj = ObjectMapper.Map<MaskDepositByCardInputDtoV2, DepositCoresInputDtoV2>(inItem);
                //Insert  transaction
                DepositByCardOutputDtoV2 rt;
                using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                {
                    try
                    {
                        AddLogElapsed("InsertTransDetail");
                        var trans = await _transactionServiceV2.InsertTransactionWithDetail(obj, true);
                        AddLogElapsed("MapReturn");
                        rt = ObjectMapper.Map<DepositCoresOutputDtoV2, DepositByCardOutputDtoV2>(trans);
                        await unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        await unitOfWork.RollbackAsync();
                        throw;
                    }
                }
                await _iPublishService.ProduceAsync(new TransactionCreatedETO
                {
                    OrderCode = inItem.OrderCode,
                    Data = rt
                });
                if (obj.Transaction.TransactionTypeId == EnmTransactionType.FirstDeposit)
                {
                    await _iPublishService.ProduceAsync(new CollectDepositTransactionCreatedETO
                    {
                        OrderCode = inItem.OrderCode,
                        Data = rt
                    });
                }
                return rt;
            }
            catch (Exception ex)
            {
                _log.LogError(_CoreName + $".DepositByCard OrderCode {inItem?.OrderCode}: {ex}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData("Message", $"Order {inItem?.OrderCode}: error - " + ex.Message);
                if ((ex is AbpRemoteCallException) || (ex is BusinessException) || (ex is UserFriendlyException))
                    throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem?.OrderCode)
                    .WithData(PaymentCoreErrorMessageKey.Message, ex.Message);
            }
        }

        public async Task<DepositByTransferOutputDtoV2> DepositByTransfer(MaskDepositByTransferInputDtoV2 inItem)
        {
            if (inItem.Transfers == null || !inItem.Transfers.Any()) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                       .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                       .WithData("Detail", ParamTypes.JustData, "Không thấy thông tin Transfer!")
                                                       .WithData("Data", "Thông tin Transfer");
            if (inItem.Transfers.Sum(c => c.Amount ?? 0m) != inItem.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("Transfer", inItem.Transfers);
            if (inItem.Transfers.Any(c => c.Amount.HasValue && c.Amount < 0m)) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                                   .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                                   .WithData("Transfers", ParamTypes.JustData, inItem.Transfers)
                                                                   .WithData("Entity", "Transfers Amount");
            var dup = inItem.Transfers.Where(c => !string.IsNullOrWhiteSpace(c.TransferNum)).GroupBy(c => c.TransferNum).Where(c => c.Count() > 1).Select(c => c.Key).ToList();
            if (dup.Any())
            {
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DUPLICATE_EXCEPTION).WithData("Transfernums", dup);
            }
            AddLogElapsed("CheckTransfernum");
            foreach (var chil in inItem.Transfers.Where(c => !string.IsNullOrEmpty(c.TransferNum)))
            {
                var chkNum = await _transferService.CheckTransferNum(chil.TransferNum );
                if (chkNum) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_TRANSFERNUM_EXISTED).WithData("transfernum", chil.TransferNum);
            }

            try
            {
                var obj = ObjectMapper.Map<MaskDepositByTransferInputDtoV2, DepositCoresInputDtoV2>(inItem);
                obj.Transaction.PaymentRequestId = inItem.PaymentRequestId;
                DepositByTransferOutputDtoV2 rt;
                using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                {
                    try
                    {
                        AddLogElapsed("InsertTransDetail");
                        var trans = await _transactionServiceV2.InsertTransactionWithDetail(obj, true);
                        AddLogElapsed("MapReturn");
                        rt = ObjectMapper.Map<DepositCoresOutputDtoV2, DepositByTransferOutputDtoV2>(trans);
                        await unitOfWork.SaveChangesAsync();

                        //var createTransactionOutput = new CreatePaymentTransactionOutputDtoV2();
                        //createTransactionOutput.Transaction = trans.Transaction;
                        ////publish kafka message
                        //var paymentTransEto = ObjectMapper.Map<CreatePaymentTransactionOutputDtoV2, WithdrawReturnCompletedOutputEto>(createTransactionOutput);
                        //paymentTransEto.PaymentCode = inItem.PaymentCode;
                        //paymentTransEto.PaymentCode = inItem.PaymentCode;
                        //paymentTransEto.Note = "DepositByTransfer";

                        //foreach (var item in trans.Transfers)
                        //{
                        //    paymentTransEto.Transfers = ObjectMapper.Map<TransferFullOutputDto, TransferFullOutputEto>(item);
                        //    await _iPublishService.ProduceAsync(paymentTransEto);
                        //}
                    }
                    catch (Exception)
                    {
                        await unitOfWork.RollbackAsync();
                        throw;
                    }
                }
                await _iPublishService.ProduceAsync(new TransactionCreatedETO
                {
                    OrderCode = inItem.OrderCode,
                    Data = rt
                });
                if (obj.Transaction.TransactionTypeId == EnmTransactionType.FirstDeposit)
                {
                    await _iPublishService.ProduceAsync(new CollectDepositTransactionCreatedETO
                    {
                        OrderCode = inItem.OrderCode,
                        Data = rt
                    });
                }
                return rt;
            }
            catch (Exception ex)
            {
                _log.LogError(_CoreName + $".DepositByTransfer OrderCode {inItem?.OrderCode}: {ex}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData("Message", $"Order {inItem?.OrderCode}: error - " + ex.Message);
                if ((ex is AbpRemoteCallException) || (ex is BusinessException) || (ex is UserFriendlyException))
                    throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem?.OrderCode)
                    .WithData(PaymentCoreErrorMessageKey.Message, ex.Message);
            }
        }

        public async Task<DepositByVoucherOutputDtoV2> DepositByVoucher(MaskDepositByVoucherInputDtoV2 inItem)
        {

            var _VoucherUsedSuccess = false;
            try
            {
                var TotalPayment = inItem.TotalPayment;

                //Check Data
                if (inItem.Transaction.Amount > TotalPayment)
                {
                    inItem.Transaction.Amount = TotalPayment;
                    var AmountRem = inItem.Transaction.Amount;
                    inItem.Vouchers.ForEach(c =>
                    {
                        if (AmountRem > 0m && AmountRem >= c.Amount)
                        {
                            AmountRem -= c.Amount;
                        }
                        else if (AmountRem > 0m && AmountRem < c.Amount)
                        {
                            c.Amount = AmountRem;
                            AmountRem = 0m;
                        }
                        else
                        {
                            c.Amount = 0m;
                        }
                    });
                }
                var obj = ObjectMapper.Map<MaskDepositByVoucherInputDtoV2, DepositCoresInputDtoV2>(inItem);
                //Insert  transaction
                DepositByVoucherOutputDtoV2 rt;
                using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                {
                    try
                    {
                        AddLogElapsed("InsertTransDetail");
                        var trans = await _transactionServiceV2.InsertTransactionWithDetail(obj, true);
                        AddLogElapsed("UseVoucher");
                        //Use Voucher
                        if (obj.Vouchers.Any(c => c.VoucherType == EnmVoucherProvider.Taptap))
                        {
                            var useVoucherTap2 = await _taptapAppService.AddTransaction(new PaymentIntegration.Dto.TapTapAddTransactionInputDto
                            {
                                BillId = inItem.OrderCode,
                                CreatedDateTime = obj.Transaction.TransactionTime.Value.ToString("dd-MM-yyyy HH:mm:ss"),
                                Money = (long)inItem.TotalPayment,
                                CustomerId = obj.CustCode,
                                CustomerName = obj.CustName,
                                CustomerMobile = obj.Phone,
                                StoreId = obj.Transaction.ShopCode,
                                Coupons = obj.Vouchers.Where(c => c.VoucherType == EnmVoucherProvider.Taptap).Select(c => new PaymentIntegration.Dto.TapTapCouponInputDto
                                {
                                    Code = c.Code,
                                    Value = (long)c.Amount.Value
                                }).ToList()
                            });
                            if (!useVoucherTap2.UseVoucherSuccessed) throw new BusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", "Sử dụng Voucher không thành công!").WithData("Data", useVoucherTap2);
                        }

                        if (obj.Vouchers.Any(c => c.VoucherType == EnmVoucherProvider.GotIT))
                        {
                            var _GotITPin = "";
                            var srhPin = await _vendorPinService.GetByVendor((int)EnmPartnerId.Gotit, obj.Transaction.ShopCode);
                            if (srhPin != null) _GotITPin = srhPin.PinCode;
                            var useVoucherGotIT = await _gotITAppService.UseMultipleVoucher(new PaymentIntegration.Dto.GotITUseMultipleVoucherInputDto
                            {
                                Bill_Number = inItem.OrderCode,
                                Code = obj.Vouchers.Where(c => c.VoucherType == EnmVoucherProvider.GotIT).Select(c => c.Code).ToList(),
                                Total_Bill = (long)inItem.TotalPayment,
                                Pin = _GotITPin
                            });
                            if (!useVoucherGotIT.Success) throw new BusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", "Sử dụng Voucher không thành công!").WithData("Data", useVoucherGotIT);
                        }
                        // SECTION: USE VOUCHER LONGCHAU.
                        if (obj.Vouchers.Any(c => c.VoucherType == EnmVoucherProvider.LC))
                        {
                            var voucherList = obj.Vouchers.Select(c => c.Code).ToList();
                            // Call voucher core service to use voucher.
                            var useVCRequest = new LongChauUseVoucherInputDto()
                            {
                                CustomerId = inItem.CustCode,
                                Note = inItem.Note,
                                OrderCode = string.IsNullOrEmpty(inItem.OrderCode) ? "" : inItem.OrderCode,
                                OrderDate = DateTime.Now,
                                PhoneNumber = inItem.Phone,
                                Shop = inItem.ShopCode,
                                VoucherCodes = voucherList,
                                UsedAmount = inItem.TotalPayment,
                                PaymentCode = inItem.PaymentCode,
                            };
                            var useVCLCResult = await _longChauService.UseVoucher(useVCRequest);
                            // Check result.
                            if (!useVCLCResult.IsValid) throw new BusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", "Sử dụng Voucher không thành công!").WithData("Data", string.Join(",", voucherList));
                        }
                        // SECTION: USE VOUCHER VANI.
                        if (obj.Vouchers.Any(c => c.VoucherType == EnmVoucherProvider.Vani))
                        {
                            var voucherList = obj.Vouchers.Select(c => c.Code).ToList();
                            // Call voucher core service to use voucher.
                            var useVCRequest = new LongChauUseVoucherInputDto()
                            {
                                CustomerId = inItem.CustCode,
                                Note = inItem.Note,
                                OrderCode = string.IsNullOrEmpty(inItem.OrderCode) ? "" : inItem.OrderCode,
                                OrderDate = DateTime.Now,
                                PhoneNumber = inItem.Phone,
                                Shop = inItem.ShopCode,
                                VoucherCodes = voucherList,
                                UsedAmount = inItem.TotalPayment,
                                PaymentCode = inItem.PaymentCode,
                                CompanyCode = "Vani"
                            };
                            var useVCLCResult = await _longChauService.UseVoucher(useVCRequest);
                            // Check result.
                            if (!useVCLCResult.IsValid) throw new BusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", "Sử dụng Voucher không thành công!").WithData("Data", string.Join(",", voucherList));
                        }
                        if (obj.Vouchers.Any(c => c.VoucherType == EnmVoucherProvider.UTop))
                        {
                            var voucherList = obj.Vouchers.Select(c => c.Code).ToList();
                            // Call voucher core service to use voucher.
                            var useVCRequest = new LongChauUseVoucherInputDto()
                            {
                                CustomerId = inItem.CustCode,
                                Note = inItem.Note,
                                OrderCode = string.IsNullOrEmpty(inItem.OrderCode) ? "" : inItem.OrderCode,
                                OrderDate = DateTime.Now,
                                PhoneNumber = inItem.Phone,
                                Shop = inItem.ShopCode,
                                VoucherCodes = voucherList,
                                UsedAmount = inItem.TotalPayment,
                                PaymentCode = inItem.PaymentCode,
                                CompanyCode = "Utop"
                            };
                            var useVCLCResult = await _longChauService.UseVoucher(useVCRequest);
                            _log.LogInformation(string.Format("result use Utop: {0}| Request body: {1} ", inItem.PaymentCode, JsonConvert.SerializeObject(useVCLCResult)));

                            // Check result.
                            if (!useVCLCResult.IsValid) throw new BusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", "Sử dụng Voucher không thành công!").WithData("Data", string.Join(",", voucherList));
                        }
                        _VoucherUsedSuccess = true;
                        AddLogElapsed("MapReturn");
                        rt = ObjectMapper.Map<DepositCoresOutputDtoV2, DepositByVoucherOutputDtoV2>(trans);
                        await unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        await unitOfWork.RollbackAsync();
                        throw;
                    }
                }
                rt.VoucherUsedSuccess = _VoucherUsedSuccess;
                await _iPublishService.ProduceAsync(new TransactionCreatedETO
                {
                    OrderCode = inItem.OrderCode,
                    Data = rt
                });
                if (obj.Transaction.TransactionTypeId == EnmTransactionType.FirstDeposit)
                {
                    await _iPublishService.ProduceAsync(new CollectDepositTransactionCreatedETO
                    {
                        OrderCode = inItem.OrderCode,
                        Data = rt
                    });
                }
                return rt;
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.DepositByVoucher PaymentCode {inItem?.PaymentCode}| VoucherUsedSuccess: {_VoucherUsedSuccess}: {ex}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData("Message", $"PaymentCode {inItem?.PaymentCode}: error - " + ex.Message);
                if ((ex is AbpRemoteCallException) || (ex is BusinessException) || (ex is UserFriendlyException))
                    throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.PaymentCode, inItem?.PaymentCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message)
                    .WithData("VoucherUsedSuccess", _VoucherUsedSuccess);
            }
        }

        public async Task<DepositByEWalletOutputDtoV2> DepositByEWallet(MaskDepositByEWalletInputDtoV2 inItem)
        {
            if (inItem.EWallet == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                        .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                        .WithData("Detail", ParamTypes.JustData, "Không thấy thông tin eWallet!")
                                                        .WithData("Data", "Thông tin eWallet");
            if (inItem.EWallet.Amount != inItem.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("Wallet", inItem.EWallet);
            if ((inItem.EWallet.Amount ?? 0m) < 0m) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                             .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                             .WithData("EWalletAmount", ParamTypes.JustData, inItem.EWallet.Amount)
                                                             .WithData("Entity", "eWallet Amount");

            try
            {
                var obj = ObjectMapper.Map<MaskDepositByEWalletInputDtoV2, DepositCoresInputDtoV2>(inItem);
                //Check VNPay, SmartPay
                if (!EwalletAvailabel.ContainsKey(obj.EWallet.TypeWalletId.Value)) throw new BusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData(PaymentCoreErrorMessageKey.MessageDetail, $"Không hỗ trợ ví {Enum.GetName(obj.EWallet.TypeWalletId.Value)}.");

                // check trans.Nếu là smartPay thì kiểm tra history nếu không phải thì checktrans bthg và lấy date của tạo qr
                var ChkPayed = new PaymentIntegration.Dto.CheckTransactionEwalletOutputDto();
                var PayDate = obj.EWallet.CreatedDate;
                var qrhisoty = await _elasticSearchIntergationService.SearchESHistory(inItem.PaymentCode);
                if ((qrhisoty != null && qrhisoty.QrDetail != null && qrhisoty.QrDetail.Count > 0))
                {
                    var data = qrhisoty.QrDetail.Where(x => x.IsPayed == true && x.PaymentRequestCode == inItem.Transaction.PaymentRequestCode).FirstOrDefault();
                    if (data != null)
                    {
                        ChkPayed.IsPayed = true;
                        ChkPayed.PaymentRequestCode = inItem.Transaction != null ? inItem.Transaction.PaymentRequestCode : "";
                        ChkPayed.DebitAmount = data.DebitAmount;
                        ChkPayed.RealAmount = data.RealAmount;
                        PayDate = data.CreatedDate;
                    }
                }
                if (obj.EWallet.TypeWalletId != EnmWalletProvider.Smartpay || ChkPayed.RealAmount == null)
                {
                    ChkPayed = await _eWalletsAppService.CheckTransactionPayed(new PaymentIntegration.Dto.CheckTransactionEwalletInputDto
                    {
                        PayDate = (DateTime)PayDate,
                        PaymentRequestCode = inItem.Transaction.PaymentRequestCode,
                        ShopCode = obj.Transaction.ShopCode,
                        ProviderId = EwalletAvailabel[obj.EWallet.TypeWalletId.Value],
                        Amount = Convert.ToInt64(obj.EWallet.Amount.Value),
                        TransactionType = obj.EWallet.TypeWalletId == EnmWalletProvider.ShopeePay ? (uint)PaymentIntegration.Dto.ShopeePay.ShopeePayTransactionType.Payment : default
                    });
                }
                _log.LogInformation(string.Format("DepositByEWallet: {0}| Request body: {1} ", inItem.PaymentCode, JsonConvert.SerializeObject(ChkPayed)));


                if (ChkPayed == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                        .WithData("ProviderResult", ParamTypes.JustData, ChkPayed)
                        .WithData("ProviderResult", "Provider Result");
                if (!ChkPayed.IsPayed) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData("ProviderResult", ParamTypes.JustData, ChkPayed)
                        .WithData("Message", "Order chưa được thanh toán!");
                var amtPayed = ChkPayed.DebitAmount ?? ChkPayed.RealAmount;
                if (obj.EWallet.Amount != amtPayed) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL)
                          .WithData("DebitAmount", ChkPayed.DebitAmount)
                          .WithData("RealAmount", ChkPayed.RealAmount)
                          .WithData("ProviderResult", ChkPayed)
                          ;
                obj.EWallet.RealAmount = ChkPayed.RealAmount ?? ChkPayed.DebitAmount;
                //Insert  transaction
                DepositByEWalletOutputDtoV2 rt;
                using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                {
                    try
                    {
                        AddLogElapsed("InsertTransDetail");
                        var trans = await _transactionServiceV2.InsertTransactionWithDetail(obj, true);
                        AddLogElapsed("MapReturn");
                        rt = ObjectMapper.Map<DepositCoresOutputDtoV2, DepositByEWalletOutputDtoV2>(trans);
                        await unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        await unitOfWork.RollbackAsync();
                        throw;
                    }
                }
                await _iPublishService.ProduceAsync(new TransactionCreatedETO
                {
                    OrderCode = inItem.OrderCode,
                    Data = rt
                });
                if (obj.Transaction.TransactionTypeId == EnmTransactionType.FirstDeposit)
                {
                    await _iPublishService.ProduceAsync(new CollectDepositTransactionCreatedETO
                    {
                        OrderCode = inItem.OrderCode,
                        Data = rt
                    });
                }
                rt.EWallet.RealAmount = obj.EWallet.RealAmount;
                return rt;
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.DepositByEWallet OrderCode {inItem?.OrderCode}:{ex}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData("Message", $"Order {inItem?.OrderCode}: error - " + ex.Message);
                if ((ex is AbpRemoteCallException) || (ex is BusinessException) || (ex is UserFriendlyException))
                    throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem.OrderCode)
                    .WithData(PaymentCoreErrorMessageKey.Message, ex.Message);
            }
        }

        public async Task<DepositByEWalletOutputDtoV2> DepositByEWalletOnline(MaskDepositByEWalletOnlineInputDtoV2 inItem)
        {
            if (inItem.EWallet == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                        .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                        .WithData("Detail", ParamTypes.JustData, "Không thấy thông tin eWallet!")
                                                        .WithData("Data", "Thông tin eWallet");
            if (inItem.EWallet.Amount != inItem.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("Wallet", inItem.EWallet);
            if ((inItem.EWallet.Amount ?? 0m) < 0m) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                             .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                             .WithData("EWalletAmount", ParamTypes.JustData, inItem.EWallet.Amount)
                                                             .WithData("Entity", "eWallet Amount");



            try
            {
                var obj = ObjectMapper.Map<MaskDepositByEWalletOnlineInputDtoV2, DepositCoresInputDtoV2>(inItem);
                var ord = await InitBeforDeposit(obj.Transaction.PaymentMethodId.Value, obj);
                //Check ví điện tử có hỗ trợ hay ko
                if (!Enum.IsDefined(typeof(EnmWalletProvider), obj.EWallet.TypeWalletId))
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", "Ví điện tử này không được hỗ trợ!");
                //Insert  transaction
                DepositByEWalletOutputDtoV2 rt;
                using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                {
                    try
                    {
                        var trans = await _transactionServiceV2.InsertTransactionWithDetail(obj, true);
                        rt = ObjectMapper.Map<DepositCoresOutputDtoV2, DepositByEWalletOutputDtoV2>(trans);
                        await unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        await unitOfWork.RollbackAsync();
                        throw;
                    }
                }
                await _iPublishService.ProduceAsync(new TransactionCreatedETO
                {
                    OrderCode = inItem.OrderCode,
                    Data = rt
                });
                return rt;
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.DepositByEWalletOnline OrderCode {inItem?.OrderCode}: {ex}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData("Message", $"Order {inItem.OrderCode}: error - " + ex.Message);
                if ((ex is AbpRemoteCallException) || (ex is BusinessException) || (ex is UserFriendlyException))
                    throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem.OrderCode)
                    .WithData(PaymentCoreErrorMessageKey.MessageDetail, ex.Message);
            }
        }

        public async Task<DepositByMultipleVoucherOutputDtoV2> DepositByMultipleVoucher(MaskDepositByVoucherInputDtoV2 inItem)
        {
            var rt = new DepositByMultipleVoucherOutputDtoV2
            {
                Succeeded = new List<DepositByVoucherOutputDtoV2>(),
                Failed = new List<VoucherFailOutputDto>()
            };
            var AmountTransInp = inItem.Transaction.Amount;
            var _AmountDeposited = 0m;
            try
            {
                var chkPayRQ = await _paymentServiceV2.GetByPaymentRequestCode(inItem.Transaction.PaymentRequestCode);
                if (chkPayRQ == null) throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_NOTFOUND)
                                                              .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem.OrderCode)
                                                              .WithData(PaymentCoreErrorMessageKey.PaymentRequestCode, inItem.Transaction.PaymentRequestCode);
                if ((chkPayRQ.Status == null) || (chkPayRQ.Status != EnmPaymentRequestStatus.Confirm)) throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYMENTREQUEST_STATUS)
                                                                .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem.OrderCode)
                                                                .WithData("Status", chkPayRQ?.Status)
                                                                .WithData(PaymentCoreErrorMessageKey.PaymentRequestCode, inItem.Transaction.PaymentRequestCode);
                //inItem.Transaction.PaymentRequestDate = chkPayRQ.PaymentRequestDate;
                inItem.Transaction.PaymentRequestCode = chkPayRQ.PaymentRequestCode;
                foreach (var vc in inItem.Vouchers)
                {
                    try
                    {
                        if (_AmountDeposited >= AmountTransInp) break;
                        var depsted = await _transactionServiceV2.GetSumAmountOfPaymentRequest(inItem.Transaction.PaymentRequestCode, new List<EnmTransactionType> { EnmTransactionType.Recharge }, chkPayRQ.PaymentRequestDate);
                        if (depsted >= chkPayRQ.TotalPayment) break;

                        var depVC = new MaskDepositByVoucherInputDtoV2
                        {
                            OrderCode = inItem.OrderCode,
                            Vouchers = new List<PaymentVoucherInputDtoV2> {
                                ObjectMapper.Map<PaymentVoucherInputDtoV2, PaymentVoucherInputDtoV2>(vc)
                            },
                            Transaction = ObjectMapper.Map<DepositTransactionInputDtoV2, DepositTransactionInputDtoV2>(inItem.Transaction)
                        };
                        depVC.Transaction.Amount = AmountTransInp - _AmountDeposited;
                        if (depVC.Transaction.Amount > depVC.Vouchers[0].Amount) depVC.Transaction.Amount = depVC.Vouchers[0].Amount;

                        var repDepst = await DepositByVoucher(depVC);
                        _AmountDeposited += repDepst.Transaction.Amount.Value;
                        rt.Succeeded.Add(repDepst);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"{_CoreName}.DepositByMultipleVoucher: OrderCode {inItem.OrderCode}-Voucher {vc.Code}: {ex}");
                        rt.Failed.Add(new VoucherFailOutputDto
                        {
                            Code = vc.Code,
                            VoucherType = vc.VoucherType,
                            ErrorMessage = ex.Message,
                            ErrorCode = (ex is IHasErrorCode) ? (ex as IHasErrorCode).Code : null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.DepositByMultipleVoucher|OrderCode {inItem?.OrderCode}| {ex} ");
                rt.Message = ex.Message;
            }
            return rt;
        }

        public async Task<VoucherFullOutputV2Dto> DepositVoucherAll(VoucherRequestDto vouchers)
        {

            var rt = new VoucherFullOutputV2Dto()
            {
                DataSucceeded = new List<SucceededVoucher>(),
                DataFailed = new List<FailedVoucher>(),
            };
            var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(vouchers.PaymentRequestCode);
            var paymentRequestQR = await _paymentRequestRepository.GetByPaymentRequestCode(vouchers.PaymentRequestCode, paymentRequestDate);
            var AmountTransInp = paymentRequestQR.TotalPayment;
            var _AmountDeposited = 0m;

            // Tính tổng tiền của voucher.
            var totalVoucherAmount = vouchers.vouchers.Where(x => x.VoucherDetail.TransactionId == null).Sum(c => c.Transaction.Amount);

            foreach (var item in vouchers.vouchers)
            {
                var DataFailed = new FailedVoucher();
                item.VoucherDetail.CreatedDate = DateTime.Now;
                try
                {
                    if (!string.IsNullOrEmpty(item.VoucherDetail.TransactionId.ToString()))
                    {
                        var itemDataSucceeded = new SucceededVoucher();
                        itemDataSucceeded.Succeeded = item;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                    else
                    {
                        if (_AmountDeposited >= AmountTransInp) break;

                        var voucherRequest = new List<PaymentVoucherInputDtoV2>();
                        var mappdata = ObjectMapper.Map<PaymentVoucherAllDtoV2, PaymentVoucherInputDtoV2>(item.VoucherDetail);
                        voucherRequest.Add(mappdata);

                        var inItem = new MaskDepositByVoucherInputDtoV2();
                        inItem.TotalPayment = paymentRequestQR.TotalPayment;
                        inItem.PaymentRequestId = paymentRequestQR.Id;
                        inItem.PaymentCode = vouchers.PaymentCode;
                        inItem.Phone = vouchers.Phone;
                        inItem.CustCode = vouchers.CustCode;
                        inItem.CustName = vouchers.CustName;
                        inItem.Transaction = ObjectMapper.Map<DepositTransactionDto, DepositTransactionInputDtoV2>(item.Transaction);
                        inItem.Vouchers = voucherRequest;
                        inItem.Transaction.ShopCode = vouchers.ShopCode;
                        inItem.Transaction.PaymentRequestCode = vouchers.PaymentRequestCode;
                        inItem.Transaction.AccountId = vouchers.AccountId;
                        inItem.TotalPayment = paymentRequestQR.TotalPayment;
                        inItem.OrderCode = vouchers.OrderCode;
                        inItem.ShopCode = vouchers.ShopCode;
                        // inItem.Transaction = item.Transaction;

                        //if (_AmountDeposited > 0)
                        //{
                        //    inItem.Transaction.Amount = AmountTransInp - _AmountDeposited;
                        //    inItem.Vouchers[0].Amount = inItem.Transaction.Amount;
                        //}
                        //if (inItem.Transaction.Amount > inItem.Vouchers[0].Amount) inItem.Transaction.Amount = inItem.Vouchers[0].Amount;

                        if (inItem.Vouchers[0].Amount > paymentRequestQR.TotalPayment)
                        {
                            inItem.Vouchers[0].Amount = paymentRequestQR.TotalPayment;
                            inItem.Transaction.Amount = inItem.Vouchers[0].Amount;
                        }
                        else
                        {
                            inItem.Transaction.Amount = (totalVoucherAmount - _AmountDeposited) > inItem.Transaction.Amount
                                                                            ? inItem.Transaction.Amount
                                                                            : ((totalVoucherAmount > AmountTransInp ? AmountTransInp : totalVoucherAmount) - _AmountDeposited);
                            inItem.Vouchers[0].Amount = inItem.Transaction.Amount;
                        }

                        var resultdepositCard = await DepositByVoucher(inItem);
                        _AmountDeposited += resultdepositCard.Transaction.Amount.Value;

                        item.VoucherDetail.TransactionId = resultdepositCard.Transaction.Id;
                        item.Transaction.Amount = inItem.Vouchers[0].Amount;
                        item.VoucherDetail.Amount = inItem.Vouchers[0].Amount;
                        var itemDataSucceeded = new SucceededVoucher();
                        itemDataSucceeded.Succeeded = item;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                }
                catch (Exception ex)
                {
                    DataFailed.Failed = item;
                    if (ex is BusinessException)
                    {
                        DataFailed.Message = CollectMessage((BusinessException)ex);
                    }
                    else if (ex is AbpRemoteCallException)
                    {
                        DataFailed.Message = CollectMessage(null, (AbpRemoteCallException)ex);
                    }
                    else
                    {
                        DataFailed.Message = ex.Message;
                    }
                }
                finally
                {
                    if (DataFailed.Failed != null)
                    {
                        DataFailed.Failed.VoucherDetail.TransactionId = null;
                        rt.DataFailed.Add(DataFailed);
                    }
                }
            };

            return rt;
        }
        public async Task<EWalletDepositFullOutputV2Dto> DepositEWalletAll(eWalletRequestDto eWallet)
        {
            var rt = new EWalletDepositFullOutputV2Dto
            {
                DataSucceeded = new List<SucceededEWallet>(),
                DataFailed = new List<FailedEWallet>()
            };

            foreach (var item in eWallet.eWallet)
            {
                var DataFailed = new FailedEWallet();
                item.EWalletDetail.CreatedDate = DateTime.Now;
                try
                {
                    if (!string.IsNullOrEmpty(item.EWalletDetail.TransactionId.ToString()))
                    {
                        var itemDataSucceeded = new SucceededEWallet();
                        itemDataSucceeded.Succeeded = item;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                    else
                    {
                        var itemDataSucceeded = new SucceededEWallet();
                        var mapEWalletRequest = ObjectMapper.Map<EWalletDepositInputDto, PaymentEWalletDepositInputDtoV2>(item.EWalletDetail);

                        var inItem = new MaskDepositByEWalletInputDtoV2();
                        inItem.PaymentCode = eWallet.PaymentCode;
                        inItem.Phone = eWallet.Phone;
                        inItem.CustCode = eWallet.CustCode;
                        inItem.CustName = eWallet.CustName;
                        inItem.Transaction = ObjectMapper.Map<DepositTransactionDto, DepositTransactionInputDtoV2>(item.Transaction);
                        inItem.EWallet = mapEWalletRequest;
                        inItem.Transaction.ShopCode = eWallet.ShopCode;
                        inItem.Transaction.PaymentRequestCode = eWallet.PaymentRequestCode;
                        inItem.Transaction.AccountId = eWallet.AccountId;
                        // inItem.Transaction = item.Transaction;
                        var resultdepositEWallet = await DepositByEWallet(inItem);
                        item.EWalletDetail.TransactionId = resultdepositEWallet.Transaction.Id;
                        item.EWalletDetail.RealAmount = resultdepositEWallet.EWallet.RealAmount;
                        itemDataSucceeded.Succeeded = item;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                }
                catch (Exception ex)
                {
                    DataFailed.Failed = item;
                    if (ex is BusinessException)
                    {
                        DataFailed.Message = CollectMessage((BusinessException)ex);
                    }
                    else if (ex is AbpRemoteCallException)
                    {
                        DataFailed.Message = ex.Message;
                    }
                    else
                    {
                        DataFailed.Message = ex.Message;
                    }
                }
                finally
                {
                    if (DataFailed.Failed != null)
                    {
                        DataFailed.Failed.EWalletDetail.TransactionId = null;
                        rt.DataFailed.Add(DataFailed);
                    }
                }
            };
            return rt;
        }
        public async Task<EWalletDepositFullOutputV2Dto> DepositEWalletOnlineAll(eWalletRequestDto eWallet)
        {
            var rt = new EWalletDepositFullOutputV2Dto
            {
                DataSucceeded = new List<SucceededEWallet>(),
                DataFailed = new List<FailedEWallet>()
            };
            foreach (var item in eWallet.eWallet)
            {
                var DataFailed = new FailedEWallet();
                item.EWalletDetail.CreatedDate = DateTime.Now;
                try
                {
                    if (!string.IsNullOrEmpty(item.EWalletDetail.TransactionId.ToString()))
                    {
                        var itemDataSucceeded = new SucceededEWallet();
                        itemDataSucceeded.Succeeded = item;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                    else
                    {
                        var itemDataSucceeded = new SucceededEWallet();
                        var mapEWalletRequest = ObjectMapper.Map<EWalletDepositInputDto, PaymentEWalletDepositInputDtoV2>(item.EWalletDetail);

                        var inItem = new MaskDepositByEWalletOnlineInputDtoV2();
                        inItem.PaymentCode = eWallet.PaymentCode;
                        inItem.Phone = eWallet.Phone;
                        inItem.CustCode = eWallet.CustCode;
                        inItem.CustName = eWallet.CustName;
                        inItem.Transaction = ObjectMapper.Map<DepositTransactionDto, DepositTransactionInputDtoV2>(item.Transaction);
                        inItem.EWallet = mapEWalletRequest;
                        inItem.Transaction.ShopCode = eWallet.ShopCode;
                        inItem.Transaction.PaymentRequestCode = eWallet.PaymentRequestCode;
                        inItem.Transaction.AccountId = eWallet.AccountId;

                        var resultdepositEWallet = await DepositByEWalletOnline(inItem);
                        item.EWalletDetail.TransactionId = resultdepositEWallet.Transaction.Id;
                        itemDataSucceeded.Succeeded = item;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                }
                catch (Exception ex)
                {
                    DataFailed.Failed = item;
                    if (ex is BusinessException)
                    {
                        DataFailed.Message = CollectMessage((BusinessException)ex);
                    }
                    else if (ex is AbpRemoteCallException)
                    {
                        DataFailed.Message = ex.Message;
                    }
                    else
                    {
                        DataFailed.Message = ex.Message;
                    }
                }
                finally
                {
                    if (DataFailed.Failed != null)
                    {
                        DataFailed.Failed.EWalletDetail.TransactionId = null;
                        rt.DataFailed.Add(DataFailed);
                    }
                }
            };
            return rt;
        }
        public async Task<TransferFullOutputV2Dto> DepositTransferAll(TransferRequestDto tranfer)
        {
            var rt = new TransferFullOutputV2Dto()
            {
                DataSucceeded = new List<SucceededTransfer>(),
                DataFailed = new List<FailedTransfer>()
            };
            var paymentRQSyncES = new PaymentRequestSyncOutPutDto();
            if (tranfer.tranfer == null)
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_METHOD_INVALID);
            var account = await _accountRepository.GetById(tranfer.AccountId.Value);
            if (account == null)
                //tài khoản có tồn tại không?
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_NOTFOUND);

            Guid paymentRqID = Guid.Parse(tranfer.PaymentRequestId);

            foreach (var item in tranfer.tranfer)
            {
                var DataFailed = new FailedTransfer();
                item.TransferDetail.CreatedDate = DateTime.Now;
                try
                {
                    if (!string.IsNullOrEmpty(item.TransferDetail.TransactionId.ToString()))
                    {
                        var itemDataSucceeded = new SucceededTransfer();
                        itemDataSucceeded.Succeeded = item;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                    else
                    {
                        if (tranfer.TypePayment == EnmPaymentType.Chi)
                        {
                            var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(tranfer.PaymentRequestCode);
                            var resultPaymentRequest = await _paymentRequestRepository.GetByPaymentRequestCode(tranfer.PaymentRequestCode, paymentRequestDate);
                            if (resultPaymentRequest == null)
                            {
                                //kiểm tra paymentRequest có tồn tại orderCode và OrderReturn không?
                                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ORDERRETURNID_PAYMENT_NOTFOUND).WithData("OrderReturnId", resultPaymentRequest.OrderReturnId);
                            }
                            else
                            {
                                paymentRQSyncES = ObjectMapper.Map<PaymentRequest, PaymentRequestSyncOutPutDto>(resultPaymentRequest);
                            }
                            if (resultPaymentRequest.Status == EnmPaymentRequestStatus.Complete)
                                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYEMNT_REQUEST_COMPLETED);

                            if (tranfer.tranfer.FirstOrDefault().Transaction.TransactionTypeId == EnmTransactionType.WithdrawDeposit)
                            {
                                var requestTransaction = new InsertTransactionInputDtoV2
                                {
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = item.TransferDetail.CreatedBy,
                                    AccountId = account.Id,
                                    TransactionTypeId = item.Transaction.TransactionTypeId,
                                    Status = EnmTransactionStatus.Created,
                                    TransactionTime = item.Transaction.TransactionTime,
                                    Amount = item.TransferDetail.Amount,
                                    ShopCode = tranfer.ShopCode,
                                    PaymentMethodId = item.Transaction.PaymentMethodId,
                                    TransactionFee = 0,
                                    Note = "",
                                    AdditionAttributes = "",
                                    PaymentRequestDate = item.Transaction.TransactionTime,
                                    PaymentRequestCode = tranfer.PaymentRequestCode,
                                    PaymentRequestId = paymentRqID,
                                };
                                //tao cashback
                                var paymentCashBackRequest = new CashbackDepositRefundBaseDtoV2()
                                {
                                    PaymentRequestCode = tranfer.PaymentRequestCode,
                                    TotalPayment = (decimal)item.TransferDetail.Amount,
                                    Transaction = requestTransaction,
                                };
                                var extAc = await CreateCashbackTransaction(paymentCashBackRequest, account);
                            }

                            if (tranfer.tranfer.FirstOrDefault().Transaction.TransactionTypeId == EnmTransactionType.Refund)
                            {
                                var createdBy = item.TransferDetail.CreatedBy;
                                var requestTransaction = new InsertTransactionInputDtoV2
                                {
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = createdBy,
                                    AccountId = account.Id,
                                    TransactionTypeId = EnmTransactionType.Refund,
                                    Status = EnmTransactionStatus.Created,
                                    TransactionTime = item.TransferDetail.DateTranfer,
                                    Amount = item.TransferDetail.Amount,
                                    ShopCode = tranfer.ShopCode,
                                    PaymentMethodId = EnmPaymentMethod.Transfer,
                                    TransactionFee = 0,
                                    Note = "",
                                    AdditionAttributes = "",
                                    PaymentRequestCode = tranfer.PaymentRequestCode,
                                    PaymentRequestDate = item.TransferDetail.DateTranfer,
                                    PaymentRequestId = paymentRqID,
                                };
                                //CashBack tiền về ví cho KH (vì ở đây khi đặt cọc sẽ bị thanh toán đơn cọc để hoàn tất cọc nên cần phải rút tiền về)
                                var paymentCashBackRequest = new CashbackDepositRefundBaseDtoV2
                                {
                                    PaymentRequestCode = tranfer.PaymentRequestCode,
                                    TotalPayment = (decimal)item.TransferDetail.Amount,
                                    Transaction = requestTransaction
                                };

                                //insert transaction
                                using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                                {
                                    try
                                    {
                                        //insert cashback
                                        var transactionInput = paymentCashBackRequest.Transaction;
                                        transactionInput.TransactionTypeId = EnmTransactionType.CashBack;
                                        transactionInput.PaymentMethodId = EnmPaymentMethod.Cash;
                                        var transactionOutput = await _transactionServiceV2.InsertTransaction(transactionInput);

                                        //Bắn kafka
                                        var cashbackEto = new CashbackCreatedEto();
                                        var transactionEto = ObjectMapper.Map<TransactionFullOutputDtoV2, TransactionFullOutputEto>(transactionOutput);
                                        cashbackEto.Transaction = transactionEto;
                                        cashbackEto.PaymentRequestCode = paymentCashBackRequest.PaymentRequestCode;
                                        await _iPublishService.ProduceAsync(cashbackEto);
                                        await unitOfWork.SaveChangesAsync();
                                    }
                                    catch (AbpDbConcurrencyException)
                                    {
                                        await unitOfWork.RollbackAsync();
                                        throw;
                                    }
                                }
                            }

                        }

                        var itemDataSucceeded = new SucceededTransfer();
                        var tranferRequest = new List<PaymentTransferInputDtoV2>();
                        var mapTransfersRequest = ObjectMapper.Map<TransferInputDto, PaymentTransferInputDtoV2>(item.TransferDetail);
                        tranferRequest.Add(mapTransfersRequest);

                        var inItem = new MaskDepositByTransferInputDtoV2();
                        inItem.PaymentCode = tranfer.PaymentCode;
                        inItem.Phone = tranfer.Phone;
                        inItem.CustCode = tranfer.CustCode;
                        inItem.CustName = tranfer.CustName;
                        inItem.Transaction = ObjectMapper.Map<DepositTransactionDto, DepositTransactionInputDtoV2>(item.Transaction);
                        inItem.Transfers = tranferRequest;
                        inItem.Transaction.ShopCode = tranfer.ShopCode;
                        inItem.Transaction.PaymentRequestCode = tranfer.PaymentRequestCode;

                        Guid paymentRequestID_temp = Guid.Parse(tranfer.PaymentRequestId);
                        inItem.PaymentRequestId = paymentRequestID_temp;
                        inItem.Transaction.AccountId = account.Id;
                        // inItem.Transaction = item.Transaction;
                        var resultdepositTransfer = await DepositByTransfer(inItem);

                        item.TransferDetail.TransactionId = resultdepositTransfer.Transaction.Id;
                        itemDataSucceeded.Succeeded = item;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                        try
                        {
                            // sync transfer to ES
                            if (resultdepositTransfer != null && tranfer.TypePayment == EnmPaymentType.Chi &&
                                (paymentRQSyncES.TypePayment == EmPaymentRequestType.PaymentCoreRequest ||
                                paymentRQSyncES.TypePayment == EmPaymentRequestType.DepositRefund))
                            {
                                var result = new TransactionDetailTransferOutputDto();

                                var resultTransferAll = new List<TransferSyncAll>();
                                var itemTransferAll = new TransferSyncAll();

                                itemTransferAll = ObjectMapper.Map<List<TransferFullOutputDto>, List<TransferSyncAll>>(resultdepositTransfer.Transfers).FirstOrDefault();
                                itemTransferAll.TransactionTypeId = (int?)resultdepositTransfer.Transaction.TransactionTypeId;
                                resultTransferAll.Add(itemTransferAll);
                                byte isConfirm = 0;
                                if (itemTransferAll.IsConfirm != null)
                                {
                                    isConfirm = itemTransferAll.IsConfirm.Value;
                                }

                                result.TransferAll = resultTransferAll;
                                result.TypePayment = (byte?)tranfer.TypePayment;
                                result.PaymentCode = paymentRQSyncES.PaymentCode;
                                result.StatusFill = (byte?)StatusFill.Filled;
                                result.CreatedDate = DateTime.Now;
                                result.AmountPayment = paymentRQSyncES.TotalPayment;
                                result.IsConfirmTransfer = isConfirm;
                                result.CreateDatePayment = paymentRQSyncES.CreatedDate;
                                result.TransactionId = itemTransferAll.TransactionId.ToString();
                                result.ShopCode = resultdepositTransfer.Transaction.ShopCode;
                                result.PaymentMethodId = (byte?)EnmPaymentMethod.Transfer;
                                var syncES = await _elasticSearchService.SyncDataESTransfer(tranfer.PaymentCode, result);

                                if (syncES == true)
                                {
                                    _log.LogInformation(string.Format("SyncTrue: {0}| Request body: {1} ", null, JsonConvert.SerializeObject(result)));
                                }
                                else
                                {
                                    _log.LogInformation(string.Format("SyncFail: {0}| Request body: {1} ", null, JsonConvert.SerializeObject(result)));
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            _log.LogError($"{_CoreName}.DepositTransferAll: {ex}| Request body: {JsonConvert.SerializeObject(tranfer)} ");
                        }
                    }
                }
                catch (Exception ex)
                {
                    DataFailed.Failed = item;
                    if (ex is BusinessException)
                    {
                        DataFailed.Message = CollectMessage((BusinessException)ex);
                    }
                    else if (ex is AbpRemoteCallException)
                    {
                        DataFailed.Message = ex.Message;
                    }
                    else
                    {
                        DataFailed.Message = ex.Message;
                    }
                }
                finally
                {
                    if (DataFailed.Failed != null)
                    {
                        DataFailed.Failed.TransferDetail.TransactionId = null;
                        rt.DataFailed.Add(DataFailed);
                    }
                }
            };

            return rt;
        }
        public async Task<CardFullOutputV2Dto> DepositCardsAll(CardRequestDto cards)
        {
            var rt = new CardFullOutputV2Dto
            {
                DataSucceeded = new List<SucceededCard>(),
                DataFailed = new List<FailedCard>(),
            };

            foreach (CardDepositAllInputDto item in cards.cards)
            {
                var DataFailed = new FailedCard();
                item.CardsDetail.CreatedDate = DateTime.Now;
                try
                {
                    if (!string.IsNullOrEmpty(item.CardsDetail.TransactionId.ToString()))
                    {
                        var itemDataSucceeded = new SucceededCard();
                        itemDataSucceeded.Succeeded = item;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                    else
                    {
                        var request = new MaskDepositByCardInputDtoV2();
                        var detail = new List<PaymentCardInputDtoV2>();
                        var dataMap = ObjectMapper.Map<PaymentCardAllAInputDtoV2, PaymentCardInputDtoV2>(item.CardsDetail);
                        detail.Add(dataMap);
                        request.Cards = detail;
                        request.Transaction = ObjectMapper.Map<DepositTransactionDto, DepositTransactionInputDtoV2>(item.Transaction);
                        request.Transaction.ShopCode = cards.ShopCode;
                        request.Transaction.PaymentRequestCode = cards.PaymentRequestCode;
                        request.Transaction.AccountId = cards.AccountId;
                        request.PaymentCode = cards.PaymentCode;
                        var resultdepositCard = await DepositByCard(request);
                        item.CardsDetail.TransactionId = resultdepositCard.Transaction.Id;
                        var itemDataSucceeded = new SucceededCard();
                        itemDataSucceeded.Succeeded = item;
                        rt.DataSucceeded.Add(itemDataSucceeded);
                    }
                }
                catch (Exception ex)
                {
                    DataFailed.Failed = item;
                    if (ex is BusinessException)
                    {
                        DataFailed.Message = CollectMessage((BusinessException)ex);
                    }
                    else if (ex is AbpRemoteCallException)
                    {
                        DataFailed.Message = ex.Message;
                    }
                    else
                    {
                        DataFailed.Message = ex.Message;
                    }
                }
                finally
                {
                    if (DataFailed.Failed != null)
                    {
                        DataFailed.Failed.CardsDetail.TransactionId = null;
                        rt.DataFailed.Add(DataFailed);
                    }
                }
            };

            return rt;
        }
        public async Task<DepositAllOutDto> DepositSyntheticAll(DepositAllInputDto inItem)
        {
            // hàm này ở gateway
            var response = new DepositAllOutDto();
            decimal TotalPaymentRequest = 0;
            bool isTransfers = false;
            int isQROrTransfer = 0;
            bool isVoucher = false;
            int remainQROrTransfer = 0;
            bool isCreate = false;

            if (inItem.Cash != null && inItem.Cash.Count > 0)
                TotalPaymentRequest = TotalPaymentRequest + (decimal)inItem.Cash.WhereIf(inItem.Cash != null, x => x.TransactionId == null).Sum(x => x.Amount);
            if (inItem.EWalletAll != null && inItem.EWalletAll.Count > 0)
            {
                isQROrTransfer = 1;
                TotalPaymentRequest = TotalPaymentRequest + (decimal)inItem.EWalletAll
                    .WhereIf(inItem.EWalletAll != null, x => x.EWalletDetail.TransactionId == null)
                    .Sum(x => x.Transaction.Amount);
            }
            if (inItem.EWalletOnlineAll != null && inItem.EWalletOnlineAll.Count > 0)
            {
                isQROrTransfer = 1;
                TotalPaymentRequest = TotalPaymentRequest + (decimal)inItem.EWalletOnlineAll
                    .WhereIf(inItem.EWalletOnlineAll != null, x => x.EWalletDetail.TransactionId == null)
                    .Sum(x => x.Transaction.Amount);
            }
            if (inItem.CardsAll != null && inItem.CardsAll.Count > 0)
                TotalPaymentRequest = TotalPaymentRequest + (decimal)inItem.CardsAll
                     .WhereIf(inItem.CardsAll != null, x => x.CardsDetail.TransactionId == null)
                    .Sum(x => x.Transaction.Amount);
            if (inItem.CODAll != null && inItem.CODAll.Count > 0)
                TotalPaymentRequest = TotalPaymentRequest + (decimal)inItem.CODAll
                     .WhereIf(inItem.CODAll != null, x => x.CODetail.TransactionId == null)
                    .Sum(x => x.Transaction.Amount);
            if (inItem.TransfersAll != null && inItem.TransfersAll.Count > 0)
            {
                var isLCDConfirm = inItem.TransfersAll.Any(x => x.TransferDetail.IsConfirm == EnmTransferIsConfirm.AdvanceTransfer);
                isQROrTransfer = 1;

                if (isLCDConfirm)
                {
                    isTransfers = true;
                };
                TotalPaymentRequest = TotalPaymentRequest + (decimal)inItem.TransfersAll
                     .WhereIf(inItem.TransfersAll != null, x => x.TransferDetail.TransactionId == null)
                    .Sum(x => x.Transaction.Amount);
            }
            if (inItem.VouchersAll != null && inItem.VouchersAll.Count > 0)
            {
                isVoucher = true;
                TotalPaymentRequest = TotalPaymentRequest + (decimal)inItem.VouchersAll
                     .WhereIf(inItem.VouchersAll != null, x => x.VoucherDetail.TransactionId == null)
                    .Sum(x => x.Transaction.Amount);
            }
            if (inItem.DebtSaleAll != null)
                TotalPaymentRequest = TotalPaymentRequest + (decimal)inItem.DebtSaleAll
                     .WhereIf(inItem.DebtSaleAll != null, x => x.Debit.TransactionId == null)
                    .Sum(x => x.Transaction.Amount);
            var totalDeviant = TotalPaymentRequest - inItem.TotalPayment;

            if (isQROrTransfer == 1 && !string.IsNullOrEmpty(inItem.PaymentRequestCode))
            {
                var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(inItem.PaymentRequestCode);
                var paymentRequestQR = await _paymentRequestRepository.GetByPaymentRequestCode(inItem.PaymentRequestCode, paymentRequestDate);

                //tổng số tiền deposit lớn hơn số tiền của paymentRequest được truyền vào 
                //và tổng số tiền deposit phải nhỏ hơn hoặc bằng số tiền của Payment.Total
                var remainTransfer = inItem.TotalPayment - TotalPaymentRequest;
                if (TotalPaymentRequest > paymentRequestQR.TotalPayment && (TotalPaymentRequest <= inItem.TotalPayment
                    || (isTransfers && Math.Abs((int)remainTransfer) <= _paymentOptions.TotalPaymentMaxDeviant)))
                {
                    remainQROrTransfer = (int)(TotalPaymentRequest - paymentRequestQR.TotalPayment);
                    isCreate = true;
                }
            }

            if (!isVoucher && ((!isTransfers && TotalPaymentRequest > inItem.TotalPayment) ||
                (isTransfers && TotalPaymentRequest > inItem.TotalPayment && Math.Abs(totalDeviant.Value) > _paymentOptions.TotalPaymentMaxDeviant)))
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_CHECKSUM_TD_TS_NOTMATCH);


            //verify paymentCode đã đủ chưa 


            var requestCreate = new CreateRequestDepositAllInputDto
            {
                PaymentRequestCode = inItem.PaymentRequestCode,
                PaymentCode = inItem.PaymentCode,
                PaymentSource = inItem.PaymentSource,
                TotalPaymentRequest = isCreate ? remainQROrTransfer : TotalPaymentRequest,
                isTransfer = isTransfers,
                isCreate = isCreate,
                isVoucher = isVoucher,
                PaymentRequestType = inItem.PaymentRequestType,
            };

            var resultCreateRequest = await CreateRequestDepositAll(requestCreate);
            //nếu tồn tại paymentRequestCode rồi mà isCreate == True thì tạo paymentRequest mới 
            var PaymentRequestCodeV1 = inItem.PaymentRequestCode;
            inItem.PaymentRequestCode = resultCreateRequest.PaymentRequestCodeV2 ?? resultCreateRequest.PaymentRequestCode;
            //infor payment
            response.CreatedBy = resultCreateRequest.CreatedBy;
            response.PaymentDate = resultCreateRequest.PaymentDate;
            response.UpdatedBy = resultCreateRequest.UpdatedBy;
            response.CreatedDate = resultCreateRequest.CreatedDate;

            //deposit voucher 
            if (inItem.VouchersAll != null && inItem.VouchersAll.Count > 0)
            {

                var requestCods = new VoucherRequestDto();
                requestCods = ObjectMapper.Map<DepositAllInputDto, VoucherRequestDto>(inItem);
                requestCods.vouchers = inItem.VouchersAll;
                var rt = await DepositVoucherAll(requestCods);
                response.Voucher = rt;
                if (rt.DataFailed.Count > 0)
                    response.Status = false;
            }
            //deposit ví điện tử
            if (inItem.EWalletAll != null)
            {
                var requestewallet = new eWalletRequestDto();
                requestewallet = ObjectMapper.Map<DepositAllInputDto, eWalletRequestDto>(inItem);
                requestewallet.eWallet = inItem.EWalletAll;
                if (isCreate)
                {
                    requestewallet.PaymentRequestCode = PaymentRequestCodeV1;
                }
                var rt = await DepositEWalletAll(requestewallet);
                response.EWallet = rt;
                if (rt.DataFailed.Count > 0)
                    response.Status = false;
            }
            //deposit ví điện tử online
            if (inItem.EWalletOnlineAll != null)
            {
                var requestewalletOnline = new eWalletRequestDto();
                requestewalletOnline = ObjectMapper.Map<DepositAllInputDto, eWalletRequestDto>(inItem);
                requestewalletOnline.eWallet = inItem.EWalletOnlineAll;
                if (isCreate)
                {
                    requestewalletOnline.PaymentRequestCode = PaymentRequestCodeV1;
                }
                var rt = await DepositEWalletOnlineAll(requestewalletOnline);
                response.EWalletOnline = rt;
                if (rt.DataFailed.Count > 0)
                    response.Status = false;
            }
            //deposit Transfer
            if (inItem.TransfersAll != null)
            {
                var requesttransfer = new TransferRequestDto();
                requesttransfer = ObjectMapper.Map<DepositAllInputDto, TransferRequestDto>(inItem);
                requesttransfer.tranfer = inItem.TransfersAll;
                if (isCreate)
                {
                    requesttransfer.PaymentRequestCode = PaymentRequestCodeV1;
                }
                var rt = await DepositTransferAll(requesttransfer);
                response.Transfer = rt;
                if (rt.DataFailed.Count > 0)
                    response.Status = false;
            }
            //deposit Thẻ
            if (inItem.CardsAll != null)
            {
                var requestCods = new CardRequestDto();
                requestCods = ObjectMapper.Map<DepositAllInputDto, CardRequestDto>(inItem);
                requestCods.cards = inItem.CardsAll;

                var rt = await DepositCardsAll(requestCods);
                response.Cards = rt;
                if (rt.DataFailed.Count > 0)
                    response.Status = false;
            }
            //deposit Tiền mặt
            if (inItem.Cash != null)
            {
                var rt = await DepositCashAll(inItem);
                response.Cash = rt;
                if (rt.DataFailed.Count > 0)
                    response.Status = false;
            }
            //deposit COD
            if (inItem.CODAll != null && inItem.CODAll.Count > 0)
            {
                var requestCods = new CodRequestDto();
                requestCods = ObjectMapper.Map<DepositAllInputDto, CodRequestDto>(inItem);
                requestCods.cods = inItem.CODAll;

                var rt = await DepositCodsAll(requestCods);
                response.CODs = rt;
                if (rt.DataFailed.Count > 0)
                    response.Status = false;
            }
            //deposit debt-sale

            if (inItem.DebtSaleAll != null)
            {
                if (inItem.DebtSaleAll != null)
                {
                    var rt = await DepositDebtSaleAll(inItem);
                    response.DebtSale = rt;
                    if (rt.DataFailed.Count > 0)
                        response.Status = false;
                }
            }

            var inputFinish = new VerifyTDTSInputDto
            {
                PaymentRequestCode = inItem.PaymentRequestCode,
                PaymentRequestCodeV2 = resultCreateRequest.PaymentRequestCodeV2,
                TotalV2 = requestCreate.TotalPaymentRequest,
                Total = inItem.TotalPayment,
                PaymentCode = inItem.PaymentCode,
                TypePaymentRequest = inItem.PaymentRequestType,
                Transaction = new CreatePaymentTransactionInputDtoV2
                {
                    AccountId = inItem.AccountId,
                    PaymentRequestCode = inItem.PaymentRequestCode,
                    Transaction = new PaymentTransactionDtoV2
                    {
                        ShopCode = inItem.ShopCode,
                        TransactionFee = 0,
                        TransactionTime = DateTime.Now,
                        Note = Decription.PaymentTransaction,
                        CreatedBy = inItem.PaymentSource != null ? inItem.PaymentSource.CreatedBy : "",

                    }
                }
            };
            var result = await FinishTSTD(inputFinish);
            response.IsPayment = result.IsPayment;
            response.IsPaymentRequest = result.IsPaymentRequest;
            response.RemainingAmount = result.RemainingAmount;
            response.PaymentCode = inItem.PaymentCode;
            response.ShopCode = inItem.ShopCode;
            //sync ES 
            var requestMapES = ObjectMapper.Map<DepositAllOutDto, MapESDepositDto>(response);
            requestMapES.CustCode = inItem.CustCode;
            requestMapES.TotalFinal = result.TotalFinal;
            requestMapES.Type = resultCreateRequest.TypePayment;
            var FinishES = await MapDataSyncES(requestMapES);
            return response;
        }
        public async Task<bool> MapDataSyncES(MapESDepositDto response)
        {
            try
            {
                var dataES = await _elasticSearchServiceV2.GetdocESByPaymentCode(response.PaymentCode);
                var requestES = ObjectMapper.Map<DepositAllOutDto, DepositAllDto>(response);
                requestES.CustomerId = String.IsNullOrEmpty(response.CustCode) ? null : Guid.Parse(response.CustCode);
                requestES.ShopCode = response.ShopCode;
                requestES.Total = response.TotalFinal;
                requestES.Type = response.Type;

                requestES.RemainingAmount = response.RemainingAmount;

                if (!(response.PaymentSourceId != null && response.PaymentSourceId.FirstOrDefault() != null))
                {
                    requestES.PaymentSource = dataES.PaymentSource;
                }
                var detail = new Detail();
                if (response.Cash != null && response.Cash.DataSucceeded.Count > 0)
                {
                    var esMapCash = new List<Cash>();
                    if (dataES != null && dataES.Detail != null && dataES.Detail.Cash != null && dataES.Detail.Cash.Count > 0)
                    {
                        esMapCash.AddRange(dataES.Detail.Cash);
                    }
                    foreach (var item in response.Cash.DataSucceeded)
                    {
                        if (item.Succeeded.TransactionId != null &&
                            ((esMapCash.Count > 0 && esMapCash.Where(x => x.TransactionId == item.Succeeded.TransactionId.ToString()).Count() == 0) || esMapCash.Count == 0))
                        {
                            var dataMap = ObjectMapper.Map<DepositCashOutPutDto, Cash>(item.Succeeded);
                            dataMap.PaymentRequestCode = response.PaymentRequestCode;
                            esMapCash.Add(dataMap);
                        }
                    }

                    detail.Cash = esMapCash;
                };
                if (response.EWallet != null && response.EWallet.DataSucceeded.Count > 0)
                {
                    var esMapEWallet = new List<EWalletAll>();
                    if (dataES != null && dataES.Detail != null && dataES.Detail.EWalletAll != null && dataES.Detail.EWalletAll.Count > 0)
                    {
                        esMapEWallet.AddRange(dataES.Detail.EWalletAll);
                    }
                    foreach (var item in response.EWallet.DataSucceeded)
                    {
                        if (item.Succeeded.EWalletDetail.TransactionId != null &&

                            ((esMapEWallet.Count > 0 && esMapEWallet.Where(x => x.EWalletDetail.TransactionId == item.Succeeded.EWalletDetail.TransactionId.ToString()).Count() == 0) || esMapEWallet.Count == 0))

                        {
                            var dataMap = ObjectMapper.Map<EWalletDepositAllDto, EWalletAll>(item.Succeeded);
                            dataMap.PaymentRequestCode = !String.IsNullOrEmpty(response.PaymentRequestCodeV2) ? response.PaymentRequestCodeV2 : response.PaymentRequestCode;
                            esMapEWallet.Add(dataMap);
                        }
                    }
                    detail.EWalletAll = esMapEWallet;
                };
                if (response.EWalletOnline != null && response.EWalletOnline.DataSucceeded.Count > 0)
                {
                    var esMapEWalletOnline = new List<EWalletOnlineAll>();
                    if (dataES != null && dataES.Detail != null && dataES.Detail.EWalletOnlineAll != null && dataES.Detail.EWalletOnlineAll.Count > 0)
                    {
                        esMapEWalletOnline.AddRange(dataES.Detail.EWalletOnlineAll);
                    }
                    foreach (var item in response.EWalletOnline.DataSucceeded)
                    {
                        if (item.Succeeded.EWalletDetail.TransactionId != null &&
                        ((esMapEWalletOnline.Count > 0 && esMapEWalletOnline.Where(x => x.EWalletDetail.TransactionId == item.Succeeded.EWalletDetail.TransactionId.ToString()).Count() == 0) || esMapEWalletOnline.Count == 0))

                        {
                            var dataMap = ObjectMapper.Map<EWalletDepositAllDto, EWalletOnlineAll>(item.Succeeded);
                            dataMap.PaymentRequestCode = !String.IsNullOrEmpty(response.PaymentRequestCodeV2) ? response.PaymentRequestCodeV2 : response.PaymentRequestCode;
                            esMapEWalletOnline.Add(dataMap);
                        }
                    }
                    detail.EWalletOnlineAll = esMapEWalletOnline;
                };
                if (response.Cards != null && response.Cards.DataSucceeded.Count > 0)
                {
                    var esMapCard = new List<CardsAll>();
                    if (dataES != null && dataES.Detail != null && dataES.Detail.CardsAll != null && dataES.Detail.CardsAll.Count > 0)
                    {
                        esMapCard.AddRange(dataES.Detail.CardsAll);
                    }
                    foreach (var item in response.Cards.DataSucceeded)
                    {
                        if (item.Succeeded.CardsDetail.TransactionId != null &&
                        ((esMapCard.Count > 0 && esMapCard.Where(x => x.CardsDetail.TransactionId == item.Succeeded.CardsDetail.TransactionId.ToString()).Count() == 0) || esMapCard.Count == 0))
                        {
                            var dataMap = ObjectMapper.Map<CardDepositAllDto, CardsAll>(item.Succeeded);
                            dataMap.PaymentRequestCode = response.PaymentRequestCode;
                            esMapCard.Add(dataMap);
                        }
                    }
                    detail.CardsAll = esMapCard;
                };
                if (response.CODs != null && response.CODs.DataSucceeded.Count > 0)
                {
                    var esMapCOD = new List<CodAll>();
                    if (dataES != null && dataES.Detail != null && dataES.Detail.CodAll != null && dataES.Detail.CodAll.Count > 0)
                    {
                        esMapCOD.AddRange(dataES.Detail.CodAll);
                    }
                    foreach (var item in response.CODs.DataSucceeded)
                    {
                        if (item.Succeeded.CODetail.TransactionId != null &&
                         ((esMapCOD.Count > 0 && esMapCOD.Where(x => x.CoDetail.TransactionId == item.Succeeded.CODetail.TransactionId.ToString()).Count() == 0) || esMapCOD.Count == 0))

                        {
                            var dataMap = ObjectMapper.Map<CODDepositAllDto, CodAll>(item.Succeeded);
                            dataMap.PaymentRequestCode = response.PaymentRequestCode;
                            esMapCOD.Add(dataMap);
                        }
                    }
                    detail.CodAll = esMapCOD;
                };
                if (response.Transfer != null && response.Transfer.DataSucceeded.Count > 0)
                {
                    var esMaptransfer = new List<TransfersAll>();
                    if (dataES != null && dataES.Detail != null && dataES.Detail.TransfersAll != null && dataES.Detail.TransfersAll.Count > 0)
                    {
                        esMaptransfer.AddRange(dataES.Detail.TransfersAll);
                    }
                    foreach (var item in response.Transfer.DataSucceeded)
                    {
                        if (item.Succeeded.TransferDetail.TransactionId != null &&
                        ((esMaptransfer.Count > 0 && esMaptransfer.Where(x => x.TransferDetail.TransactionId == item.Succeeded.TransferDetail.TransactionId).Count() == 0) || esMaptransfer.Count == 0))

                        {
                            var dataMap = ObjectMapper.Map<TransferInputDepositAllDto, TransfersAll>(item.Succeeded);
                            dataMap.PaymentRequestCode = !String.IsNullOrEmpty(response.PaymentRequestCodeV2) ? response.PaymentRequestCodeV2 : response.PaymentRequestCode;
                            esMaptransfer.Add(dataMap);
                        }
                    }
                    detail.TransfersAll = esMaptransfer;
                };
                if (response.Voucher != null && response.Voucher.DataSucceeded.Count > 0)
                {
                    var esMapVoucher = new List<VouchersAll>();
                    if (dataES != null && dataES.Detail != null && dataES.Detail.VouchersAll != null && dataES.Detail.VouchersAll.Count > 0)
                    {
                        esMapVoucher.AddRange(dataES.Detail.VouchersAll);
                    }
                    foreach (var item in response.Voucher.DataSucceeded)
                    {
                        if (item.Succeeded.VoucherDetail.TransactionId != null &&
                          ((esMapVoucher.Count > 0 && esMapVoucher.Where(x => x.VoucherDetail.TransactionId == item.Succeeded.VoucherDetail.TransactionId.ToString()).Count() == 0) || esMapVoucher.Count == 0))

                        {
                            var dataMap = ObjectMapper.Map<VoucherDepositAllDto, VouchersAll>(item.Succeeded);
                            if (response.IsCreate)
                            {
                                dataMap.PaymentRequestCode = response.PaymentRequestCodeV2;
                            }
                            else
                            {
                                dataMap.PaymentRequestCode = response.PaymentRequestCode;
                            }
                            esMapVoucher.Add(dataMap);
                        }
                    }
                    detail.VouchersAll = esMapVoucher;
                };
                if (response.DebtSale != null && response.DebtSale.DataSucceeded != null)
                {
                    var esMapDebtSale = new List<DebtSaleAll>();
                    if (dataES != null && dataES.Detail != null && dataES.Detail.DebtSaleAll != null && dataES.Detail.DebtSaleAll.Count > 0)
                    {
                        esMapDebtSale.AddRange(dataES.Detail.DebtSaleAll);
                    }
                    foreach (var item in response.DebtSale.DataSucceeded)
                    {
                        if (item.Succeeded.Debit.TransactionId != null &&
                         ((esMapDebtSale.Count > 0 && esMapDebtSale.Where(x => x.Debit.TransactionId == item.Succeeded.Debit.TransactionId.ToString()).Count() == 0) || esMapDebtSale.Count == 0))

                        {
                            var dataMap = ObjectMapper.Map<DepositDebtSaleOutPutDto, DebtSaleAll>(item.Succeeded);
                            dataMap.PaymentRequestCode = response.PaymentRequestCode;
                            esMapDebtSale.Add(dataMap);
                        }
                    }
                    detail.DebtSaleAll = esMapDebtSale;
                };
                requestES.Detail = detail;
                var finalES = await _elasticSearchServiceV2.SyncESDepositAllAsync(response.PaymentCode, requestES, "MapDataSyncES");
                _log.LogInformation(string.Format("Bắn kafka lc.payment.paymentRequest.completed : {0}| requestES: {1} ", response.PaymentCode, JsonConvert.SerializeObject(requestES)));

                ///Bắn kafka lc.payment.paymentRequest.completed
                if (requestES.IsPayment)
                {
                    if (requestES.PaymentSource != null && requestES.PaymentSource.Count > 0 &&
                        requestES.PaymentSource.Any(x => x.Type != EnmPaymentSourceCode.AR))
                    {
                        var paymentRequestCompletedOutputDto = new PaymentRequestCompletedOutputDtoV2
                        {
                            OrderCode = null,
                            PaymentCode = requestES.PaymentCode,
                            PaymentRequestCode = null
                        };

                        var transactionKaka = new TransactionFullOutputDtoV2()
                        {
                            Id = new Guid(),
                            Amount = requestES.Total,
                            TransactionTypeId = EnmTransactionType.CollectMoney
                        };

                        if (requestES.Type == EnmPaymentType.Thu)
                            transactionKaka.TransactionTypeId = EnmTransactionType.CollectMoney;
                        else
                            transactionKaka.TransactionTypeId = EnmTransactionType.Pay;

                        var detailTransactionKaka = new List<TransactionFullOutputDtoV2>();
                        if (requestES.Detail != null)
                        {
                            if (requestES.Detail.CodAll != null)
                            {
                                foreach (var itemCOD in requestES.Detail.CodAll)
                                {
                                    var detailCod = new TransactionFullOutputDtoV2();

                                    var transctionId = new Guid();
                                    Guid.TryParse(itemCOD.CoDetail.TransactionId, out transctionId);
                                    detailCod.Id = transctionId;
                                    detailCod.PaymentMethodId = EnmPaymentMethod.COD;
                                    detailCod.Amount = itemCOD.CoDetail?.Amount ?? 0;

                                    detailTransactionKaka.Add(detailCod);
                                }
                            }
                            if (requestES.Detail.CardsAll != null)
                            {
                                var detailCard = new TransactionFullOutputDtoV2();
                                foreach (var itemCard in requestES.Detail.CardsAll)
                                {
                                    var transctionId = new Guid();
                                    Guid.TryParse(itemCard.CardsDetail.TransactionId, out transctionId);
                                    detailCard.Id = transctionId;
                                    detailCard.PaymentMethodId = EnmPaymentMethod.Card;
                                    detailCard.Amount = itemCard.CardsDetail?.Amount ?? 0;
                                    detailTransactionKaka.Add(detailCard);
                                }
                            }
                        }

                        paymentRequestCompletedOutputDto.Transaction = transactionKaka;
                        paymentRequestCompletedOutputDto.DepositTransactions = detailTransactionKaka;
                        var paymentRequestEto = ObjectMapper.Map<PaymentRequestCompletedOutputDtoV2, PaymentRequestCompletedOutputEto>(paymentRequestCompletedOutputDto);
                        await _iPublishService.ProduceAsync(paymentRequestEto);
                    }
                }

                return finalES;
            }
            catch (Exception ex)
            {
                _log.LogInformation($"{_CoreName}.MapDataSyncES: PaymentCode {response.PaymentCode}: {ex}");
                return false;
            }

        }
        private string CollectMessage(BusinessException bx = null, AbpRemoteCallException ex = null)
        {
            var code = ex != null ? ex.Code : bx.Code;
            var data = ex != null ? ex.Data : bx.Data;

            var codeNamespace = code.Split(':')[0];
            var localizationResourceType = LocalizationOptions.ErrorCodeNamespaceMappings.GetOrDefault(codeNamespace);
            if (localizationResourceType == null)
            {
                return ex.Message;
            }
            var stringLocalizer = StringLocalizerFactory.Create(localizationResourceType);
            var localizedString = stringLocalizer[code];
            var localizedValue = localizedString.Value;

            if (data != null && data.Count > 0)
            {
                foreach (var key in data.Keys)
                {
                    localizedValue = localizedValue.Replace("{" + key + "}", data[key]?.ToString());
                }
                if (data.Values.Count > 0)
                {
                    localizedValue += ",";
                    foreach (var item in data.Values)
                    {
                        var dataValue = item.ToString();
                        if (ex != null)
                        {
                            var checkjson = dataValue.Contains("{");
                            if (checkjson)
                            {
                                var response = JsonConvert.DeserializeObject<MessesError>(dataValue);
                                dataValue = response.Message;
                            }
                        }
                        var checkbool = dataValue.ToLower();
                        if (!localizedValue.Contains(dataValue) && (checkbool != "true" && checkbool != "false"))
                        {
                            localizedValue += " " + dataValue;
                        }
                    }
                }
            }
            return localizedValue;
        }
        private async Task<Account> CreateCashbackTransaction(CashbackDepositRefundBaseDtoV2 cashbackDto, Account account)
        {
            if (!(cashbackDto.Transaction.TransactionTypeId == EnmTransactionType.WithdrawDeposit || cashbackDto.Transaction.TransactionTypeId == EnmTransactionType.Refund))
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

        public async Task<MigrationPaymentrequestCodeOutnputDto> MigrationPaymentRequestCode(MigrationPaymentrequestCodeInnputDto inputMigration)
        {
            try
            {
                var result = new MigrationPaymentrequestCodeOutnputDto();
                var paymentCodeSyncES = new MigrationSuscessDto();
                if (inputMigration.StartDate != null && inputMigration != null)
                {
                    var listPaymentCode = await _paymentServiceV2.GetListPaymentCodeByDateTime(ObjectMapper.Map<MigrationPaymentrequestCodeInnputDto, InputPaymentDtoV2>(inputMigration));
                    if (listPaymentCode.PaymentCode.Count > 0 && listPaymentCode != null)
                    {
                        foreach (var paymentCode in listPaymentCode.PaymentCode)
                        {
                            var resultSyncDepositAll = await _elasticSearchServiceV2.GetHistoryAll(paymentCode);
                            if (resultSyncDepositAll != null)
                            {
                                paymentCodeSyncES.PaymentCode = new List<string>() { paymentCode };
                                result.migrationSuscessDto = paymentCodeSyncES;
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(inputMigration.PaymentCode))
                {
                    var resultSyncDepositAll = await _elasticSearchServiceV2.GetHistoryAll(inputMigration.PaymentCode);

                    if (resultSyncDepositAll.PaymentCode != null)
                    {
                        paymentCodeSyncES.PaymentCode = new List<string>() { inputMigration.PaymentCode };
                        result.migrationSuscessDto = paymentCodeSyncES;
                    }
                }
                return result;
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
        public async Task<VoucherFullOutputV2Dto> VerifyVoucher(VoucherRequestDto request)
        {
            try
            {
                var rt = new VoucherFullOutputV2Dto()
                {
                    DataSucceeded = new List<SucceededVoucher>(),
                    DataFailed = new List<FailedVoucher>(),
                };

                //danh sách voucher thành công
                var vchers = new List<VoucherInputDto>();
                //verify voucher GotIT
                var gotIt = request.vouchers.Where(x => x.VoucherDetail.VoucherType == EnmVoucherProvider.GotIT);
                if (gotIt.Count() > 0)
                {
                    foreach (var chil in gotIt)
                    {
                        try
                        {
                            if (chil.VoucherDetail.Amount != chil.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("Voucher", chil.VoucherDetail);
                            if (chil.VoucherDetail.Amount < 0) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                         .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, request.PaymentCode)
                                                         .WithData("Vouchers", ParamTypes.JustData, chil.VoucherDetail)
                                                         .WithData("Entity", "Vouchers Amount");
                            var chkGotIT = await _gotITAppService.Validation(new PaymentIntegration.Dto.GotITValidationVoucherInputDto
                            {
                                Code = chil.VoucherDetail.Code,
                                Storecode = request.ShopCode
                            });
                            if (!chkGotIT.IsValidated)
                                throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                      .WithData("voucherCode", chil.VoucherDetail.Code)
                                      .WithData("Voucher", chkGotIT);
                            if (!string.IsNullOrEmpty(chkGotIT.ExpiryDate))
                            {
                                var expD = DateTime.ParseExact(chkGotIT.ExpiryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                if (DateTime.Now.Date > expD) throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                        .WithData("voucherCode", chil.VoucherDetail.Code)
                                        .WithData("Voucher", chkGotIT)
                                        .WithData("Detail", $"Voucher {chil.VoucherDetail.Code} hết hạn sử dụng ({expD:dd/MM/yyyy})");
                            }
                            if (Convert.ToDecimal(chkGotIT.Product.Value) < chil.VoucherDetail.Amount)
                            {
                                throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                                           .WithData("voucherCode", chil.VoucherDetail.Code)
                                                           .WithData("Detail", $"Giá trị Input vượt quá Giá trị cho phép của Voucher.");
                            }
                            vchers.Add(new VoucherInputDto
                            {
                                VoucherType = chil.VoucherDetail.VoucherType,
                                Code = chil.VoucherDetail.Code,
                                Amount = Convert.ToDecimal(chkGotIT.Product.Value)
                            });
                            var itemDataSucceeded = new SucceededVoucher();
                            itemDataSucceeded.Succeeded = chil;

                            rt.DataSucceeded.Add(itemDataSucceeded);
                        }
                        catch (Exception ex)
                        {
                            var DataFailed = new FailedVoucher();

                            DataFailed.Failed = chil;
                            if (ex is BusinessException)
                            {
                                DataFailed.Message = CollectMessage((BusinessException)ex);
                            }
                            if (ex is AbpRemoteCallException)
                            {
                                DataFailed.Message = (ex as AbpRemoteCallException).Message;
                            }
                            rt.DataFailed.Add(DataFailed);
                        }

                    }
                }

                //verify voucher TapTap

                var tapTap = request.vouchers.Where(x => x.VoucherDetail.VoucherType == EnmVoucherProvider.Taptap);
                if (tapTap.Count() > 0)
                {
                    //tạm thời taptap chưa sài 
                    var TotalPayment = tapTap.Sum(x => x.VoucherDetail.Amount);//SỐ tiền của PaymentRequest 

                    foreach (var chil in tapTap)
                    {
                        try
                        {

                            if (chil.VoucherDetail.Amount != chil.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("Voucher", chil.VoucherDetail);
                            if (chil.VoucherDetail.Amount < 0) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                        .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, request.PaymentCode)
                                                        .WithData("Vouchers", ParamTypes.JustData, chil.VoucherDetail)
                                                        .WithData("Entity", "Vouchers Amount");
                            var code = chil.VoucherDetail.Code;
                            var chkTaptap = await _taptapAppService.Validation(new PaymentIntegration.Dto.TapTapValidationVoucherInputDto
                            {
                                Code = code,
                                Storecode = request.ShopCode,
                            });
                            if (!chkTaptap.IsValidated) throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                   .WithData("voucherCode", code)
                                   .WithData("Voucher", chkTaptap);
                            DateTime? startdate = string.IsNullOrEmpty(chkTaptap.Data.startDate) ? null : DateTime.Parse(chkTaptap.Data.startDate, CultureInfo.InvariantCulture).ToUniversalTime();//Taptap return DateTime+7 nhưng format theo kiểu UTC :(((
                            DateTime? enddate = string.IsNullOrEmpty(chkTaptap.Data.endDate) ? null : DateTime.Parse(chkTaptap.Data.endDate, CultureInfo.InvariantCulture).ToUniversalTime();//Taptap return DateTime+7 nhưng format theo kiểu UTC :(((
                            if ((startdate.HasValue && DateTime.Now < startdate) || (enddate.HasValue && DateTime.Now > enddate)) throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                      .WithData("voucherCode", code)
                                      .WithData("Voucher", chkTaptap)
                                      .WithData("Detail", $"Voucher {code} chỉ sử dụng được trong thời gian {startdate:dd/MM/yyyy HH:mm:ss} đến {enddate:dd/MM/yyyy HH:mm:ss}.");
                            if ((chkTaptap.Data.requireMinAmount > 0 && TotalPayment < chkTaptap.Data.requireMinAmount) || (chkTaptap.Data.requireMaxAmount > 0 && TotalPayment > chkTaptap.Data.requireMaxAmount))
                                throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                       .WithData("voucherCode", code)
                                       .WithData("Voucher", chkTaptap)
                                       .WithData("Detail", $"Giá trị đơn hàng không nằm trong requireMinAmount và requireMinAmount");
                            var amount = chkTaptap.Data.valueType == 2 ? Math.Round((decimal)chkTaptap.Data.percent * (decimal)TotalPayment * 0.01m, 6) : chkTaptap.Data.value;

                            if (amount < chil.VoucherDetail.Amount)
                            {
                                throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                                          .WithData("voucherCode", chil.VoucherDetail.Code)
                                                          .WithData("Detail", $"Giá trị Input vượt quá Giá trị cho phép của Voucher.");
                            }
                            var nit = new VoucherInputDto
                            {
                                VoucherType = chil.VoucherDetail.VoucherType,
                                Code = code,
                                Amount = amount
                            };

                            if (chkTaptap.Data.valueMax > 0 && nit.Amount > chkTaptap.Data.valueMax) nit.Amount = chkTaptap.Data.valueMax;
                            vchers.Add(nit);
                            var itemDataSucceeded = new SucceededVoucher();
                            itemDataSucceeded.Succeeded = chil;
                            rt.DataSucceeded.Add(itemDataSucceeded);
                        }
                        catch (Exception ex)
                        {
                            var DataFailed = new FailedVoucher();

                            DataFailed.Failed = chil;
                            if (ex is BusinessException)
                            {
                                DataFailed.Message = CollectMessage((BusinessException)ex);
                            }
                            if (ex is AbpRemoteCallException)
                            {
                                DataFailed.Message = (ex as AbpRemoteCallException).Message;
                            }
                            rt.DataFailed.Add(DataFailed);
                        }

                    }
                }

                //verify voucher LC
                var lc = request.vouchers.Where(x => x.VoucherDetail.VoucherType == EnmVoucherProvider.LC);

                if (lc.Count() > 0)
                {
                    foreach (var chil in lc)
                    {
                        try
                        {
                            if (chil.VoucherDetail.Amount != chil.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("Voucher", chil.VoucherDetail);
                            if (chil.VoucherDetail.Amount < 0) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                         .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, request.PaymentCode)
                                                         .WithData("Vouchers", ParamTypes.JustData, chil.VoucherDetail)
                                                         .WithData("Entity", "Vouchers Amount");
                            var verifyVoucherLCInput = new LongChauVerifyVoucherInputDto()
                            {
                                PhoneNumber = request.Phone,
                                VoucherCodes = lc.Where(x => x.VoucherDetail.Code == chil.VoucherDetail.Code).Select(c => c.VoucherDetail.Code).ToList(),
                                OrderCode = request.OrderCode,
                                PaymentCode = request.PaymentCode,
                                ShopCode = request.ShopCode,
                                CompanyCode = null
                            };
                            var test = JsonConvert.SerializeObject(verifyVoucherLCInput);
                            // Call PaymentIntergration service verify voucher.
                            var chkLC = await _longChauService.VerifyVoucher(verifyVoucherLCInput);

                            // Check success validation.
                            if (!chkLC.IsValid || !chkLC.IsValidated)
                                throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_INVALID)
                                    .WithData("voucherCode", string.Join(",", chkLC.VoucherDetails.Select(c => c.SeriesCode)))
                                    .WithData("Vouchers", chkLC);
                            // Check voucher status.
                            if (chkLC.VoucherDetails.Any(c => c.Status == 1))
                                throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_USED)
                                          .WithData("voucherCode", string.Join(",", chkLC.VoucherDetails.Where(c => c.Status == 1).Select(c => c.SeriesCode)))
                                          .WithData("Vouchers", chkLC)
                                          .WithData("Detail", $"Vouchers đã được sử dụng.");

                            if (chkLC.VoucherDetails.FirstOrDefault().Price < chil.VoucherDetail.Amount)
                            {
                                throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                                         .WithData("voucherCode", chil.VoucherDetail.Code)
                                                         .WithData("Detail", $"Giá trị Input vượt quá Giá trị cho phép của Voucher.");
                            }
                            vchers.AddRange(chkLC.VoucherDetails.Select(c => new VoucherInputDto
                            {
                                VoucherType = EnmVoucherProvider.LC,
                                Code = c.SeriesCode,
                                Amount = c.Price
                            }));
                            var itemDataSucceeded = new SucceededVoucher();

                            itemDataSucceeded.Succeeded = chil;
                            var getName = chkLC.VoucherDetails.FirstOrDefault();
                            itemDataSucceeded.Succeeded.VoucherDetail.Name = getName == null ? itemDataSucceeded.Succeeded.VoucherDetail.Name : getName.VoucherName;

                            rt.DataSucceeded.Add(itemDataSucceeded);

                        }
                        catch (Exception ex)
                        {
                            var DataFailed = new FailedVoucher();

                            DataFailed.Failed = chil;
                            if (ex is BusinessException)
                            {
                                DataFailed.Message = CollectMessage((BusinessException)ex);
                            }
                            if (ex is AbpRemoteCallException)
                            {
                                DataFailed.Message = (ex as AbpRemoteCallException).Message;
                            }
                            rt.DataFailed.Add(DataFailed);
                        }

                    }
                }
                //verify lc 
                var vani = request.vouchers.Where(x => x.VoucherDetail.VoucherType == EnmVoucherProvider.Vani);

                // SECTION: VALIDATE VOUCHER VANI.
                if (vani.Count() > 0)
                {
                    AddLogElapsed("VerifyVC_VANI");
                    foreach (var chil in vani)
                    {
                        try
                        {
                            var verifyVoucherLCInput = new LongChauVerifyVoucherInputDto()
                            {
                                PhoneNumber = request.Phone,
                                VoucherCodes = vani.Where(x => x.VoucherDetail.Code == chil.VoucherDetail.Code).Select(c => c.VoucherDetail.Code).ToList(),
                                OrderCode = request.OrderCode,
                                PaymentCode = request.PaymentCode,
                                ShopCode = request.ShopCode,
                                CompanyCode = "Vani"
                            };

                            // Call PaymentIntergration service verify voucher.
                            var chkLC = await _longChauService.VerifyVoucher(verifyVoucherLCInput);

                            AddLogElapsed("EndVerifyVC_VANI");
                            // Check success validation.
                            if (!chkLC.IsValid || !chkLC.IsValidated)
                                throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_INVALID)
                                    .WithData("voucherCode", string.Join(",", chkLC.VoucherDetails.Select(c => c.SeriesCode)))
                                    .WithData("Vouchers", chkLC);
                            // Check voucher status.
                            if (chkLC.VoucherDetails.Any(c => c.Status == 1))
                                throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_USED)
                                          .WithData("voucherCode", string.Join(",", chkLC.VoucherDetails.Where(c => c.Status == 1).Select(c => c.SeriesCode)))
                                          .WithData("Vouchers", chkLC)
                                          .WithData("Detail", $"Vouchers đã được sử dụng.");

                            vchers.AddRange(chkLC.VoucherDetails.Select(c => new VoucherInputDto
                            {
                                VoucherType = EnmVoucherProvider.Vani,
                                Code = c.SeriesCode,
                                Amount = c.Price
                            }));
                            var itemDataSucceeded = new SucceededVoucher();

                            itemDataSucceeded.Succeeded = chil;
                            var getName = chkLC.VoucherDetails.FirstOrDefault();
                            itemDataSucceeded.Succeeded.VoucherDetail.Name = getName == null ? itemDataSucceeded.Succeeded.VoucherDetail.Name : getName.VoucherName;
                            rt.DataSucceeded.Add(itemDataSucceeded);
                        }
                        catch (Exception ex)
                        {
                            var DataFailed = new FailedVoucher();

                            DataFailed.Failed = chil;
                            if (ex is BusinessException)
                            {
                                DataFailed.Message = CollectMessage((BusinessException)ex);
                            }
                            if (ex is AbpRemoteCallException)
                            {
                                DataFailed.Message = (ex as AbpRemoteCallException).Message;
                            }
                            rt.DataFailed.Add(DataFailed);
                        }
                    }
                }

                //verify voucher Utop
                var uTop = request.vouchers.Where(x => x.VoucherDetail.VoucherType == EnmVoucherProvider.UTop);

                if (uTop.Count() > 0)
                {
                    foreach (var chil in uTop)
                    {
                        try
                        {
                            var verifyVoucherLCInput = new LongChauVerifyVoucherInputDto()
                            {
                                PhoneNumber = request.Phone,
                                VoucherCodes = uTop.Where(x => x.VoucherDetail.Code == chil.VoucherDetail.Code).Select(c => c.VoucherDetail.Code).ToList(),
                                OrderCode = request.OrderCode,
                                PaymentCode = request.PaymentCode,
                                ShopCode = request.ShopCode,
                                CompanyCode = "Utop"
                            };

                            // Call PaymentIntergration service verify voucher.
                            var chkLC = await _longChauService.VerifyVoucher(verifyVoucherLCInput);

                            AddLogElapsed("verify Utop");
                            // Check success validation.
                            if (!chkLC.IsValid || !chkLC.IsValidated)
                                throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_INVALID)
                                    .WithData("voucherCode", string.Join(",", chkLC.VoucherDetails.Select(c => c.SeriesCode)))
                                    .WithData("Vouchers", chkLC);
                            // Check voucher status.
                            if (chkLC.VoucherDetails.Any(c => c.Status == 1))
                                throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_USED)
                                          .WithData("voucherCode", string.Join(",", chkLC.VoucherDetails.Where(c => c.Status == 1).Select(c => c.SeriesCode)))
                                          .WithData("Vouchers", chkLC)
                                          .WithData("Detail", $"Vouchers đã được sử dụng.");

                            vchers.AddRange(chkLC.VoucherDetails.Select(c => new VoucherInputDto
                            {
                                VoucherType = EnmVoucherProvider.UTop,
                                Code = c.SeriesCode,
                                Amount = c.Price
                            }));
                            var itemDataSucceeded = new SucceededVoucher();

                            itemDataSucceeded.Succeeded = chil;
                            var getName = chkLC.VoucherDetails.FirstOrDefault();
                            itemDataSucceeded.Succeeded.VoucherDetail.Name = getName == null ? itemDataSucceeded.Succeeded.VoucherDetail.Name : getName.VoucherName;
                            rt.DataSucceeded.Add(itemDataSucceeded);
                        }
                        catch (Exception ex)
                        {
                            var DataFailed = new FailedVoucher();

                            DataFailed.Failed = chil;
                            if (ex is BusinessException)
                            {
                                DataFailed.Message = CollectMessage((BusinessException)ex);
                            }
                            if (ex is AbpRemoteCallException)
                            {
                                DataFailed.Message = (ex as AbpRemoteCallException).Message;
                            }
                            rt.DataFailed.Add(DataFailed);
                        }
                    }
                }
                return rt;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }
    }
}
