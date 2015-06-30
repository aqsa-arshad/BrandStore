// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.Sql;
using System.Data.SqlClient;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCommon;
using System.Linq;

namespace AspDotNetStorefront
{
    public partial class kitproductupload : System.Web.UI.Page
    {
        public KitProductData KitData { get; private set; }
        public KitItemData KitItem { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            int productId = Request.QueryStringNativeInt("productId");
            int groupId = Request.QueryStringNativeInt("groupId");
            int kitItemId = Request.QueryStringNativeInt("itemId");

            KitProductData kitProduct = KitProductData.Find(productId, Customer.Current);
            KitData = kitProduct;

            if (kitProduct != null)
            {
                KitGroupData kitGroup = kitProduct.GetGroup(groupId);
                if (kitGroup != null)
                {
                    KitItemData item = kitGroup.GetItem(kitItemId);
                    if (item != null)
                    {
                        KitItem = item;
                        kitProduct.TempFileStub = Request.QueryStringCanBeDangerousContent("stub");
                    }
                }
            }

            base.OnInit(e);
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            KitItem.UploadCustomerImage(flpUpload.PostedFile);
            AttachRefreshCallingKitPageScript();
        }

        private void AttachRefreshCallingKitPageScript()
        {
            StringBuilder script = new StringBuilder();
            script.AppendFormat(" Sys.Application.add_load(function() {{ window.opener.aspdnsf.Pages.KitPage.refreshUploadGroup('{0}'); self.close(); }});", KitData.TempFileStub);

            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), script.ToString(), true);
        }
    }
}

