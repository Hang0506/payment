using Volo.Abp.Modularity;
using Volo.Abp.Localization;
using FRTTMO.PaymentCore.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Validation;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace FRTTMO.PaymentCore
{
    [DependsOn(
        typeof(AbpValidationModule)
    )]
    public class PaymentCoreDomainSharedModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<PaymentCoreDomainSharedModule>();
            });

            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Add<PaymentCoreResource>("en")
                    .AddBaseTypes(typeof(AbpValidationResource))
                    .AddVirtualJson("/Localization/PaymentCore");
            });

            Configure<AbpExceptionLocalizationOptions>(options =>
            {
                options.MapCodeNamespace(PaymentCoreErrorCodes.Prefix, typeof(PaymentCoreResource));
            });
        }
    }
}
