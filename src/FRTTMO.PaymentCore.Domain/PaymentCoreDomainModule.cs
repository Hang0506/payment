using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace FRTTMO.PaymentCore
{
    [DependsOn(
        typeof(AbpDddDomainModule),
        typeof(PaymentCoreDomainSharedModule)
    )]
    public class PaymentCoreDomainModule : AbpModule
    {

    }
}
