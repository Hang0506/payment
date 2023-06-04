using System;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class LogApiDto
    {
        public int Id { set; get; }
        public EnmPartnerId? PartnerId { set; get; }
        public EnmMethodType? MethodType { set; get; }
        public string MethodName { set; get; }
        public string Request { set; get; }
        public string Response { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
    }
    
}
