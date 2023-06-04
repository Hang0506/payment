using System;
using Volo.Abp.Domain.Entities;

namespace FRTTMO.PaymentCore.Entities
{
    public class PaymentMethodDetail : Entity<int>
    {
        public int PaymentMethodId { set; get; }
        public string Name { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
    }
}
