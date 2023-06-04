using FRTTMO.PaymentCore.Dto.v2;
using System;
using Volo.Abp.Application.Services;

namespace FRTTMO.PaymentCore.Services.v2
{
    public interface IGenerateServiceV2 : IApplicationService
    {
        GeneratePaymentRequestCodeOutputDtoV2 GeneratePaymentRequestCode(string prefix);
        DateTime? GetPaymentRequestDate(string paymentRequestCode);
    }
}
