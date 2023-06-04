using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace FRTTMO.PaymentCore.Services.v2
{
    public interface IPaymentRedisService : IPaymentCoreAppServiceBase
    {
        Task<List<PaymentRedisDetailDto>> GetPriceDocumentsAsync(List<string> listPaymentCode);
        Task<bool> SyncPaymentCoreToRedis(PaymentRedisDto inputDto);
    }
}
