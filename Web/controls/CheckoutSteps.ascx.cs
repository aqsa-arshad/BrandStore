// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;

namespace AspDotNetStorefront
{

    public partial class CheckoutSteps : System.Web.UI.UserControl
    {

        protected void Page_Load(object sender, EventArgs e)
        {

            SkinBase skinBase = (SkinBase)this.Page;
            Page page = skinBase.Page;

            string pageName = page.GetType().Name;
  
            liAccount.Attributes.Remove("class");
            liShipping.Attributes.Remove("class");
            liPayment.Attributes.Remove("class");
            liReview.Attributes.Remove("class");
            liComplete.Attributes.Remove("class");

            switch (pageName)
            {
                case "checkoutanon_aspx":
                case "createaccount_aspx":
                case "account_aspx":
                    liAccount.Attributes.Add("class", "active");
                    lnkShipping.Enabled = false;
                    lnkPayment.Enabled = false;
                    lnkReview.Enabled = false;
                    lnkComplete.Enabled = false;
                    break;
                case "checkoutshippingmult_aspx":
                case "checkoutshippingmult2_aspx":
                case "checkoutshipping_aspx":
                    liShipping.Attributes.Add("class", "active");
                    lnkPayment.Enabled = false;
                    lnkReview.Enabled = false;
                    lnkComplete.Enabled = false;
                    break;
                case "checkoutgiftcard_aspx":
                case "checkoutpayment_aspx":
                    liPayment.Attributes.Add("class", "active");
                    lnkReview.Enabled = false;
                    lnkComplete.Enabled = false;
                    break;
                case "checkoutreview_aspx":
                    liReview.Attributes.Add("class", "active");
                    lnkComplete.Enabled = false;
                    break;
                case "orderconfirmation_aspx":
                    liComplete.Attributes.Add("class", "active");
                    lnkAccount.Enabled = false;
                    lnkShipping.Enabled = false;
                    lnkPayment.Enabled = false;
                    lnkReview.Enabled = false;
                    break;
                default:
                    break;
            }
        }

    }

}
