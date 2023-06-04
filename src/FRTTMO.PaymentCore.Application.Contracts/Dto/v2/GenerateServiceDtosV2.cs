using System;

namespace FRTTMO.PaymentCore.Dto.v2
{
    public class GeneratePaymentRequestCodeOutputDtoV2
    {
        public string PaymentRequestCode { get; set; }
        public DateTime? PaymentRequestDate { get; set; }
    }
}
