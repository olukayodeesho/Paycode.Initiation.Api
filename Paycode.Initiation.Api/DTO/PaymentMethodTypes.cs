using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Paycode.Initiation.Api.DTO
{
    public static class PaymentMethodTypes
    {
        /// <summary>
        /// Verve m-Pin
        /// </summary>
        public const String VMP = "VMP";

        /// <summary>
        /// Verve eCash
        /// </summary>
        public const String QTA = "QTA";

        /// <summary>
        /// Mobile Money Virtual Accounts.
        /// </summary>
        public const String MMO = "MMO";
    }
}