using System;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class VendorDetailDto
    {
        public EnmPartnerId PartnerId { set; get; }
        public string PromotionCode { set; get; }
        public DateTime? FromDate { set; get; }
        public DateTime? ToDate { set; get; }
        public PaymentChannel Channel { set; get; }
        public bool Active { set; get; }
    }
}
