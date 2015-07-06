// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Collections;
using System.Text;
using System.Web;
using System.IO;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for productimagemgr
    /// </summary>
    public partial class EntityProductImageMgr : System.Web.UI.Page
    {
        string ErrorMsg;
        int ProductID;
        int VariantID;
        String TheSize;
        Customer ThisCustomer;
		private int currentSkinID = 1;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            ErrorMsg = "";
            ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

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
                    using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                    {
                        dbconn.Open();
                        using( IDataReader rs = DB.GetRS("select SKU from product   with (NOLOCK)  where productid=" + ProductID.ToString(),dbconn))
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
                                System.IO.File.Delete(AppLogic.GetImagePath("Product", "micro", true) + FN + "_" + ImageNumber.ToLowerInvariant() + ".jpg");
                                
                            }

                            String Image2 = String.Empty;
                            String TempImage2 = String.Empty;
                            String ContentType = String.Empty;
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
                                            TempImage2 = AppLogic.GetImagePath("Product", TheSize, true) + "tmp_" + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".gif";
                                            Image2 = AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".gif";
                                            Image2File.SaveAs(TempImage2);
                                            AppLogic.ResizeEntityOrObject("Product", TempImage2, Image2, TheSize, "image/gif");
                                            ContentType = "image/gif";
                                        break;
                                    case "image/x-png":
                                    case "image/png":
                                            TempImage2 = AppLogic.GetImagePath("Product", TheSize, true) + "tmp_" + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".png";
                                            Image2 = AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".png";
                                            Image2File.SaveAs(TempImage2);
                                            AppLogic.ResizeEntityOrObject("Product", TempImage2, Image2, TheSize, "image/png");
                                            ContentType = "image/png";
                                        break;
                                    case "image/jpg":
                                    case "image/jpeg":
                                    case "image/pjpeg":
                                            TempImage2 = AppLogic.GetImagePath("Product", TheSize, true) + "tmp_" + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".jpg";
                                            Image2 = AppLogic.GetImagePath("Product", TheSize, true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ".jpg";
                                            Image2File.SaveAs(TempImage2);
                                            AppLogic.ResizeEntityOrObject("Product", TempImage2, Image2, TheSize, "image/jpeg");
                                            ContentType = "image/jpeg";
                                        break;
                                }


                                // lets try and create the other multi images if using the large multi image manager
                                if (TheSize == "large")
                                {
                                    AppLogic.MakeOtherMultis(FN, ImageNumber, SafeColor, TempImage2, ContentType);
                                }
                                else if (AppLogic.AppConfigBool("MultiMakesMicros") && TheSize == "medium" && SafeColor == "")
                                {
                                    // lets create micro images if using the medium multi image manager
                                    // since the medium icons are what show on the product pages
									AppLogic.MakeMicroPic(FN, TempImage2, ImageNumber);
                                }

                                // delete the temp image
                                AppLogic.DisposeOfTempImage(TempImage2);
                               
                            }
                        }
                    }
                    ErrorMsg += "Image(s) updated. <a href=\"javascript:;\" onclick=\"window.close();\">Close window</a>.";
                }
                catch (Exception ex)
                {
                    ErrorMsg += CommonLogic.GetExceptionDetail(ex, "<br/>");
                }
                String variantColors = String.Empty;

                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using(  IDataReader rsColors = DB.GetRS("select Colors from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(),dbconn))
                    {
                        if (rsColors.Read())
                        {
                            variantColors = DB.RSFieldByLocale(rsColors, "Colors", Localization.GetDefaultLocale());
                        }
                    }
                }
                if (AppLogic.AppConfigBool("MultiColorMakesSwatchAndMap") && variantColors.Length > 0 && TheSize.ToUpperInvariant() != "ICON")
                {
					AppLogic.MakeColorSwatch(ProductID, currentSkinID, ThisCustomer.LocaleSetting, variantColors, FN);
                }
                
            }
            this.LoadData();
        }

        protected void LoadData()
        {
            string temp = "";

            if (ErrorMsg.Length != 0)
            {
                temp += ("<p align=\"left\"><b><font color=red>" + ErrorMsg + "</font></b></p>\n");
            }

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using(IDataReader rs = DB.GetRS("select * from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(),dbconn))
                {
                    if (!rs.Read())
                    {
                        Response.Redirect("splash.aspx"); // should not happen, but...
                    }

                    String ProductName = AppLogic.GetProductName(ProductID, ThisCustomer.LocaleSetting);
                    String ProductSKU = AppLogic.GetProductSKU(ProductID);
                    String VariantName = AppLogic.GetVariantName(VariantID, ThisCustomer.LocaleSetting);
                    String VariantSKU = AppLogic.GetVariantSKUSuffix(VariantID);

                    String ImageNumbers = "1,2,3,4,5,6,7,8,9,10";
                    String Colors = "," + DB.RSFieldByLocale(rs, "Colors", Localization.GetDefaultLocale()); // add an "empty" color to the first entry, to allow an image to be specified for "no color selected"
                    String[] ColorsSplit = Colors.Split(',');
                    String[] ImageNumbersSplit = ImageNumbers.Split(',');

                    temp += ("<p align=\"left\"><b>PRODUCT: <a href=\"javascript:window.close();\">" + ProductName + " (ProductID=" + ProductID.ToString() + ")</a></b></p>");
                    temp += ("<p align=\"left\">Manage (" + TheSize + ") images for this product by image # and color. You can have up to 10 images for a product, and an image for each color, so this forms a 2 dimensional grid if images: image number x color. Each slot can have a separate picture. You should also load the " + TheSize + " image pic on the editproduct page...that image is used by default for most page displays. These images are only used on the product page, when the user actively selects a different image number icon and/or color selection.</p>\n");

                    temp += ("<script type=\"text/javascript\">\n");
                    temp += ("function MultiImageForm_Validator(theForm)\n");
                    temp += ("{\n");
                    temp += ("submitonce(theForm);\n");
                    temp += ("return (true);\n");
                    temp += ("}\n");
                    temp += ("</script>\n");

                    temp += ("<div align=\"left\">");
                    temp += ("<form enctype=\"multipart/form-data\" action=\"EntityProductImageMgr.aspx?size=" + TheSize + "&productid=" + ProductID.ToString() + "&VariantID=" + VariantID.ToString() + "\" method=\"post\" id=\"MultiImageForm\" name=\"MultiImageForm\" onsubmit=\"return (validateForm(this) && MultiImageForm_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");
                    temp += ("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");

                    temp += ("<table border=\"0\" cellspacing=\"4\" cellpadding=\"4\" border=\"1\">\n");
                    temp += ("<tr>\n");
                    temp += ("<td valign=\"middle\" align=\"right\"><b>Color\\Image#</b></td>\n");
                    for (int i = ImageNumbersSplit.GetLowerBound(0); i <= ImageNumbersSplit.GetUpperBound(0); i++)
                    {
                        temp += ("<td valign=\"middle\" align=\"center\"><b>" + AppLogic.CleanSizeColorOption(ImageNumbersSplit[i]) + "</b></td>\n");
                    }
                    temp += ("</tr>\n");
                    int FormFieldID = 1000; // arbitrary number
                    bool first = true;
                    for (int i = ColorsSplit.GetLowerBound(0); i <= ColorsSplit.GetUpperBound(0); i++)
                    {
                        if (ColorsSplit[i].Length == 0 && !first)
                        {
                            continue;
                        }
                        temp += ("<tr>\n");
                        temp += ("<td valign=\"middle\" align=\"right\"><b>" + CommonLogic.IIF(ColorsSplit[i].Length == 0, "(No Color Selected)", AppLogic.CleanSizeColorOption(ColorsSplit[i])) + "</b></td>\n");
                        for (int j = ImageNumbersSplit.GetLowerBound(0); j <= ImageNumbersSplit.GetUpperBound(0); j++)
                        {
                            temp += ("<td valign=\"bottom\" align=\"center\" bgcolor=\"#EEEEEE\">");
                            int ImgWidth = AppLogic.AppConfigNativeInt("Admin.MultiGalleryImageWidth");
							temp += ("<img " + CommonLogic.IIF(ImgWidth != 0, "width=\"" + ImgWidth.ToString() + "\"", "") + " src=\"" + AppLogic.LookupProductImageByNumberAndColor(ProductID, currentSkinID, ThisCustomer.LocaleSetting, Localization.ParseUSInt(ImageNumbersSplit[j]), AppLogic.CleanSizeColorOption(ColorsSplit[i]), TheSize) + "\"><br/>");
                            temp += ("<input style=\"font-size: 9px;\" type=\"file\" name=\"Image" + FormFieldID.ToString() + "\" size=\"24\" value=\"\"><br/>\n");
                            temp += ("<input type=\"checkbox\" name=\"Delete_" + FormFieldID.ToString() + "\"> <small>Delete</small>");
                            String sColorValue = HttpContext.Current.Server.UrlEncode(AppLogic.CleanSizeColorOption(ColorsSplit[i]));
                            temp += ("<input type=\"hidden\" name=\"Key_" + FormFieldID.ToString() + "\" value=\"" + FormFieldID.ToString() + "|" + ProductID.ToString() + "|" + VariantID.ToString() + "|" + AppLogic.CleanSizeColorOption(ImageNumbersSplit[j]) + "|" + sColorValue + "\">");
                            FormFieldID++;
                            temp += ("</td>\n");
                        }
                        temp += ("</tr>\n");
                        first = false;
                    }

                    temp += ("</table>\n");
                    temp += ("<p align=\"left\">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input class=\"normalButtons\" type=\"submit\" value=\"Update\" name=\"submit\">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input class=\"normalButtons\" type=\"reset\" value=\"Reset\" name=\"reset\"></p>\n");
                    temp += ("</form>\n");
                    temp += ("</div>");
                }
            }
            ltContent.Text = temp;
        }

    }
}
