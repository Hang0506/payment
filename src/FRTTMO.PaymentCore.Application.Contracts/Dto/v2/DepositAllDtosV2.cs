using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto.v2
{
    //Base

    #region Input

    public class DepositAllInputDto
    {
        public string PaymentCode { get; set; }
        public decimal? TotalPayment { get; set; }
        public string PaymentRequestCode { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public Guid? AccountId { set; get; }
        public string ShopCode { set; get; }
        public string Phone { get; set; }
        public string TerminalCode { get; set; }
        public string IpAddress { get; set; }
        public EmPaymentRequestType? PaymentRequestType { get; set; }
        public PaymentTransactionDto PaymentSource { get; set; }
        public List<DepositCashInputDto> Cash { get; set; }
        public List<EWalletDepositAllInputDto> EWalletAll { get; set; }
        public List<EWalletDepositAllInputDto> EWalletOnlineAll { get; set; }
        public List<CardDepositAllInputDto> CardsAll { get; set; }
        public List<CODDepositAllInputDto> CODAll { get; set; }
        public List<TransferInputDepositAllInputDto> TransfersAll { get; set; }
        public List<VoucherDepositAllInputDto> VouchersAll { get; set; }
        public List<DebtSaleInputV2Dto> DebtSaleAll { get; set; }
    }

    public class MapESDepositDto : DepositAllOutDto
    {
        public bool IsCreate { get; set; }
        public string CustCode { get; set; }
        public decimal? TotalFinal { set; get; }
        public EnmPaymentType? Type { get; set; }
    }
    public class PaymentTransactionDto
    {
        public List<PaymentTransactionBaseDto> Detail { get; set; }
        public string CreatedBy { set; get; }
        public EnmPaymentType? TypePayment { get; set; }
    }
    public class DepositCashInputDto : DepositTransactionDto
    {
        public Guid? TransactionId { set; get; }
    }
    public class DepositTransactionDto
    {
        public EnmTransactionType? TransactionTypeId { set; get; }
        public EnmPaymentMethod? PaymentMethodId { set; get; }
        public decimal? Amount { set; get; }
        public decimal? TransactionFee { set; get; }
        public DateTime? TransactionTime { set; get; }
        public string Note { set; get; }
        public string AdditionAttributes { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? CreatedDate { get; set; }
    }

    public class DebtSaleInputV2Dto : DepositTransactionDtoV2
    {
        public DebitV2Dto Debit { get; set; }
    }
    public class DebtSaleOutputDto : DepositTransactionDtoV2
    {
        public DebitV2Dto Debit { get; set; }
    }
    public class DepositTransactionDtoV2
    {
        public DepositTransactionDto Transaction { set; get; }
    }
    public class EWalletDepositAllDto : DepositTransactionDtoV2
    {
        public EWalletDepositInputDto EWalletDetail { get; set; }
    }
    public class EWalletDepositAllInputDto : EWalletDepositAllDto { }
    public class CardDepositAllDto : DepositTransactionDtoV2
    {
        public PaymentCardAllAInputDtoV2 CardsDetail { get; set; }
    }
    public class CardDepositAllInputDto : CardDepositAllDto
    {
        //public MaskDepositByCardInputDtoV2 inItem { get; set; }
    }
    public class PaymentCardAllAInputDtoV2
    {
        public Guid? TransactionId { set; get; }
        public string CardNumber { set; get; }
        public byte? CardType { set; get; }
        public string BankName { set; get; }
        public string BankCode { set; get; }
        public byte? Paymethod { set; get; }
        public decimal? Amount { set; get; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
    public class CODDepositAllDto : DepositTransactionDtoV2
    {
        public CODInputDto CODetail { get; set; }
    }
    public class CODDepositAllInputDto : CODDepositAllDto { }
    public class TransferInputDepositAllDto : DepositTransactionDtoV2
    {
        public TransferInputDto TransferDetail { get; set; }
    }
    public class TransferInputDepositAllInputDto : TransferInputDepositAllDto { }

    public class VoucherDepositAllInputDto : VoucherDepositAllDto { }
    public class VoucherDepositAllDto : DepositTransactionDtoV2
    {
        public PaymentVoucherAllDtoV2 VoucherDetail { get; set; }
    }
    public class PaymentVoucherAllDtoV2
    {
        public Guid? TransactionId { set; get; }
        public string Code { set; get; }
        public string Name { set; get; }
        public decimal? Amount { set; get; }
        public EnmVoucherProvider? VoucherType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class VerifyTDTSInputDto
    {
        public string PaymentRequestCode { get; set; }
        public string PaymentRequestCodeV2 { get; set; }
        public decimal? Total { set; get; }
        public decimal? TotalV2 { set; get; }
        public string PaymentCode { get; set; }
        public EmPaymentRequestType? TypePaymentRequest { get; set; }
        public CreatePaymentTransactionInputDtoV2 Transaction { set; get; }
    }
    public class CodRequestDto : ModelBaseMapDeposit
    {
        public List<CODDepositAllInputDto> cods { get; set; }
    }
    public class CashRequestDto : ModelBaseMapDeposit
    {
        public List<CODDepositAllInputDto> cods { get; set; }
    }
    public class CardRequestDto : ModelBaseMapDeposit
    {
        public List<CardDepositAllInputDto> cards { get; set; }
    }
    public class VoucherRequestDto : ModelBaseMapDeposit
    {
        public List<VoucherDepositAllInputDto> vouchers { get; set; }
        public string OrderCode { get; set; }
    }
    public class eWalletRequestDto : ModelBaseMapDeposit
    {
        public List<EWalletDepositAllInputDto> eWallet { get; set; }
    }
    public class TransferRequestDto : ModelBaseMapDeposit
    {
        public List<TransferInputDepositAllInputDto> tranfer { get; set; }
        public EnmPaymentType? TypePayment { get; set; }
    }
    public class ModelBaseMapDeposit
    {
        public string PaymentRequestCode { get; set; }
        public string PaymentRequestId { get; set; }
        public string PaymentCode { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public Guid? AccountId { set; get; }
        public string ShopCode { set; get; }
        public string Phone { get; set; }
        public string TerminalCode { get; set; }
        public string IpAddress { get; set; }
    }

    public class CreateRequestDepositAllInputDto
    {
        public string PaymentRequestId { get; set; }
        public string PaymentRequestCode { get; set; }
        public string PaymentCode { get; set; }
        public PaymentTransactionDto PaymentSource { get; set; }
        public decimal TotalPaymentRequest { get; set; }
        public bool isCreate { get; set; }
        public bool isTransfer { get; set; }
        public bool isVoucher { get; set; }
        public EmPaymentRequestType? PaymentRequestType { get; set; }
    }

    #endregion
    #region OutPut
    public class CreateRequestDepositAllOutputDto
    {
        public DateTime? PaymentDate { get; set; }
        public EnmPaymentType? TypePayment { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public List<PaymentSourceOutPutDto> PaymentSourceId { get; set; }
        public string PaymentRequestCode { get; set; }
        public string PaymentRequestId { get; set; }
        public string PaymentRequestCodeV2 { get; set; }
        public string PaymentRequestIdV2 { get; set; }
    }
    public class VerifyTDTOutputDto
    {
        public bool IsPayment { set; get; } = false;
        public bool IsPaymentRequest { set; get; } = false;
        public decimal? RemainingAmount { set; get; }
        public decimal? TotalFinal { set; get; }
    }
    public class DepositAllOutDto : CreateRequestDepositAllOutputDto
    {
        public decimal? RemainingAmount { set; get; }
        public string ShopCode { get; set; }
        public string PaymentCode { get; set; }
        public bool Status { get; set; } = true;
        public bool IsPayment { set; get; } = false;
        public bool IsPaymentRequest { set; get; } = false;
        public CashFullOutputV2Dto Cash { set; get; }
        public CodFullOutputDtoV2Dto CODs { set; get; }
        public EWalletDepositFullOutputV2Dto EWallet { set; get; }
        public EWalletDepositFullOutputV2Dto EWalletOnline { set; get; }
        public TransferFullOutputV2Dto Transfer { set; get; }
        public VoucherFullOutputV2Dto Voucher { set; get; }
        public CardFullOutputV2Dto Cards { set; get; }
        public DebtSaleFullOutputV2Dto DebtSale { set; get; }
    }

    public class PaymentSourceOutPutDto
    {
        public Guid? PaymentSourceId { get; set; }
        public string SourceCode { get; set; }
        public EnmPaymentSourceCode? Type { get; set; }
    }
    public class DepositCashOutPutDto : DepositCashInputDto { }

    public class DepositDebtSaleOutPutDto : DebtSaleInputV2Dto { }
    public class DebtSaleFullOutputV2Dto
    {
        public List<SucceededDebtSale> DataSucceeded { get; set; }
        public List<FailedDebtSale> DataFailed { get; set; }
    }
    public class SucceededDebtSale
    {
        public DepositDebtSaleOutPutDto Succeeded { get; set; }
    }
    public class FailedDebtSale
    {
        public DepositDebtSaleOutPutDto Failed { get; set; }
        public string Message { get; set; }
    }
    public class CashFullOutputV2Dto
    {
        public List<SucceededCash> DataSucceeded { get; set; }
        public List<FailedCash> DataFailed { get; set; }
    }
    public class SucceededCash
    {
        public DepositCashOutPutDto Succeeded { get; set; }
    }
    public class FailedCash
    {
        public DepositCashOutPutDto Failed { get; set; }
        public string Message { get; set; }

    }
    public class TransferFullOutputV2Dto
    {
        public List<SucceededTransfer> DataSucceeded { get; set; }
        public List<FailedTransfer> DataFailed { get; set; }
    }

    public class VoucherFullOutputV2Dto
    {
        public List<SucceededVoucher> DataSucceeded { get; set; }
        public List<FailedVoucher> DataFailed { get; set; }
    }

    public class FailedTransfer
    {
        public TransferInputDepositAllDto Failed { get; set; }
        public string Message { get; set; }
    }
    public class SucceededTransfer
    {
        public TransferInputDepositAllDto Succeeded { get; set; }
    }
    public class SucceededVoucher
    {
        public VoucherDepositAllDto Succeeded { get; set; }
    }
    public class CodFullOutputDtoV2Dto
    {
        public List<SucceededCod> DataSucceeded { get; set; }
        public List<FailedCOD> DataFailed { get; set; }
    }

    public class FailedCOD
    {
        public CODDepositAllDto Failed { get; set; }
        public string Message { get; set; }
    }

    public class SucceededCod
    {
        public CODDepositAllDto Succeeded { get; set; }
    }

    public class CardFullOutputV2Dto
    {
        public List<SucceededCard> DataSucceeded { get; set; }
        public List<FailedCard> DataFailed { get; set; }
    }

    public class FailedCard
    {
        public CardDepositAllDto Failed { get; set; }
        public string Message { get; set; }
    }

    public class SucceededCard
    {
        public CardDepositAllDto Succeeded { get; set; }
    }
    public class EWalletDepositFullOutputV2Dto
    {
        public List<SucceededEWallet> DataSucceeded { get; set; }
        public List<FailedEWallet> DataFailed { get; set; }
    }
    public class SucceededEWallet
    {
        public EWalletDepositAllDto Succeeded { get; set; }
    }
    public class FailedEWallet
    {
        public EWalletDepositAllDto Failed { get; set; }
        public string Message { get; set; }
    }

    public class FailedVoucher
    {
        public VoucherDepositAllDto Failed { get; set; }
        public string Message { get; set; }
    }
    #endregion
    public class PaymentSourcDto
    {
        public string PaymentCode { set; get; }
        public string SourceCode { set; get; }
        public EnmPaymentSourceCode Type { set; get; }
        public byte PaymentVersion { set; get; }
        public decimal Amount { set; get; }
        public Guid PaymentId { get; set; }
        public EnmPaymentTransactionStatus Status { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
    public class MessesError
    {
        public string Message { set; get; }
    }
}
