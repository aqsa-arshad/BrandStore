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
using System.Data.SqlClient;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for search.
    /// </summary>
    public partial class search : AdminPageBase
    {

        protected void Page_Load(Object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            Page.Form.DefaultButton = btnSubmit.UniqueID;
            Page.Form.DefaultFocus = txtSearchTerm.ClientID;

            if (!IsPostBack)
            {
                txtSearchTerm.Text = CommonLogic.QueryStringCanBeDangerousContent("SearchTerm");
            }

            SectionTitle = "Search";

            StringBuilder contents = new StringBuilder();

            EntityHelper CategoryHelper = AppLogic.LookupHelper(EntityHelpers, "Category");
            EntityHelper SectionHelper = AppLogic.LookupHelper(EntityHelpers, "Section");
            EntityHelper ManufacturerHelper = AppLogic.LookupHelper(EntityHelpers, "Manufacturer");

            String st = CommonLogic.QueryStringCanBeDangerousContent("SearchTerm").Trim();
            if (st.Length != 0)
            {
                String stlike = "%" + st + "%";
                String stquoted = DB.SQuote(stlike);


                // MATCHING CATEGORIES:
                bool anyFound = false;

                contents.Append("<table cellpadding=\"2\" cellspacing=\"0\" border=\"0\" width=\"100%\">\n");
                contents.Append("<tr><td style=\"filter:progid:DXImageTransform.Microsoft.Gradient(startColorStr='#FFFFFF', endColorStr='#6487DB', gradientType='1')\"><b>" + AppLogic.GetString("AppConfig.CategoryPromptPlural", SkinID, LocaleSetting).ToUpperInvariant() + " MATCHING: '" + st.ToUpperInvariant() + "'</b></font></td></tr>\n");

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select * from Category  with (NOLOCK)  where Category.name like " + stquoted + " and Deleted=0 order by DisplayOrder,Name", con))
                    {
                        while (rs.Read())
                        {
                            contents.Append("<tr><td>" + CategoryHelper.GetEntityBreadcrumb(DB.RSFieldInt(rs, "CategoryID"), LocaleSetting) + "</td></tr>");
                            anyFound = true;
                        }
                    }
                }

                if (!anyFound)
                {
                    contents.Append("<tr><td>No matches found</td></tr>\n");
                }
                contents.Append("<tr><td>&nbsp;</td></tr>\n");
                contents.Append("</table>\n");

                // MATCHING SECTIONS:
                anyFound = false;

                contents.Append("<table cellpadding=\"2\" cellspacing=\"0\" border=\"0\" width=\"100%\">\n");
                contents.Append("<tr><td style=\"filter:progid:DXImageTransform.Microsoft.Gradient(startColorStr='#FFFFFF', endColorStr='#6487DB', gradientType='1')\"><b>" + AppLogic.GetString("AppConfig.SectionPromptPlural", SkinID, LocaleSetting).ToUpperInvariant() + " MATCHING: '" + st.ToUpperInvariant() + "'</b></font></td></tr>\n");

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select * from [Section]  with (NOLOCK)  where Name like " + stquoted + " and Published=1 and Deleted=0 order by DisplayOrder,Name", con))
                    {
                        while (rs.Read())
                        {
                            contents.Append("<tr><td>" + SectionHelper.GetEntityBreadcrumb(DB.RSFieldInt(rs, "SectionID"), LocaleSetting) + "</td></tr>");
                            anyFound = true;
                        }
                    }
                }

                if (!anyFound)
                {
                    contents.Append("<tr><td>No matches found</td></tr>\n");
                }
                contents.Append("<tr><td>&nbsp;</td></tr>\n");
                contents.Append("</table>\n");

                // MATCHING MANUFACTURERS:
                anyFound = false;

                contents.Append("<table cellpadding=\"2\" cellspacing=\"0\" border=\"0\" width=\"100%\">\n");
                contents.Append("<tr><td style=\"filter:progid:DXImageTransform.Microsoft.Gradient(startColorStr='#FFFFFF', endColorStr='#6487DB', gradientType='1')\"><b>MANUFACTURERS MATCHING: '" + st.ToUpperInvariant() + "'</b></font></td></tr>\n");

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select * from Manufacturer  with (NOLOCK)  where Name like " + stquoted + " and Deleted=0", con))
                    {
                        while (rs.Read())
                        {
                            contents.Append("<tr><td><a href=\"newentities.aspx?entityname=manufacturer&manufacturerid=" + DB.RSFieldInt(rs, "ManufacturerID").ToString() + "\">" + CommonLogic.HighlightTerm(DB.RSFieldByLocale(rs, "Name", LocaleSetting), st) + "</a></td></tr>\n");
                            anyFound = true;
                        }
                    }
                }

                if (!anyFound)
                {
                    contents.Append("<tr><td>No matches found</td></tr>\n");
                }
                contents.Append("<tr><td>&nbsp;</td></tr>\n");
                contents.Append("</table>\n");


                // MATCHING PRODUCTS:
                ProductCollection products = new ProductCollection();
                products.PageSize = 0;
                products.PageNum = 1;
                products.SearchMatch = st;
                products.SearchDescriptionAndSummaryFields = false;
                products.PublishedOnly = false;
                DataSet dsProducts = products.LoadFromDB(true);
                int NumProducts = products.NumProducts;

                anyFound = false;
                if (NumProducts > 0)
                {
                    anyFound = true;
                    contents.Append("  <table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"1\" width=\"100%\">\n");
                    contents.Append("<tr><td colspan=\"4\" style=\"filter:progid:DXImageTransform.Microsoft.Gradient(startColorStr='#FFFFFF', endColorStr='#6487DB', gradientType='1')\"><b>PRODUCTS MATCHING: '" + st.ToUpperInvariant() + "'</b></font></td></tr>\n");
                    contents.Append("    <tr>\n");
                    contents.Append("      <td align=\"left\"><b>" + AppLogic.GetString("search.aspx.6", SkinID, LocaleSetting) + "</b></td>\n");
                    contents.Append("      <td align=\"center\"><b>" + AppLogic.GetString("search.aspx.7", SkinID, LocaleSetting) + "</b></td>\n");
                    contents.Append("      <td align=\"center\"><b>" + AppLogic.GetString("AppConfig.CategoryPromptSingular", SkinID, LocaleSetting) + "</b></td>\n");
                    contents.Append("      <td align=\"center\"><b>" + AppLogic.GetString("search.aspx.8", SkinID, LocaleSetting) + "</b></td>\n");
                    contents.Append("    </tr>\n");
                    foreach (DataRow row in dsProducts.Tables[0].Rows)
                    {
                        String url = "newentities.aspx?productid=" + DB.RowFieldInt(row, "ProductID").ToString();
                        contents.Append("<tr>");
                        contents.Append("<td valign=\"middle\" align=\"left\" >");
                        contents.Append("<a href=\"" + url + "\">" + AppLogic.MakeProperObjectName(DB.RowFieldByLocale(row, "Name", LocaleSetting), DB.RowFieldByLocale(row, "VariantName", LocaleSetting), LocaleSetting) + "</a>");
                        // QuickEdit
                        contents.Append("</td>");
                        contents.Append("<td align=\"center\">" + CommonLogic.HighlightTerm(AppLogic.MakeProperProductSKU(DB.RowField(row, "SKU"), DB.RowField(row, "SKUSuffix"), "", ""), st) + "</td>");
                        String Cats = CategoryHelper.GetObjectEntities(DB.RowFieldInt(row, "ProductID"), false);
                        if (Cats.Length != 0)
                        {
                            String[] CatIDs = Cats.Split(',');
                            contents.Append("<td align=\"center\">");
                            bool firstCat = true;
                            foreach (String s in CatIDs)
                            {
                                if (!firstCat)
                                {
                                    contents.Append(", ");
                                }
                                contents.Append("<a href=\"newentities.aspx?entityname=category&categoryid=" + s + "\"\">" + CategoryHelper.GetEntityName(Localization.ParseUSInt(s), LocaleSetting).Trim() + "</a>");
                                firstCat = false;
                            }
                            contents.Append("</td>\n");
                        }
                        else
                        {
                            contents.Append("<td align=\"center\">");
                            contents.Append("&nbsp;");
                            contents.Append("</td>\n");
                        }
                        contents.Append("<td align=\"center\"><a href=\"newentities.aspx?entityname=manufacturer&manufacturerid=" + DB.RowFieldInt(row, "ManufacturerID").ToString() + "\">" + CommonLogic.HighlightTerm(ManufacturerHelper.GetEntityName(DB.RowFieldInt(row, "ManufacturerID"), LocaleSetting), st) + "</a></td>");
                        contents.Append("</tr>\n");
                        anyFound = true;
                    }
                    contents.Append("</table>\n");

                }
                products.Dispose();
                dsProducts.Dispose();

                if (!anyFound)
                {
                    contents.Append("<tr><td colspan=\"4\">No matches found</td></tr>\n");
                }
                contents.Append("<tr><td colspan=\"4\">&nbsp;</td></tr>\n");
                contents.Append("</table>\n");
                ltContents.Text = contents.ToString();
            }
            Page.Form.DefaultButton = btnSubmit.UniqueID;
        }

        protected void btnSubmit_OnClick(Object sender, EventArgs e)
        {
            Response.Redirect(AppLogic.AdminLinkUrl("search.aspx?SearchTerm=" + txtSearchTerm.Text));
        }
    }
}
