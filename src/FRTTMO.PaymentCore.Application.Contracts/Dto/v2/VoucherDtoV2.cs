using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto.v2
{
    public class PaymentVoucherInputDtoV2
    {
        public string Code { set; get; }
        public string Name { set; get; }
        public decimal? Amount { set; get; }
        public EnmVoucherProvider? VoucherType { get; set; }
    }
}
