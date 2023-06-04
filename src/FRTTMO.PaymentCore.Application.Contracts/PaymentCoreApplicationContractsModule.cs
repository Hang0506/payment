using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;

namespace FRTTMO.PaymentCore
{
    [DependsOn(
        typeof(PaymentCoreDomainSharedModule),
        typeof(AbpDddApplicationContractsModule),
        typeof(AbpAuthorizationModule)
        )]
    public class PaymentCoreApplicationContractsModule : AbpModule
    {

    }
}
