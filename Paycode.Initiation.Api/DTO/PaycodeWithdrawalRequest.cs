using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Paycode.Initiation.Api.DTO
{
    public class PaycodeWithdrawalRequest
    {
        public string IP { get; set; }
        public string transactionID { get; set; }
        public string accountNumber { get; set; }
        public string providerToken { get; set; }
        public string transactionType { get; set; }
        public string sessionAuth { get; set; }
        public decimal amount { get; set; }
    }
}