// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Text;
using System.Globalization;
using System.Data.SqlClient;

using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for EMailproduct.
    /// </summary>
    public partial class EMailProductPage : SkinBase
    {
        int ProductID;

        protected void Page_Load(object sender, EventArgs e)
        {
            ProductID = CommonLogic.QueryStringUSInt("ProductID");
            
            if (AppLogic.AppConfigBool("GoNonSecureAgain"))
            {
                GoNonSecureAgain();
            }
            // DOS attack prevention:
            if (AppLogic.OnLiveServer() && (Request.UrlReferrer == null || Request.UrlReferrer.Authority != Request.Url.Authority))
            {
                Response.Redirect(SE.MakeDriverLink("EmailError")); 
            }
            if (ProductID == 0)
            {
                HttpContext.Current.Response.StatusCode = 404;
                HttpContext.Current.Server.Transfer("pagenotfound.aspx");
            }
            if (AppLogic.ProductHasBeenDeleted(ProductID))
            {
                HttpContext.Current.Response.StatusCode = 404;
                HttpContext.Current.Server.Transfer("pagenotfound.aspx");
            }

                
            EmailProduct ep = (EmailProduct)LoadControl("~/Controls/EmailProduct.ascx");
            ep.ProductID = ProductID;

            pnlContent.Controls.Add(ep);
        }

        
        String HT = String.Empty;

        override protected void OnPreInit(EventArgs e)
        {            
            if (CommonLogic.QueryStringUSInt("CategoryID") != 0)
            {
                HT = AppLogic.GetCurrentEntityTemplateName(EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName);
            }
            else if (CommonLogic.QueryStringUSInt("SectionID") != 0)
            {
                HT = AppLogic.GetCurrentEntityTemplateName(EntityDefinitions.readonly_SectionEntitySpecs.m_EntityName);
            }
            else if (CommonLogic.QueryStringUSInt("ManufacturerID") != 0)
            {
                HT = AppLogic.GetCurrentEntityTemplateName(EntityDefinitions.readonly_ManufacturerEntitySpecs.m_EntityName);
            }
            else if (CommonLogic.QueryStringUSInt("DistributorID") != 0)
            {
                HT = AppLogic.GetCurrentEntityTemplateName(EntityDefinitions.readonly_DistributorEntitySpecs.m_EntityName);
            }
            else if (CommonLogic.QueryStringUSInt("GenreID") != 0)
            {
                HT = AppLogic.GetCurrentEntityTemplateName(EntityDefinitions.readonly_GenreEntitySpecs.m_EntityName);
            }
            else if (CommonLogic.QueryStringUSInt("VectorID") != 0)
            {
                HT = AppLogic.GetCurrentEntityTemplateName(EntityDefinitions.readonly_VectorEntitySpecs.m_EntityName);
            }
            
            base.OnPreInit(e);
        }

        protected override string OverrideTemplate()
        {
            if (!String.IsNullOrEmpty(HT))
                    return HT;

            return "template";
        }


    }
}
