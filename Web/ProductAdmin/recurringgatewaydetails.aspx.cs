// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for recurringgatewaydetails.
    /// </summary>
    public partial class recurringgatewaydetails : AdminPageBase
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            SectionTitle = "Recurring Subscription Gateway Details";

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            pnlInput1.Visible = false;
            pnlInput2.Visible = false;
            pnlResults.Visible = false;

            if (!ThisCustomer.IsAdminUser) // safety check
            {
                ltError.Text = "<b><font color=red>PERMISSION DENIED</b></font>";
            }
            else
            {
                String RecurringSubscriptionID = CommonLogic.QueryStringCanBeDangerousContent("RecurringSubscriptionID");
                int OriginalRecurringOrderNumber = CommonLogic.QueryStringUSInt("OriginalRecurringOrderNumber");

                if (RecurringSubscriptionID.Length == 0 && OriginalRecurringOrderNumber > 0)
                {
                    RecurringSubscriptionID = AppLogic.GetRecurringSubscriptionIDFromOrder(OriginalRecurringOrderNumber);
                }
                if (OriginalRecurringOrderNumber == 0 && RecurringSubscriptionID.Length != 0)
                {
                    OriginalRecurringOrderNumber = AppLogic.GetOriginalRecurringOrderNumberFromSubscriptionID(RecurringSubscriptionID);
                }

                if (OriginalRecurringOrderNumber == 0 || RecurringSubscriptionID.Length == 0)
                {
                    ltError.Text = "<b><font color=red>Need Original Order Number or Subscription ID</b></font>";
                    pnlInput1.Visible = true;
                    pnlInput2.Visible = true;
                }
                else if (OriginalRecurringOrderNumber > 0 && RecurringSubscriptionID.Length == 0)
                {
                    ltError.Text = "<b><font color=red>Subscription ID was not found for Order Number " + OriginalRecurringOrderNumber.ToString() + "</b></font>";
                    pnlInput1.Visible = true;
                    pnlInput2.Visible = true;
                } 
                else
                {
                    pnlResults.Visible = true;
                    String GW = AppLogic.ActivePaymentGatewayCleaned();
                    ltResults.Text = "<strong>Results from Gateway:</strong><br />";

                    if (GW == Gateway.ro_GWVERISIGN || GW == Gateway.ro_GWPAYFLOWPRO)
                    {
                        ltResults.Text += PayFlowProController.RecurringBillingInquiryDisplay(RecurringSubscriptionID);
                    }
                    else
                    {
                        ltError.Text = "<b><font color=red>Gateway " + GW + " not supported.</b></font>";
                    }
                }
            }
            Page.Form.DefaultFocus = txtOrderNumber.ClientID;
        }

        protected void btnOrderNumber_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppLogic.AdminLinkUrl("recurringgatewaydetails.aspx")+ "?OriginalRecurringOrderNumber=" + txtOrderNumber.Text);
        }
        protected void btnSubscriptionID_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppLogic.AdminLinkUrl("recurringgatewaydetails.aspx")+ "?RecurringSubscriptionID=" + txtSubscriptionID.Text);
        }
}
}
