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
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;
using AjaxControlToolkit;
using Telerik.Web.UI;
using System.IO;
using AspDotNetStorefrontAdmin.Controls;

namespace AspDotNetStorefrontControls
{
    public partial class StoreControl : BaseUserControl<List<Store>>
    {
        public StoreControl()
        {
        }

        private List<Store> lstStores
        {
            get;
            set;
        }

        private void BindData()
        {
            BindData(false);
        }
     
        private void BindData(bool refresh)
        {
            lstStores = Store.GetStores(true);

            Datasource = lstStores;

            grdStores.DataSource = lstStores;
            grdStores.DataBind();
        }

        protected string DetermineLineItemCss(Store store)
        {
            var css = "store_normal";

            if (store.Deleted)
            {
                css = "store_deleted";
            }
            if (store.Published == false)
            {
                css = "store_unpublished";
            }

            return css;
        }

        /// <summary>
        /// Telerik compatible extension function for each individual bound item to get the actual type bound
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        protected T DataItemAs<T>(GridItem item) where T : class
        {
            return item.DataItem as T;
        }

        protected void grdStores_ItemCommand(object source, GridCommandEventArgs e)
        {
            bool requiresRestartCache = false;
            bool requiresRebind = false;

            switch (e.CommandName)
            {
                case "CopyStore":
                    var cboCopystoreFrom = e.Item.FindControl<DropDownList>("cboCopystoreFrom");
                    var sourceStoreId = cboCopystoreFrom.SelectedValue.ToNativeInt();

                    var destinationStoreId = e.CommandArgument.ToString().ToNativeInt();

                    CopyFromStore(sourceStoreId, destinationStoreId);

                    requiresRestartCache = true;
                    requiresRebind = true;
                    break;

                case "DeleteToggle":
                    var storeId = e.CommandArgument.ToString().ToNativeInt();
                    DeleteToggle(storeId);
                    requiresRestartCache = true;
                    requiresRebind = true;
                    break;

                case "PublishToggle":
                    var pstoreId = e.CommandArgument.ToString().ToNativeInt();
                    PublishToggle(pstoreId);
                    requiresRestartCache = true;
                    requiresRebind = true;
                    break;
            }

            // requires restart cache
            if (requiresRestartCache)
            {
                RestartCache();
            }

            if (requiresRebind)
            {
                BindData(true);
            }
        }

        private void RestartCache()
        {
            AppLogic.m_RestartApp();
        }

        private void DeleteToggle(int storeId)
        {
            var store = Datasource.Find(s => s.StoreID == storeId);
            if (store != null)
            {
                if(store.Deleted == false)
                {
                    store.DeleteStore();
                }
                else
                {
                    store.UnDeleteStore();
                }                
            }
        }

        private void PublishToggle(int storeId)
        {
            var store = Datasource.Find(s => s.StoreID == storeId);
            if (store != null)
            {
                store.PublishSwitch();
            }
        }

        /// <summary>
        /// Copies and overwrites the settings of the source store to the destination store
        /// </summary>
        /// <param name="sourceStoreId"></param>
        /// <param name="destinationStoreId"></param>
        private void CopyFromStore(int sourceStoreId, int destinationStoreId)
        {
            var from_Store = Datasource.Find(store => store.StoreID == sourceStoreId);
            var to_Store = Datasource.Find(store => store.StoreID == destinationStoreId);

            if (from_Store != null && 
                to_Store != null)
            {
                to_Store.CopyFrom(from_Store);
            }
        }


        protected void grdStores_SortCommand(object source, GridSortCommandEventArgs e)
        {
        }

