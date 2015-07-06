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
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for saleprices.
    /// </summary>
    public partial class salepricescontent : AdminPageBase
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (CommonLogic.FormBool("IsSubmit"))
            {
                for (int i = 0; i <= Request.Form.Count - 1; i++)
                {
                    if (Request.Form.Keys[i].IndexOf("XPrice_") != -1 && Request.Form.Keys[i].IndexOf("_vldt") == -1)
                    {
                        String[] keys = Request.Form.Keys[i].Split('_');
                        int VariantID = Localization.ParseUSInt(keys[1]);
                        decimal Price = System.Decimal.Zero;
                        try
                        {
                            if (CommonLogic.FormCanBeDangerousContent("XPrice_" + VariantID.ToString()).Length != 0)
                            {
                                Price = CommonLogic.FormUSDecimal("XPrice_" + VariantID.ToString());
                            }
                            DB.ExecuteSQL("update ProductVariant set Price=" + CommonLogic.IIF(Price != System.Decimal.Zero, Localization.CurrencyStringForDBWithoutExchangeRate(Price), "NULL") + " where VariantID=" + VariantID.ToString());
                        }
                        catch { }
                    }
                    if (Request.Form.Keys[i].IndexOf("YPrice") != -1 && Request.Form.Keys[i].IndexOf("_vldt") == -1)
                    {
                        String[] keys = Request.Form.Keys[i].Split('_');
                        int VariantID = Localization.ParseUSInt(keys[1]);
                        decimal SalePrice = System.Decimal.Zero;
                        try
                        {
                            if (CommonLogic.FormCanBeDangerousContent("YPrice_" + VariantID.ToString()).Length != 0)
                            {
                                SalePrice = CommonLogic.FormUSDecimal("YPrice_" + VariantID.ToString());
                            }
                            DB.ExecuteSQL("update ProductVariant set SalePrice=" + CommonLogic.IIF(SalePrice != System.Decimal.Zero, Localization.CurrencyStringForDBWithoutExchangeRate(SalePrice), "NULL") + " where VariantID=" + VariantID.ToString());
                        }
                        catch { }
                    }
                }
            }
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            int CategoryFilterID = CommonLogic.QueryStringUSInt("CategoryFilterID");
            int SectionFilterID = CommonLogic.QueryStringUSInt("SectionFilterID");
            int ProductTypeFilterID = CommonLogic.QueryStringUSInt("ProductTypeFilterID");
            int ManufacturerFilterID = CommonLogic.QueryStringUSInt("ManufacturerFilterID");
            int DistributorFilterID = CommonLogic.QueryStringUSInt("DistributorFilterID");
            int GenreFilterID = CommonLogic.QueryStringUSInt("GenreFilterID");
            int VectorFilterID = CommonLogic.QueryStringUSInt("VectorFilterID");
            int AffiliateFilterID = CommonLogic.QueryStringUSInt("AffiliateFilterID");
            int CustomerLevelFilterID = CommonLogic.QueryStringUSInt("CustomerLevelFilterID");

            String ENCleaned = CommonLogic.QueryStringCanBeDangerousContent("EntityName").Trim();

            // kludge for now, during conversion to properly entity/object setup:
            if (ENCleaned.Equals("CATEGORY", StringComparison.InvariantCultureIgnoreCase))
            {
                CategoryFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
                SectionFilterID = 0;
                ProductTypeFilterID = 0;
                ManufacturerFilterID = 0;
                DistributorFilterID = 0;
                GenreFilterID = 0;
                VectorFilterID = 0;
                AffiliateFilterID = 0;
                CustomerLevelFilterID = 0;
            }
            if (ENCleaned.Equals("SECTION", StringComparison.InvariantCultureIgnoreCase))
            {
                CategoryFilterID = 0;
                SectionFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
                ProductTypeFilterID = 0;
                ManufacturerFilterID = 0;
                DistributorFilterID = 0;
                GenreFilterID = 0;
                VectorFilterID = 0;
                AffiliateFilterID = 0;
                CustomerLevelFilterID = 0;
            }
            if (ENCleaned.Equals("MANUFACTURER", StringComparison.InvariantCultureIgnoreCase))
            {
                CategoryFilterID = 0;
                SectionFilterID = 0;
                ProductTypeFilterID = 0;
                ManufacturerFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
                DistributorFilterID = 0;
                GenreFilterID = 0;
                VectorFilterID = 0;
                AffiliateFilterID = 0;
                CustomerLevelFilterID = 0;
            }
            if (ENCleaned.Equals("DISTRIBUTOR", StringComparison.InvariantCultureIgnoreCase))
            {
                CategoryFilterID = 0;
                SectionFilterID = 0;
                ProductTypeFilterID = 0;
                ManufacturerFilterID = 0;
                DistributorFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
                GenreFilterID = 0;
                VectorFilterID = 0;
                AffiliateFilterID = 0;
                CustomerLevelFilterID = 0;
            }
            if (ENCleaned.Equals("GENRE", StringComparison.InvariantCultureIgnoreCase))
            {
                CategoryFilterID = 0;
                SectionFilterID = 0;
                ProductTypeFilterID = 0;
                ManufacturerFilterID = 0;
                DistributorFilterID = 0;
                GenreFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
                VectorFilterID = 0;
                AffiliateFilterID = 0;
                CustomerLevelFilterID = 0;
            }

            if (ENCleaned.Equals("VECTOR", StringComparison.InvariantCultureIgnoreCase))
            {
                CategoryFilterID = 0;
                SectionFilterID = 0;
                ProductTypeFilterID = 0;
                ManufacturerFilterID = 0;
                DistributorFilterID = 0;
                GenreFilterID = 0;
                VectorFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
                AffiliateFilterID = 0;
                CustomerLevelFilterID = 0;
            }
            if (ENCleaned.Equals("AFFILIATE", StringComparison.InvariantCultureIgnoreCase))
            {
                CategoryFilterID = 0;
                SectionFilterID = 0;
                ProductTypeFilterID = 0;
                ManufacturerFilterID = 0;
                DistributorFilterID = 0;
                GenreFilterID = 0;
                VectorFilterID = 0;
                AffiliateFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
                CustomerLevelFilterID = 0;
            }
            if (ENCleaned.Equals("CUSTOMERLEVEL", StringComparison.InvariantCultureIgnoreCase))
            {
                CategoryFilterID = 0;
                SectionFilterID = 0;
                ProductTypeFilterID = 0;
                ManufacturerFilterID = 0;
                DistributorFilterID = 0;
                GenreFilterID = 0;
                VectorFilterID = 0;
                AffiliateFilterID = 0;
                CustomerLevelFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
            }
            // end kludge


            if (CommonLogic.QueryStringCanBeDangerousContent("CategoryFilterID").Length == 0 && CommonLogic.QueryStringCanBeDangerousContent("EntityFilterID").Length == 0)
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilter").Length == 0)
                {
                    CategoryFilterID = Convert.ToInt32(CommonLogic.IIF(CommonLogic.IsInteger(Profile.AdminCategoryFilterID), Profile.AdminCategoryFilterID, "0"));
                }
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("SectionFilterID").Length == 0 && CommonLogic.QueryStringCanBeDangerousContent("EntityFilterID").Length == 0)
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilter").Length == 0)
                {
                    SectionFilterID = Convert.ToInt32(CommonLogic.IIF(CommonLogic.IsInteger(Profile.AdminSectionFilterID), Profile.AdminSectionFilterID, "0"));
                }
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("ManufacturerFilterID").Length == 0 && CommonLogic.QueryStringCanBeDangerousContent("EntityFilterID").Length == 0)
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilter").Length == 0)
                {
                    ManufacturerFilterID = Convert.ToInt32(CommonLogic.IIF(CommonLogic.IsInteger(Profile.AdminManufacturerFilterID), Profile.AdminManufacturerFilterID, "0"));
                }
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("DistributorFilterID").Length == 0 && CommonLogic.QueryStringCanBeDangerousContent("EntityFilterID").Length == 0)
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilter").Length == 0)
                {
                    DistributorFilterID = Convert.ToInt32(CommonLogic.IIF(CommonLogic.IsInteger(Profile.AdminDistributorFilterID), Profile.AdminDistributorFilterID, "0"));
                }
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("GenreFilterID").Length == 0 && CommonLogic.QueryStringCanBeDangerousContent("EntityFilterID").Length == 0)
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilter").Length == 0)
                {
                    GenreFilterID = Convert.ToInt32(CommonLogic.IIF(CommonLogic.IsInteger(Profile.AdminGenreFilterID), Profile.AdminGenreFilterID, "0"));
                }
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("VectorFilterID").Length == 0 && CommonLogic.QueryStringCanBeDangerousContent("EntityFilterID").Length == 0)
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilter").Length == 0)
                {
                    VectorFilterID = Convert.ToInt32(CommonLogic.IIF(CommonLogic.IsInteger(Profile.AdminVectorFilterID), Profile.AdminVectorFilterID, "0"));
                }
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("AffiliateFilterID").Length == 0 && CommonLogic.QueryStringCanBeDangerousContent("EntityFilterID").Length == 0)
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilter").Length == 0)
                {
                    AffiliateFilterID = Convert.ToInt32(CommonLogic.IIF(CommonLogic.IsInteger(Profile.AdminAffiliateFilterID), Profile.AdminAffiliateFilterID, "0"));
                }
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("CustomerLevelFilterID").Length == 0 && CommonLogic.QueryStringCanBeDangerousContent("EntityFilterID").Length == 0)
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilter").Length == 0)
                {
                    CustomerLevelFilterID = Convert.ToInt32(CommonLogic.IIF(CommonLogic.IsInteger(Profile.AdminCustomerLevelFilterID), Profile.AdminCustomerLevelFilterID, "0"));
                }
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("ProductTypeFilterID").Length == 0 && CommonLogic.QueryStringCanBeDangerousContent("EntityFilterID").Length == 0)
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilter").Length == 0)
                {
                    ProductTypeFilterID = Convert.ToInt32(CommonLogic.IIF(CommonLogic.IsInteger(Profile.AdminProductTypeFilterID), Profile.AdminProductTypeFilterID, "0"));
                }
                if (ProductTypeFilterID != 0 && !AppLogic.ProductTypeHasVisibleProducts(ProductTypeFilterID))
                {
                    ProductTypeFilterID = 0;
                }
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilter").Length != 0)
            {
                CategoryFilterID = 0;
                SectionFilterID = 0;
                ManufacturerFilterID = 0;
                DistributorFilterID = 0;
                GenreFilterID = 0;
                VectorFilterID = 0;
                AffiliateFilterID = 0;
                CustomerLevelFilterID = 0;
                ProductTypeFilterID = 0;
            }

            Profile.AdminCategoryFilterID = CategoryFilterID.ToString();
            Profile.AdminSectionFilterID = SectionFilterID.ToString();
            Profile.AdminManufacturerFilterID = ManufacturerFilterID.ToString();
            Profile.AdminDistributorFilterID = DistributorFilterID.ToString();
            Profile.AdminGenreFilterID = GenreFilterID.ToString();
            Profile.AdminVectorFilterID = VectorFilterID.ToString();
            Profile.AdminAffiliateFilterID = AffiliateFilterID.ToString();
            Profile.AdminCustomerLevelFilterID = CustomerLevelFilterID.ToString();
            Profile.AdminProductTypeFilterID = ProductTypeFilterID.ToString();

            EntityHelper CategoryHelper = AppLogic.LookupHelper(EntityHelpers, "Category");
            EntityHelper SectionHelper = AppLogic.LookupHelper(EntityHelpers, "Section");
            EntityHelper ManufacturerHelper = AppLogic.LookupHelper(EntityHelpers, "Manufacturer");
            EntityHelper DistributorHelper = AppLogic.LookupHelper(EntityHelpers, "Distributor");
            EntityHelper GenreHelper = AppLogic.LookupHelper(EntityHelpers, "Genre");
            EntityHelper VectorHelper = AppLogic.LookupHelper(EntityHelpers, "VECTOR");
            EntityHelper AffiliateHelper = AppLogic.LookupHelper(EntityHelpers, "Affiliate");
            EntityHelper CustomerLevelHelper = AppLogic.LookupHelper(EntityHelpers, "CustomerLevel");

            if (CommonLogic.QueryStringCanBeDangerousContent("DeleteID").Length != 0)
            {
                int DeleteID = CommonLogic.QueryStringUSInt("DeleteID");
                DB.ExecuteSQL("delete from ShoppingCart where productid=" + DeleteID.ToString());
                DB.ExecuteSQL("delete from kitcart where productid=" + DeleteID.ToString());
                DB.ExecuteSQL("update Product set deleted=1 where ProductID=" + DeleteID.ToString());
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("NukeID").Length != 0)
            {
                int DeleteID = CommonLogic.QueryStringUSInt("NukeID");
                DB.ExecuteLongTimeSQL("aspdnsf_NukeProduct " + DeleteID.ToString(), 120);
            }
            //writer.Append("</form>");
            writer.Append("<form id=\"FilterForm\" style=\"height:26px;padding:2px 5px;\" name=\"FilterForm\" method=\"GET\" action=\"" + AppLogic.AdminLinkUrl("salepricescontent.aspx") + "\">\n");
            writer.Append("<a href=\"" + AppLogic.AdminLinkUrl("salepricescontent.aspx") + "?resetfilters=true&categoryfilterid=0&sectionfilterid=0&producttypefilterid=0&manufacturerfilterid=0&distributorfilterid=0&genreid=0&Vectorid=0&affiliatefilterid=0&customerlevelfilterid=0\">RESET FILTERS</a>&nbsp;&nbsp;&nbsp;&nbsp;");

            String CatSel = CategoryHelper.GetEntitySelectList(0, String.Empty, 0, LocaleSetting, false);
            // mark current Category:
            CatSel = CatSel.Replace("<option value=\"" + CategoryFilterID.ToString() + "\">", "<option value=\"" + CategoryFilterID.ToString() + "\" selected>");
            if (CategoryHelper.m_TblMgr.NumRootLevelNodes > 0)
            {
                writer.Append(AppLogic.GetString("AppConfig.CategoryPromptSingular", SkinID, LocaleSetting) + ": ");
                writer.Append("<select onChange=\"document.FilterForm.submit()\" style=\"font-size: 9px;\" size=\"1\" name=\"CategoryFilterID\">\n");
                writer.Append("<OPTION VALUE=\"0\" " + CommonLogic.IIF(CategoryFilterID == 0, " selected ", "") + ">All " + AppLogic.GetString("AppConfig.CategoryPromptPlural", SkinID, LocaleSetting) + "</option>\n");
                writer.Append(CatSel);
                writer.Append("</select>\n");
                writer.Append("&nbsp;&nbsp;");
            }

            String SecSel = SectionHelper.GetEntitySelectList(0, String.Empty, 0, LocaleSetting, false);
            if (SectionHelper.m_TblMgr.NumRootLevelNodes > 0)
            {
                SecSel = SecSel.Replace("<option value=\"" + SectionFilterID.ToString() + "\">", "<option value=\"" + SectionFilterID.ToString() + "\" selected>");
                writer.Append(AppLogic.GetString("AppConfig.SectionPromptSingular", SkinID, LocaleSetting) + ": ");
                writer.Append("<select onChange=\"document.FilterForm.submit()\" style=\"font-size: 9px;\" size=\"1\" name=\"SectionFilterID\">\n");
                writer.Append("<OPTION VALUE=\"0\" " + CommonLogic.IIF(SectionFilterID == 0, " selected ", "") + ">All " + AppLogic.GetString("AppConfig.SectionPromptPlural", SkinID, LocaleSetting) + "</option>\n");
                writer.Append(SecSel);
                writer.Append("</select>\n");
                writer.Append("&nbsp;&nbsp;");
            }

            String MfgSel = ManufacturerHelper.GetEntitySelectList(0, String.Empty, 0, LocaleSetting, false);
            if (ManufacturerHelper.m_TblMgr.NumRootLevelNodes > 0)
            {
                MfgSel = MfgSel.Replace("<option value=\"" + ManufacturerFilterID.ToString() + "\">", "<option value=\"" + ManufacturerFilterID.ToString() + "\" selected>");
                writer.Append("Manufacturer: <select size=\"1\" name=\"ManufacturerFilterID\" onChange=\"document.FilterForm.submit();\">\n");
                writer.Append("<OPTION VALUE=\"0\" " + CommonLogic.IIF(ManufacturerFilterID == 0, " selected ", "") + ">All Manufacturers</option>\n");
                writer.Append(MfgSel);
                writer.Append("</select>\n");
                writer.Append("&nbsp;&nbsp;");
            }
          
            string sqlCount = "select count(*) as N from ProductType with (NOLOCK)";
            string sqlProd = string.Empty;

            sqlProd = "select * from ProductType   with (NOLOCK)  order by DisplayOrder,Name";

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(sqlCount + ";" + sqlProd, con))
                {
                    if (rs.Read() && DB.RSFieldInt(rs, "N") > 0)
                    {
                        if (rs.NextResult())
                        {
                            writer.Append("Product Type: <select size=\"1\" name=\"ProductTypeFilterID\" onChange=\"document.FilterForm.submit();\">\n");
                            writer.Append("<OPTION VALUE=\"0\" " + CommonLogic.IIF(ProductTypeFilterID == 0, " selected ", "") + ">All Product Types</option>\n");
                            while (rs.Read())
                            {
                                writer.Append("<option value=\"" + DB.RSFieldInt(rs, "ProductTypeID").ToString() + "\"");
                                if (DB.RSFieldInt(rs, "ProductTypeID") == ProductTypeFilterID)
                                {
                                    writer.Append(" selected");
                                }
                                writer.Append(">" + DB.RSFieldByLocale(rs, "Name", LocaleSetting) + "</option>");
                            }
                        }

                        writer.Append("</select>\n");
                    }
                }
            }

            writer.Append("</form>\n");

            int PageSize = AppLogic.AppConfigUSInt("Admin_ProductPageSize");
            if (PageSize == 0)
            {
                PageSize = 50;
            }
            int PageNum = CommonLogic.QueryStringUSInt("PageNum");
            if (PageNum == 0)
            {
                PageNum = 1;
            }
            bool ShowAll = (CommonLogic.QueryStringCanBeDangerousContent("show").Equals("ALL", StringComparison.InvariantCultureIgnoreCase));
            if (ShowAll)
            {
                PageSize = 0;
                PageNum = 1;
            }

            ProductCollection products = new ProductCollection();
            products.PageSize = PageSize;
            products.PageNum = PageNum;
            products.CategoryID = CategoryFilterID;
            products.SectionID = SectionFilterID;
            products.ManufacturerID = ManufacturerFilterID;
            products.DistributorID = DistributorFilterID;
            products.GenreID = GenreFilterID;
            products.VectorID = VectorFilterID;
            products.AffiliateID = AffiliateFilterID;
            products.CustomerLevelID = CustomerLevelFilterID;
            products.ProductTypeID = ProductTypeFilterID;
            products.PublishedOnly = false;
            products.OnSaleOnly = true;
            products.ReturnAllVariants = true;
            DataSet dsProducts = products.LoadFromDB();

            String QueryParms = "categoryfilterid=" + CategoryFilterID.ToString() + "&sectionfilterid=" + SectionFilterID.ToString() + "&manufacturerfilterid=" + ManufacturerFilterID.ToString() + "&distributorfilterid=" + DistributorFilterID.ToString() + "&genrefilterid=" + GenreFilterID.ToString() + "&Vectorfilterid=" + VectorFilterID.ToString() + "&affiliatefilterid=" + AffiliateFilterID.ToString() + "&customerlevelfilterid=" + CustomerLevelFilterID.ToString() + "&producttypefilterid=" + ProductTypeFilterID.ToString();

            int NumPages = products.NumPages;

            // ---------------------------------------------------
            // Append paging info:
            // ---------------------------------------------------
            if (NumPages > 1 || ShowAll)
            {
                writer.Append("<p class=\"PageNumber\" align=\"left\">");
                writer.Append("<p class=\"PageNumber\" align=\"left\">");
                if (CommonLogic.QueryStringCanBeDangerousContent("show") == "all")
                {
                    writer.Append("Click <a class=\"PageNumber\" href=\"" + AppLogic.AdminLinkUrl("salepricescontent.aspx") + "?" + QueryParms + "&pagenum=1\">here</a> to turn paging back on.");
                }
                else
                {
                    writer.Append("Page: ");
                    for (int u = 1; u <= NumPages; u++)
                    {
                        if (u == PageNum)
                        {
                            writer.Append(u.ToString() + " ");
                        }
                        else
                        {
                            writer.Append("<a class=\"PageNumber\" href=\"" + AppLogic.AdminLinkUrl("salepricescontent.aspx") + "?" + QueryParms + "&pagenum=" + u.ToString() + "\">" + u.ToString() + "</a> ");
                        }
                    }
                    writer.Append(" <a class=\"PageNumber\" href=\"" + AppLogic.AdminLinkUrl("salepricescontent.aspx") + "?" + QueryParms + "&show=all\">all</a>");
                }
                writer.Append("</p>\n");
            }

            writer.Append("<script type=\"text/javascript\">\n");
            writer.Append("function PriceForm_Validator(theForm)\n");
            writer.Append("{\n");
            writer.Append("submitonce(theForm);\n");
            writer.Append("return (true);\n");
            writer.Append("}\n");
            writer.Append("</script>\n");

            writer.Append("<form id=\"PriceForm\" name=\"PriceForm\" method=\"POST\" style=\"height:auto;\" action=\"" + AppLogic.AdminLinkUrl("salepricescontent.aspx") + "?categoryfilterfilterid=" + CategoryFilterID.ToString() + "&manufacturerFilterID=" + ManufacturerFilterID.ToString() + "&distributorFilterID=" + DistributorFilterID.ToString() + "&genreFilterID=" + GenreFilterID.ToString() + "&distributorFilterID=" + VectorFilterID.ToString() + "&VectorFilterID=" + AffiliateFilterID.ToString() + "&customerlevelFilterID=" + CustomerLevelFilterID.ToString() + "&producttypefilterid=" + ProductTypeFilterID.ToString() + "&pagenum=" + PageNum.ToString() + "\" onsubmit=\"return (validateForm(this) && PriceForm_Validator(this))\" >\n");
            writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
            writer.Append("  <table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\">\n");
            writer.Append("    <tr class=\"table-header\">\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">Product ID</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">Variant ID</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">Product</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">SKU</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">Price</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">SalePrice</td>\n");
            writer.Append("    </tr>\n");
            foreach (DataRow row in dsProducts.Tables[0].Rows)
            {
                writer.Append("<tr class=\"table-row\">\n");
                writer.Append("<td align=\"left\" valign=\"middle\">" + DB.RowFieldInt(row, "ProductID").ToString() + "</td>\n");
                writer.Append("<td align=\"left\" valign=\"middle\">" + DB.RowFieldInt(row, "VariantID").ToString() + "</td>\n");
                writer.Append("<td align=\"left\" valign=\"middle\">");

                String Image1URL = AppLogic.LookupImage("Product", DB.RowFieldInt(row, "ProductID"), "icon", SkinID, LocaleSetting);
                if (Image1URL.Length != 0)
                {
                    writer.Append("<img src=\"" + Image1URL + "\" height=\"25\" border=\"0\" align=\"absmiddle\">");
                }
                writer.Append(DB.RowFieldByLocale(row, "Name", LocaleSetting));
                if (DB.RowField(row, "VName").Length != 0)
                {
                    writer.Append(" - ");
                }
                writer.Append(DB.RowField(row, "VName"));

                writer.Append("</td>\n");
                writer.Append("<td align=\"left\" valign=\"middle\">" + DB.RowField(row, "SKU") + DB.RowField(row, "SKUSuffix") + "</td>\n");
                writer.Append("<td align=\"left\" valign=\"middle\">");
                writer.Append("<input name=\"XPrice_" + DB.RowFieldInt(row, "VariantID").ToString() + "\" type=\"text\" size=\"10\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(row, "Price")) + "\">");
                writer.Append("<input name=\"XPrice_" + DB.RowFieldInt(row, "VariantID").ToString() + "_vldt\" type=\"hidden\" value=\"[number][invalidalert=please enter a valid dollar amount]\">");
                writer.Append("</td>\n");
                writer.Append("<td align=\"left\" valign=\"middle\">");
                writer.Append("<input name=\"YPrice_" + DB.RowFieldInt(row, "VariantID").ToString() + "\" type=\"text\" size=\"10\" value=\"" + CommonLogic.IIF(DB.RowFieldDecimal(row, "SalePrice") != System.Decimal.Zero, Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(row, "SalePrice")), "") + "\">");
                writer.Append("<input name=\"YPrice_" + DB.RowFieldInt(row, "VariantID").ToString() + "_vldt\" type=\"hidden\" value=\"[number][invalidalert=please enter a valid dollar amount]\">");
                writer.Append("</td>\n");
                writer.Append("</tr>\n");
            }
            products.Dispose();
            writer.Append("</table>\n");
            writer.Append("<p align=\"right\"><input class=\"normalbuttons\" type=\"submit\" value=\"Update\" name=\"Submit\"></p>\n");
            writer.Append("</form>\n");
            ltContent.Text = writer.ToString();
            dsProducts.Dispose();
        }

    }
}
