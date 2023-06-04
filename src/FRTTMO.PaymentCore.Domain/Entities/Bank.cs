namespace FRTTMO.PaymentCore.Entities
{
    public class Bank : BaseEntity<int>
    {
        public string BankCode { set; get; }
        public string BankName { set; get; }
        public bool IsDefault { set; get; }
        //public byte? Type { set; get; }
        public bool IsDeleted { set; get; }

    }
}
