using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{
    public partial class JWMyAccount : SkinBase
    {
        /// <summary>
        /// Override JeldWen Master Template
        /// </summary>
        protected override string OverrideTemplate()
        {
            String MasterHome = AppLogic.HomeTemplate();

            if (MasterHome.Trim().Length == 0)
            {
                MasterHome = "JeldWenTemplate";
            }

            if (MasterHome.EndsWith(".ascx"))
            {
                MasterHome = MasterHome.Replace(".ascx", ".master");
            }

            if (!MasterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
            {
                MasterHome = MasterHome + ".master";
            }

            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + base.SkinID.ToString() + "/" + MasterHome)))
            {
                MasterHome = "JeldWenTemplate";
            }

            return MasterHome;
        }

        /// <summary>
        /// Page Load Event
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            RequireSecurePage();
            RequiresLogin(CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING"));

            if (!Page.IsPostBack)
            {
                LoadAddresses();
            }
        }

        /// <summary>
        /// Load Primary Billing & Shipping Addresses
        /// </summary>
        private void LoadAddresses()
        {
            try
            {
                if (ThisCustomer.PrimaryBillingAddressID == 0)
                    lblBANA.Text = "N/A";
                else if (ThisCustomer.PrimaryBillingAddress != null)
                {
                    lblBAFullName.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.FirstName) ? "" : ThisCustomer.PrimaryBillingAddress.FirstName + " " + ThisCustomer.PrimaryBillingAddress.LastName;
                    lblBAAddress1.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.Address1) ? "" : ThisCustomer.PrimaryBillingAddress.Address1;

                    lblBAStateZip.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.State) ? "" : ThisCustomer.PrimaryBillingAddress.State;

                    if (!string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.State))
                        lblBAStateZip.Text += string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.Zip) ? "" : ", " + ThisCustomer.PrimaryBillingAddress.Zip;
                    else
                        lblBAStateZip.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.Zip) ? "" : ThisCustomer.PrimaryBillingAddress.Zip;

                    lblBACountry.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.Country) ? "" : ThisCustomer.PrimaryBillingAddress.Country;
                    lblBAPhone.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.Phone) ? "" : ThisCustomer.PrimaryBillingAddress.Phone;
                }

                if (ThisCustomer.PrimaryShippingAddressID == 0)
                    lblSANA.Text = "N/A";
                else if (ThisCustomer.PrimaryShippingAddress != null)
                {
                    lblSAFullName.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.FirstName) ? "" : ThisCustomer.PrimaryShippingAddress.FirstName + " " + ThisCustomer.PrimaryShippingAddress.LastName;
                    lblSAAddress1.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.Address1) ? "" : ThisCustomer.PrimaryShippingAddress.Address1;

                    lblSAStateZip.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.State) ? "" : ThisCustomer.PrimaryShippingAddress.State;

                    if (!string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.State))
                        lblSAStateZip.Text += string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.Zip) ? "" : ", " + ThisCustomer.PrimaryShippingAddress.Zip;
                    else
                        lblSAStateZip.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.Zip) ? "" : ThisCustomer.PrimaryShippingAddress.Zip;

                    lblSACountry.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.Country) ? "" : ThisCustomer.PrimaryShippingAddress.Country;
                    lblSAPhone.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.Phone) ? "" : ThisCustomer.PrimaryShippingAddress.Phone;
                }
            }
            catch (Exception ex)
            {

                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }

        /// <summary>
        /// View All Billing Addresses
        /// </summary>
        protected void btnChangeBillingAddress_Click(object sender, EventArgs e)
        {
            Response.Redirect("JWMyAddresses.aspx?AddressType=" + (int)AddressTypes.Billing);
        }

        /// <summary>
        /// View All Shipping Addresses
        /// </summary>
        protected void btnChangeShippingAddress_Click(object sender, EventArgs e)
        {
            Response.Redirect("JWMyAddresses.aspx?AddressType=" + (int)AddressTypes.Shipping);
        }
    }
}