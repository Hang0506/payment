using System;

namespace FRTTMO.PaymentCore.Entities
{
    public class COD : BaseEntity<Guid>
    {
        public Guid? TransactionId { set; get; }
        public Guid? TransporterID { set; get; }
        public string TransporterName { set; get; }
        public string waybillnumber { set; get; }
        public decimal? Amount { set; get; }
        public int? TransporterCode { get; set; }
    }
}
