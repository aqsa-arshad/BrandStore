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
using System.Web;
using System.IO;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for editskinpreview
    /// </summary>
    public partial class editskinpreview : AdminPageBase
    {

        String SearchFor;
        String GroupName;
        String BeginsWith;
        int SkinPreviewID;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");


            SearchFor = CommonLogic.QueryStringCanBeDangerousContent("SearchFor");
            GroupName = CommonLogic.QueryStringCanBeDangerousContent("GroupName");
            BeginsWith = CommonLogic.QueryStringCanBeDangerousContent("BeginsWith");
            SkinPreviewID = 0;

            if (CommonLogic.QueryStringCanBeDangerousContent("SkinPreviewID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("SkinPreviewID") != "0")
            {
                Editing = true;
                SkinPreviewID = Localization.ParseUSInt(CommonLogic.QueryStringCanBeDangerousContent("SkinPreviewID"));
            }
            else
            {
                Editing = false;
            }

            if (CommonLogic.FormBool("IsSubmit"))
            {
                StringBuilder sql = new StringBuilder(2500);
                if (!Editing)
                {
                    // ok to add them:
                    String NewGUID = DB.GetNewGUID();
                    sql.Append("insert into SkinPreview(SkinPreviewGUID,Name,GroupName,SkinID) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Name")) + ",");
                    sql.Append(DB.SQuote(CommonLogic.FormCanBeDangerousContent("GroupName")) + ",");
                    sql.Append(CommonLogic.FormUSInt("SkinID").ToString());
                    sql.Append(")");
                    DB.ExecuteSQL(sql.ToString());

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select SkinPreviewID from SkinPreview   with (NOLOCK)  where SkinPreviewGUID=" + DB.SQuote(NewGUID), dbconn))
                        {
                            rs.Read();
                            SkinPreviewID = DB.RSFieldInt(rs, "SkinPreviewID");
                            Editing = true;
                        }
                    }
                    DataUpdated = true;
                }
                else
                {
                    // ok to update:
                    sql.Append("update SkinPreview set ");
                    sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXml("Name")) + ",");
                    sql.Append("GroupName=" + DB.SQuote(CommonLogic.FormCanBeDangerousContent("GroupName")) + ",");
                    sql.Append("SkinID=" + CommonLogic.FormUSInt("SkinID").ToString());
                    sql.Append(" where SkinPreviewID=" + SkinPreviewID.ToString());
                    DB.ExecuteSQL(sql.ToString());
                    DataUpdated = true;
                    Editing = true;
                }

                // handle image uploaded:
                String FN = SkinPreviewID.ToString();
                try
                {
                    String Image1 = String.Empty;
                    HttpPostedFile Image1File = Request.Files["Image1"];
                    if (Image1File.ContentLength != 0)
                    {
                        // delete any current image file first
                        try
                        {
                            System.IO.File.Delete(AppLogic.GetImagePath("SkinPreviews", "icon", true) + FN + ".jpg");
                            System.IO.File.Delete(AppLogic.GetImagePath("SkinPreviews", "icon", true) + FN + ".gif");
                            System.IO.File.Delete(AppLogic.GetImagePath("SkinPreviews", "icon", true) + FN + ".png");
                        }
                        catch
                        { }

                        String s = Image1File.ContentType;
                        switch (Image1File.ContentType)
                        {
                            case "image/gif":
                                Image1 = AppLogic.GetImagePath("SkinPreviews", "icon", true) + FN + ".gif";
                                Image1File.SaveAs(Image1);
                                break;
                            case "image/x-png":
                                Image1 = AppLogic.GetImagePath("SkinPreviews", "icon", true) + FN + ".png";
                                Image1File.SaveAs(Image1);
                                break;
                            case "image/jpg":
                            case "image/jpeg":
                            case "image/pjpeg":
                                Image1 = AppLogic.GetImagePath("SkinPreviews", "icon", true) + FN + ".jpg";
                                Image1File.SaveAs(Image1);
                                break;
                        }
                    }

                    String Image2 = String.Empty;
                    HttpPostedFile Image2File = Request.Files["Image2"];
                    if (Image2File.ContentLength != 0)
                    {
                        // delete any current image file first
                        try
                        {
                            System.IO.File.Delete(AppLogic.GetImagePath("SkinPreviews", "medium", true) + FN + ".jpg");
                            System.IO.File.Delete(AppLogic.GetImagePath("SkinPreviews", "medium", true) + FN + ".gif");
                            System.IO.File.Delete(AppLogic.GetImagePath("SkinPreviews", "medium", true) + FN + ".png");
                        }
                        catch
                        { }

                        String s = Image2File.ContentType;
                        switch (Image2File.ContentType)
                        {
                            case "image/gif":
                                Image2 = AppLogic.GetImagePath("SkinPreviews", "medium", true) + FN + ".gif";
                                Image2File.SaveAs(Image2);
                                break;
                            case "image/x-png":
                                Image2 = AppLogic.GetImagePath("SkinPreviews", "medium", true) + FN + ".png";
                                Image2File.SaveAs(Image2);
                                break;
                            case "image/jpg":
                            case "image/jpeg":
                            case "image/pjpeg":
                                Image2 = AppLogic.GetImagePath("SkinPreviews", "medium", true) + FN + ".jpg";
                                Image2File.SaveAs(Image2);
                                break;
                        }
                    }

                }
                catch (Exception ex)
                {
                    ErrorMsg = CommonLogic.GetExceptionDetail(ex, "<br/>");
                }
            }
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("skinpreview.aspx") + "?GroupName=" + Server.UrlEncode(GroupName) + "&beginsWith=" + Server.UrlEncode(BeginsWith) + "&searchfor=" + Server.UrlEncode(SearchFor) + "\">" + AppLogic.GetString("admin.menu.SkinPreviews", SkinID, LocaleSetting) + "</a> - " + AppLogic.GetString("admin.editskinpreview.AddEditSkinPreview", SkinID, LocaleSetting);
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from SkinPreview   with (NOLOCK)  where SkinPreviewID=" + SkinPreviewID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        Editing = true;
                    }

                    if (AppLogic.NumLocaleSettingsInstalled() > 1)
                    {
                        writer.Append("<script type='text/javascript' src='Scripts/tabs.js'></script>");
                    }

                    if (ErrorMsg.Length != 0)
                    {
                        writer.Append("<p align=\"left\"><b><font color=red>" + ErrorMsg + "</font></b></p>\n");
                    }
                    if (DataUpdated)
                    {
                        writer.Append("<p align=\"left\"><b><font color=blue>" + AppLogic.GetString("admin.editCreditCard.Updated", SkinID, LocaleSetting) + "</font></b></p>\n");
                    }


                    if (ErrorMsg.Length == 0)
                    {

                        if (Editing)
                        {
                            writer.Append("<p align=\"left\"><b>" + String.Format(AppLogic.GetString("admin.editskinpreview.EditingSkinPreview", SkinID, LocaleSetting),DB.RSField(rs, "Name"),DB.RSFieldInt(rs, "SkinPreviewID").ToString(),DB.RSFieldInt(rs, "SkinID").ToString()) + "</b></p>\n");
                        }
                        else
                        {
                            writer.Append("<div style=\"padding-top:5px;height:19px;\" class=\"tablenormal\">" + AppLogic.GetString("admin.editskinpreview.AddNewSkinPreview", SkinID, LocaleSetting) + ":</div>\n");
                        }

                        writer.Append("<script type=\"text/javascript\">\n");
                        writer.Append("function Form_Validator(theForm)\n");
                        writer.Append("{\n");
                        writer.Append("submitonce(theForm);\n");
                        writer.Append("return (true);\n");
                        writer.Append("}\n");
                        writer.Append("</script>\n");

                        writer.Append("<p>" + AppLogic.GetString("admin.editskinpreview.SkinPreviewInfo", SkinID, LocaleSetting) + "</p>\n");
                        writer.Append("<form enctype=\"multipart/form-data\" action=\"" + AppLogic.AdminLinkUrl("editSkinPreview.aspx") + "?GroupName=" + Server.UrlEncode(GroupName) + "&beginsWith=" + Server.UrlEncode(BeginsWith) + "&searchfor=" + Server.UrlEncode(SearchFor) + "&SkinPreviewID=" + SkinPreviewID.ToString() + "&edit=" + Editing.ToString() + "\" method=\"post\" id=\"Form1\" name=\"Form1\" onsubmit=\"return (validateForm(this) && Form_Validator(this))\" onReset=\"return confirm('" + AppLogic.GetString("admin.common.ResetAllFieldsPrompt", SkinID, LocaleSetting) + "');\">\n");
                        writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                        writer.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\">\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"100%\" colspan=\"2\" align=\"left\">\n");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"25%\" align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.editskinpreview.SkinID", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("                	<input maxLength=\"10\" size=\"10\" name=\"SkinID\" value=\"" + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldInt(rs, "SkinID") != 0, DB.RSFieldInt(rs, "SkinID").ToString(), ""), "") + "\"> " + AppLogic.GetString("admin.editskinpreview.SpecifySkin", SkinID, LocaleSetting) + "\n");
                        writer.Append("                	<input type=\"hidden\" name=\"SkinID_vldt\" value=\"[req][number][invalidalert=" + AppLogic.GetString("admin.common.ValidIntegerNumberPrompt", SkinID, LocaleSetting) + "]\">\n");
                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"25%\" align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.editskinpreview.SkinPreviewName", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        
                        writer.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "Name"), "Name", false, true, true, AppLogic.GetString("admin.editskinpreview.EnterSkinPreview", SkinID, LocaleSetting), 100, 30, 0, 0, false));
                        
                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"25%\" align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.editskinpreview.GroupName", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("                	<input maxLength=\"100\" size=\"50\" name=\"GroupName\" value=\"" + CommonLogic.IIF(Editing, Server.HtmlEncode(DB.RSField(rs, "GroupName")), "") + "\">\n");
                        writer.Append("                	<input type=\"hidden\" name=\"GroupName_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("admin.editskinpreview.EnterSkinPreviewGroup", SkinID, LocaleSetting) + "]\">\n");
                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("  <tr>\n");
                        writer.Append("    <td valign=\"top\" align=\"right\">" + AppLogic.GetString("admin.common.Icon", SkinID, LocaleSetting) + ":\n");
                        writer.Append("</td>\n");
                        writer.Append("    <td valign=\"top\" align=\"left\">");
                        writer.Append("    <input type=\"file\" name=\"Image1\" size=\"50\" value=\"" + CommonLogic.IIF(Editing, "", "") + "\">\n");
                        String Image1URL = AppLogic.LookupImage("SkinPreviews", SkinPreviewID, "icon", SkinID, LocaleSetting);
                        String clicklink = String.Empty;
                        if (Image1URL.Length != 0)
                        {
                            clicklink= "<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image1URL + "','Pic1');\">Click here</a>";
                            if (Image1URL.IndexOf("nopicture") == -1)
                            {
                                writer.Append(String.Format(AppLogic.GetString("admin.editgallery.DeleteImage", SkinID, LocaleSetting),clicklink) + "<br/>\n");
                            }
                            writer.Append("<br/><img id=\"Pic1\" name=\"Pic1\" border=\"0\" src=\"" + Image1URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\">\n");
                        }
                        writer.Append("</td>\n");
                        writer.Append(" </tr>\n");

                        writer.Append("  <tr>\n");
                        writer.Append("    <td valign=\"top\" align=\"right\">" + AppLogic.GetString("admin.common.MediumPic", SkinID, LocaleSetting) + ":\n");
                        writer.Append("</td>\n");
                        writer.Append("    <td valign=\"top\" align=\"left\">");
                        writer.Append("    <input type=\"file\" name=\"Image2\" size=\"50\" value=\"" + CommonLogic.IIF(Editing, "", "") + "\">\n");
                        String Image2URL = AppLogic.LookupImage("SkinPreviews", SkinPreviewID, "medium", SkinID, LocaleSetting);
                        if (Image2URL.Length != 0)
                        {
                            clicklink = "<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image2URL + "','Pic2');\">Click here</a>";
                            if (Image2URL.IndexOf("nopicture") == -1)
                            {
                                writer.Append(String.Format(AppLogic.GetString("admin.editgallery.DeleteImage", SkinID, LocaleSetting),clicklink) + "<br/>\n");
                            }
                            writer.Append("<br/><img id=\"Pic2\" name=\"Pic2\" border=\"0\" src=\"" + Image2URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\">\n");
                        }
                        writer.Append("</td>\n");
                        writer.Append(" </tr>\n");

                        writer.Append("<tr>\n");
                        writer.Append("<td></td><td align=\"left\" valign=\"top\"><br/>\n");
                        if (Editing)
                        {
                            writer.Append("<input class=\"normalButtons\" type=\"submit\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"submit\">\n");
                            writer.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input class=\"normalButtons\" type=\"reset\" value=\"" + AppLogic.GetString("admin.common.Reset", SkinID, LocaleSetting) + "\" name=\"reset\">\n");
                        }
                        else
                        {
                            writer.Append("<input class=\"normalButtons\" type=\"submit\" value=\"" + AppLogic.GetString("admin.common.AddNew", SkinID, LocaleSetting) + "\" name=\"submit\">\n");
                        }
                        writer.Append("        </td>\n");
                        writer.Append("      </tr>\n");
                        writer.Append("  </table>\n");
                        writer.Append("</form>\n");

                        writer.Append("<script type=\"text/javascript\">\n");

                        writer.Append("function DeleteImage(imgurl,name)\n");
                        writer.Append("{\n");
                        writer.Append("window.open('" + AppLogic.AdminLinkUrl("deleteimage.aspx") + "?imgurl=' + imgurl + '&FormImageName=' + name,\"Admin_ML\",\"height=250,width=440,top=10,left=20,status=no,toolbar=no,menubar=no,scrollbars=yes,location=no\")\n");
                        writer.Append("}\n");

                        writer.Append("</SCRIPT>\n");

                    }
                }
            }
            ltContent.Text = writer.ToString();
        }

    }
}
