// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Globalization;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for setssl.
    /// </summary>
    public partial class setssl : AdminPageBase
    {
        private void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            SectionTitle = "SetSSL On/Off";
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();  
            if (ThisCustomer.IsAdminSuperUser)
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("set").ToUpper(CultureInfo.InvariantCulture) == "TRUE")
                {

                    bool OkToUseSSL = false;
                    String WorkerWindowInSSL = String.Empty;
                    String TestSSLUrl = AppLogic.GetStoreHTTPLocation(false).Replace("http://", "https://") + "empty.htm";
                    writer.Append("Testing URL: " + TestSSLUrl + "...<br/>");
                    try
                    {
                        WorkerWindowInSSL = CommonLogic.AspHTTP(TestSSLUrl,30);
                    }
                    catch (Exception ex)
                    {
                        writer.Append("Failed: " + CommonLogic.GetExceptionDetail(ex, "<br/>") + "<br/>");
                    }
                    writer.Append("Worker Window Contents: <textarea rows=\"10\" cols=\"60\">" + WorkerWindowInSSL + "</textarea><br/>");
                    if (WorkerWindowInSSL.IndexOf("Worker") != -1)
                    {
                        OkToUseSSL = true;
                    }
                    if (OkToUseSSL)
                    {
                        AppLogic.SetAppConfig("UseSSL", "true");
                        writer.Append("SSL ON");
                    }
                    else
                    {
                        AppLogic.SetAppConfig("UseSSL", "false");
                        writer.Append("<font color=\"red\"><b>No SSL certificate was found on your site. Please check with your hosting company! You must be able to invoke your store site using https:// before turning SSL on in this admin site!</b></font>");
                    }
                }
                else
                {
                    AppLogic.SetAppConfig("UseSSL", "false");
                    writer.Append("SSL OFF");
                }
            }
            ltContent.Text = writer.ToString();
        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Load += new System.EventHandler(Page_Load);
        }
        #endregion
    }
}
