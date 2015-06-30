// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using AspDotNetStorefrontCore;
using System.Web;
using System.Data.SqlClient;
using System.Data;

namespace Vortx.AdnsfHelpers
{
    public class AddressHelper
    {
        public Address Address { get; set; }

        public AddressHelper(Address address)
        {
            this.Address = address;
        }

        public String InputCardHTML(Customer ThisCustomer, bool Validate, bool CheckForTerms)
        {
            StringBuilder tmpS = new StringBuilder(1000);

            tmpS.Append("<script type=\"text/javascript\" language=\"Javascript\" src=\"jscripts/tooltip.js\" /></script> ");

            // Credit Card fields

            if (this.Address.CardExpirationMonth == "")
            {
                this.Address.CardExpirationMonth = CommonLogic.ParamsCanBeDangerousContent("CardExpirationMonth");
            }

            if (this.Address.CardExpirationYear == "")
            {
                this.Address.CardExpirationYear = CommonLogic.ParamsCanBeDangerousContent("CardExpirationYear");
            }

            if (this.Address.CardType == "")
            {
                this.Address.CardType = CommonLogic.ParamsCanBeDangerousContent("CardType");
            }

            if (this.Address.CardName == "")
            {
                this.Address.CardName = CommonLogic.ParamsCanBeDangerousContent("CardName");
            }

            if (this.Address.CardNumber == "")
            {
                this.Address.CardNumber = CommonLogic.ParamsCanBeDangerousContent("CardNumber");
            }

            if (this.Address.CardExpirationMonth == "")
            {
                this.Address.CardExpirationMonth = CommonLogic.ParamsCanBeDangerousContent("CardExpirationMonth");
            }

            string CardExtraCode = AppLogic.SafeDisplayCardExtraCode(AppLogic.GetCardExtraCodeFromSession(ThisCustomer));
            if (CardExtraCode == "")
            {
                CardExtraCode = CommonLogic.ParamsCanBeDangerousContent("CardExtraCode");
            }

            tmpS.Append("    <table id=\"checkoutCreditCardForm\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">");

            tmpS.Append("      <tr><td colspan=2 height=10></td></tr>");

            // Vortx
            // -- Here is where we needed to make a change -- to allow mobile framework to checkout
            // 
            if (ThisCustomer.MasterShouldWeStoreCreditCardInfo ||
                CommonLogic.GetThisPageName(false).Equals("mobilecheckoutpayment.aspx", StringComparison.InvariantCultureIgnoreCase) || 
                CommonLogic.GetThisPageName(false).Equals("checkoutpayment.aspx", StringComparison.InvariantCultureIgnoreCase) || 
                CommonLogic.GetThisPageName(false).Equals("editaddressrecurring.aspx", StringComparison.InvariantCultureIgnoreCase))
            {
                tmpS.Append("      <tr>");
                tmpS.Append("        <td >" + AppLogic.GetString("address.cs.23", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</td>\n");
                tmpS.Append("        <td >\n");
                tmpS.Append("        <input type=\"text\" name=\"CardName\" id=\"CardName\" size=\"20\" maxlength=\"100\" value=\"" + HttpContext.Current.Server.HtmlEncode(this.Address.CardName.Trim()) + "\">\n");
                if (Validate)
                {
                    tmpS.Append("        <input type=\"hidden\" name=\"CardName_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("address.cs.24", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "]\">\n");
                }
                tmpS.Append("        </td>");
                tmpS.Append("      </tr>");

                tmpS.Append("     <tr><td colspan=2 height=2></td></tr>");
                tmpS.Append("      <tr>");
                tmpS.Append("        <td >" + AppLogic.GetString("address.cs.25", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</td>");
                tmpS.Append("        <td >\n");

                tmpS.Append("        <input type=\"text\" autocomplete=\"off\" name=\"CardNumber\" id=\"CardNumber\" size=\"20\" maxlength=\"20\" value=\""
                    + AppLogic.SafeDisplayCardNumber(this.Address.CardNumber, "Address", Address.AddressID) + "\"> "
                    + AppLogic.GetString("shoppingcart.cs.106", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\n");
                if (Validate)
                {
                    tmpS.Append("        <input type=\"hidden\" name=\"CardNumber_vldt\" value=\"[req][len=8][blankalert=" + AppLogic.GetString("address.cs.26", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "][invalidalert=" + AppLogic.GetString("address.cs.27", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "]\">\n");
                }
                tmpS.Append("        </td>");
                tmpS.Append("      </tr>");

                tmpS.Append("      <tr><td width=\"100%\" height=\"10\" colspan=\"2\"></td></tr>");

                if (CommonLogic.GetThisPageName(false).Equals("checkoutpayment.aspx", StringComparison.InvariantCultureIgnoreCase) ||
                    CommonLogic.GetThisPageName(false).Equals("mobilecheckoutpayment.aspx", StringComparison.InvariantCultureIgnoreCase))
                {
                    tmpS.Append("      <tr>");
                    tmpS.Append("        <td >" + AppLogic.GetString("address.cs.28", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</td>");
                    tmpS.Append("        <td >\n");
                    tmpS.Append("        <input type=\"text\" autocomplete=\"off\" name=\"CardExtraCode\" id=\"CardExtraCode\" size=\"5\" maxlength=\"10\" value=\"" + CardExtraCode + "\">\n");
                    if (Validate)
                    {
                        tmpS.Append("        <input type=\"hidden\" name=\"CardExtraCode_vldt\" value=\"" + CommonLogic.IIF(!AppLogic.AppConfigBool("CardExtraCodeIsOptional"), "[req]", "") + "[len=3][blankalert=" + AppLogic.GetString("address.cs.29", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "][invalidalert=" + AppLogic.GetString("address.cs.30", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "]\">\n");
                    }
                    tmpS.Append("        </td>");
                    tmpS.Append("      </tr>");
                    tmpS.Append("      <tr><td width=\"100%\" height=\"10\" colspan=\"2\"></td></tr>");
                }

                tmpS.Append("      <tr>");
                tmpS.Append("        <td >" + AppLogic.GetString("address.cs.31", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</td>");
                tmpS.Append("        <td >\n");
                tmpS.Append("        <select size=\"1\" name=\"CardType\" id=\"CardType\">");
                tmpS.Append("				<option value=\"\">" + AppLogic.GetString("address.cs.32", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rsCard = DB.GetRS("select * from creditcardtype  with (NOLOCK)  where Accepted=1 order by CardType", dbconn))
                    {
                        while (rsCard.Read())
                        {
                            tmpS.Append("<option value=\"" + DB.RSField(rsCard, "CardType") + "\" " + CommonLogic.IIF(this.Address.CardType == DB.RSField(rsCard, "CardType"), " selected ", "") + ">" + DB.RSField(rsCard, "CardType") + "</option>\n");
                        }
                    }
                }

                tmpS.Append("              </select>\n");
                tmpS.Append("        </td>");
                tmpS.Append("      </tr>");

                tmpS.Append("      <tr><td width=\"100%\" height=\"2\" colspan=\"2\"></td></tr>");

                tmpS.Append("      <tr>");
                tmpS.Append("        <td >" + AppLogic.GetString("address.cs.33", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</td>");
                tmpS.Append("        <td >\n");
                tmpS.Append("        <select size=\"1\" name=\"CardExpirationMonth\" id=\"CardExpirationMonth\">");
                tmpS.Append("<option value=\"\">" + AppLogic.GetString("address.cs.34", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                for (int i = 1; i <= 12; i++)
                {
                    tmpS.Append("<option value=\"" + i.ToString().PadLeft(2, '0') + "\" " + CommonLogic.IIF(this.Address.CardExpirationMonth == i.ToString().PadLeft(2, '0'), " selected ", "") + ">" + i.ToString().PadLeft(2, '0') + "</option>");
                }
                tmpS.Append("</select>    <select size=\"1\" name=\"CardExpirationYear\" id=\"CardExpirationYear\">");
                tmpS.Append("<option value=\"\" SELECTED>" + AppLogic.GetString("address.cs.35", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                for (int y = 0; y <= 10; y++)
                {
                    tmpS.Append("<option value=\"" + (System.DateTime.Now.Year + y).ToString() + "\" " + CommonLogic.IIF(this.Address.CardExpirationYear == (System.DateTime.Now.Year + y).ToString(), " selected ", "") + ">" + (System.DateTime.Now.Year + y).ToString() + "</option>");
                }
                tmpS.Append("</select>");
                tmpS.Append("</td></tr>\n");

                if (AppLogic.AppConfigBool("ShowCardStartDateFields"))
                {
                    tmpS.Append("      <tr><td width=\"100%\" height=\"10\" colspan=\"2\"></td></tr>");

                    tmpS.Append("     <tr><td colspan=2 height=2></td></tr>");
                    tmpS.Append("      <tr>");
                    tmpS.Append("        <td >*" + AppLogic.GetString("address.cs.59", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</td>");
                    tmpS.Append("        <td >\n");

                    String CardStartDateMonth = String.Empty;
                    try
                    {
                        this.Address.CardStartDate.Substring(0, 2);
                    }
                    catch { }
                    String CardStartDateYear = String.Empty;
                    try
                    {
                        CardStartDateYear = this.Address.CardStartDate.Substring(2, 2);
                    }
                    catch { }

                    tmpS.Append("        <select size=\"1\" name=\"CardStartDateMonth\" id=\"CardStartDateMonth\">");
                    tmpS.Append("<option value=\"\">" + AppLogic.GetString("address.cs.34", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                    for (int i = 1; i <= 12; i++)
                    {
                        tmpS.Append("<option value=\"" + i.ToString().PadLeft(2, '0') + "\" " + CommonLogic.IIF(CardStartDateMonth == i.ToString().PadLeft(2, '0'), " selected ", "") + ">" + i.ToString().PadLeft(2, '0') + "</option>");
                    }
                    tmpS.Append("</select>    <select size=\"1\" name=\"CardStartDateYear\" id=\"CardStartDateYear\">");
                    tmpS.Append("<option value=\"\" SELECTED>" + AppLogic.GetString("address.cs.35", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                    for (int y = -4; y <= 10; y++)
                    {
                        tmpS.Append("<option value=\"" + (System.DateTime.Now.Year + y).ToString().Substring(2, 2) + "\" " + CommonLogic.IIF(CardStartDateYear == (System.DateTime.Now.Year + y).ToString(), " selected ", "") + ">" + (System.DateTime.Now.Year + y).ToString() + "</option>");
                    }
                    tmpS.Append("</select>");

                    tmpS.Append("        </td>");
                    tmpS.Append("      </tr>");
                    tmpS.Append("      <tr><td width=\"100%\" height=\"10\" colspan=\"2\"></td></tr>");

                    tmpS.Append("     <tr><td colspan=2 height=2></td></tr>");
                    tmpS.Append("      <tr>");
                    tmpS.Append("        <td >" + AppLogic.GetString("address.cs.61", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</td>");
                    tmpS.Append("        <td >\n");
                    tmpS.Append("        <input type=\"text\" autocomplete=\"off\" name=\"CardIssueNumber\" id=\"CardIssueNumber\" size=\"2\" maxlength=\"2\" value=\"" + this.Address.CardIssueNumber + "\"> " + AppLogic.GetString("address.cs.63", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\n");
                    tmpS.Append("        </td>");
                    tmpS.Append("      </tr>");
                    tmpS.Append("      <tr><td width=\"100%\" height=\"10\" colspan=\"2\"></td></tr>");
                }
            }

            tmpS.Append("</table>\n");

            return tmpS.ToString();
        }

    }
}

