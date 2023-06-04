using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface IBankingOnlineService : IPaymentCoreAppServiceBase
    {
        Task<List<BankingOnlineOutPutDto>> GetlistAsync();
    }
}
