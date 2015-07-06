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
    /// Summary description for bulkeditinventory.
    /// </summary>
    public partial class entityBulkInventory : AdminPageBase
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
                ltBody.Text = AppLogic.GetString("admin.common.InvalidParameters", SkinID, LocaleSetting);
                return;
            }

            LoadBody();
        }

        protected void LoadBody()
        {
            StringBuilder tmpS = new StringBuilder(4096);

            tmpS.Append("<div style=\"width: 100%; border-top: solid 1px #d2d2d2; padding-top: 3px; margin-top: 5px;\">");

			Int32 mappingCount = DB.GetSqlN("select count(*) as N from Product" + this.m_EntitySpecs.m_EntityName + " where " + m_EntitySpecs.m_EntityName + "Id = " + this.EntityID.ToString());

            ProductCollection products = new ProductCollection(EntityName, EntityID);
            products.PageSize = 0;
            products.PageNum = 1;
            products.PublishedOnly = false;
            products.ReturnAllVariants = true;
			DataSet dsProducts = new DataSet();
			if (mappingCount > 0)
				dsProducts = products.LoadFromDB();

            int NumProducts = products.NumProducts;
			if (NumProducts > 1000)
			{
				tmpS.Append("<p><b>" + AppLogic.GetString("admin.common.ImportExcession", SkinID, LocaleSetting) + "</b></p>");
			}
			else if (NumProducts > 0)
            {
                tmpS.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                tmpS.Append("<table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\">\n");
                //tmpS.Append("<tr><td colspan=\"5\" align=\"right\"><input type=\"submit\" value=\"" + AppLogic.GetString("admin.entityBulkInventory.InventoryUpdate", SkinID, LocaleSetting) + "\" class=\"normalButtons\" name=\"Submit\"></td></tr>");
                tmpS.Append("<tr class=\"table-header\">\n");
                tmpS.Append("<td><b>ProductID</b></td>\n");
                tmpS.Append("<td><b>VariantID</b></td>\n");
                tmpS.Append("<td><b>Product Name</b></td>\n");
                tmpS.Append("<td><b>Variant Name</b></td>\n");
                tmpS.Append("<td><b>Inventory</b></td>\n");
                tmpS.Append("</tr>\n");

                int rowcount = dsProducts.Tables[0].Rows.Count;

                for (int i = 0; i < rowcount; i++)
                {
                    DataRow row = dsProducts.Tables[0].Rows[i];

                    int ThisProductID = DB.RowFieldInt(row, "ProductID");
                    int ThisVariantID = DB.RowFieldInt(row, "VariantID");

                    if (i % 2 == 0)
                    {
                        tmpS.Append("<tr class=\"table-row2\">\n");
                    }
                    else
                    {
                        tmpS.Append("<tr class=\"table-alternatingrow2\">\n");
                    }
                    tmpS.Append("<td align=\"left\" valign=\"middle\">");
                    tmpS.Append(ThisProductID.ToString());
                    tmpS.Append("</td>");
                    tmpS.Append("<td align=\"left\" valign=\"middle\">");
                    tmpS.Append(ThisVariantID.ToString());
                    tmpS.Append("</td>");
                    tmpS.Append("<td align=\"left\" valign=\"middle\">");
                    bool showlinks = false;
                    if (showlinks)
                        tmpS.Append("<a target=\"entityBody\" href=\"" + AppLogic.AdminLinkUrl("entityeditproducts.aspx") + "?iden=" + ThisProductID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityID.ToString() + "\">");
                    tmpS.Append(DB.RowFieldByLocale(row, "Name", cust.LocaleSetting));
                    if (showlinks)
                        tmpS.Append("</a>");
                    tmpS.Append("</td>\n");
                    tmpS.Append("<td align=\"left\" valign=\"middle\">");
                    if (showlinks)
                        tmpS.Append("<a target=\"entityBody\" href=\"" + AppLogic.AdminLinkUrl("entityeditproductvariant.aspx") + "?iden=" + ThisProductID.ToString() + "&variantid=" + ThisVariantID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityID.ToString() + "\">");
                    tmpS.Append(DB.RowFieldByLocale(row, "VariantName", cust.LocaleSetting));
                    if (showlinks)
                        tmpS.Append("</a>"); 
                    tmpS.Append("</td>\n");
                    tmpS.Append("<td align=\"left\" valign=\"middle\">");
                    String s = AppLogic.GetInventoryTable(ThisProductID, ThisVariantID, true, SkinID, false, true);
                    tmpS.Append(s);
                    tmpS.Append("</td>\n");
                    tmpS.Append("</tr>\n");
                    
                }
                //tmpS.Append("<tr><td colspan=\"5\" align=\"right\"><input type=\"submit\" value=\"" + AppLogic.GetString("admin.entityBulkInventory.InventoryUpdate", SkinID, LocaleSetting) + "\" class=\"normalButtons\" name=\"Submit\"></td></tr>");
                tmpS.Append("</table>\n");
                
            }
            else
            {
                tmpS.Append("<p><b>" + AppLogic.GetString("admin.common.NoProductsFound", SkinID, LocaleSetting) + "</b></p>");
            }
            dsProducts.Dispose();
            products.Dispose();

            tmpS.Append("</div>");
            ltBody.Text = tmpS.ToString();
        }

        protected void BtnInventoryUpdate_click(object sender, EventArgs e)
        {
            for (int i = 0; i <= Request.Form.Count - 1; i++)
            {
                String FieldName = Request.Form.Keys[i];
                if (FieldName.IndexOf("|") != -1 && ((FieldName.StartsWith("simple", StringComparison.InvariantCultureIgnoreCase) || FieldName.StartsWith("sizecolor", StringComparison.InvariantCultureIgnoreCase))))
                {
                    String KeyVal = CommonLogic.FormCanBeDangerousContent(FieldName);
                    // this field should be processed
                    String[] FieldNameSplit = FieldName.Split('|');
                    String InventoryType = FieldNameSplit[0].ToLower(CultureInfo.InvariantCulture);
                    int TheProductID = Localization.ParseUSInt(FieldNameSplit[1]);
                    int TheVariantID = Localization.ParseUSInt(FieldNameSplit[2]);
                    String Size = FieldNameSplit[3];
                    String Color = FieldNameSplit[4];
                    int inputVal = CommonLogic.FormUSInt(FieldName);
                    if (InventoryType == "simple")
                    {
                        DB.ExecuteSQL("update ProductVariant set Inventory=" + inputVal.ToString() + " where VariantID=" + TheVariantID.ToString());
                    }
                    else
                    {
                        String sql = "select count(*) as N from Inventory  with (NOLOCK)  where VariantID=" + TheVariantID.ToString() + " and lower([size])=" + DB.SQuote(AppLogic.CleanSizeColorOption(Size).ToLowerInvariant()) + " and lower(color)=" + DB.SQuote(AppLogic.CleanSizeColorOption(Color).ToLowerInvariant());
                        if (DB.GetSqlN(sql) == 0)
                        {
                            sql = "insert into Inventory(InventoryGUID,VariantID,[Size],Color,Quan) values(" + DB.SQuote(DB.GetNewGUID()) + "," + TheVariantID.ToString() + "," + DB.SQuote(AppLogic.CleanSizeColorOption(Size)) + "," + DB.SQuote(AppLogic.CleanSizeColorOption(Color)) + "," + inputVal.ToString() + ")";
                            DB.ExecuteSQL(sql);
                        }
                        else
                        {
                            sql = "update Inventory set Quan=" + inputVal.ToString() + " where VariantID=" + TheVariantID.ToString() + " and lower([size])=" + DB.SQuote(AppLogic.CleanSizeColorOption(Size).ToLowerInvariant()) + " and lower(color)=" + DB.SQuote(AppLogic.CleanSizeColorOption(Color).ToLowerInvariant());
                            DB.ExecuteSQL(sql);
                        }
                    }
                }
            }
            DB.ExecuteSQL("Update Inventory set Quan=0 where Quan<0"); // safety check
            DB.ExecuteSQL("Update ProductVariant set Inventory=0 where Inventory<0"); // safety check
            LoadBody();
        }
    }
}
