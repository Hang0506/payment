using Volo.Abp.Application;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace FRTTMO.PaymentCore
{
    [DependsOn(
        typeof(AbpHttpClientModule),
        typeof(AbpDddApplicationContractsModule),
        typeof(AbpDddApplicationModule))]
    public class PaymentCoreIntegrateModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<PaymentCoreIntegrateModule>();
            });

        }
    }
}
