using FRTTMO.PaymentCore.Dto.v2;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Services.v2
{
    public class GenerateServiceV2 : PaymentCoreAppService, ITransientDependency, IGenerateServiceV2
    {
        private readonly ILogger<GenerateServiceV2> _log;

        public GenerateServiceV2(ILogger<GenerateServiceV2> log) : base()
        {
            _log = log;
        }

        public GeneratePaymentRequestCodeOutputDtoV2 GeneratePaymentRequestCode(string prefix)
        {
            try
            {
                var dateNow = DateTimeOffset.Now;
                var paymentRequestCode = "";
                paymentRequestCode = prefix + "-" + dateNow.ToUnixTimeMilliseconds();
                return new GeneratePaymentRequestCodeOutputDtoV2
                {
                    PaymentRequestCode = paymentRequestCode,
                    PaymentRequestDate = dateNow.Date
                };
            }
            
            catch (Exception ex)
            {
                _log.LogError(_CoreName + $".GeneratePaymentRequestCode: {ex}| Request body: {JsonConvert.SerializeObject(prefix)} | Error: {ex}");
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"GeneratePaymentRequestCode {prefix}: error - " + ex.Message);
            }
            
        }
        public DateTime? GetPaymentRequestDate(string paymentRequestCode)
        {
            try
            {
                var paymentArr = paymentRequestCode.Split("-")[1];
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(paymentArr));
                return dateTimeOffset.LocalDateTime.Date;
            }
            catch (Exception ex)
            {
                _log.LogError(_CoreName + $".GetPaymentRequestDate: {ex}");
                return null;
            }
        }
    }
}
