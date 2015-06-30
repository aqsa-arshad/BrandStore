// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using System.Text.RegularExpressions;
using AspDotNetStorefront;
using AspDotNetStorefrontGateways;
using System.Collections;

namespace AspDotNetStorefront
{
    public partial class SecureNetVaultPanel : System.Web.UI.UserControl
    {
        Customer ThisCustomer;

        protected void Page_Load(object sender, EventArgs e)
        {
            ThisCustomer = (Page as SkinBase).ThisCustomer;
            InitSecureNetVault();
        }

        private void InitSecureNetVault()
        {
            if (AppLogic.ActivePaymentGatewayCleaned() != Gateway.ro_GWSECURENETVAULTV4 || !AppLogic.SecureNetVaultIsEnabled())
                return;

            pnlSecureNetVaultCards.Visible = true;
            litSNCError.Text = "";

            lbAddNewAddress.Text = "account.addcreditcard".StringResource();

            if (Page.IsPostBack)
                return;

            LoadData();
        }

        private void LoadData()
        {
            List<CustomerVaultPayment> ds;
            CustomerVault cv = SecureNetDataReport.GetCustomerVault(ThisCustomer.CustomerID);
            if (cv != null)
                ds = SecureNetDataReport.GetCustomerVault(ThisCustomer.CustomerID).SavedPayments;
            else
                ds = new List<CustomerVaultPayment>();

            dlSecureNetVault.Visible = ds.Count > 0;
            dlSecureNetVault.DataSource = ds;
            dlSecureNetVault.DataBind();
        }

        public void btnSecureNetSaveCard_Click(object sender, EventArgs e)
        {
            Page.Validate("SecureNetAddCard");
            if (!Page.IsValid)
            {
                return;
            }
            SecureNetVault snv = new SecureNetVault(ThisCustomer);
            try
            {
                ServiceResponse response = snv.AddCreditCardToCustomerVault(SecureNetCreditCardPanel.CreditCardName, SecureNetCreditCardPanel.CreditCardNumber, SecureNetCreditCardPanel.CreditCardVerCd, SecureNetCreditCardPanel.CreditCardType, SecureNetCreditCardPanel.CardExpMonth, SecureNetCreditCardPanel.CardExpYr);
                if (!response.HasError)
                {
                    LoadData();
                    pnlAddSecureNetCard.Visible = false;
                    pnlAddSecureNetCardPrompt.Visible = true;
                    SecureNetCreditCardPanel.Clear();
                }
                else
                {
                    litSNCError.Text = response.Message + "";
                }
            }
            catch
            {
                litSNCError.Text = "There was an error adding your credit card. Please validate the provided information and try again.";
            }
            
        }

        protected void dlSecureNetVault_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem))
            {

            }
            else if (e.Item.ItemType == ListItemType.Footer)
            {

            }
        }

        protected void dlSecureNetVault_DeleteCommand(object sender, DataListCommandEventArgs e)
        {
            HiddenField hfVaultId = e.Item.FindControl("hfVaultId") as HiddenField;
            if (hfVaultId != null && !string.IsNullOrEmpty(hfVaultId.Value))
            {
                SecureNetVault vault = new SecureNetVault(ThisCustomer);
                try
                {
                    vault.DeletePaymentMethod(hfVaultId.Value);
                }
                catch { }
            }
            LoadData();
        }

        protected void AddNewVaultCard(object sender, EventArgs e)
        {
            pnlAddSecureNetCard.Visible = true;
            pnlAddSecureNetCardPrompt.Visible = false;
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "initAddCCPanel", string.Format(SecureNetCreditCardPanel.InitJS, "ctl00_PageContent_ctl02_SecureNetCreditCardPanel_hlnkWhat"), true);
        }
    }
}
