using System;

namespace FRTTMO.PaymentCore.Dto
{
    public class CashbackInputBaseDto
    {
        public string OrderCode { get; set; }
        public Guid OrderReturnId { get; set; }
        public decimal Totalpayment { get; set; }
        public InsertTransactionInputDto Transaction { get; set; }
    }

}
