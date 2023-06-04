using FRTTMO.PaymentCore.Dto;
using System;
using System.Collections.Generic;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto.v2
{
    public class DepositCoresAllOutputDtoV2 : MaskDepositInputBaseDtoV2
    {
        public string PaymentRequestCode { get; set; }
        public Guid? AccountId { set; get; }
        public int? TransactionTypeId { set; get; }
        public Guid? PaymentRequestId { set; get; }
        public string ShopCode { set; get; }
        //public decimal? Amount { set; get; }
        public EnmPaymentRequestStatus? PaymentRequestStatus { set; get; }
        //public PaymentInfoDepositDetailDtoV2 Detail { set; get; }
        public DepositAllOutDto Detail { set; get; }
        //public TransactionFullInputDepositDetailDto Detail { set; get; }

    }
    public class PaymentInfoDepositDetailDtoV2 : TransactionFullOutputDtoV2
    {
        public TransactionFullInputDepositDetailDto Detail { set; get; }
    }
    public class TransactionFullInputDepositDetailDto
    {
        public List<CashFullOutputV2Dto> Cash { set; get; }
        public List<CodFullOutputDtoV2Dto> CODs { set; get; }
        public List<EWalletDepositFullOutputV2Dto> EWallets { set; get; }
        public List<EWalletDepositFullOutputV2Dto> EWalletOnline { set; get; }
        public List<TransferFullOutputV2Dto> Transfers { set; get; }
        public List<VoucherFullOutputV2Dto> Vouchers { set; get; }
        public List<CardFullOutputV2Dto> Cards { set; get; }
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
        public Guid? PaymentRequestId { set; get; }
        public string ShopCode { set; get; }
        public EnmPaymentRequestStatus? PaymentRequestStatus { set; get; }
        public PaymentInfoDepositDetailDtoV2 Detail { set; get; }
    }
}
