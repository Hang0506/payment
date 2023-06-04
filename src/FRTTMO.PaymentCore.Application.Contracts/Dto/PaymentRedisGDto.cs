using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class PaymentRedisGetDto
    {
        [JsonPropertyName("paymentcode")]
        public string PaymentCode { get; set; }
        [JsonPropertyName("customerid")]
        public Guid? CustomerId { get; set; }
        [JsonPropertyName("total")]
        public decimal? Total { get; set; }
        [JsonPropertyName("type")]
        public EnmPaymentType? Type { get; set; }
        [JsonPropertyName("paymentdate")]
        public DateTime? PaymentDate { get; set; }
        [JsonPropertyName("createddate")]
        public DateTime? CreatedDate { get; set; }
        [JsonPropertyName("createdby")]
        public string CreatedBy { get; set; }
        [JsonPropertyName("updatedby")]
        public string UpdatedBy { get; set; }
        [JsonPropertyName("remainingamount")]
        public decimal? RemainingAmount { get; set; }
        [JsonPropertyName("status")]
        public bool Status { get; set; }
        [JsonPropertyName("ispayment")]
        public bool IsPayment { get; set; }
    }
}
