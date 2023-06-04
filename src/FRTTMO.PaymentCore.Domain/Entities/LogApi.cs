using System;
using Volo.Abp.Domain.Entities;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Entities
{
    public class LogApi : Entity<int>
    {        
        public EnmPartnerId? PartnerId { set; get; }
        public EnmMethodType? MethodType { set; get; }
        public string MethodName { set; get; }
        public string Request { set; get; }
        public string Response { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
    }
    public class LogApiES
    {
        public EnmPartnerId? PartnerId { set; get; }
        public EnmMethodType? MethodType { set; get; }
        public string MethodName { set; get; }
        public string Request { set; get; }
        public string Response { set; get; }
    }
}
