// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;
using System.Text;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for cst_selectaddress.
    /// </summary>
    public partial class cst_selectaddress : AdminPageBase
    {

        private Customer TargetCustomer;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            TargetCustomer = new Customer(CommonLogic.QueryStringUSInt("CustomerID"), true);
            if (TargetCustomer.CustomerID == 0)
            {
                Response.Redirect(AppLogic.AdminLinkUrl("customers.aspx"));
            }
            if (TargetCustomer.IsAdminSuperUser && !ThisCustomer.IsAdminSuperUser)
            {
                throw new ArgumentException(AppLogic.GetString("admin.common.SecurityException", SkinID, LocaleSetting));
            }
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("customers.aspx") + "?searchfor=" + TargetCustomer.CustomerID.ToString() + "\">" + AppLogic.GetString("admin.menu.Customers", SkinID, LocaleSetting) + "</a> - " + AppLogic.GetString("admin.phoneorder.label.AccountInfo", SkinID, LocaleSetting) + " <a href=\"" + AppLogic.AdminLinkUrl("cst_account.aspx") + "?customerid=" + TargetCustomer.CustomerID.ToString() + "\">" + TargetCustomer.FullName() + " (" + TargetCustomer.EMail + ")</a>";
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            bool AllowShipToDifferentThanBillTo = AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") && !AppLogic.AppConfigBool("SkipShippingOnCheckout");
            String TabImage = String.Empty;

            AddressTypes AddressType = AddressTypes.Unknown;
            String AddressTypeString = CommonLogic.QueryStringCanBeDangerousContent("AddressType");

            AddressType = (AddressTypes)Enum.Parse(typeof(AddressTypes), AddressTypeString, true);

            if (AddressType == AddressTypes.Billing)
            {
                TabImage = AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/selectbillingaddress.gif");
            }
            if (AddressType == AddressTypes.Shipping)
            {
                TabImage = AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/selectshippingaddress.gif");
            }

            int OriginalRecurringOrderNumber = CommonLogic.QueryStringUSInt("OriginalRecurringOrderNumber");
            string ReturnUrl = CommonLogic.QueryStringCanBeDangerousContent("ReturnUrl");

            // ACCOUNT BOX:
            writer.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
            writer.Append("<tr><td align=\"left\" valign=\"top\">\n");
            writer.Append("<img src=\"skins/Skin_" + SkinID.ToString() + "/images/" + TabImage + "\" border=\"0\"><br/>");
            writer.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
            writer.Append("<tr><td align=\"left\" valign=\"top\">\n");

            writer.Append("</td><tr><td>\n");
            writer.Append("<table width=\"100%\" border=\"0\">");
            writer.Append("<tr>");

            Addresses custAddresses = new Addresses();
            custAddresses.LoadCustomer(TargetCustomer.CustomerID);
            int pos = 0;
            foreach (Address adr in custAddresses)
            {
                writer.Append("<td align=\"left\" valign=\"top\">\n");
                writer.Append(String.Format("<img class=\"actionelement\" src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/usethisaddress.gif") + "\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("cst_selectaddress_process.aspx") + "?CustomerID={0}&AddressType={1}&AddressID={2}&OriginalRecurringOrderNumber={3}&ReturnUrl={4}'\"><br/>", TargetCustomer.CustomerID, AddressType, adr.AddressID, OriginalRecurringOrderNumber, ReturnUrl));
                writer.Append(adr.DisplayHTML(true));
                if (adr.CardNumber.Length != 0)
                {
                    writer.Append(adr.DisplayCardHTML());
                }
                writer.Append(String.Format("<img class=\"actionelement\" src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/edit2.gif") + "\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("cst_editaddress.aspx") + "?CustomerID={0}&AddressType={1}&AddressID={2}&ReturnUrl={3}'\"><br/><br/>", TargetCustomer.CustomerID, AddressType, adr.AddressID, ReturnUrl));
                writer.Append("</td>");
                pos++;
                if ((pos % 2) == 0)
                {
                    writer.Append("</tr><tr>");
                }
            }

            writer.Append("</tr></table>");

            writer.Append("</td></tr>");
            writer.Append("<tr><td align=\"left\" valign=\"top\">\n");

            // ADDRESS BOX:

            Address newAddress = new Address();
            newAddress.AddressType = AddressType;

            String act = String.Format(AppLogic.AdminLinkUrl("cst_selectaddress_process.aspx")+ "?CustomerID={0}&AddressType={1}&ReturnUrl={2}", TargetCustomer.CustomerID, AddressType, AppLogic.ReturnURLEncode(ReturnUrl));
            if (OriginalRecurringOrderNumber != 0)
            {
                act += String.Format("OriginalRecurringOrderNumber={0}&", OriginalRecurringOrderNumber);
            }

            writer.Append("<form method=\"POST\" action=\"" + act + "\" name=\"SelectAddressForm\" id=\"SelectAddressForm\" onSubmit=\"return (validateForm(this))\">");

            writer.Append(String.Format("<hr/><b>" + AppLogic.GetString("admin.cst_selectaddress.OrEnterNewAddress", SkinID, LocaleSetting) + "</b><hr/>", AddressType));

            //Display the Address input form fields
            writer.Append(newAddress.InputHTML());

            //Button to submit the form
            writer.Append("<p align=\"center\"><input type=\"submit\" value=\"" + AppLogic.GetString("admin.cst_selectaddress.AddNewAddress", SkinID, LocaleSetting) + "\" name=\"Continue\" class=\"normalButtons\"></p>");

            writer.Append("</td></tr>\n");
            writer.Append("</table>\n");
            writer.Append("</form>");
            writer.Append("</td></tr>\n");
            writer.Append("</table>\n");
            writer.Append("</td></tr>\n");
            writer.Append("</table>\n");
            ltContent.Text = writer.ToString();
        }

    }
}
