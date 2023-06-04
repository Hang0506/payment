namespace FRTTMO.PaymentCore
{
    public static class PaymentCoreErrorCodes
    {
        public const string Prefix = "Pay_Core";
        //Add your business exception error codes here...
        public static string Error_ValidationException = $"{Prefix}:00100";
        public static string ERROR_EXCEPTION = $"{Prefix}:99902";
        public static string ERROR_DATA_NULL_EXCEPTION = $"{Prefix}:99903";
        public static string ERROR_CONCURENCY_EXCEPTION = $"{Prefix}:99904";
        public static string ERROR_DATA_EXISTED_EXCEPTION = $"{Prefix}:99905";
        public static string ERROR_DUPLICATE_EXCEPTION = $"{Prefix}:00905";

        #region Customer
        public static string ERROR_CUSTOMER_INACTIVE = $"{Prefix}:00702";
        public static string ERROR_CUSTOMER_NOTFOUND = $"{Prefix}:00701";
        //
        public static string ERROR_CUSTOMER_EXISTS = $"{Prefix}:01701";
        public static string ERROR_CUSTOMER_FULL_NAME = $"{Prefix}:01702";
        public static string ERROR_CUSTOMER_TAX_NUMBER = $"{Prefix}:01703";
        public static string ERROR_CUSTOMER_ADDRESS = $"{Prefix}:01704";
        public static string ERROR_CUSTOMER_MOBILE = $"{Prefix}:01705";
        public static string ERROR_CUSTOMER_EMAIL = $"{Prefix}:01706";
        #endregion
        #region Payment Info
        public static string ERROR_PAYMENT_INFO_NOTFOUND = $"{Prefix}:00708";
        public static string ERROR_PAYMENT_DATA_CHANGED = $"{Prefix}:00709";
        public static string ERROR_ORDER_CODE_NOT_EMPTY = $"{Prefix}:00511";
        //public static string ERROR_ORDER_RETURN_NOTFOUND = $"{Prefix}:00504";
        public static string ERROR_PAYMENT_REQUEST_NOTFOUND = $"{Prefix}:00512";
        public static string ERROR_TRANSACTION_TYPE_INVALID = $"{Prefix}:00513";
        public static string ERROR_PAYMENT_REQUEST_BY_ORDERCODE_EXISTED = $"{Prefix}:00514";
        public static string ERROR_PAYMENT_CHECKSUM_TD_TS_NOTMATCH = $"{Prefix}:00815";
        public static string ERROR_PAYMENT_REQUEST_COMPLETE_CANCEL = $"{Prefix}:00848";
        public static string ERROR_PAYMENT_TYPE_INVALID = $"{Prefix}:00515";

        #endregion

        public static string ERROR_ORDERCODE_NOTFOUND = $"{Prefix}:00602";


        //Rechange wallet
        public static string ERROR_ORDER_CANCEL = $"{Prefix}:00703";
        public static string ERROR_ACCOUNT_NOTFOUND = $"{Prefix}:00704";
        public static string ERROR_MONNEY_PAYMENT_NOTEQUAL = $"{Prefix}:00705";
        public static string ERROR_MONNEY_PAYMENT_NOTMATCHORDER = $"{Prefix}:00706";
        public static string ERROR_MONNEY_PAYMENT_NOTENOUGH = $"{Prefix}:00707";

        public static string ERROR_ORDERRETURNID_PAYMENT_NOTFOUND = $"{Prefix}:00801";
        public static string ERROR_PAYMENT_ORDERRETURNID_NOT_REMEMBER_ORDER = $"{Prefix}:00802";
        public static string ERROR_PAYMENT_REQUEST_CODE_INVALID = $"{Prefix}:00806";

        public static string ERROR_PAYMENT_REQUEST_CODE_NOTFOUND = $"{Prefix}:00807";
        public static string ERROR_PAYMENT_NOT_TRANSACTION = $"{Prefix}:00808";
        public static string ERROR_PAYMENT_NOT_TRANFER_ACCOUNT_NOTFOUND = $"{Prefix}:00809";

        public static string ERROR_PROVIDER_QRCODE_INVALID = $"{Prefix}:00805";
        public static string ERROR_PAYMENT_METHOD_INVALID = $"{Prefix}:00804";
        public static string ERROR_PAYMENT_PAYMENTREQUEST_CANCELFAILD = $"{Prefix}:00816";
        public static string ERROR_PAYMENT_PAYMENTREQUEST_STATUS = $"{Prefix}:00817";
        public static string ERROR_PAYMENT_PAYMENTREQUEST_DONT_SELL = $"{Prefix}:00819";
        public static string ERROR_PAYMENT_FILE_LENGTH_INVALID = $"{Prefix}:00820";
        public static string ERROR_PAYMENT_FILE_TYPE_INVALID = $"{Prefix}:00821";
        public static string ERROR_PAYMENT_PAYEMNT_METHOD_NOT_COLLECT_MONNEY = $"{Prefix}:00822";
        public static string ERROR_PAYMENT_PAYEMNT_REQUEST_COMPLETED = $"{Prefix}:00823";
        public static string ERROR_PAYMENT_PAYEMNT_REQUEST_CANCEL = $"{Prefix}:00824";
        public static string ERROR_PAYMENT_TRANSFERNUM_EXISTED = $"{Prefix}:00825";
        public static string ERROR_PAYMENT_DEPOSIT_REMAINMONNEY_INVALID = $"{Prefix}:00826";
        public static string ERROR_PAYMENT_DEPOSITMONEY_MORETHAN_REMAINMONEY = $"{Prefix}:00827";
        public static string ERROR_PAYMENT_ENTITY_INVALID = $"{Prefix}:00840";
        public static string ERROR_PAYMENT_TRANSACTIONSELL_MONEY = $"{Prefix}:00842";
        public static string ERROR_PAYMENT_DEPOSIT_VOUCHER_AMOUNT_NOTMATCH_AMOUNT_VERIFY = $"{Prefix}:00845";
        public static string ERROR_PAYMENT_REQUEST_ID_INVALID = $"{Prefix}:00847";
        public static string ERROR_PAYMENT_GETBANK_TYPE_INVALID = $"{Prefix}:00846";


        #region Adjust Payment
        public static string ERROR_PAYMENT_CODE_EXITS = $"{Prefix}:00901";
        public static string ERROR_PAYMENT_NOT_COLLECT_MONNEY = $"{Prefix}:00902";
        public static string ERROR_PAYMENTTRANSACTION_NOT_VALID = $"{Prefix}:00903";
        public static string ERROR_PAYMENT_CODE_NOT_VALID = $"{Prefix}:00904";
        public static string ERROR_PAYMENT_FINISHED = $"{Prefix}:00906";
        public static string ERROR_MONNEY_PAYMENT_DEPOSIT_NOTENOUGH = $"{Prefix}:00907";
        public static string ERROR_PAYMENT_REQUEST_NOT_FINISH = $"{Prefix}:00908";
        public static string ERROR_QR_EXITS = $"{Prefix}:00909";
        #endregion

        #region Payoo
        public static string ERROR_CHECK_TRANS_FAIL = $"{Prefix}:00900";
        #endregion
    }
    public static class PaymentCoreErrorMessageKey
    {
        public static string Detail = "Detail";
        public static string Message = "Message";
        public static string Data = "Data";
        public static string OrderCode = "OrderCode";
        public static string OrderId = "OrderId";
        public static string AccountId = "AccountId";
        public static string PaymentRequestId = "PaymentRequestId";
        public static string PaymentRequestCode = "PaymentRequestCode";
        public static string PaymentCode = "PaymentCode";
        public static string MessageDetail = "MessageDetail";
    }
}
