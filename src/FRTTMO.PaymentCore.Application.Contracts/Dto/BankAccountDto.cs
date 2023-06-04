
using System;

namespace FRTTMO.PaymentCore.Dto
{
    public class BankAccountDto
    {
        public int BankId { set; get; }
        public string AccountNum { set; get; }
        public string AccountName { set; get; }        
        public string BranchName { set; get; }
        public bool? IsDefault { set; get; }

    }

    public class BankAccountFullOutputDto : BankAccountDto
    {
        public int Id { set; get; }
        public string BankName { set; get; }
        public string BankCode { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
}
