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
    /// Summary description for products.
    /// </summary>
    public partial class products : AdminPageBase
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            SectionTitle = AppLogic.GetString("admin.menu.ProductMgr", SkinID, LocaleSetting);
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
                if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilter").Length == 0 )
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
                if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilter").Length == 0 )
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
                if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilter").Length == 0 )
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
            //EntityHelper DistributorHelper = AppLogic.LookupHelper(EntityHelpers, "Distributor");
            //EntityHelper GenreHelper = AppLogic.LookupHelper(EntityHelpers, "Genre");
            //EntityHelper VectorHelper = AppLogic.LookupHelper(EntityHelpers, "VECTOR");
            //EntityHelper AffiliateHelper = AppLogic.LookupHelper(EntityHelpers, "Affiliate");
            //EntityHelper CustomerLevelHelper = AppLogic.LookupHelper(EntityHelpers, "CustomerLevel");

            if (CommonLogic.QueryStringCanBeDangerousContent("DeleteID").Length != 0)
            {
                int DeleteID = CommonLogic.QueryStringUSInt("DeleteID");
                DB.ExecuteSQL("delete from ShoppingCart where productid=" + DeleteID.ToString());
                DB.ExecuteSQL("delete from kitcart where productid=" + DeleteID.ToString());
                DB.ExecuteSQL("update Product set deleted=1 where ProductID=" + DeleteID.ToString());

                /* Modified by : mark
                 * Date : 11.16.2006
                 * No : 108
                 * Remove all Entity mappings for this product
                 */
                DB.ExecuteSQL("delete from ProductAffiliate where productid=" + DeleteID.ToString());
                DB.ExecuteSQL("delete from ProductCategory where productid=" + DeleteID.ToString());
                DB.ExecuteSQL("delete from ProductCustomerLevel where productid=" + DeleteID.ToString());
                DB.ExecuteSQL("delete from ProductDistributor where productid=" + DeleteID.ToString());
                DB.ExecuteSQL("delete from ProductGenre where productid=" + DeleteID.ToString());
                DB.ExecuteSQL("delete from ProductVector where productid=" + DeleteID.ToString());
                DB.ExecuteSQL("delete from ProductLocaleSetting where productid=" + DeleteID.ToString());
                DB.ExecuteSQL("delete from ProductManufacturer where productid=" + DeleteID.ToString());
                DB.ExecuteSQL("delete from ProductSection where productid=" + DeleteID.ToString());
                /******* end modification ****************/
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("NukeID").Length != 0)
            {
                int DeleteID = CommonLogic.QueryStringUSInt("NukeID");
                DB.ExecuteLongTimeSQL("aspdnsf_NukeProduct " + DeleteID.ToString(), 120);
            }

            writer.Append("<form id=\"FilterForm\" name=\"FilterForm\" method=\"GET\" action=\"" + AppLogic.AdminLinkUrl("products.aspx") + "\">\n");
            writer.Append("<a href=\"" + AppLogic.AdminLinkUrl("products.aspx") + "?resetfilters=true&categoryfilterid=0&sectionfilterid=0&producttypefilterid=0&manufacturerfilterid=0&distributorfilterid=0&genreid=0&Vectorid=0&affiliatefilterid=0&customerlevelfilterid=0\">" + AppLogic.GetString("admin.common.ResetFiltersUC", SkinID, LocaleSetting) + "</a>&nbsp;&nbsp;&nbsp;&nbsp;");

            String CatSel = CategoryHelper.GetEntitySelectList(0, String.Empty, 0, LocaleSetting, false);
            // mark current Category:
            CatSel = CatSel.Replace("<option value=\"" + CategoryFilterID.ToString() + "\">", "<option value=\"" + CategoryFilterID.ToString() + "\" selected>");
            if (CategoryHelper.m_TblMgr.NumRootLevelNodes > 0)
            {
                writer.Append(AppLogic.GetString("AppConfig.CategoryPromptSingular", SkinID, LocaleSetting) + ": ");
                writer.Append("<select onChange=\"document.FilterForm.submit()\" style=\"font-size: 9px;\" size=\"1\" name=\"CategoryFilterID\">\n");
                writer.Append("<OPTION VALUE=\"0\" " + CommonLogic.IIF(CategoryFilterID == 0, " selected ", "") + ">" + AppLogic.GetString("admin.common.AllCategories", SkinID, LocaleSetting) + "</option>\n");
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
                writer.Append("<OPTION VALUE=\"0\" " + CommonLogic.IIF(SectionFilterID == 0, " selected ", "") + ">" + AppLogic.GetString("admin.common.AllSections", SkinID, LocaleSetting) + "</option>\n");
                writer.Append(SecSel);
                writer.Append("</select>\n");
                writer.Append("&nbsp;&nbsp;");
            }

            String MfgSel = ManufacturerHelper.GetEntitySelectList(0, String.Empty, 0, LocaleSetting, false);
            if (ManufacturerHelper.m_TblMgr.NumRootLevelNodes > 0)
            {
                MfgSel = MfgSel.Replace("<option value=\"" + ManufacturerFilterID.ToString() + "\">", "<option value=\"" + ManufacturerFilterID.ToString() + "\" selected>");
                writer.Append(AppLogic.GetString("admin.common.Manufacturer", SkinID, LocaleSetting) + " <select size=\"1\" name=\"ManufacturerFilterID\" onChange=\"document.FilterForm.submit();\">\n");
                writer.Append("<OPTION VALUE=\"0\" " + CommonLogic.IIF(ManufacturerFilterID == 0, " selected ", "") + ">" + AppLogic.GetString("admin.common.AllManufacturers", SkinID, LocaleSetting) + "</option>\n");
                writer.Append(MfgSel);
                writer.Append("</select>\n");
                writer.Append("&nbsp;&nbsp;");
            }

            string prodTypeSql = string.Empty;
            prodTypeSql = "select count(*) as N from ProductType   with (NOLOCK); select * from ProductType   with (NOLOCK)  order by DisplayOrder,Name";
            
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(prodTypeSql, dbconn))
                {
                    if (rs.Read() && DB.RSFieldInt(rs, "N") > 0)
                    {
                        writer.Append(AppLogic.GetString("admin.common.ProductType", SkinID, LocaleSetting) + " <select size=\"1\" name=\"ProductTypeFilterID\" onChange=\"document.FilterForm.submit();\">\n");
                        writer.Append("<OPTION VALUE=\"0\" " + CommonLogic.IIF(ProductTypeFilterID == 0, " selected ", "") + ">" + AppLogic.GetString("admin.common.AllProductTypes", SkinID, LocaleSetting) + "</option>\n");
                        if (rs.NextResult())
                        {
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
            DataSet dsProducts = products.LoadFromDB(true);

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
                    writer.Append(String.Format(AppLogic.GetString("admin.common.Clickheretoturnpagingbackon", SkinID, LocaleSetting),"<a class=\"PageNumber\" href=\"" + AppLogic.AdminLinkUrl("products.aspx") + "?" + QueryParms + "&pagenum=1\">","</a>"));
                }
                else
                {
                    writer.Append(AppLogic.GetString("admin.common.Page", SkinID, LocaleSetting) + ":");
                    for (int u = 1; u <= NumPages; u++)
                    {
                        if (u == PageNum)
                        {
                            writer.Append(u.ToString() + " ");
                        }
                        else
                        {
                            writer.Append("<a class=\"PageNumber\" href=\"" + AppLogic.AdminLinkUrl("products.aspx") + "?" + QueryParms + "&pagenum=" + u.ToString() + "\">" + u.ToString() + "</a> ");
                        }
                    }
                    writer.Append(" <a class=\"PageNumber\" href=\"" + AppLogic.AdminLinkUrl("products.aspx") + "?" + QueryParms + "&show=all\">" + AppLogic.GetString("admin.common.All", SkinID, LocaleSetting) + "</a>");
                }
                writer.Append("</p>\n");
            }

            writer.Append("<form id=\"Form1\" name=\"Form1\" method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("products.aspx") + "?categoryfilterfilterid=" + CategoryFilterID.ToString() + "&manufacturerFilterID=" + ManufacturerFilterID.ToString() + "&distributorFilterID=" + DistributorFilterID.ToString() + "&genreFilterID=" + GenreFilterID.ToString() + "&VectorFilterID=" + VectorFilterID.ToString() + "&affiliateFilterID=" + AffiliateFilterID.ToString() + "&customerlevelFilterID=" + CustomerLevelFilterID.ToString() + "&producttypefilterid=" + ProductTypeFilterID.ToString() + "\">\n");
            writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
            if (AppLogic.MaxProductsExceeded())
            {
                writer.Append("<font class=\"errorMsg\">" + AppLogic.GetString("admin.entityProduct.ErrorMsg", SkinID, LocaleSetting) + "</font>");
                writer.Append("<p align=\"left\"><input class=\"normalButtonsDisabled\" type=\"button\" value=\"" + AppLogic.GetString("admin.products.AddNew", SkinID, LocaleSetting) + "\" name=\"AddNew\"  disabled=\"disabled\"></p>");
            }
            else
            {
                writer.Append("<p align=\"left\"><input class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.products.AddNew", SkinID, LocaleSetting) + "\" name=\"AddNew\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("editProduct.aspx") + "?categoryfilterid=" + CategoryFilterID.ToString() + "&sectionfilterid=" + SectionFilterID.ToString() + "&manufacturerfilterID=" + ManufacturerFilterID.ToString() + "&producttypefilterid=" + ProductTypeFilterID.ToString() + "';\"></p>");
            }
            writer.Append("  <table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\">");
            writer.Append("    <tr class=\"table-header\">\n");
            writer.Append("</td>");
            writer.Append("      <td align=\"center\" valign=\"middle\">" + AppLogic.GetString("admin.common.ID", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.common.Product", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.common.SKU", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.common.MfgPartNo", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.common.Inventory", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"center\" valign=\"middle\">" + AppLogic.GetString("admin.common.Edit", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"center\" valign=\"middle\">" + AppLogic.GetString("admin.common.Clone", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"center\" valign=\"middle\">" + AppLogic.GetString("admin.common.Variants", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"center\" valign=\"middle\">" + AppLogic.GetString("admin.common.Ratings", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"center\" valign=\"middle\">" + AppLogic.GetString("admin.common.SoftDelete", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"center\" valign=\"middle\">" + AppLogic.GetString("admin.common.Nuke", SkinID, LocaleSetting) + "</td>\n");            
            writer.Append("    </tr>\n");


            string style;
            int counter = 0;

            foreach (DataRow row in dsProducts.Tables[0].Rows)
            {
                if (counter % 2 == 0)
                {
                    style = "\"table-row2\"";
                }
                else
                {
                    style = "\"table-alternatingrow2\"";
                }
                writer.Append("    <tr class=" + style + ">\n");
                writer.Append("      <td align=\"center\" valign=\"middle\">" + DB.RowFieldInt(row, "ProductID").ToString() + "</td>\n");
                writer.Append("      <td align=\"left\" valign=\"middle\">");

                String Image1URL = AppLogic.LookupImage("Product", DB.RowFieldInt(row, "ProductID"), "icon", SkinID, LocaleSetting);
                writer.Append("<a href=\"" + AppLogic.AdminLinkUrl("editProduct.aspx") + "?Productid=" + DB.RowFieldInt(row, "ProductID").ToString() + "&categoryfilterid=" + CategoryFilterID.ToString() + "&sectionFilterID=" + SectionFilterID.ToString() + "&manufacturerfilterid=" + ManufacturerFilterID.ToString() + "&producttypefilterid=" + ProductTypeFilterID.ToString() + "\">");
                writer.Append("<img src=\"" + Image1URL + "\" height=\"25\" border=\"0\" align=\"absmiddle\">");
                writer.Append("</a>&nbsp;\n");
                writer.Append("<a href=\"" + AppLogic.AdminLinkUrl("editProduct.aspx") + "?Productid=" + DB.RowFieldInt(row, "ProductID").ToString() + "&categoryfilterid=" + CategoryFilterID.ToString() + "&sectionFilterID=" + SectionFilterID.ToString() + "&manufacturerfilterid=" + ManufacturerFilterID.ToString() + "&producttypefilterid=" + ProductTypeFilterID.ToString() + "\">");
                writer.Append(DB.RowFieldByLocale(row, "Name", LocaleSetting));
                writer.Append("</a>");

                writer.Append("</td>\n");
                writer.Append("<td align=\"left\" valign=\"middle\">" + DB.RowField(row, "SKU") + "</td>\n");
                writer.Append("<td align=\"left\" valign=\"middle\">" + DB.RowField(row, "ManufacturerPartNumber") + "</td>\n");
                writer.Append("<td align=\"left\" valign=\"middle\">");
                if (DB.RowFieldBool(row, "IsSystem"))
                {
                    writer.Append("System<br/>Product"); // this type of product can only be deleted in the db!
                }
                else
                {
                    if (AppLogic.ProductTracksInventoryBySizeAndColor(DB.RowFieldInt(row, "ProductID")))
                    {
                        writer.Append("<a href=\"" + AppLogic.AdminLinkUrl("editinventory.aspx") + "?productid=" + DB.RowFieldInt(row, "ProductID").ToString() + "\">" + AppLogic.GetString("admin.common.Inventory", SkinID, LocaleSetting) + "</a>\n");
                    }
                    else
                    {
                        writer.Append(DB.RowFieldInt(row, "Inventory").ToString());
                    }
                }
                writer.Append("</td>");

                writer.Append("<td align=\"center\" valign=\"middle\">");
                if (DB.RowFieldBool(row, "IsSystem"))
                {
                    writer.Append("System<br/>Product"); // this type of product can only be deleted in the db!
                }
                else
                {
                    writer.Append("<input class=\"normalButtons\"  type=\"button\" value=\"Edit\" name=\"Edit_" + DB.RowFieldInt(row, "ProductID").ToString() + "\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("editProduct.aspx") + "?Productid=" + DB.RowFieldInt(row, "ProductID").ToString() + "&categoryfilterid=" + CategoryFilterID.ToString() + "&sectionfilterID=" + SectionFilterID.ToString() + "&manufacturerfilterid=" + ManufacturerFilterID.ToString() + "&producttypefilterid=" + ProductTypeFilterID.ToString() + "'\">\n");
                }
                writer.Append("</td>");

                writer.Append("<td align=\"center\" valign=\"middle\">");
                if (DB.RowFieldBool(row, "IsSystem"))
                {
                    writer.Append(AppLogic.GetString("admin.common.System-Product", SkinID, LocaleSetting)); // this type of product can only be deleted in the db!
                }
                else
                {
                    if (AppLogic.MaxProductsExceeded())
                    {
                        writer.Append("<input class=\"normalButtonsDisabled\" type=\"button\" value=\"" + AppLogic.GetString("admin.common.Clone", SkinID, LocaleSetting) + "\" name=\"Clone_" + DB.RowFieldInt(row, "ProductID").ToString() + " disabled=\"disabled\" \">\n");
                    }
                    else
                    {
                        writer.Append("<input class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.common.Clone", SkinID, LocaleSetting) + "\" name=\"Clone_" + DB.RowFieldInt(row, "ProductID").ToString() + "\" onClick=\"if(confirm('" + AppLogic.GetString("admin.entityProducts.CloneQuestion", SkinID, LocaleSetting) + "')) {self.location='" + AppLogic.AdminLinkUrl("cloneproduct.aspx") + "?productid=" + DB.RowFieldInt(row, "ProductID").ToString() + "&categoryfilterid=" + CategoryFilterID.ToString() + "&sectionfilterID=" + SectionFilterID.ToString() + "&manufacturerfilterid=" + ManufacturerFilterID.ToString() + "&producttypefilterid=" + ProductTypeFilterID.ToString() + "';}\">\n");
                    }

                }
                writer.Append("</td>");

                writer.Append("<td align=\"center\" valign=\"middle\">");
                if (DB.RowFieldBool(row, "IsSystem"))
                {
                    writer.Append(AppLogic.GetString("admin.common.System-Product", SkinID, LocaleSetting)); // this type of product can only be deleted in the db!
                }
                else
                {
                    int NumVariants = DB.GetSqlN("select count(*) as N from productvariant   with (NOLOCK)  where deleted=0 and productid=" + DB.RowFieldInt(row, "ProductID").ToString());
                    writer.Append("<input class=\"normalButtons\" type=\"button\" value=\"" + String.Format(AppLogic.GetString("admin.products.Variants", SkinID, LocaleSetting),NumVariants.ToString()) + "\" name=\"Variants_" + DB.RowFieldInt(row, "ProductID").ToString() + "\" onClick=\"self.location='" + CommonLogic.IIF(NumVariants == 1, "" + AppLogic.AdminLinkUrl("variants.aspx") + "?Productid=" + DB.RowFieldInt(row, "ProductID").ToString(), "" + AppLogic.AdminLinkUrl("variants.aspx") + "?Productid=" + DB.RowFieldInt(row, "ProductID").ToString()) + "'\">\n");
                }
                writer.Append("</td>");
                writer.Append("<td align=\"center\" valign=\"middle\">");
                    
                if (DB.RowFieldBool(row, "IsSystem"))
                {
                    writer.Append(AppLogic.GetString("admin.common.System-Product", SkinID, LocaleSetting)); // this type of product can only be deleted in the db!
                }
                else
                {

                    int NumRatings = DB.GetSqlN("select count(*) as N from rating   with (NOLOCK)  where productid=" + DB.RowFieldInt(row, "ProductID").ToString());
                    writer.Append("<input class=\"normalButtons\" type=\"button\" value=\"" + String.Format(AppLogic.GetString("admin.entityProducts.Ratings", SkinID, LocaleSetting),NumRatings.ToString()) + "\" name=\"Ratings_" + DB.RowFieldInt(row, "ProductID").ToString() + "\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("productratings.aspx") + "?Productid=" + DB.RowFieldInt(row, "ProductID").ToString() + "'\">\n");
                }

                writer.Append("</td>");

                writer.Append("<td align=\"center\" valign=\"middle\">");
                if (DB.RowFieldBool(row, "IsSystem"))
                {
                    writer.Append(AppLogic.GetString("admin.common.System-Product", SkinID, LocaleSetting)); // this type of product can only be deleted in the db!
                }
                else
                {
                    if (DB.GetSqlN("select count(*) N from ShoppingCart with (NOLOCK) where VariantID=" + DB.RowFieldInt(row, "VariantID").ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()) > 0)
                    {
                        //product is in recurring order. cannot be deleted
                        writer.Append("<input disabled  type=\"image\" src=\"Skins/Skin_1/images/disabledButton.jpg\" alt=\"" + AppLogic.GetString("admin.common.DeleteNAInfoRecurring", SkinID, LocaleSetting) + "\" value='" + AppLogic.GetString("admin.common.DeleteNA", SkinID, LocaleSetting) + "' /> ");
                    }
                    else
                    {
                        //product is not in any recurring order. can be deleted
                        writer.Append("<input class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "\" name=\"Delete_" + DB.RowFieldInt(row, "ProductID").ToString() + "\" onClick=\"DeleteProduct(" + DB.RowFieldInt(row, "ProductID").ToString() + ")\">");
                    }

                }
                writer.Append("</td>");

                writer.Append("<td align=\"center\" valign=\"middle\">");
                if (DB.RowFieldBool(row, "IsSystem"))
                {
                    writer.Append(AppLogic.GetString("admin.common.System-Product", SkinID, LocaleSetting)); // this type of product can only be deleted in the db!
                }
                else
                {
                    writer.Append("<input class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.common.Nuke", SkinID, LocaleSetting) + "\" name=\"Nuke_" + DB.RowFieldInt(row, "ProductID").ToString() + "\" onClick=\"NukeProduct(" + DB.RowFieldInt(row, "ProductID").ToString() + ")\">");
                }
                writer.Append("</td>");
                writer.Append("</tr>\n");
                counter++;
            }
            products.Dispose();
            writer.Append("<tr class=\"table-footer\">\n");
            writer.Append("      <td align=\"left\" valign=\"top\" colspan=\"13\"></td>\n");
            writer.Append("</tr>\n");
            writer.Append("  </table>\n");
            if (AppLogic.MaxProductsExceeded())
            {
                writer.Append("<p align=\"left\"><input class=\"normalButtonsDisabled\" type=\"button\" value=\"" + AppLogic.GetString("admin.products.AddNew", SkinID, LocaleSetting) + "\" name=\"AddNew\"  disabled=\"disabled\"></p>");
            }
            else
            {
                writer.Append("<p align=\"left\"><input class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.products.AddNew", SkinID, LocaleSetting) + "\" name=\"AddNew\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("editProduct.aspx") + "?categoryfilterid=" + CategoryFilterID.ToString() + "&sectionfilterid=" + SectionFilterID.ToString() + "&manufacturerfilterid=" + ManufacturerFilterID.ToString() + "&producttypefilterid=" + ProductTypeFilterID.ToString() + "';\"></p>");
            }
            writer.Append("</form>\n");


            if (NumPages > 1 || ShowAll)
            {
                writer.Append("<p class=\"PageNumber\" align=\"left\">");
                writer.Append("<p class=\"PageNumber\" align=\"left\">");
                if (CommonLogic.QueryStringCanBeDangerousContent("show") == "all")
                {
                    writer.Append(String.Format(AppLogic.GetString("admin.common.Clickheretoturnpagingbackon", SkinID, LocaleSetting),"<a class=\"PageNumber\" href=\"" + AppLogic.AdminLinkUrl("products.aspx") + "?" + QueryParms + "&pagenum=1\">","</a>"));
                }
                else
                {
                    writer.Append(AppLogic.GetString("admin.common.Page", SkinID, LocaleSetting) + ": ");
                    for (int u = 1; u <= NumPages; u++)
                    {
                        if (u == PageNum)
                        {
                            writer.Append(u.ToString() + " ");
                        }
                        else
                        {
                            writer.Append("<a class=\"PageNumber\" href=\"" + AppLogic.AdminLinkUrl("products.aspx") + "?" + QueryParms + "&pagenum=" + u.ToString() + "\">" + u.ToString() + "</a> ");
                        }
                    }
                    writer.Append(" <a class=\"PageNumber\" href=\"" + AppLogic.AdminLinkUrl("products.aspx") + "?" + QueryParms + "&show=all\">" + AppLogic.GetString("admin.common.All", SkinID, LocaleSetting) + "</a>");
                }
                writer.Append("</p>\n");
            }

            writer.Append("<script type=\"text/javascript\">\n");
            writer.Append("function DeleteProduct(id)\n");
            writer.Append("{\n");
            writer.Append("  if(confirm('" + String.Format(AppLogic.GetString("admin.common.SoftDeleteProductsQuestion", SkinID, LocaleSetting),"'+ id +'")+"'\n");
            writer.Append("  {\n");
            writer.Append("      self.location = '" + AppLogic.AdminLinkUrl("products.aspx") + "?deleteid=' + id;\n");
            writer.Append("  }\n");
            writer.Append("}\n");

            writer.Append("function NukeProduct(id)\n");
            writer.Append("{\n");
            writer.Append("  if(confirm(" + String.Format(AppLogic.GetString("admin.common.NukeProductQuestion", SkinID, LocaleSetting),"'+ id +'")+ "'\n");
            writer.Append("  {\n");
            writer.Append("      self.location = '" + AppLogic.AdminLinkUrl("products.aspx") + "?nukeid=' + id;\n");
            writer.Append("  }\n");
            writer.Append("}\n");

            writer.Append("</SCRIPT>\n");
            dsProducts.Dispose();
            ltContent.Text = writer.ToString();
        }

    }
}
