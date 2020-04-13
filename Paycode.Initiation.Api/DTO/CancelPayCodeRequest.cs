using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Paycode.Initiation.Api.DTO
{
    [Serializable]
    public class CancelPayCodeRequest
    {
        // [JsonIgnore]
        public string transactionRef { get; set; }

        public string frontEndPartner { get; set; }


    }
}