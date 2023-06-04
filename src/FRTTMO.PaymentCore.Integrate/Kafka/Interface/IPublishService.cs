using FRTTMO.PaymentCore.Kafka.Eto;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace FRTTMO.PaymentCore.Kafka.Interface
{
    public interface IPublishService<T> : IApplicationService where T : BaseETO
    {
        public Task ProduceAsync(T message);
    }
}
