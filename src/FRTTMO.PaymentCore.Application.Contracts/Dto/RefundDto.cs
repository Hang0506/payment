using System;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class RefundDto
    {
        public string OrderCode { get; set; }
        public Guid CustomerId { get; set; }
        //
        public int TransactionTypeId { get; set; }
        public Guid? PaymentRequestId { get; set; }
        public string ShopCode { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal? Amount { get; set; }
        public decimal? TransactionFee { get; set; }
        public DateTime? TransactionTime { get; set; }
        public string Note { get; set; }
        public string AdditionAttributes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public EnmTransactionStatus? Status { set; get; }
    }
    public class RefundOutputDto
    {
        public Guid Id { get; set; }
        public int? ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
    }
    public class RefundFullOutputDto : RefundDto
    {
        public Guid Id { get; set; }
    }
}
