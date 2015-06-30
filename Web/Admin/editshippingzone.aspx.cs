// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for editshippingzone
    /// </summary>
    public partial class editshippingzone : AdminPageBase
    {

        int ShippingZoneID;
        private int usCountryExist = DB.GetSqlN("select countryid N from Country where TwoLetterISOCode = 'US' and Published = 1 and PostalCodeRequired = 1");

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");


            ShippingZoneID = 0;

            if (CommonLogic.QueryStringCanBeDangerousContent("ShippingZoneID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("ShippingZoneID") != "0")
            {
                Editing = true;
                ShippingZoneID = Localization.ParseUSInt(CommonLogic.QueryStringCanBeDangerousContent("ShippingZoneID"));
            }
            else
            {
                Editing = false;
            }

            if (CommonLogic.FormBool("IsSubmit"))
            {
                StringBuilder sql = new StringBuilder(2500);

                string sZipCodesWithoutSpace = CleanZipCodes(CommonLogic.FormCanBeDangerousContent("ZipCodes"));

                int countryID = CommonLogic.FormNativeInt("hfAddressCountry");
                bool zipCodeEntryValid = ValidateZipCodes(sZipCodesWithoutSpace, countryID);

                if (zipCodeEntryValid)
                {
                if (!Editing)
                {
                    // ok to add:
                    String NewGUID = DB.GetNewGUID();
                    sql.Append("insert into shippingZone(ShippingZoneGUID,Name,ZipCodes, CountryID) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Name")) + ",");
                        sql.Append(DB.SQuote(sZipCodesWithoutSpace) + ",");
                        sql.Append(CommonLogic.FormCanBeDangerousContent("hfAddressCountry"));
                    sql.Append(")");
                    DB.ExecuteSQL(sql.ToString());

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select ShippingZoneID from shippingZone   with (NOLOCK)  where deleted=0 and ShippingZoneGUID=" + DB.SQuote(NewGUID), dbconn))
                        {
                            rs.Read();
                            ShippingZoneID = DB.RSFieldInt(rs, "ShippingZoneID");
                            Editing = true;
                        }
                    }
                    DataUpdated = true;
                    Response.Redirect("shippingzones.aspx", true);
                }
                else
                {
                    int ZoneCountryID;
                    if (!int.TryParse(CommonLogic.FormCanBeDangerousContent("hfAddressCountry"), out ZoneCountryID))
                        ZoneCountryID = usCountryExist;

                    // ok to update:
                    sql.Append("update shippingZone set ");
                    sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXml("Name")) + ",");
                        sql.Append("ZipCodes=" + DB.SQuote(Regex.Replace(CommonLogic.FormCanBeDangerousContent("ZipCodes"), "\\s+", "", RegexOptions.Compiled)) + ",");
                        sql.Append("CountryID=" + ZoneCountryID);
                    sql.Append(" where ShippingZoneID=" + ShippingZoneID.ToString());
                    DB.ExecuteSQL(sql.ToString());
                    DataUpdated = true;
                        Editing = true;
                    }
                }
                else
                {
                    Editing = true;
                    DataUpdated = false;

                    string exampleFormat = AppLogic.GetCountryPostalExample(countryID);
                    ErrorMsg = string.Format(AppLogic.GetString("admin.editshippingzone.EnterZipCodes", SkinID, LocaleSetting), exampleFormat);
                }
            }
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("shippingzones.aspx") + "\">" + AppLogic.GetString("admin.menu.ShippingZones", SkinID, LocaleSetting) + "</a> - " + AppLogic.GetString("admin.editshippingzone.ManageShippingZones", SkinID, LocaleSetting) + "";
            RenderHtml();
        }

        private string CleanZipCodes(string zipCodes)
        {
            return Regex.Replace(zipCodes, "\\s+", "", RegexOptions.Compiled);
        }

        private const int START_RANGE = 0;
        private const int END_RANGE = 1;

        private bool ValidateZipCodes(string zipCodes, int countryID)
        {
            if (zipCodes.Length > 0)
            {
                string[] commaZipCodesSplit = Regex.Split(zipCodes, ",", RegexOptions.IgnorePatternWhitespace);

                foreach (string zipCode in commaZipCodesSplit)
                {
                    //Returns true if the Special Character (-) do not occur or occur 1 time
                    bool isHyphenated = zipCode.IndexOf('-') >= 0;

                    if (isHyphenated)
                    {
                        bool hyphenOcurrenceIsOnlyOnce = zipCode.IndexOf("-") == zipCode.LastIndexOf("-");
                        if (hyphenOcurrenceIsOnlyOnce)
                        {
                            string[] hypenZipCodesSplit = Regex.Split(zipCode, "-", RegexOptions.IgnorePatternWhitespace);

                            if (!ValidateZipCode(hypenZipCodesSplit[START_RANGE], countryID) ||
                                !ValidateZipCode(hypenZipCodesSplit[END_RANGE], countryID))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!ValidateZipCode(zipCode, countryID))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool ValidateZipCode(string zipCode, int countryID)
        {
            if (zipCode.Length < 5)
            {
                zipCode = zipCode.PadRight(5, '0');
            }

            return CommonLogic.IsInteger(zipCode) &&
                   AppLogic.ValidatePostalCode(zipCode, countryID);
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from ShippingZone   with (NOLOCK)  where deleted=0 and ShippingZoneID=" + ShippingZoneID.ToString(), dbconn))
                {
                    Editing = false;
                    if (rs.Read())
                    {
                        Editing = true;
                    }

                    if (ErrorMsg.Length != 0)
                    {
                        writer.Append("<p align=\"left\"><b><font color=red>" + ErrorMsg + "</font></b></p>\n");
                    }
                    if (DataUpdated)
                    {
                        writer.Append("<p align=\"left\"><b><font color=blue>" + AppLogic.GetString("admin.editCreditCard.Updated", SkinID, LocaleSetting) + "</font></b></p>\n");
                    }

                    writer.Append("<script type='text/javascript' src='Scripts/tabs.js'></script>");



                    if (Editing)
                    {
                        writer.Append("<p align=\"left\"><b>" + String.Format(AppLogic.GetString("admin.editshippingzone.EditingShippingZone", SkinID, LocaleSetting),DB.RSFieldByLocale(rs, "Name", LocaleSetting),DB.RSFieldInt(rs, "ShippingZoneID").ToString()) + "</p></b>\n");
                    }
                    else
                    {
                        writer.Append("<div style=\"height:17;padding-top:3px;\" class=\"tablenormal\">" + AppLogic.GetString("admin.editshippingzone.AddNewShippingZone", SkinID, LocaleSetting) + ":</div>\n");
                    }

                    writer.Append("<script type=\"text/javascript\">\n");

            

                    writer.Append("</script>\n");

                    writer.Append("<p align=\"left\">" + AppLogic.GetString("admin.editshippingzone.ZoneInfo", SkinID, LocaleSetting) + "</p>\n");
                    writer.Append("<form action=\"" + AppLogic.AdminLinkUrl("editshippingzone.aspx") + "?ShippingZoneID=" + ShippingZoneID.ToString() + "&edit=" + Editing.ToString() + "\" Method=\"post\" id=\"ShippingZoneForm\" name=\"ShippingZoneForm\" onsubmit=\"return (validateForm(this) && ShippingZoneForm_Validator(this))\" onReset=\"return confirm('" + AppLogic.GetString("admin.common.ResetAllFieldsPrompt", SkinID, LocaleSetting) + "');\">\n");
                    writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                    writer.Append("<input type=\"hidden\" name=\"hfAddressCountry\" id=\"hfAddressCountry\" value=\"\">\n");
                    writer.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\">\n");
                    writer.Append("              <tr valign=\"middle\">\n");
                    writer.Append("                <td width=\"100%\" colspan=\"2\" align=\"left\">\n");
                    writer.Append("                </td>\n");
                    writer.Append("              </tr>\n");
                    writer.Append("              <tr valign=\"middle\">\n");
                    writer.Append("                <td width=\"25%\" align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.common.Name", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                    writer.Append("                <td align=\"left\" valign=\"top\">\n");
                    if (usCountryExist > 0)
                    {
                        string nameHTML = string.Empty;
                        if (Editing)
                        {
                            nameHTML = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
                        }
                        else
                        {
                            nameHTML = AppLogic.FormLocaleXml("Name");
                        }


                        writer.Append(AppLogic.GetLocaleEntryFields(nameHTML, "Name", false, true, true, AppLogic.GetString("admin.editshippingzone.ZoneName", SkinID, LocaleSetting), 100, 30, 0, 0, false));
                    }
                    else
                    {
                        writer.Append("<input type=\"text\" disabled size=\"30px\" value=\"" + DB.RSFieldByLocale(rs, "Name", LocaleSetting) + "\" />\n");
                    }
                    writer.Append("                	</td>\n");
                    writer.Append("              </tr>\n");

                    string sDisabled = "disabled";
                    bool USexist = false;

                    writer.Append("              <tr valign=\"middle\">\n");
                    writer.Append("                <td width=\"25%\" align=\"right\" valign=\"top\">*" + AppLogic.GetString("admin.common.Country", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                    writer.Append("                <td align=\"left\" valign=\"top\">\n");
                    writer.Append("                  <select name=\"AddressCountry\" id=\"AddressCountry\" size=\"1\" " + sDisabled + ">");

                    using (SqlConnection dbconn2 = DB.dbConn())
                    {
                        dbconn2.Open();
                        using (IDataReader reader = DB.GetRS("select * from country  with (NOLOCK)  where Published = 1 order by DisplayOrder,Name", dbconn2))
                        {
                            while (reader.Read())
                            {
                                if (DB.RSField(reader, "TwoLetterISOCode").Equals("US", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    writer.Append("      <option value=\"" + DB.RSFieldInt(reader, "CountryID") + "\" selected >" + DB.RSField(reader, "Name") + "</option>");
                                    USexist = true;
                                }
                            }
                        }
                    }

                    writer.Append("        </select><span>&nbsp;Shipping Zones are supported only in the U.S.</span>");
                    writer.Append("                	</td>\n");
                    writer.Append("              </tr>\n");


                    writer.Append("              <tr valign=\"middle\">\n");
                    writer.Append("                <td width=\"25%\" align=\"right\" valign=\"top\">*" + AppLogic.GetString("admin.editshippingzone.ZipCodes", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                    writer.Append("                <td align=\"left\" valign=\"top\">\n");
                    writer.Append(AppLogic.GetString("admin.editshippingzone.EnterTarget", SkinID, LocaleSetting) + "<br/>");
                    string zipCodes = string.Empty;
                    if (Editing)
                    {
                        zipCodes = DB.RSField(rs, "ZipCodes");
                    }
                    else
                    {
                        zipCodes = CommonLogic.FormCanBeDangerousContent("ZipCodes");
                    }
                    writer.Append("                	<textarea id=\"ZipCodes\"" + CommonLogic.IIF(usCountryExist > 0, "", sDisabled) + " name=\"ZipCodes\" cols=\"" + AppLogic.AppConfig("Admin_TextareaWidth") + "\"" + CommonLogic.IIF(USexist, "", "disabled") + " rows=\"" + AppLogic.AppConfig("Admin_TextareaHeightSmall") + "\">" + Server.HtmlEncode(zipCodes) + "</textarea>\n");
                    writer.Append("                	</td>\n");
                    writer.Append("              </tr>\n");

                    writer.Append("<tr>\n");
                    writer.Append("<td></td><td align=\"left\" valign=\"top\"><br/>\n");
                    if (Editing)
                    {
                        writer.Append("<input class=\"normalButtons\" type=\"submit\"" + CommonLogic.IIF(usCountryExist > 0, "", sDisabled) + " value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"submit\">\n");
                    }
                    else
                    {
                        writer.Append("<input class=\"normalButtons\" type=\"submit\"" + CommonLogic.IIF(usCountryExist > 0, "", sDisabled) + " value=\"" + AppLogic.GetString("admin.common.AddNew", SkinID, LocaleSetting) + "\" name=\"submit\" onClick=\"ShippingZoneForm_Validator(this.Form);\">\n");
                    }
                    writer.Append("        </td>\n");
                    writer.Append("      </tr>\n");
                    writer.Append("  </table>\n");
                    writer.Append("</form>\n");

                }
            }
            ltContent.Text = writer.ToString();
        }

    }
}
