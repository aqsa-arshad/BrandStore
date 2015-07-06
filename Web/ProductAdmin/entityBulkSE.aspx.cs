// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Globalization;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for bulkeditsearch.
    /// </summary>
    public partial class entityBulkSE : AdminPageBase
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
            Helper = new EntityHelper(m_EntitySpecs, 0);
      
            if (EntityID == 0 || EntityName.Length == 0)
            {
                Response.Redirect(AppLogic.AdminLinkUrl("default.aspx"));
            }

            if (CommonLogic.FormCanBeDangerousContent("IsSubmit").Equals("TRUE", StringComparison.InvariantCultureIgnoreCase))
            {
                for (int i = 0; i <= Request.Form.Count - 1; i++)
                {
                    String FieldName = Request.Form.Keys[i];
                    if (FieldName.StartsWith("setitle", StringComparison.InvariantCultureIgnoreCase))
                    {
                        String[] FieldNameSplit = FieldName.Split('_');
                        int TheProductID = Localization.ParseUSInt(FieldNameSplit[1]);
                        int TheVariantID = Localization.ParseUSInt(FieldNameSplit[2]);
                        string inputVal = AppLogic.FormLocaleXml("SETitle", CommonLogic.FormCanBeDangerousContent(FieldName), ThisCustomer.ThisCustomerSession.Session("entityUserLocale"), "Product", TheProductID);
                        if (inputVal.Length == 0)
                        {
                            DB.ExecuteSQL("update Product set SETitle=NULL where ProductID=" + TheProductID.ToString());
                        }
                        else
                        {
                            DB.ExecuteSQL("update Product set SETitle=" + DB.SQuote(inputVal) + " where ProductID=" + TheProductID.ToString());
                        }
                    }
                    if (FieldName.StartsWith("sekeywords", StringComparison.InvariantCultureIgnoreCase))
                    {
                        String[] FieldNameSplit = FieldName.Split('_');
                        int TheProductID = Localization.ParseUSInt(FieldNameSplit[1]);
                        int TheVariantID = Localization.ParseUSInt(FieldNameSplit[2]);
                        string inputVal = AppLogic.FormLocaleXml("SEKeywords", CommonLogic.FormCanBeDangerousContent(FieldName), ThisCustomer.ThisCustomerSession.Session("entityUserLocale"), "Product", TheProductID);
                        if (inputVal.Length == 0)
                        {
                            DB.ExecuteSQL("update Product set SEKeywords=NULL where ProductID=" + TheProductID.ToString());
                        }
                        else
                        {
                            DB.ExecuteSQL("update Product set SEKeywords=" + DB.SQuote(inputVal) + " where ProductID=" + TheProductID.ToString());
                        }
                    }
                    if (FieldName.StartsWith("sedescription", StringComparison.InvariantCultureIgnoreCase))
                    {
                        String[] FieldNameSplit = FieldName.Split('_');
                        int TheProductID = Localization.ParseUSInt(FieldNameSplit[1]);
                        int TheVariantID = Localization.ParseUSInt(FieldNameSplit[2]);
                        string inputVal = AppLogic.FormLocaleXml("SEDescription", CommonLogic.FormCanBeDangerousContent(FieldName), ThisCustomer.ThisCustomerSession.Session("entityUserLocale"), "Product", TheProductID);
                        if (inputVal.Length == 0)
                        {
                            DB.ExecuteSQL("update Product set SEDescription=NULL where ProductID=" + TheProductID.ToString());
                        }
                        else
                        {
                            DB.ExecuteSQL("update Product set SEDescription=" + DB.SQuote(inputVal) + " where ProductID=" + TheProductID.ToString());
                        }
                    }
                    if (FieldName.StartsWith("senoscript", StringComparison.InvariantCultureIgnoreCase))
                    {
                        String[] FieldNameSplit = FieldName.Split('_');
                        int TheProductID = Localization.ParseUSInt(FieldNameSplit[1]);
                        int TheVariantID = Localization.ParseUSInt(FieldNameSplit[2]);
                        string inputVal = AppLogic.FormLocaleXml("SENoScript", CommonLogic.FormCanBeDangerousContent(FieldName), ThisCustomer.ThisCustomerSession.Session("entityUserLocale"), "Product", TheProductID);
                        if (inputVal.Length == 0)
                        {
                            DB.ExecuteSQL("update Product set SENoScript=NULL where ProductID=" + TheProductID.ToString());
                        }
                        else
                        {
                            DB.ExecuteSQL("update Product set SENoScript=" + DB.SQuote(inputVal) + " where ProductID=" + TheProductID.ToString());
                        }
                    }
                   
                }
            }

            LoadBody();
        }

        protected void LoadBody()
        {
			Int32 mappingCount = DB.GetSqlN("select count(*) as N from Product" + this.m_EntitySpecs.m_EntityName + " where " + m_EntitySpecs.m_EntityName + "Id = " + this.EntityID.ToString());

            ProductCollection products = new ProductCollection(m_EntitySpecs.m_EntityName, EntityID);
            products.PageSize = 0;
            products.PageNum = 1;
            products.PublishedOnly = false;
            products.ReturnAllVariants = false;

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

                ltBody.Text += ("<form id=\"Form1\" name=\"Form1\" method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("entityBulkSE.aspx") + "?entityid=" + EntityID.ToString() + "&entityname=" + m_EntitySpecs.m_EntityName + "\" onsubmit=\"alert('" + AppLogic.GetString("admin.entityBulkSE.Patient", SkinID, LocaleSetting) + "');return (validateForm(document.forms[0]) && Form_Validator(document.forms[0]))\" onReset=\"return confirm('" + AppLogic.GetString("admin.common.ResetAllFieldsPrompt", SkinID, LocaleSetting) + "');\">\n");
                ltBody.Text += ("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                ltBody.Text += ("<table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\">\n");
                ltBody.Text += ("<tr><td colspan=\"5\" align=\"right\"><input type=\"submit\" value=\"" + AppLogic.GetString("admin.entityBulkSE.SearchEngineUpdate", SkinID, LocaleSetting) + "\" name=\"Submit\" class=\"normalButtons\"></td></tr>\n");
                ltBody.Text += ("<tr class=\"table-header\">\n");
                ltBody.Text += ("<td><b>" + AppLogic.GetString("admin.common.ProductID", SkinID, LocaleSetting) + "</b></td>\n");
                ltBody.Text += ("<td><b>" + AppLogic.GetString("admin.common.VariantID", SkinID, LocaleSetting) + "</b></td>\n");
                ltBody.Text += ("<td><b>" + AppLogic.GetString("admin.common.ProductName", SkinID, LocaleSetting) + "</b></td>\n");
                ltBody.Text += ("<td><b>" + AppLogic.GetString("admin.common.VariantName", SkinID, LocaleSetting) + "</b></td>\n");
                ltBody.Text += ("<td align=\"left\"><b>" + AppLogic.GetString("admin.common.ProductFields", SkinID, LocaleSetting) + "</b></td>\n");
               
                ltBody.Text += ("</tr>\n");
                int LastProductID = 0;



                int rowcount = dsProducts.Tables[0].Rows.Count;

                for (int i = 0; i < rowcount; i++)
                {
                    DataRow row = dsProducts.Tables[0].Rows[i];

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
                    ltBody.Text += ("<td align=\"left\" valign=\"top\">");
                    ltBody.Text += (ThisProductID.ToString());
                    ltBody.Text += ("</td>");
                    ltBody.Text += ("<td align=\"left\" valign=\"top\">");
                    ltBody.Text += (ThisVariantID.ToString());
                    ltBody.Text += ("</td>");
                    ltBody.Text += ("<td align=\"left\" valign=\"top\">");
                    bool showlinks = false;
                    if (showlinks)
                        ltBody.Text += ("<a href=\"" + AppLogic.AdminLinkUrl("entityeditproducts.aspx") + "?iden=" + ThisProductID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityID.ToString() + "\">");
                    ltBody.Text += (DB.RowFieldByLocale(row, "Name", LocaleSetting));
                    if (showlinks)
                        ltBody.Text += ("</a>");
                    ltBody.Text += ("</td>\n");
                    ltBody.Text += ("<td align=\"left\" valign=\"top\">");
                    if (showlinks)
                        ltBody.Text += ("<a href=\"" + AppLogic.AdminLinkUrl("entityeditproductvariant.aspx") + "?iden=" + ThisProductID.ToString() + "&variantid=" + ThisVariantID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityID.ToString() + "\">");
                    ltBody.Text += (DB.RowFieldByLocale(row, "VariantName", LocaleSetting));
                    if (showlinks)
                        ltBody.Text += ("</a>");
                    ltBody.Text += ("</td>\n");
                    ltBody.Text += ("<td align=\"left\" valign=\"top\">");
                    ltBody.Text += ("<div align=\"left\">");
                    ltBody.Text += ("<b>" + AppLogic.GetString("admin.topic.setitle", SkinID, LocaleSetting) + "</b><br/>");
                    ltBody.Text += ("<input maxLength=\"100\" name=\"SETitle_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "\" id=\"SETitle_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "\" value=\"" + XmlCommon.GetLocaleEntry(DB.RowField(row, "SETitle"), ThisCustomer.ThisCustomerSession.Session("entityUserLocale"), false) + "\" class=\"singleLongest\" /><br/>");
                    ltBody.Text += ("<b>" + AppLogic.GetString("admin.topic.sekeywords", SkinID, LocaleSetting) + "</b><br/>");
                    ltBody.Text += ("<input maxLength=\"255\" name=\"SEKeywords_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "\" id=\"SEKeywords" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "\" value=\"" + XmlCommon.GetLocaleEntry(DB.RowField(row, "SEKeywords"), ThisCustomer.ThisCustomerSession.Session("entityUserLocale"), false) + "\" class=\"singleLongest\" /><br/>");
                    ltBody.Text += ("<b>" + AppLogic.GetString("admin.topic.sedescription", SkinID, LocaleSetting) + "</b><br/>");
                    ltBody.Text += ("<input maxLength=\"255\" name=\"SEDescription_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "\" id=\"SEDescription" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "\" value=\"" + XmlCommon.GetLocaleEntry(DB.RowField(row, "SEDescription"), ThisCustomer.ThisCustomerSession.Session("entityUserLocale"), false) + "\" class=\"singleLongest\" /><br/>");
                    ltBody.Text += ("<b>" + AppLogic.GetString("admin.entityBulkSE.SearchEngineNoScript", SkinID, LocaleSetting) + ":</b><br/>");
                    ltBody.Text += ("<textarea name=\"SENoScript_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "\" id=\"SENoScript" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "\" class=\"multiLong\">" + XmlCommon.GetLocaleEntry(DB.RowField(row, "SENoScript"), ThisCustomer.ThisCustomerSession.Session("entityUserLocale"), false) + "</textarea><br/>");
                   
                    ltBody.Text += ("</div>");
                    ltBody.Text += ("</td>\n");
                    ltBody.Text += ("</tr>\n");
                    LastProductID = ThisProductID;

                }
                ltBody.Text += ("<tr><td colspan=\"5\" align=\"right\"><input type=\"submit\" value=\"" + AppLogic.GetString("admin.entityBulkSE.SearchEngineUpdate", SkinID, LocaleSetting) + "\" name=\"Submit\" class=\"normalButtons\"></td></tr>\n");                                
                ltBody.Text += ("</table>\n");
               
                ltBody.Text += ("</form>\n");
            }
            else
            {
                ltBody.Text += ("<p><b>" + AppLogic.GetString("admin.common.NoProductsFound", SkinID, LocaleSetting) + "</b></p>");
            }
            dsProducts.Dispose();
            products.Dispose();
        }
    }
}
