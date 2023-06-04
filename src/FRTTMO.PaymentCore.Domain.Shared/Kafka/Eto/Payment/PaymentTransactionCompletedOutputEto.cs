using System;
namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class PaymentTransactionCompletedOutputEto : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_TRANSACTION_COMPLETED;
        public string OrderCode { get; set; }
        public string PaymentCode { get; set; }
        public TransactionFullOutputEto Transaction { get; set; }
    }
    public class WithdrawDepositCompletedOutputEto : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_TRANSACTION_RD_CREATED;
        public string OrderCode { get; set; }
        public string PaymentCode { get; set; }
        public string CreatedByName { get; set; }
        public TransactionFullOutputEto Transaction { get; set; }
        public TransferFullOutputEto Transfers { get; set; }
    }
    public class WithdrawReturnCompletedOutputEto : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_TRANSACTION_RT_CREATED;
        public string OrderReturnId { get; set; }
        public string PaymentCode { get; set; }
        public string OrderCode { get; set; }
        public string Note { get; set; }
        public TransactionFullOutputEto Transaction { get; set; }
        public TransferFullOutputEto Transfers { get; set; }
    }
}
