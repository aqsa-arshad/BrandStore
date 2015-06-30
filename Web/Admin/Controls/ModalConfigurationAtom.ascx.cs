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
    public partial class ModalConfigurationAtom : UserControl
    {
        public IConfigurationAtom AtomConfigurationDataSource 
        {
            get
            {
                return ConfigurationAtom.AtomConfigurationDataSource;
            }
            set
            {
                litTitle.Text = value.Title;
                ConfigurationAtom.AtomConfigurationDataSource = value;
                btnToggleAdvanced.Visible = value.AtomAppConfigs.Any(a => a.IsAdvanced);
            }
        }

        public Boolean ShowConfigureLink { get; set; }
        public String ConfigureLinkText { get; set; }
        public String ConfigurationFileName { get; protected set; }

        public event EventHandler AtomSaved;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!String.IsNullOrEmpty(ConfigureLinkText))
                    btnConfigureAtomConfig.Text = ConfigureLinkText;

                if (!ShowConfigureLink)
                    btnConfigureAtomConfig.Text = "";

                if (!String.IsNullOrEmpty(this.ConfigurationFileName))
                {
                    this.AtomConfigurationDataSource = new ConfigurationAtom(ConfigurationFileName);
                    this.DataBind();
                }
            }
            ConfigurationAtom.Saved += ConfigurationAtom_Saved;
        }

        public void SetConfigurationFile(string fileName)
        {
            if (!String.IsNullOrEmpty(fileName))
            {
                this.ConfigurationFileName = fileName;
                this.AtomConfigurationDataSource = new ConfigurationAtom(ConfigurationFileName);
                this.DataBind();
            }
        }

        protected void btnCancelConfiguration_Click(object sender, EventArgs e)
        {
            mpConfigurationAtom.Hide();
        }
        
        protected void ConfigurationAtom_Saved(object sender, EventArgs e)
        {
            mpConfigurationAtom.Hide();

            if (AtomSaved != null)
                AtomSaved(this, new EventArgs());
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            ConfigurationAtom.Save();
        }
       
        public void Show()
        {
            mpConfigurationAtom.Show();
        }

        public void Hide()
        {
            mpConfigurationAtom.Hide();
        }   
    }
}


