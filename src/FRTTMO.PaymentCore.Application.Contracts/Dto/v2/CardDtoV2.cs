namespace FRTTMO.PaymentCore.Dto.v2
{
    public class PaymentCardInputDtoV2
    {
        public string CardNumber { set; get; }
        public byte? CardType { set; get; }
        public string BankName { set; get; }
        public decimal? Amount { set; get; }
        public string BankCode { set; get; }
        public byte? Paymethod { set; get; }
    }
}
