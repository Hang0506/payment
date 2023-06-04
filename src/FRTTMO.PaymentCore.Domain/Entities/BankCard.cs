namespace FRTTMO.PaymentCore.Entities
{
    public class BankCard : BaseEntity<int>
    {
        public string BankCode { set; get; }
        public string BankName { set; get; }
        public bool IsDefault { set; get; }
        public bool IsDeleted { set; get; }
        public byte? Paymethod { get; set; }
    }
}
