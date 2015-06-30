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
using System.Text;
using System.Text.RegularExpressions;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class EditAppConfigInput : UserControl
    {
        #region Private Instance Properties
        private bool _DataSourceExists = false;
        private AppConfig _PassedConfig;
        private AppConfig _DataSource;
        #endregion

        #region Public Instance Properties
        public AppConfig DataSource
        {
            get
            {
                return _DataSource;
            }
            set
            {
                _PassedConfig = value;
                _DataSourceExists = AppConfigManager.AppConfigExists(value.StoreId, value.Name);
                if (_DataSourceExists)
                    _DataSource = AppConfigManager.GetAppConfig(value.StoreId, value.Name);
                else
                    _DataSource = value;
            }
        }
        public int TargetStoreId 
        {
            get
            {
                return StoreId.Value.ToNativeInt();
            }
            set
            {
                StoreId.Value = value.ToString();
            }
        }
        public string Value
        {
            get { return GetInputControlValue(); }
        }
        public string GetAppConfigName()
        {
            int acId;
            if (int.TryParse(AppConfigId.Value, out acId))
	        {
                AppConfig ac = new AppConfig(acId);
                return ac.Name;
	        }
            return null;
        }
        public Boolean InheritingDefault
        {
            get
            {
                return pnlInheritWarning.Visible;
            }
        }
        #endregion

        #region EventHandler
        protected void btnSave_Click(object sender, EventArgs e)
        {
            Save(new List<string>());
        }
        #endregion

        #region Public Instance Methods

        public void Save()
        {
            Save(new List<string>());
        }
        public void Save(List<string> dontDupStoresForValues)
        {
            String value = GetInputControlValue();
            if (value == null)
                return;

            int updateAppConfg;

            if (!String.IsNullOrEmpty(AppConfigId.Value) && int.TryParse(AppConfigId.Value, out updateAppConfg))
            {
                AppConfig acDS = new AppConfig(updateAppConfg);
                if (TargetStoreId < 1)
                    TargetStoreId = acDS.StoreId;

                if (rblValue.Visible) //boolean - always duplicate config
                    dontDupStoresForValues.Clear();

                AppConfigManager.SetAppConfigDBAndCache(TargetStoreId, acDS.Name, value, dontDupStoresForValues);
            }
            else
            {
                throw new NotImplementedException("New app configs not implemented.");
            }
        }
        public override void DataBind()
        {
            if (DataSource == null)
                return;
            initConfigValue();

            base.DataBind();
        }
        #endregion

        #region Private Instance Methods
        private void initConfigValue()
        {
            if (DataSource.AppConfigID < 1)
                return;

            if (DataSource.StoreId == 0 && TargetStoreId > 0)
            {
                litStoreName.Text = Store.GetStoreName(TargetStoreId);
                pnlInheritWarning.Visible = true;
            }
            else
                pnlInheritWarning.Visible = false;

            AppConfigId.Value = DataSource.AppConfigID.ToString();
            AppConfigType type = AppConfig.ParseTypeFromString(DataSource.ValueType);
            phValidators.Controls.Clear();
            tbxValue.Visible = ddValue.Visible = cblValue.Visible = rblValue.Visible = false;
            switch (type)
            {
                case AppConfigType.@string:
                    tbxValue.Visible = true;
                    tbxValue.Text = DataSource.ConfigValue;
					//tbxValue.CssClass = DataSource.Name.Replace(".", string.Empty);
                    break;
                case AppConfigType.boolean:
                    rblValue.Visible = true;
                    rblValue.SelectedValue = DataSource.ConfigValue.ToBool().ToString().ToLower();
					//rblValue.CssClass = DataSource.Name.Replace(".", string.Empty);
                    pnlInheritWarning.Visible = false;
                    break;
                case AppConfigType.integer:
                    tbxValue.Visible = true;
                    tbxValue.Text = DataSource.ConfigValue;
					//tbxValue.CssClass = DataSource.Name.Replace(".", string.Empty);
                    RegularExpressionValidator regExInt = new RegularExpressionValidator();
                    regExInt.ValidationExpression = @"^-{0,1}\d+$";
					regExInt.ErrorMessage = String.Format(AppLogic.GetString("admin.atom.integerError", Localization.GetDefaultLocale()), DataSource.Name);
                    regExInt.ControlToValidate = tbxValue.ID;
                    phValidators.Controls.Add(regExInt);
                    break;
                case AppConfigType.@decimal:
                case AppConfigType.@double:
                    tbxValue.Visible = true;
                    tbxValue.Text = DataSource.ConfigValue;
					//tbxValue.CssClass = DataSource.Name.Replace(".", string.Empty);
                    FilteredTextBoxExtender extFilterNumeric = new FilteredTextBoxExtender();
                    extFilterNumeric.TargetControlID = tbxValue.ID;
                    extFilterNumeric.FilterType = FilterTypes.Numbers | FilterTypes.Custom;
                    extFilterNumeric.ValidChars = ".";
                    phValidators.Controls.Add(extFilterNumeric);
                    break;
                case AppConfigType.@enum:
                    ddValue.Visible = true;
                    ddValue.Items.Clear();
					ddValue.CssClass = DataSource.Name.Replace(".", string.Empty);
                    foreach (string value in DataSource.AllowableValues)
                        ddValue.Items.Add(value);
                    if (ddValue.Items.FindByValue(DataSource.ConfigValue) != null)
                        ddValue.SelectedValue = DataSource.ConfigValue;
                    break;
                case AppConfigType.multiselect:
                    cblValue.Visible = true;
                    cblValue.Items.Clear();
					//cblValue.CssClass = DataSource.Name.Replace(".", string.Empty);
                    Func<string, bool> isIncluded = (optionValue) =>
                    {
                        return DataSource.ConfigValue.Split(',').Any(val => val.Trim().EqualsIgnoreCase(optionValue));
                    };

                    foreach (string optionValue in DataSource.AllowableValues)
                    {
                        ListItem option = new ListItem(optionValue, optionValue);
                        option.Selected = isIncluded(optionValue);
                        cblValue.Items.Add(option);
                    }

                    List<string> selectedValues = new List<string>();
                    foreach (ListItem option in cblValue.Items)
                    {
                        if (option.Selected)
                        {
                            selectedValues.Add(option.Value);
                        }
                    }
                    break;
                case AppConfigType.invoke:
                    ddValue.Visible = true;
                    ddValue.Items.Clear();
					//ddValue.CssClass = DataSource.Name.Replace(".", string.Empty);
                    bool invocationSuccessful = false;
                    string errorMessage = string.Empty;

                    try
                    {
                        if (DataSource.AllowableValues.Count == 3)
                        {
                            // format should be
                            // {FullyQualifiedName},MethodName
                            // Fully qualified name format is Namespace.TypeName,AssemblyName
                            string typeName = DataSource.AllowableValues[0];
                            string assemblyName = DataSource.AllowableValues[1];
                            string methodName = DataSource.AllowableValues[2];

                            string fqName = typeName + "," + assemblyName;
                            Type t = Type.GetType(fqName);
                            object o = Activator.CreateInstance(t);
                            MethodInfo method = o.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                            object result = method.Invoke(o, new object[] { });

                            if (result is IEnumerable)
                            {
                                ddValue.DataSource = result;
                                ddValue.DataBind();

                                invocationSuccessful = true;

                                if (ddValue.Items.FindByValue(DataSource.ConfigValue) != null)
                                    ddValue.SelectedValue = DataSource.ConfigValue;
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
                        ddValue.Items.Add(errorMessage);

                    break;
                default:
                    break;
            }
        }
        private String GetInputControlValue()
        {
            
            if (cblValue.Visible)
            {
                StringBuilder sb = new StringBuilder();
                foreach (ListItem li in cblValue.Items)
                    if (li.Selected)
                        sb.Append("," + li.Value);
                if (sb.ToString().Length > 1)
                    return sb.ToString().Substring(1);
                return "";
            } 
            else if (rblValue.Visible)
                return rblValue.SelectedValue;
            else if (ddValue.Visible)
                return ddValue.SelectedValue;
            return tbxValue.Text;
        }
        #endregion
    }
}


