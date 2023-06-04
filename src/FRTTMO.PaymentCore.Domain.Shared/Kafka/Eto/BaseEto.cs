using FRTTMO.PaymentCore.Kafka.Interface;
using System;

namespace FRTTMO.PaymentCore.Kafka.Eto
{
    [Serializable]
    public abstract class BaseETO : IEventName
    {
        public abstract string EventName { get; }
    }
}
