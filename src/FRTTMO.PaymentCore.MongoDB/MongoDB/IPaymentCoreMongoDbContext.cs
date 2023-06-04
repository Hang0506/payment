using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace FRTTMO.PaymentCore.MongoDB
{
    [ConnectionStringName(PaymentCoreDbProperties.ConnectionStringName)]
    public interface IPaymentCoreMongoDbContext : IAbpMongoDbContext
    {
        /* Define mongo collections here. Example:
         * IMongoCollection<Question> Questions { get; }
         */
    }
}
