using System;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class PaymentTransactionInputEto
    {
        public Guid? PaymentRequestCode { set; get; }
        public string ShopCode { set; get; }
        public decimal? TransactionFee { set; get; }
        public DateTime? TransactionTime { set; get; }
        public decimal Amount { set; get; }
        public string Note { set; get; }
        public string AdditionAttributes { set; get; }
        public DateTime CreatedDate { set; get; }
        public string CreatedBy { set; get; }

        public EnmTransactionType TransactionTypeId { set; get; }
        public EnmTransactionStatus Status { set; get; }
    }
}
