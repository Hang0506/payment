using System;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Entities
{
    public class Refund : BaseEntity<Guid>
    {
        public string OrderCode { get; set; }
        public Guid CustomerId { get; set; }
        //
        public int TransactionTypeId { get; set; }
        public Guid? PaymentRequestId { get; set; }
        public string ShopCode { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal? Amount { get; set; }
        public decimal? TransactionFee { get; set; }
        public DateTime? TransactionTime { get; set; }
        public string Note { get; set; }
        public string AdditionAttributes { get; set; }
        public EnmTransactionStatus? Status { set; get; }

    }
}
