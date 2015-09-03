// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AjaxControlToolkit;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web.UI;


[assembly: System.Web.UI.WebResource("AspDotNetStorefrontControls.Extenders.TextStateChangePostbackBehavior.js", "text/javascript")]

namespace AspDotNetStorefrontControls.Extenders
{
    [ClientScriptResource("aspdnsf.Controls.Extenders.TextStateChangePostbackBehavior", "AspDotNetStorefrontControls.Extenders.TextStateChangePostbackBehavior.js")]
    [TargetControlType(typeof(Control))]
    public class TextStateChangePostbackBehavior : ExtenderControlBase
    {
        [ExtenderControlProperty]
        [DefaultValue(500)]
        [ClientPropertyName("timeout")]
        [RequiredProperty]
        public int Timeout
        {
            get { return GetPropertyValue<int>("Timeout", 500); }
            set { SetPropertyValue<int>("Timeout", value); }
        }

        [ExtenderControlProperty]
        [DefaultValue(false)]
        [ClientPropertyName("debugMode")]
        public bool DebugMode
        {
            get { return GetPropertyValue<bool>("DebugMode", false); }
            set { SetPropertyValue<bool>("DebugMode", value); }
        }

        [ExtenderControlProperty]
        [DefaultValue(false)]
        [ClientPropertyName("monitorTextChanged")]
        public bool MonitorTextChanged
        {
            get { return GetPropertyValue<bool>("MonitorTextChanged", false); }
            set { SetPropertyValue<bool>("MonitorTextChanged", value); }
        }
    }
}

