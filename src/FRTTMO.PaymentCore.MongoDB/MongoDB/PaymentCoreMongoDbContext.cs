using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace FRTTMO.PaymentCore.MongoDB
{
    [ConnectionStringName(PaymentCoreDbProperties.ConnectionStringName)]
    public class PaymentCoreMongoDbContext : AbpMongoDbContext, IPaymentCoreMongoDbContext
    {
        /* Add mongo collections here. Example:
         * public IMongoCollection<Question> Questions => Collection<Question>();
         */

        protected override void CreateModel(IMongoModelBuilder modelBuilder)
        {
            base.CreateModel(modelBuilder);

            modelBuilder.ConfigurePaymentCore();
        }
    }
}