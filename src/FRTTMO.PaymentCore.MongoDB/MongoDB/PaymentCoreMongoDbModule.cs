using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace FRTTMO.PaymentCore.MongoDB
{
    [DependsOn(
        typeof(PaymentCoreDomainModule),
        typeof(AbpMongoDbModule)
        )]
    public class PaymentCoreMongoDbModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddMongoDbContext<PaymentCoreMongoDbContext>(options =>
            {
                /* Add custom repositories here. Example:
                 * options.AddRepository<Question, MongoQuestionRepository>();
                 */
            });
        }
    }
}
