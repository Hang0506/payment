using Amazon.S3;
using Amazon.S3.Model;
using FRTTMO.PaymentCore.Common;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Dto.v2;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Kafka.Eto;
using FRTTMO.PaymentCore.Kafka.Eto.v2;
using FRTTMO.PaymentCore.Kafka.Interface;
using FRTTMO.PaymentCore.Options;
using FRTTMO.PaymentCore.Repositories;
using FRTTMO.PaymentIntegration.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;
using static FRTTMO.PaymentCore.Common.EnumType;
using TaskStatus = System.Threading.Tasks.TaskStatus;

namespace FRTTMO.PaymentCore.Services.v2
{
    public class ElasticSearchServiceV2 : PaymentCoreAppService, ITransientDependency, IElasticSearchServiceV2
    {
        private readonly ILogger<ElasticSearchServiceV2> _log;
        private readonly IElasticClient _elasticClient;
        private readonly IPublishService<BaseETO> _iPublishService;
        private readonly string _indexPaymentFinal;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly ITransactionServiceV2 _transactionServiceV2;
        private readonly IAccountRepository _accountRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITransferService _transferService;
        private readonly ICardService _cardService;
        private readonly IEWalletDepositService _eWalletDepositService;
        private readonly IVoucherService _voucherService;
        private readonly ICODService _codService;
        private readonly IDebitService _debitService;
        private readonly AWSOptions _configAws;
        private readonly IAmazonS3 _awsS3;
        private readonly IPaymentRedisService _paymentRedisService;
        private readonly IGenerateServiceV2 _generateServiceV2;
        private readonly IPaymentRedisService _paymentRedis;
        public ElasticSearchServiceV2(
            ILogger<ElasticSearchServiceV2> log,
            IElasticClient elasticClient,
            IPublishService<BaseETO> iPublishService,
            IConfiguration configuration,
            IPaymentRequestRepository paymentRequestRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            ITransactionServiceV2 transactionServiceV2,
            IAccountRepository accountRepository,
            IPaymentRepository paymentRepository,
            ITransferService transferService,
            ICardService cardService,
            IEWalletDepositService eWalletDepositService,
            IVoucherService voucherService,
            ICODService codService,
            IDebitService debitService,
            IOptions<AWSOptions> configAws,
            IAmazonS3 awsS3,
            IPaymentRedisService paymentRedisService,
            IGenerateServiceV2 generateServiceV2,
            IPaymentRedisService paymentRedis

            ) : base()
        {
            _log = log;
            _elasticClient = elasticClient;
            _iPublishService = iPublishService;
            _indexPaymentFinal = configuration.GetSection("LongChauElasticsearch:Indices:PaymentDepositAll").Value;
            _paymentRequestRepository = paymentRequestRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _transactionServiceV2 = transactionServiceV2;
            _accountRepository = accountRepository;
            _paymentRepository = paymentRepository;
            _transferService = transferService;
            _cardService = cardService;
            _eWalletDepositService = eWalletDepositService;
            _voucherService = voucherService;
            _codService = codService;
            _debitService = debitService;
            _configAws = configAws.Value;
            _awsS3 = awsS3;
            _paymentRedisService = paymentRedisService;
            _generateServiceV2 = generateServiceV2;
            _paymentRedis = paymentRedis;
        }

        public async Task<object> AddSyncData(object result, Dictionary<string, object> listSync)
        {
            Type t = result.GetType();
            foreach (KeyValuePair<string, object> kvp in listSync)
            {
                foreach (PropertyInfo pi in t.GetProperties())
                {
                    if (pi.Name == kvp.Key)
                    {
                        pi.SetValue(result, kvp.Value, null);
                        break;
                    }
                }
            }
            return await Task.FromResult(result);
        }
        //private async Task<bool> UpdateAsyncBulk(DepositAllIndex traffics)
        //{
        //    _log.LogInformation("ElasticSearchService-UpdateAsyncBulk-{Traffics}", JsonConvert.SerializeObject(traffics));
        //    var response = await _elasticClient.UpdateAsync<DepositAllIndex>(traffics.id, u => u.Doc(traffics).Index(_indexPaymentFinal));
        //    _log.LogInformation("ElasticSearchService-UpdateAsyncBulk-{Response}", JsonConvert.SerializeObject(response));
        //    if (!response.IsValid)
        //    {
        //        _log.LogError(string.Format("Failed to index document {0}: {1}", response.Id, response.ServerError));
        //    }
        //    return true;
        //}
        //private async Task<bool> SaveProductBulk(DepositAllIndex traffics)
        //{
        //    try
        //    {
        //        var response = await _elasticClient.IndexAsync(traffics, descriptor => descriptor.Index(_indexPaymentFinal));
        //        if (!response.IsValid)
        //        {
        //            _log.LogError(string.Format("Failed to index document {0}: {1}", response.Id, response.ServerError));