        protected void grdStores_ItemCreated(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.Item ||
                e.Item.ItemType == GridItemType.AlternatingItem)
            {
                var chkDefault = e.Item.FindControl<CheckBox>("chkDefault");
                chkDefault.CheckedChanged += new EventHandler(chkDefault_CheckedChanged);

                var ctrlEditStore = e.Item.FindControl<AspDotNetStorefrontAdmin.Controls.StoreEdit>("ctrlEditStore");
                if (ctrlEditStore != null)
                {
                    // attach the double-click event
                    //e.Item.Attributes["ondblclick"] = ctrlEditStore.GetPopupCommandScript();

                    var btnEditStore = e.Item.FindControl<ImageButton>("btnEditStore");
                    btnEditStore.OnClientClick = ctrlEditStore.GetPopupCommandScript();

                    // attach the event handler that will notify us whenever the content has been updated
                    ctrlEditStore.UpdatedChanges += new EventHandler(ctrlStore_UpdatedChanges);
                }
            }
        }

        private void chkDefault_CheckedChanged(object sender, EventArgs e)
        {
            var chkDefault = sender as DataCheckBox;
            int storeId = chkDefault.Data.ToString().ToNativeInt();
            var store = Datasource.Find(s => s.StoreID == storeId);
            if (store != null)
            {
                store.SetDefault();
                RestartCache();
                BindData(true);
            }
        }

