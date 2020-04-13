using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Paycode.Initiation.Api.DTO
{
    [Serializable]
    public class MobileTokenRequest
    {
        // [JsonIgnore]
        public string subscriberId { get; set; }

        public string paymentMethodTypeCode { get; set; }

        public string paymentMethodCode { get; set; }

        //Returned WICode
        [JsonIgnore]
        public string payWithMobileToken { get; set; }


        public string frontEndPartnerId { get; set; }

        public int tokenLifeTimeInMinutes { get; set; }

        public string payWithMobileChannel { get; set; }

        public string providerToken { get; set; }

        public string transactionType { get; set; }

        public string codeGenerationChannel { get; set; }
        [JsonIgnore]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public String FormattedAmount
        {
            get
            {
                return String.Format("{0:N0}", (Amount * 100)).Replace(",", "");
            }
        }

        public string ttid { get; set; }
        //Optional
        public string codeGenerationChannelProvider { get; set; }
        public string oneTimePin { get; set; }
        [JsonIgnore]
        public string Secure { get; set; }
        [JsonIgnore]
        public string pinData { get; set; }
        public string macData { get; set; }


    }

}