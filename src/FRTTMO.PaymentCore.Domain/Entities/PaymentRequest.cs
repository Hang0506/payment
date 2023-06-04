using System;
using System.ComponentModel.DataAnnotations.Schema;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Entities
{
    public class PaymentRequest : BaseEntity<Guid>
    {
        [Column(TypeName = "varchar(25)")]
        public string PaymentCode { set; get; }
        public string OrderCode { set; get; }
        public Guid? OrderReturnId { set; get; }
        [Column(TypeName = "varchar(20)")]
        public string PaymentRequestCode { set; get; }
        public decimal TotalPayment { set; get; }
        public EnmPaymentRequestStatus? Status { set; get; }
        public EmPaymentRequestType? TypePayment { set; get; }
        public DateTime? PaymentRequestDate { set; get; }
        public byte[] RowVersion { get; set; }
        public string CreatedByName { set; get; }
    }
}
