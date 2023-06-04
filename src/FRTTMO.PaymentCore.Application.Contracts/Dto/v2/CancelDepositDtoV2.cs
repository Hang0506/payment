using System;
using System.Collections.Generic;
using System.Text;

namespace FRTTMO.PaymentCore.Dto.v2
{
    public class CashbackDepositRefundBaseDtoV2
    {
        public string PaymentRequestCode { get; set; }
        public decimal TotalPayment { set; get; }
        public InsertTransactionInputDtoV2 Transaction { get; set; }
    }

    public class TransactionCancelDepositTransferV2
    {
        public string PaymentRequestCode { set; get; }
        public Guid? CustomerId { set; get; }
        public string ShopCode { set; get; }
        public DateTime? TransactionTime { set; get; }
        public PaymentTransferInputDto Transfers { get; set; }
        public string CreatedByName { set; get; }
    }
}
