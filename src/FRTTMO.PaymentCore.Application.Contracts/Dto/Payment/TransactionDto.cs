using System;
using System.Collections.Generic;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class TransactionDto
    {
        public Guid? AccountId { set; get; }
        public EnmTransactionType? TransactionTypeId { set; get; }
        public Guid? PaymentRequestId { set; get; }
        public string PaymentRequestCode { set; get; }
        public string ShopCode { set; get; }
        public EnmPaymentMethod? PaymentMethodId { set; get; }
        public decimal? Amount { set; get; }
        public decimal? TransactionFee { set; get; }
        public DateTime? TransactionTime { set; get; }
        public string Note { set; get; }
        public string AdditionAttributes { set; get; }
        public EnmTransactionStatus? Status { set; get; }
        public DateTime? PaymentRequestDate { set; get; }
    }
    public class TransactionFullOutputDto : TransactionDto
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }

    public class GetByPaymentRequestInfoInput
    {
        public Guid paymentRequestId { set; get; }
        public List<EnmTransactionType> transactionTypeIds { set; get; }
        public DateTime? paymentRequestDate { set; get; }
    }

    public class InsertTransactionInputDto : TransactionDto
    {
        public DateTime CreatedDate { set; get; }
        public string CreatedBy { set; get; }
    }
}
