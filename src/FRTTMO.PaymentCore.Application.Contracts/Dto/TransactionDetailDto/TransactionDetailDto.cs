using FRTTMO.PaymentCore.Dto.v2;
using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto.TransactionDetailDto
{
    public class TransactionDetailOutputDto //: TransactionDetailPaymentMethod
    {
        public string OrderCode { get; set; }
        public string PaymentCode { get; set; }
        public string TransactionID { get; set; }
        public DateTime? CreatedDate { get; set; }
        public decimal? Amount { get; set; }
        public int? PaymentMethodId { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
    }
    public class TransferInfoDetailOutputDto
    {
        public int? Total { set; get; }
        public List<TransactionDetailTransferOutputDto> Result { set; get; }
    }
    public class DepositCoresAllOutputFullDtoV2 : MaskDepositInputBaseDtoV2
    {
        public string PaymentRequestCode { get; set; }
        public string TransactionID { get; set; }
        public DateTime? createdDate { get; set; }
        public decimal? Amount { get; set; }
        public int? PaymentMethodId { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public Guid? AccountId { set; get; }
        public int? TransactionTypeId { set; get; }
        public string ShopCode { set; get; }
        //public decimal? Amount { set; get; }
        public EnmPaymentRequestStatus? PaymentRequestStatus { set; get; }
        public PaymentInfoDepositDetailDtoV2 Detail { set; get; }

    }
    public class PaymentInfoDepositDetailDtoV2 : TransactionFullOutputDtoV2
    {
        public TransactionFullInputDepositDetailDto Detail { set; get; }
    }
    public class TransactionFullInputDepositDetailDto
    {
        //public List<EWalletDepositFullOutputV2Dto> EWallets { set; get; }
        //public List<EWalletDepositFullOutputV2Dto> EWalletOnline { set; get; }
        //public List<CardFullOutputV2Dto> Cards { set; get; }
        //public List<VoucherFullOutputV2Dto> Vouchers { set; get; }
        //public List<CodFullOutputDtoV2Dto> CODs { set; get; }
        //public List<TransferFullOutputV2Dto> Transfers { set; get; }
        //public List<CashFullOutputV2Dto> Cash { set; get; }
    }
    public class TransactionDetailTransferOutputDto
    {
        [Keyword] public string PaymentCode { get; set; }
        public List<SourceCodeSyncES> SourceCode { get; set; }
        public string TransactionId { get; set; }
        public string PaymentRequestCode { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? CreateDatePayment { get; set; }
        public decimal? AmountPayment { get; set; }
        public byte? PaymentSoureType { set; get; }
        public byte? TypePayment { set; get; }
        public byte? PaymentMethodId { get; set; }
        public string CreatedBy { get; set; }
        public byte? IsConfirmTransfer { get; set; }
        public string ShopCode { get; set; }
        public byte? StatusFill { get; set; }
        public List<TransferSyncAll> TransferAll { get; set; }
        public List<ResponseMethodEs> ListMethodId { get; set; }
    }
    public class SearchESByPaymentCodeRequestDto
    {
        [Required]
        public List<string> PaymentCode { get; set; }
    }
    public class TransactionTransferDto
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
        public Guid? PaymentRequestId { set; get; }
        public Guid? AccountId { set; get; }
        public EnmTransactionType? TransactionTypeId { set; get; }
        public string PaymentRequestCode { set; get; }
        public string ShopCode { set; get; }
        public EnmPaymentMethod? PaymentMethodId { set; get; }
        public decimal? Amount { set; get; }
        public decimal? TransactionFee { set; get; }
        public DateTime? TransactionTime { set; get; }
        public string Note { set; get; }
        public string AdditionAttributes { set; get; }
        public EnmTransactionStatus? Status { set; get; }
        public DateTime? PaymentRequestDate { set; get; }
    }
    public class TransferFullSyncESDto
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
        public Guid? TransactionId { set; get; }
        public string AccountNum { set; get; }
        public string AccountName { set; get; }
        public string BankName { set; get; }
        public DateTime? DateTranfer { set; get; }
        public string Image { set; get; }
        public string TransferNum { set; get; }
        public string Content { set; get; }
        public decimal? Amount { set; get; }
        public byte? IsConfirm { set; get; }
        public string UserConfirm { set; get; }
        public string CreatedByName { set; get; }
        public string ReferenceBanking { set; get; }
        public EnmPartnerId? PartnerId { set; get; }
    }
    public class PaymentRequestSyncOutPutDto
    {
        public string PaymentCode { set; get; }
        public string OrderCode { set; get; }
        public Guid? OrderReturnId { set; get; }
        public string PaymentRequestCode { set; get; }
        public decimal TotalPayment { set; get; }
        public EnmPaymentRequestStatus? Status { set; get; }
        public EmPaymentRequestType? TypePayment { set; get; }
        public DateTime? PaymentRequestDate { set; get; }
        public byte[] RowVersion { get; set; }
        public string CreatedByName { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
    public class PaymentSyncDto
    {
        public string PaymentCode { set; get; }
        public decimal? Total { set; get; }
        public EnmPaymentType Type { set; get; }
        public DateTime? PaymentDate { set; get; }
        public byte PaymentVersion { set; get; }
        public EnmPaymentStatus? Status { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
    public class TransferSyncAll
    {
        //public TransactionTransferDto Transaction { set; get; }
        //public TransferFullSyncESDto Transfer { set; get; }
        public Guid? Id { get; set; }
        public int? TransactionTypeId { get; set; }
        public int? PaymentMethodId { get; set; }
        public decimal? Amount { get; set; }
        public Guid? TransactionId { get; set; }
        public string AccountNum { get; set; }
        public string AccountName { get; set; }
        public string BankName { get; set; }
        public DateTime? DateTranfer { get; set; }
        public string TransferNum { get; set; }
        public byte? IsConfirm { get; set; }
        public string ShopCode { get; set; }
    }
    public class TransferInfoInputDto
    {
        public string PaymentCode { set; get; }
        public string SourceCode { set; get; }
        public DateTimeOffset? StartDate { set; get; }
        public DateTimeOffset? EndDate { set; get; }
        public DateTimeOffset? StartDateTranfer { set; get; }
        public DateTimeOffset? EndDateTranfer { set; get; }
        public string ShopCode { set; get; }
        public EnmTransferIsConfirm? IsConfirm { set; get; }
        public StatusFill? StatusFill { set; get; }
        public int PageNumber { set; get; }
        public int PageSize { set; get; }
    }
    public class SourceCodeSyncES
    {
        [Keyword] public string SourceCode { get; set; }
    }
}
