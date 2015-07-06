// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for importProductsFromXML
    /// </summary>
    public partial class importProductsFromXML : AdminPageBase
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            Server.ScriptTimeout = 1000000;

            Page.Form.DefaultButton = btnUpload.UniqueID;

            if (!IsPostBack)
            {
                divReview.Visible = false;
            }
            Page.Form.DefaultButton = btnUpload.UniqueID;
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">" + AppLogic.GetString("admin.common.Notice", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

            if (error.Length > 0)
                str += error + "";
            else
                str = "";

            ltError.Text = str;
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            String XmlName = "Import_" + Localization.ToThreadCultureShortDateString(System.DateTime.Now).Replace(" ", "").Replace("/", "").Replace(":", "").Replace(".", "");
            // handle file upload:
            try
            {
                String Image1 = String.Empty;
                HttpPostedFile Image1File = fuFile.PostedFile;
                String XmlFile = CommonLogic.SafeMapPath("../images" + "/" + XmlName + ".xml");
                if (Image1File.ContentLength != 0)
                {
                    Image1File.SaveAs(XmlFile);
                }

                if (fuFile.FileName.Length != 0)
                {
                    resetError(AppLogic.GetString("admin.common.FileUploadedPleaseReviewBelow", SkinID, LocaleSetting), false);
                    divReview.Visible = true;
                    Import.ProcessXmlImportFile(XmlFile);
                    ltResults.Text = String.Format(AppLogic.GetString("admin.common.ViewImportLog", SkinID, LocaleSetting),"<a href=\"../images/import.htm\" target=\"_blank\">","</a>");
                }
                else
                {
                    divReview.Visible = false;
                    resetError(AppLogic.GetString("admin.importProductsFromXML.UploadError", SkinID, LocaleSetting), true);
                }

            }
            catch (Exception ex)
            {
                divReview.Visible = false;
                resetError(String.Format(AppLogic.GetString("admin.importProductsFromExcel.UploadError", SkinID, LocaleSetting), CommonLogic.GetExceptionDetail(ex, "<br/>")), true);
            }
        }

        protected void btnAccept_Click(object sender, EventArgs e)
        {
            DB.ExecuteLongTimeSQL("aspdnsf_ClearAllImportFlags", 1000);
            resetError("<span style=\"color: red; font-weight: bold;\">" + AppLogic.GetString("admin.common.ImportAcceptedUC", SkinID, LocaleSetting) + "</span>", false);
            divReview.Visible = false;
        }

        protected void btnUndo_Click(object sender, EventArgs e)
        {
            DB.ExecuteLongTimeSQL("aspdnsf_UndoImport", 1000);
            resetError("<span style=\"color: red; font-weight: bold;\">" + AppLogic.GetString("admin.common.ImportHasBeenDone", SkinID, LocaleSetting) + "</span>", false);
            divReview.Visible = false;
        }
    }
}
