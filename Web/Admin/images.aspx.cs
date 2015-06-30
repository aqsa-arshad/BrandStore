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
using System.Collections;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for images.
    /// </summary>
    public partial class images : AdminPageBase
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            SectionTitle = AppLogic.GetString("admin.sectiontitle.images", SkinID, LocaleSetting);
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            String SFP = CommonLogic.SafeMapPath("../images/spacer.gif").Replace("images\\spacer.gif", "images\\upload");

            if (CommonLogic.QueryStringCanBeDangerousContent("DeleteID").Length != 0)
            {
                // delete the image:
                System.IO.File.Delete(SFP + "/" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID"));
            }

            if (CommonLogic.FormCanBeDangerousContent("IsSubmit") == "true")
            {
                // handle upload if any also:
                HttpPostedFile Image1File = Request.Files["Image1"];
                if (Image1File.ContentLength != 0)
                {
                    String tmp = Image1File.FileName.ToLowerInvariant();
                    if (tmp.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || tmp.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase) || tmp.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (tmp.LastIndexOf('\\') != -1)
                        {
                            tmp = tmp.Substring(tmp.LastIndexOf('\\') + 1);
                        }
                        String fn = SFP + "/" + tmp;
                        Image1File.SaveAs(fn);
                    }
                }
            }


            writer.Append("<form enctype=\"multipart/form-data\" id=\"Form1\" name=\"Form1\" method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("images.aspx") + "\">\n");
            writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
            writer.Append("  <table border=\"0\" cellpadding=\"2\" border=\"0\" cellspacing=\"1\" width=\"100%\">\n");
            writer.Append("    <tr bgcolor=\"" + AppLogic.AppConfig("LightCellColor") + "\">\n");
            writer.Append("      <td class=\"tablenormal\"><b>" + AppLogic.GetString("admin.common.FileName", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td class=\"tablenormal\"><b>" + AppLogic.GetString("admin.common.ImgTagSrc", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td class=\"tablenormal\"><b>" + AppLogic.GetString("admin.common.Dimensions", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td class=\"tablenormal\"><b>" + AppLogic.GetString("admin.images.Size", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td class=\"tablenormal\"><b>" + AppLogic.GetString("admin.common.Image", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td class=\"tablenormal\"><b>" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("    </tr>\n");

            // create an array to hold the list of files
            ArrayList fArray = new ArrayList();

            // get information about our initial directory
            DirectoryInfo dirInfo = new DirectoryInfo(SFP);

            // retrieve array of files & subdirectories
            FileSystemInfo[] myDir = dirInfo.GetFileSystemInfos();

            for (int i = 0; i < myDir.Length; i++)
            {
                // check the file attributes

                // if a subdirectory, add it to the sArray    
                // otherwise, add it to the fArray
                if (((Convert.ToUInt32(myDir[i].Attributes) & Convert.ToUInt32(FileAttributes.Directory)) > 0))
                {
                    
                }
                else
                {
                    bool skipit = false;
                    if (myDir[i].FullName.StartsWith("_") || (!myDir[i].FullName.EndsWith("jpg", StringComparison.InvariantCultureIgnoreCase) && !myDir[i].FullName.EndsWith("gif", StringComparison.InvariantCultureIgnoreCase) && !myDir[i].FullName.EndsWith("png", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        skipit = true;
                    }
                    if (!skipit)
                    {
                        fArray.Add(Path.GetFileName(myDir[i].FullName));
                    }
                }
            }

            if (fArray.Count != 0)
            {
                // sort the files alphabetically
                fArray.Sort(0, fArray.Count, null);
                for (int i = 0; i < fArray.Count; i++)
                {
                    string className = "gridRowPlain";

                    if (i % 2 == 0)
                    {
                        className = "gridAlternatingRowPlain";
                    }

                    String src = "../images/upload/" + fArray[i].ToString();
                    System.Drawing.Size size = CommonLogic.GetImagePixelSize(src);
                    long s = CommonLogic.GetImageSize(src);
                    int SizeInKB = (int) s / 1000;
                    writer.Append("    <tr bgcolor=\"" + AppLogic.AppConfig("LightCellColor") + "\">\n");
                    writer.Append("      <td  class=\"" + className + "\">" + fArray[i].ToString() + "</td>\n");
                    writer.Append("      <td class=\"" + className + "\">../images/upload/" + fArray[i].ToString() + "</td>\n");
                    writer.Append("      <td class=\"" + className + "\">" + size.Width.ToString() + "x" + size.Height.ToString() + "</td>\n");
                    writer.Append("      <td class=\"" + className + "\">" + String.Format(AppLogic.GetString("admin.images.KB", SkinID, LocaleSetting), SizeInKB) + "</td>\n");
                    writer.Append("<td class=\"" + className + "\"><a target=\"_blank\" href=\"" + src + "\">\n");
                    writer.Append("<img border=\"0\" src=\"" + src + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\"" + CommonLogic.IIF(size.Height > 50, " height=\"50\"", "") + ">\n");
                    writer.Append("</a></td>\n");
                    writer.Append("      <td align=\"center\" class=\"" + className + "\"><input type=\"button\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "\" name=\"Delete_" + i.ToString() + "\" onClick=\"DeleteImage(" + CommonLogic.SQuote(fArray[i].ToString()) + ")\"></td>\n");
                    writer.Append("    </tr>\n");
                }
            }

            writer.Append("    <tr>\n");
            writer.Append("      <td colspan=\"6\" height=5></td>\n");
            writer.Append("    </tr>\n");
            writer.Append("  </table>\n");
            writer.Append("<p align=\"left\">" + AppLogic.GetString("admin.images.UploadNewImage", SkinID, LocaleSetting) + ": <input type=\"file\" name=\"Image1\" size=\"50\"><br/><input type=\"submit\" value=\"" + AppLogic.GetString("admin.common.Submit", SkinID, LocaleSetting) + "\" name=\"submit\" class=\"normalButtons\"></p>\n");
            writer.Append("</form>\n");

            writer.Append("</center></b>\n");

            writer.Append("<script type=\"text/javascript\">\n");
            writer.Append("function DeleteImage(name)\n");
            writer.Append("{\n");
            writer.Append("if(confirm('" + String.Format(AppLogic.GetString("admin.images.ConfirmDeleteImage", SkinID, LocaleSetting), "'+ name") + "))\n");
            writer.Append("{\n");
            writer.Append("self.location = '" + AppLogic.AdminLinkUrl("images.aspx") + "?deleteid=' + name;\n");
            writer.Append("}\n");
            writer.Append("}\n");
            writer.Append("</SCRIPT>\n");
            ltContent.Text = writer.ToString();
        }

    }
}
