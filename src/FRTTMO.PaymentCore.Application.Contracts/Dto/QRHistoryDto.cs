using System;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class QRHistoryDetailDto
    {
        public EnmPartnerId? PartnerId { set; get; }
        public string PartnerName { set; get; }
        public string QrCode { set; get; }
        public string Response { set; get; }
        public bool? IsPayed { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string Message { set; get; }
        public decimal? DebitAmount { set; get; }
        public decimal? RealAmount { set; get; }
        public decimal? OrgAmount { get; set; }
        public string TxnCode { set; get; }
    }
}
