// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for cst_signup.
	/// </summary>
    public partial class cst_signup : AdminPageBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");


            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("customers.aspx") + "\">" + AppLogic.GetString("admin.menu.Customers", SkinID, LocaleSetting) + "</a> - " + AppLogic.GetString("admin.phoneorder.button.CreateNewCustomer", SkinID, LocaleSetting) + "";
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
			if(CommonLogic.QueryStringCanBeDangerousContent("unknownerror").Length > 0)
			{
                writer.Append("<p align=\"left\"><b><font color=\"#FF0000\">" + AppLogic.GetString("admin.common.CstMsg8", SkinID, LocaleSetting) + "</font></b></p>");
			}
			if(CommonLogic.QueryStringCanBeDangerousContent("badlogin").Length > 0)
			{
                writer.Append("<p align=\"left\"><b><font color=\"#FF0000\">" + AppLogic.GetString("admin.common.CstMsg4", SkinID, LocaleSetting) + "</font></b></p>");
			}
			if(CommonLogic.QueryStringCanBeDangerousContent("msg").Length > 0)
			{
                writer.Append("<p align=\"left\"><b><font color=\"blue\">" + AppLogic.GetString("admin.common.CstMsg6", SkinID, LocaleSetting) + "</font></b></p>");
			}

			if(CommonLogic.QueryStringNativeInt("errormsg") > 0)
			{
                ErrorMessage e = new ErrorMessage(CommonLogic.QueryStringNativeInt("errormsg"));
				writer.Append("<p align=\"left\"><b><font color=\"red\">" + Server.HtmlEncode(e.Message) + "</font></b></p>");
			}

            bool AllowShipToDifferentThanBillTo = AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") && !AppLogic.AppConfigBool("SkipShippingOnCheckout");

			writer.Append("<script type=\"text/javascript\">\n");
      
			writer.Append("function copyaccount(theForm)\n");
			writer.Append("{\n");
			writer.Append("if (theForm.ShippingEqualsAccount.checked)\n");
			writer.Append("{\n");
			writer.Append("theForm.AddressFirstName.value = theForm.FirstName.value;\n");
			writer.Append("theForm.AddressLastName.value = theForm.LastName.value;\n");
			writer.Append("theForm.AddressPhone.value = theForm.Phone.value;\n");
			writer.Append("}\n");
			writer.Append("return (true);\n");
			writer.Append("}\n");
			writer.Append("\n");

			writer.Append("function NewCustForm_Validator(theForm)\n");
			writer.Append("{\n");
			writer.Append("  submitonce(theForm);\n");
			writer.Append("  if (theForm.FirstName.value == \"\" && theForm.LastName.value == \"\")\n");
			writer.Append("  {\n");
            writer.Append("    alert(\"" + AppLogic.GetString("admin.common.CstMsg10", SkinID, LocaleSetting) + "\");\n");
			writer.Append("    theForm.FirstName.focus();\n");
			writer.Append("    submitenabled(theForm);\n");
			writer.Append("    return (false);\n");
			writer.Append("  }\n");
			writer.Append("  if (theForm.Password.value.length < 5)\n");
			writer.Append("  {\n");
            writer.Append("    alert(\"" + AppLogic.GetString("admin.cst_signup.PasswordNotification", SkinID, LocaleSetting) + "\");\n");
			writer.Append("    theForm.Password.focus();\n");
			writer.Append("    submitenabled(theForm);\n");
			writer.Append("    return (false);\n");
			writer.Append("  }\n");
			writer.Append("  if (theForm.Password.value != theForm.Password2.value)\n");
			writer.Append("  {\n");
            writer.Append("    alert(\"" + AppLogic.GetString("admin.cst_signup.PasswordMismatch", SkinID, LocaleSetting) + "\");\n");
			writer.Append("    theForm.Password2.focus();\n");
			writer.Append("    submitenabled(theForm);\n");
			writer.Append("    return (false);\n");
			writer.Append("  }\n");
            writer.Append("  if (theForm.StoreName.value == 0)\n");
            writer.Append("  {\n");
            writer.Append("    alert(\"" + AppLogic.GetString("admin.cst_signup.CustomerStoreMap", SkinID, LocaleSetting) + "\");\n");
            writer.Append("    theForm.StoreName.focus();\n");
            writer.Append("    submitenabled(theForm);\n");
            writer.Append("    return (false);\n");
            writer.Append("  }\n");
			
			writer.Append("  return (true);\n");
			writer.Append("}\n");

            writer.Append("function setFocus() {\n");
            writer.Append("    document.getElementById('FirstName').focus();\n");
            writer.Append("}\n");

            writer.Append("    window.onload = setFocus;\n");

			writer.Append("</script>\n");
			
			writer.Append("  <!-- calendar stylesheet -->\n");
			writer.Append("  <link rel=\"stylesheet\" type=\"text/css\" media=\"all\" href=\"jscalendar/calendar-win2k-cold-1.css\" title=\"win2k-cold-1\" />\n");
			writer.Append("\n");
			writer.Append("  <!-- main calendar program -->\n");
			writer.Append("  <script type=\"text/javascript\" src=\"jscalendar/calendar.js\"></script>\n");
			writer.Append("\n");
			writer.Append("  <!-- language for the calendar -->\n");
			writer.Append("  <script type=\"text/javascript\" src=\"jscalendar/lang/" + Localization.JSCalendarLanguageFile() + "\"></script>\n");
			writer.Append("\n");
			writer.Append("  <!-- the following script defines the Calendar.setup helper function, which makes\n");
			writer.Append("       adding a calendar a matter of 1 or 2 lines of code. -->\n");
			writer.Append("  <script type=\"text/javascript\" src=\"jscalendar/calendar-setup.js\"></script>\n");
			
			String act = AppLogic.AdminLinkUrl("cst_signup_process.aspx");
			writer.Append("<form method=\"POST\" action=\"" + act + "\" name=\"NewCustForm\" id=\"NewCustForm\")\" onSubmit=\"return (validateForm(this) && NewCustForm_Validator(this))\">");

			// ACCOUNT BOX:
			writer.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
			writer.Append("<tr><td align=\"left\" valign=\"top\">\n");
            writer.Append("<img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/accountinfo.gif") + "\" border=\"0\"><br/>");
			writer.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
			writer.Append("<tr><td align=\"left\" valign=\"top\">\n");

			writer.Append("    <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">");
			writer.Append("      <tr>");
            writer.Append("        <td width=\"100%\" colspan=\"2\"><b>" + AppLogic.GetString("admin.common.CstMsg5", SkinID, LocaleSetting) + "</b></td>");
			writer.Append("      </tr>");
			writer.Append("      <tr>");
			writer.Append("        <td width=\"100%\" colspan=\"2\">");
			writer.Append("          <hr/>");
			writer.Append("        </td>");
			writer.Append("      </tr>");
			writer.Append("      <tr>");
            writer.Append("        <td width=\"25%\" align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.phoneorder.label.FirstName", SkinID, LocaleSetting) + "</td>");
			writer.Append("        <td width=\"75%\" align=\"left\" valign=\"middle\">");
			writer.Append("        <input type=\"text\" id=\"FirstName\" name=\"FirstName\" size=\"20\" maxlength=\"50\" value=\"\">");
            writer.Append("        <input type=\"hidden\" name=\"FirstName_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("admin.cst_signup.EnterFirstName", SkinID, LocaleSetting) + "]\">");
			writer.Append("        </td>");
			writer.Append("      </tr>");
			writer.Append("      <tr>");
            writer.Append("        <td width=\"25%\" align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.phoneorder.label.LastName", SkinID, LocaleSetting) + "</td>");
			writer.Append("        <td width=\"75%\" align=\"left\" valign=\"middle\">");
			writer.Append("        <input type=\"text\" name=\"LastName\" size=\"20\" maxlength=\"50\" value=\"\">");
            writer.Append("        <input type=\"hidden\" name=\"LastName_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("admin.cst_signup.EnterLastName", SkinID, LocaleSetting) + "]\">");
			writer.Append("        </td>");
			writer.Append("      </tr>");
			writer.Append("      <tr>");
            writer.Append("        <td width=\"25%\" align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.common.E-Mail", SkinID, LocaleSetting) + ":</td>");
			writer.Append("        <td width=\"75%\" align=\"left\" valign=\"middle\">");
			writer.Append("        <input type=\"text\" name=\"EMail\" size=\"37\" maxlength=\"100\" value=\"\">");
            writer.Append("        <input type=\"hidden\" name=\"EMail_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("admin.cst_signup.EnterEmailAddress", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("admin.common.ValidE-MailAddressPrompt", SkinID, LocaleSetting) + "]\">");
			writer.Append("        </td>");
			writer.Append("      </tr>");
			writer.Append("      <tr>");
            writer.Append("        <td width=\"25%\" align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.cst_signup.CreatePassword", SkinID, LocaleSetting) + ":</td>");
			writer.Append("        <td width=\"75%\" align=\"left\" valign=\"middle\">");
			writer.Append("        <input type=\"password\" name=\"Password\" size=\"37\" maxlength=\"100\" value=\"\">");
            writer.Append("        <input type=\"hidden\" name=\"Password_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("admin.cst_signup.EnterPassword", SkinID, LocaleSetting) + "]\">\n");
			writer.Append("        </td>");
			writer.Append("      </tr>");
						
			writer.Append("      <tr>");
            writer.Append("        <td width=\"25%\" align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.cst_signup.RepeatPassword", SkinID, LocaleSetting) + "</td>");
			writer.Append("        <td width=\"75%\" align=\"left\" valign=\"middle\">");
			writer.Append("        <input type=\"password\" name=\"Password2\" size=\"37\" maxlength=\"100\" value=\"\">");
            writer.Append("        <input type=\"hidden\" name=\"Password2_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("admin.cst_signup.VerifyPassword", SkinID, LocaleSetting) + "]\">\n");
			writer.Append("        </td>");
			writer.Append("      </tr>");
            

            StringBuilder tmpS = new StringBuilder(4096);
            System.Collections.Generic.List<Store> StoreInfo = Store.GetStoreList();
            string storeName = string.Empty;
            writer.Append("      <tr>");
            writer.Append("        <td width=\"25%\" align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.cst_signup.MapToStore", SkinID, LocaleSetting) + "</td>");
            writer.Append("        <td width=\"75%\" align=\"left\" valign=\"middle\">");
            tmpS.Append("<select size=\"1\" id=\"StoreName\" name=\"StoreName\">");
            tmpS.Append("<option value=\"0\">" + AppLogic.GetString("admin.cst_signup.SelectStore", SkinID, LocaleSetting) + "</option>");
            foreach (var store in StoreInfo)
            {
                storeName = CommonLogic.IIF(store.IsDefault, store.Name + " (" + AppLogic.GetString("admin.cst_signup.Default", SkinID, LocaleSetting) + ")", store.Name);
                tmpS.Append("<option value=\"" + store.StoreID + "\">" + storeName + "</option>");
            }
            tmpS.Append("</select>");
            writer.Append(tmpS);
            writer.Append("        </td>");
            writer.Append("      </tr>");
            writer.Append("      <tr>");
            writer.Append("        <td width=\"25%\" align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.cst_account.SubscriptionExpiresOn", SkinID, LocaleSetting) + "</td>");
            writer.Append("        <td width=\"75%\" align=\"left\" valign=\"middle\">");
            writer.Append("        <input type=\"text\" name=\"SubscriptionExpiresOn\" id=\"SubscriptionExpiresOn\" size=\"12\" value=\"\">&nbsp;<img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/calendar.gif") + "\" class=\"actionelement\" align=\"absmiddle\" id=\"f_trigger_s\">");

            writer.Append("        </td>");
            writer.Append("      </tr>");

			writer.Append("      <tr>");
            writer.Append("        <td width=\"25%\" align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.phoneorder.label.RequiredPhone", SkinID, LocaleSetting) + "</td>");
			writer.Append("        <td width=\"75%\" align=\"left\" valign=\"middle\">");
			writer.Append("        <input type=\"text\" name=\"Phone\" size=\"14\" maxlength=\"20\" value=\"\">");
			writer.Append("        </td>");
			writer.Append("      </tr>");

			writer.Append("      <tr>");
            writer.Append("        <td width=\"25%\" align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.cst_account.SpecialOfferCouponCode", SkinID, LocaleSetting) + "</td>");
			writer.Append("        <td width=\"75%\" align=\"left\" valign=\"middle\">");
            writer.Append("        <input type=\"text\" name=\"CouponCode\" size=\"14\" maxlength=\"50\" value=\"\">" + AppLogic.GetString("admin.common.CstMsg2", SkinID, LocaleSetting) + "");
			writer.Append("        </td>");
			writer.Append("      </tr>");

			writer.Append("      <tr>");
            writer.Append("        <td width=\"25%\" align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.cst_account.DateOfBirth", SkinID, LocaleSetting) + "</td>");
			writer.Append("        <td width=\"75%\" align=\"left\" valign=\"middle\">");
			writer.Append("        <input type=\"text\" name=\"DateOfBirth\" size=\"14\" maxlength=\"20\" value=\"\"> " + String.Format(AppLogic.GetString("admin.cst_signup.InFormat", SkinID, LocaleSetting),Localization.ShortDateFormat()));
			
			writer.Append("        </td>");
			writer.Append("      </tr>");

			writer.Append("      <tr>");
            writer.Append("        <td width=\"25%\" align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.phoneorder.label.OkToEmail", SkinID, LocaleSetting) + "</td>");
			writer.Append("        <td width=\"75%\" align=\"left\" valign=\"middle\">");
			writer.Append(AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"OKToEMail\" value=\"1\"  checked >\n");
			writer.Append(AppLogic.GetString("admin.common.No", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"OKToEMail\" value=\"0\">\n");
            writer.Append("<small>" + AppLogic.GetString("admin.common.CstMsg1", SkinID, LocaleSetting) + "</small>");
			writer.Append("        </td>");
			writer.Append("      </tr>");

            writer.Append("        <tr>");
            writer.Append("            <td width=\"25%\" align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.cst_account.Notes", SkinID, LocaleSetting) + "</td>");
            writer.Append("            <td width=\"75%\" align=\"left\" valign=\"top\"> <textarea rows=\"5\" style=\"width: 100%;\" name=\"Notes\"></textarea></td>");
            writer.Append("        </tr>");

			writer.Append("</table>\n");
			
			writer.Append("</td></tr>\n");
			writer.Append("</table>\n");
			writer.Append("</td></tr>\n");
			writer.Append("</table>\n");

            writer.Append("<p align=\"center\"><input class=\"normalButtons\" type=\"submit\" value=\"" + AppLogic.GetString("admin.cst_signup.CreateCustomerRecord", SkinID, LocaleSetting) + "\" name=\"Continue\"></p>");
			writer.Append("</form>");
            writer.Append("\n<script type=\"text/javascript\">\n");
            writer.Append("    Calendar.setup({\n");
            writer.Append("        inputField     :    \"SubscriptionExpiresOn\",      // id of the input field\n");
            writer.Append("        ifFormat       :    \"" + Localization.JSCalendarDateFormatSpec() + "\",       // format of the input field\n");
            writer.Append("        showsTime      :    false,            // will display a time selector\n");
            writer.Append("        button         :    \"f_trigger_s\",   // trigger for the calendar (button ID)\n");
            writer.Append("        singleClick    :    true            // Single-click mode\n");
            writer.Append("    });\n");
            writer.Append("</script>\n");

            ltContent.Text = writer.ToString();
		}

	}
}
