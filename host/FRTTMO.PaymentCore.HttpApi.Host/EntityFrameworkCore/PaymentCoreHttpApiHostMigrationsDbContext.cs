using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace FRTTMO.PaymentCore.EntityFrameworkCore
{
    public class PaymentCoreHttpApiHostMigrationsDbContext : AbpDbContext<PaymentCoreHttpApiHostMigrationsDbContext>
    {
        public PaymentCoreHttpApiHostMigrationsDbContext(DbContextOptions<PaymentCoreHttpApiHostMigrationsDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ConfigurePaymentCore();
        }
    }
}
