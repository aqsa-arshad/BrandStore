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
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using System.Text;

namespace AspDotNetStorefront.Kit
{
    public partial class FileUploadItems : BaseKitGroupControl
    {
        protected string GenerateFileUploadLink(int groupId, int kitItemId)
        {
            return string.Format("javascript:window.open('kitproductupload.aspx?productId={0}&groupId={1}&itemId={2}&stub={3}', 'manageimages', 'scrollbars=1;width:300;height;200;status=0;toolbar=0;location=0;'); javascript:void(0);",
                                    KitGroup.Kit.Id,
                                    groupId,
                                    kitItemId,
                                    KitGroup.Kit.TempFileStub);
        }

        protected void rptItems_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
        }

        public override void ResolveSelection()
        {
            rptItems.ForEachItemTemplate(RepeaterItemIterator);
        }

        private void RepeaterItemIterator(RepeaterItem item)
        {
            HiddenField hdfKitItemId = item.FindControl<HiddenField>("hdfKitItemId");            

            int kitItemId = hdfKitItemId.Value.ToNativeInt();
            KitItemData kitItem = KitGroup.GetItem(kitItemId);

            kitItem.IsSelected = kitItem.HasCustomerUploadedImage;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            StringBuilder script = new StringBuilder();
            script.AppendFormat(" Sys.Application.add_load(function() {{ \n");
            script.AppendFormat("     var ctrl = new aspdnsf.Controls.FileUploadControl({0}, '{1}');\n", KitGroup.Id, KitGroup.Kit.TempFileStub);
            script.AppendFormat("     ctrl.set_refreshCommand( function(){{ {0} }} ); \n", Page.ClientScript.GetPostBackEventReference(lnkRefresh, string.Empty));
            script.AppendFormat("     aspdnsf.Pages.KitPage.add_uploadGroup(ctrl);\n");
            script.AppendFormat(" }});\n");

            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), script.ToString(), true);
        }
        
    }
}


