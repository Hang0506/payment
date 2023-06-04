using DotNetCore.CAP;
using FRTTMO.PaymentCore.Kafka.Eto;
using FRTTMO.PaymentCore.Kafka.Interface;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace FRTTMO.PaymentCore.Kafka.Service
{
    public class PublishService<T> : ApplicationService, IPublishService<T> where T : BaseETO
    {
        private readonly ICapPublisher _capPublisher;

        public PublishService(ICapPublisher capPublisher)
        {
            _capPublisher = capPublisher;
        }
        public async Task ProduceAsync(T message)
        {
            await _capPublisher.PublishAsync<T>(message.EventName, message);
        }
    }
}
