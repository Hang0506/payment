namespace FRTTMO.PaymentCore.RemoteAPIs
{
    public static class InternalApiUrl
    {
        public const string createDebit = "/api/DebitService/debit/create";
        public const string updateCompany = "​/api/DebitService/debit/compay-info";
        public const string infoAccounting = "​/api/DebitService/accounting/info/";
        public const string hasBeenDeducted = "/api/DebitService/accounting/pay";
    }
    public static class InternalOMSApiUrl
    {
        public const string GetListOrderByUpdates = "/api/oms/support/get-list-order-vnpay";
        public const string UpdateDataPaymentRequestCodeVNPay = "/api/oms/support/update-list-order-vnpay";
        public const string orderInfo = "/api/oms/es/get-by-orderCode";
    }
}
    