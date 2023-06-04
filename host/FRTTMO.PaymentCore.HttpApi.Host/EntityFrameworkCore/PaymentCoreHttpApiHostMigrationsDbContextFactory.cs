using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FRTTMO.PaymentCore.EntityFrameworkCore
{
    public class PaymentCoreHttpApiHostMigrationsDbContextFactory : IDesignTimeDbContextFactory<PaymentCoreHttpApiHostMigrationsDbContext>
    {
        public PaymentCoreHttpApiHostMigrationsDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();

            var builder = new DbContextOptionsBuilder<PaymentCoreHttpApiHostMigrationsDbContext>()
                .UseSqlServer(configuration.GetConnectionString("PaymentCore"));

            return new PaymentCoreHttpApiHostMigrationsDbContext(builder.Options);
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            return builder.Build();
        }
    }
}
