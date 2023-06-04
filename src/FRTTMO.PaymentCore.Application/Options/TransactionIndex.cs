using FRTTMO.PaymentCore.Dto.TransactionDetailDto;
using FRTTMO.PaymentCore.Dto.v2;
using Nest;
using System;
using System.Collections.Generic;

namespace FRTTMO.PaymentCore.Options
{
    public class TransactionDepositAllIndex : DepositCoresAllOutputDtoV2
    {
        public string id { get; set; }
    }
    public class TransactionIndex : TransactionDetailTransferOutputDto
    {
        public string id { get; set; }
    }

    public class DepositAllIndex : DepositAllDto
    {
        public string id { get; set; }
    }
    [ElasticsearchType(RelationName = "payment", IdProperty = "PaymentCore")]

    public class PaymentTransIndex
    {
        public string id { get; set; }
        [Nested] public Detail Detail { get; set; }
    }
    public class HeaderFinalIndex : BaseHeadFinal
    {
        public string id { get; set; }
    }
    public class BaseHeadFinal
    {
        [Keyword] public string PaymentCode { get; set; }
        [Keyword] public string ShopCode { get; set; }
        [Keyword] public Guid? CustomerId { get; set; }
        public decimal? Total { get; set; }
        public byte? Type { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public decimal? RemainingAmount { get; set; }
        public bool Status { get; set; }
        public bool IsPayment { get; set; }
        [Nested] public List<PaymentSourceId> PaymentSource { get; set; }
        public string PaymentRequestCode { get; set; }
        public string PaymentRequestCodeV2 { get; set; }
    }
}
