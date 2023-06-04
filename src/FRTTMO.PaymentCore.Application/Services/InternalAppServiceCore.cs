using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Http.Client;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services
{
    public class InternalAppServiceCore : PaymentCoreAppService, IInternalAppServiceCore, ITransientDependency
    {

        private readonly ILogger<InternalAppServiceCore> _log;
        private AbpRemoteServiceOptions _RemoteServiceOptions { get; }
        private readonly ILogApiService _logApiService;
        public InternalAppServiceCore(
                            ILogger<InternalAppServiceCore> log,
                            IOptionsSnapshot<AbpRemoteServiceOptions> remoteServiceOptions,
                            ILogApiService logApiService
        ) : base()
        {
            _log = log;
            _RemoteServiceOptions = remoteServiceOptions.Value;
            _logApiService = logApiService;
        }
        public async Task<T> InvokeInternalAPI_GetData<T>(string RemoteRoot, string path, bool Writelog = false, EnmPartnerId? Partner = null, EnmMethodType? methodType = null)
        {
            var request = new RestSharp.RestRequest(path, RestSharp.Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "*/*");
            var client = new RestSharp.RestClient(_RemoteServiceOptions.RemoteServices.GetConfigurationOrDefault(RemoteRoot).BaseUrl)
            {
                Timeout = -1
            };
            var response = await client.ExecuteAsync(request);
            if (Writelog)
            {
                await _logApiService.WriteLogApi(new LogApiDto
                {
                    MethodName = client.BaseUrl.AbsoluteUri + "/" + request.Resource,
                    PartnerId = Partner,
                    MethodType = methodType,
                    //Request = JsonConvert.SerializeObject(objRequest),
                    Response = response.Content
                });
            }
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            else
            {
                _log.LogError($"{_CoreName}.InvokeInternalAPI {RemoteRoot}{path} -> Response {response.StatusCode}-{response.ErrorMessage}-{response.Content}");
                var obEr = JObject.Parse(response.Content);
                throw new AbpRemoteCallException(JsonConvert.DeserializeObject<RemoteServiceErrorInfo>(obEr["error"].ToString()))
                {
                    HttpStatusCode = (int)response.StatusCode
                };
            }
        }

        public async Task<T> InvokeApi<T>(
           string remoteRoot,
           string path,
           RestSharp.Method method,
           string jsonInput = null,
           Dictionary<string, object> queryParams = null
       )
        {
            var request = new RestSharp.RestRequest(path, method);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "*/*");
            if (!string.IsNullOrEmpty(jsonInput))
            {
                request.AddParameter("application/json", jsonInput, RestSharp.ParameterType.RequestBody);
            }
            if (queryParams != null && queryParams.Count > 0)
            {
                foreach (var pairs in queryParams)
                {
                    request.AddParameter(pairs.Key, pairs.Value);
                }
            }

            var client = new RestSharp.RestClient(_RemoteServiceOptions.RemoteServices.GetConfigurationOrDefault(remoteRoot).BaseUrl)
            {
                Timeout = -1
            };
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            else
            {
                _log.LogError($"{_CoreName}.InvokeApi {remoteRoot}{path} -> Response {response.StatusCode}-{response.ErrorMessage}-{response.Content}");
                var obEr = JObject.Parse(response.Content);
                var messenge = new AbpRemoteCallException(JsonConvert.DeserializeObject<RemoteServiceErrorInfo>(obEr["error"].ToString()))
                {
                    HttpStatusCode = (int)response.StatusCode
                };
                return JsonConvert.DeserializeObject<T>(response.Content);

            }
        }
    }
}
