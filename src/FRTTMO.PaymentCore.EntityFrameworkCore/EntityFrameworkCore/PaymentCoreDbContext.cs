using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace FRTTMO.PaymentCore.EntityFrameworkCore
{
    [ConnectionStringName(PaymentCoreDbProperties.ConnectionStringName)]
    public class PaymentCoreDbContext : AbpDbContext<PaymentCoreDbContext>, IPaymentCoreDbContext
    {
        /* Add DbSet for each Aggregate Root here. Example:
         * public DbSet<Question> Questions { get; set; }
         */

        public PaymentCoreDbContext(DbContextOptions<PaymentCoreDbContext> options) 
            : base(options)
        {

        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<PaymentRequest> PaymentRequests { get; set; }
        public DbSet<EWalletDeposit> EWalletDeposits { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<COD> COD { get; set; }
        public DbSet<Transfer> Transfer { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<LogApi> LogApis { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { set; get; }
        public DbSet<Vendor> Vendor { set; get; }
        public DbSet<VendorPin> VendorPin { set; get; }
        public DbSet<Bank> Banks { set; get; }
        public DbSet<BankCard> BankCards { set; get; }
        public DbSet<CardType> CardTypes { get; set; }
        public DbSet<BankAccount> BankAccounts { set; get; }
        public DbSet<PaymentMethodDetail> PaymentMethodDetail { get; set; }
        public DbSet<CreditSales> CreditSales { get; set; }
        public DbSet<VendorDetail> VendorDetail { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<PaymentSource> PaymentTransaction { get; set; }
        public DbSet<BankingOnline> BankingOnline { get ; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ConfigurePaymentCore();
        }
    }
}