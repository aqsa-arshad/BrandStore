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
    /// Displays the Currency Select List dropdown
    /// </summary>
    public class CurrencySelectList : BaseSelectList
    {
        /// <summary>
        /// Gets whether to display this control
        /// </summary>
        protected override bool ShouldRender
        {
            get
            {
                return Currency.NumPublishedCurrencies() > 1;
            }
        }

        /// <summary>
        /// Initializes the datasource
        /// </summary>
        /// <param name="datasource"></param>
        protected override void InitializeDataSource(ListItemCollection datasource)
        {
            string query = "select CurrencyCode,Name from Currency  with (NOLOCK) where Published=1  order by Published desc, DisplayOrder,Name";

            Action<IDataReader> readAction = rs =>
            {
                while (rs.Read())
                {
                    string currencyCode = DB.RSField(rs, "CurrencyCode");
                    string currencyName = DB.RSField(rs, "Name");

                    datasource.Add(new ListItem(String.Format("{0}({1})", currencyCode, currencyName), currencyCode));
                }
            };

            DB.UseDataReader(query, readAction);

            this.SelectedValue = ThisCustomer.CurrencySetting;
        }

        /// <summary>
        /// Performs the necessary action for this control per postback
        /// </summary>
        /// <param name="selectedValue"></param>
        protected override void DoPostbackRoutine(string selectedValue)
        {
            String CurrencySetting = Localization.CheckCurrencySettingForProperCase(selectedValue);

            Customer thisCustomer = Customer.Current;
            thisCustomer.CurrencySetting = CurrencySetting;

            RefreshPage();            
        }

    }

}
