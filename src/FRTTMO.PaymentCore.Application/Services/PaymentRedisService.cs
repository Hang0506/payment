using FRTTMO.PaymentCore.Application.Redis.Payment;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Services.v2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;

namespace FRTTMO.PaymentCore.PricingAPIService
{
    public class PaymentRedisService : PaymentCoreAppService, IPaymentRedisService
    {
        private readonly ILogger<PaymentRedisService> _log;
        private readonly IConfiguration _configuration;
        private readonly IPaymentRedisRepositotyService<PaymentRedisDetailDto> _paymentRedisService;
        private readonly string RedisKey_Prefix = "";

        public PaymentRedisService(ILogger<PaymentRedisService> log,
                                                 IConfiguration configuration,
                                                 IPaymentRedisRepositotyService<PaymentRedisDetailDto> paymentRedisService
        )
        {
            _log = log;
            _configuration = configuration;
            _paymentRedisService = paymentRedisService;
            RedisKey_Prefix = string.Concat(PaymentCoreAPISettings.GroupKey, PaymentCoreAPISettings.PaymentKey);
        }

        public async Task<List<PaymentRedisDetailDto>> GetPriceDocumentsAsync(List<string> listPaymentCode)
        {
            try
            {
                List<string> keys = new List<string>();
                foreach (string key in listPaymentCode) keys.Add(RedisKey_Prefix + key);

                var dataPriceRedis = await _paymentRedisService.GetListAsync(keys);
                _log.LogInformation("PaymentRedisService-GetPriceDocumentsAsync: {redisResponse}", JsonConvert.SerializeObject(dataPriceRedis));

                if (dataPriceRedis == null)
                    return new List<PaymentRedisDetailDto> { null };

                return dataPriceRedis;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "PriceRedisAppService-GetPriceDocumentsAsync-Request body: {RequestObject}", listPaymentCode);
                throw new UserFriendlyException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }
        public async Task<bool> SyncPaymentCoreToRedis(PaymentRedisDto inputDto)
        {
            try
            {
                if (inputDto.Items.Count > 0)
                {
                    foreach (var itemPayment in inputDto.Items)
                    {
                        double cachingExpiration = 0;
                        double.TryParse(_configuration["Redis:ExpiredTime"]?.ToString(), out cachingExpiration);

                        await _paymentRedisService.SetAsync(string.Concat(RedisKey_Prefix) + itemPayment.PaymentCode, itemPayment, cachingExpiration);
                    }
                }
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "PaymentCoreToRedis-SynPaymentToRedis-Request body: {RequestObject}", inputDto.PaymentCode);
                throw new UserFriendlyException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

    }
}
