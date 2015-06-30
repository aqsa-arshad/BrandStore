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
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for searchadv.
	/// </summary>
	public partial class searchadv : SkinBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			string searchXmlPackageName = AppLogic.AppConfig("XmlPackage.SearchAdvPage");
			searchPackage.PackageName = String.IsNullOrEmpty(searchXmlPackageName) ? "page.searchadv.xml.config" : searchXmlPackageName;

			if(AppLogic.AppConfigBool("GoNonSecureAgain"))
			{
				GoNonSecureAgain();
			}
            SectionTitle =  AppLogic.GetString("searchadv.aspx.1",SkinID,ThisCustomer.LocaleSetting);

            String st = CommonLogic.QueryStringCanBeDangerousContent("SearchTerm").Trim();
            if (st.Length != 0)
            {
                DB.ExecuteSQL("insert into SearchLog(SearchTerm,CustomerID,LocaleSetting) values(" + DB.SQuote(CommonLogic.Ellipses(st, 97, true)) + "," + ThisCustomer.CustomerID.ToString() + "," + DB.SQuote(ThisCustomer.LocaleSetting) + ")");
            }

			searchPackage.SetContext = this;

		}
	}
}
