// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Text;
using System.Xml;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefrontAdmin
{
    public partial class AdminEntityEdit : AdminMasterPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String ctrlName = AppLogic.AdminLinkUrl("Controls/EntityMenu.ascx", true);

            pnlEntityMenu.Controls.Add(LoadControl(ctrlName));
        }
    }
}
