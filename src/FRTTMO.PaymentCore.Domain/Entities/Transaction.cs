using System;
using System.ComponentModel.DataAnnotations.Schema;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Entities
{
    public class Transaction : BaseEntity<Guid>
    {
        public Guid? AccountId { set; get; }
        public EnmTransactionType? TransactionTypeId { set; get; }
        public Guid? PaymentRequestId { set; get; }
        [Column(TypeName = "varchar(20)")]
        public string PaymentRequestCode { set; get; }
        public string ShopCode { set; get; }
        public EnmPaymentMethod? PaymentMethodId { set; get; }
        public decimal? Amount { set; get; }
        public decimal? TransactionFee { set; get; }
        public DateTime? TransactionTime { set; get; }
        public string Note { set; get; }
        public string AdditionAttributes { set; get; }
        public EnmTransactionStatus? Status { set; get; }
        public DateTime? PaymentRequestDate { set; get; }
    }
}
