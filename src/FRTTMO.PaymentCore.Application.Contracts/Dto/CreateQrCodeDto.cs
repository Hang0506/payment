using System;
using System.ComponentModel.DataAnnotations;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Dto
{
    public class CreateQrCodeDto
    {
        public class CreateQrCodeInputDto
        {
            public EnmWalletProvider ProviderQrCode { get; set; }

            public string ServiceCode { set; get; }
            public string CountryCode { set; get; }
            public string PayType { set; get; }
            public string ProductId { set; get; }
            public string TxnId { set; get; }
            public string BillNumber { set; get; }
            public decimal? Amount { set; get; }
            public string Ccy { set; get; }
            public DateTime? ExpDate { set; get; }
            public string Desc { set; get; }
            public string MasterMerCode { set; get; }
            public string TipAndFee { set; get; }
            public string ConsumerID { set; get; }
            public string Purpose { set; get; }
            //
            public string SubChannel { set; get; }
            public string OrderNo { set; get; }
            public string ShopCode { set; get; }
            public string Extras { set; get; }
            public DateTime? Created { set; get; }
        }
        public class CreateQrCodeOutputDto
        {
            public string QrCode { set; get; }
        }

        public class CreateQrCodeVNPayInputDto
        {
            [MaxLength(20)]
            public string serviceCode { set; get; }
            [MaxLength(2)]
            public string countryCode { set; get; }
            [MaxLength(4)]
            public string payType { set; get; }
            [MaxLength(20)]
            public string productId { set; get; }
            [MaxLength(15)]
            public string txnId { set; get; }
            [MaxLength(15)]
            public string billNumber { set; get; }
            [MaxLength(13)]
            public string amount { set; get; }
            [MaxLength(3)]
            public string ccy { set; get; }
            [MaxLength(14)]
            public string expDate { set; get; }
            [MaxLength(19)]
            public string desc { set; get; }
            [MaxLength(100)]
            public string masterMerCode { set; get; }
            [MaxLength(20)]
            public string tipAndFee { set; get; }
            [MaxLength(20)]
            public string consumerID { set; get; }
            [MaxLength(19)]
            public string purpose { set; get; }
            [MaxLength(8)]
            public string terminalId { set; get; }
            [MaxLength(100)]
            public string appId { set; get; }
            [MaxLength(25)]
            public string merchantName { set; get; }
            [MaxLength(20)]
            public string merchantCode { set; get; }
            [MaxLength(9)]
            public string merchantType { set; get; }
            [MaxLength(32)]
            public string checksum { set; get; }
        }
        public class CreateQrCodeVNPayOutputDto
        {
            public string Code { set; get; }
            public string Message { set; get; }
            public string Data { set; get; }
            public string Url { set; get; }
            public string CheckSum { set; get; }
            public bool IsSuccess() => Code == "00";
        }

        public class CreateQrCodeSmartpayInputDto
        {
            public string merchantId { set; get; }
            public string notifyUrl { set; get; }
            public string channel { set; get; }
            public string branchCode { set; get; }
            //
            [MaxLength(8)]
            public string subChannel { set; get; }
            [MaxLength(128)]
            public string desc { set; get; }
            [MaxLength(25)]
            public string orderNo { set; get; }
            [MaxLength(10)]
            public string terminalId { set; get; }
            [MaxLength(256)]
            public string extras { set; get; }
            [MaxLength(10)]
            public long amount { set; get; }
            [MaxLength(5)]
            public string requestType { set; get; }
            [Required]
            public string created { set; get; }
        }
        public class CreateQrCodeSmartpayOutputDto
        {
            public string Code { set; get; }
            public CreateCodeSmartpayResponseData Data { set; get; }
            public bool IsSuccess() => Code == "OK";
        }

        public class CreateCodeSmartpayResponseData
        {
            public string Payload { set; get; }
        }
    }
}
