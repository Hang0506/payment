using FRTTMO.PaymentCore.Dto.TransactionDetailDto;
using FRTTMO.PaymentCore.Dto.v2;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Kafka.Eto;
using FRTTMO.PaymentCore.Kafka.Interface;
using FRTTMO.PaymentCore.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services.v2
{
    public class DepositAdjustService : PaymentCoreAppService, ITransientDependency, IDepositAdjustService
    {
        private readonly ILogger<DepositServiceV2> _log;
        private readonly IPublishService<BaseETO> _iPublishService;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IGenerateServiceV2 _generateServiceV2;
        private readonly IPaymentRepository _paymentRepository;
        private readonly FRTTMO.PaymentIntegration.Services.IElasticSearchService _elasticSearchIntergationService;
        private readonly ITransactionServiceV2 _transactionServiceV2;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly IElasticSearchService _elasticSearch;


        public DepositAdjustService(
            ILogger<DepositServiceV2> log,
            IPaymentRequestRepository paymentRequestRepository,
            IGenerateServiceV2 generateServiceV2,
            IPublishService<BaseETO> publishService,
            IPaymentRepository paymentRepository,
            FRTTMO.PaymentIntegration.Services.IElasticSearchService elasticSearchIntergationService,
            ITransactionServiceV2 transactionServiceV2,
            IPaymentTransactionRepository paymentTransactionRepository,
            ElasticSearchService elasticSearchService,
            IElasticSearchService elasticSearch
        ) : base()
        {
            _log = log;
            _paymentRequestRepository = paymentRequestRepository;
            _generateServiceV2 = generateServiceV2;
            //_syncDataESAppService = syncDataESAppService;
            _iPublishService = publishService;
            _paymentRepository = paymentRepository;
            _elasticSearchIntergationService = elasticSearchIntergationService;
            _transactionServiceV2 = transactionServiceV2;
            _elasticSearchService = elasticSearchService;
            _paymentTransactionRepository = paymentTransactionRepository;
            _elasticSearch = elasticSearch;
        }

        public async Task<bool> CancelRequestByPaymentRequestCode(string paymentRequestCode)
        {
            var paymentRequestDate = _generateServiceV2.GetPaymentRequestDate(paymentRequestCode);
            var paymentRequest = await _paymentRequestRepository.GetByPaymentRequestCode(paymentRequestCode, paymentRequestDate);
            //kiểm tra paymentRequest có khác Status=4 không
            if (paymentRequest != null && paymentRequest.Status == EnmPaymentRequestStatus.Confirm)
            {
                //nếu status là 1 và chưa deposit và không có trong historty qr
                //chưa tạo qr
                var qrhisoty = await _elasticSearchIntergationService.SearchESHistory(paymentRequest.PaymentCode);
                if (qrhisoty != null && qrhisoty.QrDetail != null
                    && qrhisoty.QrDetail.Count > 0 && qrhisoty.PaymentCode != null)
                {
                    var request = qrhisoty.QrDetail.Where(x => x.PaymentRequestCode == paymentRequestCode).FirstOrDefault();
                    if (request != null && request.PartnerId != (int)EnmPartnerId.VPB)
                    {
                        return false;
                    }
                }
                var isTransaction = await _transactionServiceV2.GetByPaymentRequestCode(paymentRequest.PaymentRequestCode, paymentRequestDate);

                if (isTransaction.Count > 0)
                {
                    return false;
                }
                paymentRequest.Status = EnmPaymentRequestStatus.Cancel;
                await _paymentRequestRepository.Update(paymentRequest);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> CancelReuqest(string paymentCode)
        {
            var paymentRequest = await _paymentRequestRepository.GetListOfCode(paymentCode);
            var paymentDate = _generateServiceV2.GetPaymentRequestDate(paymentCode);
            var payment = await _paymentRepository.Get(paymentCode, paymentDate);
            if (paymentRequest != null && payment != null)
            {
                if (paymentRequest.Any(m => m.TypePayment == EmPaymentRequestType.PaymentCoreRequest && m.Status == EnmPaymentRequestStatus.Complete))
                {
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_FINISHED).WithData("PaymentCode", paymentCode);
                }

                paymentRequest.ForEach(m => { m.Status = EnmPaymentRequestStatus.Cancel; });
                var list = await _paymentRequestRepository.UpdateManyAsync(paymentRequest);

                payment.Status = EnmPaymentStatus.Cancel;
                await _paymentRepository.UpdateAsync(payment);
                // xóa trên es nếu có 
                await _elasticSearch.DeleteDataESTransfer(payment.PaymentCode);
                return true;
            }
            else
            {
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_CODE_NOT_VALID).WithData("PaymentCode", paymentCode);
            }
        }

        public async Task<PaymentRequestOutputDto> CreateRequest(PaymentRequestInputDto paymentRequestInsert)
        {
            //var isPaymentCode = await _paymentRequestRepository.IsPaymentCode(paymentRequestInsert.PaymentCode);
            //if (isPaymentCode != null)
            //    throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_CODE_EXITS).WithData("PaymentCode", paymentRequestInsert.PaymentCode);
            var request = ObjectMapper.Map<PaymentRequestInputDto, PaymentRequest>(paymentRequestInsert);
            //
            var paymentRqGen = _generateServiceV2.GeneratePaymentRequestCode("");
            request.PaymentRequestCode = paymentRqGen.PaymentRequestCode;
            request.PaymentRequestDate = paymentRqGen.PaymentRequestDate;
            request.TypePayment = paymentRequestInsert.TypePaymentValue;
            //
            request.Status = EnmPaymentRequestStatus.Confirm;
            var response = await _paymentRequestRepository.InsertObj(request);
            var returnData = ObjectMapper.Map<PaymentRequest, PaymentRequestOutputDto>(response);

            // bắn Kafka Topic lc.payment.paymentRequest.created
            var paymentRequestEto = ObjectMapper.Map<PaymentRequest, PaymentRequestCreatedETO>(response);
            await _iPublishService.ProduceAsync(paymentRequestEto);
            //// get Payment
            //var paymentDate = _generateServiceV2.GetPaymentRequestDate(response.PaymentCode);
            //var checkPayPM = await _paymentRepository.Get(response.PaymentCode, paymentDate);
            //if (checkPayPM != null)
            //{
            //    if (checkPayPM.Type == EnmPaymentType.Chi &&
            //                (paymentRequestInsert.PaymentSourceType == EnmPaymentSourceCode.RT || paymentRequestInsert.PaymentSourceType == EnmPaymentSourceCode.ReturnCancelDeposit))
            //    {
            //        var sourceCode = paymentRequestInsert.SourceCode;
            //        if (!string.IsNullOrEmpty(paymentRequestInsert.SourceCode))
            //        {
            //            var insertInput = new PaymentSource();
            //            insertInput.Type = (EnmPaymentSourceCode)paymentRequestInsert.PaymentSourceType;
            //            insertInput.SourceCode = sourceCode;
            //            insertInput.Amount = paymentRequestInsert.TotalPayment;
            //            insertInput.PaymentId = checkPayPM.Id;
            //            insertInput.PaymentVersion = 1;
            //            insertInput.Status = EnmPaymentTransactionStatus.Complete;
            //            insertInput.PaymentCode = response.PaymentCode;
            //            var isExists = await _paymentTransactionRepository.IsCheckInfor(insertInput);
            //            if (!isExists)
            //            {
            //                sourceCode = (await _paymentTransactionRepository.Insert(insertInput)).SourceCode;
            //            }
            //        }   
            //        var result = new TransactionDetailTransferOutputDto();
            //        result.TypePayment = (byte?)checkPayPM.Type;
            //        result.PaymentCode = paymentRequestInsert.PaymentCode;
            //        result.IsConfirmTransfer = (byte?)EnmTransferIsConfirm.AdvanceTransfer;
            //        result.CreatedDate = DateTime.Now;
            //        result.StatusFill = (byte?)StatusFill.NotFilled;
            //        result.PaymentMethodId = (byte?)EnmPaymentMethod.Transfer;
            //        result.PaymentSoureType = (byte?)paymentRequestInsert.PaymentSourceType;
            //        result.AmountPayment = paymentRequestInsert.TotalPayment;
            //        result.SourceCode = new List<SourceCodeSyncES>() { new SourceCodeSyncES() { SourceCode = sourceCode } };
            //        var syncES = _elasticSearchService.SyncDataESTransfer(response.PaymentCode, result);

            //        if (syncES.Result == true)
            //        {
            //            _log.LogInformation(string.Format("SyncTransferTrue: {0}| Request body: {1} ", null, JsonConvert.SerializeObject(result)));
            //        }
            //        else
            //        {
            //            _log.LogInformation(string.Format("SyncTransferFail: {0}| Request body: {1} ", null, JsonConvert.SerializeObject(result)));
            //        }
            //    }
            //}
            return returnData;
        }
    }
}
