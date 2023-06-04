using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace FRTTMO.PaymentCore.EntityFrameworkCore
{
    [DependsOn(
        typeof(PaymentCoreDomainModule),
        typeof(AbpEntityFrameworkCoreModule)
    )]
    public class PaymentCoreEntityFrameworkCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAbpDbContext<PaymentCoreDbContext>(options =>
            {
                /* Add custom repositories here. Example:
                 * options.AddRepository<Question, EfCoreQuestionRepository>();
                 */
                options.AddDefaultRepositories(includeAllEntities: true);
            });
        }
    }
}