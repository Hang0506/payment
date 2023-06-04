using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FRTTMO.PaymentCore.Dto
{
    public class CallAPIPaymentInputDto
    {
        [Required]
        public string OrderCode { get; set; }
        [Required]
        public DateTime PayDate { get; set; }
        [Required]
        public string PaymentRequestCode { get; set; }
        [Required]
        public string UpdateBy { get; set; }
    }
    public class OrderOutputSupporting
    {
        [JsonPropertyName("orderID")]
        public Guid OrderID { get; set; }
        [JsonPropertyName("custCode")]
        public string CustCode { get; set; }
        [JsonPropertyName("custName")]
        public string CustName { get; set; }
        [JsonPropertyName("orderCode")]
        public string OrderCode { get; set; }
        [JsonPropertyName("phone")]
        public string Phone { get; set; }
        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }
        [JsonPropertyName("createdByName")]
        public string CreatedByName { get; set; }
        [JsonPropertyName("paymentRequestCode")]
        public string PaymentRequestCode { get; set; }
        [JsonPropertyName("paymentRequestID")]
        public string PaymentRequestID { get; set; }
        [JsonPropertyName("shopCode")]
        public string ShopCode { get; set; }
        [JsonPropertyName("orderStatus")]
        public int OrderStatus { get; set; }
        [JsonPropertyName("amountCOD")]
        public decimal? AmountCOD { get; set; }
        [JsonPropertyName("createdDate")]
        public DateTime? CreatedDate { get; set; }
    }
    public class UpdateOMSDto
    {
        [JsonPropertyName("ordercode")]
        public string Ordercode { get; set; }
        [JsonPropertyName("paymentRequestCode")]
        public string PaymentRequestCode { get; set; }
        [JsonPropertyName("updateBy")]
        public string UpdateBy { get; set; }
    }
}
