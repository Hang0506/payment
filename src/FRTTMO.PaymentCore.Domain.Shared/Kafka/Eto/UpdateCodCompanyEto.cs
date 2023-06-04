using System;
using System.Collections.Generic;
using System.Text;

namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class UpdateCodCompanyEto : BaseETO
    {
        public override string EventName => KafkaTopics.PAYMENT_COMPANY_COD_UPDATED;
        public Guid? TransporterID { set; get; }
        public string TransporterName { set; get; }
        public int? TransporterCode { get; set; }
        public string OrderCode { get; set; }
        public Guid TransactionId { set; get; }
    }
}
