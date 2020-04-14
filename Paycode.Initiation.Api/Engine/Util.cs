using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Paycode.Initiation.Api.dbml;
using Paycode.Initiation.Api.DTO;

namespace Paycode.Initiation.Api.Engine
{
    public class Util
    {
        public static PaycodeGenerationResponse RetrievePaycodeGenerationCustomResponse(CardlessWithdrawalTransaction transaction) 
        {
            var response = new PaycodeGenerationResponse();
            try
             {
                if (transaction != null)
                {
                    response.amount = (decimal) transaction.TransactionAmount;
                    response.isSuccessful = true;
                    response.payCode = transaction.PayWithMobileToken;
                    response.tokenLifeTimeInMinutes = (int) transaction.TokenLifeTimeInMinutes;
                    response.transactionRef = transaction.TransactionReference;
                }
                else { response.isSuccessful = false;  }
              } catch (Exception e) { }
            return response; 
        }
    }
}