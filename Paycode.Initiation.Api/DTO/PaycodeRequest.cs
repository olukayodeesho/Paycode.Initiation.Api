using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Paycode.Initiation.Api.DTO
{
    public class PaycodeRequest
    {
        public string sessionAuth { get; set; }
        public string sourceAccount { get; set; }
        public string sourceChannel { get; set; }
        public string subscriberId { get; set; }
        public string oneTimePin { get; set; }
        public string CIF { get; set; }
        public decimal amount { get; set; }
    }
}