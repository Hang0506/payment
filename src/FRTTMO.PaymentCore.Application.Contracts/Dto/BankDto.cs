using System;

namespace FRTTMO.PaymentCore.Dto
{
    public class BankDto
    {        
        public string BankCode { set; get; }
        public string BankName { set; get; }
        public bool IsDefault { set; get; }
    }

    public class BankFullOutputDto : BankDto
    {
        public int Id { set; get; } 
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
}
