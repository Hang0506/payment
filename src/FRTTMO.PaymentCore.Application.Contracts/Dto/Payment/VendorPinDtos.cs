using System;

namespace FRTTMO.PaymentCore.Dto
{
    public class VendorPinDto
    {
        public int Id { get; set; }
        public int? VendorId { get; set; }
        public string ShopCode { get; set; }
        public string PinCode { get; set; }
    }

    public class VendorPinFullOutputDto : VendorPinDto
    {
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }

    public class InsertVenderInputDto
    {
        public int? VendorId { get; set; }
        public string ShopCode { get; set; }
        public string PinCode { get; set; }
    }
    public class UpdateVenderInputDto
    {
        public string PinCode { get; set; }
    }
}
