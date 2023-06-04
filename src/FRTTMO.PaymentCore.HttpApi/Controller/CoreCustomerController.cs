using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FRTTMO.PaymentGateway.Dto;
using FRTTMO.PaymentGateway.Services;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace FRTTMO.PaymentGateway.Controller
{
    [Area(PaymentGatewayRemoteServiceConsts.ModuleName)]
    [RemoteService(Name = PaymentGatewayRemoteServiceConsts.RemoteServiceName)]
    [Route("api/PaymentGateway/CoreCustomer")]
    public class CoreCustomerController : PaymentGatewayController, ICoreCustomerService
    {
        private readonly ICoreCustomerService _customerService;

        public CoreCustomerController(ICoreCustomerService customerService)
        {
            _customerService = customerService;
        }
        [HttpPost]
        public Task<CoreCustomerDto> CreateAsync([FromBody] CoreCustomerInputDto coreCustomer)
        {
            return _customerService.CreateAsync(coreCustomer);
        }
        [HttpGet]
        [Route("{customerId}")]
        public Task<CoreCustomerDto> GetAsync(Guid customerId, IEnumerable<string> includeAttributes)
        {
            return _customerService.GetAsync(customerId, includeAttributes);
        }
        [HttpPost]
        [Route("search")]
        public Task<CoreCustomerProfile> SearchAsync([FromBody] SearchCoreCustomerDto searchCoreCustomerDto)
        {
            return _customerService.SearchAsync(searchCoreCustomerDto);
        }
    }
}
