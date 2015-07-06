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
    public partial class GlobalConfigEdit : BaseUserControl<GlobalConfig>
    {
        private string m_cssclass;
        public string CssClass
        {
            get { return m_cssclass; }
            set { m_cssclass = value; }
        }

        private IEnumerable<string> m_configgroups;
        public IEnumerable<string> ConfigGroups
        {
            get { return m_configgroups; }
            set { m_configgroups = value; }
        }

        private const int DEFAULT_COLUMN_LENGTH = 35;
        private const int MAX_COLUMN_LENGTH = 75;
        private const int MAX_ROW_LENGTH = 4;

        public override void DataBind()
        {
            base.DataBind();

            cboGroupName.ClearSelection();
            cboGroupName.SelectedValue = this.Datasource.GroupName;

            DetermineConfigEditControl();

            ThisCustomer = (Page as AdminPageBase).ThisCustomer;
            // selector row
            if (ThisCustomer.IsAdminSuperUser)
            {
                trAdvanced.Visible = true;
                pnlAdvanced.Visible = true;

                InitializeAdvancedConfigs();
            }
        }

        public Func<string> GetConfigValue { get; private set; }

        private void DetermineConfigEditControl()
        {
            // clear out the controls collection for would be-new type of controls
            plhConfigValueEditor.Controls.Clear();

            GlobalConfig config = this.Datasource;
            string editorId = string.Format("ctrlConfigEditor{0}", config.ID);

            if (config.ValueType.EqualsIgnoreCase("integer"))
            {
                TextBox txt = new TextBox();
                txt.ID = editorId;
                txt.Text = config.ConfigValue;
                GetConfigValue = () => { return txt.Text; };

                FilteredTextBoxExtender extFilterInt = new FilteredTextBoxExtender();
                extFilterInt.ID = "extFilterInt";
                extFilterInt.TargetControlID = editorId;
                extFilterInt.FilterType = FilterTypes.Numbers;

                plhConfigValueEditor.Controls.Add(txt);
                plhConfigValueEditor.Controls.Add(extFilterInt);
            }
            else if (config.ValueType.EqualsIgnoreCase("decimal") || config.ValueType.EqualsIgnoreCase("double"))
            {
                TextBox txt = new TextBox();
                txt.ID = editorId;
                txt.Text = config.ConfigValue;
                GetConfigValue = () => { return txt.Text; };

                FilteredTextBoxExtender extFilterNumeric = new FilteredTextBoxExtender();
                extFilterNumeric.ID = "extFilterNumeric";
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

                optionYes.Selected = isYes;
                optionNo.Selected = !isYes;

                rbYesNo.Items.Add(optionYes);
                rbYesNo.Items.Add(optionNo);

                GetConfigValue = () => { return optionYes.Selected.ToString().ToLowerInvariant(); };

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

                GetConfigValue = () => { return cboValues.SelectedValue; };

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

                GetConfigValue = () => { return cboValues.SelectedValue; };

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

                GetConfigValue = () =>
                {
                    var selectedValues = new List<string>();
                    foreach (ListItem option in chkValues.Items)
                    {
                        if (option.Selected)
                        {
                            selectedValues.Add(option.Value);
                        }
                    }

                    // format the selected values into a comma delimited string
                    return selectedValues.ToCommaDelimitedString();
                };

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

                GetConfigValue = () => { return txt.Text; };

                plhConfigValueEditor.Controls.Add(txt);
            }
        }

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


        private const int MIN_ROW_LENGTH = 2;

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

        private void ClearErrors()
        {
            HasErrors = false;
            pnlError.Visible = false;
            lblError.Text = string.Empty;
        }

        public override bool UpdateChanges()
        {
            var thisConfig = this.Datasource;
            
            thisConfig.ConfigValue = GetConfigValue();
            thisConfig.GroupName = cboGroupName.SelectedValue;
            thisConfig.SuperOnly = rbSuperOnly.Items[0].Selected; // 0 is Yes
            thisConfig.ValueType = cboValueType.SelectedValue;
            if (!string.IsNullOrEmpty(txtAllowableValues.Text))
            {
                var allowableValues = new List<string>();
                allowableValues.AddCommaDelimited(txtAllowableValues.Text);
                thisConfig.AllowableValues = allowableValues;
            }

            thisConfig.Save();

            OnUpdatedChanges(EventArgs.Empty);

            return true;
        }

        private void InitializeAdvancedConfigs()
        {
            var thisConfig = Datasource;

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
            var types = new Dictionary<string, string>();
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
