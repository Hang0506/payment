using System;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class PaymentRequestCreatedETO : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_PAYMENT_REQUEST_CREATED;
        public Guid Id { get; set; }
        public string PaymentCode { set; get; }
        public string OrderCode { get; set; }
        public string PaymentRequestCode { get; set; }
        public decimal TotalPayment { set; get; }
        public Guid OrderReturnId { get; set; }
        public string RequestCode { set; get; }
        public EmPaymentRequestType? TypePayment { set; get; }
        public EnmPaymentRequestStatus? Status { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
}
