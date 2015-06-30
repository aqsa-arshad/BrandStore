// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Displays the Locale Select List dropdown
    /// </summary>
    public class LanguageSelectList : BaseSelectList
    {
        /// <summary>
        /// Gets whether to display this control
        /// </summary>
        protected override bool ShouldRender
        {
            get
            {
                return AppLogic.NumLocaleSettingsInstalled() > 1;
            }
        }

        /// <summary>
        /// Initializes the datasource
        /// </summary>
        /// <param name="datasource"></param>
        protected override void InitializeDataSource(ListItemCollection datasource)
        {
            string query = "select name,description from LocaleSetting   with (NOLOCK)  order by displayorder,description";

            Action<IDataReader> readAction = rs =>
            {
                while (rs.Read())
                {
                    datasource.Add(new ListItem(DB.RSField(rs, "description"), DB.RSField(rs, "name")));
                }
            };

            DB.UseDataReader(query, readAction);

            this.SelectedValue = ThisCustomer.LocaleSetting;
        }

        /// <summary>
        /// Performs the necessary action for this control per postback
        /// </summary>
        /// <param name="selectedValue"></param>
        protected override void DoPostbackRoutine(string selectedValue)
        {
            ThisCustomer.RequireCustomerRecord();
            ThisCustomer.LocaleSetting = selectedValue;

            String CurrencySetting = AppLogic.GetLocaleDefaultCurrency(selectedValue);
            if (CommonLogic.IsStringNullOrEmpty(CurrencySetting))
            {
                CurrencySetting = CommonLogic.QueryStringCanBeDangerousContent("Currency");
            }
            if (CommonLogic.IsStringNullOrEmpty(CurrencySetting))
            {
                CurrencySetting = Localization.GetPrimaryCurrency();
            }

            AppLogic.CheckForScriptTag(CurrencySetting);
            CurrencySetting = Localization.CheckCurrencySettingForProperCase(CurrencySetting);
            ThisCustomer.CurrencySetting = CurrencySetting;

            RefreshPage();
        }
    }
}
