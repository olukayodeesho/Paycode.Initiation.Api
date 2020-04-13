using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Paycode.Initiation.Api.DTO
{
    public static class TransactionTypes
    {
        public const String Payment = "Payment";
        public const String CashWithdrawal = "Withdrawal";
        public const String Deposit = "Deposit";
    }
}