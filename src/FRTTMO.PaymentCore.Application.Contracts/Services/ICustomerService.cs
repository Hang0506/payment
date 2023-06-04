using FRTTMO.PaymentCore.Dto;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace FRTTMO.PaymentCore.Services
{
    public interface ICustomerService : IApplicationService
    {
        Task<CustomerFullOutputDto> CreateAsync(CustomerInsertInputDto Customer);
        Task<CustomerFullOutputDto> VerifyCustomerAsync(VerifyCustomerRequestDto requestDto);
    }
}
