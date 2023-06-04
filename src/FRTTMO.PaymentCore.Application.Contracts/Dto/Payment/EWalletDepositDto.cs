using System;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public interface IEWalletPayed
    {
        bool IsPayed { get; }
    }
    public class EWalletDepositDto
    {
        public Guid? TransactionId { set; get; }
        public string TransactionVendor { set; get; }
        public EnmWalletProvider? TypeWalletId { set; get; }
        public decimal? Amount { set; get; }
        public decimal? RealAmount { set; get; }
    }
    public class EWalletDepositInputDto : EWalletDepositDto
    {
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
    }
    public class EWalletDepositFullOutputDto : EWalletDepositDto
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }

    public class PaymentEWalletDepositInputDto
    {
        public string TransactionVendor { set; get; }
        public EnmWalletProvider? TypeWalletId { set; get; }
        public decimal? Amount { set; get; }
        public decimal? RealAmount { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
    }
}
