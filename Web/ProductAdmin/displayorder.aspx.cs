// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text;
using System.Data;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for displayorder.
    /// </summary>
    public partial class displayorder : AdminPageBase
    {

        int EntityID;
        String EntityName;
        EntitySpecs m_EntitySpecs;
        EntityHelper Helper;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");


            EntityID = CommonLogic.QueryStringUSInt("EntityID"); ;
            EntityName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
            m_EntitySpecs = EntityDefinitions.LookupSpecs(EntityName);
            Helper = AppLogic.LookupHelper(EntityHelpers, m_EntitySpecs.m_EntityName);

            if (EntityID == 0 || EntityName.Length == 0)
            {
                Response.Redirect(AppLogic.AdminLinkUrl("default.aspx"));
            }

            if (CommonLogic.FormBool("IsSubmit"))
            {
                if (EntityID != 0)
                {
                    DB.ExecuteSQL(String.Format("delete from {0}{1} where {2}ID={3}", m_EntitySpecs.m_ObjectName, m_EntitySpecs.m_EntityName, m_EntitySpecs.m_EntityName, EntityID.ToString()));
                }

                for (int i = 0; i <= Request.Form.Count - 1; i++)
                {
                    if (Request.Form.Keys[i].IndexOf("DisplayOrder_") != -1)
                    {
                        String[] keys = Request.Form.Keys[i].Split('_');
                        int ObjectID = Localization.ParseUSInt(keys[1]);
                        int DispOrd = 1;
                        try
                        {
                            DispOrd = Localization.ParseUSInt(Request.Form[Request.Form.Keys[i]]);
                        }
                        catch { }
                        DB.ExecuteSQL(String.Format("insert into {0}{1}({2}ID,{3}ID,DisplayOrder) values({4},{5},{6})", m_EntitySpecs.m_ObjectName, m_EntitySpecs.m_EntityName, m_EntitySpecs.m_EntityName, m_EntitySpecs.m_ObjectName, EntityID.ToString(), ObjectID.ToString(), DispOrd.ToString()));
                    }
                }
            }
            SectionTitle = AppLogic.GetString("admin.sectiontitle.displayorder", SkinID, LocaleSetting);
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            String sql = "select ~.*,DisplayOrder from ~   with (NOLOCK)  left outer join ~^  with (NOLOCK)  on ~.~id=~^.~id where ~^.^id=" + EntityID.ToString() + " and deleted=0 ";
            sql += " and ~.~ID in (select distinct ~id from ~^   with (NOLOCK)  where ^id=" + EntityID.ToString() + ")";
            sql += " order by DisplayOrder,Name";

            sql = sql.Replace("^", m_EntitySpecs.m_EntityName).Replace("~", m_EntitySpecs.m_ObjectName);

            String prompt = String.Format(AppLogic.GetString("admin.displayorder.SettingDisplayOrder", SkinID, LocaleSetting), m_EntitySpecs.m_ObjectName, m_EntitySpecs.m_EntityName) + " " + Helper.GetEntityName(EntityID, LocaleSetting);
            

            writer.Append("<p><b>" + prompt + "</b></p>");

            writer.Append("<form id=\"Form1\" name=\"Form1\" method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("displayorder.aspx") + "?entityid=" + EntityID.ToString() + "&entityname=" + m_EntitySpecs.m_EntityName + "\">\n");
            writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
            writer.Append("  <table border=\"0\" cellpadding=\"2\" border=\"0\" cellspacing=\"1\" width=\"100%\">\n");
            writer.Append("    <tr bgcolor=\"" + AppLogic.AppConfig("LightCellColor") + "\">\n");
            writer.Append("      <td><b>ID</b></td>\n");
            writer.Append("      <td><b>" + m_EntitySpecs.m_ObjectName + "</b></td>\n");
            writer.Append("      <td align=\"center\"><b>" + AppLogic.GetString("admin.common.DisplayOrder", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("    </tr>\n");

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        int ThisID = DB.RSFieldInt(rs, m_EntitySpecs.m_ObjectName + "ID");
                        writer.Append("    <tr bgcolor=\"" + AppLogic.AppConfig("LightCellColor") + "\">\n");
                        writer.Append("      <td >" + ThisID.ToString() + "</td>\n");
                        writer.Append("      <td >");
                        String Image1URL = AppLogic.LookupImage(m_EntitySpecs.m_ObjectName, ThisID, "icon", SkinID, LocaleSetting);
                        writer.Append("<a href=\"" + AppLogic.AdminLinkUrl("edit" + m_EntitySpecs.m_ObjectName + ".aspx") +"?" + m_EntitySpecs.m_ObjectName + "id=" + ThisID.ToString() + "\">");
                        writer.Append("<img src=\"" + Image1URL + "\" height=\"25\" border=\"0\" align=\"absmiddle\">");
                        writer.Append("</a>&nbsp;\n");
                        writer.Append("<a href=\"" + AppLogic.AdminLinkUrl("edit" + m_EntitySpecs.m_ObjectName + ".aspx") + "?" + m_EntitySpecs.m_ObjectName + "id=" + ThisID.ToString() + "\">");
                        writer.Append(DB.RSFieldByLocale(rs, "Name", LocaleSetting));
                        writer.Append("</a>");

                        writer.Append("</a>");
                        writer.Append("</td>\n");
                        writer.Append("      <td align=\"center\"><input size=2 type=\"text\" name=\"DisplayOrder_" + ThisID.ToString() + "\" value=\"" + CommonLogic.IIF(DB.RSFieldInt(rs, "DisplayOrder") == 0, "1", DB.RSField(rs, "DisplayOrder")) + "\"></td>\n");
                        writer.Append("    </tr>\n");
                    }
                }
            }

            writer.Append("    <tr>\n");
            writer.Append("      <td colspan=\"2\" align=\"left\"></td>\n");
            writer.Append("      <td align=\"center\" bgcolor=\"" + AppLogic.AppConfig("LightCellColor") + "\"><input type=\"submit\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"Submit\"></td>\n");
            writer.Append("    </tr>\n");
            writer.Append("  </table>\n");
            writer.Append("</form>\n");

            writer.Append("</center></b>\n");
            ltContent.Text = writer.ToString();
        }

    }
}
