using System;
using System.Collections.Generic;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class PaymentRequestDto
    {
        public string OrderCode { set; get; }
        public Guid? OrderReturnId { set; get; }
    }
    public class PaymentRequestFullOutputDto : PaymentRequestDto
    {
        public Guid Id { get; set; }
        public string PaymentRequestCode { set; get; }
        public string PaymentCode { get; set; }
        public decimal TotalPayment { set; get; }
        public byte? TypePayment { set; get; }
        public byte? Status { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
        public DateTime? PaymentRequestDate { set; get; }
    }
    public class TranferUpdateInputDto
    {
        public List<Guid> TranferId { set; get; }
        public string UserConfirm { set; get; }
    }
    public class PaymentInfoInputDto
    {
        public EnmTransactionType TransactionTypeId { set; get; }
        public Guid PaymentRequestId { set; get; }

    }
    public class PaymentInfoOutputDto
    {
        public Guid? AccountId { set; get; }
        public EnmTransactionType? TransactionTypeId { set; get; }
        public Guid? PaymentRequestId { set; get; }
        public string ShopCode { set; get; }
        public decimal? Amount { set; get; }
        public EnmPaymentRequestStatus? PaymentRequestStatus { set; get; }
        public List<PaymentInfoDetailDto> Detail { set; get; }
    }

    public class PaymentInfoDetailDto : TransactionFullOutputDto
    {
        public TransactionFullOutputDetailDto Detail { set; get; }
    }
    public class TransactionFullOutputDetailDto
    {
        public List<EWalletDepositFullOutputDto> EWallets { set; get; }
        public List<CardFullOutputDto> Cards { set; get; }
        public List<VoucherFullOutputDto> Vouchers { set; get; }
        public List<CODFullOutputDto> CODs { set; get; }
        public List<TransferFullOutputDto> Transfers { set; get; }
    }
    public class PaymentRequestInsertDto
    {
        public string OrderCode { set; get; }
        public Guid? OrderReturnId { set; get; }
        public decimal TotalPayment { set; get; }
        public string CreatedBy { set; get; }
        private EmPaymentRequestType? TypePaymentValue;
        public EmPaymentRequestType? TypePayment
        {
            set
            {
                TypePaymentValue = value.Value;
            }
            get
            {
                return (TypePaymentValue == null ? EmPaymentRequestType.PaymentCoreRequest : TypePaymentValue.Value);
            }
        }
    }

    public class PaymentCancelInputDto
    {
        public string PaymentRequestCode { set; get; }

    }

    public class PaymentDepositInfoInputDto
    {
        public Guid? PaymentRequestId { set; get; }
    }
    public class PaymentDepositInfoOutputDto : PaymentRequestFullOutputDto
    {
        public decimal? RemainingAmount { set; get; }
        public PaymentRequestDetailDto Detail { set; get; } = new PaymentRequestDetailDto();
    }
    public class PaymentRequestDetailDto
    {
        public TransactionFullOutputDto PaymentTransaction { set; get; }
        public List<PaymentInfoDetailDto> DepositTransactions { set; get; }
        public List<QRHistoryDetailDto> QrHistory { set; get; }
    }

    public class GetPresignUploadOutputDto
    {
        public string KeyName { set; get; }
        public string PresignUrl { set; get; }
        public long ExpiresMinutes { set; get; }
        public string PrivateUrl { set; get; }
    }

    public class GetPresignUploadInputDto
    {
        public string KeyName { set; get; }
        public string ContentType { set; get; }

    }

    public class PaymentDepositRequestIdInfoOutputDto : PaymentRequestFullOutputDto
    {
        public decimal? RemainingAmount { set; get; }
        public PaymentRequestIdDetailDto Detail { set; get; } = new PaymentRequestIdDetailDto();
    }
    public class PaymentRequestIdDetailDto
    {
        public TransactionFullOutputDto PaymentTransaction { set; get; }
        public List<PaymentInfoDetailDto> DepositTransactions { set; get; }
        public List<QRHistoryDetailDto> QrHistory { set; get; }
        public List<PaymentInfoDetailDto> CashBacks { set; get; }
    }

    public class PaymentAccountingOutputDto : PaymentInfoOutputDto
    {
        public AccountingHistoryDetailOutputDto AccountingDetail{ set; get; }
    }
    public class AccountingHistoryDetailOutputDto
    {
        public Guid? AccountingId { get; set; }
        public string AccountingCode { get; set; }
        public string PaymentDebitRequestCode { get; set; }
        public string OrderCode { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalDebitAmount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { set; get; }
        public DateTime? AccountingDate { get; set; }
        public string CustomerName { get; set; } // Tên khách hàng
        public string PaymentMethod { get; set; }
        public string EmployeeId { set; get; }
        public string EmployeeName { set; get; }
        public DateTime? InComeDate { set; get; }
        public decimal? TotalInCome { set; get; }
        public byte? Status { set; get; }
        public string Description { set; get; }
        public string AccountingName { set; get; }
        public decimal? CashAmount { set; get; }
        public EnmCompanyType? CompanyID { get; set; }
        public string ShopCode { get; set; }
        public string CustomerId { get; set; }
        public Guid? PaymentRequestId { set; get; }
        public string PaymentRequestCode { set; get; }
        public string CompanyName { set; get; }
        public string TaxCode { set; get; }
        public string Phone { set; get; }
    }
}
