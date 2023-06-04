using System;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class VoucherDto
    {
        public string Code { set; get; }
        public Guid? TransactionId { set; get; }
        public string Name { set; get; }
        public decimal? Amount { set; get; }
    }
    public class VoucherFullOutputDto : VoucherDto
    {
        public Guid Id { get; set; }
        public EnmVoucherProvider? VoucherType { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
    public class VoucherInputDto : VoucherDto
    {
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public EnmVoucherProvider? VoucherType { get; set; }
    }

    public class PaymentVoucherInputDto
    {
        public string Code { set; get; }
        public string Name { set; get; }
        public decimal? Amount { set; get; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public EnmVoucherProvider? VoucherType { get; set; }
    }

    //public class VoucherInputDto
    //{
    //    public string Code { get; set; }
    //    public string Name { get; set; }
    //    public decimal? Amount { set; get; }
    //    //public int Type
    //    //+type( 1 LC , 2 got IT, 3 taptap)
    //    //+amount
    //    //+createdDate
    //    //+createdBy
    //    //+modifiedDate
    //    //+modifiedBy
    //}
}
