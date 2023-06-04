using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Services
{
    public class GenerateService : PaymentCoreAppService, ITransientDependency, IGenerateService
    {
        private readonly ILogger<CardService> _log;

        public GenerateService(ILogger<CardService> log) : base()
        {
            _log = log;
        }

        public async Task<string> GeneratePaymentRequestCode(string shopCode)
        {
            try
            {
                string paymentRequestCode = "";
                paymentRequestCode = shopCode + "-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return await Task.FromResult(paymentRequestCode);
            }
            
            catch (Exception ex)
            {
                _log.LogError(_CoreName + $".GeneratePaymentRequestCode: {ex}| Request body: {JsonConvert.SerializeObject(shopCode)} | Error: {ex}");
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"GeneratePaymentRequestCode {shopCode}: error - " + ex.Message);
            }
            
        }

    }
}
