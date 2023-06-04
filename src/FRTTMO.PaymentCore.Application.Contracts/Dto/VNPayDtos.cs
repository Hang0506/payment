using FRTTMO.PaymentCore.Entities;
using Newtonsoft.Json;
using System;

namespace FRTTMO.PaymentCore.Dto
{
    public class VNPayCheckOrderInputDto
    {
        [JsonProperty("txnId")]
        public string TxnId { get; set; }

        [JsonProperty("payDate")]
        public DateTime PayDate { get; set; }

        [JsonProperty("shopCode")]
        public string ShopCode { get; set; }
    }
    public class VNPayCheckOrderOutputDto : InternalIntergrationResponseBase, IEWalletPayed
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("masterMerchantCode")]
        public string MasterMerchantCode { get; set; }

        [JsonProperty("merchantCode")]
        public string MerchantCode { get; set; }

        [JsonProperty("terminalID")]
        public string TerminalID { get; set; }

        [JsonProperty("billNumber")]
        public string BillNumber { get; set; }

        [JsonProperty("txnId")]
        public string TxnId { get; set; }

        [JsonProperty("payDate")]
        public string PayDate { get; set; }

        [JsonProperty("qrTrace")]
        public string QrTrace { get; set; }

        [JsonProperty("bankCode")]
        public string BankCode { get; set; }

        [JsonProperty("debitAmount")]
        public string DebitAmount { get; set; }

        [JsonProperty("realAmount")]
        public string RealAmount { get; set; }

        [JsonProperty("checkSum")]
        public string CheckSum { get; set; }

        //public bool IsPayed() => Is_Success();
        public bool IsPayed { get; set; }
        //public bool Is_Success() => Code == "00";
    }


}
