using System;
using Volo.Abp.Domain.Entities;

namespace FRTTMO.PaymentCore.Entities
{
    public class VendorPin : Entity<int>
    {
        public int? VendorId { get; set; }
        public string ShopCode { get; set; }
        public string PinCode { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
}
