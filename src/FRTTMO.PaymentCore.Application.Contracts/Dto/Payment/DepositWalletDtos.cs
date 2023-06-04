using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class DepositInputBaseDto
    {
        public string OrderCode { get; set; }
        public string Phone { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public InsertTransactionInputDto Transaction { get; set; }
    }
    public class DepositCoresInputDto
    {
        public string OrderCode { get; set; }
        public string Phone { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public InsertTransactionInputDto Transaction { get; set; }
        public EWalletDepositInputDto EWallet { get; set; }
        public List<CardInputDto> Cards { get; set; }
        public CODInputDto COD { get; set; }
        public List<TransferInputDto> Transfers { get; set; }
        public List<VoucherInputDto> Vouchers { get; set; }
        public DebitDto Debit { get; set; }
    }
    public class DepositByCashInputDto : DepositInputBaseDto
    {
    }
    public class DepositByEWalletInputDto : DepositInputBaseDto
    {
        public PaymentEWalletDepositInputDto EWallet { get; set; }
    }
    public class DepositByCardInputDto : DepositInputBaseDto
    {
        public List<PaymentCardInputDto> Cards { get; set; }
    }
    public class DepositByCODInputDto : DepositInputBaseDto
    {
        public PaymentCODInputDto COD { get; set; }
    }
    public class DepositByTransferInputDto : DepositInputBaseDto
    {
        public List<PaymentTransferInputDto> Transfers { get; set; }
    }
    public class DepositByVoucherInputDto : DepositInputBaseDto
    {
        public List<PaymentVoucherInputDto> Vouchers { get; set; }
    }
    //Output
    public class DepositOutputBaseDto
    {
        public TransactionFullOutputDto Transaction { get; set; }
    }
    public class DepositByCashOutputDto : DepositOutputBaseDto
    {
    }
    public class DepositByCardOutputDto : DepositOutputBaseDto
    {
        public List<CardFullOutputDto> Cards { get; set; }
    }
    public class DepositByCODOutputDto : DepositOutputBaseDto
    {
        public CODFullOutputDto COD { get; set; }
    }
    public class DepositByTransferOutputDto : DepositOutputBaseDto
    {
        public List<TransferFullOutputDto> Transfers { get; set; }
    }
    public class DepositByEWalletOutputDto : DepositOutputBaseDto
    {
        public EWalletDepositFullOutputDto EWallet { get; set; }
    }
    public class DepositByVoucherOutputDto : DepositOutputBaseDto
    {
        /// <summary>
        /// Call api Use Voucher thành công
        /// </summary>
        public bool VoucherUsedSuccess { get; set; }
        public List<VoucherFullOutputDto> Vouchers { get; set; }
    }
    public class DepositByMultipleVoucherOutputDto
    {
        ///// <summary>
        ///// Chưa được xử lý
        ///// </summary>
        //public List<PaymentVoucherInputDto> Untreated { get; set; }

        /// <summary>
        /// Deposit thành công
        /// </summary>
        public List<DepositByVoucherOutputDto> Succeeded { get; set; }

        /// <summary>
        /// Deposit lỗi
        /// </summary>
        public List<VoucherFailOutputDto> Failed { get; set; }
        public string Message { get; set; }
    }
    public class VoucherFailOutputDto
    {
        public string Code { set; get; }
        public EnmVoucherProvider? VoucherType { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class DepositWalletOutputDto
    {
        public Guid TransactionId { get; set; }
        public TransactionFullOutputDto Transaction { get; set; }
        public EWalletDepositFullOutputDto EWallet { get; set; }
        public List<CardFullOutputDto> Cards { get; set; }
        public CODFullOutputDto COD { get; set; }
        public List<TransferFullOutputDto> Transfers { get; set; }
        public List<VoucherFullOutputDto> Vouchers { get; set; }
        public DebitDto Debit { get; set; }
    }
    public class DepositByEWalletOnlineInputDto : DepositInputBaseDto
    {
        public string TerminalCode { get; set; }
        public string IpAddress { get; set; }
        public string TransactionCode { get; set; }
        public PaymentEWalletDepositInputDto EWallet { get; set; }
    }
    public class DepositDebtSaleInputDto : DepositInputBaseDto
    {
        public DebitDto Debit { get; set; }
    }
    public class DepositDebtSaleOutputDto : DepositOutputBaseDto
    {
        public DebitDto Debit { get; set; }
    }

    //input create Debit 
    public class DebitCreateInputDto
    {
        public string OrderCode { get; set; }
        public string CustomerID { get; set; } // Mã khách hàng
        public string CustomerName { get; set; } // Tên khách hàng
        public decimal? TotalAmount { get; set; } //tổng tiền đơn hàng
        public decimal? TotalDebitAmount { get; set; } //tiền COD cần thu nợ
        public decimal? TotalPayment { set; get; }
        public string CreatedBy { get; set; }// Người hoàn tất đơn hàng
        public string ShopCode { get; set; }
        public EnmCompanyType? CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string Phone { get; set; }
        public string TaxCode { set; get; }
    }
    public class AccountingDetailOutputDto
    {
        public string AccountingCode { set; get; }
        public string PaymentDebitRequestCode { set; get; }
        public string OrderCode { set; get; }
        public decimal? TotalAmount { set; get; }
        public decimal? TotalDebit { set; get; }
        public decimal? TotalPayment { set; get; }
        public Guid? PaymentRequestId { set; get; }
        public string PaymentRequestCode { set; get; }
        public string ShopCode { set; get; }
        public string CustomerId { get; set; }
        public Guid? AccountingId { set; get; }
        public Guid? DebitId { set; get; }
        public EnmAccountingStatus? Status { set; get; }
    }
    public enum EnmAccountingStatus : byte
    {
        /// <summary>
        /// Chờ xử lý
        /// </summary>
        Pending = 1,
        /// <summary>
        /// Đã thu tiền
        /// </summary>
        HasPaid = 3,
        /// <summary>
        /// Đã gạch nợ
        /// </summary>
        HasBeenDeducted = 4,
        /// <summary>
        /// Đã hủy
        /// </summary>
        Canceled = 13
    }
    public class AccountingByCashInputDto
    {
        public string OrderCode { get; set; }
        public string AccountingCode { get; set; }
        public string AccountingBy { set; get; }
        public string PaymentMethod { get; set; }
        public string Description { get; set; }
        public string EmployeeId { set; get; }
        public string EmployeeName { set; get; }
        public decimal? TotalInCome { set; get; }
        public byte? Type { set; get; }
        public string AccountingName { set; get; }
        public decimal? CashAmount { set; get; }
    }
    public class SearchESOrder_OutPutDto
    {
        public Guid orderID { get; set; }
      
        public List<ShipmentPlanningOutputDto> ShipmentPlannings { get; set; }
    }
    public class ShipmentPlanningOutputDto
    {
        public string CarrierName { get; set; }
    }
    public class SearchESByOrderCodeRequestDto
    {
        [Required]
        public List<string> OrderCode { get; set; }
    }
}
