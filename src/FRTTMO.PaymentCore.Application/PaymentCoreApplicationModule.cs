using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.Application;
using FRTTMO.CoreCustomerAPI;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using FRTTMO.PaymentIntegration;
using Volo.Abp.FluentValidation;

namespace FRTTMO.PaymentCore
{
    [DependsOn(
        typeof(AbpFluentValidationModule),
        typeof(PaymentCoreDomainModule),
        typeof(PaymentCoreApplicationContractsModule),
        typeof(AbpDddApplicationModule),
        typeof(AbpAutoMapperModule),
        typeof(CoreCustomerAPIApplicationContractsModule),
        typeof(PaymentIntegrationApplicationContractsModule),
        typeof(PaymentIntegrationApplicationContractsModule),
        typeof(DebitService.DebitServiceApplicationContractsModule)
        )]
    public class PaymentCoreApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            context.Services.AddAutoMapperObjectMapper<PaymentCoreApplicationModule>();
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<PaymentCoreApplicationModule>(validate: false);
            });
            //Configure<VNPayConnect>( configuration.GetSection(EnvironmentSetting.VNPayConnect));
            //Configure<SmartpayConnect>(configuration.GetSection(EnvironmentSetting.SmartPayConnect));
            //Configure<GotITConnect>(configuration.GetSection(EnvironmentSetting.GotITConnect));
            context.Services.AddAWSS3(configuration);
        }
    }
    public static class AWS
    {
        public static void AddAWSS3(this IServiceCollection services, IConfiguration configuration)
        {
            var _config = new AmazonS3Config
            {
                ServiceURL = configuration["AWSS3:ServiceURL"],
                ForcePathStyle = true,
                //RegionEndpoint =  /*Amazon.RegionEndpoint.GetBySystemName(configuration["AWSS3:region"])*/ Amazon.RegionEndpoint.USWest2
            };
            var s3 = new AmazonS3Client(
                awsAccessKeyId: configuration["AWSS3:awsAccessKeyId"],
                awsSecretAccessKey: configuration["AWSS3:awsSecretAccessKey"],
                clientConfig: _config
            );
            services.AddSingleton<IAmazonS3>(s3);
        }
    }
}
