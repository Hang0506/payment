using System;

namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class PaymentRequestFailedOutputEto : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_PAYMENT_REQUEST_FAILED;
        public Guid AccountId { set; get; }
        public string PaymentRequestCode { get; set; }
        public string OrderCode { get; set; }
        public PaymentTransactionInputEto Transaction { get; set; }
    }
}
