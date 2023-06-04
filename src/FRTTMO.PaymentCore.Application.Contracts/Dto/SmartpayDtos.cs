using FRTTMO.PaymentCore.Entities;
using Newtonsoft.Json;
using System;

namespace FRTTMO.PaymentCore.Dto
{
    public class SmartpayCheckOrderInputDto
    {
        /// <summary>
        /// DateTime created QR
        /// </summary>
        [JsonProperty("created")]
        public DateTime CreateTime { get; set; }

        [JsonProperty("orderNo")]
        public string OrderNo { get; set; }
    }

    public abstract class SmartpayResultBaseDto<T> : InternalIntergrationResponseBase
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }
        
        //public bool Is_Success() => Code == "OK";
    }
    public class SmartpayCheckOrderResultDetailDto
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("subChannel")]
        public string SubChannel { get; set; }

        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty("branchCode")]
        public string BranchCode { get; set; }

        [JsonProperty("orderNo")]
        public string OrderNo { get; set; }

        [JsonProperty("transId")]
        public string TransId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("extras")]
        public string Extras { get; set; }
    }
    public class SmartpayCheckOrderOutputDto : SmartpayResultBaseDto<SmartpayCheckOrderResultDetailDto>, IEWalletPayed
    {
        public bool IsPayed { get; set; }
    }



}
