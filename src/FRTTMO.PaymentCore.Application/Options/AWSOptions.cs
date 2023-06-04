namespace FRTTMO.PaymentCore.Options
{
    public class AWSOptions
    {
        public string BucketName { get; set; }
        public long FileMaxLength { get; set; }
        public long ExpiresMinutes { get; set; }
        public long ExpiresMinutesPresignUrlUpload { get; set; }
    }
}
