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
    public partial class GeneralInfo : BaseUserControl<AppConfig>
    {
        public string DefaultText { get; set; }

        public String Description
        {
            get;
            protected set;
        }
        public String StringResource;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(StringResource))
                SetDescription(StringResource.StringResource());
            else if (!string.IsNullOrEmpty(DefaultText))
                SetDescription(DefaultText);
        }

        public void SetDescription(String description)
        {
            Description = description;
            if (!string.IsNullOrEmpty(StringResource))
                imgGeneralInfo.Attributes.Add("title", StringResource.StringResource());
            else
                imgGeneralInfo.Attributes.Add("title", Description);
        }
    }
}


