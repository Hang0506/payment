using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Entities
{
    public class PaymentMethod : Entity<int>
    {
        public string Name { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("PaymentMethodId")]
        public List<Vendor> Vendors { set; get; }
        public bool? Status { get; set; }
        public EnumPaymentMethodType? Type { get; set; }
        public List<PaymentMethodDetail> Detail { get; set; }
    }
}
