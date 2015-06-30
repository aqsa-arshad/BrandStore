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
using System.Text;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for variants.
    /// </summary>
    public partial class variants : AdminPageBase
    {
        int ProductID = 0;
        String ProductName;
        String ProductSKU;
        bool ProductTracksInventoryBySizeAndColor;

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

            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("products.aspx") + "\">Manage Products</a> - Add/Edit Variants";

            if (CommonLogic.QueryStringCanBeDangerousContent("DeleteAllVariants").Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) == false)
            {
                AppLogic.MakeSureProductHasAtLeastOneVariant(ProductID);
            }
            AppLogic.EnsureProductHasADefaultVariantSet(ProductID);
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            writer.Append("<p align=\"left\"<b>Editing Variants for Product: <a href=\"" + AppLogic.AdminLinkUrl("editproduct.aspx") + "?productid=" + ProductID.ToString() + "\">" + ProductName + "</a> (Product SKU=" + ProductSKU + ", ProductID=" + ProductID.ToString() + ")</b>");
            writer.Append("&nbsp;&nbsp;");
            writer.Append("<a class=\"ProductNavLink\" href=\"" + AppLogic.AdminLinkUrl("editproduct.aspx") + "?productid=" + ProductID.ToString() + "\"><img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/up.gif") + "\" border=\"0\" align=\"absmiddle\"></a>");


            bool HasRecurringOrder = 0 < DB.GetSqlN("select count(*) N from ShoppingCart  with (NOLOCK)  where ProductID=" + ProductID.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString());

            if (!HasRecurringOrder)
            {
                writer.Append("&nbsp;&nbsp;");
                writer.Append("<input style=\"font-size: 9px;\" type=\"button\" class=\"normalButtons\" value=\"Delete All Variants\" name=\"DeleteAllVariants_" + ProductID.ToString() + "\" onClick=\"if(confirm('Are you sure you want to delete ALL variants for this product? This will also remove all items from customer shopping carts, wish lists, and gift registries IF they reference any of these variants!')){self.location='" + AppLogic.AdminLinkUrl("variants.aspx") + "?DeleteAllVariants=true&productid=" + ProductID.ToString() + "';}\">");
            }

            writer.Append("</p>\n");

            writer.Append("<form id=\"Form1\" name=\"Form1\" method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("variants.aspx") + "?productid=" + ProductID.ToString() + "\">\n");
            writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
            writer.Append("<table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\">\n");
            writer.Append("<tr class=\"table-header\">\n");
            writer.Append("<td><b>ID</b></td>\n");
            writer.Append("<td><b>Variant</b></td>\n");
            writer.Append("<td><b>Variant SKU Suffix</b></td>\n");
            writer.Append("<td><b>Full SKU</b></td>\n");
            writer.Append("<td><b>Price</b></td>\n");
            writer.Append("<td><b>Sale Price</b></td>\n");
            writer.Append("<td align=\"center\"><b>Inventory</b></td>\n");
            writer.Append("<td align=\"center\"><b>Display Order</b></td>\n");
            writer.Append("<td align=\"center\"><b>Is Default Variant</b></td>\n");
            writer.Append("<td align=\"center\"><b>Edit</b></td>\n");
            writer.Append("<td align=\"center\"><b>Clone</b></td>\n");
            writer.Append("<td align=\"center\"><b>Move</b></td>\n");
            writer.Append("<td align=\"center\"><b>Delete</b></td>\n");
            writer.Append("</tr>\n");

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                string sql = "select * from productvariant   with (NOLOCK)  where deleted=0 and ProductID=" + ProductID.ToString() + " order by DisplayOrder,Name";

                using (IDataReader rs = DB.GetRS(sql, conn))
                {
                    int i = 0;
                    while (rs.Read())
                    {
                        if (i % 2 == 0)
                        {
                            writer.Append("<tr class=\"table-row2\">\n");
                        }
                        else
                        {
                            writer.Append("<tr class=\"table-alternatingrow2\">\n");
                        }
                        writer.Append("<td >" + DB.RSFieldInt(rs, "VariantID").ToString() + "</td>\n");
                        writer.Append("<td >");
                        String Image1URL = AppLogic.LookupImage("Variant", DB.RSFieldInt(rs, "VariantID"), "icon", SkinID, LocaleSetting);
                        if (Image1URL.Length != 0)
                        {
                            writer.Append("<a href=\"" + AppLogic.AdminLinkUrl("editVariant.aspx") + "?Variantid=" + DB.RSFieldInt(rs, "VariantID").ToString() + "\">");
                            writer.Append("<img src=\"" + Image1URL + "\" height=\"35\" border=\"0\" align=\"absmiddle\">");
                            writer.Append("</a>&nbsp;\n");
                        }
                        writer.Append("<a href=\"" + AppLogic.AdminLinkUrl("editVariant.aspx") + "?productid=" + ProductID.ToString() + "&Variantid=" + DB.RSFieldInt(rs, "VariantID").ToString() + "\">" + CommonLogic.IIF(DB.RSFieldByLocale(rs, "Name", LocaleSetting).Length == 0, "(Unnamed Variant)", DB.RSFieldByLocale(rs, "Name", LocaleSetting)) + "</a>");
                        writer.Append("</td>\n");
                        writer.Append("<td >" + DB.RSField(rs, "SKUSuffix") + "</td>\n");
                        writer.Append("<td >" + AppLogic.GetProductSKU(ProductID) + DB.RSField(rs, "SKUSuffix") + "</td>\n");
                        writer.Append("<td >" + ThisCustomer.CurrencyString(DB.RSFieldDecimal(rs, "Price")) + "</td>\n");
                        writer.Append("<td >" + CommonLogic.IIF(DB.RSFieldDecimal(rs, "SalePrice") != System.Decimal.Zero, ThisCustomer.CurrencyString(DB.RSFieldDecimal(rs, "SalePrice")), "&nbsp;") + "</td>\n");
                        if (ProductTracksInventoryBySizeAndColor)
                        {
                            writer.Append("<td  align=\"center\"><a href=\"" + AppLogic.AdminLinkUrl("editinventory.aspx") + "?productid=" + ProductID.ToString() + "&variantid=" + DB.RSFieldInt(rs, "VariantID").ToString() + "\">Click Here</a></td>\n");
                        }
                        else
                        {
                            writer.Append("<td  align=\"center\">");
                            writer.Append(DB.RSFieldInt(rs, "Inventory").ToString());
                            writer.Append("</td>");
                        }
                        writer.Append("<td align=\"center\"><input size=2 type=\"text\" name=\"DisplayOrder_" + DB.RSFieldInt(rs, "VariantID").ToString() + "\" value=\"" + DB.RSFieldInt(rs, "DisplayOrder").ToString() + "\"></td>\n");
                        writer.Append("<td align=\"center\"><input type=\"radio\" name=\"IsDefault\" value=\"" + DB.RSFieldInt(rs, "VariantID").ToString() + "\" " + CommonLogic.IIF(DB.RSFieldBool(rs, "IsDefault"), " checked ", "") + "></td>\n");
                        writer.Append("<td align=\"center\"><input type=\"button\" class=\"normalButtons\" value=\"Edit\" name=\"Edit_" + DB.RSFieldInt(rs, "VariantID").ToString() + "\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("editVariant.aspx") + "?productid=" + ProductID.ToString() + "&Variantid=" + DB.RSFieldInt(rs, "VariantID").ToString() + "'\"></td>\n");
                        writer.Append("<td align=\"center\">");
                        if (DB.RSFieldBool(rs, "IsSystem"))
                        {
                            writer.Append("System<br/>Product"); // this type of product can only be deleted in the db!
                        }
                        else
                        {
                            writer.Append("<input type=\"button\" value=\"Clone\" class=\"normalButtons\" name=\"Clone_" + DB.RSFieldInt(rs, "ProductID").ToString() + "\" onClick=\"if(confirm('Are you sure you want to create a clone of this variant?')) {self.location='" + AppLogic.AdminLinkUrl("variants.aspx") + "?productid=" + ProductID.ToString() + "&cloneid=" + DB.RSFieldInt(rs, "VariantID").ToString() + "';}\">\n");
                        }
                        writer.Append("</td>");

                        if (HasRecurringOrder && 0 < DB.GetSqlN("select count(*) N from ShoppingCart  with (NOLOCK)  where VariantID=" + DB.RSFieldInt(rs, "VariantID").ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()))
                        {
                            writer.Append("<td align=\"center\"> </td>\n");
                            writer.Append("<td align=\"center\"> </td>\n");
                        }
                        else
                        {
                            writer.Append("<td align=\"center\"><input type=\"button\" class=\"normalButtons\" value=\"Move\" name=\"Move_" + DB.RSFieldInt(rs, "VariantID").ToString() + "\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("moveVariant.aspx") + "?productid=" + ProductID.ToString() + "&Variantid=" + DB.RSFieldInt(rs, "VariantID").ToString() + "'\"></td>\n");
                            writer.Append("<td align=\"center\"><input type=\"button\" class=\"normalButtons\" value=\"Delete\" name=\"Delete_" + DB.RSFieldInt(rs, "VariantID").ToString() + "\" onClick=\"DeleteVariant(" + DB.RSFieldInt(rs, "VariantID").ToString() + ")\"></td>\n");
                        }
                        writer.Append("</tr>\n");
                        i++;
                    }
                }
            }
       
            writer.Append("<tr>\n");
            writer.Append("<td colspan=\"7\" align=\"left\"></td>\n");
            writer.Append("<td colspan=\"2\" align=\"center\" ><input type=\"submit\" class=\"normalButtons\" value=\"Update\" name=\"Submit\"></td>\n");
            writer.Append("<td colspan=\"3\"></td>\n");
            writer.Append("</tr>\n");
            writer.Append("</table>\n");
            writer.Append("<input type=\"button\" class=\"normalButtons\" value=\"Add New Variant\" name=\"AddNew\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("editVariant.aspx") + "?productid=" + ProductID.ToString() + "';\">\n");

            writer.Append("</form>\n");

            writer.Append("<script type=\"text/javascript\">\n");
            writer.Append("function DeleteVariant(id)\n");
            writer.Append("{\n");
            writer.Append("if(confirm('Are you sure you want to delete Variant: ' + id + '? This will also remove all items from customer shopping carts, wish lists, and gift registries IF they reference this variant!'))\n");
            writer.Append("{\n");
            writer.Append("self.location = '" + AppLogic.AdminLinkUrl("variants.aspx") + "?productid=" + ProductID.ToString() + "&deleteid=' + id;\n");
            writer.Append("}\n");
            writer.Append("}\n");
            writer.Append("</SCRIPT>\n");
            ltContent.Text = writer.ToString();
        }

    }
}
