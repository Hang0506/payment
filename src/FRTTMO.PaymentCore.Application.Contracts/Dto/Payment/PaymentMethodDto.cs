using System;
using System.Collections.Generic;

namespace FRTTMO.PaymentCore.Dto
{
    public class PaymentMethodDto
    {
        public int Id { get; set; }
        public string Name { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public List<VendorDto> Vendors { set; get; }
        public bool? Status { get; set; }
    }
    public class PaymentMethodOnlineDto
    {
        public int Id { get; set; }
        public string Name { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public List<VendorDto> Vendors { set; get; }
        public bool? Status { get; set; }
    }
}
