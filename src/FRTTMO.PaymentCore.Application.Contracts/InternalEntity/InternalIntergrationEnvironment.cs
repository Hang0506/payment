using System;

namespace FRTTMO.PaymentCore.Entities
{
    public interface IInternalIntergrationResponse
    {
        bool Is_Success();
        string ErrorCode { get; }
        string ErrorMessage { get; }
    }
    public class InternalErrorObj
    {
        public string code { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }
    public abstract class InternalIntergrationResponseBase : IInternalIntergrationResponse
    {
        public string ErrorCode => error?.code;
        public string ErrorMessage => error?.message;
        public bool Is_Success() => error == null;
        public InternalErrorObj error { get; set; }
    }

    #region OMSService
    public enum OMSOrderStatus
    {
        Confirmed = 1,
        Cancelled = 2,
        Completed = 4,
        VerifyFailed = 9
    }
    public class OMS_OrderResult /*: IInternalIntergrationResponse*/
    {
        public Guid OrderID { get; set; }
        public string OrderCode { get; set; }
        public OMSOrderStatus OrderStatus { get; set; }
        public string PaymentRequestCode { get; set; }
        public decimal TotalPayment { get; set; }
        public Guid? PaymentRequestId { get; set; }
    }

    public class OrderReturnResult : IInternalIntergrationResponse
    {
        public int OrderReturnStatus { get; set; }
        public Guid OrderReturnId { get; set; }
        public string OrderCode { get; set; }
        public Guid OrderId { get; set; }
        public decimal TotalPayment { get; set; }

        public string ErrorCode => null;
        public string ErrorMessage => null;
        public bool Is_Success() => true;
    }
    #endregion
}
