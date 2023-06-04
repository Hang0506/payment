using System;

namespace FRTTMO.PaymentCore.Dto
{
    public class CODDto
    {
        public Guid? TransactionId { set; get; }
        public Guid? TransporterID { set; get; }
        public string TransporterName { set; get; }
        public string waybillnumber { set; get; }
        public decimal? Amount { set; get; }
        public string Description { set; get; }
        public int? TransporterCode { get; set; }
    }
    public class CODInputDto : CODDto
    {
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
    }
    public class CODFullOutputDto : CODDto
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }

    public class PaymentCODInputDto
    {
        public Guid? TransporterID { set; get; }
        public string TransporterName { set; get; }
        public string waybillnumber { set; get; }
        public decimal? Amount { set; get; }
        public int? TransporterCode { get; set; }
        public string CreatedBy { set; get; }
    }
}
