using System;

namespace FRTTMO.PaymentCore.Dto
{
    public class AccountDto
    {
        public Guid CustomerId { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string CreatedBy { get; set; }
    }
    public class AccountInsertInputDto : AccountDto
    {
    }

    public class AccountUpdateInputDto : AccountDto
    {
        public Guid Id { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? AccountNumber { get; set; }
    }

    public class AccountOutputDto
    {
        public Guid Id { get; set; }
        public int? ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class AccountFullOutputDto : AccountDto
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? AccountNumber { get; set; }
    }
}
