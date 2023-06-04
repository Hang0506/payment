using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FRTTMO.PaymentCore.EntityFrameworkCore;
using StackExchange.Redis;
using Microsoft.OpenApi.Models;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc.UI.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Swashbuckle;
using System.Reflection;
using FRTTMO.PaymentCore.Kafka.Interface;
using FRTTMO.PaymentCore.Kafka.Service;
using Confluent.Kafka;
using Volo.Abp.Http.Client;
using Microsoft.Extensions.Options;
using Volo.Abp.Auditing;
using FRTTMO.CoreCustomerAPI;
using FRTTMO.PaymentCore.Options;
using Elastic.Apm.NetCoreAll;
using Elastic.Apm;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Volo.Abp.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using FRTTMO.PaymentCore.Application.Redis;

namespace FRTTMO.PaymentCore
{
    [DependsOn(
        typeof(PaymentCoreApplicationModule),
        typeof(PaymentCoreEntityFrameworkCoreModule),
        typeof(PaymentCoreHttpApiModule),
        typeof(AbpAspNetCoreMvcUiMultiTenancyModule),
        typeof(AbpAutofacModule),
        typeof(AbpCachingStackExchangeRedisModule),
        typeof(AbpEntityFrameworkCoreSqlServerModule),
        //typeof(AbpAuditLoggingEntityFrameworkCoreModule),
        //TODO Enable when we need to store permission and setting in required db. Disable in template.
        //typeof(AbpPermissionManagementEntityFrameworkCoreModule),
        //typeof(AbpSettingManagementEntityFrameworkCoreModule),
        typeof(AbpAspNetCoreSerilogModule),
        //typeof(AbpEventBusRabbitMqModule),
        //TODO Enable this if we use kafka instead of rabbitmq
        //typeof(AbpKafkaModule),
        typeof(AbpSwashbuckleModule),
        typeof(PaymentCoreIntegrateModule),
        typeof(CoreCustomerAPIHttpApiClientModule),
        typeof(DebitService.DebitServiceHttpApiClientModule),
        typeof(PaymentIntegration.PaymentIntegrationHttpApiClientModule),
        typeof(PaymentCoreAPIApplicationRedisModule)
        )]
    public class PaymentCoreHttpApiHostModule : AbpModule
    {
        private readonly string _apiVersion = typeof(PaymentCoreHttpApiModule).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        private readonly string _apiTitle = "PaymentCore API";

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var hostingEnvironment = context.Services.GetHostingEnvironment();
            var configuration = context.Services.GetConfiguration();
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            //add AddElasticsearch
            context.Services.AddElasticsearch(configuration);

            Configure<AbpDbContextOptions>(options =>
            {
                options.UseSqlServer();
            });

            Configure<AbpMultiTenancyOptions>(options =>
            {
                options.IsEnabled = true;
            });

            //if (hostingEnvironment.IsDevelopment())
            //{
            //    Configure<AbpVirtualFileSystemOptions>(options =>
            //    {
            //        options.FileSets.ReplaceEmbeddedByPhysical<PaymentCoreDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}FRTTMO.PaymentCore.Domain.Shared", Path.DirectorySeparatorChar)));
            //        options.FileSets.ReplaceEmbeddedByPhysical<PaymentCoreDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}FRTTMO.PaymentCore.Domain", Path.DirectorySeparatorChar)));
            //        options.FileSets.ReplaceEmbeddedByPhysical<PaymentCoreApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}FRTTMO.PaymentCore.Application.Contracts", Path.DirectorySeparatorChar)));
            //        options.FileSets.ReplaceEmbeddedByPhysical<PaymentCoreApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}FRTTMO.PaymentCore.Application", Path.DirectorySeparatorChar)));
            //    });
            //}

