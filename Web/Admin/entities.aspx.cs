// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.IO;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for entities.
	/// </summary>
    public partial class entities : AspDotNetStorefront.SkinBase
	{
		
		private EntitySpecs m_EntitySpecs;

		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (CommonLogic.QueryStringCanBeDangerousContent("EntityName").Length == 0)
			{
				Response.Redirect(AppLogic.AdminLinkUrl("default.aspx"));
			}
			m_EntitySpecs = EntityDefinitions.LookupSpecs(CommonLogic.QueryStringCanBeDangerousContent("EntityName"));
			SectionTitle = String.Format(AppLogic.GetString("Token", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),AppLogic.GetString("AppConfig." + m_EntitySpecs.m_EntityName + "PromptPlural",SkinID,ThisCustomer.LocaleSetting));
			AdminLogic.EntityListPageFormHandler(m_EntitySpecs,ThisCustomer,SkinID);
		}

		protected override void RenderContents(System.Web.UI.HtmlTextWriter writer)
		{
			writer.Write(AdminLogic.EntityListPageRender(this,m_EntitySpecs,ThisCustomer,SkinID));
		}

	}
}
