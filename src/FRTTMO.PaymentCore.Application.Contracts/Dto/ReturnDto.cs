using System;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class ReturnCashDto
    {
        public string OrderCode { get; set; }
        public Guid OrderReturnId { get; set; }
        public decimal Totalpayment { get; set; }
        public string CreatedBy { set; get; }
        public TransactionReturnDto Transaction { get; set; }
    }
    public class TransactionReturnDto
    {
        public Guid? CustomerId { set; get; }
        public EnmTransactionType? TransactionTypeId { set; get; }
        public string ShopCode { set; get; }
        public decimal? Amount { set; get; }
        public decimal? TransactionFee { set; get; }
        public DateTime? TransactionTime { set; get; }
        public string Note { set; get; }
        public string AdditionAttributes { set; get; }
        public EnmTransactionStatus? Status { set; get; }
    }
    public class ReturnTransferDto
    {
        public Guid OrderReturnId { get; set; }
        public string OrderCode { get; set; }
        public Guid? CustomerId { set; get; }
        public string ShopCode { set; get; }
        public DateTime? TransactionTime { set; get; }
        public PaymentTransferInputDto Transfers { get; set; }
    }
}
