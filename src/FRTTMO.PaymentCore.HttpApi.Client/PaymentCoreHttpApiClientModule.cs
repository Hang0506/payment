using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace FRTTMO.PaymentCore
{
    [DependsOn(
        typeof(PaymentCoreApplicationContractsModule),
        typeof(AbpHttpClientModule))]
    public class PaymentCoreHttpApiClientModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddHttpClientProxies(
                typeof(PaymentCoreApplicationContractsModule).Assembly,
                PaymentCoreRemoteServiceConsts.RemoteServiceName
            );

            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<PaymentCoreHttpApiClientModule>();
            });

        }
    }
}
