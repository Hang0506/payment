using System;
using System.Collections.Generic;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class PaymentCreatedEto : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_CREATED;
        public string PaymentCode { set; get; }
        public decimal? Total { set; get; }
        public EnmPaymentType Type { set; get; }
        public byte PaymentVersion { set; get; }
        public EnmPaymentStatus? Status { set; get; }
    }
    public class PaymentTransactionCreatedEto : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENTTRANSACTION_CREATED;
        public string PaymentCode { set; get; }
        public string SourceCode { set; get; }
        public EnmPaymentSourceCode? Type { set; get; }
        public byte PaymentVersion { set; get; }
        public decimal Amount { set; get; }
        public EnmPaymentTransactionStatus Status { set; get; }
    }
}
