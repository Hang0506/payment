using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface IBankService : IPaymentCoreAppServiceBase
    {
        Task<List<BankCardFullOutputDto>> GetBankCardListAsync();
        Task<List<BankFullOutputDto>> GetListAsync();
        Task<List<BankAccountFullOutputDto>> GetAccountByBankIdAsync(int BankId);
    }
}
