using System.Collections.Generic;

namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class PaymentRequestCompletedAROutputEto : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_PAYMENT_REQUEST_AR_COMPLETED;
        public string PaymentRequestCode { get; set; }
        public string OrderCode { get; set; }
        public string PaymentCode { get; set; }
        public TransactionFullOutputEto Transaction { get; set; }
        public List<PaymentInfoDetailEto> DepositTransactions { set; get; }
    }

}
