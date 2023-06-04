using System;
using System.Collections.Generic;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Kafka.Eto.v2
{
    public class DepositAllEto : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_ASSEMBLER_STREAM;
        public string paymentCode { get; set; }
        public Guid? customerId { get; set; }
        public decimal? total { get; set; }
        public EnmPaymentSourceCode? type { get; set; }
        public DateTime? paymentDate { get; set; }
        public DateTime? createdDate { get; set; }
        public string createdBy { get; set; }
        public string shopcode { get; set; }
        public string updatedBy { get; set; }
        public decimal? remainingamount { get; set; }
        public bool status { get; set; }
        public bool isPayment { get; set; }
        public List<PaymentSourceDto> paymentSource { get; set; }
        public Detail detail { get; set; }
        public string PaymentRequestCode { get; set; }
        public string PaymentRequestCodeV2 { get; set; }
        public string InsertFrom { get; set; }
    }

    public class PaymentSourceDto
    {
        public string id { get; set; }
        public string sourceCode { get; set; }
        public EnmPaymentSourceCode? type { get; set; }
    }

    public class Detail
    {
        public List<Cash> cash { get; set; }
        public List<EWalletAll> eWalletAll { get; set; }
        public List<EWalletOnlineAll> eWalletOnlineAll { get; set; }
        public List<CardsAll> cardsAll { get; set; }
        public List<CodAll> codAll { get; set; }
        public List<TransfersAll> transfersAll { get; set; }
        public List<VouchersAll> vouchersAll { get; set; }
        public List<DebtSaleAll> debtSaleAll { get; set; }
    }

    public class CardsAll
    {
        public string paymentRequestCode { get; set; }
        public TransactionDeposit transaction { get; set; }
        public CardsDetail cardsDetail { get; set; }
    }

    public class CardsDetail
    {
        public string transactionId { get; set; }
        public string cardNumber { get; set; }
        public int? cardType { get; set; }
        public string bankName { get; set; }
        public decimal? amount { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string bankCode { get; set; }
        public byte? paymethod { set; get; }
    }

    public class Cash
    {
        public string paymentRequestCode { get; set; }
        public EnmTransactionType? transactionTypeId { get; set; }
        public EnmPaymentMethod? paymentMethodId { get; set; }
        public decimal? amount { get; set; }
        public decimal? transactionFee { get; set; }
        public DateTime? transactionTime { get; set; }
        public string note { get; set; }
        public string additionAttributes { get; set; }
        public string createdBy { get; set; }
        public string transactionId { get; set; }
        public DateTime? createdDate { get; set; }
    }

    public class CodAll
    {
        public string paymentRequestCode { get; set; }
        public TransactionDeposit transaction { get; set; }
        public CoDetail coDetail { get; set; }
    }

    public class CoDetail
    {
        public string transactionId { get; set; }
        public string transporterID { get; set; }
        public string transporterName { get; set; }
        public string waybillnumber { get; set; }
        public decimal? amount { get; set; }
        public string description { get; set; }
        public int? transporterCode { get; set; }
        public DateTime? createdDate { get; set; }
        public string createdBy { get; set; }
    }

    public class Debit
    {
        public string taxCode { get; set; }
        public string transactionId { get; set; }
        public decimal? amount { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdDate { get; set; }
    }

    public class DebtSaleAll
    {
        public string paymentRequestCode { get; set; }
        public TransactionDeposit transaction { get; set; }
        public Debit debit { get; set; }
    }

    public class EWalletAll
    {
        public string paymentRequestCode { get; set; }
        public TransactionDeposit transaction { get; set; }
        public EWalletDetail eWalletDetail { get; set; }
    }

    public class EWalletDetail
    {
        public string transactionId { get; set; }
        public string transactionVendor { get; set; }
        public EnmWalletProvider? typeWalletId { get; set; }
        public decimal? amount { get; set; }
        public decimal? realAmount { get; set; }
        public DateTime? createdDate { get; set; }
        public string createdBy { get; set; }
    }

    public class EWalletOnlineAll
    {
        public string paymentRequestCode { get; set; }
        public TransactionDeposit transaction { get; set; }
        public EWalletDetail eWalletDetail { get; set; }
    }

    public class TransactionDeposit
    {
        public EnmTransactionType? transactionTypeId { get; set; }
        public EnmPaymentMethod? paymentMethodId { get; set; }
        public decimal? amount { get; set; }
        public decimal? transactionFee { get; set; }
        public DateTime? transactionTime { get; set; }
        public string note { get; set; }
        public string additionAttributes { get; set; }
        public string createdBy { get; set; }
    }

    public class TransferDetailDeposit
    {
        public Guid? transactionId { set; get; }
        public string accountNum { set; get; }
        public string accountName { set; get; }
        public string bankName { set; get; }
        public DateTime? dateTranfer { set; get; }
        public string image { set; get; }
        public string imageOrigin { set; get; }
        public string transferNum { set; get; }
        public string content { set; get; }
        public decimal? amount { set; get; }
        public EnmTransferIsConfirm? isConfirm { set; get; }
        public string referenceBanking { set; get; }
        public EnmPartnerId? partnerId { set; get; }
        public string tranferId { set; get; }
        public DateTime? createdDate { get; set; }
    }

    public class TransfersAll
    {
        public string paymentRequestCode { get; set; }
        public TransactionDeposit transaction { get; set; }
        public TransferDetailDeposit transferDetail { get; set; }
    }

    public class VoucherDetailDeposit
    {
        public string transactionId { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public decimal? amount { get; set; }
        public EnmVoucherProvider? voucherType { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdDate { get; set; }
    }

    public class VouchersAll
    {
        public string paymentRequestCode { get; set; }
        public TransactionDeposit transaction { get; set; }
        public VoucherDetailDeposit voucherDetail { get; set; }
    }
}
