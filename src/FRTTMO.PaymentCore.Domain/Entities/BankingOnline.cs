using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Entities
{
    public class BankingOnline : BaseEntity<int>
    {
        [Column(TypeName = "nvarchar(200)")]
        public string BankName { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string BankCode { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string ImgLink { get; set; }
        public bool Status { get; set; }
    }
}
