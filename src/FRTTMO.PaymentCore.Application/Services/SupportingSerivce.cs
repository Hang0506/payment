using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.RemoteAPIs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public class SupportingService : PaymentCoreAppService, ISupportingService
    {

        private readonly IInternalAppServiceCore _internalAppService;
        public SupportingService(IInternalAppServiceCore internalAppService) : base()
        {
            _internalAppService = internalAppService;
        }
        public async Task<bool> UpdateOrderOMS(UpdateOMSDto input)
        {
            try
            {
                var result = await _internalAppService.InvokeApi<object>(
                                EnvironmentSetting.RemoteOMSService,
                                InternalOMSApiUrl.UpdateDataPaymentRequestCodeVNPay,
                                RestSharp.Method.PUT,
                                JsonConvert.SerializeObject(input)
                            );
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }
        public async Task<List<OrderOutputSupporting>> GetOrders(string orderCode)
        {
            var result = new List<OrderOutputSupporting>();
            try
            {
               result = await _internalAppService.InvokeInternalAPI_GetData<List<OrderOutputSupporting>>(EnvironmentSetting.RemoteOMSService, $"{InternalOMSApiUrl.GetListOrderByUpdates}?orderCode={orderCode}");
            }
            catch (Exception ex)
            {
                // Todo log
                throw ex;
            }
            return result;
        }
    }
}
