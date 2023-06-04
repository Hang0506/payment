using System;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Entities
{
    public class Voucher : BaseEntity<Guid>
    {
        public string Code { set; get; }
        public Guid? TransactionId { set; get; }
        public string Name { set; get; }
        public decimal? Amount { set; get; }
        public EnmVoucherProvider? VoucherType { get; set; }
    }
}
