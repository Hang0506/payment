using FRTTMO.PaymentCore.Kafka.Eto;
using System;
using System.Collections.Generic;
using static FRTTMO.PaymentCore.Common.EnumType;
using FRTTMO.PaymentCore.Entities;

namespace FRTTMO.PaymentCore.Dto
{

    public class PaymentTransactionInputDto
    {
        public Guid? PaymentRequestId { set; get; }
        public string ShopCode { set; get; }
        public decimal? TransactionFee { set; get; }
        public DateTime? TransactionTime { set; get; }
        public decimal Amount { set; get; }
        public string Note { set; get; }
        public string AdditionAttributes { set; get; }
        public DateTime CreatedDate { set; get; }
        public string CreatedBy { set; get; }
    }
    public class CreatePaymentTransactionInputDto
    {
        public Guid AccountId { set; get; }
        public string PaymentRequestCode { get; set; }
        public string OrderCode { get; set; }
        public PaymentTransactionInputDto Transaction { get; set; }
    }

    public class CreatePaymentTransactionOutputDto
    {
        public TransactionFullOutputDto Transaction { get; set; }
    }

    public class PaymentRequestCompletedOutputDto
    {
        public string OrderCode { get; set; }
        public string PaymentRequestCode { get; set; }
        public TransactionFullOutputDto Transaction { get; set; }
        public List<TransactionFullOutputDto> DepositTransactions { get; set; }
        public Guid? OrderReturnId { get; set; }
        public EmPaymentRequestType? TypePayment { get; set; }
    }

    public class CreateWithdrawDepositInputDto
    {
        public string OrderCode { get; set; }
        public Guid? AccountId { set; get; }
        public PaymentTransactionInputDto Transaction { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
    }
    public class CreateWithdrawDepositTransferInputDto
    {
        public string OrderCode { get; set; }
        public Guid? AccountId { set; get; }
        public PaymentTransactionInputDto Transaction { get; set; }
        public List<PaymentTransferInputDto> Transfers { get; set; }
    }
    public class CreateWithdrawDepositTransferOutputDto
    {
        public TransactionFullOutputDto Transaction { get; set; }
        public List<TransferFullOutputDto> Transfers { get; set; }
    }
    public class TransactionDetailPaymentMethod
    {
        public TransactionFullOutputEto CashDetail { get; set; }
        public List<CardDetail> CardDetail { get; set; }
        public List<TransferDetail> TransferDetail { get; set; }
        public List<VoucherDetail> VoucherDetail { get; set; }
        public List<EWalletDepositDetail> EWalletDepositDetail { get; set; }
        public List<CODDetail> CODDetail { get; set; }
    }
    public class CardDetail : BasePaymentDetail
    {
        public Guid? TransactionId { set; get; }
        public string CardNumber { set; get; }
        public byte? CardType { set; get; }
        public string BankName { set; get; }
        public decimal? Amount { set; get; }
    }
    public class BasePaymentDetail
    {
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
    public class TransferDetail : BasePaymentDetail
    {
        public Guid? TransactionId { set; get; }
        public string AccountNum { set; get; }
        public string AccountName { set; get; }
        public string BankName { set; get; }
        public DateTime? DateTranfer { set; get; }
        public string Image { set; get; }
        public string TransferNum { set; get; }
        public string Content { set; get; }
        public decimal? Amount { set; get; }
        public EnmTransferIsConfirm? IsConfirm { set; get; }
        public string ReferenceBanking { set; get; }
    }
    public class VoucherDetail : BasePaymentDetail
    {
        public string Code { set; get; }
        public Guid? TransactionId { set; get; }
        public string Name { set; get; }
        public decimal? Amount { set; get; }
        public EnmVoucherProvider? VoucherType { get; set; }
    }
    public class EWalletDepositDetail : BasePaymentDetail
    {
        public Guid? TransactionId { set; get; }
        public string TransactionVendor { set; get; }
        public EnmWalletProvider? TypeWalletId { set; get; }
        public decimal? Amount { set; get; }
        public decimal? RealAmount { set; get; }
    }
    public class CODDetail : BasePaymentDetail
    {
        public Guid? TransactionId { set; get; }
        public Guid? TransporterID { set; get; }
        public string TransporterName { set; get; }
        public string waybillnumber { set; get; }
        public decimal? Amount { set; get; }
        public int? TransporterCode { get; set; }
    }
    public class CancelDebitDto
    {
        public string OrderCode { set; get; }
        public string ModifyBy { set; get; }
        public DateTime? ModifyDate { set; get; }
        public byte? Status { set; get; }
        public string Description { set; get; }
        public string AccountingName { set; get; }
        public DateTime? AccountingDate { set; get; }
        public DateTime? InComeDate { set; get; }
        public string EmployeeName { set; get; }
        public string PaymentMethod { set; get; }
    }
}
