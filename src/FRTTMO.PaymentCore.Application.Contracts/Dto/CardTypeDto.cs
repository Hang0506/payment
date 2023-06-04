
using System;

namespace FRTTMO.PaymentCore.Dto
{
    public class CardTypeDto
    {
        public string Name { set; get; }
        public byte? Status { set; get; }
    }

    public class CardTypeFullOutputDto : CardTypeDto
    {
        public int Id { set; get; } 
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
}
