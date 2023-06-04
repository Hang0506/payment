using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace FRTTMO.PaymentCore
{
    public interface IPaymentCoreController : IPaymentCoreAppServiceBase
    {
        ILogger Log { get; }
    }

    [CustAuditLog]
    [Area(PaymentCoreRemoteServiceConsts.ModuleName)]
    [RemoteService(Name = PaymentCoreRemoteServiceConsts.RemoteServiceName)]
    [ApiController]
    public abstract class PaymentCoreController<T_Services> : AbpControllerBase, IPaymentCoreController where T_Services : IPaymentCoreAppServiceBase
    {
        public ILogger Log { get; protected set; }
        protected string _CoreName;
        protected T_Services _mainService { get; private set; }
        protected T_Services MainService
        {
            set
            {
                _mainService = value;
                Log = value?.GetLoggerServices();
            }
        }
        protected PaymentCoreController()
        {
            LocalizationResource = typeof(PaymentCoreResource);
            _CoreName = GetType().FullName;
        }       

        [NonAction]
        public ILogger GetLoggerServices() => null;
        [NonAction]
        public virtual List<string> GetLogExecutionTimes() => _mainService?.GetLogExecutionTimes();
        [NonAction]
        public virtual long? GetLastElapsed() => _mainService?.GetLastElapsed();

        [NonAction]
        public void Stopwatch_StartNew()
        {
            if (_mainService != null) _mainService.Stopwatch_StartNew();
        }

        [NonAction]
        public void AddLogElapsed(string message, StopWatchType stopWatchType = StopWatchType.Continute)
        {
            if (_mainService != null) _mainService.AddLogElapsed(message, stopWatchType);
        }
    }
    public sealed class CustAuditLogAttribute : ActionFilterAttribute
    {
        private const string CustomeRequestHeaderId = "PAYCORE-x-request-id";
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(context.HttpContext.Request.Headers[CustomeRequestHeaderId])) context.HttpContext.Request.Headers[CustomeRequestHeaderId] = $"PAYCORE{DateTime.UtcNow:HHmmssfff}x" + Guid.NewGuid().ToString().Replace("-", "");
                if (context.Controller != null)
                {
                    var ctrl = context.Controller as IPaymentCoreController;
                    ctrl.Stopwatch_StartNew();
                    if (ctrl.Log != null)
                    {
                        ctrl.Log.LogInformation($"RequestBody {context.HttpContext.Request.Headers[CustomeRequestHeaderId]} , {DateTime.UtcNow:ddMMMyy HH:mm:ss:fffz} for {context.HttpContext.Request.Path.Value}: {Newtonsoft.Json.JsonConvert.SerializeObject(context.ActionArguments)} ");
                    }
                    ctrl.AddLogElapsed("StartActs");
                }
            }
            catch (Exception)
            {
            }
            base.OnActionExecuting(context);
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            try
            {
                if (context.Controller != null)
                {
                    var ctrl = context.Controller as IPaymentCoreController;
                    var extms = ctrl.GetLogExecutionTimes();
                    var lstElap = ctrl.GetLastElapsed() ?? 0;

                    var x = context.HttpContext.Request.Headers[CustomeRequestHeaderId];
                    var y = context.HttpContext.Request.Path.Value;

                if (ctrl.Log != null)
                    {
                        if (extms != null && extms.Any() && lstElap > 2000)
                        {
                            ctrl.Log.LogInformation($"ExecutionTimes {context.HttpContext.Request.Headers[CustomeRequestHeaderId]} , {DateTime.UtcNow:ddMMMyy HH:mm:ss:fffz} ({lstElap}ms) : {Newtonsoft.Json.JsonConvert.SerializeObject(extms)} ");
                        }
                        if (context.Exception != null)
                        {
                            ctrl.Log.LogError($"ResultBody {context.HttpContext.Request.Headers[CustomeRequestHeaderId]} , {DateTime.UtcNow:ddMMMyy HH:mm:ss:fffz} for {context.HttpContext.Request.Path.Value}: {Newtonsoft.Json.JsonConvert.SerializeObject(context.Exception)} ");
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            base.OnActionExecuted(context);
        }
    }
}
