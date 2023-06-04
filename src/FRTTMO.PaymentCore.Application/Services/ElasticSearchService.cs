using FRTTMO.PaymentCore.Dto.TransactionDetailDto;
using FRTTMO.PaymentCore.Dto.v2;
using FRTTMO.PaymentCore.Options;
using FRTTMO.PaymentCore.Services.v2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services
{
    public class ElasticSearchService : PaymentCoreAppService, ITransientDependency, IElasticSearchService
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ElasticSearchService> _log;
        private readonly string _IndexPaymentCore;
        protected readonly IConfiguration _configuration;
        private readonly IElasticSearchServiceV2 _elasticSearchServiceV2;

        public ElasticSearchService(
            IElasticClient client,
            ILogger<ElasticSearchService> log,
            IConfiguration configuration,
            IElasticSearchServiceV2 elasticSearchServiceV2
        ) : base()
        {
            _elasticClient = client;
            _log = log;
            _IndexPaymentCore = configuration.GetSection("LongChauElasticsearch:Indices:PaymentCore").Value;
            _elasticSearchServiceV2 = elasticSearchServiceV2;
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
        private async Task<bool> UpdateAsyncBulk(TransactionIndex traffics)
        {
            _log.LogInformation("ElasticSearchService-UpdateAsyncBulk-{Traffics}", JsonConvert.SerializeObject(traffics));
            var response = await _elasticClient.UpdateAsync<TransactionIndex>(traffics.PaymentCode, u => u.Doc(traffics));
            _log.LogInformation("ElasticSearchService-UpdateAsyncBulk-{Response}", JsonConvert.SerializeObject(response));
            if (!response.IsValid)
            {
                _log.LogError(string.Format("Failed to index document {0}: {1}", response.Id, response.ServerError));
            }
            return true;
        }
        private async Task<bool> SaveProductBulk(TransactionIndex traffics)
        {
            var response = await _elasticClient.IndexDocumentAsync(traffics);
            if (!response.IsValid)
            {
                _log.LogError(string.Format("Failed to index document {0}: {1}", response.Id, response.ServerError));

                return false;
            }
            return true;
        }
        public async Task<bool> SyncDataESTransfer(string paymentCode, TransactionDetailTransferOutputDto data = null)
        {
            try
            {
                _log.LogInformation("ElasticSearchService-DeleteDataESTransfer-{Traffics}", JsonConvert.SerializeObject(data));
                var requestES = new Dictionary<string, object>();
                var doc = await _elasticClient.GetAsync<TransactionIndex>(paymentCode);
                var requestMethod = new RequestListPaymentMethodEs();
                requestMethod.OrdersCode = new List<string>();
                if (doc.Source != null)
                {
                    if (data != null)
                    {
                        requestES.Add("IsConfirmTransfer", data.IsConfirmTransfer ?? doc.Source.IsConfirmTransfer);
                        requestES.Add("CreatedDate", data.CreatedDate ?? doc.Source.CreatedDate);
                        requestES.Add("PaymentRequestCode", data.PaymentRequestCode ?? doc.Source.PaymentRequestCode);
                        requestES.Add("CreateDatePayment", data.CreateDatePayment ?? doc.Source.CreateDatePayment);
                        requestES.Add("AmountPayment", data.AmountPayment ?? doc.Source.AmountPayment);
                        requestES.Add("TypePayment", data.TypePayment ?? doc.Source.TypePayment);
                        requestES.Add("PaymentSoureType", data.PaymentSoureType ?? doc.Source.PaymentSoureType);
                        requestES.Add("CreatedBy", data.CreatedBy ?? doc.Source.CreatedBy);
                        requestES.Add("StatusFill", data.StatusFill ?? doc.Source.StatusFill);
                        requestES.Add("ShopCode", data.ShopCode ?? doc.Source.ShopCode);
                        requestES.Add("TransferAll", data.TransferAll ?? doc.Source.TransferAll);
                        requestES.Add("PaymentMethodId", data.PaymentMethodId ?? doc.Source.PaymentMethodId);

                        if (doc.Source.SourceCode != null)
                        {
                            foreach (var item in doc.Source.SourceCode)
                            {
                                if (data.SourceCode != null)
                                {
                                    if (!data.SourceCode.Select(x => x.SourceCode).Contains(item.SourceCode))
                                    {
                                        var ordercode = new SourceCodeSyncES()
                                        {
                                            SourceCode = item.SourceCode
                                        };
                                        data.SourceCode.Add(ordercode);
                                    }
                                }
                            }
                        }
                        requestES.Add("SourceCode", data.SourceCode ?? doc.Source.SourceCode);
                        //nếu data truyền vào hoặc data từ es orderCode thì mới tìm kiếm thông tin hình thức thu tiền 
                        if (doc.Source.ListMethodId != null && doc.Source.ListMethodId.Count == 0)
                        {
                            var sourceCodes = new List<string>();
                            if (data.SourceCode != null && data.SourceCode.Count > 0)
                            {
                                sourceCodes = data.SourceCode.ToList().Select(x => x.SourceCode).ToList();
                            }
                            else if (doc.Source.SourceCode != null && doc.Source.SourceCode.Count > 0)
                            {
                                sourceCodes = doc.Source.SourceCode.ToList().Select(x => x.SourceCode).ToList();
                            }
                            if (sourceCodes.Count > 0)
                            {
                                requestMethod.OrdersCode.AddRange(sourceCodes);
                                var listMethod = await _elasticSearchServiceV2.SearchESByOrdercode(requestMethod);
                                requestES.Add("ListMethodId", listMethod ?? doc.Source.ListMethodId);
                            }
                        }
                    }
                    await AddSyncData(doc.Source, requestES);
                    return await UpdateAsyncBulk(doc.Source);
                }
                else
                {
                    var traffics = ObjectMapper.Map<TransactionDetailTransferOutputDto, TransactionIndex>(data);
                    //nếu có thông tin orderCode thì mới tìm kiếm thông tin hình thức thu tiền 
                    if (data.SourceCode != null && data.SourceCode.Count > 0)
                    {
                        var param = data.SourceCode.ToList().Select(x => x.SourceCode);
                        requestMethod.OrdersCode.AddRange(param);
                        traffics.ListMethodId = await _elasticSearchServiceV2.SearchESByOrdercode(requestMethod);
                    }
                    return await SaveProductBulk(traffics);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "SyncData-KHTO-ES: {listSync}", JsonConvert.SerializeObject(data));
                return false;
            }
        }

        public async Task<List<TransactionDetailTransferOutputDto>> SearchESByPaymentCode(SearchESByPaymentCodeRequestDto request)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<TransactionDetailTransferOutputDto>(s => s
                                        .Query(mu => mu.Terms(s => s.Field(m => m.PaymentCode).Terms(request.PaymentCode)
                                            ))
                .Sort(x => x.Descending(a => a.CreatedDate))
                                    );
                var result = new List<TransactionDetailTransferOutputDto>();
                result.AddRange(searchResponse.Documents);
                return result;
            }
            catch (BusinessException bex)
            {
                _log.LogError(string.Format("Search ES By PaymentCode :{0}|PaymentCode :{1}", bex, request.PaymentCode));
                throw bex;
            }
            catch (Exception ex)
            {
                _log.LogError(string.Format("Search ES By PaymentCode :{0}|PaymentCode :{1}", ex, request.PaymentCode));
                throw new UserFriendlyException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }

        }
        public async Task<TransactionDetailTransferOutputDto> GetDocTransferES(string paymentCode)
        {
            try
            {
                var searchResponse = await _elasticClient.GetAsync<TransactionIndex>(paymentCode);
                if (searchResponse.Source == null)
                {
                    return null;
                }

                return searchResponse.Source;
            }
            catch (BusinessException bex)
            {
                _log.LogError(string.Format("GetDocTransferES By PaymentCode :{0}|PaymentCode :{1}", bex, paymentCode));
                throw bex;
            }
            catch (Exception ex)
            {
                _log.LogError(string.Format("GetDocTransferES By PaymentCode :{0}|PaymentCode :{1}", ex, paymentCode));
                throw new UserFriendlyException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }

        }
        public async Task<bool> DeleteDataESTransfer(string paymentCode)
        {
            try
            {
                if (GetDocTransferES(paymentCode).Result == null)
                {
                    return false;
                }
                var resultDelete = await _elasticClient.DeleteAsync<TransactionIndex>(paymentCode);
                if (resultDelete == null)
                {
                    _log.LogError("DeleteDataESTransfer", paymentCode);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "DeleteDataESTransfer", paymentCode);
                return false;
            }
        }
        public async Task<TransferInfoDetailOutputDto> SearchESTransferInfoByPaymentCode(TransferInfoInputDto request)
        {
            try
            {
                var statusfill = new List<byte>();
                if (request.StatusFill == null)
                {
                    statusfill = new List<byte>()
                    {
                       (byte)StatusFill.Filled,
                       (byte)StatusFill.NotFilled
                    };

                }
                else
                {
                    if (request.StatusFill == StatusFill.Filled)
                    {
                        statusfill = new List<byte>()
                        {
                            (byte) StatusFill.Filled
                        };
                    }
                    else
                    {
                        statusfill = new List<byte>()
                        {
                            (byte) StatusFill.NotFilled
                        };
                    }
                }
                var IsConfirm = new List<byte>();
                if (request.IsConfirm == null)
                {
                    IsConfirm = new List<byte>()
                    {
                        (byte)EnmTransferIsConfirm.AdvanceTransfer,
                        (byte)EnmTransferIsConfirm.Confirm
                    };
                }
                else
                {
                    if (request.IsConfirm == EnmTransferIsConfirm.Confirm)
                    {
                        IsConfirm = new List<byte>()
                        {
                            (byte)EnmTransferIsConfirm.Confirm
                        };
                    }
                    else
                    {
                        IsConfirm = new List<byte>()
                        {
                            (byte)EnmTransferIsConfirm.AdvanceTransfer
                        };
                    }
                }


                var searchResponse = await _elasticClient.SearchAsync<TransactionDetailTransferOutputDto>
                                        (s => s
                                            .Query(q => q
                                                .Bool(b => b
                                                    .Must(
                                                          mu => mu.Match(s => s.Field(m => m.PaymentCode).Query(request.PaymentCode)),
                                                          mu => mu.Match(s => s.Field(m => m.ShopCode).Query(request.ShopCode)),
                                                          mu => mu.Terms(s => s.Field(m => m.IsConfirmTransfer).Terms(IsConfirm)),
                                                          mu => mu.Terms(s => s.Field(m => m.StatusFill).Terms(statusfill)),
                                                          mu => mu.Terms(s => s.Field(m => m.SourceCode.Select(x => x.SourceCode))
                                                          .Terms(request.SourceCode)),
                                                          mu => mu.Terms(s => s.Field(m => m.PaymentMethodId).Terms(EnmPaymentMethod.Transfer))
                                                        )
                                                            .Filter(f => f.DateRange(r =>
                                                            (r.Field(p => p.CreateDatePayment)
                                                                            .GreaterThanOrEquals(request.StartDate != null
                                                                            ? DateTime.Parse(request.StartDate.Value.ToString("yyyy-MM-dd 00:00:00.000")) : null)
                                                                            .LessThanOrEquals(request.EndDate != null
                                                                            ? DateTime.Parse(request.EndDate.Value.ToString("yyyy-MM-dd 23:59:59.000")) : null))
                                                                            )
                                                            ||
                                                            (f.DateRange(r => r.Field(p => p.TransferAll.Select(x => x.DateTranfer)).TimeZone("+07:00")
                                                                            .GreaterThanOrEquals(request.StartDateTranfer != null
                                                                            ? DateTime.Parse(request.StartDateTranfer.Value.ToString("yyyy-MM-dd 00:00:00.000")) : null)
                                                                            .LessThanOrEquals(request.EndDateTranfer != null
                                                                            ? DateTime.Parse(request.EndDateTranfer.Value.ToString("yyyy-MM-dd 23:59:59.000")) : null)
                                                            ))
                                                            )))
                                .Sort(x => x.Descending(a => a.CreatedDate))
                                .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
                                    );
                var result = new TransferInfoDetailOutputDto();
                result.Total = (int)searchResponse.Total;
                var statelist = searchResponse.Documents.ToList();
                result.Result = statelist;
                return result;
            }
            catch (BusinessException bex)
            {
                _log.LogError(string.Format("SearchESTranferInfoByPaymentCode :{0}|OrderCode :{1}", bex, request.PaymentCode));
                throw bex;
            }
            catch (Exception ex)
            {
                _log.LogError(string.Format("SearchESTranferInfoByPaymentCode :{0}|PaymentCode :{1}", ex, request.PaymentCode));
                throw new UserFriendlyException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }
    }
}
