using System;
using System.ComponentModel.DataAnnotations.Schema;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Entities
{
    public class PaymentSource : BaseEntity<Guid>
    {
        [Column(TypeName = "varchar(25)")]
        public string PaymentCode { set; get; }
        [Column(TypeName = "varchar(50)")]
        public string SourceCode { set; get; }
        public EnmPaymentSourceCode Type { set; get; }
        public byte PaymentVersion { set; get; }
        public decimal Amount { set; get; }
        public Guid PaymentId { get; set; }
        public DateTime? PaymentDate { set; get; }
        public EnmPaymentTransactionStatus Status { set; get; }
    }
}
