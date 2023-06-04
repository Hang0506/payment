using Volo.Abp;
using Volo.Abp.MongoDB;

namespace FRTTMO.PaymentCore.MongoDB
{
    public static class PaymentCoreMongoDbContextExtensions
    {
        public static void ConfigurePaymentCore(
            this IMongoModelBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));
        }
    }
}
