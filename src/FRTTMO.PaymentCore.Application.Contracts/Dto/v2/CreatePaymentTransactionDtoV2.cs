using System;
using System.Collections.Generic;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto.v2
{

    public class PaymentTransactionInputDtoV2
    {
        public string PaymentRequestCode { set; get; }
        public string ShopCode { set; get; }
        public decimal? TransactionFee { set; get; }
        public DateTime? TransactionTime { set; get; }
        public decimal Amount { set; get; }
        public string Note { set; get; }
        public string AdditionAttributes { set; get; }
        public string CreatedBy { set; get; }
    }
    public class PaymentTransactionDtoV2
    {
        public string ShopCode { set; get; }
        public decimal? TransactionFee { set; get; }
        public DateTime? TransactionTime { set; get; }
        public string Note { set; get; }
        public string AdditionAttributes { set; get; }
        public string CreatedBy { set; get; }
    }
    public class CreatePaymentTransactionInputDtoV2
    {
        public Guid? AccountId { set; get; }
        public decimal Total { get; set; }
        public string PaymentRequestCode { set; get; }
        public string OrderCode { get; set; }
        public string PaymentCode { get; set; }
        public PaymentTransactionDtoV2 Transaction { get; set; }
    }

    public class CreatePaymentTransactionOutputDtoV2
    {
        public TransactionFullOutputDtoV2 Transaction { get; set; }
    }

    public class PaymentRequestCompletedOutputDtoV2
    {
        public string OrderCode { get; set; }
        public string PaymentRequestCode { get; set; }
        public string PaymentCode { get; set; }
        public TransactionFullOutputDtoV2 Transaction { get; set; }
        public List<TransactionFullOutputDtoV2> DepositTransactions { get; set; }
        public Guid? OrderReturnId { get; set; }
        public EmPaymentRequestType? TypePayment { get; set; }
    }

    public class CreateWithdrawDepositInputDtoV2
    {        
        public string OrderCode { get; set; }
        public Guid? AccountId { set; get; }
        public PaymentTransactionInputDtoV2 Transaction { get; set; }
    }
    public class CreateWithdrawDepositTransferInputDtoV2
    {
        public string OrderCode { get; set; }
        public Guid? AccountId { set; get; }
        public PaymentTransactionInputDtoV2 Transaction { get; set; }
        public List<PaymentTransferInputDto> Transfers { get; set; }
    }
    public class CreateWithdrawDepositTransferOutputDtoV2
    {
        public TransactionFullOutputDtoV2 Transaction { get; set; }
        public List<TransferFullOutputDto> Transfers { get; set; }
    }

    public class CreatePaymentInputDto
    {
        public string PaymentCode { get; set; }
        public decimal Amount { set; get; }
        public EnmPaymentType Type { get; set; }
        public string SourceCode { get; set; }
        public string CreatedBy { set; get; }
        public EnmPaymentSourceCode? PaymentSourceType { set; get; }
    }
    public class PaymentTransactionBaseDto
    {
        public EnmPaymentSourceCode? Type { set; get; }
        public string SourceCode { get; set; }
        public decimal? Amount { set; get; }
    }
    public class CreatePaymentOutputDto
    {
        public Guid Id { get; set; }
        public string PaymentCode { set; get; }
        public decimal? Total { set; get; }
        public EnmPaymentType Type { set; get; }
        public DateTime? PaymentDate { set; get; }
        public byte PaymentVersion { set; get; }
        public EnmPaymentStatus? Status { set; get; }
    }

}
