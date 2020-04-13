using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Paycode.Initiation.Api.DTO
{
    [Serializable]
    public class QueryPayCodeStatus
    {
        // [JsonIgnore]
        public string paycode { get; set; }

        public string subscriberID { get; set; }


    }
}