        //            return false;
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.LogError(ex, "Sync data Elastic Search: {listSync}", new Dictionary<string, object>());
        //        return false;
        //    }
        //}

        public async Task<bool> SyncESDepositAllAsync(string paymentCode, DepositAllDto data = null, string insertFrom = "")
        {
            try
            {
                var dataKafka = ObjectMapper.Map<DepositAllDto, DepositAllEto>(data);
                dataKafka.InsertFrom = insertFrom;
                await _iPublishService.ProduceAsync(dataKafka);
                //sync payment to Redis
                var inputSyncRedis = new PaymentRedisDto();

                inputSyncRedis.Items = new List<PaymentRedisDetailDto>() { ObjectMapper.Map<DepositAllDto, PaymentRedisDetailDto>(data) };
                inputSyncRedis.CreateDate = DateTime.Now;

                await _paymentRedisService.SyncPaymentCoreToRedis(inputSyncRedis);
                return true;

            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Sync data Elastic Search: {listSync}", new Dictionary<string, object>());
                return false;
            }
        }
        public async Task<DepositAllDto> GetHistoryAll(string paymentCode)
        {
            #region variable
            var resultDeposit = new DepositAllDto();

            #endregion
            var payRQ = await _paymentRequestRepository.GetListByPaymentCode(paymentCode);
            var list = payRQ.Where(x => x.Status != EnmPaymentRequestStatus.Cancel).ToList();
            var paymentRequestES = payRQ.Where(x => x.Status != EnmPaymentRequestStatus.Cancel).FirstOrDefault();

            if (payRQ.Count != 0)
            {
                var account = new Account();
                var detail = new Dto.v2.Detail();
                var listCash = new List<Dto.v2.Cash>();
                var listTransfer = new List<Dto.v2.TransfersAll>();
                var listCards = new List<Dto.v2.CardsAll>();
                var listEWalletAll = new List<Dto.v2.EWalletAll>();
                var listEWalletOnlineAll = new List<Dto.v2.EWalletOnlineAll>();

                var listCodAll = new List<Dto.v2.CodAll>();
                var listVouchersAll = new List<Dto.v2.VouchersAll>();
                var listDebtSaleAll = new List<Dto.v2.DebtSaleAll>();
                var listQrHistory = new List<QrHistory>();
                var transactionDeposit = new Dto.v2.TransactionDeposit();
                var debit = new Dto.v2.Debit();
                var voucherDetail = new Dto.v2.VoucherDetailDeposit();
                var codetail = new Dto.v2.CoDetail();
                var eWalletdetail = new Dto.v2.EWalletDetail();
                var cardsdetail = new Dto.v2.CardsDetail();
                var Transfersdetail = new Dto.v2.TransferDetailDeposit();
                string paymentrequestCodeHistory = string.Empty;
                #region Header DataDepositAll
                paymentrequestCodeHistory = list.FirstOrDefault() != null ? list.FirstOrDefault().PaymentRequestCode
                   : (paymentRequestES != null ? paymentRequestES.PaymentRequestCode : null);
                var paymentDate = _generateServiceV2.GetPaymentRequestDate(paymentCode);
                var payment = await _paymentRepository.Get(paymentCode, paymentDate);
                if (payment != null)
                {
                    var ListPaymentSource = await _paymentTransactionRepository.GetListByPaymentCode(paymentCode);
                    resultDeposit.IsPayment = payment.Status == EnmPaymentStatus.Complete ? true : false;
                    resultDeposit.PaymentSource = ObjectMapper.Map<List<PaymentSource>, List<PaymentSourceId>>(ListPaymentSource);
                    resultDeposit.Type = payment.Type;
                    resultDeposit.PaymentDate = payment.PaymentDate;
                    resultDeposit.CreatedDate = payment.CreatedDate;
                    resultDeposit.CreatedBy = payment.CreatedBy;
                    resultDeposit.UpdatedBy = payment.ModifiedBy;
                    resultDeposit.Status = true;
                    resultDeposit.Total = payment.Total;
                    resultDeposit.PaymentCode = paymentCode;
                    #endregion
                    foreach (var itemPaymentRequest in list)
                    {
                        #region Detail Methods Deposit

                        var sumRechargeAmount = payRQ.Where(x => x.Status == EnmPaymentRequestStatus.Complete).Sum(x => x.TotalPayment);
                        var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(itemPaymentRequest.PaymentRequestCode);
                        var trans = await _transactionServiceV2.GetByPaymentRequestCode(itemPaymentRequest.PaymentRequestCode, paymentRequestDate);
                        if (trans.Count == 0)
                        {
                            break;
                        }
                        account = await _accountRepository.GetById((Guid)trans.FirstOrDefault().AccountId);
                        resultDeposit.PaymentCode = paymentCode;
                        resultDeposit.PaymentRequestCode = itemPaymentRequest.Status == EnmPaymentRequestStatus.Confirm
                            && itemPaymentRequest.TypePayment == EmPaymentRequestType.PaymentCoreRequest ? itemPaymentRequest.PaymentRequestCode : paymentrequestCodeHistory;

                        resultDeposit.RemainingAmount = (sumRechargeAmount <= itemPaymentRequest.TotalPayment) ? (itemPaymentRequest.TotalPayment - sumRechargeAmount) : 0;
                        resultDeposit.ShopCode = trans.FirstOrDefault().ShopCode;
                        var listtranId = trans.Select(x => x.Id).ToList();

                        foreach (var itemTranc in trans.ToList())
                        {
                            if (itemTranc.PaymentMethodId == EnmPaymentMethod.Cash)
                            {
                                var cash = ObjectMapper.Map<TransactionFullOutputDtoV2, Dto.v2.Cash>(itemTranc);
                                listCash.Add(cash);
                                detail.Cash = listCash;
                                resultDeposit.Detail = detail;
                            }
                            //deposit Transfer
                            else if (itemTranc.PaymentMethodId == EnmPaymentMethod.Transfer)
                            {
                                var transferDtos = await _transferService.GetByTransactionId(itemTranc.Id);
                                foreach (var itemtransfers in transferDtos)
                                {
                                    var transfersall = new Dto.v2.TransfersAll();
                                    transactionDeposit = ObjectMapper.Map<TransferFullOutputDto, Dto.v2.TransactionDeposit>(itemtransfers);
                                    Transfersdetail = ObjectMapper.Map<TransferFullOutputDto, Dto.v2.TransferDetailDeposit>(itemtransfers);
                                    transfersall.TransferDetail = Transfersdetail;
                                    transfersall.Transaction = transactionDeposit;
                                    transfersall.Transaction.PaymentMethodId = EnmPaymentMethod.Transfer;
                                    transfersall.Transaction.TransactionTypeId = itemTranc.TransactionTypeId;
                                    transfersall.Transaction.Note = itemTranc.Note;
                                    transfersall.Transaction.TransactionTime = itemTranc.TransactionTime;
                                    transfersall.Transaction.TransactionFee = itemTranc.TransactionFee;
                                    transfersall.PaymentRequestCode = itemPaymentRequest.PaymentRequestCode;

                                    listTransfer.Add(transfersall);
                                }
                                detail.TransfersAll = listTransfer;
                                resultDeposit.Detail = detail;
                            }
                            //deposit Thẻ
                            else if (itemTranc.PaymentMethodId == EnmPaymentMethod.Card)
                            {
                                var cardDtos = await _cardService.GetByTransactionId(itemTranc.Id);
                                foreach (var itemcard in cardDtos)
                                {
                                    var cardall = new Dto.v2.CardsAll();
                                    transactionDeposit = ObjectMapper.Map<CardFullOutputDto, Dto.v2.TransactionDeposit>(itemcard);
                                    cardsdetail = ObjectMapper.Map<CardFullOutputDto, Dto.v2.CardsDetail>(itemcard);
                                    cardall.CardsDetail = cardsdetail;
                                    cardall.Transaction = transactionDeposit;
                                    cardall.Transaction.PaymentMethodId = EnmPaymentMethod.Card;
                                    cardall.Transaction.TransactionTypeId = itemTranc.TransactionTypeId;
                                    cardall.Transaction.Note = itemTranc.Note;
                                    cardall.Transaction.TransactionTime = itemTranc.TransactionTime;
                                    cardall.Transaction.TransactionFee = itemTranc.TransactionFee;
                                    cardall.PaymentRequestCode = itemPaymentRequest.PaymentRequestCode;
                                    listCards.Add(cardall);
                                }
                                detail.CardsAll = listCards;
                                resultDeposit.Detail = detail;
                            }
                            //deposit ví điện tử
                            else if (itemTranc.PaymentMethodId == EnmPaymentMethod.Wallet)
                            {

                                var eWalletDtos = await _eWalletDepositService.GetByTransactionId(itemTranc.Id);
                                foreach (var itemeWalletAll in eWalletDtos)
                                {
                                    var eWalletAll = new Dto.v2.EWalletAll();
                                    transactionDeposit = ObjectMapper.Map<EWalletDepositFullOutputDto, Dto.v2.TransactionDeposit>(itemeWalletAll);
                                    eWalletdetail = ObjectMapper.Map<EWalletDepositFullOutputDto, Dto.v2.EWalletDetail>(itemeWalletAll);
                                    eWalletAll.EWalletDetail = eWalletdetail;
                                    eWalletAll.Transaction = transactionDeposit;
                                    eWalletAll.Transaction.PaymentMethodId = EnmPaymentMethod.Wallet;
                                    eWalletAll.Transaction.TransactionTypeId = itemTranc.TransactionTypeId;
                                    eWalletAll.Transaction.Note = itemTranc.Note;
                                    eWalletAll.Transaction.TransactionTime = itemTranc.TransactionTime;
                                    eWalletAll.Transaction.TransactionFee = itemTranc.TransactionFee;
                                    eWalletAll.PaymentRequestCode = itemPaymentRequest.PaymentRequestCode;
                                    listEWalletAll.Add(eWalletAll);
                                }
                                detail.EWalletAll = listEWalletAll;
                                resultDeposit.Detail = detail;
                            }
                            //deposit COD
                            else if (itemTranc.PaymentMethodId == EnmPaymentMethod.COD)
                            {
                                var codDtos = await _codService.GetByTransactionId(itemTranc.Id);
                                foreach (var itemCod in codDtos)
                                {
                                    var codall = new Dto.v2.CodAll();
                                    transactionDeposit = ObjectMapper.Map<CODFullOutputDto, Dto.v2.TransactionDeposit>(itemCod);
                                    codetail = ObjectMapper.Map<CODFullOutputDto, Dto.v2.CoDetail>(itemCod);
                                    codall.CoDetail = codetail;
                                    codall.Transaction = transactionDeposit;
                                    codall.Transaction.PaymentMethodId = EnmPaymentMethod.COD;
                                    codall.Transaction.TransactionTypeId = itemTranc.TransactionTypeId;
                                    codall.Transaction.Note = itemTranc.Note;
                                    codall.Transaction.TransactionTime = itemTranc.TransactionTime;
                                    codall.Transaction.TransactionFee = itemTranc.TransactionFee;
                                    codall.PaymentRequestCode = itemPaymentRequest.PaymentRequestCode;
                                    listCodAll.Add(codall);
                                }
                                detail.CodAll = listCodAll;
                                resultDeposit.Detail = detail;
                            }
                            //deposit Voucher
                            else if (itemTranc.PaymentMethodId == EnmPaymentMethod.Voucher)
                            {
                                var voucherDtos = await _voucherService.GetByTransactionId(itemTranc.Id);
                                foreach (var itemVoucher in voucherDtos)
                                {
                                    var vouchersAll = new Dto.v2.VouchersAll();
                                    transactionDeposit = ObjectMapper.Map<VoucherFullOutputDto, Dto.v2.TransactionDeposit>(itemVoucher);
                                    voucherDetail = ObjectMapper.Map<VoucherFullOutputDto, Dto.v2.VoucherDetailDeposit>(itemVoucher);
                                    vouchersAll.VoucherDetail = voucherDetail;
                                    vouchersAll.Transaction = transactionDeposit;
                                    vouchersAll.Transaction.PaymentMethodId = EnmPaymentMethod.Voucher;
                                    vouchersAll.Transaction.TransactionTypeId = itemTranc.TransactionTypeId;
                                    vouchersAll.Transaction.Note = itemTranc.Note;
                                    vouchersAll.Transaction.TransactionTime = itemTranc.TransactionTime;
                                    vouchersAll.Transaction.TransactionFee = itemTranc.TransactionFee;
                                    vouchersAll.PaymentRequestCode = itemPaymentRequest.PaymentRequestCode;
                                    listVouchersAll.Add(vouchersAll);
                                }
                                detail.VouchersAll = listVouchersAll;
                                resultDeposit.Detail = detail;
                            }
                            //deposit Bán Nợ
                            else if (itemTranc.PaymentMethodId == EnmPaymentMethod.DebtSale)
                            {
                                var debitDtos = await _debitService.GetByTransactionId(itemTranc.Id);
                                foreach (var itemdebit in debitDtos)
                                {
                                    var debitAll = new Dto.v2.DebtSaleAll();
                                    transactionDeposit = ObjectMapper.Map<DebitFullOutputDto, Dto.v2.TransactionDeposit>(itemdebit);
                                    debit = ObjectMapper.Map<DebitFullOutputDto, Dto.v2.Debit>(itemdebit);
                                    debitAll.Debit = debit;
                                    debitAll.Transaction = transactionDeposit;
                                    debitAll.Transaction.PaymentMethodId = EnmPaymentMethod.DebtSale;
                                    debitAll.Transaction.TransactionTypeId = itemTranc.TransactionTypeId;
                                    debitAll.Transaction.Note = itemTranc.Note;
                                    debitAll.Transaction.TransactionTime = itemTranc.TransactionTime;
                                    debitAll.Transaction.TransactionFee = itemTranc.TransactionFee;
                                    debitAll.PaymentRequestCode = itemPaymentRequest.PaymentRequestCode;
                                    listDebtSaleAll.Add(debitAll);
                                }
                                detail.DebtSaleAll = listDebtSaleAll;
                                resultDeposit.Detail = detail;
                            }
                        }
                        #endregion
                    }
                    return resultDeposit;
                }
            }
            return null;
        }
        public async Task<DepositAllDto> SearchESHistory(string paymentCode)
        {
            try
            {
                #region declare
                var resultDeposit = new DepositAllDto();
                if (!string.IsNullOrEmpty(paymentCode))
                {
                    //get redis
                    var payments = new List<string>();
                    payments.Add(paymentCode);
                    var dataRedis = await _paymentRedis.GetPriceDocumentsAsync(payments);
                    if (dataRedis != null && dataRedis.Count > 0)
                    {
                        resultDeposit = ObjectMapper.Map<PaymentRedisDetailDto, DepositAllDto>(dataRedis.FirstOrDefault());

                    }
                    else if (dataRedis == null || dataRedis.Count == 0)
                    {
                        // get thông tin từ es
                        resultDeposit = await GetdocESByPaymentCode(paymentCode);
                        if (!paymentCode.Contains("PM") && string.IsNullOrEmpty(resultDeposit.PaymentCode))
                        {
                            #region get history from Database
                            return await GetHistoryAll(paymentCode);
                            #endregion
                        }
                    }
                    if (resultDeposit.Detail != null && resultDeposit.Detail.TransfersAll != null && resultDeposit.Detail.TransfersAll.Count > 0)
                    {
                        foreach (var item in resultDeposit.Detail.TransfersAll)
                        {
                            if (!string.IsNullOrEmpty(item.TransferDetail.Image))
                            {
                                if (!string.IsNullOrEmpty(item.TransferDetail.ImageOrigin))
                                {
                                    var privateUrl = item.TransferDetail.ImageOrigin.Split('/');
                                    if (privateUrl.Length == 2 || privateUrl[0] == _configAws.BucketName)
                                    {
                                        var imgUrlPublic = GetPublicUrlImageS3(privateUrl[1]);
                                        item.TransferDetail.Image = imgUrlPublic;
                                    }
                                }
                                else
                                {
                                    var privateUrl = item.TransferDetail.Image.Split('/');
                                    if (privateUrl.Length == 2 || privateUrl[0] == _configAws.BucketName)
                                    {
                                        var imgUrlPublic = GetPublicUrlImageS3(privateUrl[1]);
                                        item.TransferDetail.Image = imgUrlPublic;
                                    }
                                }
                            }
                        }

                    }
                }
                #endregion
                return resultDeposit;
            }
            catch (BusinessException bex)
            {
                _log.LogError(string.Format("Search ES SearchESHistory :{0}|paymentCode :{1}", bex, paymentCode));
                throw bex;
            }
            catch (Exception ex)
            {
                _log.LogError(string.Format("Search ES SearchESHistory :{0}|paymentCode :{1}", ex, paymentCode));
                throw new UserFriendlyException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
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
        public async Task<DepositAllDto> GetdocESByPaymentCode(string paymentCode)
        {
            try
            {
                var resultDeposit = new DepositAllDto();
                var header = await _elasticClient.SearchAsync<HeaderFinalIndex>(s => s
                            .Query(q => q
                                       .Bool(b => b
                                           .Must(
                                               mu => mu.MatchPhrase(f => f.Field(m => m.id).Query(paymentCode)
                                       )
                            )
                          )));


                if (header.Documents != null)
                {
                    var result = header.Documents.FirstOrDefault();
                    if (result != null)
                    {
                        resultDeposit = ObjectMapper.Map<HeaderFinalIndex, DepositAllDto>(result);
                        var inforDetail = await _elasticClient.SearchAsync<PaymentTransIndex>(s => s
                            .Query(q => q
                                       .Bool(b => b
                                           .Must(
                                               mu => mu.MatchPhrase(f => f.Field(m => m.id).Query(paymentCode)
                                       )
                            )
                          )));
                        if (inforDetail != null)
                        {
                            var detail = inforDetail.Documents.FirstOrDefault();
                            if (detail != null)
                            {
                                resultDeposit.Detail = detail.Detail;
                                if (resultDeposit.Detail.TransfersAll != null && resultDeposit.Detail.TransfersAll.Count > 0)
                                {
                                    foreach (var item in resultDeposit.Detail.TransfersAll)
                                    {
                                        if (!string.IsNullOrEmpty(item.TransferDetail.Image))
                                        {
                                            if (!string.IsNullOrEmpty(item.TransferDetail.ImageOrigin))
                                            {
                                                var privateUrl = item.TransferDetail.ImageOrigin.Split('/');
                                                if (privateUrl.Length == 2 || privateUrl[0] == _configAws.BucketName)
                                                {
                                                    var imgUrlPublic = GetPublicUrlImageS3(privateUrl[1]);
                                                    item.TransferDetail.Image = imgUrlPublic;
                                                }
                                            }
                                            else
                                            {
                                                var privateUrl = item.TransferDetail.Image.Split('/');
                                                if (privateUrl.Length == 2 || privateUrl[0] == _configAws.BucketName)
                                                {
                                                    var imgUrlPublic = GetPublicUrlImageS3(privateUrl[1]);
                                                    item.TransferDetail.Image = imgUrlPublic;
                                                }
                                            }
                                        }
                                    }
                                }


                            }
                        }
                    }
                }
                return resultDeposit;
            }
            catch (BusinessException bex)
            {
                _log.LogError(string.Format("GetdocESByPaymentCode :{0}|paymentCode :{1}", bex, paymentCode));
                throw bex;
            }
            catch (Exception ex)
            {
                _log.LogError(string.Format("GetdocESByPaymentCode :{0}|paymentCode :{1}", ex, paymentCode));
                throw new UserFriendlyException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

        public async Task<List<ResponseMethodEs>> SearchESByOrdercode(RequestListPaymentMethodEs request)
        {
            var type = ((int)EnumType.EnmPaymentSourceCode.OMS).ToString();
            var header = await _elasticClient.SearchAsync<HeaderFinalIndex>(s => s
                             .Query(q => q
                                        .Bool(b => b
                                            .Must(
                                                mu => mu.Terms(s => s.Field(m => m.PaymentSource.FirstOrDefault().SourceCode).Terms(request.OrdersCode)),
                                                    mu => mu.MatchPhrase(f => f.Field(m => m.PaymentSource.FirstOrDefault().Type).Query(type)))

                                        )
                                ));
            var headerFinal = header.Documents.ToList();
            if (headerFinal != null && headerFinal.Count > 0)
            {
                var paymentPay = new RequestPaymentPay();
                paymentPay.PaymentCode = headerFinal.ToList().Select(x => x.PaymentCode).ToList();
                var inforDetail = await SearchTicketByFilterAsync(paymentPay);

                return  (from x in inforDetail
                              select new ResponseMethodEs
                              {
                                  PaymentCode = x.id,
                                  Name = GetNameMethod(x.Detail)
                              }).ToList();
                             ;
            }
            return new List<ResponseMethodEs>();
        }
        public string GetNameMethod(FRTTMO.PaymentCore.Dto.v2.Detail detail)
        {

            var name = new List<string>();
            if (detail != null)
            {
                if (detail.Cash != null && detail.Cash.Count() > 0)
                {
                    name.Add("Tiền mặt");
                }
                if (detail.EWalletAll != null && detail.EWalletAll.Count() > 0)
                {
                    name.Add("Ví điện tử");
                }
                if (detail.EWalletOnlineAll != null && detail.EWalletOnlineAll.Count() > 0)
                {
                    name.Add("Ví điện tử");
                }
                if (detail.CardsAll != null && detail.CardsAll.Count() > 0)
                {
                    name.Add("Thẻ");
                }
                if (detail.CodAll != null && detail.CodAll.Count() > 0)
                {
                    name.Add("COD");
                }
                if (detail.TransfersAll != null && detail.TransfersAll.Count() > 0)
                {
                    name.Add("Chuyển Khoản");
                }
                if (detail.VouchersAll != null && detail.VouchersAll.Count() > 0)
                {
                    name.Add("Voucher");
                }
                if (detail.DebtSaleAll != null && detail.DebtSaleAll.Count() > 0)
                {
                    name.Add("Mua Nợ");
                }
                return string.Join(",",name.ToArray());
            }
            return "";
        }
        public async Task<List<PaymentTransIndex>> SearchTicketByFilterAsync(RequestPaymentPay filters)
        {
            try
            {
                if (filters is null)
                {
                    return (new List<PaymentTransIndex>());
                }

                var mustQuery = new List<QueryContainer>();
                var boolQuery = new BoolQuery();

                if (filters.PaymentCode?.Count > 0)
                {

                    mustQuery.Add(new TermsQuery
                    {
                        Field = "id.keyword",
                        Terms = filters.PaymentCode
                    });


                    boolQuery.Must = mustQuery;
                    //  mustQuery = new List<QueryContainer>();
                }


                var searchRequest = new SearchRequest<PaymentTransIndex>()
                {
                    From = 0,
                    Size = 100,
                    Query = boolQuery,
                };


                var response = await _elasticClient.SearchAsync<PaymentTransIndex>(searchRequest);

                return (response.Documents?.ToList() ?? new List<PaymentTransIndex>());
            }
            catch (Exception ex)
            {
                return new List<PaymentTransIndex>();
            }
        }
    }
}
