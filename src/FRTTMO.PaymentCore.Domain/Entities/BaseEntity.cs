using System;
using Volo.Abp.Domain.Entities;

namespace FRTTMO.PaymentCore.Entities
{
    public class BaseEntity<T> : Entity<T>
    {
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
}
