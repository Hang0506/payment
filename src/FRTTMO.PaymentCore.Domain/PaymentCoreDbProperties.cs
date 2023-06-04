namespace FRTTMO.PaymentCore
{
    public static class PaymentCoreDbProperties
    {
        public static string DbTablePrefix { get; set; } = "PaymentCore";

        public static string DbSchema { get; set; } = null;

        public const string ConnectionStringName = "PaymentCore";
    }
}
