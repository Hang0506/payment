using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace FRTTMO.PaymentCore.EntityFrameworkCore
{
    public static class PaymentCoreDbContextModelCreatingExtensions
    {
        public static void ConfigurePaymentCore(
            this ModelBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));
            // Table Customer
            builder.Entity<Customer>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(Customer));

                //Properties
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.Property(q => q.ShopCode).HasMaxLength(10);
                b.Property(q => q.FullName).HasMaxLength(50);
                b.Property(q => q.Mobile).HasMaxLength(20);
                b.Property(q => q.TaxNumber).HasMaxLength(20);
                b.Property(q => q.Address).HasMaxLength(200);
                b.Property(q => q.Email).HasMaxLength(50);
                b.Property(q => q.Gender).HasMaxLength(20);
                b.Property(q => q.IdNumber).HasMaxLength(20);
                b.Property(q => q.CreatedDate);
                b.ConfigureByConvention();
            });
            builder.Entity<Account>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(Account));

                //Properties
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.Property(q => q.AccountNumber).IsRequired();
                b.Property(q => q.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerId")
                    .HasColumnType("uniqueidentifier")
                    ;
                b.Property(q => q.CurrentBalance).HasColumnName("CurrentBalance").HasPrecision(19, 6);
                b.Property(q => q.CreatedDate);
                b.ConfigureByConvention();
                b.Property(q => q.RowVersion).IsRowVersion();
                b.HasIndex(b => b.CustomerId)
                   .HasDatabaseName("index_account_customerId")
                   ;
            });
            builder.Entity<PaymentRequest>(b =>
            {
                b.ToTable(nameof(PaymentRequest));
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.Property(q => q.OrderCode)
                    .HasColumnType("varchar")
                    .HasMaxLength(40);
                b.Property(q => q.PaymentRequestCode)
                    .HasColumnName("PaymentRequestCode")
                    .HasColumnType("varchar")
                    .HasMaxLength(20)
                    .IsUnicode(false);
                b.Property(q => q.CreatedBy).HasMaxLength(50);
                b.Property(q => q.ModifiedBy).HasMaxLength(50);
                b.Property(q => q.TypePayment).HasDefaultValue(Common.EnumType.EmPaymentRequestType.PaymentCoreRequest);
                b.Property(q => q.Status);
                b.Property(q => q.RowVersion).IsRowVersion();
                b.HasIndex(b => b.PaymentRequestCode)
                    .HasDatabaseName("index_paymentRequest_code")
                    ;
                b.Property(q => q.PaymentRequestDate);
                b.ConfigureByConvention();
            });
            builder.Entity<Transaction>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(Transaction));
                b.Property(q => q.ShopCode).HasMaxLength(10);
                b.Property(q => q.Note).HasMaxLength(200);
                b.Property(q => q.CreatedBy).HasMaxLength(50);
                b.Property(q => q.ModifiedBy).HasMaxLength(50);
                b.Property(q => q.Amount).HasPrecision(19, 6);
                b.Property(q => q.TransactionFee).HasPrecision(19, 6);
                b.Property(q => q.PaymentRequestId)
                    .HasColumnName("PaymentRequestId")
                    .HasColumnType("uniqueidentifier")
                ;
                b.HasIndex(b => b.PaymentRequestCode);
                b.Property(q => q.PaymentRequestDate);
                b.Property(q => q.PaymentRequestCode).HasMaxLength(20);
                b.ConfigureByConvention();
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.HasIndex(b => b.PaymentRequestId)
                    .HasDatabaseName("index_paymentRequest_id")
                    ;
            });
            builder.Entity<EWalletDeposit>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(EWalletDeposit));
                b.Property(q => q.TransactionVendor).HasMaxLength(50);
                b.Property(q => q.CreatedBy).HasMaxLength(50);
                b.Property(q => q.ModifiedBy).HasMaxLength(50);
                b.ConfigureByConvention();
                b.Property(q => q.Amount).HasPrecision(19, 6);
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.Property(q => q.TransactionId)
                    .HasColumnName("TransactionId")
                    .HasColumnType("uniqueidentifier")
                ;
                b.HasIndex(b => b.TransactionId)
                   .HasDatabaseName("index_ewallet_transaction_id")
                   ;
            });
            builder.Entity<Card>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(Card));
                b.Property(q => q.BankName).HasMaxLength(100);
                b.Property(q => q.CreatedBy).HasMaxLength(50);
                b.Property(q => q.ModifiedBy).HasMaxLength(50);
                b.Property(q => q.TransactionId)
                    .HasColumnName("TransactionId")
                    .HasColumnType("uniqueidentifier")
                ;
                b.ConfigureByConvention();
                b.Property(q => q.Amount).HasPrecision(19, 6);
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.HasIndex(b => b.TransactionId)
                   .HasDatabaseName("index_card_transaction_id")
                   ;
            });
            builder.Entity<COD>(b =>
            {
                b.ToTable(nameof(COD));
                b.Property(q => q.TransporterName).HasMaxLength(50);
                b.Property(q => q.waybillnumber).HasMaxLength(50);
                b.Property(q => q.CreatedBy).HasMaxLength(50);
                b.Property(q => q.ModifiedBy).HasMaxLength(50);
                b.Property(q => q.TransactionId)
                    .HasColumnName("TransactionId")
                    .HasColumnType("uniqueidentifier")
                ;
                b.ConfigureByConvention();
                b.Property(q => q.Amount).HasPrecision(19, 6);
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.HasIndex(b => b.TransactionId)
                   .HasDatabaseName("index_cod_transaction_id")
                   ;
            });
            builder.Entity<Transfer>(b =>
            {
                b.ToTable(nameof(Transfer));
                b.Property(q => q.AccountNum).HasMaxLength(50);
                b.Property(q => q.AccountName).HasMaxLength(50);
                b.Property(q => q.BankName).HasMaxLength(100);
                b.Property(q => q.Image).HasMaxLength(100);
                b.Property(q => q.TransferNum).HasMaxLength(40);
                b.Property(q => q.Content).HasMaxLength(100);
                b.Property(q => q.CreatedBy).HasMaxLength(50);
                b.Property(q => q.ModifiedBy).HasMaxLength(50);
                b.Property(q => q.ReferenceBanking).HasMaxLength(40);
                b.ConfigureByConvention();
                b.Property(q => q.Amount).HasPrecision(19, 6);
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.Property(q => q.TransactionId)
                    .HasColumnName("TransactionId")
                    .HasColumnType("uniqueidentifier")
                ;
                b.HasIndex(b => b.TransactionId)
                   .HasDatabaseName("index_transfer_transaction_id")
                   ;
            });
            builder.Entity<Voucher>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(Voucher));
                b.Property(q => q.Name).HasMaxLength(200);
                b.Property(q => q.Code).HasMaxLength(30);
                b.Property(q => q.CreatedBy).HasMaxLength(50);
                b.Property(q => q.ModifiedBy).HasMaxLength(50);
                b.ConfigureByConvention();
                b.Property(q => q.Amount).HasPrecision(19, 6);
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.Property(q => q.TransactionId)
                    .HasColumnName("TransactionId")
                    .HasColumnType("uniqueidentifier")
                ;
                b.HasIndex(b => b.TransactionId)
                   .HasDatabaseName("index_voucher_transaction_id")
                   ;
            });

            builder.Entity<LogApi>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(LogApi));
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.ConfigureByConvention();
            });

            builder.Entity<PaymentMethod>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(PaymentMethod));
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.ConfigureByConvention();
                //b.HasMany(e=> e.Venders).WithOne().HasForeignKey(qt=> qt.PaymentMethodId); 
            });
            builder.Entity<Vendor>(b =>
            {
                b.ToTable(nameof(Vendor));
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.Property(q => q.VendorName).HasMaxLength(50);
                b.Property(q => q.ImageUrl).HasMaxLength(200);
                b.Property(q => q.ApiUrl).HasMaxLength(100);
                b.Property(q => q.CreatedBy).HasMaxLength(50);
                b.Property(q => q.ModifiedBy).HasMaxLength(50);
                b.Property(q => q.Domestic).HasMaxLength(255);
                b.ConfigureByConvention();
            });
            builder.Entity<VendorPin>(b =>
            {
                b.ToTable(nameof(VendorPin));
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.Property(q => q.ShopCode).HasMaxLength(20);
                b.Property(q => q.PinCode).HasMaxLength(50);
                b.Property(q => q.CreatedBy).HasMaxLength(50);
                b.Property(q => q.ModifiedBy).HasMaxLength(50);
                b.ConfigureByConvention();
            });
            builder.Entity<Bank>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(Bank));
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.ConfigureByConvention();
            });
            builder.Entity<BankCard>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(BankCard));
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.ConfigureByConvention();
            });

            builder.Entity<BankAccount>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(BankAccount));
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.ConfigureByConvention();
            });

            builder.Entity<CardType>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(CardType));
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.ConfigureByConvention();
            });
            builder.Entity<PaymentMethodDetail>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(PaymentMethodDetail));
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.ConfigureByConvention();
            });
            builder.Entity<CreditSales>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(CreditSales));
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.ConfigureByConvention();
            });
            builder.Entity<VendorDetail>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(VendorDetail));
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.Property(q => q.PromotionCode).HasMaxLength(100);
                b.ConfigureByConvention();
            });
            builder.Entity<Payment>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(Payment));
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.ConfigureByConvention();
            });
            builder.Entity<PaymentSource>(b =>
            {
                //Configure table & schema name
                b.ToTable(nameof(PaymentSource));
                b.Property(q => q.Id).IsRequired().ValueGeneratedOnAdd();
                b.ConfigureByConvention();
            });
        }
    }
}
