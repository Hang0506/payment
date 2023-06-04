using Confluent.Kafka;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace FRTTMO.PaymentCore
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {            
            var configBuilderOpts = new AbpConfigurationBuilderOptions();
            var config = ConfigurationHelper.BuildConfiguration(configBuilderOpts);
            services.AddApplication<PaymentCoreHttpApiHostModule>();
            var producerConfig = new ProducerConfig();
            producerConfig.BootstrapServers = config["CAP:Kafka:Connections:Default:BootstrapServers"];

            //add more health check at here 
            services.AddHealthChecks()
                     .AddSqlServer(
                        connectionString: config.GetConnectionString("PaymentCore"),
                        name: "database",
                        failureStatus: HealthStatus.Degraded,
                        tags: new string[] { "db", "sql", "sqlserver" }
                    ).AddKafka(
                        config: producerConfig,
                        name: "kafka",
                        failureStatus: HealthStatus.Degraded,
                        tags: new string[] { "message queue", "message" }
                    );
            //.AddRedis(
            //    redisConnectionString: config["Redis:Configuration"],
            //    name: "redis",
            //    failureStatus: HealthStatus.Degraded,
            //    tags: new string[] { "db", "redis" }
            //);
            services.AddHealthChecksUI().AddInMemoryStorage();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.InitializeApplication();
            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.UseHealthChecksUI();
        }
    }
}
