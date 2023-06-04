using System;

namespace FRTTMO.PaymentCore.Entities
{
    public class Account: BaseEntity<Guid>
    {
        public Guid CustomerId { get; set; }
        public Guid? AccountNumber { get; set; }
        public decimal? CurrentBalance { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
