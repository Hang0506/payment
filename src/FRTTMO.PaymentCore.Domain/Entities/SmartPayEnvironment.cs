using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FRTTMO.PaymentCore.Entities
{
    public class SmartPayEnvironment
    {
        public static Dictionary<string, string> ErrorCodes = new Dictionary<string, string>
        {
            {"OK","The request is processed successfully." },
            {"ERR_INVALID_REQUEST_DATA","Invalid request data." },
            {"ERR_INVALID_TIME","Invalid time." },
            {"ERR_INVALID_SIGN","Invalid signature." },
            {"ERR_INVALID_ORDER_NO","Invalid orderNo." },
            {"ERR_INVALID_URL","Invalid url." },
            {"ERR_INVALID_AMOUNT","Invalid amount." },
            {"ERR_REQUEST_ID_DUPLICATE","RequestId duplicate" },
            {"ERR_ORDER_NO_DUPLICATE","OrderNo duplicate" },
            {"ERR_INVALID_CHANNEL","Channel info is invalid." },
            {"ERR_INVALID_SUB_CHANNEL","SubChannel info is invalid." },
            {"ERR_INVALID_MERCHANT_ID","Merchant Id is invalid." },
            {"ERR_INVALID_BRANCH_CODE","Branch Code is invalid." },
            {"ERR_INVALID_REQUEST_TYPE","Request Type is invalid." },
            {"ERR_INVALID_DESC","Desc is invalid" },
            {"ERR_INVALID_EXTRAS","Extras is invalid" },
            {"ERR_INVALID_TERMINAL_ID","Terminal Id is invalid" },
            {"ERR_INVALID_CALLER","Caller is invalid" },
            {"ERR_TRANSACTION_NOT_FOUND","Transaction not found" },
            {"ERR_MERCHANT_NOT_SUPPORTED","Merchant Id or Channel not supported" },
            {"ERR_EXPIRED","Transaction expired" },
            {"OTHER_ERROR","Something error." }
        };
    }
    public class SmartpayCheckOrderEntity
    {
        [StringLength(3, MinimumLength = 3)]
        public string channel { get; set; }

        [StringLength(25)]
        public string orderNo { get; set; }

        /// <summary>
        /// Format to yyyy-MM-ddTHH:mm:ss
        /// </summary>
        [StringLength(20)]
        public string created { get; set; }
    }
    public abstract class SmartpayResultBase<T>
    { 
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("data")]
        public T Data { get; set; }
        public bool Is_Success() => Code == "OK";
    }
    public class SmartpayCheckOrderResultDetail
    {
        /// <summary>
        /// The Partner code that is assigned by SmartPay
        /// </summary>
        [JsonProperty("channel")]
        public string Channel { get; set; }

        /// <summary>
        /// The subPartner code that is assigned by SmartPay
        /// </summary>
        [JsonProperty("subChannel")]
        public string SubChannel { get; set; }

        /// <summary>
        /// The unique ID assigned by the partner to identify a merchant.MerchantId is the merchant who uses qrcode to do payment.
        /// </summary>
        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        /// <summary>
        /// The unique ID assigned by the partner to identify a store.
        /// </summary>
        [JsonProperty("branchCode")]
        public string BranchCode { get; set; }

        /// <summary>
        /// The same with Create Order API.
        /// </summary>
        [JsonProperty("orderNo")]
        public string OrderNo { get; set; }

        /// <summary>
        /// The serial number assigned by SmartPay to identify a  transaction in the SmartPay system.
        /// </summary>
        [JsonProperty("transId")]
        public string TransId { get; set; }

        /// <summary>
        /// Description of the current transaction status of Trade  Status and guild for the next step
        // (OPEN, PAYED, PROCESSING, PAY_ERROR).
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// Specifies the total amount for a transaction.
        /// </summary>
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Extras data the partner need when response query or  notify.
        /// </summary>
        [JsonProperty("extras")]
        public string Extras { get; set; }
    }
    public class SmartpayCheckOrderResultEntity : SmartpayResultBase<SmartpayCheckOrderResultDetail> { }
}
