using System;
using System.Collections.Generic;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto.v2
{
    public class PaymentRequestDtoV2
    {
        public string OrderCode { set; get; }
        public Guid? OrderReturnId { set; get; }
    }
    public class PaymentRequestFullOutputDtoV2 : PaymentRequestDtoV2
    {
        public Guid Id { get; set; }
        public string PaymentRequestCode { set; get; }
        public decimal TotalPayment { set; get; }
        public EmPaymentRequestType? TypePayment { set; get; }
        public EnmPaymentRequestStatus? Status { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
        public DateTime? PaymentRequestDate { set; get; }
        public string PaymentCode { set; get; }
    }

    public class PaymentInfoInputDtoV2
    {
        public EnmTransactionType TransactionTypeId { set; get; }
        public string PaymentRequestCode { set; get; }

    }
    public class PaymentInfoOutputDtoV2
    {
        public Guid? AccountId { set; get; }
        public EnmTransactionType? TransactionTypeId { set; get; }
        public Guid? PaymentRequestId { set; get; }
        public string PaymentRequestCode { set; get; }
        public string ShopCode { set; get; }
        public decimal? Amount { set; get; }
        public EnmPaymentRequestStatus? PaymentRequestStatus { set; get; }
        public List<PaymentInfoDetailDtoV2> Detail { set; get; }
    }

    public class PaymentInfoDetailDtoV2 : TransactionFullOutputDtoV2
    {
        public TransactionFullOutputDetailDto Detail { set; get; }
    }
    public class TransactionFullOutputDetailDtoV2
    {
        public List<EWalletDepositFullOutputDto> EWallets { set; get; }
        public List<CardFullOutputDto> Cards { set; get; }
        public List<VoucherFullOutputDto> Vouchers { set; get; }
        public List<CODFullOutputDto> CODs { set; get; }
        public List<TransferFullOutputDto> Transfers { set; get; }
    }
    public class PaymentRequestInsertDtoV2
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

    public class PaymentCancelInputDtoV2
    {
        public string PaymentRequestCode { set; get; }

    }

    public class PaymentDepositInfoInputDtoV2
    {
        public string PaymentRequestCode { set; get; }
    }
    public class PaymentDepositInfoOutputDtoV2 : PaymentRequestFullOutputDtoV2
    {
        public decimal? RemainingAmount { set; get; }
        public PaymentRequestDetailDtoV2 Detail { set; get; } = new PaymentRequestDetailDtoV2();
    }
    public class PaymentRequestDetailDtoV2
    {
        public TransactionFullOutputDtoV2 PaymentTransaction { set; get; }
        public List<PaymentInfoDetailDtoV2> DepositTransactions { set; get; }
        public List<QRHistoryDetailDto> QrHistory { set; get; }
    }
    public class Success
    {
        public string TransferIdSuccessful { set; get; }
        public bool Successful { set; get; }
    }
    public class TransferUpdateOutDto
    {
        public List<Success> Success { set; get; }
        public List<Failed> Failed { set; get; }
    }
    public class Failed
    {
        public bool StatusFail { set; get; }
        public string MessageFail { set; get; }
    }
    public class TransferUpdateInputDto
    {
        public List<InPutUpdateTransfer> InPutUpdateTransfer { set; get; }
        public string UserConfirm { set; get; }
    }
    public class InPutUpdateTransfer
    {
        public Guid TransferId { set; get; }
        public string PaymentCode { set; get; }
    }
}
