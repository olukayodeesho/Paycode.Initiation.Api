using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Paycode.Initiation.Api.DTO;

namespace Paycode.Initiation.Api.Controllers
{

    [System.Web.Http.RoutePrefix("cardless")]
    public class PaycodeAuthorizationController : ApiController
    {
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("authorize-paycode-withdrawal")]
        public CardlessWithdrawalResponse DoKycTier3Req(PaycodeWithdrawalRequest req)
        {
            PaycodeCore cardlessEngine = new PaycodeCore();
            return cardlessEngine.AuthorizeTransaction(req.IP, req.transactionID, req.accountNumber, req.providerToken, req.transactionType, req.sessionAuth, req.amount);
        }
    }
}
