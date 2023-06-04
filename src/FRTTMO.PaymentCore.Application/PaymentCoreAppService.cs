using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using Volo.Abp.Application.Services;

namespace FRTTMO.PaymentCore
{
    public abstract class PaymentCoreAppService : ApplicationService, IPaymentCoreAppServiceBase
    {
        protected string _CoreName;
        protected PaymentCoreAppService()
        {
            LocalizationResource = typeof(PaymentCoreResource);
            ObjectMapperContext = typeof(PaymentCoreApplicationModule);
            _CoreName = GetType().FullName;
        }
        public ILogger GetLoggerServices() => Logger;
        #region Log Execute
        private Stopwatch stopWatch;
        private List<string> _ExecutionTimes;
        public void Stopwatch_StartNew()
        {
            if (stopWatch == null) stopWatch = new Stopwatch();
            if (!stopWatch.IsRunning) stopWatch.Start();
        }
        public void AddLogElapsed(string message, StopWatchType stopWatchType = StopWatchType.Continute)
        {
            if (stopWatch == null) return;
            if (_ExecutionTimes == null) _ExecutionTimes = new List<string>();
            _ExecutionTimes.Add($"{message}: {stopWatch.ElapsedMilliseconds}");
            if (stopWatchType == StopWatchType.Stop)
                stopWatch.Stop();
            else if (stopWatchType == StopWatchType.Reset)
                stopWatch.Reset();
            else
                return;
        }
        public long? GetLastElapsed()
        {
            if (stopWatch != null) stopWatch.Stop();
            return stopWatch?.ElapsedMilliseconds;
        }

        public List<string> GetLogExecutionTimes() => _ExecutionTimes;

        #endregion
    }
}
