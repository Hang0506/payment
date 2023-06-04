using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Application.Contracts
{
    public interface IPaymentCoreAppServiceBase : IApplicationService, ITransientDependency
    {
        ILogger GetLoggerServices();
        List<string> GetLogExecutionTimes();
        long? GetLastElapsed();
        void Stopwatch_StartNew();
        void AddLogElapsed(string message, StopWatchType stopWatchType = StopWatchType.Continute);
    }
    public enum StopWatchType
    {
        Continute = 1,
        Reset = 2,
        Stop = 3
    }
}
