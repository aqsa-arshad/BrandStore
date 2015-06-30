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
using AspDotNetStorefrontControls;
using AjaxControlToolkit;
using Telerik.Web.UI;
using System.IO;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class StoreEdit : BaseUserControl<Store>
    {
        private string m_headertext;
        private string m_popuptargetcontrolid;
        private List<string> m_skins;
        private bool m_clonemode;

        public string HeaderText
        {
            get { return m_headertext; }
            set { m_headertext = value; }
        }

        public string PopupTargetControlID
        {
            get { return m_popuptargetcontrolid; }
            set { m_popuptargetcontrolid = value; }
        }

        public List<string> Skins
        {
            get { return m_skins; }
            set { m_skins = value; }
        }

        public bool VisibleOnPageLoad
        {
            get
            {
                return string.IsNullOrEmpty(pnlEditStore.Style["display"]);
            }
            set
            {
                pnlEditStore.Style["display"] = value ? string.Empty : "none";
            }
        }

        public bool CloneMode
        {
            get { return m_clonemode; }
            set { m_clonemode = value; }
        }

        public override void DataBind()
        {
            base.DataBind();

            // this control might be reinitialized on postback
            // thefore clear out the previous selection and initialize as fresh
            cmbSkinID.ClearSelection();
            foreach (var skin in Skins)
            {
                var id = skin.Split('_')[1];
                cmbSkinID.Items.Add(new ListItem(skin, id));
            }

            //cmbSkinID.DataSource = this.Skins;
            //cmbSkinID.DataBind();

            var store = this.Datasource;
            cmbSkinID.SelectedValue = store.SkinID.ToString();

            phRegisterWithBuySafe.Visible = cbxBuySafe.Checked = store.StoreID < 1;

            extEditStorePanel.TargetControlID = this.PopupTargetControlID;          

            if (this.CloneMode)
            {
                txtStoreName.Text = "{0} - Clone".FormatWith(Datasource.Name);
            }
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

        public override bool UpdateChanges()
        {
            var store = this.Datasource;
            if (this.CloneMode)
            {   // clone first the details
                store = store.CloneStore();
            }
            store.Name = txtStoreName.Text;
            store.Description = txtDescription.Text;
            store.ProductionURI = txtProductionURI.Text;
            store.StagingURI = txtStagingURI.Text;
            store.DevelopmentURI = txtDevURI.Text;
            store.Published = chkPublished.Checked;
            store.SkinID = cmbSkinID.SelectedValue.ToNativeInt();
            if (store.StoreID < 1 && cbxBuySafe.Checked)
                AspDotNetStorefrontBuySafe.BuySafeController.RegisterStore(store);
            store.Save();
            OnUpdatedChanges(EventArgs.Empty);
			AspDotNetStorefront.CachelessStore.resetStoreCache();
            return true;
        }

        public string GetPopupCommandScript()
        {            
            return "$find('{0}').show();return false;".FormatWith(extEditStorePanel.ClientID);
        }

        protected void cmdSave_Click(object sender, EventArgs e)
        {
            if (txtStoreName.Text.Trim().Length == 0 && IsPostBack == true)
            {
                lblStoreNameError.Text = "You must enter a store name.";
                extEditStorePanel.Show();
				return;
            }

			try
			{
				if (!String.IsNullOrEmpty(txtProductionURI.Text))
				{
					Uri url = new Uri(String.Format("http://{0}", txtProductionURI.Text.Replace("http://", String.Empty)));
					if (url.AbsolutePath != "/")
					{
						lblStoreNameError.Text = "Your production url should only be the host url and not include http:// or any virtual directory information.";
						extEditStorePanel.Show();
						return;
					}

					txtProductionURI.Text = url.Host;
				}
			}
			catch
			{
				lblStoreNameError.Text = "Your production url should only be the host url and not include http:// or any virtual directory information.";
				extEditStorePanel.Show();
				return;
			}

			try
			{
				if (!String.IsNullOrEmpty(txtStagingURI.Text))
				{
					Uri url = new Uri(String.Format("http://{0}", txtStagingURI.Text.Replace("http://", String.Empty)));
					if (url.AbsolutePath != "/")
					{
						lblStoreNameError.Text = "Your staging url should only be the host url and not include http:// or any virtual directory information.";
						extEditStorePanel.Show();
						return;
					}

					txtStagingURI.Text = url.Host;
				}
			}
			catch
			{
				lblStoreNameError.Text = "Your staging url should only be the host url and not include http:// or any virtual directory information.";
				extEditStorePanel.Show();
				return;
			}

			try
			{
				if (!String.IsNullOrEmpty(txtDevURI.Text))
				{
					Uri url = new Uri(String.Format("http://{0}", txtDevURI.Text.Replace("http://", String.Empty)));
					if (url.AbsolutePath != "/")
					{
						lblStoreNameError.Text = "Your development url should only be the host url and not include http:// or any virtual directory information.";
						extEditStorePanel.Show();
						return;
					}

					txtDevURI.Text = url.Host;
				}
			}
			catch
			{
				lblStoreNameError.Text = "Your development url should only be the host url and not include http:// or any virtual directory information.";
				extEditStorePanel.Show();
				return;
			}
			
			UpdateChanges();
        }
    }
}


