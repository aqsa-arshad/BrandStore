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
using System.Web;
using System.IO;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for productimagemgr
    /// </summary>
    public partial class productimagemgr : AdminPageBase
    {

        int ProductID;
        int VariantID;
        String TheSize;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            ProductID = CommonLogic.QueryStringUSInt("ProductID");
            VariantID = CommonLogic.QueryStringUSInt("VariantID");
            TheSize = CommonLogic.QueryStringCanBeDangerousContent("Size");
            if (TheSize.Length == 0)
            {
                TheSize = "medium";
            }
            if (VariantID == 0)
            {
                VariantID = AppLogic.GetDefaultProductVariant(ProductID);
            }

            if (CommonLogic.FormBool("IsSubmit"))
            {
                String FN = ProductID.ToString();
                if (AppLogic.AppConfigBool("UseSKUForProductImageName"))
                {
                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS("select SKU from product   with (NOLOCK)  where productid=" + ProductID.ToString(), conn))
                        {
                            if (rs.Read())
                            {
                                String SKU = DB.RSField(rs, "SKU").Trim();
                                if (SKU.Length != 0)
                                {
                                    FN = SKU;
                                }
                            }
                        }
                    }
                }
                try
                {
                    for (int i = 0; i <= Request.Form.Count - 1; i++)
                    {
                        String FieldName = Request.Form.Keys[i];
                        if (FieldName.IndexOf("Key_") != -1)
                        {
                            String KeyVal = CommonLogic.FormCanBeDangerousContent(FieldName);
                            // this field should be processed
                            String[] KeyValSplit = KeyVal.Split('|');
                            int TheFieldID = Localization.ParseUSInt(KeyValSplit[0]);
                            int TheProductID = Localization.ParseUSInt(KeyValSplit[1]);
                            int TheVariantID = Localization.ParseUSInt(KeyValSplit[2]);
                            String ImageNumber = AppLogic.CleanSizeColorOption(KeyValSplit[3]);
                            String Color = AppLogic.CleanSizeColorOption(HttpContext.Current.Server.UrlDecode(KeyValSplit[4]));
                            String SafeColor = CommonLogic.MakeSafeFilesystemName(Color);
                            bool DeleteIt = (CommonLogic.FormCanBeDangerousContent("Delete_" + TheFieldID.ToString()).Length != 0);
                            if (DeleteIt)
                            {
                                System.IO.File.Delete(AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".jpg");
                                System.IO.File.Delete(AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".gif");
                                System.IO.File.Delete(AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".png");
                            }

                            String Image2 = String.Empty;
                            HttpPostedFile Image2File = Request.Files["Image" + TheFieldID.ToString()];
                            if (Image2File.ContentLength != 0)
                            {
                                // delete any current image file first
                                try
                                {
                                    System.IO.File.Delete(AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".jpg");
                                    System.IO.File.Delete(AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".gif");
                                    System.IO.File.Delete(AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".png");
                                }
                                catch
                                { }

                                String s = Image2File.ContentType;
                                switch (Image2File.ContentType)
                                {
                                    case "image/gif":
                                        Image2 = AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".gif";
                                        Image2File.SaveAs(Image2);
                                        break;
                                    case "image/x-png":
                                        Image2 = AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".png";
                                        Image2File.SaveAs(Image2);
                                        break;
                                    case "image/jpg":
                                    case "image/jpeg":
                                    case "image/pjpeg":
                                        Image2 = AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".jpg";
                                        Image2File.SaveAs(Image2);
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg += CommonLogic.GetExceptionDetail(ex, "<br/>");
                }
            }
            SectionTitle = String.Format(AppLogic.GetString("admin.sectiontitle.productimagemgr", SkinID, LocaleSetting),"<a href=\"" + AppLogic.AdminLinkUrl("editproduct.aspx") + "?productid=" + ProductID.ToString() + "\">","</a>",TheSize);
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            if (ErrorMsg.Length != 0)
            {
                writer.Append("<p align=\"left\"><b><font color=red>" + ErrorMsg + "</font></b></p>\n");
            }
            if (DataUpdated)
            {
                writer.Append("<p align=\"left\"><b><font color=blue>" + AppLogic.GetString("admin.editCreditCard.Updated", SkinID, LocaleSetting) + "</font></b></p>\n");
            }

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), conn))
                {
                    if (!rs.Read())
                    {
                        Response.Redirect(AppLogic.AdminLinkUrl("default.aspx")); // should not happen, but...
                    }

                    String ProductName = AppLogic.GetProductName(ProductID, LocaleSetting);
                    String ProductSKU = AppLogic.GetProductSKU(ProductID);
                    String VariantName = AppLogic.GetVariantName(VariantID, LocaleSetting);
                    String VariantSKU = AppLogic.GetVariantSKUSuffix(VariantID);

                    String ImageNumbers = "1,2,3,4,5,6,7,8,9,10";
                    String Colors = "," + DB.RSFieldByLocale(rs, "Colors", Localization.GetDefaultLocale()); // add an "empty" color to the first entry, to allow an image to be specified for "no color selected"
                    String[] ColorsSplit = Colors.Split(',');
                    String[] ImageNumbersSplit = ImageNumbers.Split(',');

                    writer.Append("<p align=\"left\"><b>" + String.Format(AppLogic.GetString("admin.productimagemgr.Product", SkinID, LocaleSetting),AppLogic.AdminLinkUrl("editproduct.aspx"),ProductID.ToString(),ProductName,ProductID.ToString()) + "</b></p>");
                    writer.Append("<p align=\"left\">" + String.Format(AppLogic.GetString("admin.common.ManageImageforthisProduct", SkinID, LocaleSetting),TheSize,TheSize)+"</p>\n");

                    writer.Append("<script type=\"text/javascript\">\n");
                    writer.Append("function MultiImageForm_Validator(theForm)\n");
                    writer.Append("{\n");
                    writer.Append("submitonce(theForm);\n");
                    writer.Append("return (true);\n");
                    writer.Append("}\n");
                    writer.Append("</script>\n");

                    writer.Append("<div align=\"left\">");
                    writer.Append("<form enctype=\"multipart/form-data\" action=\"" + AppLogic.AdminLinkUrl("productimagemgr.aspx") + "?size=" + TheSize + "&productid=" + ProductID.ToString() + "&VariantID=" + VariantID.ToString() + "\" method=\"post\" id=\"MultiImageForm\" name=\"MultiImageForm\" onsubmit=\"return (validateForm(this) && MultiImageForm_Validator(this))\" onReset=\"return confirm('" + AppLogic.GetString("admin.common.ResetAllFieldsPrompt", SkinID, LocaleSetting) + "');\">\n");
                    writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");

                    writer.Append("<table border=\"0\" cellspacing=\"4\" cellpadding=\"4\" border=\"1\">\n");
                    writer.Append("<tr>\n");
                    writer.Append("<td valign=\"middle\" align=\"right\"><b>" + AppLogic.GetString("admin.common.ColorImageNumber", SkinID, LocaleSetting) + "</b></td>\n");
                    for (int i = ImageNumbersSplit.GetLowerBound(0); i <= ImageNumbersSplit.GetUpperBound(0); i++)
                    {
                        writer.Append("<td valign=\"middle\" align=\"center\"><b>" + AppLogic.CleanSizeColorOption(ImageNumbersSplit[i]) + "</b></td>\n");
                    }
                    writer.Append("</tr>\n");
                    int FormFieldID = 1000; // arbitrary number
                    bool first = true;
                    for (int i = ColorsSplit.GetLowerBound(0); i <= ColorsSplit.GetUpperBound(0); i++)
                    {
                        if (ColorsSplit[i].Length == 0 && !first)
                        {
                            continue;
                        }
                        writer.Append("<tr>\n");
                        writer.Append("<td valign=\"middle\" align=\"right\"><b>" + CommonLogic.IIF(ColorsSplit[i].Length == 0, AppLogic.GetString("admin.productimagemgr.NoColorSelected", SkinID, LocaleSetting), AppLogic.CleanSizeColorOption(ColorsSplit[i])) + "</b></td>\n");
                        for (int j = ImageNumbersSplit.GetLowerBound(0); j <= ImageNumbersSplit.GetUpperBound(0); j++)
                        {
                            writer.Append("<td valign=\"bottom\" align=\"center\" bgcolor=\"#EEEEEE\">");
                            int ImgWidth = AppLogic.AppConfigNativeInt("Admin.MultiGalleryImageWidth");
                            writer.Append("<img " + CommonLogic.IIF(ImgWidth != 0, "width=\"" + ImgWidth.ToString() + "\"", "") + " src=\"" + AppLogic.LookupProductImageByNumberAndColor(ProductID, SkinID, LocaleSetting, Localization.ParseUSInt(ImageNumbersSplit[j]), ColorsSplit[i], TheSize) + "\"><br/>");
                            writer.Append("<input style=\"font-size: 9px;\" type=\"file\" name=\"Image" + FormFieldID.ToString() + "\" size=\"24\" value=\"\"><br/>\n");
                            writer.Append("<input type=\"checkbox\" name=\"Delete_" + FormFieldID.ToString() + "\"> " + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "");
                            String sColorValue = HttpContext.Current.Server.UrlEncode(AppLogic.CleanSizeColorOption(ColorsSplit[i]));
                            writer.Append("<input type=\"hidden\" name=\"Key_" + FormFieldID.ToString() + "\" value=\"" + FormFieldID.ToString() + "|" + ProductID.ToString() + "|" + VariantID.ToString() + "|" + AppLogic.CleanSizeColorOption(ImageNumbersSplit[j]) + "|" + sColorValue + "\">");
                            FormFieldID++;
                            writer.Append("</td>\n");
                        }
                        writer.Append("</tr>\n");
                        first = false;
                    }

                    writer.Append("</table>\n");
                    writer.Append("<p align=\"left\">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type=\"submit\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"submit\">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type=\"reset\" value=\"" + AppLogic.GetString("admin.common.Reset", SkinID, LocaleSetting) + "\" name=\"reset\"></p>\n");
                    writer.Append("</form>\n");
                    writer.Append("</div>");
                }
            }
            ltContent.Text = writer.ToString();
        }

    }
}
