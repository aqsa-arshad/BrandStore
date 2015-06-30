// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;
using AjaxControlToolkit;
using AspDotNetStorefront;
using System.Reflection;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class AppConfigInfo : BaseUserControl<AppConfig>
    {
        public String AppConfigName;
        private AppConfig _AppConfig
        {
            get
            {
                return AppLogic.GetAppConfig(0, AppConfigName);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            imgAppConfigInfo.Attributes.Add("title", Server.HtmlEncode(_AppConfig.Description));
        }
    }
}


