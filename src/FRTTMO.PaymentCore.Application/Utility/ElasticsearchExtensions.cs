using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Elasticsearch.Net;
using FRTTMO.PaymentCore.Options;

namespace FRTTMO.PaymentCore
{
    public static class ElasticsearchExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            var diseasesIndexName = configuration.GetSection("LongChauElasticsearch:Indices:PaymentCore")?.Value;

            IConnectionPool pool = null;
            if (configuration.GetSection("LongChauElasticsearch:Url").GetChildren().Count() > 0)
            {
                IConfigurationSection[] arrUris = null;
                if (configuration.GetSection("LongChauElasticsearch:Url").GetChildren().Count() > 0)
                    arrUris = configuration.GetSection("LongChauElasticsearch:Url").GetChildren().ToArray();
                else
                    arrUris = new IConfigurationSection[] {
                        configuration.GetSection("Elasticsearch:Url") };
                var uris = arrUris.Select(v => new Uri(v.Value)).ToArray();
                pool = new StaticConnectionPool(uris);
            }
            else
            {
                var uri = new Uri(configuration.GetSection("LongChauElasticsearch:Url").Value);
                pool = new SingleNodeConnectionPool(uri);
            }
            var settings = new ConnectionSettings(pool)
                .DefaultIndex(diseasesIndexName)
                .EnableDebugMode()
                .PrettyJson()
                .RequestTimeout(TimeSpan.FromMinutes(2));

            if (!String.IsNullOrEmpty(configuration.GetSection("LongChauElasticsearch:Username").Value))
            {
                settings.ServerCertificateValidationCallback((o, certificate, chain, errors) => true);
                settings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);
                settings.BasicAuthentication(configuration.GetSection("LongChauElasticsearch:Username").Value,
                                            configuration.GetSection("LongChauElasticsearch:Password").Value);
            }

            settings.DefaultMappingFor<TransactionIndex>(t => t.IndexName(configuration.GetSection("LongChauElasticsearch:Indices:PaymentCore")?.Value));
            settings.DefaultMappingFor<PaymentTransIndex>(t => t.IndexName(configuration.GetSection("LongChauElasticsearch:Indices:PaymentTrans")?.Value));
            settings.DefaultMappingFor<HeaderFinalIndex>(t => t.IndexName(configuration.GetSection("LongChauElasticsearch:Indices:PaymentHeader")?.Value));

            //kiểm tra model này đã ở ES chưa ,nếu chưa thì lấy cài đặt ở model để đưa lên ES
            var client = new ElasticClient(settings);

            if (!client.Indices.Exists(configuration.GetSection("LongChauElasticsearch:Indices:PaymentCore")?.Value).Exists)
            {
                client.Indices.CreateAsync(configuration.GetSection("LongChauElasticsearch:Indices:PaymentCore").Value,
                    descriptor => descriptor.Map<TransactionIndex>(x =>
                        x.AutoMap()
                    )).GetAwaiter().GetResult();

                client.Indices.RefreshAsync(configuration.GetSection("LongChauElasticsearch:Indices:PaymentCore").Value).GetAwaiter().GetResult();
            }

            services.AddSingleton<IElasticClient>(client);
        }
    }
}
