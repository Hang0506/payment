using FRTTMO.PaymentCore.Dto;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace FRTTMO.PaymentCore.Application.Redis.Payment
{
    public interface IPaymentRedisRepositotyService<T> : IApplicationService
    {
        Task<PaymentRedisDetailDto> GetAsync([NotNull] string key);
        Task<List<PaymentRedisDetailDto>> GetListAsync([NotNull] ICollection<string> keys);
        Task SetAsync(string key, PaymentRedisDetailDto value, double expiredTime = 0);
        Task<bool> DeleteBulkGroupAsync(string group);
    }
}
