using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using FRTTMO.PaymentCore.Application.Redis.Redis;
using FRTTMO.PaymentCore.Application.Redis.Payment;
using FRTTMO.PaymentCore.Dto;

namespace FRTTMO.PaymentCore.Application.Redis
{
    [DependsOn(
        typeof(PaymentCoreApplicationContractsModule),
        typeof(AbpAutoMapperModule),
        typeof(AbpAutofacModule)
        )]
    public class PaymentCoreAPIApplicationRedisModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            //var configuration = context.Services.GetSingletonInstance<IHostingEnvironment>().BuildConfiguration(options);

            //Configure<AbpAutoMapperOptions>(options =>
            //{
            //options.AddProfile<InventoryAPIRedisAutoMapperProfile>(validate: true);
            //});

            var configuration = context.Services.GetConfiguration();
            Configure<FdxRedisOptions>(options =>
            {
                options.RedisConnectionString = configuration["Redis:Configuration"];
            });
            context.Services.AddSingleton<IPaymentRedisRepositotyService<PaymentRedisDetailDto>, PaymentAppRedisService>();
        }

        public override void OnApplicationShutdown(ApplicationShutdownContext context)
        {
            //COMMENT When close redis, does it release all the cache data or it still persited in redis server?
            context.ServiceProvider
                .GetRequiredService<IRedisConnectionFactory>().Connection().CloseAsync();
        }
    }
}
