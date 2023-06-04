using System;
using System.Collections.Generic;
using System.Text;

namespace FRTTMO.PaymentCore.Dto.v2
{
    public class InputPaymentDtoV2
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class OutPutPaymentDtoV2
    {
        public List<string> PaymentCode { get; set; }
    }
}
