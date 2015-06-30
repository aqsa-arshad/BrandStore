// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using AspDotNetStorefrontCore;
public partial class Admin_DownloadLicenseFile : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        DownloadLicenseFile(sender, e);
    }

    protected void DownloadLicenseFile(object sender, EventArgs e)
    {
        
        Response.ContentType = "application/octet-stream";
        Response.AddHeader("Content-Disposition", "attachment; filename=license.licx");
        Response.Write(AppLogic.GlobalConfig("LicenseKey"));
        

        Response.Flush();
        Response.Close();
    }
}
