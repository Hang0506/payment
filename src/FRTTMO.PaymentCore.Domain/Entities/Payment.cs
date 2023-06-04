using System;
using System.ComponentModel.DataAnnotations.Schema;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Entities
{
    public class Payment : BaseEntity<Guid>
    {
        [Column(TypeName = "varchar(25)")]
        public string PaymentCode { set; get; }
        public decimal? Total { set; get; }
        public EnmPaymentType Type { set; get; }
        public DateTime? PaymentDate { set; get; }
        public byte PaymentVersion { set; get; }
        public EnmPaymentStatus? Status { set; get; }
    }
}
