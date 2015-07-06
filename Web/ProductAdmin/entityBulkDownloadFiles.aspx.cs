// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Text;
using System.Globalization;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for bulkeditdownloadfiles.
    /// </summary>
    public partial class entityBulkDownloadFiles : AdminPageBase
    {
        int EntityID;
        String EntityName;
        EntitySpecs m_EntitySpecs;
        EntityHelper Helper;
        private Customer cust;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            
            EntityID = CommonLogic.QueryStringUSInt("EntityID");
            if (EntityID < 1)
                EntityID = CommonLogic.FormNativeInt("EntityID");
            EntityName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
            if (String.IsNullOrEmpty(EntityName))
                EntityName = CommonLogic.FormCanBeDangerousContent("EntityName");
            m_EntitySpecs = EntityDefinitions.LookupSpecs(EntityName);
            Helper = new EntityHelper(m_EntitySpecs, 0);
          
            if (EntityID == 0 || EntityName.Length == 0)
            {
                ltBody.Text = AppLogic.GetString("admin.common.InvalidParameters", SkinID, LocaleSetting);
                return;
            }

            if (CommonLogic.FormCanBeDangerousContent("IsSubmit").Equals("TRUE",StringComparison.InvariantCultureIgnoreCase))
            {
                ProductCollection products = new ProductCollection(m_EntitySpecs.m_EntityName, EntityID);
                products.PageSize = 0;
                products.PageNum = 1;
                products.PublishedOnly = false;
                products.ReturnAllVariants = true;
                DataSet dsProducts = products.LoadFromDB();
                int NumProducts = products.NumProducts;
                foreach (DataRow row in dsProducts.Tables[0].Rows)
                {
                    if (DB.RowFieldBool(row, "IsDownload"))
                    {
                        int ThisProductID = DB.RowFieldInt(row, "ProductID");
                        int ThisVariantID = DB.RowFieldInt(row, "VariantID");
                        StringBuilder sql = new StringBuilder(1024);
                        sql.Append("update productvariant set ");
                        String DLoc = CommonLogic.FormCanBeDangerousContent("DownloadLocation_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString());
                        if (DLoc.StartsWith("/"))
                        {
                            DLoc = DLoc.Substring(1, DLoc.Length - 1); // remove leading / char!
                        }
                        sql.Append("DownloadLocation=" + DB.SQuote(DLoc));
                        sql.Append(" where VariantID=" + ThisVariantID.ToString());
                        DB.ExecuteSQL(sql.ToString());
                    }
                }
                dsProducts.Dispose();
            }

            LoadBody();
        }

        protected void LoadBody()
        {
            ltBody.Text = "<form method=\"post\" action=\"entityBulkDownloadFiles.aspx?entityname="+EntityName+"&EntityID="+EntityID+"\">";
            ltBody.Text += "<div style=\"width: 100%; border-top: solid 1px #d2d2d2; padding-top: 3px; margin-top: 5px;\">";
            ltBody.Text += ("<input type=\"hidden\" name=\"EntityName\" value=\""+EntityName+"\">\n");
            ltBody.Text += ("<input type=\"hidden\" name=\"EntityID\" value=\""+EntityID+"\">\n");

            ProductCollection products = new ProductCollection(m_EntitySpecs.m_EntityName, EntityID);
            products.PageSize = 0;
            products.PageNum = 1;
            products.PublishedOnly = false;
            products.ReturnAllVariants = true;

			Int32 mappingCount = DB.GetSqlN("select count(*) as N from Product" + this.m_EntitySpecs.m_EntityName + " where " + m_EntitySpecs.m_EntityName + "Id = " + this.EntityID.ToString());

			DataSet dsProducts = new DataSet();
			if (mappingCount > 0)
				dsProducts = products.LoadFromDB();

            int NumProducts = products.NumProducts;
			if (NumProducts > 1000)
			{
				ltBody.Text += ("<p><b>" + AppLogic.GetString("admin.common.ImportExcession", SkinID, LocaleSetting) + "</b></p>");
			}
			else if (NumProducts > 0)
            {
                ltBody.Text += ("<script type=\"text/javascript\">\n");
                ltBody.Text += ("function Form_Validator(theForm)\n");
                ltBody.Text += ("{\n");
                ltBody.Text += ("submitonce(theForm);\n");
                ltBody.Text += ("return (true);\n");
                ltBody.Text += ("}\n");
                ltBody.Text += ("</script>\n");

                ltBody.Text += ("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                ltBody.Text += ("<table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\">\n");
                ltBody.Text += ("<tr class=\"table-header\">\n");
                ltBody.Text += ("<td><b>" + AppLogic.GetString("admin.common.ProductID", SkinID, LocaleSetting) + "</b></td>\n");
                ltBody.Text += ("<td><b>" + AppLogic.GetString("admin.common.VariantID", SkinID, LocaleSetting) + "</b></td>\n");
                ltBody.Text += ("<td><b>" + AppLogic.GetString("admin.common.ProductName", SkinID, LocaleSetting) + "</b></td>\n");
                ltBody.Text += ("<td><b>" + AppLogic.GetString("admin.common.VariantName", SkinID, LocaleSetting) + "</b></td>\n");
                ltBody.Text += ("<td align=\"left\"><b>" + AppLogic.GetString("admin.common.DownloadFile", SkinID, LocaleSetting) + "</b></td>\n");
                ltBody.Text += ("</tr>\n");
                int LastProductID = 0;


                int rowcount = dsProducts.Tables[0].Rows.Count;

                for (int i = 0; i < rowcount; i++)
                {
                    DataRow row = dsProducts.Tables[0].Rows[i];

                    if (DB.RowFieldBool(row, "IsDownload"))
                    {
                        int ThisProductID = DB.RowFieldInt(row, "ProductID");
                        int ThisVariantID = DB.RowFieldInt(row, "VariantID");

                        if (i % 2 == 0)
                        {
                            ltBody.Text += ("<tr class=\"table-row2\">\n");
                        }
                        else
                        {
                            ltBody.Text += ("<tr class=\"table-alternatingrow2\">\n");
                        }
                        ltBody.Text += ("<td align=\"left\" valign=\"middle\">");
                        ltBody.Text += (ThisProductID.ToString());
                        ltBody.Text += ("</td>");
                        ltBody.Text += ("<td align=\"left\" valign=\"middle\">");
                        ltBody.Text += (ThisVariantID.ToString());
                        ltBody.Text += ("</td>");
                        ltBody.Text += ("<td align=\"left\" valign=\"middle\">");
                        bool showlinks = false;
                        if (showlinks)
                            ltBody.Text += ("<a target=\"entityBody\" href=\"" + AppLogic.AdminLinkUrl("entityeditproducts.aspx") + "?iden=" + ThisProductID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityID.ToString() + "\">");
                        ltBody.Text += (DB.RowFieldByLocale(row, "Name", cust.LocaleSetting));
                        if (showlinks)
                            ltBody.Text += ("</a>");
                        ltBody.Text += ("</td>\n");
                        ltBody.Text += ("<td align=\"left\" valign=\"middle\">");
                        if (showlinks)
                            ltBody.Text += ("<a target=\"entityBody\" href=\"" + AppLogic.AdminLinkUrl("entityeditproductvariant.aspx") + "?iden=" + ThisProductID.ToString() + "&variantid=" + ThisVariantID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityID.ToString() + "\">");
                        ltBody.Text += (DB.RowFieldByLocale(row, "VariantName", cust.LocaleSetting));
                        if (showlinks)
                            ltBody.Text += ("</a>");
                        ltBody.Text += ("</td>\n");
                        ltBody.Text += ("<td align=\"left\" valign=\"middle\">");
                        ltBody.Text += ("<input maxLength=\"1000\" class=\"singleNormal\" onkeypress=\"javascript:return WebForm_FireDefaultButton(event, 'btnsubmit')\" name=\"DownloadLocation_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "\" value=\"" + DB.RowField(row, "DownloadLocation") + "\">\n");
                        ltBody.Text += ("</td>\n");
                        ltBody.Text += ("</tr>\n");
                        LastProductID = ThisProductID;
                    }
                }
                if (LastProductID == 0)
                {
                    ltBody.Text += ("</table>\n");
                    ltBody.Text += ("<p align=\"left\"><b>" + AppLogic.GetString("admin.common.NoDownloadProductsFound", SkinID, LocaleSetting) + "</b></p>");
                }
                else
                {
                    ltBody.Text += ("<tr><td colspan=\"5\" align=\"right\"><input type=\"submit\" id=\"btnsubmit\" value=\"" + AppLogic.GetString("admin.entityBulkDownloadFiles.DownloadUpdate", SkinID, LocaleSetting) + "\" name=\"Submit\" class=\"normalButtons\"></td></tr>\n");
                    ltBody.Text += ("</table>\n");
                }
               
            }
            else
            {
                ltBody.Text += ("<p><b>" + AppLogic.GetString("admin.common.NoProductsFound", SkinID, LocaleSetting) + "</b></p>");
            }
            dsProducts.Dispose();
            products.Dispose();

            ltBody.Text += ("</div>");
            ltBody.Text += ("</form>");
        }

    }
}
