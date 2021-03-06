// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for search.
    /// </summary>
    [PageType("search")]
    public partial class search : SkinBase
    {
        protected string IsProductExist = string.Empty;
        protected override void OnInit(EventArgs e)
        {
            if (AppLogic.AppConfigBool("GoNonSecureAgain"))
            {
                GoNonSecureAgain();
            }

            // this may be overwridden by the XmlPackage below!
            SectionTitle = AppLogic.GetString("search.aspx.1", SkinID, ThisCustomer.LocaleSetting);

            int minSearchLength = AppLogic.AppConfigNativeInt("MinSearchStringLength");

            string searchTermFromQueryString = CommonLogic.QueryStringCanBeDangerousContent("SearchTerm");
            if (minSearchLength <= searchTermFromQueryString.Length)
            {
                RunSearch(Server.UrlEncode(searchTermFromQueryString));
            }
            else
            {
                litSearch.Text = String.Format(AppLogic.GetString("search.aspx.2", ThisCustomer.LocaleSetting), minSearchLength);
            }

            base.OnInit(e);
        }

        private void RunSearch(string searchTerm)
        {
            if (searchTerm.Length != 0)
            {
                DB.ExecuteSQL("insert into SearchLog(SearchTerm,CustomerID,LocaleSetting) values(" + DB.SQuote(CommonLogic.Ellipses(searchTerm, 97, true)) + "," + ThisCustomer.CustomerID.ToString() + "," + DB.SQuote(ThisCustomer.LocaleSetting) + ")");
            }

            string searchXmlPackageName = AppLogic.AppConfig("XmlPackage.SearchPage");
            string searchHTML = AppLogic.RunXmlPackage(searchXmlPackageName,
                                    null,
                                    ThisCustomer,
                                    ThisCustomer.SkinID,
                                    string.Empty,
                                    string.Format("SearchTerm={0}", searchTerm),
                                    true,
                                    false);

            IsProductExist = searchHTML.Contains("Your search did not result in any matches") ? "false" : "true";
            litSearch.Text = searchHTML;            
        }
        protected override string OverrideTemplate()
        {
            var masterHome = AppLogic.HomeTemplate();
            if (masterHome.Trim().Length == 0)
            {
                masterHome = "JeldWenTemplate";
            }
            if (masterHome.EndsWith(".ascx"))
            {
                masterHome = masterHome.Replace(".ascx", ".master");
            }
            if (!masterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
            {
                masterHome = masterHome + ".master";
            }
            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + SkinID + "/" + masterHome)))
            {
                masterHome = "JeldWenTemplate";
            }
            return masterHome;
        }

    }
}
