using FRTTMO.PaymentGateway.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace FRTTMO.PaymentGateway.Services
{
    public interface ICoreCustomerService : IApplicationService
    {
        Task<CoreCustomerDto> CreateAsync(CoreCustomerInputDto coreCustomer);
        Task<CoreCustomerDto> GetAsync(Guid customerId, IEnumerable<string> includeAttributes);
        Task<CoreCustomerProfile> SearchAsync(SearchCoreCustomerDto searchCoreCustomerDto);
    }
}