            context.Services.AddAbpSwaggerGenWithOAuth(
                configuration["AuthServer:Authority"],
                new Dictionary<string, string>
                {
                    {"PaymentCore", _apiTitle}
                },
                options =>
                {
                    var provider = context.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                    var apiVersionDescriptions = provider.ApiVersionDescriptions;
                    foreach (var description in apiVersionDescriptions)
                    {
                        options.SwaggerDoc(description.GroupName, new OpenApiInfo
                        {
                            Version = description.ApiVersion.ToString(),
                            Title = $"{_apiTitle} {hostingEnvironment.EnvironmentName ?? ""} - Build {_apiVersion}",
                        });
                    }
                    options.DocInclusionPredicate((docName, desc) =>
                    {
                        var apiVersion = desc.Properties[typeof(ApiVersion)];
                        var descVersion = apiVersionDescriptions.FirstOrDefault(x => x.ApiVersion.ToString() == apiVersion.ToString());
                        return descVersion.GroupName.ToLower() == docName.ToLower();
                    });
                    options.CustomSchemaIds(type => type.FullName);
                   
                });
            #region versioning api
            context.Services.AddAbpApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            context.Services.AddVersionedApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                }
            );
            #endregion
            Configure<AbpLocalizationOptions>(options =>
            {
                options.Languages.Add(new LanguageInfo("en", "en", "English"));
                options.Languages.Add(new LanguageInfo("vi", "vi", "Vietnamese"));
            });

            context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration["AuthServer:Authority"];
                    options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                    options.Audience = "PaymentCore";
                });

            Configure<AWSOptions>(configuration.GetSection(EnvironmentSetting.AWSS3));

            Configure<AbpDistributedCacheOptions>(options =>
            {
                options.KeyPrefix = "PaymentCore:";
            });

            var dataProtectionBuilder = context.Services.AddDataProtection().SetApplicationName("PaymentCore");
            if (!hostingEnvironment.IsDevelopment())
            {
                var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
                dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "PaymentCore-Protection-Keys");
            }

            context.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .WithOrigins(
                            configuration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )
                        .WithAbpExposedHeaders()
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration["CAP:Kafka:Connections:Default:BootstrapServers"]
            };
            context.Services.AddSingleton(producerConfig);
            context.Services.AddCap(option =>
            {
                option.UseMongoDB(cfg =>
                {
                    cfg.DatabaseName = configuration["CAP:DBName"];
                    cfg.DatabaseConnection = configuration["CAP:ConnectionString"];
                });
                //option.UseKafka(configuration["CAP:Kafka:Connections:Default:BootstrapServers"].ToString());
                option.UseKafka(opt =>
                {
                    opt.Servers = configuration["CAP:Kafka:Connections:Default:BootstrapServers"].ToString();
                    if (!string.IsNullOrEmpty(configuration["CAP:Kafka:Protocol"]))
                    {
                        opt.MainConfig.Add("security.protocol", configuration["CAP:Kafka:Protocol"].ToString());
                    }
                    if (!string.IsNullOrEmpty(configuration["CAP:Kafka:Mechanism"]))
                    {
                        opt.MainConfig.Add("sasl.mechanism", configuration["CAP:Kafka:Mechanism"].ToString());
                    }
                    if (!string.IsNullOrEmpty(configuration["CAP:Kafka:Username"]))
                    {
                        opt.MainConfig.Add("sasl.username", configuration["CAP:Kafka:Username"].ToString());
                    }
                    if (!string.IsNullOrEmpty(configuration["CAP:Kafka:Password"]))
                    {
                        opt.MainConfig.Add("sasl.password", configuration["CAP:Kafka:Password"].ToString());
                    }
                    opt.MainConfig.Add("allow.auto.create.topics", configuration["CAP:Kafka:AutoCreateTopics"]);
                });

                var failedRetryCount = int.Parse(configuration["Cap:FailedRetryCount"].ToString());
                var group = configuration["Cap:Group"].ToString();
                var succeedMessageExpiredAfter = int.Parse(configuration["Cap:SucceedMessageExpiredAfter"].ToString());
                var consumerThreadCount = int.Parse(configuration["Cap:ConsumerThreadCount"].ToString());
                var failedRetryInterval = int.Parse(configuration["Cap:FailedRetryInterval"].ToString());

                option.DefaultGroupName = configuration["Cap:DefaultGroupName"].ToString() ?? option.DefaultGroupName;
                option.ConsumerThreadCount = consumerThreadCount;
                option.FailedRetryInterval = failedRetryInterval;
                option.FailedRetryCount = failedRetryCount;
                option.SucceedMessageExpiredAfter = succeedMessageExpiredAfter;
                option.UseDashboard(o => o.PathMatch = "/cap");
            });
            context.Services.AddScoped(typeof(IPublishService<>), typeof(PublishService<>));
            context.Services.AddHttpClient(EnvironmentSetting.RemoteOMSService).ConfigureHttpClient((service, client) =>
            {
                var options = service.GetRequiredService<IOptions<AbpRemoteServiceOptions>>();
                var posOption = options.Value.RemoteServices.GetConfigurationOrDefault(EnvironmentSetting.RemoteOMSService);
                client.BaseAddress = new Uri(posOption.BaseUrl);
            });
            context.Services.AddHttpClient(EnvironmentSetting.PaymentIntegration).ConfigureHttpClient((service, client) =>
            {
                var options = service.GetRequiredService<IOptions<AbpRemoteServiceOptions>>();
                var posOption = options.Value.RemoteServices.GetConfigurationOrDefault(EnvironmentSetting.PaymentIntegration);
                client.BaseAddress = new Uri(posOption.BaseUrl);
            });
            Configure<AbpAuditingOptions>(options =>
            {
                options.IsEnabled = false; //Disables the auditing system
            });
            Configure<PaymentOptions>(configuration.GetSection("PaymentOptions"));
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();
            var _configuration = context.GetConfiguration();
            app.UseAllElasticApm(_configuration);
            Agent.Subscribe(new HttpDiagnosticsSubscriber());
            Agent.Subscribe(new EfCoreDiagnosticsSubscriber());



            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseCorrelationId();
            app.UseStaticFiles();
            app.UseRouting();
            //Comment All api go through gateway and cors are implemented in gateway. Don't enable cors here.
            //app.UseCors();
            app.UseAuthentication();
            app.UseMultiTenancy();
            app.UseAbpRequestLocalization();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseAbpSwaggerUI(options =>
            {
                var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"PaymentCore {description.GroupName.ToUpperInvariant()}");
                }

                var configuration = context.GetConfiguration();
                options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
                options.OAuthClientSecret(configuration["AuthServer:SwaggerClientSecret"]);
                options.OAuthScopes("PaymentCore");
            });
            app.UseAuditing();
            app.UseAbpSerilogEnrichers();
            app.UseConfiguredEndpoints();
        }
    }
}