        protected void grdStores_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.Item ||
                e.Item.ItemType == GridItemType.AlternatingItem)
            {
                var currentStore = e.Item.DataItem as Store;

                e.Item.CssClass = DetermineLineItemCss(currentStore);

                var cboCopystoreFrom = e.Item.FindControl<DropDownList>("cboCopystoreFrom");
                if (cboCopystoreFrom != null)
                {
                    var otherStores = this.Datasource.Except(new Store[] { currentStore  });

                    foreach (var otherStore in otherStores)
                    {
                        var text = "({0}) {1}".FormatWith(otherStore.StoreID, otherStore.Name);
                        cboCopystoreFrom.Items.Add(new ListItem(text, otherStore.StoreID.ToString()));
                    }
                }

                var lnkDeleteToggle = e.Item.FindControl<LinkButton>("lnkDeleteToggle");
                if (lnkDeleteToggle != null)
                {
                    lnkDeleteToggle.OnClientClick = PrepareRadDeleteConfirm(lnkDeleteToggle, currentStore);
                }

                if (currentStore.Deleted == false)
                {
                    var lnkPublishToggle = e.Item.FindControl<LinkButton>("lnkPublishToggle");
                    if (lnkPublishToggle != null)
                    {
                        lnkPublishToggle.OnClientClick = PrepareRadPublishConfirm(lnkPublishToggle, currentStore);
                    }
                }

            }
        }

        protected override void OnInit(EventArgs e)
        {
            InitializeSkinsDatasource();
            BindData();
            InitializeAddNewStore();            

            base.OnInit(e);
        }

        protected string HTTPFy(string url)
        {
            return "http://" + url;
        }

        protected string PrepareRadDeleteConfirm(LinkButton lnkDeleteToggle, Store boundStore)
        {
            var callBack = "function cbX(arg){ if(arg){ " + Page.ClientScript.GetPostBackEventReference(lnkDeleteToggle, string.Empty) + "}}";

            var prompt = string.Empty;
            var title = string.Empty;

            if (boundStore.Deleted == false)
            {
                prompt = "<h3>Are you sure you want to delete this store: {0}?</h3><p>This will soft-delete this store but <span style=\'font-weight:bold;color:red;\'>will remove all mappings</span>. Click Ok to continue, otherwise cancel.</p>".FormatWith(HttpUtility.HtmlEncode(boundStore.Name));
                title = "Are you sure you want to delete this store?";
            }
            else
            {
                prompt = "<h3>Are you sure you want to Un-delete this store: {0}?</h3>".FormatWith(HttpUtility.HtmlEncode(boundStore.Name));
                title = "Are you sure you want to Un-delete this store?";
            }
            
            var script = "radconfirm(\"" + prompt + "\", " + callBack + ", 330, 100, null,\"" + title + "\"); return false;";

            return script;
        }

        protected string PrepareRadPublishConfirm(LinkButton lnkPublishToggle, Store boundStore)
        {
            var callBack = "function cbX(arg){ if(arg){ " + Page.ClientScript.GetPostBackEventReference(lnkPublishToggle, string.Empty) + "}}";

            var prompt = string.Empty;
            var title = string.Empty;

            if (boundStore.Published)
            {
                prompt = "<h3>Are you sure you want to Un-Publish this store: {0}?</h3>".FormatWith(HttpUtility.HtmlEncode(boundStore.Name));
                title = "Are you sure you want to Un-Publish this store?";
            }
            else
            {
                prompt = "<h3>Are you sure you want to Publish this store: {0}?</h3>".FormatWith(HttpUtility.HtmlEncode(boundStore.Name));
                title = "Are you sure you want to Publish this store?";
            }

            var script = "radconfirm(\"" + prompt + "\", " + callBack + ", 330, 100, null,\"" + title + "\"); return false;";

            return script;
        }



        protected string DeleteText(Store boundStore)
        {
            var text = "Delete";
            if (boundStore.Deleted)
            {
                text = "Un-Delete";
            }

            return text;
        }

        protected string PublishText(Store boundStore)
        {
            var text = "Un-Publish";
            if (boundStore.Published == false)
            {
                text = "Publish";
            }

            return text;
        }

        private void InitializeAddNewStore()
        {
            var newStore = new Store();
            newStore.Deleted = false;
            newStore.Description = string.Empty;
            newStore.DevelopmentURI = string.Empty;
            newStore.StagingURI = string.Empty;
            newStore.ProductionURI = string.Empty;
            newStore.IsDefault = false;
            newStore.Published = true;
            newStore.CreatedOn = DateTime.Now;
            newStore.Name = string.Empty;
            newStore.SkinID = 1;

            // setup this control manually since this isn't take part in the databinding process of the grid
            // we'll bind it manually

            ctrlAddStore.Skins = this.Skins;
            ctrlAddStore.Datasource = newStore;
            ctrlAddStore.ThisCustomer = ThisCustomer;
            ctrlAddStore.VisibleOnPageLoad = false;
            ctrlAddStore.DataBind();
        }

        private List<string> m_skins;
        public List<string> Skins
        {
            get { return m_skins; }
            set { m_skins = value; }
        }

        private void InitializeSkinsDatasource()
        {
            Skins = new List<string>();
            
            DirectoryInfo xinfo = new DirectoryInfo(Request.PhysicalApplicationPath + "\\App_Templates\\");
            foreach (DirectoryInfo skinDirs in xinfo.GetDirectories("SKIN*"))
            {
                Skins.Add(skinDirs.Name);
            }
        }

        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (lstStores == null)
            {
                lstStores = Store.GetStoreList();
                
            }            
        }

        protected void DownloadLicenseFile(object sender, EventArgs e)
        {
            litDLFrame.Text = "<iframe src='DownloadLicenseFile.aspx' width='1' height='1' visible='false' frameborder='0' />";

        }
       
        #region "Bind Events"

        #endregion


        #region "Grid Handlers"
    

        protected void chkDefault_Changed(object sender, EventArgs e)
        {
            foreach (Store xObj in lstStores)
            {
                if ((Guid)xObj.StoreGuid == new Guid(((WebControl)sender).ToolTip))
                {
                    xObj.SetDefault();
                    BindData();
                    return;
                }
            }
        }
        protected void chkPublished_Changed(object sender, EventArgs e)
        {
            foreach (Store xObj in lstStores)
            {
                if ((Guid)xObj.StoreGuid == new Guid(((WebControl)sender).ToolTip))
                {
                    xObj.PublishSwitch();
                    BindData();
                    return;
                }
            }
        }
        #endregion

        #region "Edit Box Handlers"


        protected void ctrlStore_UpdatedChanges(object sender, EventArgs e)
        {
            // re-initialize add
            InitializeAddNewStore();

            BindData(true);
        }

        #endregion

    }

}
