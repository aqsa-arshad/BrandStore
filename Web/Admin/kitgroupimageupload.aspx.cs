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
using System.Text;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    public partial class kitgroupimageupload : AdminPageBase
    {
        public new Customer ThisCustomer { get; private set; }
        public KitGroupData KitGroup { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = -1;
            Response.AddHeader("pragma", "no-cache");

            ThisCustomer = Customer.Current;

            int kitId = Request.QueryStringNativeInt("kitId");
            int groupId = Request.QueryStringNativeInt("groupId");

            KitProductData kit = KitProductData.Find(kitId, ThisCustomer);
            KitGroup = kit.GetGroup(groupId);

            BindData();


            base.OnInit(e);
        }

        private void BindData()
        {
            if (KitGroup.HasImage)
            {
                pnlGroupImage.Visible = true;
                imgGroupImage.ImageUrl = KitGroup.ImagePath;
            }
            else
            {
                pnlGroupImage.Visible = false;
            }

            rptItemImages.DataSource = KitGroup.Items;
            rptItemImages.DataBind();
        }

        public string Localize(string value)
        {
            return value;
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            if (flpGroup.HasFile)
            {
                KitGroup.UploadImage(flpGroup.PostedFile);
            }

            foreach (RepeaterItem item in rptItemImages.Items)
            {
                if (item.ItemType == ListItemType.Item || 
                    item.ItemType == ListItemType.AlternatingItem)
                {
                    HiddenField hdfKitItemId = item.FindControl<HiddenField>("hdfKitItemId");
                    int kitItemId = hdfKitItemId.Value.ToNativeInt();
                    var kitItem = KitGroup.GetItem(kitItemId);
                    if (kitItem != null)
                    {
                        FileUpload flpKitItem = item.FindControl<FileUpload>("flpKitItem");
                        if (flpKitItem.HasFile)
                        {
                            kitItem.UploadImage(flpKitItem.PostedFile);
                        }
                    }
                }
            }
            BindData();
        }

        protected void lnkDeleteGroupImage_Click(object sender, EventArgs e)
        {
            KitGroup.DeleteImage();
            BindData();
        }

        protected void rptItemImages_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "DeleteKitItemImage")
            {
                int kitItemId = e.CommandArgument.ToString().ToNativeInt();
                KitItemData kitItem = KitGroup.GetItem(kitItemId);
                if (kitItem != null)
                {
                    kitItem.DeleteImage();
                }
            }
            BindData();
        }
    }

}
