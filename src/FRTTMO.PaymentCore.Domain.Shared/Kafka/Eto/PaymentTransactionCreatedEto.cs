namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class PaymentTransactionCreatedETO : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_TRANSACTION_CREATED;
        public int Id { get; set; }
        public string OrderCode { get; set; }
    }
    public class TransactionCreatedETO : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_TRANSACTION_CREATED;
        public string OrderCode { get; set; }
        public string PaymentCode { get; set; }
        public object Data { get; set; }
    }
    public class CollectDepositTransactionCreatedETO : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_TRANSACTION_CD_CREATED;
        public string OrderCode { get; set; }
        public string PaymentCode { get; set; }
        public object Data { get; set; }
    }
}
