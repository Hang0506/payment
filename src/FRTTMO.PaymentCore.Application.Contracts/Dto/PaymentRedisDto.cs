using System;
using System.Collections.Generic;
using System.Text;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class PaymentRedisDetailDto : RedisItemDto
    {
        public string ShopCode { get; set; }
        public Guid? CustomerId { get; set; }
        public decimal? Total { get; set; }
        public EnmPaymentType? Type { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public decimal? RemainingAmount { get; set; }
        public bool Status { get; set; }
        public bool IsPayment { get; set; }
        public List<PaymentSourceRedis> PaymentSource { get; set; }
        public DetailRedis Detail { get; set; }
        public string PaymentRequestCode { get; set; }
        public string PaymentRequestCodeV2 { get; set; }
    }
    public class PaymentSourceRedis
    {
        public string id { get; set; }
        public string sourceCode { get; set; }
        public EnmPaymentSourceCode? type { get; set; }
    }
    public class PaymentRedisDto : RedisItemDto
    {
        public List<PaymentRedisDetailDto> Items { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class DetailRedis
    {
        public List<CashRedis> Cash { get; set; }
        public List<EWalletAllRedis> EWalletAll { get; set; }
        public List<EWalletOnlineAllRedis> EWalletOnlineAll { get; set; }
        public List<CardsAllRedis> CardsAll { get; set; }
        public List<CodAllRedis> CodAll { get; set; }
        public List<TransfersAllRedis> TransfersAll { get; set; }
        public List<VouchersAllRedis> VouchersAll { get; set; }
        public List<DebtSaleAllRedis> DebtSaleAll { get; set; }
    }

    public class CardsAllRedis
    {
        public string PaymentRequestCode { get; set; }
        public TransactionDepositRedis Transaction { get; set; }
        public CardsDetailRedis CardsDetail { get; set; }
    }

    public class CardsDetailRedis
    {
        public string TransactionId { get; set; }
        public string CardNumber { get; set; }
        public int? CardType { get; set; }
        public string BankName { get; set; }
        public decimal? Amount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string BankCode { get; set; }
        public byte? Paymethod { set; get; }
    }

    public class CashRedis
    {
        public string PaymentRequestCode { get; set; }
        public EnmTransactionType? TransactionTypeId { get; set; }
        public EnmPaymentMethod? PaymentMethodId { get; set; }
        public decimal? Amount { get; set; }
        public decimal? TransactionFee { get; set; }
        public DateTime? TransactionTime { get; set; }
        public string Note { get; set; }
        public string AdditionAttributes { get; set; }
        public string CreatedBy { get; set; }
        public string TransactionId { get; set; }
        public DateTime? CreatedDate { set; get; }
    }

    public class CodAllRedis
    {
        public string PaymentRequestCode { get; set; }
        public TransactionDepositRedis Transaction { get; set; }
        public CoDetailRedis CoDetail { get; set; }
    }

    public class CoDetailRedis
    {
        public string TransactionId { get; set; }
        public string TransporterID { get; set; }
        public string TransporterName { get; set; }
        public string Waybillnumber { get; set; }
        public decimal? Amount { get; set; }
        public string Description { get; set; }
        public int? TransporterCode { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }

    public class DebitRedis
    {
        public string TaxCode { get; set; }
        public string TransactionId { get; set; }
        public decimal? Amount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { set; get; }
    }

    public class DebtSaleAllRedis
    {
        public string PaymentRequestCode { get; set; }
        public TransactionDepositRedis Transaction { get; set; }
        public DebitRedis Debit { get; set; }
    }

    public class EWalletAllRedis
    {
        public string PaymentRequestCode { get; set; }
        public TransactionDepositRedis Transaction { get; set; }
        public EWalletDetailRedis EWalletDetail { get; set; }
    }

    public class EWalletDetailRedis
    {
        public string TransactionId { get; set; }
        public string TransactionVendor { get; set; }
        public EnmWalletProvider? TypeWalletId { get; set; }
        public decimal? Amount { get; set; }
        public decimal? RealAmount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }

    public class EWalletOnlineAllRedis
    {
        public string PaymentRequestCode { get; set; }
        public TransactionDepositRedis Transaction { get; set; }
        public EWalletDetailRedis EWalletDetail { get; set; }
    }

    public class QrHistoryRedis
    {
        public string PaymentRequestCode { get; set; }
        public int? PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string QrCode { get; set; }
        public string Response { get; set; }
        public bool IsPayed { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Message { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? RealAmount { get; set; }
        public decimal? OrgAmount { get; set; }
        public string TxnCode { get; set; }
    }

    public class TransactionDepositRedis
    {
        public EnmTransactionType? TransactionTypeId { get; set; }
        public EnmPaymentMethod? PaymentMethodId { get; set; }
        public decimal? Amount { get; set; }
        public decimal? TransactionFee { get; set; }
        public DateTime? TransactionTime { get; set; }
        public string Note { get; set; }
        public string AdditionAttributes { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { set; get; }
    }

    public class TransferDetailDepositRedis
    {
        public Guid? TransactionId { set; get; }
        public string AccountNum { set; get; }
        public string AccountName { set; get; }
        public string BankName { set; get; }
        public DateTime? DateTranfer { set; get; }
        public string Image { set; get; }
        public string ImageOrigin { set; get; }
        public string TransferNum { set; get; }
        public string Content { set; get; }
        public decimal? Amount { set; get; }
        public EnmTransferIsConfirm? IsConfirm { set; get; }
        public string ReferenceBanking { set; get; }
        public EnmPartnerId? PartnerId { set; get; }
        public string TranferId { set; get; }
        public DateTime? CreatedDate { set; get; }
    }

    public class TransfersAllRedis
    {
        public string PaymentRequestCode { get; set; }
        public TransactionDepositRedis Transaction { get; set; }
        public TransferDetailDepositRedis TransferDetail { get; set; }
    }

    public class VoucherDetailDepositRedis
    {
        public string TransactionId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal? Amount { get; set; }
        public EnmVoucherProvider? VoucherType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { set; get; }
    }

    public class VouchersAllRedis
    {
        public string PaymentRequestCode { get; set; }
        public TransactionDepositRedis Transaction { get; set; }
        public VoucherDetailDepositRedis VoucherDetail { get; set; }
    }
}
