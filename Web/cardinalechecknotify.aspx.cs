// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Globalization;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for cardinalechecknotify.
    /// </summary>
    public partial class cardinalechecknotify : System.Web.UI.Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            String AuthResult = String.Empty;
            String NotificationID = CommonLogic.FormCanBeDangerousContent("NotificationId");

            if (NotificationID.Length != 0)
            {
                String PAResStatus = String.Empty;
                String CardinalOrderId = String.Empty;
                String SignatureVerification = String.Empty;
                String ErrorNo = String.Empty;
                String ErrorDesc = String.Empty;

                String CardinalAuthenticateResult = String.Empty;
                AuthResult = Cardinal.MyECheckStatus(NotificationID, out CardinalOrderId, out PAResStatus, out SignatureVerification, out ErrorNo, out ErrorDesc, out CardinalAuthenticateResult);

                // We can't match up this request with a customer cart to place the order at this point.
                // Cardinal.MyECheckStatus() logs to FailedTransactions.
            }
        }
    }
}
