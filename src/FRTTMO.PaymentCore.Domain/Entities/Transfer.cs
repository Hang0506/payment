using System;
using System.ComponentModel.DataAnnotations.Schema;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Entities
{
    public class Transfer : BaseEntity<Guid>
    {
        public Guid? TransactionId { set; get; }
        public string AccountNum { set; get; }
        public string AccountName { set; get; }
        public string BankName { set; get; }
        public DateTime? DateTranfer { set; get; }
        public string Image { set; get; }
        [Column(TypeName = "varchar(40)")]
        public string TransferNum { set; get; }
        public string Content { set; get; }
        public decimal? Amount { set; get; }
        public EnmTransferIsConfirm? IsConfirm { set; get; }
        public string ReferenceBanking { set; get; }
        public EnmPartnerId? PartnerId { set; get; }
        public string UserConfirm { set; get; }
    }
}
