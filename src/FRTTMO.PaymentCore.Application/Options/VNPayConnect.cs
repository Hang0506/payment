using System;
using System.Collections.Generic;
using System.Text;

namespace FRTTMO.PaymentGateway
{
    public class VNPayConnect
    {
        public string url { get; set; }
        public string merchantCode { get; set; }
        public string merchantName { get; set; }
        public string merchantType { get; set; }
        public string terminalId { get; set; }
        public string terminalName { get; set; }
        public string secretKey_createQR { get; set; }
        public string secretKey_checktrans { get; set; }
        public string appId { get; set; }
    }
}
