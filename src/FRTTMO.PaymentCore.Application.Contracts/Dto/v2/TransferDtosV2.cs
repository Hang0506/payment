using System;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto.v2
{
    public class PaymentTransferInputDtoV2
    {
        public string AccountNum { set; get; }
        public string AccountName { set; get; }
        public string BankName { set; get; }
        public DateTime? DateTranfer { set; get; }
        public string Image { set; get; }
        public string TransferNum { set; get; }
        public string Content { set; get; }
        public decimal? Amount { set; get; }
        public byte? IsConfirm { set; get; }
        public string ReferenceBanking { set; get; }
        public EnmPartnerId? PartnerId { set; get; }
    }
    public class TransferFullOutputDtoV2 : TransferDto
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
}
