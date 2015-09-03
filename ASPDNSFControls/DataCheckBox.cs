// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
    [ToolboxData("<{0}:DataCheckBox runat=server></{0}:DataCheckBox>")]
    public class DataCheckBox : CheckBox
    {
        [Bindable(false)]
        [Category("Data")]
        [DefaultValue("")]
        [Localizable(false)]
        public object Data
        {
            get
            {
                return this.ViewState["DataCheckBox.Data"];
            }
            set
            {
                this.ViewState["DataCheckBox.Data"] = value;
            }
        }
    }
}
