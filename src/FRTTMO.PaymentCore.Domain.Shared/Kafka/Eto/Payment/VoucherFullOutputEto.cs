using System;

namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class VoucherFullOutputEto
    {
        public string Code { set; get; }
        public Guid? TransactionId { set; get; }
        public string Name { set; get; }
        public decimal? Amount { set; get; }

        public Guid Id { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
}
