using System;
using System.ComponentModel.DataAnnotations;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class CustomerDto
    {
        [MaxLength(10)]
        public string ShopCode { set; get; }
        public CustomerType? CustomerType { set; get; }
        [MaxLength(50)]
        public string FullName { set; get; }
        public string Mobile { set; get; }
        public string TaxNumber { set; get; }
        public string Address { set; get; }
        public string Email { set; get; }
        public string Gender { set; get; }
        public string IdNumber { set; get; }
        public DateTime? Dob { set; get; }
        
        public CustomerStatus Status { set; get; }
    }

    public class CustomerInsertInputDto : CustomerDto
    {
        public string CreatedBy { set; get; }
        /*public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (!IsValidEmail(Email))
            {
                yield return new ValidationResult("Email không đúng định dạng!");
            }
        }*/
    }

    public class CustomerUpdateInputDto : CustomerDto
    {
        public string ModifiedBy { set; get; }
    }

    public class CustomerOutputDto
    {
        public Guid Id { get; set; }
        public int? ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class CustomerFullOutputDto : CustomerDto
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { set; get; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
    public class VerifyCustomerRequestDto
    {
        public Guid? Id { get; set; }
        public string Mobile { get; set; }
        public string TaxNumber { get; set; }
    }

}
