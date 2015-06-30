// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Text;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for phonesearch.
    /// </summary>
    public partial class phonesearch : AdminPageBase
    {

        String m_IGD = String.Empty;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            m_IGD = CommonLogic.QueryStringCanBeDangerousContent("IGD");

            EntityHelper CategoryHelper = AppLogic.LookupHelper(EntityHelpers, "Category");
            EntityHelper SectionHelper = AppLogic.LookupHelper(EntityHelpers, "Section");
            EntityHelper ManufacturerHelper = AppLogic.LookupHelper(EntityHelpers, "Manufacturer");
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            writer.Append("<form method=\"GET\" action=\"" + AppLogic.AdminLinkUrl("phonesearch.aspx") + "\" onsubmit=\"return (validateForm(this) && SearchForm2_Validator(this))\" name=\"SearchForm2\">\n");
            writer.Append("<input type=\"hidden\" name=\"IGD\" id=\"IGD\" value=\"" + m_IGD + "\">");
            writer.Append("<p>" + AppLogic.GetString("admin.phonesearch.SearchInfo", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + ":</p>\n");
            writer.Append("<input type=\"text\" name=\"SearchTerm\" onkeypress=\"javascript:return WebForm_FireDefaultButton(event, 'btnsearch')\" size=\"25\" maxlength=\"70\" value=\"" + Server.HtmlEncode(CommonLogic.QueryStringCanBeDangerousContent("SearchTerm")) + "\">\n");
            writer.Append("<input type=\"hidden\" name=\"SearchTerm_Vldt\" value=\"[req][blankalert=" + AppLogic.GetString("admin.common.SomethingToSearchForPrompt", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "]\">\n");
            writer.Append("&nbsp;<input type=\"submit\" id=\"btnsearch\" value=\"" + AppLogic.GetString("admin.common.Search", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\" name=\"B1\"></td>\n");
            writer.Append("<br/>");
            writer.Append("<br/>");


            String st = CommonLogic.QueryStringCanBeDangerousContent("SearchTerm").Trim();
            if (st.Length != 0)
            {
                String stlike = "%" + st + "%";
                String stquoted = DB.SQuote(stlike);

                // MATCHING PRODUCTS:
                ProductCollection products = new ProductCollection();
                products.PageSize = 0;
                products.PageNum = 1;
                products.SearchMatch = st;
                products.SearchDescriptionAndSummaryFields = false;
                products.PublishedOnly = false;
                products.ExcludePacks = true;
                DataSet dsProducts = products.LoadFromDB();
                int NumProducts = products.NumProducts;

                bool anyFound = false;
                if (NumProducts > 0)
                {
                    anyFound = true;
                    foreach (DataRow row in dsProducts.Tables[0].Rows)
                    {
                        String url = "javascript:window.parent.frames['RightPanel2Frame'].location.href='../showproduct.aspx?IGD=" + m_IGD + "&amp;productid=" + DB.RowFieldInt(row, "ProductID").ToString() + "';javascript:void(0);";

                        writer.Append("<a href=\"" + url + "\">" + AppLogic.MakeProperObjectName(DB.RowFieldByLocale(row, "Name", ThisCustomer.LocaleSetting), DB.RowFieldByLocale(row, "VariantName", ThisCustomer.LocaleSetting), ThisCustomer.LocaleSetting) + "</a>");
                        writer.Append("<br/>");
                        anyFound = true;
                    }

                }
                products.Dispose();
                dsProducts.Dispose();

                if (!anyFound)
                {
                    writer.Append(AppLogic.GetString("admin.common.NoMatchesFound", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\n");
                }
            }
            writer.Append("</form>\n");
            ltContent.Text = writer.ToString();
        }
    }
}
