// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;
using AjaxControlToolkit;
using AspDotNetStorefront;
using System.Reflection;
using System.Text;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class EditAppConfigAtom : UserControl
    {
        #region Private Instance Properties
        private bool _DataSourceExists = false;
        private AppConfig _PassedConfig;
        private AppConfig _DataSource;
        private String _CssClass = "editAppConfig";
        #endregion

        #region Public Instance Properties
        public AppConfig DataSource
        {
            get
            {
                if (_DataSource == null && !String.IsNullOrEmpty(AppConfig))
                    return AppConfigManager.GetAppConfig(0, AppConfig);
                return _DataSource;
            }
            set
            {
                _PassedConfig = value;
                _DataSourceExists = value != null && AppConfigManager.AppConfigExists(value.StoreId, value.Name);
                if (_DataSourceExists)
                    _DataSource = AppConfigManager.GetAppConfig(value.StoreId, value.Name);
                else
                    _DataSource = value;

                if(_DataSourceExists)
                    AppConfig = _DataSource.Name;
            }
        }
        public bool ShowSaveButton { get; set; }
        public String CssClass
        {
            get { return _CssClass; }
            set { _CssClass = value; }
        }
        public Boolean HideTableNode { get; set; }
        public String OverriddenDescription { get; set; }
        public String FriendlyName { get; set; }
        public String AppConfig {
            get { return hfAppConfigName.Value; }
            set { hfAppConfigName.Value = value; }
        }
		public Boolean HideLabel { get; set; }
        #endregion

        #region EventHandler
        protected void Page_Load(object sender, EventArgs e)
        {
            //DataBind();
            if (!Page.IsPostBack && !String.IsNullOrEmpty(AppConfig))
                DataBind();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        protected void StoreValues_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                EditAppConfigInput acStoreValue = e.Item.FindControl("acStoreValue") as EditAppConfigInput;
                Literal litStoreName = e.Item.FindControl("litStoreName") as Literal;
                Store s = e.Item.DataItem as Store;

                litStoreName.Text = s.Name;
                if (AppConfigManager.AppConfigExists(s.StoreID, DataSource.Name))
                    acStoreValue.DataSource = AppConfigManager.GetAppConfig(s.StoreID, DataSource.Name);
                else
                    acStoreValue.DataSource = DataSource;

                acStoreValue.TargetStoreId = s.StoreID;

                acStoreValue.DataBind();
            }
        }
        #endregion

        #region Public Instance Methods
        public void Save()
        {
            if (DataSource == null)
                return;
            List<string> dontDupStoresForValues = new List<string>();
            dontDupStoresForValues.Add(AppLogic.AppConfig(acDefault.GetAppConfigName(), 0, false));
            acDefault.Save();
            dontDupStoresForValues.Add(AppLogic.AppConfig(acDefault.GetAppConfigName(), 0, false));

            foreach (RepeaterItem ri in repHiddenStoreValues.Items)
            {
                EditAppConfigInput i = ri.FindControl("acStoreValue") as EditAppConfigInput;
                i.Save(dontDupStoresForValues);
            }
            foreach (RepeaterItem ri in repVisibleStoreValues.Items)
            {
                EditAppConfigInput i = ri.FindControl("acStoreValue") as EditAppConfigInput;
                i.Save(dontDupStoresForValues);
            }
            BindStores();
        }
        public override void DataBind()
        {
            if (DataSource == null)
            {
                lblTitle.Text = "<em>{App Config Missing}</em>";
                return;
            }

            ltDescription.Text = GetDescription();
			pnlDescription.Visible = ltDescription.Text.Length > 0;

            if (!string.IsNullOrEmpty(FriendlyName))
                lblTitle.Text = FriendlyName;
            else
                lblTitle.Text = DataSource.Name;

			if (HideLabel)
			{
				lblTitleWrap.Visible = false;
			}

            BindStores();

            initConfigValue();

            base.DataBind();
        }

        private void BindStores()
        {
            List<Store> visibleStores = new List<Store>();
            List<Store> hiddenStores = new List<Store>();

            List<Store> allStores = Store.GetStores(true).Where(s => s.Deleted == false).ToList();

            if (DataSource.ValueType.EqualsIgnoreCase("boolean"))
            {
                visibleStores = allStores;
            }
            else
            {
                foreach (Store s in allStores)
                {
                    if (AppConfigManager.AppConfigExists(s.StoreID, DataSource.Name))
                        visibleStores.Add(s);
                    else
                        hiddenStores.Add(s);
                }
            }

			MoreStoreLink.Visible = hiddenStores.Count > 0;

            repHiddenStoreValues.DataSource = hiddenStores;
            repHiddenStoreValues.DataBind();
            repVisibleStoreValues.DataSource = visibleStores;
            repVisibleStoreValues.DataBind();
        }
        public String GetValue(int storeId)
        {
            if (storeId == 0)
                return acDefault.Value;

            foreach (RepeaterItem ri in repVisibleStoreValues.Items)
            {
                HiddenField hfStoreId = ri.FindControl("hfStoreId") as HiddenField;
                EditAppConfigInput acStoreValue = ri.FindControl("acStoreValue") as EditAppConfigInput;
                int rStoreId;
                if (int.TryParse(hfStoreId.Value, out rStoreId) && rStoreId == storeId)
                    return acStoreValue.Value;
            }

            foreach (RepeaterItem ri in repHiddenStoreValues.Items)
            {
                HiddenField hfStoreId = ri.FindControl("hfStoreId") as HiddenField;
                EditAppConfigInput acStoreValue = ri.FindControl("acStoreValue") as EditAppConfigInput;
                int rStoreId;
                if (int.TryParse(hfStoreId.Value, out rStoreId) && rStoreId == storeId)
                    return acStoreValue.Value;
            }

            return null;
        }
        #endregion

        private string GetDescription()
        {
            StringBuilder sb = new StringBuilder();
            //if (!string.IsNullOrEmpty(FriendlyName))
            //    sb.AppendFormat("App Config: {0}. ", DataSource.Name);

            if (!string.IsNullOrEmpty(OverriddenDescription))
                sb.Append(OverriddenDescription);
            else
                sb.Append(DataSource.Description);

            return sb.ToString();
        }

        #region Private Instance Methods
        private void initConfigValue()
        {
            acDefault.DataSource = DataSource;
            acDefault.DataBind();
        }
        #endregion
    }
}


