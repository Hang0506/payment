using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Services
{
    public class LogApiService : PaymentCoreAppService, ITransientDependency, ILogApiService
    {
        private readonly ILogger<LogApiService> _log;
        //private readonly ILogApiRepository _iLogApiRepository;


        public LogApiService(ILogger<LogApiService> log) : base()
        {
            _log = log;
        }

        public async Task<bool> WriteLogApi(LogApiDto logApi)
        {
            try
            {
                var request = ObjectMapper.Map<LogApiDto, LogApiES>(logApi);
                //await _iLogApiRepository.InsertAsync(request);
                _log.LogInformation($"LogApiServicePayment PartnerId= {request?.PartnerId} , MethodType= {request?.MethodType} , MethodName= {request?.MethodName} : {JsonConvert.SerializeObject(request)}");
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}
