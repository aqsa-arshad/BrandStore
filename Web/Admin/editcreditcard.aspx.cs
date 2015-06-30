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
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for editcreditcard
    /// </summary>
    public partial class editcreditcard : AdminPageBase
    {

        int CardTypeID;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            CardTypeID = 0;

            if (CommonLogic.QueryStringCanBeDangerousContent("CardTypeID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("CardTypeID") != "0")
            {
                Editing = true;
                CardTypeID = Localization.ParseUSInt(CommonLogic.QueryStringCanBeDangerousContent("CardTypeID"));
            }
            else
            {
                Editing = false;
            }

            if (CommonLogic.FormBool("IsSubmit"))
            {
                String redirectlink = "<a href=\"javascript:history.back(-1);\">go back</a>";
                if (Editing)
                {
                    // see if this card type already exists:
                    int N = DB.GetSqlN("select count(CardType) as N from CreditCardType   with (NOLOCK)  where CardTypeID<>" + CardTypeID.ToString() + " and upper(CardType)=" + DB.SQuote(CommonLogic.FormCanBeDangerousContent("CardType").ToUpperInvariant()));
                    if (N != 0)
                    {
                        ErrorMsg = "<p><b><font color=red>" + AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting) + "<br/><br/></font><blockquote>" + String.Format(AppLogic.GetString("admin.editCreditCard.ExistingCard", SkinID, LocaleSetting),redirectlink) + ".</b></blockquote></p>";
                    }
                }
                else
                {
                    // see if this cardtype is already there:
                    int N = DB.GetSqlN("select count(CardType) as N from CreditCardType   with (NOLOCK)  where upper(CardType)=" + DB.SQuote(CommonLogic.FormCanBeDangerousContent("CardType").ToUpperInvariant()));
                    if (N != 0)
                    {
                        ErrorMsg = "<p><b><font color=red>" + AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting) + "<br/><br/></font><blockquote>" + String.Format(AppLogic.GetString("admin.editCreditCard.ExistingCard", SkinID, LocaleSetting), redirectlink) + ".</b></blockquote></p>";
                    }
                }

                if (ErrorMsg.Length == 0)
                {
                    StringBuilder sql = new StringBuilder(2500);
                    if (!Editing)
                    {
                        // ok to add them:
                        String NewGUID = DB.GetNewGUID();
                        sql.Append("insert into CreditCardType(CardTypeGUID,CardType) values(");
                        sql.Append(DB.SQuote(NewGUID) + ",");
                        sql.Append(DB.SQuote(CommonLogic.Left(CommonLogic.FormCanBeDangerousContent("CardType"), 100)));
                        sql.Append(")");
                        DB.ExecuteSQL(sql.ToString());

                        using (SqlConnection dbconn = DB.dbConn())
                        {
                            dbconn.Open();
                            using (IDataReader rs = DB.GetRS("select CardTypeID from CreditCardType   with (NOLOCK)  where CardTypeGUID=" + DB.SQuote(NewGUID), dbconn))
                            {
                                rs.Read();
                                CardTypeID = DB.RSFieldInt(rs, "CardTypeID");
                                Editing = true;
                            }
                        }
                        DataUpdated = true;
                    }
                    else
                    {
                        // ok to update:
                        sql.Append("update CreditCardType set ");
                        sql.Append("CardType=" + DB.SQuote(CommonLogic.Left(CommonLogic.FormCanBeDangerousContent("CardType"), 100)));
                        sql.Append(" where CardTypeID=" + CardTypeID.ToString());
                        DB.ExecuteSQL(sql.ToString());
                        DataUpdated = true;
                        Editing = true;
                    }
                }

            }
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("creditcards.aspx") + "\">Credit Cards</a> - Manage Credit Card Types";
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from CreditCardType   with (NOLOCK)  where CardTypeID=" + CardTypeID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        rs.Close();
                        Editing = true;
                    }


                    if (ErrorMsg.Length != 0)
                    {
                        writer.Append("<p><b><font color=red>" + ErrorMsg + "</font></b></p>\n");
                    }
                    if (DataUpdated)
                    {
                        writer.Append("<p align=\"left\"><b><font color=blue>" + AppLogic.GetString("admin.editCreditCard.Updated", SkinID, LocaleSetting) + "</font></b></p>\n");
                    }


                    if (ErrorMsg.Length == 0)
                    {

                        if (Editing)
                        {
                            writer.Append("<b>" + String.Format(AppLogic.GetString("admin.editCreditCard.Editing", SkinID, LocaleSetting), DB.RSField(rs, "CardType"),DB.RSFieldInt(rs, "CardTypeID").ToString())+"<br/><br/></b>\n");
                        }
                        else
                        {
                            writer.Append("<b>" + AppLogic.GetString("admin.editCreditCard.AddingCreditType", SkinID, LocaleSetting) + "<br/><br/></b>\n");
                        }

                        writer.Append("<script type=\"text/javascript\">\n");
                        writer.Append("function Form_Validator(theForm)\n");
                        writer.Append("{\n");
                        writer.Append("submitonce(theForm);\n");
                        writer.Append("return (true);\n");
                        writer.Append("}\n");
                        writer.Append("</script>\n");

                        writer.Append("<p>" + AppLogic.GetString("admin.editCreditCard.EnterCCTypeInfo", SkinID, LocaleSetting) + "</p>\n");
                        writer.Append("<form enctype=\"multipart/form-data\" action=\"" + AppLogic.AdminLinkUrl("editcreditcard.aspx") + "?CardTypeID=" + CardTypeID.ToString() + "&edit=" + Editing.ToString() + "\" method=\"post\" id=\"Form1\" name=\"Form1\" onsubmit=\"return (validateForm(this) && Form_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");
                        writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                        writer.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\">\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"100%\" colspan=\"2\" align=\"left\">\n");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"25%\" align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.editCreditCard.CCType", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("                	<input maxLength=\"100\" size=\"30\" name=\"CardType\" value=\"" + CommonLogic.IIF(Editing, Server.HtmlEncode(DB.RSField(rs, "CardType")), "") + "\">\n");
                        writer.Append("                	<input type=\"hidden\" name=\"CardType_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("admin.editCreditCard.EnterCCType", SkinID, LocaleSetting) + "]\">\n");
                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("<tr>\n");
                        writer.Append("<td></td><td align=\"left\" valign=\"top\"><br/>\n");
                        if (Editing)
                        {
                            writer.Append("<input type=\"submit\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"submit\">\n");
                            writer.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type=\"reset\" class=\"CPButton\" value=\"" + AppLogic.GetString("admin.common.Reset", SkinID, LocaleSetting) + "\" name=\"reset\">\n");
                        }
                        else
                        {
                            writer.Append("<input type=\"submit\" value=\"" + AppLogic.GetString("admin.common.AddNew", SkinID, LocaleSetting) + "\" name=\"submit\">\n");
                        }
                        writer.Append("        </td>\n");
                        writer.Append("      </tr>\n");
                        writer.Append("  </table>\n");
                        writer.Append("</form>\n");
                    }
                }
            }
            ltContent.Text = writer.ToString();
        }

    }
}
