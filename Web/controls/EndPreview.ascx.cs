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
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront
{
	public partial class controls_EndPreview : System.Web.UI.UserControl
	{
        protected void Page_Load(object sender, EventArgs e)
        {
            Customer thisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            int previewSkinId = int.Parse(HttpContext.Current.Profile.GetPropertyValue("PreviewSkinID").ToString());

            Skin previewSkin = new SkinProvider().GetSkinById(previewSkinId);

            string previewSkinName = !String.IsNullOrEmpty(previewSkin.DisplayName) ? previewSkin.DisplayName : previewSkin.Name;

            litPreviewText.Text = String.Format(AppLogic.GetString("admin.skinselector.PreviewWarning", thisCustomer.LocaleSetting), previewSkinName);
        }

		protected void EndPreviewButton_Click(object sender, EventArgs e)
		{
			//remove the profile Value
			HttpContext.Current.Profile.SetPropertyValue("PreviewSkinID", "");

			//redirect to the homepage without the querystring
			Response.Redirect("~/");
		}
	}
}
