using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class CancelDepositDto
    {
        [Required]
        [MinLength(20)]
        public string OrderCode { get; set; }
        public decimal TotalPayment { set; get; }
        public string CreatedBy { set; get; }
        public string CreatedByName { set; get; }
        [Description("PaymentCoreRequest :1,PaymentDebitRequest:2,PaymentWithdrawDeposit:3")]
        private EmPaymentRequestType? TypePaymentValue;
        public EmPaymentRequestType? TypePayment
        {
            set
            {
                TypePaymentValue = value.Value;
            }
            get
            {
                return (TypePaymentValue == null ? EmPaymentRequestType.PaymentCoreRequest : TypePaymentValue.Value);
            }
        }
        public InsertTransactionCancelInputDto Transaction { get; set; }
    }
    public class InsertTransactionCancelInputDto : TransactionCancelDto
    {
        public DateTime CreatedDate { set; get; }
        public string CreatedBy { set; get; }
    }
    public class TransactionCancelDto
    {
        public Guid? AccountId { set; get; }
        public EnmTransactionType? TransactionTypeId { set; get; }
        public string ShopCode { set; get; }
        public EnmPaymentMethod? PaymentMethodId { set; get; }
        public decimal? Amount { set; get; }
        public decimal? TransactionFee { set; get; }
        public DateTime? TransactionTime { set; get; }
        public string Note { set; get; }
        public string AdditionAttributes { set; get; }
        public EnmTransactionStatus? Status { set; get; }
    }
    public class PaymentPayMethodDto
    {
        public int Id { get; set; }
        public string Name { set; get; }
        public List<PaymentMethodDetailDto> Detail { get; set; }
        public bool Isdefault { get; set; }
    }

    public class PaymentMethodDetailDto
    {
        public int Id { set; get; }
        public string Name { set; get; }
    }
    public class CashbackDepositRefundBaseDto
    {
        public string OrderCode { get; set; }
        public decimal TotalPayment { set; get; }
        public InsertTransactionInputDto Transaction { get; set; }
    }
    public class TransactionCancelDepositTransfer
    {
        public Guid? PaymentRequestId { set; get; }
        public Guid? CustomerId { set; get; }
        public string ShopCode { set; get; }
        public DateTime? TransactionTime { set; get; }
        public PaymentTransferInputDto Transfers { get; set; }
        public string CreatedByName { set; get; }
    }

    public class PaymentRequestTransferDto
    {
        public string OrderCode { set; get; }
        public decimal TotalPayment { set; get; }
        public string CreatedBy { set; get; }
        public string CreatedByName { set; get; }
    }
}
