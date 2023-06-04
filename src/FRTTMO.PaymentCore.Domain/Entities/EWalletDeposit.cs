using System;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Entities
{
    public class EWalletDeposit : BaseEntity<Guid>
    {
        public Guid? TransactionId { set; get; }
        public string TransactionVendor { set; get; }
        public EnmWalletProvider? TypeWalletId { set; get; }
        public decimal? Amount { set; get; }
        public decimal? RealAmount { set; get; }
    }
}
