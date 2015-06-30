// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Globalization;
using AspDotNetStorefrontCommon;
using AspDotNetStorefrontCore;
using Vortx.MobileFramework;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for search.
    /// </summary>
    public partial class mobilesearch : SkinBase
    {
        protected override void OnInit(EventArgs e)
        {
            MobileHelper.RedirectPageWhenMobileIsDisabled("~/search.aspx", ThisCustomer);

            if (AppLogic.AppConfigBool("GoNonSecureAgain"))
            {
                GoNonSecureAgain();
            }

            // this may be overwridden by the XmlPackage below!
            SectionTitle = AppLogic.GetString("search.aspx.1", SkinID, ThisCustomer.LocaleSetting);

            string searchTermFromQueryString = CommonLogic.QueryStringCanBeDangerousContent("SearchTerm");
            if (!CommonLogic.IsStringNullOrEmpty(searchTermFromQueryString) && 
                AppLogic.AppConfigUSInt("MinSearchStringLength") <= searchTermFromQueryString.Length)
            {
                //ctrlPageSearch.SearchText = searchTermFromQueryString;
                RunSearch(searchTermFromQueryString);
            }
           
            base.OnInit(e);
        }

        private void RunSearch(string searchTerm)
        {
            if (searchTerm.Length != 0)
            {
                DB.ExecuteSQL("insert into SearchLog(SearchTerm,CustomerID,LocaleSetting) values(" + DB.SQuote(CommonLogic.Ellipses(searchTerm, 97, true)) + "," + ThisCustomer.CustomerID.ToString() + "," + DB.SQuote(ThisCustomer.LocaleSetting) + ")");
            }

            string searchHTML = AppLogic.RunXmlPackage("mobile.page.search.xml.config",
                                    null,
                                    ThisCustomer,
                                    ThisCustomer.SkinID,
                                    string.Empty,
                                    string.Format("SearchTerm={0}", searchTerm),
                                    true,
                                    false);

            litSearch.Text = searchHTML;
        }
        
    }
}
