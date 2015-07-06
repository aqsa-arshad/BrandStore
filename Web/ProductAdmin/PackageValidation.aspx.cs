// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Schema;
using System.Xml;
using AspDotNetStorefrontCore;
namespace AspDotNetStorefrontAdmin
{
    public partial class PackageValidation : AdminPageBase
    {
        int intValidErrors = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            ltError.Text = "";
        }

        private void ValidatePackageX(string XmlPackagepath)
        {
            intValidErrors = 0;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas.Add("", Server.MapPath("../Xmlpackages/xmlpackage.xsd"));
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationHandler);

            XmlReader xmlReaderObj = XmlReader.Create(XmlPackagepath, settings);

            try
            {
                while (xmlReaderObj.Read()) { }
                ltError.Text = String.Format(AppLogic.GetString("admin.PackageValidation.XMLValidated", SkinID, LocaleSetting),intValidErrors,ltError.Text);
            }
            catch (Exception ex)
            {
                ltError.Text += String.Format(AppLogic.GetString("admin.PackageValidation.ParserError", SkinID, LocaleSetting),ex.Message);
            }
            xmlReaderObj.Close();
        }


        private void ValidationHandler(object sender, ValidationEventArgs args)
        {
            //event handler called when a validation error is found
            intValidErrors += 1;   //increment count of errors

            //check the severity of the error
            string strSeverity = "";
            if (args.Severity == XmlSeverityType.Error) strSeverity = "Error";
            if (args.Severity == XmlSeverityType.Warning) strSeverity = "Warning";

            //display a message
            ltError.Text += String.Format(AppLogic.GetString("admin.PackageValidation.ValidationError", SkinID, LocaleSetting),args.Message);
            if (args.Exception.LineNumber > 0)
            {
                ltError.Text += String.Format(AppLogic.GetString("admin.PackageValidation.LineCharacter", SkinID, LocaleSetting),args.Exception.LineNumber.ToString(),args.Exception.LinePosition.ToString());
            }
            ltError.Text += String.Format(AppLogic.GetString("admin.PackageValidation.SeverityLevel", SkinID, LocaleSetting),strSeverity);
        }



        protected void validatepackage_Click(object sender, EventArgs e)
        {
            bool inputvalid = false;

            String TargetFile = CommonLogic.SafeMapPath("../images" + "/" + ThisCustomer.CustomerGUID + DateTime.Now.Millisecond.ToString() + ".xml.config");
            StreamWriter sw = null;

            try
            {
                HttpPostedFile XmlPackageFile = xmlpackage.PostedFile;
                if (XmlPackageFile != null && XmlPackageFile.ContentLength != 0)
                {
                    XmlPackageFile.SaveAs(TargetFile);
                    inputvalid = true;
                }
                if (xmltext.Text.Trim().Length != 0)
                {
                    using (sw = new StreamWriter(TargetFile))
                    {
                        // Add some text to the file.
                        sw.Write(xmltext.Text);
                        sw.Close();
                    }
                    inputvalid = true;
                }

                if (inputvalid)
                {
                    ValidatePackageX(TargetFile);
                }
                else
                {
                    ltError.Text = "<font color=\"red\"><b>" + AppLogic.GetString("admin.PackageValidation.NoValidInput", SkinID, LocaleSetting) + "</b></font>";
                }
            }
            catch (Exception ex)
            {
                if (sw != null)
                {
                    sw.Close();
                }
                ltError.Text += "<br />" + CommonLogic.GetExceptionDetail(ex, "<br/>");
            }
            try { File.Delete(TargetFile); }
            catch { }
            ltError.Text = String.Format(AppLogic.GetString("admin.PackageValidation.Output", SkinID, LocaleSetting),ltError.Text);
        }
    }
}
