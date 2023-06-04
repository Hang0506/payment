using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Application.Contracts
{
    public interface IInternalAppServiceCore: IApplicationService
    {       
        Task<T> InvokeInternalAPI_GetData<T>(string RemoteRoot, string path, bool Writelog = false, EnmPartnerId? Partner = null, EnmMethodType? methodType = null);
        Task<T> InvokeApi<T>(
           string remoteRoot,
           string path,
           Method method,
           string jsonInput = null,
           Dictionary<string, object> queryParams = null
       );
    }
}
