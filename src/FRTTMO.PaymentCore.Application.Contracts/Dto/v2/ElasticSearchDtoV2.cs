using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto.v2
{
    public class DepositAllDto
    {
        public string PaymentCode { get; set; }
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
        public List<PaymentSourceId> PaymentSource { get; set; }
        public Detail Detail { get; set; }
        public string PaymentRequestCode { get; set; }
        public string PaymentRequestCodeV2 { get; set; }
    }

    public class PaymentSourceId
    {
        [Keyword] public string Id { get; set; }
        [Keyword] public string SourceCode { get; set; }
        [Keyword] public EnmPaymentSourceCode? Type { get; set; }
    }

    public class Detail
    {
        public List<Cash> Cash { get; set; }
        public List<EWalletAll> EWalletAll { get; set; }
        public List<EWalletOnlineAll> EWalletOnlineAll { get; set; }
        public List<CardsAll> CardsAll { get; set; }
        public List<CodAll> CodAll { get; set; }
        public List<TransfersAll> TransfersAll { get; set; }
        public List<VouchersAll> VouchersAll { get; set; }
        public List<DebtSaleAll> DebtSaleAll { get; set; }
        public List<QrHistory> QrHistory { get; set; }
    }

    public class CardsAll
    {
        public string PaymentRequestCode { get; set; }
        public TransactionDeposit Transaction { get; set; }
        public CardsDetail CardsDetail { get; set; }
    }

    public class CardsDetail
    {
        public string TransactionId { get; set; }
        public string CardNumber { get; set; }
        public int? CardType { get; set; }
        public string BankName { get; set; }
        public decimal? Amount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string BankCode { set; get; }
        public byte? Paymethod { set; get; }
    }

    public class Cash
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

    public class CodAll
    {
        public string PaymentRequestCode { get; set; }
        public TransactionDeposit Transaction { get; set; }
        public CoDetail CoDetail { get; set; }
    }

    public class CoDetail
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

    public class Debit
    {
        public string TaxCode { get; set; }
        public string TransactionId { get; set; }
        public decimal? Amount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { set; get; }
    }

    public class DebtSaleAll
    {
        public string PaymentRequestCode { get; set; }
        public TransactionDeposit Transaction { get; set; }
        public Debit Debit { get; set; }
    }

    public class EWalletAll
    {
        public string PaymentRequestCode { get; set; }
        public TransactionDeposit Transaction { get; set; }
        public EWalletDetail EWalletDetail { get; set; }
    }

    public class EWalletDetail
    {
        public string TransactionId { get; set; }
        public string TransactionVendor { get; set; }
        public EnmWalletProvider? TypeWalletId { get; set; }
        public decimal? Amount { get; set; }
        public decimal? RealAmount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }

    public class EWalletOnlineAll
    {
        public string PaymentRequestCode { get; set; }
        public TransactionDeposit Transaction { get; set; }
        public EWalletDetail EWalletDetail { get; set; }
    }

    public class QrHistory
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

    public class TransactionDeposit
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

    public class TransferDetailDeposit
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

    public class TransfersAll
    {
        public string PaymentRequestCode { get; set; }
        public TransactionDeposit Transaction { get; set; }
        public TransferDetailDeposit TransferDetail { get; set; }
    }

    public class VoucherDetailDeposit
    {
        public string TransactionId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal? Amount { get; set; }
        public EnmVoucherProvider? VoucherType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { set; get; }
    }

    public class VouchersAll
    {
        public string PaymentRequestCode { get; set; }
        public TransactionDeposit Transaction { get; set; }
        public VoucherDetailDeposit VoucherDetail { get; set; }
    }
    public class MigrationPaymentrequestCodeOutnputDto
    {
        public MigrationSuscessDto migrationSuscessDto { set; get; }
        public MigrationErrorDto migrationErrorDto { set; get; }
    }
    public class MigrationSuscessDto
    {
        public List<string> PaymentCode { set; get; }
    }
    public class MigrationErrorDto
    {
        public List<string> PaymentrequestCode { set; get; }
    }
    public class MigrationPaymentrequestCodeInnputDto
    {
        public DateTime? StartDate { set; get; }
        public DateTime? EndDate { set; get; }
        public string PaymentCode { set; get; }
    }
    public class RequestListPaymentMethodEs
    {
        public List<string> OrdersCode { get; set; }

    }
    public class RequestPaymentPay
    {
        public List<string> PaymentCode { get; set; }
    }

    public class ResponseMethodEs
    {
        public string Name { get; set; }
        //paymentcode thu tiền đơn hàng
        public string PaymentCode { get; set; }
    }
}
