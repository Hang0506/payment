using System;

namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class CardFullOutputEto
    {
        public Guid? TransactionId { set; get; }
        public string CardNumber { set; get; }
        public byte? CardType { set; get; }
        public string BankName { set; get; }
        public decimal? Amount { set; get; }

        public Guid Id { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
}
