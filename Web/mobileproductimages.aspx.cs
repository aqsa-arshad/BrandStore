// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;
using Vortx.MobileFramework;
using System.Collections.Generic;

namespace AspDotNetStorefront
{
    public partial class mobileproductimages : SkinBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            MobileHelper.RedirectPageWhenMobileIsDisabled("~/", ThisCustomer);

            int productid = CommonLogic.QueryStringNativeInt("productid");
            XmlPackage2 xp = new XmlPackage2("mobile.vortxmultiimage", ThisCustomer, ThisCustomer.SkinID, "", "productid=" + productid);
            XMLPackagePlaceHolder.Text = AppLogic.RunXmlPackage(xp, null, ThisCustomer, ThisCustomer.SkinID, true, false);
        }
    }
}
