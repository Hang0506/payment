using System;

namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class EWalletDepositFullOutputEto
    {
        public Guid? TransactionId { set; get; }
        public string TransactionVendor { set; get; }
        public int? TypeWalletId { set; get; }
        public decimal? Amount { set; get; }
        public Guid Id { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
}
