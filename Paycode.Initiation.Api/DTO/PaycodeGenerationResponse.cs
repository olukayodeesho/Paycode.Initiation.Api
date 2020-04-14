using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Paycode.Initiation.Api.DTO
{
    public class PaycodeGenerationResponse
    {
        public Boolean isSuccessful { get; set; }
        public string subscriberId { get; set; }
        public string transactionRef { get; set; }
        public decimal amount { get; set; }
        public int tokenLifeTimeInMinutes { get; set; }
        public string payCode { get; set; } 
    }
}