using System.Collections.Generic;

namespace FRTTMO.PaymentCore.Kafka.Eto
{
    public class PaymentInfoDetailEto : TransactionFullOutputEto
    {
        public TransactionFullOutputDetailEto Detail { set; get; }
    }
    public class TransactionFullOutputDetailEto
    {
        public List<EWalletDepositFullOutputEto> EWallets { set; get; }
        public List<CardFullOutputEto> Cards { set; get; }
        public List<VoucherFullOutputEto> Vouchers { set; get; }
        public List<CODFullOutputEto> CODs { set; get; }
        public List<TransferFullOutputEto> Transfers { set; get; }
    }
}
