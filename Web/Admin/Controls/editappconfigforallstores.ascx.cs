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
using System.Text.RegularExpressions;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class EditAppConfigForAllStores : BaseUserControl<AppConfig>
    {
        #region Private Instance Variables
        private const int DEFAULT_COLUMN_LENGTH = 35;
        private const int MAX_COLUMN_LENGTH = 75;
        private const int MAX_ROW_LENGTH = 4;
        private const int MIN_ROW_LENGTH = 2;

        private string m_cssclass;
        private IEnumerable<string> m_configgroups;
        private List<Store> m_stores;
        private string m_configvalue;
        private Dictionary<int, StoreConfigDataConstruct> m_storeConfigValues;
        private Dictionary<int, StoreConfigDataConstruct> StoreConfigValues
        {
            get
            {
                if (m_storeConfigValues == null)
                {
                    m_storeConfigValues = new Dictionary<int, StoreConfigDataConstruct>();
                    AppConfig configForStore = AppConfigManager.GetAppConfig(0, this.Datasource.Name);
                    StoreConfigDataConstruct data = new StoreConfigDataConstruct()
                    {
                        EditorPlaceHolder = DetermineConfigEditControl("Default For All Stores", 0, configForStore),
                        StoreId = 0,
                        StoreName = "Default For All Stores",
                        ConfigName = this.Datasource.Name,
                        EditingConfig = configForStore,
                    };
                    m_storeConfigValues.Add(0, data);
                    foreach (Store s in Stores)
                    {
                        configForStore = AppConfigManager.GetAppConfig(s.StoreID, this.Datasource.Name);
                        data = new StoreConfigDataConstruct()
                        {
                            EditorPlaceHolder = DetermineConfigEditControl(s.Name, s.StoreID, configForStore),
                            StoreId = s.StoreID,
                            StoreName = s.Name,
                            ConfigName = this.Datasource.Name,
                            EditingConfig = configForStore,
                        };

                        m_storeConfigValues.Add(s.StoreID, data);
                    }
                }
                return m_storeConfigValues;
            }
        }
        private string m_OriginalDefaultValue;
        private string m_OriginalDefaultType;
        #endregion

        #region Public Properties
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

        public List<Store> Stores
        {
            get { return m_stores; }
            set { m_stores = value; }
        } 
        #endregion

        #region Public Instance Methods
        public override bool UpdateChanges()
        {
            bool addedNew = false;
            foreach (KeyValuePair<int, StoreConfigDataConstruct> kvp in StoreConfigValues)
            {
                AppConfig currentConfig = kvp.Value.EditingConfig;
                if (currentConfig != null)
                {
                    int currentStoreId = kvp.Value.StoreId;

                    //plhConfigValueEditor
                    String editorID =
                    currentConfig.Description = txtDescription.Text;
                    currentConfig.ConfigValue = GetConfigValue(kvp.Value.EditorPlaceHolder, currentConfig, false, kvp.Value.StoreId); //this.txtConfigValue.Text;
                    currentConfig.GroupName = cboGroupName.SelectedValue;

                    if (ThisCustomer.IsAdminSuperUser)
                    {
                        currentConfig.SuperOnly = rbSuperOnly.Items[0].Selected; // 0 is Yes
                        currentConfig.ValueType = cboValueType.SelectedValue;
                        if (!string.IsNullOrEmpty(txtAllowableValues.Text))
                        {
                            List<string> allowableValues = new List<string>();
                            allowableValues.AddCommaDelimited(txtAllowableValues.Text);
                            currentConfig.AllowableValues = allowableValues;
                        }
                    }
                }
                else // adding duplicate for store
                {
                    AppConfig DefaultAppConfig = AppLogic.GetAppConfigRouted(kvp.Value.ConfigName, kvp.Value.StoreId);
                    String NewConfigValue = GetConfigValue(phStoreValues, DefaultAppConfig, true, kvp.Value.StoreId);
                    if (DefaultAppConfig.ValueType.EqualsIgnoreCase("boolean") || (DefaultAppConfig != null && NewConfigValue.ToLower() != DefaultAppConfig.ConfigValue.ToLower() && NewConfigValue.ToLower() != m_OriginalDefaultValue.ToLower()))
	                {
                        AppConfig newConfig = AppConfig.Create(DefaultAppConfig.Name, DefaultAppConfig.Description, NewConfigValue, DefaultAppConfig.GroupName, DefaultAppConfig.SuperOnly, kvp.Value.StoreId);
                        newConfig.ValueType = DefaultAppConfig.ValueType;
                        newConfig.AllowableValues = DefaultAppConfig.AllowableValues;
                        addedNew = true;
                    }
                }
            }

            OnUpdatedChanges(EventArgs.Empty);

            if (addedNew)
            {
                AppConfigManager.LoadAllConfigs();
            }

            return false;
        }

        public override void DataBind()
        {
            base.DataBind();

            // provide an "All" Option
            //if (Stores.Count > 1)
            //{
            //    Store defaultStore = this.Stores.Find(st => st.IsDefault);
            //    ListItem allStoresOption = new ListItem("All", defaultStore.StoreID.ToString());
            //    cboStores.Items.Insert(0, allStoresOption);
            //}

            cboGroupName.ClearSelection();
            cboGroupName.SelectedValue = this.Datasource.GroupName;

            m_OriginalDefaultValue = this.Datasource.ConfigValue;
            m_OriginalDefaultType = AppLogic.GetAppConfigRouted(this.Datasource.Name, 0).ValueType;

            //DetermineConfigEditControl();

            ThisCustomer = (Page as AdminPageBase).ThisCustomer;
            // selector row
            if (ThisCustomer.IsAdminSuperUser)
            {
                trAdvanced.Visible = true;
                pnlAdvanced.Visible = true;

                InitializeAdvancedConfigs();
            }
            InitStoreValues();
        } 
        #endregion

        #region Protected Instance Methods
        protected int DetermineProperTextColumnLength(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length < DEFAULT_COLUMN_LENGTH)
            {
                return DEFAULT_COLUMN_LENGTH;
            }

            int len = text.Length;
            if (len >= MAX_COLUMN_LENGTH)
            {
                return MAX_COLUMN_LENGTH;
            }
            else
            {
                return len + 10; // offset
            }
        }

        protected int DetermineProperTextRowLength(string text)
        {
            
            int rows = (text.Length / MAX_COLUMN_LENGTH);
            int offset = (text.Length % MAX_COLUMN_LENGTH);

            if (rows < MIN_ROW_LENGTH)
            {
                rows = MIN_ROW_LENGTH;
            }
            if (offset > 0)
            {
                rows++;
            }

            return rows;
        }
        #endregion

        #region Private Instance Methods
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
            cboValueType.SelectedValue = m_OriginalDefaultType;

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

        private String GetConfigValue(PlaceHolder plhConfigValueEditor, AppConfig thisConfig, Boolean findNew, int StoreId)
        {
            if (thisConfig == null)
                throw new ArgumentException("config cannot be null.");
            
            string configVal = thisConfig.ConfigValue;

            string editorId;
            if (findNew)
            {
                editorId = string.Format("ctrlNewConfig{0}_{1}", StoreId, this.Datasource.AppConfigID);
            }
            else
            {
                editorId = string.Format("ctrlConfigEditor{0}", thisConfig.AppConfigID);
            }

            if (m_OriginalDefaultType.EqualsIgnoreCase("integer"))
            {
                TextBox txt = plhConfigValueEditor.FindControl<TextBox>(editorId);
                configVal = txt.Text;
            }
            else if (m_OriginalDefaultType.EqualsIgnoreCase("decimal") || thisConfig.ValueType.EqualsIgnoreCase("double"))
            {
                TextBox txt = plhConfigValueEditor.FindControl<TextBox>(editorId);
                configVal = txt.Text;
            }
            else if (m_OriginalDefaultType.EqualsIgnoreCase("boolean"))
            {
                bool isYes = Localization.ParseBoolean(thisConfig.ConfigValue);

                RadioButtonList rbYesNo = plhConfigValueEditor.FindControl<RadioButtonList>(editorId);
                configVal = rbYesNo.Items[0].Selected.ToString().ToLowerInvariant();
            }
            else if (m_OriginalDefaultType.EqualsIgnoreCase("enum"))
            {
                DropDownList cboValues = plhConfigValueEditor.FindControl<DropDownList>(editorId);
                configVal = cboValues.SelectedValue;
            }
            else if (m_OriginalDefaultType.EqualsIgnoreCase("invoke"))
            {
                DropDownList cboValues = plhConfigValueEditor.FindControl<DropDownList>(editorId);

                configVal = cboValues.SelectedValue;
            }
            else if (m_OriginalDefaultType.EqualsIgnoreCase("multiselect"))
            {
                CheckBoxList chkValues = plhConfigValueEditor.FindControl<CheckBoxList>(editorId);

                List<string> selectedValues = new List<string>();
                foreach (ListItem option in chkValues.Items)
                {
                    if (option.Selected)
                    {
                        selectedValues.Add(option.Value);
                    }
                }

                // format the selected values into a comma delimited string
                configVal = selectedValues.ToCommaDelimitedString();
            }
            else
            {
                TextBox txt = plhConfigValueEditor.FindControl<TextBox>(editorId);
                configVal = txt.Text;
            }

            return configVal;
        }

        private void ClearErrors()
        {
            HasErrors = false;
            pnlError.Visible = false;
            lblError.Text = string.Empty;
        }

        private PlaceHolder DetermineConfigEditControl(string StoreName, int StoreId, AppConfig config)
        {
            PlaceHolder plhConfigValueEditor = new PlaceHolder();
            String editorId;
            Boolean isnew = (config == null);

            if (isnew)
            {
                //return AddParameterControl(StoreName, StoreId, config);
                config = AppLogic.GetAppConfigRouted(this.Datasource.Name, StoreId);
                editorId = string.Format("ctrlNewConfig{0}_{1}", StoreId, this.Datasource.AppConfigID);
                plhConfigValueEditor.Controls.Add(new LiteralControl("<small style=\"color:gray;\">(Unassigned for <em>{0}</em>. Currently using default.)</small><br />".FormatWith(Store.GetStoreName(StoreId))));
            }
            else
            {
                editorId = string.Format("ctrlConfigEditor{0}", config.AppConfigID);
            }


            if (config.ValueType.EqualsIgnoreCase("integer"))
            {
                TextBox txt = new TextBox();
                txt.ID = editorId;
                txt.Text = config.ConfigValue;
                m_configvalue = txt.Text;

                CompareValidator compInt = new CompareValidator();
                compInt.Type = ValidationDataType.Integer;
                compInt.Operator = ValidationCompareOperator.DataTypeCheck;
                compInt.ControlToValidate = txt.ID;
                compInt.ErrorMessage = AppLogic.GetString("admin.editquantitydiscounttable.EnterInteger", ThisCustomer.LocaleSetting);

                plhConfigValueEditor.Controls.Add(txt);
                plhConfigValueEditor.Controls.Add(compInt);
            }
            else if (config.ValueType.EqualsIgnoreCase("decimal") || config.ValueType.EqualsIgnoreCase("double"))
            {
                TextBox txt = new TextBox();
                txt.ID = editorId;
                txt.Text = config.ConfigValue;
                m_configvalue = txt.Text;

                FilteredTextBoxExtender extFilterNumeric = new FilteredTextBoxExtender();
                extFilterNumeric.ID = DB.GetNewGUID();
                extFilterNumeric.TargetControlID = editorId;
                extFilterNumeric.FilterType = FilterTypes.Numbers | FilterTypes.Custom;
                extFilterNumeric.ValidChars = ".";

                plhConfigValueEditor.Controls.Add(txt);
                plhConfigValueEditor.Controls.Add(extFilterNumeric);
            }
            else if (config.ValueType.EqualsIgnoreCase("boolean"))
            {
                bool isYes = Localization.ParseBoolean(config.ConfigValue);

                RadioButtonList rbYesNo = new RadioButtonList();
                rbYesNo.ID = editorId;
                rbYesNo.RepeatDirection = System.Web.UI.WebControls.RepeatDirection.Horizontal;

                ListItem optionYes = new ListItem("Yes");
                ListItem optionNo = new ListItem("No");

                if (StoreId == 0)
                {
                    optionYes.Attributes.Add("onclick", "$(this).parents('.modal_popup_Content').find('span.defaultTrue input').each(function (index, value) {$(value).click();});");
                    optionNo.Attributes.Add("onclick", "$(this).parents('.modal_popup_Content').find('span.defaultFalse input').each(function (index, value) {$(value).click();});");
                }
                else if (isnew)
                {
                    optionYes.Attributes.Add("class", "defaultTrue");
                    optionNo.Attributes.Add("class", "defaultFalse");  
                }

                optionYes.Selected = isYes;
                optionNo.Selected = !isYes;

                rbYesNo.Items.Add(optionYes);
                rbYesNo.Items.Add(optionNo);

                m_configvalue = optionYes.Selected.ToString().ToLowerInvariant();

                plhConfigValueEditor.Controls.Add(rbYesNo);
            }
            else if (config.ValueType.EqualsIgnoreCase("enum"))
            {
                DropDownList cboValues = new DropDownList();
                cboValues.ID = editorId;
                foreach (string value in config.AllowableValues)
                {
                    cboValues.Items.Add(value);
                }
                cboValues.SelectedValue = config.ConfigValue;

                m_configvalue = cboValues.SelectedValue;

                plhConfigValueEditor.Controls.Add(cboValues);
            }
            else if (config.ValueType.EqualsIgnoreCase("invoke"))
            {
                DropDownList cboValues = new DropDownList();
                cboValues.ID = editorId;

                bool invocationSuccessful = false;
                string errorMessage = string.Empty;

                try
                {
                    if (config.AllowableValues.Count == 3)
                    {
                        // format should be
                        // {FullyQualifiedName},MethodName
                        // Fully qualified name format is Namespace.TypeName,AssemblyName
                        string typeName = config.AllowableValues[0];
                        string assemblyName = config.AllowableValues[1];
                        string methodName = config.AllowableValues[2];

                        string fqName = typeName + "," + assemblyName;
                        Type t = Type.GetType(fqName);
                        object o = Activator.CreateInstance(t);
                        MethodInfo method = o.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                        object result = method.Invoke(o, new object[] { });

                        if (result is IEnumerable)
                        {
                            cboValues.DataSource = result;
                            cboValues.DataBind();

                            invocationSuccessful = true;

                            // now try to find which one is matching, case-insensitive
                            foreach (ListItem item in cboValues.Items)
                            {
                                item.Selected = item.Text.EqualsIgnoreCase(config.ConfigValue);
                            }
                        }
                        else
                        {
                            errorMessage = "Invocation method must return IEnumerable";
                            invocationSuccessful = false;
                        }
                    }
                    else
                    {
                        errorMessage = "Invalid invocation value";
                        invocationSuccessful = false;
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    invocationSuccessful = false;
                }

                if (invocationSuccessful == false)
                {
                    cboValues.Items.Add(errorMessage);
                }

                m_configvalue = cboValues.SelectedValue;

                plhConfigValueEditor.Controls.Add(cboValues);
            }
            else if (config.ValueType.EqualsIgnoreCase("multiselect"))
            {
                CheckBoxList chkValues = new CheckBoxList();
                chkValues.ID = editorId;

                Func<string, bool> isIncluded = (optionValue) =>
                {
                    return config.ConfigValue.Split(',').Any(val => val.Trim().EqualsIgnoreCase(optionValue));
                };

                foreach (string optionValue in config.AllowableValues)
                {
                    ListItem option = new ListItem(optionValue, optionValue);
                    option.Selected = isIncluded(optionValue);
                    chkValues.Items.Add(option);
                }

                List<string> selectedValues = new List<string>();
                foreach (ListItem option in chkValues.Items)
                {
                    if (option.Selected)
                    {
                        selectedValues.Add(option.Value);
                    }
                }

                // format the selected values into a comma delimited string
                m_configvalue = selectedValues.ToCommaDelimitedString();

                plhConfigValueEditor.Controls.Add(chkValues);
            }
            else
            {
                // determine length for proper control to use
                string configValue = config.ConfigValue;

                TextBox txt = new TextBox();
                txt.ID = editorId;
                txt.Text = configValue;

                if (configValue.Length >= MAX_COLUMN_LENGTH)
                {
                    // use textArea
                    txt.TextMode = TextBoxMode.MultiLine;
                    txt.Rows = 4; // make it 4 rows by default
                }

                txt.Columns = DetermineProperTextColumnLength(configValue);

                m_configvalue = txt.Text;

                plhConfigValueEditor.Controls.Add(txt);
            }

            return plhConfigValueEditor;
        }

        private void InitStoreValues()
        {
            //must return table rows with two columns            
            foreach (KeyValuePair<int, StoreConfigDataConstruct> item in StoreConfigValues)
            {
                phStoreValues.Controls.Add(new LiteralControl("<tr><td>" + item.Value.StoreName + "</td><td>"));
                phStoreValues.Controls.Add(item.Value.EditorPlaceHolder);
                phStoreValues.Controls.Add(new LiteralControl("</td></tr>"));
            }
        }
        #endregion

        private class StoreConfigDataConstruct
        {
            public PlaceHolder EditorPlaceHolder { get; set; }
            public String StoreName { get; set; }
            public String ConfigName { get; set; }
            public int StoreId { get; set; }
            public AppConfig EditingConfig { get; set; }
        }
    }

    
}


