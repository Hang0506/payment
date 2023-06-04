namespace FRTTMO.PaymentCore.Dto
{
    public class VendorDto
    {
        public int Id { get; set; }
        public string VendorName { set; get; }
        public string VendorCode { set; get; }
        public string ImageUrl { set; get; }
        public string ApiUrl { set; get; }
        public int? PaymentMethodId { set; get; }
        public bool? Status { get; set; }
        public string Domestic { set; get; }
        public DomesticOption DomesticOption { get; set; }
    }
    public class DomesticOption
    {
        public bool AllowDomestic { set; get; }
        public int CheckoutType { get; set; }

    }

}
