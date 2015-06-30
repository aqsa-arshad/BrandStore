// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontLayout;
using System.Data.SqlClient;

namespace AspDotNetStorefront
{
    public partial class LayoutHost : SkinBase 
    {
        protected override void OnInit(EventArgs e)
        {
            int LayoutID = CommonLogic.QueryStringNativeInt("layoutid");

            ThisLayout = new LayoutData(LayoutID);

            if (ThisLayout != null)
            {
                bool exists = CommonLogic.FileExists(ThisLayout.LayoutFile);

                if (!exists)
                {
                    exists = ThisLayout.CreateLayoutControl();
                }

                if (exists)
                {
                    Control ctrl = LoadControl("layouts/" + ThisLayout.LayoutID.ToString() + "/" + ThisLayout.Name + "_" + ThisLayout.Version.ToString() + ".ascx");

                    pnlLayout.Controls.Add(ctrl);
                }
            }
            
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //if (ThisLayout != null)
            //{

            //}
        }

    }
}
