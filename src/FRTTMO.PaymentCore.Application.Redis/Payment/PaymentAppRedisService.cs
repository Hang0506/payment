using FRTTMO.PaymentCore.Application.Redis.Redis;
using FRTTMO.PaymentCore.Dto;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Volo.Abp.Json;

namespace FRTTMO.PaymentCore.Application.Redis.Payment
{
    public class PaymentAppRedisService : RedisBaseService<PaymentRedisDetailDto>, IPaymentRedisRepositotyService<PaymentRedisDetailDto>
    {
        private readonly IDatabase _db;
        private readonly IRedisConnectionFactory _redis;
        private readonly ILogger<PaymentAppRedisService> _log;
        public PaymentAppRedisService(IRedisConnectionFactory redis,
                                                   IJsonSerializer jsonSerializer,
                                                   ILogger<PaymentAppRedisService> log
        )
        {
            _redis = redis;
            _db = this._redis.Connection().GetDatabase();
            _jsonSerializer = jsonSerializer;
            _log = log;
        }

        public async Task<PaymentRedisDetailDto> GetAsync([NotNull] string key)
        {
            var result = await _db.StringGetAsync((RedisKey)key);
            if (result.IsNull)
            {
                return null;
            }
            return Deserialize(result);
        }

        public async Task<List<PaymentRedisDetailDto>> GetListAsync([NotNull] ICollection<string> keys)
        {
            var listDataPrice = new List<PaymentRedisDetailDto>();
            foreach (var key in keys)
            {
                var result = await _db.StringGetAsync((RedisKey)key);
                if (result.HasValue)
                    listDataPrice.Add(Deserialize(result));
            }

            return listDataPrice;
        }

        public Task SetAsync(string key, PaymentRedisDetailDto value, double expiredTime = 0)
        {
            TimeSpan? cachingExpiration = null;

            if(expiredTime > 0)
                cachingExpiration = TimeSpan.FromDays(expiredTime);

            return _db.StringSetAsync(key, Serialize(value), cachingExpiration);
        }

        public async Task<bool> DeleteBulkGroupAsync(string group)
        {
            try
            {

                var server = this._redis.Connection().GetServer(_redis.Connection().Configuration);
                foreach (var key in server.Keys(pattern: string.Concat(group, "*")))
                {
                    await _db.KeyDeleteAsync(key);
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "PaymentRedisService-DeleteBulkAsync");
                return false;
            }
        }
    }
}
