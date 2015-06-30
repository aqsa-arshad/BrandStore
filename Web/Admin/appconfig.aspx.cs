// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using AspDotNetStorefrontCore;
using System.Collections.Generic;

namespace AspDotNetStorefrontAdmin
{
    public partial class AppConfigPage : AdminPageBase
    {
        protected override void OnInit(EventArgs e)
        {
            ctrlAppConfigList.ThisCustomer = ThisCustomer;
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SectionTitle = AppLogic.GetString("admin.sectiontitle.appconfig", SkinID, LocaleSetting);

            ctrlAppConfigList.AppRelativeTemplateSourceDirectory = AppRelativeTemplateSourceDirectory;
        }
    }
}



