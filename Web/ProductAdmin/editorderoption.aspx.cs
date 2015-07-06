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
using System.IO;
using System.Web;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for editorderoption
	/// </summary>
    public partial class editorderoption : AdminPageBase
	{
		
		int OrderOptionID;
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache"); 
            

			OrderOptionID = 0;
			
			if(CommonLogic.QueryStringCanBeDangerousContent("OrderOptionID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("OrderOptionID") != "0") 
			{
				Editing = true;
				OrderOptionID = Localization.ParseUSInt(CommonLogic.QueryStringCanBeDangerousContent("OrderOptionID"));
			} 
			else 
			{
				Editing = false;
			}

						
			if(CommonLogic.FormBool("IsSubmit"))
			{
				StringBuilder sql = new StringBuilder(2500);
				decimal Cost = System.Decimal.Zero;
				if(CommonLogic.FormCanBeDangerousContent("Cost").Length != 0)
				{
					Cost = CommonLogic.FormUSDecimal("Cost");
				}
				if(!Editing)
				{
					// ok to add them:
					String NewGUID = DB.GetNewGUID();
					sql.Append("insert into OrderOption(OrderOptionGUID,Name,Description,DefaultIsChecked,Cost) values(");
					sql.Append(DB.SQuote(NewGUID) + ",");
					sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Name")) + ",");
					if(AppLogic.FormLocaleXml("Description").Length != 0)
					{
						sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Description")) + ",");
					}
					else
					{
						sql.Append("NULL,");
					}
                    sql.Append(CommonLogic.FormUSInt("DefaultIsChecked").ToString() + ",");
                    sql.Append(CommonLogic.IIF(Cost != System.Decimal.Zero, Localization.DecimalStringForDB(Cost), "0.0"));
					sql.Append(")");
					DB.ExecuteSQL(sql.ToString());

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select OrderOptionID from OrderOption   with (NOLOCK)  where OrderOptionGUID=" + DB.SQuote(NewGUID), dbconn))
                        {
                            rs.Read();
                            OrderOptionID = DB.RSFieldInt(rs, "OrderOptionID");
                            Editing = true;
                        }
                    }
					DataUpdated = true;
				}
				else
				{
					// ok to update:
					sql.Append("update OrderOption set ");
					sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXml("Name")) + ",");
					if(AppLogic.FormLocaleXml("Description").Length != 0)
					{
						sql.Append("Description=" + DB.SQuote(AppLogic.FormLocaleXml("Description")) + ",");
					}
					else
					{
						sql.Append("Description=NULL,");
					}
                    sql.Append("DefaultIsChecked=" + CommonLogic.FormUSInt("DefaultIsChecked").ToString() + ",");
                    sql.Append("Cost=" + CommonLogic.IIF(Cost != System.Decimal.Zero, Localization.DecimalStringForDB(Cost), "0.0"));
					sql.Append(" where OrderOptionID=" + OrderOptionID.ToString());
					DB.ExecuteSQL(sql.ToString());
					DataUpdated = true;
					Editing = true;
				}
				// handle image uploaded:
				try
				{
					String Image1 = String.Empty;
					HttpPostedFile Image1File = Request.Files["Image1"];
					if(Image1File.ContentLength != 0)
					{
						// delete any current image file first
						try
						{
							System.IO.File.Delete(AppLogic.GetImagePath("OrderOption","icon",true) + OrderOptionID.ToString() + ".jpg");
							System.IO.File.Delete(AppLogic.GetImagePath("OrderOption","icon",true) + OrderOptionID.ToString() + ".gif");
							System.IO.File.Delete(AppLogic.GetImagePath("OrderOption","icon",true) + OrderOptionID.ToString() + ".png");
						}
						catch
						{}

						String s = Image1File.ContentType;
						switch(Image1File.ContentType)
						{
							case "image/gif":
								Image1 = AppLogic.GetImagePath("OrderOption","icon",true) + OrderOptionID.ToString() + ".gif";
								Image1File.SaveAs(Image1);
								break;
							case "image/x-png":
								Image1 = AppLogic.GetImagePath("OrderOption","icon",true) + OrderOptionID.ToString() + ".png";
								Image1File.SaveAs(Image1);
								break;
                            case "image/jpg":
                            case "image/jpeg":
							case "image/pjpeg":
								Image1 = AppLogic.GetImagePath("OrderOption","icon",true) + OrderOptionID.ToString() + ".jpg";
								Image1File.SaveAs(Image1);
								break;
						}
					}
				}
				catch(Exception ex)
				{
					ErrorMsg = CommonLogic.GetExceptionDetail(ex,"<br/>");
				}
			}
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("orderoptions.aspx") + "\">" + AppLogic.GetString("admin.menu.OrderOptions", SkinID, LocaleSetting) + "</a> - " + String.Format(AppLogic.GetString("admin.editorderoption.ManageOrderOptions", SkinID, LocaleSetting),CommonLogic.IIF(DataUpdated, " (Updated)", ""));
            RenderHtml();
		}

		private void RenderHtml()
		{
            StringBuilder writer = new StringBuilder();

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from OrderOption   with (NOLOCK)  where OrderOptionID=" + OrderOptionID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        Editing = true;
                    }

                    if (ErrorMsg.Length != 0)
                    {
                        writer.Append("<p><b><font color=red>" + ErrorMsg + "</font></b></p>\n");
                    }


                    if (ErrorMsg.Length == 0)
                    {

                        if (Editing)
                        {
                            writer.Append("<p><b>" + String.Format(AppLogic.GetString("admin.editorderoption.EditingOrderOption", SkinID, LocaleSetting),DB.RSFieldByLocale(rs, "Name", LocaleSetting),DB.RSFieldInt(rs, "OrderOptionID").ToString()) + "<br/><br/></b>\n");
                        }
                        else
                        {
                            writer.Append("<div style=\"height:17;padding-top:3px;\" class=\"tablenormal\">" + AppLogic.GetString("admin.editorderoption.AddingNewOrderOption", SkinID, LocaleSetting) + "</div><br/></b>\n");
                        }

                        writer.Append("<script type=\"text/javascript\">\n");
                        writer.Append("function Form_Validator(theForm)\n");
                        writer.Append("{\n");
                        writer.Append("submitonce(theForm);\n");
                        writer.Append("return (true);\n");
                        writer.Append("}\n");
                        writer.Append("</script>\n");

                        if (AppLogic.NumLocaleSettingsInstalled() > 1)
                        {
                            writer.Append("<script type='text/javascript' src='Scripts/tabs.js'></script>");
                        }

                        writer.Append("<p>" + AppLogic.GetString("admin.editorderoption.OrderOptionInfo", SkinID, LocaleSetting) + "</p>\n");
                        writer.Append("<form enctype=\"multipart/form-data\" action=\"" + AppLogic.AdminLinkUrl("editorderoption.aspx") + "?OrderOptionID=" + OrderOptionID.ToString() + "&edit=" + Editing.ToString() + "\" method=\"post\" id=\"Form1\" name=\"Form1\" onsubmit=\"return (validateForm(this) && Form_Validator(this))\" onReset=\"return confirm('" + AppLogic.GetString("admin.common.ResetAllFieldsPrompt", SkinID, LocaleSetting) + "');\">\n");
                        writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                        writer.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\">\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"100%\" colspan=\"2\" align=\"left\">\n");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"25%\" align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.editorderoption.OrderOptionName", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "Name"), "Name", false, true, true, AppLogic.GetString("admin.editorderoption.EnterOrderOption", SkinID, LocaleSetting), 100, 30, 0, 0, false));
                        
                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.common.Description", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "Description"), "Description", true, true, false, "", 0, 0, AppLogic.AppConfigUSInt("Admin_TextareaHeight"), AppLogic.AppConfigUSInt("Admin_TextareaWidth"), true));
                        
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.editorderoption.DefaultIsChecked", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\">\n");
                        writer.Append(AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"DefaultIsChecked\" value=\"1\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "DefaultIsChecked"), " checked ", ""), " checked ") + ">\n");
                        writer.Append(AppLogic.GetString("admin.common.No", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"DefaultIsChecked\" value=\"0\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "DefaultIsChecked"), "", " checked "), "") + ">\n");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"25%\" align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.common.Cost", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("                	<input maxLength=\"10\" size=\"10\" name=\"Cost\" value=\"" + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldDecimal(rs, "Cost") != System.Decimal.Zero, Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "Cost")), ""), "") + "\"> " + AppLogic.GetString("admin.common.InFormat", SkinID, LocaleSetting) + "\n");
                        writer.Append("                	<input type=\"hidden\" name=\"Cost_vldt\" value=\"[number][invalidalert=" + AppLogic.GetString("admin.common.ValidDollarAmountPrompt", SkinID, LocaleSetting) + "]\">\n");
                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("  <tr>\n");
                        writer.Append("    <td valign=\"top\" align=\"right\">Icon:\n");
                        writer.Append("</td>\n");
                        writer.Append("    <td valign=\"top\" align=\"left\">");
                        writer.Append("    <input type=\"file\" name=\"Image1\" size=\"30\" value=\"" + CommonLogic.IIF(Editing, "", "") + "\">\n");
                        String Image1URL = AppLogic.LookupImage("OrderOption", OrderOptionID, "icon", SkinID, LocaleSetting);
                        if (Image1URL.Length != 0)
                        {
                            if (Image1URL.IndexOf("nopicture") == -1)
                            {
                                String clicklink = "<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image1URL + "','CatPic');\">Click here</a>";
                                writer.Append(String.Format(AppLogic.GetString("admin.editgallery.DeleteImage", SkinID, LocaleSetting),clicklink) + "<br/>\n");
                            }
                            writer.Append("<br/><img id=\"CatPic\" name=\"CatPic\" border=\"0\" src=\"" + Image1URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\">\n");
                        }
                        writer.Append("</td>\n");
                        writer.Append(" </tr>\n");

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

			writer.Append("<script type=\"text/javascript\">\n");
			writer.Append("function DeleteImage(imgurl,name)\n");
			writer.Append("{\n");
            writer.Append("window.open('" + AppLogic.AdminLinkUrl("deleteimage.aspx") + "?imgurl=' + imgurl + '&FormImageName=' + name,\"AspDotNetStorefrontAdmin_ML\",\"height=250,width=440,top=10,left=20,status=no,toolbar=no,menubar=no,scrollbars=yes,location=no\")\n");
			writer.Append("}\n");
			writer.Append("</SCRIPT>\n");
            ltContent.Text = writer.ToString();
		}

	}
}
