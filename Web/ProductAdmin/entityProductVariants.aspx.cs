// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Data;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for variants.
    /// </summary>
    public partial class entityProductVariants : AdminPageBase
    {
        private int ProductID = 0;
        private String ProductName;
        private String ProductSKU;
        private bool ProductTracksInventoryBySizeAndColor;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            ProductID = CommonLogic.QueryStringUSInt("ProductID");
            if (ProductID == 0)
            {
                Response.Redirect(AppLogic.AdminLinkUrl("products.aspx"));
            }

            ProductName = AppLogic.GetProductName(ProductID, LocaleSetting);
            ProductSKU = AppLogic.GetProductSKU(ProductID);

            ProductTracksInventoryBySizeAndColor = AppLogic.ProductTracksInventoryBySizeAndColor(ProductID);

            if (CommonLogic.QueryStringCanBeDangerousContent("CloneID").Length != 0)
            {
                int CloneID = CommonLogic.QueryStringUSInt("CloneID");
                DB.ExecuteSQL("aspdnsf_CloneVariant " + CloneID.ToString() + "," + ThisCustomer.CustomerID.ToString());
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("DeleteID").Length != 0)
            {
                int DeleteID = CommonLogic.QueryStringUSInt("DeleteID");
                DB.ExecuteSQL("delete from KitCart where VariantID=" + DeleteID.ToString());
                DB.ExecuteSQL("delete from ShoppingCart where VariantID=" + DeleteID.ToString());
                DB.ExecuteSQL("delete from ProductVariant where VariantID=" + DeleteID.ToString());
            }

            if (CommonLogic.QueryStringBool("DeleteAllVariants"))
            {
                DB.ExecuteSQL("delete from KitCart where VariantID in (select VariantID from ProductVariant where ProductID=" + ProductID.ToString() + ")");
                DB.ExecuteSQL("delete from ShoppingCart where VariantID in (select VariantID from ProductVariant where ProductID=" + ProductID.ToString() + ")");
                DB.ExecuteSQL("delete from ProductVariant where ProductID=" + ProductID.ToString());
            }

            if (CommonLogic.FormBool("IsSubmit"))
            {
                DB.ExecuteSQL("update ProductVariant set IsDefault=0 where ProductID=" + ProductID.ToString());
                if (CommonLogic.FormCanBeDangerousContent("IsDefault").Length == 0 || CommonLogic.FormUSInt("IsDefault") == 0)
                {
                    // try to force a default variant, none was specified!
                    DB.ExecuteSQL("update ProductVariant set IsDefault=1 where ProductID=" + ProductID.ToString() + " and VariantID in (SELECT top 1 VariantID from ProductVariant where ProductID=" + ProductID.ToString() + " order by DisplayOrder,Name)");
                }
                else
                {
                    DB.ExecuteSQL("update ProductVariant set IsDefault=1 where ProductID=" + ProductID.ToString() + " and VariantID=" + CommonLogic.FormUSInt("IsDefault").ToString());
                }
                for (int i = 0; i <= Request.Form.Count - 1; i++)
                {
                    if (Request.Form.Keys[i].IndexOf("DisplayOrder_") != -1)
                    {
                        String[] keys = Request.Form.Keys[i].Split('_');
                        int VariantID = Localization.ParseUSInt(keys[1]);
                        int DispOrd = 1;
                        try
                        {
                            DispOrd = Localization.ParseUSInt(Request.Form[Request.Form.Keys[i]]);
                        }
                        catch { }
                        DB.ExecuteSQL("update productvariant set DisplayOrder=" + DispOrd.ToString() + " where VariantID=" + VariantID.ToString());
                    }
                }
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("DeleteAllVariants").Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) == false)
            {
                AppLogic.MakeSureProductHasAtLeastOneVariant(ProductID);
            }
            AppLogic.EnsureProductHasADefaultVariantSet(ProductID);

            LoadData();
        }

        protected void LoadData()
        {
            string temp = "";
            temp += ("<form id=\"Form1\" name=\"Form1\" method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("entityProductVariants.aspx") + "?ProductID=" + CommonLogic.QueryStringNativeInt("ProductID") + "&entityname=" + CommonLogic.QueryStringCanBeDangerousContent("entityname") + "&EntityID=" + CommonLogic.QueryStringNativeInt("EntityID") + "\">\n");
            temp += ("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
            temp += ("<table border=\"0\" cellpadding=\"2\" border=\"0\" cellspacing=\"0\" width=\"100%\">\n");
            temp += ("<tr class=\"table-header\">\n");
            temp += ("<td><b>" + AppLogic.GetString("admin.common.ID", SkinID, LocaleSetting) + "</b></td>\n");
            temp += ("<td><b>" + AppLogic.GetString("admin.common.Variant", SkinID, LocaleSetting) + "</b></td>\n");
            temp += ("<td><b>" + AppLogic.GetString("admin.common.VariantSKUSuffix", SkinID, LocaleSetting) + "</b></td>\n");
            temp += ("<td><b>" + AppLogic.GetString("admin.common.FullSKU", SkinID, LocaleSetting) + "</b></td>\n");
            temp += ("<td><b>" + AppLogic.GetString("admin.common.Price", SkinID, LocaleSetting) + "</b></td>\n");
            temp += ("<td><b>" + AppLogic.GetString("admin.common.SalePrice", SkinID, LocaleSetting) + "</b></td>\n");
            temp += ("<td align=\"center\"><b>" + AppLogic.GetString("admin.common.Inventory", SkinID, LocaleSetting) + "</b></td>\n");
            temp += ("<td align=\"center\"><b>" + AppLogic.GetString("admin.common.DisplayOrder", SkinID, LocaleSetting) + "</b></td>\n");
            temp += ("<td align=\"center\"><b>" + AppLogic.GetString("admin.common.IsDefaultVariant", SkinID, LocaleSetting) + "</b></td>\n");
            temp += ("<td align=\"center\"><b>" + AppLogic.GetString("admin.entityProducts.Clone", SkinID, LocaleSetting) + "</b></td>\n");
            temp += ("<td align=\"center\"><b>" + AppLogic.GetString("admin.common.Move", SkinID, LocaleSetting) + "</b></td>\n");
            temp += ("<td align=\"center\"><b>" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "</b></td>\n");
            temp += ("</tr>\n");
            string sqlProd = "select * from productvariant   with (NOLOCK)  where deleted=0 and ProductID=" + ProductID.ToString() + " order by DisplayOrder,Name";
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(sqlProd, con))
                {
                    string style;
                    int counter = 0;

                    while (rs.Read())
                    {
                        if (counter % 2 == 0)
                        {
                            style = "\"table-row2\"";
                        }
                        else
                        {
                            style = "\"table-alternatingrow2\"";
                        }
                        temp += ("    <tr class=" + style + ">\n");
                        temp += ("<td >" + DB.RSFieldInt(rs, "VariantID").ToString() + "</td>\n");
                        temp += ("<td >");
                        String Image1URL = AppLogic.LookupImage("Variant", DB.RSFieldInt(rs, "VariantID"), "icon", SkinID, LocaleSetting);
                        if (Image1URL.Length != 0)
                        {
                            temp += ("<a href=\"" + AppLogic.AdminLinkUrl("entityEditProductVariant.aspx") + "?ProductID=" + CommonLogic.QueryStringNativeInt("ProductID") + "&entityname=" + CommonLogic.QueryStringCanBeDangerousContent("entityname") + "&EntityID=" + CommonLogic.QueryStringNativeInt("EntityID") + "&Variantid=" + DB.RSFieldInt(rs, "VariantID").ToString() + "\">");
                            temp += ("<img src=\"" + Image1URL + "\" height=\"35\" border=\"0\" align=\"absmiddle\">");
                            temp += ("</a>&nbsp;\n");
                        }
                        temp += ("<a href=\"" + AppLogic.AdminLinkUrl("entityEditProductVariant.aspx") + "?ProductID=" + CommonLogic.QueryStringNativeInt("ProductID") + "&entityname=" + CommonLogic.QueryStringCanBeDangerousContent("entityname") + "&EntityID=" + CommonLogic.QueryStringNativeInt("EntityID") + "&Variantid=" + DB.RSFieldInt(rs, "VariantID").ToString() + "\">" + CommonLogic.IIF(DB.RSFieldByLocale(rs, "Name", LocaleSetting).Length == 0, "(Unnamed Variant)", DB.RSFieldByLocale(rs, "Name", LocaleSetting)) + "</a>");
                        temp += ("</td>\n");
                        temp += ("<td >" + DB.RSField(rs, "SKUSuffix") + "</td>\n");
                        temp += ("<td >" + AppLogic.GetProductSKU(ProductID) + DB.RSField(rs, "SKUSuffix") + "</td>\n");
                        temp += ("<td >" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "Price")) + "</td>\n");
                        temp += ("<td >" + CommonLogic.IIF(DB.RSFieldDecimal(rs, "SalePrice") != System.Decimal.Zero, Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "SalePrice")), "&nbsp;") + "</td>\n");
                        if (ProductTracksInventoryBySizeAndColor)
                        {
                            temp += ("<td  align=\"center\"><a href=\"" + AppLogic.AdminLinkUrl("entityEditInventory.aspx") + "?productid=" + ProductID.ToString() + "&variantid=" + DB.RSFieldInt(rs, "VariantID").ToString() + "&entityname=" + CommonLogic.QueryStringCanBeDangerousContent("entityname") + "\">Click Here</a></td>\n");
                        }
                        else
                        {
                            temp += ("<td  align=\"center\">");
                            temp += (DB.RSFieldInt(rs, "Inventory").ToString());
                            temp += ("</td>");
                        }
                        temp += ("<td align=\"center\"><input size=2 type=\"text\" name=\"DisplayOrder_" + DB.RSFieldInt(rs, "VariantID").ToString() + "\" value=\"" + DB.RSFieldInt(rs, "DisplayOrder").ToString() + "\"></td>\n");
                        temp += ("<td align=\"center\"><input type=\"radio\" name=\"IsDefault\" value=\"" + DB.RSFieldInt(rs, "VariantID").ToString() + "\" " + CommonLogic.IIF(DB.RSFieldBool(rs, "IsDefault"), " checked ", "") + "></td>\n");
                        temp += ("<td align=\"center\">");
                        if (DB.RSFieldBool(rs, "IsSystem"))
                        {
                            temp += ("System<br/>Product"); // this type of product can only be deleted in the db!
                        }
                        else
                        {
                            temp += ("<input type=\"button\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.entityProducts.Clone", SkinID, LocaleSetting) + "\" name=\"Clone_" + DB.RSFieldInt(rs, "ProductID").ToString() + "\" onClick=\"if(confirm('" + AppLogic.GetString("admin.common.CreateCloneVariantQuestion", SkinID, LocaleSetting) + "')) {self.location='" + AppLogic.AdminLinkUrl("entityProductVariants.aspx") + "?ProductID=" + CommonLogic.QueryStringNativeInt("ProductID") + "&entityname=" + CommonLogic.QueryStringCanBeDangerousContent("entityname") + "&EntityID=" + CommonLogic.QueryStringNativeInt("EntityID") + "&cloneid=" + DB.RSFieldInt(rs, "VariantID").ToString() + "';}\">\n");
                        }
                        temp += ("</td>");
                        temp += ("<td align=\"center\"><input type=\"button\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Move", SkinID, LocaleSetting) + "\" name=\"Move_" + DB.RSFieldInt(rs, "VariantID").ToString() + "\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("entityMoveVariant.aspx") + "?productid=" + ProductID.ToString() + "&Variantid=" + DB.RSFieldInt(rs, "VariantID").ToString() + "'\"></td>\n");
                        temp += ("<td align=\"center\"><input type=\"button\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "\" name=\"Delete_" + DB.RSFieldInt(rs, "VariantID").ToString() + "\" onClick=\"DeleteVariant(" + DB.RSFieldInt(rs, "VariantID").ToString() + ")\"></td>\n");
                        temp += ("</tr>\n");
                        counter++;
                    }
                }
            }
            temp += ("<tr class=\"table-footer\">\n");
            temp += ("<td colspan=\"7\" align=\"left\"></td>\n");
            temp += ("<td colspan=\"2\" align=\"center\"><input type=\"submit\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"Submit\"></td>\n");
            temp += ("<td colspan=\"3\"></td>\n");
            temp += ("</tr>\n");
            temp += ("</table>\n");
            temp += ("</form>\n");
            temp += ("<script type=\"text/javascript\">\n");
            temp += ("function DeleteVariant(id)\n");
            temp += ("{\n");
            temp += ("if(confirm('" + String.Format(AppLogic.GetString("admin.common.DeleteVariantQuestion", SkinID, LocaleSetting),"+ id +"));
            temp += ("{\n");
            temp += ("self.location = '" + AppLogic.AdminLinkUrl("variants.aspx") + "?productid=" + ProductID.ToString() + "&deleteid=' + id;\n");
            temp += ("}\n");
            temp += ("}\n");
            temp += ("</SCRIPT>\n");
            ltContent.Text = temp;
        }

    }
}
