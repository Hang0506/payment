namespace FRTTMO.PaymentCore.Entities
{
    public class BankAccount : BaseEntity<int>
    {
        public int BankId { set; get; }
        public string AccountNum { set; get; }
        public string AccountName { set; get; }
        public string BranchName { set; get; }
        public bool? IsDefault { set; get; }
    }
    public class BankAccountFull : BaseEntity<int>
    {
        public int Id { set; get; }
        public int BankId { set; get; }
        public string AccountNum { set; get; }
        public string AccountName { set; get; }
        public string BranchName { set; get; }
        public bool? IsDefault { set; get; }
        public string BankName { set; get; }
        public string BankCode { set; get; }
    }
}
