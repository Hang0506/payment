namespace FRTTMO.PaymentCore.Entities
{
    public class CardType : BaseEntity<int>
    {
        public string Name { set; get; }
        public byte? Status { set; get; }
    }
}
