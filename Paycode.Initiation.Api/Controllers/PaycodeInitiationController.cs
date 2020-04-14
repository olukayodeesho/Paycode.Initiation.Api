using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Paycode.Initiation.Api.DTO;
using Paycode.Initiation.Api.Engine;

namespace Paycode.Initiation.Api.Controllers
{
    [System.Web.Http.RoutePrefix("cardless")]
    public class PaycodeInitiationController : ApiController
    {
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("generate-paycode")]
        public PaycodeGenerationResponse DoKycTier3Req(PaycodeRequest paycodeRequest)
        {
            PaycodeCore cardlessEngine = new PaycodeCore();
            return Util.RetrievePaycodeGenerationCustomResponse(cardlessEngine.GeneratePayCode(paycodeRequest.sessionAuth, paycodeRequest.sourceChannel, paycodeRequest.sourceAccount, paycodeRequest.subscriberId, paycodeRequest.oneTimePin, paycodeRequest.CIF, paycodeRequest.amount));

        }
    }
}
