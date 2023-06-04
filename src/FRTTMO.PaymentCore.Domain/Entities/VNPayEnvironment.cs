using Newtonsoft.Json;
using System.Collections.Generic;

namespace FRTTMO.PaymentCore.Entities
{
    public class VNPayEnvironment
    {
        public static Dictionary<string, string> ErrorCodes_CheckOrder = new Dictionary<string, string>
        {
            {"00","Giao dịch thành công" },
            {"01","Không tìm thấy giao dịch" },
            {"02","PayDate không đúng định dạng" },
            {"03","TxnId không được null hoặc empty" },
            {"04", "Giao dich thất bại."},
            {"05","Giao dich nghi vấn." },
            {"14","IP bị khóa." },
            {"11","Dữ liệu đầu vào không đúng định dạng." },
            {"99","Internal error." }
        };
    }
    public class VNPayCheckOrderEntity
    {
        /// <summary>
        /// Số hóa đơn đối với Qr Terminal động
        /// </summary>
        public string txnId { get; set; }

        /// <summary>
        /// Mã của merchant
        /// </summary>
        public string merchantCode { get; set; }

        /// <summary>
        /// Mã của terminal
        /// </summary>
        public string terminalID { get; set; }

        /// <summary>
        /// Thời gian giao dịch của bank truyền về cho hệ thống MMS, định dạng “dd/MM/yyyy”
        /// </summary>
        public string payDate { get; set; }

        /// <summary>
        /// Checksum của dữ liệu gửi. Được tính theo công thức (trong đó secretKey là một mã bí mật): data =EncodeMD5(payDate|txnId|merchantCode|terminalID|secretKey)
        /// </summary>
        public string checkSum { get; set; }

        public string GetChecksumStr(string iSecretKey) => $"{payDate}|{txnId}|{merchantCode}|{terminalID}|{iSecretKey}";
    }
    public abstract class VNPayResultBase
    {
        /// <summary>
        /// Mã lỗi trả về
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }
        /// <summary>
        /// Mô tả mã lỗi đính kèm
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

    }
    public class VNPayCheckOrderResultEntity : VNPayResultBase
    {
        /// <summary>
        /// Tên master merchant
        /// </summary>
        [JsonProperty("masterMerchantCode")]
        public string MasterMerchantCode { get; set; }

        /// <summary>
        /// Định danh merchant
        /// </summary>
        [JsonProperty("merchantCode")]
        public string MerchantCode { get; set; }

        /// <summary>
        /// Định danh terminal
        /// </summary>
        [JsonProperty("terminalID")]
        public string TerminalID { get; set; }

        /// <summary>
        /// Số hóa đơn
        /// </summary>
        [JsonProperty("billNumber")]
        public string BillNumber { get; set; }

        /// <summary>
        /// Số hóa đơn
        /// </summary>
        [JsonProperty("txnId")]
        public string TxnId { get; set; }

        /// <summary>
        /// Thời gian thanh toán dd/MM/yyyy HH:mm:ss
        /// </summary>
        [JsonProperty("payDate")]
        public string PayDate { get; set; }

        /// <summary>
        /// Số trace VNPAY
        /// </summary>
        [JsonProperty("qrTrace")]
        public string QrTrace { get; set; }

        /// <summary>
        /// Ngân hang thanh toán
        /// </summary>
        [JsonProperty("bankCode")]
        public string BankCode { get; set; }

        /// <summary>
        /// Số tiền trước KM VND ####0
        /// </summary>
        [JsonProperty("debitAmount")]
        public string DebitAmount { get; set; }

        /// <summary>
        /// Số tiền sau khuyến mãi VND ####0
        /// </summary>
        [JsonProperty("realAmount")]
        public string RealAmount { get; set; }

        /// <summary>
        /// Checksum của dữ liệu gửi. Được tính theo công thức data =EncodeMD5(MasterMerchantCode|MerchantCode|TerminalID|TxnId|PayDate|BankCode|QrTrace|DebitAmount|RealAmount|secretKey)
        /// </summary>
        [JsonProperty("checkSum")]
        public string CheckSum { get; set; }

        public bool Is_Success() => Code == "00";
        public string GetChecksumStr(string iSecretKey) => $"{MasterMerchantCode}|{MerchantCode}|{TerminalID}|{TxnId}|{PayDate}|{BankCode}|{QrTrace}|{DebitAmount}|{RealAmount}|{iSecretKey}";//EncodeMD5()
    }
}
