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
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;
using AjaxControlToolkit;
using AspDotNetStorefront;
using System.Reflection;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class ConfigurationAtomControl : UserControl
    {
        public IConfigurationAtom AtomConfigurationDataSource { get; set; }
        public Boolean ShowSaveButton { get; set; }
        public Boolean LoadAdvancedConfigs { get; set; }
        

        public event EventHandler Saved;

        public override void DataBind()
        {
            if (AtomConfigurationDataSource == null)
                return;

            litHTMLHeader.Text = AtomConfigurationDataSource.HTMLHeader;

            repAppConfigs.DataSource = AtomConfigurationDataSource.AtomAppConfigs.Where(aac => aac.IsAdvanced == false || this.LoadAdvancedConfigs);
            repAppConfigs.DataBind();

            base.DataBind();
        }

        protected String ACClass(Boolean isAdvanced)
        {
            if (isAdvanced)
                return "ConfigAtomAdvanced";
            return "ConfigAtom";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                DataBind();
        }
        protected void repAppConfigs_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                EditAppConfigAtom AppConfigAtom = e.Item.FindControl("AppConfigAtom") as EditAppConfigAtom;
                AppConfigAtomInfo ds = e.Item.DataItem as AppConfigAtomInfo;

                AppConfigAtom.CssClass = ACClass(ds.IsAdvanced);

                if (!string.IsNullOrEmpty(ds.ContextualDescription))
                    AppConfigAtom.OverriddenDescription = ds.ContextualDescription.Trim();

                if (!string.IsNullOrEmpty(ds.FriendlyName))
                    AppConfigAtom.FriendlyName = ds.FriendlyName;

                AppConfigAtom.DataSource = ds.Config;
                if(ds.Config != null)
                    AppConfigAtom.AppConfig = ds.Config.Name;
                AppConfigAtom.DataBind();
            }
        
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        public void Save()
        {

            foreach (RepeaterItem item in repAppConfigs.Items)
            {
                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    EditAppConfigAtom eaca = item.FindControl("AppConfigAtom") as EditAppConfigAtom;
                    eaca.Save();
                }
            }

            if (Saved != null)
                Saved(this, new EventArgs());
        }
    }
}


