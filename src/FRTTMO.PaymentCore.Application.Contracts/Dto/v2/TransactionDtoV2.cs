using System;
using System.Collections.Generic;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto.v2
{
    public class TransactionDtoV2
    {
        public Guid? PaymentRequestId { set; get; }
        public Guid? AccountId { set; get; }
        public EnmTransactionType? TransactionTypeId { set; get; }
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
    public class TransactionFullOutputDtoV2 : TransactionDtoV2
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }

    public class GetByPaymentRequestInfoInputV2
    {
        public string PaymentRequestCode { set; get; }
        public List<EnmTransactionType> TransactionTypeIds { set; get; }
        public DateTime? PaymentRequestDate { set; get; }
    }

    public class InsertTransactionInputDtoV2 : TransactionDtoV2
    {
        public DateTime CreatedDate { set; get; }
        public string CreatedBy { set; get; }
    }
    public class GetByListPaymentRequestIdsInputDtoV2
    {
        public string PaymentRequestCode { set; get; }
        public DateTime? PaymentRequestDate { set; get; }
    }
}
