// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Globalization;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for bulkeditshippingcosts.
    /// </summary>
    public partial class entityBulkShipping : AdminPageBase
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

            EntityID = CommonLogic.QueryStringUSInt("EntityID"); ;
            EntityName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
            m_EntitySpecs = EntityDefinitions.LookupSpecs(EntityName);
            Helper = new EntityHelper(m_EntitySpecs, 0);
           
            if (EntityID == 0 || EntityName.Length == 0)
            {
                ltBody.Text = AppLogic.GetString("admin.common.InvalidParameters", SkinID, LocaleSetting);
                return;
            }
            if (CommonLogic.FormCanBeDangerousContent("IsSubmit").Equals("TRUE", StringComparison.InvariantCultureIgnoreCase))
            {
                for (int i = 0; i <= Request.Form.Count - 1; i++)
                {
                    String FieldName = Request.Form.Keys[i];
                    if (FieldName.StartsWith("shippingcost", StringComparison.InvariantCultureIgnoreCase) && !FieldName.EndsWith("_vldt", StringComparison.InvariantCultureIgnoreCase))
                    {
                        String[] FieldNameSplit = FieldName.Split('_');
                        int ThisVariantID = Localization.ParseUSInt(FieldNameSplit[1]);
                        int ThisShippingMethodID = Localization.ParseUSInt(FieldNameSplit[2]);
                        decimal ShippingCost = CommonLogic.FormUSDecimal(FieldName);
                        DB.ExecuteSQL("delete from ShippingByProduct where VariantID=" + ThisVariantID.ToString() + " and ShippingMethodID=" + ThisShippingMethodID.ToString());
                        DB.ExecuteSQL("insert ShippingByProduct(VariantID,ShippingMethodID,ShippingCost) values(" + ThisVariantID.ToString() + "," + ThisShippingMethodID.ToString() + "," + Localization.CurrencyStringForDBWithoutExchangeRate(ShippingCost) + ")");
                    }
                }
            }

            LoadBody();
        }

		protected void LoadBody()
		{
			StringBuilder tmpS = new StringBuilder(4096);
			tmpS.Append("<div style=\"width: 100%; border-top: solid 1px #d2d2d2; padding-top: 3px; margin-top: 5px;\">");

			using (ProductCollection products = new ProductCollection(m_EntitySpecs.m_EntityName, EntityID))
			{
				Int32 mappingCount = DB.GetSqlN("select count(*) as N from Product" + this.m_EntitySpecs.m_EntityName + " where " + m_EntitySpecs.m_EntityName + "Id = " + this.EntityID.ToString());

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
					using (DataTable dtShippingMethods = new DataTable())
					{
						using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
						{
							con.Open();
							using (IDataReader rs = DB.GetRS("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 order by DisplayOrder", con))
							{
								dtShippingMethods.Load(rs);
							}
						}


						tmpS.Append("<script type=\"text/javascript\">\n");
						tmpS.Append("function Form_Validator(theForm)\n");
						tmpS.Append("{\n");
						tmpS.Append("submitonce(theForm);\n");
						tmpS.Append("return (true);\n");
						tmpS.Append("}\n");
						tmpS.Append("</script>\n");

						tmpS.Append("<form id=\"Form1\" name=\"Form1\" method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("entityBulkShipping.aspx") + "?entityid=" + EntityID.ToString() + "&entityname=" + m_EntitySpecs.m_EntityName + "\" onsubmit=\"alert('" + AppLogic.GetString("admin.entityBulkShipping.Patient", SkinID, LocaleSetting) + "');return (validateForm(document.forms[0]) && Form_Validator(document.forms[0]))\" onReset=\"return confirm('" + AppLogic.GetString("admin.common.ResetAllFieldsPrompt", SkinID, LocaleSetting) + "');\">\n");
						tmpS.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");

						tmpS.Append("<table border=\"0\" cellpadding=\"2\" border=\"0\" cellspacing=\"0\" width=\"100%\">\n");
						tmpS.Append("    <tr >\n");
						tmpS.Append("      <td colspan=\"8\" align=\"right\"><input type=\"submit\" value=\"" + AppLogic.GetString("admin.entityBulkShipping.ShippingUpdate", SkinID, LocaleSetting) + "\" name=\"Submit\" class=\"normalButtons\"></td>\n");
						tmpS.Append("    </tr>\n");
						tmpS.Append("<tr class=\"table-header\">\n");
						tmpS.Append("<td><b>" + AppLogic.GetString("admin.common.ProductID", SkinID, LocaleSetting) + "</b></td>\n");
						tmpS.Append("<td><b>" + AppLogic.GetString("admin.common.VariantID", SkinID, LocaleSetting) + "</b></td>\n");
						tmpS.Append("<td><b>" + AppLogic.GetString("admin.common.ProductName", SkinID, LocaleSetting) + "</b></td>\n");
						tmpS.Append("<td><b>" + AppLogic.GetString("admin.common.VariantName", SkinID, LocaleSetting) + "</b></td>\n");
						foreach (DataRow row in dtShippingMethods.Rows)
						{
							tmpS.Append("<td align=\"center\"><b>" + DB.RowFieldByLocale(row, "Name", cust.LocaleSetting) + " Cost</b><br/><small>" + AppLogic.GetString("admin.entityBulkPrices.Format", SkinID, LocaleSetting) + "</small></td>\n");
						}
						tmpS.Append("</tr>\n");
						int LastProductID = 0;
						for (int i = 0; i < dsProducts.Tables[0].Rows.Count; i++)
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

							tmpS.Append("<td align=\"left\" valign=\"top\">");
							tmpS.Append(ThisProductID.ToString());
							tmpS.Append("</td>");
							tmpS.Append("<td align=\"left\" valign=\"top\">");
							tmpS.Append(ThisVariantID.ToString());
							tmpS.Append("</td>");
							tmpS.Append("<td align=\"left\" valign=\"top\">");
							tmpS.Append("<a target=\"entityBody\" href=\"" + AppLogic.AdminLinkUrl("entityeditproducts.aspx") + "?iden=" + ThisProductID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityID.ToString() + "\">");
							tmpS.Append(DB.RowFieldByLocale(row, "Name", cust.LocaleSetting));
							tmpS.Append("</a>");
							tmpS.Append("</td>\n");
							tmpS.Append("<td align=\"left\" valign=\"top\">");
							tmpS.Append("<a target=\"entityBody\" href=\"" + AppLogic.AdminLinkUrl("entityeditproductvariant.aspx") + "?productid=" + ThisProductID.ToString() + "&variantid=" + ThisVariantID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityID.ToString() + "\">");
							tmpS.Append(DB.RowFieldByLocale(row, "VariantName", cust.LocaleSetting));
							tmpS.Append("</a>");
							tmpS.Append("</td>\n");
							foreach (DataRow row2 in dtShippingMethods.Rows)
							{
								tmpS.Append("<td align=\"center\" valign=\"top\">");
								tmpS.Append("<input class=\"default\" maxLength=\"10\" size=\"10\" name=\"ShippingCost_" + ThisVariantID.ToString() + "_" + DB.RowFieldInt(row2, "ShippingMethodID") + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(Shipping.GetVariantShippingCost(ThisVariantID, DB.RowFieldInt(row2, "ShippingMethodID"))) + "\">\n");
								tmpS.Append("<input type=\"hidden\" name=\"ShippingCost_" + ThisVariantID.ToString() + "_" + DB.RowFieldInt(row2, "ShippingMethodID") + "_vldt\" value=\"[number][invalidalert=" + AppLogic.GetString("admin.common.ValidDollarAmountPrompt", SkinID, LocaleSetting) + "]\">\n");
								tmpS.Append("</td>\n");
							}
							tmpS.Append("</tr>\n");
							LastProductID = ThisProductID;
						}
						tmpS.Append("    <tr>\n");
						tmpS.Append("      <td colspan=\"8\" align=\"right\"><input type=\"submit\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.entityBulkShipping.ShippingUpdate", SkinID, LocaleSetting) + "\" name=\"Submit\"></td>\n");
						tmpS.Append("    </tr>\n");
						tmpS.Append("</table>\n");

						tmpS.Append("</form>\n");
					}
				}
				else
				{
					tmpS.Append("<p><b>" + AppLogic.GetString("admin.common.NoProductsFound", SkinID, LocaleSetting) + "</b></p>");
				}

				tmpS.Append("</div>");
				ltBody.Text = tmpS.ToString();
			}
		}
    }
}
