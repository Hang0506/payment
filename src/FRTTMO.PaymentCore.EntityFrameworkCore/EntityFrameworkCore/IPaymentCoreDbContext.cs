using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace FRTTMO.PaymentCore.EntityFrameworkCore
{
    [ConnectionStringName(PaymentCoreDbProperties.ConnectionStringName)]
    public interface IPaymentCoreDbContext : IEfCoreDbContext
    {
        /* Add DbSet for each Aggregate Root here. Example:
         * DbSet<Question> Questions { get; }
         */
        DbSet<Account> Accounts { get; set; }
        DbSet<Customer> Customers { get; set; }
        DbSet<Transaction> Transactions { get; set; }
        DbSet<PaymentRequest> PaymentRequests { get; set; }
        DbSet<EWalletDeposit> EWalletDeposits { get; set; }
        DbSet<Card> Cards { get; set; }
        DbSet<COD> COD { get; set; }
        DbSet<Transfer> Transfer { get; set; }
        DbSet<Voucher> Vouchers { get; set; }
        DbSet<LogApi> LogApis { get; set; }
        DbSet<PaymentMethod> PaymentMethods { get; set; }
        DbSet<Vendor> Vendor { get; set; }
        DbSet<VendorPin> VendorPin { get; set; }
        DbSet<Bank> Banks { get; set; }
        DbSet<BankCard> BankCards { set; get; }
        DbSet<CardType> CardTypes { get; set; }
        DbSet<BankAccount> BankAccounts { get; set; }
        DbSet<PaymentMethodDetail> PaymentMethodDetail { get; set; }
        DbSet<CreditSales> CreditSales { get; set; }
        DbSet<VendorDetail> VendorDetail { get; set; }
        DbSet<Payment> Payment { get; set; }
        DbSet<PaymentSource> PaymentTransaction { get; set; }
        DbSet<BankingOnline> BankingOnline { get; set; }
    }
}