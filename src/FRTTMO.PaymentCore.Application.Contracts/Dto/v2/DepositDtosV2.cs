using System;
using System.Collections.Generic;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto.v2
{
    //Base
    public class DepositCoresInputDtoV2
    {
        public string OrderCode { get; set; }
        public string Phone { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public InsertTransactionInputDtoV2 Transaction { get; set; }
        public EWalletDepositInputDto EWallet { get; set; }
        public List<CardInputDto> Cards { get; set; }
        public CODInputDto COD { get; set; }
        public List<TransferInputDto> Transfers { get; set; }
        public List<VoucherInputDto> Vouchers { get; set; }
        public DebitDto Debit { get; set; }
    }

    public class MaskDepositInputBaseDtoV2
    {
        public string OrderCode { get; set; }
        public string PaymentCode { get; set; }
        public Guid? PaymentRequestId { get; set; }
        public string Phone { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public decimal? TotalPayment { get; set; }
        public DepositTransactionInputDtoV2 Transaction { get; set; }
    }

    public class DepositTransactionInputDtoV2
    {
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
        public string CreatedBy { set; get; }
    }

    public class MaskDepositByCashInputDtoV2 : MaskDepositInputBaseDtoV2 { }

    public class MaskDepositByEWalletInputDtoV2 : MaskDepositInputBaseDtoV2
    {
        public PaymentEWalletDepositInputDtoV2 EWallet { get; set; }
    }

    public class MaskDepositByCardInputDtoV2 : MaskDepositInputBaseDtoV2
    {
        public List<PaymentCardInputDtoV2> Cards { get; set; }
    }

    public class MaskDepositByCODInputDtoV2 : MaskDepositInputBaseDtoV2
    {
        public PaymentCODInputDtoV2 COD { get; set; }
    }
    public class DebtSaleInputDto : MaskDepositInputBaseDtoV2
    {
        public DebitDto Debit { get; set; }
    }
    public class MaskDepositByTransferInputDtoV2 : MaskDepositInputBaseDtoV2
    {
        public List<PaymentTransferInputDtoV2> Transfers { get; set; }
    }

    public class MaskDepositByVoucherInputDtoV2 : MaskDepositInputBaseDtoV2
    {
        public List<PaymentVoucherInputDtoV2> Vouchers { get; set; }
        public string Note { get; set; }
        public string ShopCode { get; set; }
    }

    public class MaskDepositByEWalletOnlineInputDtoV2 : MaskDepositInputBaseDtoV2
    {
        public string TerminalCode { get; set; }
        public string IpAddress { get; set; }
        public string TransactionCode { get; set; }
        public PaymentEWalletDepositInputDtoV2 EWallet { get; set; }
    }


    //Output
    public class DepositOutputBaseDtoV2
    {
        public TransactionFullOutputDtoV2 Transaction { get; set; }
    }
    public class DepositByCashOutputDtoV2 : DepositOutputBaseDtoV2
    {
    }
    public class DepositByCardOutputDtoV2 : DepositOutputBaseDtoV2
    {
        public List<CardFullOutputDto> Cards { get; set; }
    }
    public class DepositByCODOutputDtoV2 : DepositOutputBaseDtoV2
    {
        public CODFullOutputDto COD { get; set; }
    }
    public class DepositByTransferOutputDtoV2 : DepositOutputBaseDtoV2
    {
        public List<TransferFullOutputDto> Transfers { get; set; }
    }
    public class DepositByEWalletOutputDtoV2 : DepositOutputBaseDtoV2
    {
        public EWalletDepositFullOutputDto EWallet { get; set; }
    }
    public class DepositByVoucherOutputDtoV2 : DepositOutputBaseDtoV2
    {
        public bool VoucherUsedSuccess { get; set; }
        public List<VoucherFullOutputDto> Vouchers { get; set; }
    }
    public class DepositByMultipleVoucherOutputDtoV2
    {
        public List<DepositByVoucherOutputDtoV2> Succeeded { get; set; }
        public List<VoucherFailOutputDto> Failed { get; set; }
        public string Message { get; set; }
    }
    public class DepositCoresOutputDtoV2
    {
        public Guid TransactionId { get; set; }
        public TransactionFullOutputDtoV2 Transaction { get; set; }
        public EWalletDepositFullOutputDto EWallet { get; set; }
        public List<CardFullOutputDto> Cards { get; set; }
        public CODFullOutputDto COD { get; set; }
        public List<TransferFullOutputDto> Transfers { get; set; }
        public List<VoucherFullOutputDto> Vouchers { get; set; }
        public DebitDto Debit { get; set; }
    }

}
