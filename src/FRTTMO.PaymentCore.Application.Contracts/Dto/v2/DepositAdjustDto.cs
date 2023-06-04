using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto.v2
{
    //Base
    //hằng make

    #region Input

    public class PaymentRequestInputDto : PaymentRequestDto
    {
        public decimal TotalPayment { set; get; }
        [Required]
        public string PaymentCode { set; get; }
        public string CreatedBy { set; get; }
        public EmPaymentRequestType? TypePaymentValue { set; get; }
        public string SourceCode { set; get; }
        public EnmPaymentSourceCode? PaymentSourceType { set; get; }
    }

    #endregion
    #region OutPut

    public class PaymentRequestDto
    {
        public Guid? OrderReturnId { set; get; }
    }
    public class PaymentRequestOutputDto : PaymentRequestDto
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
    }
    #endregion


}
