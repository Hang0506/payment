using System;
using System.Collections.Generic;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class PaymentRequestCompletedOutputEto : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_PAYMENT_REQUEST_COMPLETED;
        public string PaymentRequestCode { get; set; }
        public string OrderCode { get; set; }
        public string PaymentCode { get; set; }
        public TransactionFullOutputEto Transaction { get; set; }
        public List<TransactionFullOutputEto> DepositTransactions { get; set; }
        public Guid? OrderReturnId { get; set; }
        public EmPaymentRequestType? TypePayment { get; set; }
    }
    public class PaymentRequestCompletedDebbitOutputEto : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_PAYMENT_DEBBIT_COMPLETED;
        public string PaymentRequestCode { get; set; }
        public string OrderCode { get; set; }
        public string PaymentCode { get; set; }
        public TransactionFullOutputEto Transaction { get; set; }
        public List<TransactionFullOutputEto> DepositTransactions { get; set; }
        public Guid? OrderReturnId { get; set; }
        public EmPaymentRequestType? TypePayment { get; set; }
    }
}
