// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Xml;
using System.Xml.Xsl;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for importProductPricing
    /// </summary>
    public partial class importProductPricing : AdminPageBase
    {
        private Customer cust;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            Server.ScriptTimeout = 1000000;

            cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            if (!IsPostBack)
            {
                trResults.Visible = false;
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
            trResults.Visible = true;
            string errors = "";

            if (fuFile.HasFile)
            {
                HttpPostedFile PostedFile = fuFile.PostedFile;
                if (!PostedFile.FileName.EndsWith("xls", StringComparison.InvariantCultureIgnoreCase) && !PostedFile.FileName.EndsWith("xml", StringComparison.InvariantCultureIgnoreCase) && !PostedFile.FileName.EndsWith("csv", StringComparison.InvariantCultureIgnoreCase) && PostedFile.FileName.Trim() != "")
                {
                    errors = String.Format(AppLogic.GetString("admin.importProductPricing.InvalidFileType", SkinID, LocaleSetting),CommonLogic.IIF(PostedFile.ContentLength == 0, AppLogic.GetString("admin.common.FileContentsWereEmpty", SkinID, LocaleSetting), AppLogic.GetString("admin.common.CommaDelimitedFilesPrompt", SkinID, LocaleSetting)));
                }
                else
                {
                    string filename = System.Guid.NewGuid().ToString();
                    string FullFilePath = CommonLogic.SafeMapPath("../images") + "\\" + filename + PostedFile.FileName.ToLowerInvariant().Substring(PostedFile.FileName.LastIndexOf('.'));
                    string xml = String.Empty;

                    PostedFile.SaveAs(FullFilePath);
                    StreamReader sr = new StreamReader(FullFilePath);
                    string filecontent = sr.ReadToEnd();
                    sr.Close();

                    if (PostedFile.FileName.EndsWith("csv", StringComparison.InvariantCultureIgnoreCase))
                    {
                        xml = "<productlist>";
                        string[] rows = filecontent.Split(Environment.NewLine.ToCharArray());
                        for (int i = 1; i < rows.Length; i++)
                        {
                            if (rows[i].Length > 0)
                            {
                                xml += "<productvariant>";
                                string delim = ",";
                                string[] cols = rows[i].Split(delim.ToCharArray());
                                xml += "<ProductID>" + cols[0] + "</ProductID>";
                                xml += "<VariantID>" + cols[1] + "</VariantID>";
                                xml += "<KitItemID>" + cols[2] + "</KitItemID>";
                                xml += "<Name>" + cols[3] + "</Name>";
                                xml += "<KitGroup>" + cols[4] + "</KitGroup>";
                                xml += "<SKU>" + cols[5] + "</SKU>";
                                xml += "<SKUSuffix>" + cols[7] + "</SKUSuffix>";
                                xml += "<ManufacturerPartNumber>" + cols[6] + "</ManufacturerPartNumber>";
                                xml += "<Cost>" + cols[8] + "</Cost>";
                                xml += "<MSRP>" + cols[9] + "</MSRP>";
                                xml += "<Price>" + cols[10] + "</Price>";
                                xml += "<SalePrice>" + cols[11] + "</SalePrice>";
                                xml += "<Inventory>" + cols[12] + "</Inventory>";
                                xml += "</productvariant>";
                            }
                        }
                        xml += "</productlist>";
                    }
                    else if (PostedFile.FileName.EndsWith("xls", StringComparison.InvariantCultureIgnoreCase))
                    {
                        xml = Import.ConvertPricingFileToXml(FullFilePath);
                        XslCompiledTransform xForm = new XslCompiledTransform();
                        xForm.Load(CommonLogic.SafeMapPath("XmlPackages/ExcelPricingImport.xslt"));
                        Localization ExtObj = new Localization();
                        XsltArgumentList m_TransformArgumentList = new XsltArgumentList();
                        m_TransformArgumentList.AddExtensionObject("urn:aspdnsf", ExtObj);
                        XmlDocument xdoc = new XmlDocument();
                        xdoc.LoadXml(xml);
                        StringWriter xsw = new StringWriter();
                        xForm.Transform(xdoc, m_TransformArgumentList, xsw);
                        xml = xsw.ToString();
                    }
                    else
                    {
                        xml = filecontent;
                    }
                    File.Delete(FullFilePath);
                    errors = AppLogic.ImportProductList(xml);
                }
            }
            else
            {
                errors = (AppLogic.GetString("admin.importProductPricing.NothingToImport", SkinID, LocaleSetting));
            }

            if (errors.Length == 0)
            {
                ltResult.Text = (AppLogic.GetString("admin.importProductPricing.ImportOK", SkinID, LocaleSetting));
            }
            else
            {
                ltResult.Text = (String.Format(AppLogic.GetString("admin.importProductPricing.ImportError", SkinID, LocaleSetting), errors ));
            }
        }
    }
}
