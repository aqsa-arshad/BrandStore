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
    public partial class AddAppConfig : BaseUserControl<AppConfig>
    {
        protected const int DEFAULT_COLUMN_LENGTH = 35;
        protected const int MAX_ROW_LENGTH = 4;

        private string m_cssclass;
        private IEnumerable<string> m_configgroups;

        public string CssClass
        {
            get { return m_cssclass; }
            set { m_cssclass = value; }
        }

        public IEnumerable<string> ConfigGroups
        {
            get { return m_configgroups; }
            set { m_configgroups = value; }
        }

        public override void DataBind()
        {
            base.DataBind();

            InitConfigValueTextBox();
            ThisCustomer = (Page as AdminPageBase).ThisCustomer;
            // selector row
            if (ThisCustomer.IsAdminSuperUser)
            {
                trAdvanced.Visible = true;
                pnlAdvanced.Visible = true;
                InitializeAdvancedConfigs();
            }
        }
        
        private void InitConfigValueTextBox()
        {
            txtConfigValue.Columns = DEFAULT_COLUMN_LENGTH;
        }

        private void ClearErrors()
        {
            HasErrors = false;
            pnlError.Visible = false;
            lblError.Text = string.Empty;
        }

        public override bool UpdateChanges()
        {     
            String name = txtName.Text;
            if (string.IsNullOrEmpty(name))
            {
                this.HasErrors = true;
                pnlError.Visible = true;
                lblError.Text = "Please specify name";
                return false;
            }

            int storeId = 0; //adding to storeid 0 or default app config

            bool allow = true;

            // general check if config exists on any store
            if (AppConfigManager.AppConfigExists(name))
            {
                // check if the selected store is not existing on the selected store
                AppConfig existingConfig = AppConfigManager.GetAppConfig(0, name);
                allow = (existingConfig == null);

                if (!allow)
                {
                    this.HasErrors = true;
                    pnlError.Visible = true;
                    lblError.Text = "AppConfig already exists. please specify a different name";
                    return false;
                }
            }

            string description = txtDescription.Text;
            string configValue = txtConfigValue.Text;
            string groupName = cboGroupName.SelectedValue;
                
            // add the appconfig and refresh our references to it
            AppConfig newConfig = AppLogic.AddAppConfig(name, description, configValue, groupName, false, storeId);
            this.Datasource = newConfig;

            ClearErrors(); // if any
            

            if (ThisCustomer.IsAdminSuperUser)
            {
                newConfig.SuperOnly = rbSuperOnly.Items[0].Selected; // 0 is Yes
                newConfig.ValueType = cboValueType.SelectedValue;
                if(!string.IsNullOrEmpty(txtAllowableValues.Text))
                {
                    List<string> allowableValues = new List<string>();
                    allowableValues.AddCommaDelimited(txtAllowableValues.Text);
                    newConfig.AllowableValues = allowableValues;
                }
            }

            OnUpdatedChanges(EventArgs.Empty);
            AppConfigManager.LoadAllConfigs();

            return true;
        }

        private void InitializeAdvancedConfigs()
        {
            AppConfig thisConfig = Datasource;

            // start fresh if reinitialized
            rbSuperOnly.Items.Clear();
            rbSuperOnly.Items.Add("Yes");
            rbSuperOnly.Items.Add("No");
            rbSuperOnly.Items[0].Selected = thisConfig.SuperOnly;
            rbSuperOnly.Items[1].Selected = !thisConfig.SuperOnly;

            // start fresh if reinitialized 
            cboValueType.Items.Clear();
            cboValueType.DataSource = GetConfigValueTypes();
            cboValueType.DataTextField = "Key";
            cboValueType.DataValueField = "Value";
            cboValueType.DataBind();
            cboValueType.SelectedValue = thisConfig.ValueType;

            txtAllowableValues.Text = thisConfig.AllowableValues.ToCommaDelimitedString();
        }

        private Dictionary<string, string> GetConfigValueTypes()
        {
            Dictionary<string, string> types = new Dictionary<string, string>();
            types.Add("String", "string");
            types.Add("Boolean", "boolean");
            types.Add("Integer", "integer");
            types.Add("Decimal", "decimal");
            types.Add("Double", "double");
            types.Add("Enumeration", "enum");
            types.Add("Multi-Select", "multiselect");
            types.Add("Dynamic Invoke", "invoke");

            return types;
        }
    }
}


