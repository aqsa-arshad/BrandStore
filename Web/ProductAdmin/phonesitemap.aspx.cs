// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for phonesitemap.
    /// </summary>
    public partial class phonesitemap : AdminPageBase
    {

        String IGD = String.Empty;
        Customer TargetCustomer;
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            IGD = CommonLogic.QueryStringCanBeDangerousContent("IGD");
            Guid TargetCustomerGuid = new Guid(IGD);
            TargetCustomer = new Customer(TargetCustomerGuid, true);

            System.Collections.Generic.Dictionary<string, EntityHelper> eh = AppLogic.MakeEntityHelpers();
            SiteMap1.LoadXml(new SiteMapPhoneOrder(eh, 1, TargetCustomer, IGD).Contents);
        }
    }
}
