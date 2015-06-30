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
using System.Text;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class EditAppConfigStoresAtom : UserControl
    {
        private AppConfig _DataSource;
        private bool _DataSourceExists = false;
        private AppConfig _PassedConfig;

        public AppConfig DataSource
        {
            get
            {
                return _DataSource;
            }
            set
            {
                _PassedConfig = value;
                _DataSourceExists = AppConfigManager.AppConfigExists(0, value.Name);
                if (_DataSourceExists)
                    _DataSource = AppConfigManager.GetAppConfig(0, value.Name);
                else
                    _DataSource = value;
            }
        }
        public Boolean HideTableNode { get; set; }
        public String CssClass { get; set; }
    }
}


