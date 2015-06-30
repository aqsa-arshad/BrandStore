// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for lat_driver.
	/// </summary>
	public partial class lat_driver : SkinBase
	{
        int AffiliateID = 0;
		protected void Page_Load(object sender, System.EventArgs e)
		{
            RequireSecurePage();
            AffiliateID = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(Profile.LATAffiliateID), Profile.LATAffiliateID, "0"));            

            if (AffiliateID == 0 || !AppLogic.IsValidAffiliate(AffiliateID))
            {
                Response.Redirect("lat_signin.aspx?returnurl=" + Server.UrlEncode(CommonLogic.GetThisPageName(true) + "?" + CommonLogic.ServerVariables("QUERY_STRING")));
            }

            if (!IsPostBack)
            {
                UpdatePageContent();
            }
        }

        private void UpdatePageContent()
        {
            AskAQuestion.NavigateUrl = "mailto:" + AppLogic.AppConfig("AffiliateEMailAddress");

            String TN = CommonLogic.QueryStringCanBeDangerousContent("topic");
            AppLogic.CheckForScriptTag(TN);
            Topic t = new Topic(TN,ThisCustomer.LocaleSetting,SkinID,base.GetParser);
			if(t.Contents.Length == 0)
			{
				PageTopic.Text = "<img src=\"images/spacer.gif\" border=\"0\" height=\"100\" width=\"1\">\n";
				PageTopic.Text += "<p align=\"center\"><font class=\"big\"><b>This page is currently empty. Please check back again for an update.</b></font></p>";
			}
			else
			{
                PageTopic.Text = "<!-- READ FROM " + CommonLogic.IIF(t.FromDB, "DB", "FILE") + ": " + " -->";
				PageTopic.Text += t.Contents.Replace("%AFFILIATEID%",AffiliateID.ToString());
                PageTopic.Text += "<!-- END OF " + CommonLogic.IIF(t.FromDB, "DB", "FILE") + ": " + " -->";
			}
            
        }
	}
}
