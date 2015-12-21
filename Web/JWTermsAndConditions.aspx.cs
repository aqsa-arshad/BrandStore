using System;
using System.Web.Security;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using System.Web.UI;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCommon;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Terms And Conditions Static Page - code behind cs file
    /// </summary>
    public partial class JWTermsAndConditions : SkinBase
    {
        /// <summary>
        /// Page Load Event
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        
        /// <summary>
        /// Override JeldWen Master Template
        /// </summary>
        protected override string OverrideTemplate()
        {
            String MasterHome = AppLogic.HomeTemplate();

            if (MasterHome.Trim().Length == 0)
            {

                MasterHome = "JeldWenTemplate";
            }

            if (MasterHome.EndsWith(".ascx"))
            {
                MasterHome = MasterHome.Replace(".ascx", ".master");
            }

            if (!MasterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
            {
                MasterHome = MasterHome + ".master";
            }

            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + base.SkinID.ToString() + "/" + MasterHome)))
            {
                //Change template name to JELD-WEN template by Tayyab on 07-09-2015
                MasterHome = "JeldWenTemplate";// "template.master";
            }

            return MasterHome;
        }
    }
}