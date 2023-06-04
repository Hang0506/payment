using System;

namespace FRTTMO.PaymentCore.Entities
{
    public class Card : BaseEntity<Guid>
    {
        public Guid? TransactionId { set; get; }
        public string CardNumber { set; get; }
        public byte? CardType { set; get; }
        public string BankName { set; get; }
        public decimal? Amount { set; get; }
        public string BankCode { set; get; }
        public byte? Paymethod { set; get; }
    }
}
