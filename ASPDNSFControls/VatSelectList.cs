// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Displays the VAT Select List dropdown
    /// </summary>
    public class VatSelectList : BaseSelectList
    {
        /// <summary>
        /// Gets whether to display this control
        /// </summary>
        protected override bool ShouldRender
        {
            get
            {
                return AppLogic.VATIsEnabled() && AppLogic.AppConfigBool("VAT.AllowCustomerToChooseSetting");
            }
        }

        /// <summary>
        /// Initializes the datasource
        /// </summary>
        /// <param name="datasource"></param>
        protected override void InitializeDataSource(System.Web.UI.WebControls.ListItemCollection datasource)
        {
            String msg2 = AppLogic.GetString("setvatsetting.aspx.3", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            String msg3 = AppLogic.GetString("setvatsetting.aspx.4", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

            datasource.Add(new ListItem(msg2, ((int)VATSettingEnum.ShowPricesInclusiveOfVAT).ToString()));
            datasource.Add(new ListItem(msg3, ((int)VATSettingEnum.ShowPricesExclusiveOfVAT).ToString()));

            this.SelectedValue = ((int)ThisCustomer.VATSettingRAW).ToString();
        }

        /// <summary>
        /// Performs the necessary action for this control per postback
        /// </summary>
        /// <param name="selectedValue"></param>
        protected override void DoPostbackRoutine(string selectedValue)
        {
            String VATSetting = Customer.ValidateVATSetting(selectedValue);
            String vtr = "";

            ThisCustomer.RequireCustomerRecord();

            VATSettingEnum vt = (VATSettingEnum)System.Int32.Parse(VATSetting);
            ThisCustomer.VATSettingRAW = vt;

			Exception vatServiceException;
			if (vtr.Length != 0 && !AppLogic.VATRegistrationIDIsValid(ThisCustomer, vtr, out vatServiceException))
            {
                vtr = String.Empty;
            }
            if (ThisCustomer.VATRegistrationID != vtr)
            {
                ThisCustomer.SetVATRegistrationID(vtr);
            }

            RefreshPage();
        }
    }
}
