
using System;

namespace FRTTMO.PaymentCore.Dto
{
    public class BankCardDto
    {
        public string BankCode { set; get; }
        public string BankName { set; get; }
        public bool IsDefault { set; get; }
        public bool IsDeleted { set; get; }
        public byte? Paymethod { get; set; }
    }

    public class BankCardFullOutputDto : BankCardDto
    {
        public int Id { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
}