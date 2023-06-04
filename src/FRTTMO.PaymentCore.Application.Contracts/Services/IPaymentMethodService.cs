using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface IPaymentMethodService : IPaymentCoreAppServiceBase
    {
        Task<List<PaymentMethodDto>> GetListAsync();
        Task<List<PaymentMethodDto>> GetListPaymentMethodByOrderCode(GetListPaymentMethodByOrderCodeInputDto inputDto);
    }
}
