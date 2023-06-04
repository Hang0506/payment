using System;
using System.ComponentModel.DataAnnotations;

namespace FRTTMO.PaymentCore.Dto
{
    public class DebitDto
    {
        public string TaxCode { set; get; }
        public Guid? TransactionId { set; get; }
        public Guid? CustCode { set; get; }
        public string CustName { set; get; }
        public string Phone { set; get; }
        public decimal? Amount { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? CreatedDate { set; get; }
    }

    public class DebitFullOutputDto : DebitDto
    {
        public Guid Id { get; set; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }

    public class UpdateCompanyInfoInputDto
    {
        [Required]
        public string OrderCode { get; set; }
        public Guid? TransporterID { set; get; }
        public string TransporterName { set; get; }
        [Required]
        public int? TransporterCode { get; set; }
    }

    public class UpdateCompanyDebitInputDto
    {
        [Required]
        public string OrderCode { get; set; }
        [Required]
        public int CompanyID { get; set; } // ID hãng
    }
    public class DebitV2Dto
    {
        public string TaxCode { set; get; }
        public Guid? TransactionId { set; get; }
        public decimal? Amount { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? CreatedDate { set; get; }
    }
}