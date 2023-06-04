using System;

namespace FRTTMO.PaymentCore.Dto
{
    public class PaymentCODInputDtoV2
    {
        public Guid? TransporterID { set; get; }
        public string TransporterName { set; get; }
        public string waybillnumber { set; get; }
        public decimal? Amount { set; get; }
        public int? TransporterCode { get; set; }
        public string Decription { get; set; }
        public string CreatedBy { set; get; }

    }
}
