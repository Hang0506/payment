namespace FRTTMO.PaymentCore.Kafka
{
    public class KafkaTopics
    {
        public const string PAYMENT_TRANSACTION_CREATED = "lc.payment.transaction.created";
        public const string PAYMENT_PAYMENT_REQUEST_CREATED = "lc.payment.paymentRequest.created";
        public const string PAYMENT_PAYMENT_REQUEST_STATUS_UPDATED = "lc.payment.paymentRequest.status.updated";
        public const string PAYMENT_PAYMENT_VENDOR_TRANSACTION_STATUS_UPDATED = "lc.payment.vendor.transaction.status.updated";
        public const string PAYMENT_PAYMENT_REQUEST_FAILED = "lc.payment.paymentRequest.failed";
        public const string PAYMENT_PAYMENT_REQUEST_COMPLETED = "lc.payment.paymentRequest.completed";
        public const string PAYMENT_TRANSACTION_COMPLETED = "lc.payment.transaction.Completed";
        public const string PAYMENT_PAYMENT_REQUEST_AR_COMPLETED = "lc.payment.paymentRequest.AR.completed";
        public const string PAYMENT_PAYMENT_DEBBIT_COMPLETED = "lc.payment.debbit.completed";
        public const string PAYMENT_TRANSACTION_CD_CREATED = "lc.payment.transaction.CD.created";
        public const string PAYMENT_TRANSACTION_RD_CREATED = "lc.payment.transaction.RD.created";
        public const string PAYMENT_TRANSACTION_RT_CREATED = "lc.payment.transaction.RT.created";
        public const string PAYMENT_COMPANY_COD_UPDATED = "lc.payment.COD.updated";
        public const string PAYMENT_CREATED = "lc.payment.created";
        public const string PAYMENTTRANSACTION_CREATED = "lc.paymenttransaction.created";
        public const string PAYMENT_ASSEMBLER_STREAM = "lc.payment.assembler.stream";
    }
}
