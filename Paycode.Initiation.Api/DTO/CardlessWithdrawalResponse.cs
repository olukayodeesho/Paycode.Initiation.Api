using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Paycode.Initiation.Api.DTO
{
    public class CardlessWithdrawalResponse
    {
        public string ResponseCode
        {
            get;
            set;
        }

        public string ResponseDescription
        {
            get;
            set;
        }

        public CardlessWithdrawalResponse()
        {
        }
    }
}