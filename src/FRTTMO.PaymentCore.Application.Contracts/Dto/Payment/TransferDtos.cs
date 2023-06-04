using System;
using System.Collections.Generic;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class TransferDto
    {
        public Guid? TransactionId { set; get; }
        public string AccountNum { set; get; }
        public string AccountName { set; get; }
        public string BankName { set; get; }
        public DateTime? DateTranfer { set; get; }
        public string Image { set; get; }
        public string ImageOrigin { set; get; }
        public string TransferNum { set; get; }
        public string Content { set; get; }
        public decimal? Amount { set; get; }
        public EnmTransferIsConfirm? IsConfirm { set; get; }
        public string ReferenceBanking { set; get; }
        public EnmPartnerId? PartnerId { set; get; }
        public string UserConfirm { set; get; }
    }
    public class TransferInputDto : TransferDto
    {
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
    }
    public class TransferFullOutputDto : TransferDto
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
    public class SearchTransferCondi
    {
        public string PaymentRequestCode { get; set; }
    }
    public class PaymentTransferInputDto
    {
        public string AccountNum { set; get; }
        public string AccountName { set; get; }
        public string BankName { set; get; }
        public DateTime? DateTranfer { set; get; }
        public string Image { set; get; }
        public string TransferNum { set; get; }
        public string Content { set; get; }
        public decimal? Amount { set; get; }
        public byte? IsConfirm { set; get; }
        public string CreatedBy { set; get; }
        public string CreatedByName { set; get; }
        public string ReferenceBanking { set; get; }
        public EnmPartnerId? PartnerId { set; get; }
    }
    public class TransferFullInputDto : TransferDto
    {
        public Guid Id { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
    }
}
