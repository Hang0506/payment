using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FRTTMO.PaymentCore.Dto
{
    public class BankingOnlineOutPutDto
    {
        public int Id { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string ImgLink { get; set; }
        public bool Status { get; set; }
        public DateTime? CreatedDate { set; get; }
        public string CreatedBy { set; get; }
        public DateTime? ModifiedDate { set; get; }
        public string ModifiedBy { set; get; }
    }
}
