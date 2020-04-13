using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Paycode.Initiation.Api.DTO
{
    public class TokenRequest
    {
        public decimal Amount { get; set; }
        public string codeGenerationChannel { get; set; }
        public string frontEndPartnerId { get; set; }
        public string oneTimePin { get; set; }
        public string paymentMethodCode { get; set; }
        public string paymentMethodTypeCode { get; set; }
        public string payWithMobileChannel { get; set; }
        public string payWithMobileToken { get; set; }
        public string providerToken { get; set; }
        public string subscriberId { get; set; }
        public int tokenLifeTimeInMinutes { get; set; }
        public string transactionType { get; set; }
    }
}