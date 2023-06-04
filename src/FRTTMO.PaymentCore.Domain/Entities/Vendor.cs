using System;
using Volo.Abp.Domain.Entities;

namespace FRTTMO.PaymentCore.Entities
{
    public class Vendor : Entity<int>
    {
        public string VendorName { set; get; }
        public string VendorCode { set; get; }
        public string ImageUrl { set; get; }
        public string ApiUrl { set; get; }
        public int? PaymentMethodId { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
        public bool? Status { get; set; }
        public string Domestic { set; get; }
    }
}
