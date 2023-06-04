using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface ISupportingService : IPaymentCoreAppServiceBase
    {
        Task<List<OrderOutputSupporting>> GetOrders(string orderCode);
        Task<bool> UpdateOrderOMS(UpdateOMSDto input);
    }
}
