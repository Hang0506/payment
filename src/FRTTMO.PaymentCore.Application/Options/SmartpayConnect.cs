using System;
using System.Collections.Generic;
using System.Text;

namespace FRTTMO.PaymentGateway
{
    public class SmartpayConnect
    {
        public string url { get; set; }
        public string channel { get; set; }
        public string merchantId { get; set; }
        public string branchCode { get; set; }
        public string hashKey { get; set; }
        public string notifyUrl { get; set; }
    }
}
