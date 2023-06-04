using System;

namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class CODEto
    {
        public Guid? TransactionId { set; get; }
        public Guid? TransporterID { set; get; }
        public string TransporterName { set; get; }
        public string waybillnumber { set; get; }
        public decimal? Amount { set; get; }
    }
    public class CODFullOutputEto : CODEto
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
}
