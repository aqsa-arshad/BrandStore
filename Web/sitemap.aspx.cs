// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for sitemap.
    /// </summary>
    public partial class sitemap : SkinBase
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            if (AppLogic.AppConfigBool("GoNonSecureAgain"))
            {
                GoNonSecureAgain();
            }
            SectionTitle = AppLogic.GetString("sitemap.aspx.1", SkinID, ThisCustomer.LocaleSetting);
            String XmlPackageName = AppLogic.AppConfig("XmlPackage.SiteMapPage");            
            if (XmlPackageName.Length != 0)
            {
                XmlPackageControl ctrl = this.LoadControl("XmlPackageControl.ascx") as XmlPackageControl;
                ctrl.EnforceDisclaimer = true;
                ctrl.EnforcePassword = true;
                ctrl.EnforceSubscription = true;
                ctrl.AllowSEPropogation = true;
                ctrl.ThisCustomer = ThisCustomer;
                ctrl.PackageName = XmlPackageName;
                ctrl.SetContext = this;
                PackagePanel.Controls.Add(ctrl);

                PackagePanel.Visible = true;
                EntityPanel.Visible = false;
            }
            else
            {
                PackagePanel.Visible = false;
                EntityPanel.Visible = true;

                Literal1.Text = getXMLTransform();
            }

        }

        private string getXMLTransform()
        {
            System.Xml.Xsl.XslCompiledTransform xTran = new XslCompiledTransform();


            xTran.Load((Request.PhysicalApplicationPath + "/XmlPackages/SiteMap.xslt"));

            MemoryStream xmlStream = new MemoryStream();
            MemoryStream outStream = new MemoryStream();

            StreamWriter xmlWriter = new StreamWriter(xmlStream);
            xmlWriter.AutoFlush = true;
            xmlWriter.Write(new AspDotNetStorefrontCore.SiteMap(AppLogic.AppConfigBool("SiteMap.ShowCustomerService")).Contents);
            xmlStream.Position = 0;

            XmlTextReader xRdr = new XmlTextReader(xmlStream);
            xTran.Transform(xRdr, null, outStream);
            outStream.Flush();
            outStream.Position = 0;
            StreamReader outReader = new StreamReader(outStream);
            return outReader.ReadToEnd();
        }

    }
}
