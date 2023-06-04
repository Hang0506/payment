using System;

namespace FRTTMO.PaymentCore.Dto
{
    public class CardDto
    {
        public Guid? TransactionId { set; get; }
        public string CardNumber { set; get; }
        public byte? CardType { set; get; }
        public string BankName { set; get; }
        public decimal? Amount { set; get; }
        public string BankCode { set; get; }
        public byte? Paymethod { set; get; }
    }
    public class CardInputDto : CardDto
    {
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
    }
    public class CardFullOutputDto : CardDto
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }

    public class PaymentCardInputDto
    {
        public string CardNumber { set; get; }
        public byte? CardType { set; get; }
        public string BankName { set; get; }
        public decimal? Amount { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
    }
}
