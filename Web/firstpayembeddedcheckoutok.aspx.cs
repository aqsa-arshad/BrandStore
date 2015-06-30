// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontEncrypt;
using System.Collections.Generic;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for paypalexpressok.
    /// </summary>
    public partial class firstpayembeddedcheckoutok : SkinBase
    {
        private bool QSResultCode
        {
            get
            {
                bool success;
                if (Request.QueryString["success"] == null || !bool.TryParse(Request.QueryString["success"], out success))
                    return false;

                return success;
            }
        }
        private string QSResponseMessage
        {
            get
            {
                if (string.IsNullOrEmpty(Request.QueryString["error_message"]))
                    return "There was an error processing your payment. Please contact customer support.";

                AppLogic.CheckForScriptTag(Request.QueryString["error_message"]);

                return Server.UrlDecode(Request.QueryString["error_message"]);
            }
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {

            String postData = String.IsNullOrEmpty(Request.Form.ToString()) ? Request.QueryString.ToString() : Request.Form.ToString();
            
            FirstPayCallBackProcessor processor = new FirstPayCallBackProcessor(ThisCustomer, postData);

            //post back from gateway
            if (FirstPay.order_id > 0 && processor.OrderNumber == FirstPay.order_id)           
            {
                string redirectPage = processor.ProcessCallBack();
                
                //set the order_id back to 0, eliminates edge cases where 1stpay complains about duplicate order numbers.
                FirstPay.order_id = 0;

                Response.Redirect(redirectPage, true);
            }
            else
                Response.Redirect("~", true);
        }
    }
}
