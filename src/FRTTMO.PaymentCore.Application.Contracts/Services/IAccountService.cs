using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface IAccountService : IPaymentCoreAppServiceBase
    {
        Task<AccountFullOutputDto> GetBalanceByCustomerId(Guid customerId);
        Task<AccountFullOutputDto> CreateAsync(AccountInsertInputDto inputDto);
    }
}
