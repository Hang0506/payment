namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class CashbackCreatedEto : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_TRANSACTION_CREATED;
        public string OrderCode { get; set; }
        public string PaymentRequestCode { get; set; }
        public TransactionFullOutputEto Transaction { get; set; }
    }
}
