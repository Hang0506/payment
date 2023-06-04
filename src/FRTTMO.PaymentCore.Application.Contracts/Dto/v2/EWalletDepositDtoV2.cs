using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto.v2
{
    public class PaymentEWalletDepositInputDtoV2
    {
        public string TransactionVendor { set; get; }
        public EnmWalletProvider? TypeWalletId { set; get; }
        public decimal? Amount { set; get; }
        //public decimal? RealAmount { set; get; }
    }
}
