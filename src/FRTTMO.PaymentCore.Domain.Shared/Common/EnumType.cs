using System.ComponentModel;

namespace FRTTMO.PaymentCore.Common
{
    public class EnumType
    {
        public enum CustomerStatus : byte
        {
            InActive = 0,
            Active = 1
        }
        public enum CustomerType : byte
        {
            Company = 1,
            Individual = 2
        }

        public enum EnmWalletProvider
        {
            VNPAY = 1,
            Smartpay = 2,
            Alepay = 3,
            ShopeePay = 4,
            Zalopay = 11,
            Moca = 13,
            Foxpay = 15,
        }
        public enum EnmVoucherProvider : byte
        {
            LC = 1,
            GotIT = 2,
            Taptap = 3,
            UTop = 9,
            Vani = 14,
        }
        public enum EnmPaymentMethod
        {
            Cash = 1,
            Card = 2,
            Wallet = 3,
            Voucher = 4,
            Transfer = 5,
            COD = 6,
            VNPayGateway = 7,
            AlepayGateway = 8,
            [Description("bán nợ")]
            DebtSale = 9,
            MocaEWallet = 10,
            ZaloPayGateway = 11
        }
        public enum EnmTransactionType
        {
            /// <summary>
            /// Nạp tiền vào ví
            /// </summary>
            Recharge = 1,
            /// <summary>
            /// Thanh toán - thu tiền
            /// </summary>
            CollectMoney = 2,
            Refund = 3,
            /// <summary>
            /// Thanh toán - chi tiền
            /// </summary>
            Pay = 4,
            CashBack = 5,
            /// <summary>
            /// Thu tiền đặt cọc
            /// </summary>
            FirstDeposit = 6,
            /// <summary>
            /// Chi tiền cọc
            /// </summary>
            WithdrawDeposit = 7
        }

        public enum EnmTransactionStatus : byte
        {
            Created = 1
        }

        public enum EnmPaymentRequestStatus : byte
        {
            Confirm = 1,
            Cancel = 2,
            Complete = 4
        }

        public enum EmPaymentRequestType : byte
        {
            PaymentCoreRequest = 1,
            PaymentDebitRequest = 2,
            [Description("chi tiền cọc")]
            DepositRefund = 3,
            [Description("bán nợ")]
            DebtSale = 4,
            [Description("dịch vụ")]
            Service = 5,
        }

        public enum EnmTransferIsConfirm : byte
        {
            [Description("Chuyển khoản trước")]
            AdvanceTransfer = 0,
            [Description("Chuyển khoản có phê duyệt")]
            Confirm = 1
        }
        public enum EnmBankType : byte
        {
            TransferAndCard = 0,
            Transfer = 1,
            Card = 2
        }
        public enum EnmPartnerId : byte
        {
            Taptap = 1,
            Gotit = 2,
            VNpay = 3,
            Smartpay = 4,
            Alepay = 5,
            LongChau = 6,
            ShopeePay = 7,
            Momo = 8,
            Utop = 9,
            VPB = 10,
            Zalopay = 11,
            Legacy = 12,
            Moca = 13,
            Foxpay = 15,
        }

        public enum EnmMethodType : byte
        {
            Default = 0,
            GenQrCode = 1,
            VerifyVoucher = 2,
            RedeemVoucher = 3,
            CheckStatus = 4,
            CreatePaymentRequest = 5,
            GetInfo = 6,
            GetInstallmentInfo = 7,
            UpdateOrderPaymentStatusAsync = 8,
        }
        /// <summary>
        /// 1 = dùng chung
        //2 = Thu tiền
        //3 = Chi tiền
        /// </summary>
        public enum EnumPaymentMethodType : byte
        {
            Common = 1,
            TakeDeposit = 2,
            DepositRefund = 3
        }
        public enum EnmCompanyType : byte
        {
            NoDilivery = 0,
            GRAB = 1,
            VNPOST = 2,
            VIETTELPOST = 3,
            AHAMOVE = 4,
            GHN = 5,
            LCD = 6,
            //khách nợ
            CusDebit = 7
        }
        public enum PaymentChannel : byte
        {
            Offline = 0,
            Online = 1,
            Both = 2,
        }
        public enum EnmDebitStatus : byte
        {
            /// <summary>
            /// Chờ xử lý
            /// </summary>
            Pending = 1,
            /// <summary>
            /// Đã gạch nợ
            /// </summary>
            HasBeenDeducted = 4,
            /// <summary>
            /// Đã gạch nợ
            /// </summary>
            Canceled = 13,
        }
        public enum EnmPaymentTransactionStatus : byte
        {
            Complete = 4
        }
        public enum EnmPaymentStatus : byte
        {
            Created = 1,
            Cancel = 2,
            Complete = 4
        }
        public enum EnmPaymentSourceCode: byte
        {
            OMS=1,
            SMO=2,
            RT=3,
            AR = 4,
            ReturnCancelDeposit = 5
        }
        public enum EnmPaymentType : byte
        {
            Thu = 1,
            Chi = 2
        }
        public enum StatusFill : byte
        {
            /// <summary>
            /// payment request status = 4 
            /// </summary>
            Filled = 1,
            /// <summary>
            /// payment request status = 1 
            /// </summary>
            NotFilled = 0
        }
    }
}
