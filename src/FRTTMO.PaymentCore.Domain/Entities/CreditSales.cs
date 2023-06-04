using System;

namespace FRTTMO.PaymentCore.Entities
{
    public class CreditSales : BaseEntity<Guid>
    {
        public string TaxCode { set; get; }
        public Guid? TransactionId { set; get; }
        public Guid? CustomerId { set; get; }
        public string CustName { set; get; }
        public string Phone { set; get; }
        public decimal? Amount { set; get; }
    }
}
