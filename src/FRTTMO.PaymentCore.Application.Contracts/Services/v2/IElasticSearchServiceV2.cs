using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Dto.v2;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace FRTTMO.PaymentCore.Services.v2
{
    public interface IElasticSearchServiceV2 : IPaymentCoreAppServiceBase
    {
        Task<bool> SyncESDepositAllAsync(string paymentCode, DepositAllDto data, string insertFrom = "");
        Task<DepositAllDto> SearchESHistory(string paymentCode);
        Task<DepositAllDto> GetHistoryAll(string paymentCode);
        Task<DepositAllDto> GetdocESByPaymentCode(string paymentCode);
        Task<List<ResponseMethodEs>> SearchESByOrdercode(RequestListPaymentMethodEs request);
    }
}
