// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for popup.
    /// </summary>
    public partial class popup : System.Web.UI.Page
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            String PageTitle = ScrubImageURLS(CommonLogic.QueryStringCanBeDangerousContent("Title"));
            AppLogic.CheckForScriptTag(PageTitle);
            if (PageTitle.Length == 0)
            {
                PageTitle = "Popup Window " + CommonLogic.GetRandomNumber(1, 1000000).ToString();
            }
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            Response.Write("<!DOCTYPE html><html xmlns=\"http://www.w3.org/1999/xhtml\">\n");
            Response.Write("<head>\n");
            Response.Write("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" /><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />\n");
            Response.Write("<title>" + PageTitle + "</title>\n");
            Response.Write("<link rel=\"stylesheet\" href=\"App_Themes/Skin_" + ThisCustomer.SkinID.ToString() + "/style.css\" type=\"text/css\">\n");
            Response.Write("<script type=\"text/javascript\" src=\"jscripts/formValidate.js\"></script>\n");
            Response.Write("</head>\n");


			if((CommonLogic.QueryStringCanBeDangerousContent("src").Length != 0) && (!CommonLogic.QueryStringCanBeDangerousContent("src").Contains("nopicture.gif") || !CommonLogic.QueryStringCanBeDangerousContent("src").Contains("nopictureicon.gif")))
            {
				string sSrc = CommonLogic.QueryStringCanBeDangerousContent("src");
				if(!string.IsNullOrEmpty(CommonLogic.QueryStringCanBeDangerousContent("size")))
				{
					sSrc += string.Format("&size={0}", CommonLogic.QueryStringCanBeDangerousContent("size"));
				}
				if(!string.IsNullOrEmpty(CommonLogic.QueryStringCanBeDangerousContent("imgurl")))
				{
					sSrc += string.Format("&imgurl={0}", CommonLogic.QueryStringCanBeDangerousContent("imgurl"));
				}

				// IMAGE POPUP:
                Response.Write("<body style=\"margin: 0px;\" bottommargin=\"0\" leftmargin=\"0\" marginheight=\"0\" marginwidth=\"0\" rightmargin=\"0\" topmargin=\"0\" bgcolor=\"#FFFFFF\" " + CommonLogic.IIF(AppLogic.AppConfigBool("OnBlurPopups"), "ONBLUR=\"self.close();\"", "") + " onClick=\"self.close();\" onLoad=\"self.focus()\">\n");
                Response.Write("<center>\n");
                Response.Write("<img name=\"Image1\" onClick=\"javascript:self.close();\" class=\"actionelement\" alt=\"" + AppLogic.GetString("popup.aspx.1", 1, Localization.GetDefaultLocale()) + "\" border=\"0\" src=\"" + ScrubImageURLS(Server.HtmlEncode(sSrc)) + "\" />\n");
                Response.Write("</center>\n");
            }
            else if (CommonLogic.QueryStringCanBeDangerousContent("orderoptionid").Length != 0)
            {
                // kit group info popoup:
                Response.Write("<body style=\"margin: 0px;\" bottommargin=\"0\" leftmargin=\"0\" marginheight=\"0\" marginwidth=\"0\" rightmargin=\"0\" topmargin=\"0\" bgcolor=\"#FFFFFF\" onLoad=\"self.focus()\">\n");

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("Select * from orderoption   with (NOLOCK)  where orderoptionid=" + CommonLogic.QueryStringUSInt("orderoptionid").ToString(), conn))
                    {
                        if (rs.Read())
                        {
                            Response.Write("<p align=\"left\"><b>" + DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting) + "</b>:</p>");
                            Response.Write("<p align=\"left\">" + DB.RSFieldByLocale(rs, "Description", ThisCustomer.LocaleSetting) + "</p>");
                        }
                        else
                        {
                            Response.Write("<p align=\"left\"><b><font color=red>" + AppLogic.GetString("popup.aspx.2", 1, Localization.GetDefaultLocale()) + "</font></b>:</p>");
                        }                        
                    }
                }                
            }
            else if (CommonLogic.QueryStringCanBeDangerousContent("kitgroupid").Length != 0)
            {
                // kit group info popoup:
                Response.Write("<body style=\"margin: 0px;\" bottommargin=\"0\" leftmargin=\"0\" marginheight=\"0\" marginwidth=\"0\" rightmargin=\"0\" topmargin=\"0\" bgcolor=\"#FFFFFF\" onLoad=\"self.focus()\">\n");

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("Select * from kitgroup   with (NOLOCK)  where kitgroupid=" + CommonLogic.QueryStringUSInt("kitgroupid").ToString(), conn))
                    {
                        if (rs.Read())
                        {
                            Response.Write("<p align=\"left\"><b>" + DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting) + "</b>:</p>");
                            Response.Write("<p align=\"left\">" + DB.RSFieldByLocale(rs, "Description", ThisCustomer.LocaleSetting) + "</p>");
                        }
                        else
                        {
                            Response.Write("<p align=\"left\"><b><font color=red>" + AppLogic.GetString("popup.aspx.3", 1, Localization.GetDefaultLocale()) + "</font></b>:</p>");
                        }                        
                    }
                }                
            }
            else if (CommonLogic.QueryStringCanBeDangerousContent("KitItemID").Length != 0)
            {
                // kit group info popoup:
                Response.Write("<body style=\"margin: 0px;\" bottommargin=\"0\" leftmargin=\"0\" marginheight=\"0\" marginwidth=\"0\" rightmargin=\"0\" topmargin=\"0\" bgcolor=\"#FFFFFF\" onLoad=\"self.focus()\">\n");
                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs3 = DB.GetRS("Select * from kititem   with (NOLOCK)  where KitItemID=" + CommonLogic.QueryStringUSInt("KitItemID").ToString(), conn))
                    {
                        if (rs3.Read())
                        {
                            Response.Write("<p align=\"left\"><b>" + DB.RSFieldByLocale(rs3, "Name", ThisCustomer.LocaleSetting) + "</b>:</p>");
                            Response.Write("<p align=\"left\">" + DB.RSFieldByLocale(rs3, "Description", ThisCustomer.LocaleSetting) + "</p>");
                        }
                        else
                        {
                            Response.Write("<p align=\"left\"><b><font color=red>" + AppLogic.GetString("popup.aspx.4", 1, Localization.GetDefaultLocale()) + "</font></b>:</p>");
                        }                        
                    }
                }                
            }
            else
            {
                // CONTENT POPUP:
                Response.Write("<body style=\"margin: 10px;\" bottommargin=\"10\" leftmargin=\"10\" marginheight=\"10\" marginwidth=\"10\" rightmargin=\"10\" topmargin=\"10\" bgcolor=\"#FFFFFF\" onLoad=\"self.focus()\">\n");

                Topic t = new Topic(CommonLogic.QueryStringCanBeDangerousContent("Topic"), ThisCustomer.LocaleSetting, ThisCustomer.SkinID, null);

                if (t.Contents.Length == 0)
                {
                    Response.Write("<img src=\"images/spacer.gif\" border=\"0\" height=\"100\" width=\"1\">\n");
                    Response.Write("<p align=\"center\"><font class=\"big\"><b>" + AppLogic.GetString("popup.aspx.5", 1, Localization.GetDefaultLocale()) + "</b></font></p>");
                }
                else
                {
                    Response.Write("\n");
                    Response.Write("<!-- READ FROM " + CommonLogic.IIF(t.FromDB, "DB", "FILE") + ": " + " -->");
                    Response.Write("\n");
                    Response.Write(t.Contents.Replace("(!SKINID!)", ThisCustomer.SkinID.ToString()));
                    Response.Write("\n");
                    Response.Write("<!-- END OF " + CommonLogic.IIF(t.FromDB, "DB", "FILE") + ": " + " -->");
                    Response.Write("\n");
                }

            }
            Response.Write("</body>\n");
            Response.Write("</html>\n");

        }
        private static String ScrubImageURLS(String url)
        {
            if (url.ToLower().Contains("javascript")
                || url.ToLower().Contains("//")
                || url.ToLower().Contains("'")
                || url.ToLower().Contains("<")
                || url.ToLower().Contains(">")
                || url.ToLower().Contains(":")
               )
                throw new ArgumentException("Dangerous value detected in src parameter(s).");

            return url;
        }
    }
}
