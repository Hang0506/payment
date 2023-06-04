using System;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Entities
{
    public class Customer: BaseEntity<Guid>
    {
        public string ShopCode { set; get; }
        public CustomerType? CustomerType { set; get; }
        public string FullName { set; get; }
        public string Mobile { set; get; }
        public string TaxNumber { set; get; }
        public string Address { set; get; }
        public string Email { set; get; }
        public string Gender { set; get; }
        public string IdNumber { set; get; }
        public DateTime? Dob { set; get; }
        public CustomerStatus Status { set; get; }
    }
}
