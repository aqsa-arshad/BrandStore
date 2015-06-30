// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    public partial class ContactUs : SkinBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
			this.Title = String.Format("{0} - {1}", AppLogic.AppConfig("StoreName"), AppLogic.GetString("ContactUs.PageTitle", SkinID, ThisCustomer.LocaleSetting));
			SectionTitle = AppLogic.GetString("ContactUs.PageTitle", SkinID, ThisCustomer.LocaleSetting);

            Contact c = (Contact)LoadControl("~/Controls/Contact.ascx");
            pnlContact.Controls.Add(c);
        }
    }
}
