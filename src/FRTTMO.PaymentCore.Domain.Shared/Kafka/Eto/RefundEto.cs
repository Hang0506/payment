namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class RefundEto: BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_TRANSACTION_CREATED;
        public object Data { get; set; }
    }
    public class RefundCreatedETO_1 : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_TRANSACTION_CREATED;
        public string OrderCode { get; set; }
        public object Data { get; set; }
    }
    public class RefundCreatedETO_2 : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_PAYMENT_REQUEST_CREATED;
        public string OrderCode { get; set; }
        public object Data { get; set; }
    }
}
