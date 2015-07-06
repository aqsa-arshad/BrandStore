// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;
using System.Text;
using System.Globalization;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{

    public partial class Admin_entityProductBulkDisplayOrder : System.Web.UI.Page
    {

        int EntityID;
        String EntityName;
        EntitySpecs m_EntitySpecs;
        EntityHelper Helper;
        new int SkinID = 1;
        private Customer cust;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            EntityID = CommonLogic.QueryStringUSInt("EntityID"); ;
            EntityName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
            m_EntitySpecs = EntityDefinitions.LookupSpecs(EntityName);
            Helper = new EntityHelper(m_EntitySpecs, 0);
         
            if (EntityID == 0 || EntityName.Length == 0)
            {
                ltBody.Text = "Invalid Parameters";
                return;
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

            LoadBody();
        }

        protected void LoadBody()
        {
            StringBuilder tmpS = new StringBuilder(4096);
            tmpS.Append("<div style=\"width: 100%; border-top: solid 1px #d2d2d2; padding-top: 3px; margin-top: 5px;\">");

            String sql = "select ~.*,DisplayOrder from ~   with (NOLOCK)  left outer join ~^  with (NOLOCK)  on ~.~id=~^.~id where ~^.^id=" + EntityID.ToString() + " and deleted=0 ";
            sql += " and ~.~ID in (select distinct ~id from ~^   with (NOLOCK)  where ^id=" + EntityID.ToString() + ")";
            sql += " order by DisplayOrder,Name";

            sql = sql.Replace("^", m_EntitySpecs.m_EntityName).Replace("~", m_EntitySpecs.m_ObjectName);

            tmpS.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
            tmpS.Append("  <table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\">\n");
            tmpS.Append("    <tr>\n");
            tmpS.Append("      <td colspan=\"2\" align=\"left\"></td>\n");
            tmpS.Append("      <td align=\"center\"><input type=\"submit\" value=\"Order Update\" name=\"Submit\" class=\"normalButtons\"></td>\n");
            tmpS.Append("    </tr>\n");
            tmpS.Append("    <tr class=\"table-header\">\n");
            tmpS.Append("      <td><b>ID</b></td>\n");
            tmpS.Append("      <td><b>" + m_EntitySpecs.m_ObjectName + "</b></td>\n");
            tmpS.Append("      <td align=\"center\"><b>Display Order</b></td>\n");
            tmpS.Append("    </tr>\n");

            int counter = 0;
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(sql, conn))
                {
                    while (rs.Read())
                    {
                        int ThisID = DB.RSFieldInt(rs, m_EntitySpecs.m_ObjectName + "ID");
                        if (counter % 2 == 0)
                        {
                            tmpS.Append("    <tr class=\"table-row2\">\n");
                        }
                        else
                        {
                            tmpS.Append("    <tr class=\"table-alternatingrow2\">\n");
                        }
                        tmpS.Append("      <td >" + ThisID.ToString() + "</td>\n");
                        tmpS.Append("      <td >");
                        String Image1URL = AppLogic.LookupImage(m_EntitySpecs.m_ObjectName, ThisID, "icon", SkinID, cust.LocaleSetting);
                        bool showlinks = m_EntitySpecs.m_ObjectName != "Product";
                        if (showlinks)
                            tmpS.Append("<a target=\"entityBody\" href=\"EntityEdit" + m_EntitySpecs.m_ObjectName + "s.aspx?iden=" + ThisID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityID.ToString() + "\">");
                        tmpS.Append("<img src=\"" + Image1URL + "\" height=\"25\" border=\"0\" align=\"absmiddle\">");
                        if (showlinks)
                            tmpS.Append("</a>\n");
                        tmpS.Append("&nbsp;");
                        if (showlinks)
                            tmpS.Append("<a target=\"entityBody\" href=\"EntityEdit" + m_EntitySpecs.m_ObjectName + "s.aspx?iden=" + ThisID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityID.ToString() + "\">");
                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", cust.LocaleSetting));
                        if (showlinks)
                            tmpS.Append("</a>");
                        tmpS.Append("</td>\n");
                        tmpS.Append("      <td align=\"center\"><input size=2 class=\"default\" type=\"text\" name=\"DisplayOrder_" + ThisID.ToString() + "\" value=\"" + CommonLogic.IIF(DB.RSFieldInt(rs, "DisplayOrder") == 0, "1", (DB.RSFieldInt(rs, "DisplayOrder")).ToString()) + "\"></td>\n");
                        tmpS.Append("    </tr>\n");
                        counter++;
                    }
                }
            }

            tmpS.Append("    <tr>\n");
            tmpS.Append("      <td colspan=\"2\" align=\"left\"></td>\n");
            tmpS.Append("      <td align=\"center\"><input type=\"submit\" value=\"Order Update\" name=\"Submit\" class=\"normalButtons\"></td>\n");
            tmpS.Append("    </tr>\n");
            tmpS.Append("  </table>\n");

            tmpS.Append("</div>");
            ltBody.Text = tmpS.ToString();
        }
    }
}
