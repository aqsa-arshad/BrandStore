// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.Settings;
using Vortx.OnePageCheckout.UI;
using Vortx.OnePageCheckout.Views;
using Vortx.OnePageCheckout.WebUtility;

public partial class VortxControls_BillingAddressUKEdit :
    OPCUserControl<IAddressModel>,
    IAddressView
{
    #region Page Events
    
    protected void BillZipCityState_OnChanged(object sender, EventArgs e)
    {
        // Encountered a problem where billing address was not being saved, but overwritten by Billping address.
        SaveViewToModel();

        this.UpdatePanelBillingAddressWrap.Update();
    }

    protected void FindBillAddress_Click(object sender, EventArgs e)
    {
        // Remove all white space
        BillZip.Text = BillZip.Text.Replace(" ", "");

        if (BillZip.Text.Length == 5)
        {
            BillZip.Text = BillZip.Text.Insert(2, " ");
        }
        else if (BillZip.Text.Length == 6)
        {
            BillZip.Text = BillZip.Text.Insert(3, " ");
        }
        else if (BillZip.Text.Length == 7)
        {
            BillZip.Text = BillZip.Text.Insert(4, " ");
        }

        BillZip.Text = BillZip.Text.ToUpper();
        this.UpdatePanelBillingAddressWrap.Update();
    }

    #endregion

    #region Private Methods

    #endregion

    #region IView Members

    public override void BindView()
    {
        if (!PageUtility.IsAsyncPostBackForControl(this, ConfigurationProvider.DefaultProvider.ScriptManagerId))
        {
            if (!string.IsNullOrEmpty(this.Model.Country))
            {
                BillFirstName.Text = Model.FirstName;
                BillLastName.Text = Model.LastName;
                BillZip.Text = Model.PostalCode;
                BillAddress1.Text = Model.Address1;
                BillAddress2.Text = Model.Address2;
                BillCounty.Text = Model.State;
                BillCity.Text = Model.City;
				BillPhone.Text = Model.Phone ?? string.Empty;
  
                this.UpdatePanelBillingAddressWrap.Update();
            };
        }
    }

    public override void BindView(object id)
    {
    }

    public override void SaveViewToModel()
    {
        Page.Validate("VGBillingAddress");

        if (Page.IsValid)
        {            
			this.Model.FirstName = this.BillFirstName.Text;
			this.Model.LastName = this.BillLastName.Text;
			this.Model.Address1 = this.BillAddress1.Text;
			this.Model.Address2 = this.BillAddress2.Text;
			this.Model.City = this.BillCity.Text;
			this.Model.State = this.BillCounty.Text;
			this.Model.PostalCode = this.BillZip.Text;
			this.Model.Country = "United Kingdom";
            this.Model.Email = AspDotNetStorefrontCore.Customer.Current.EMail;
			this.Model.Phone = this.BillPhone.Text;

			this.Model.Save();
        }
    }

    public override void Initialize()
    {
        if (!PageUtility.IsAsyncPostBackForControl(this, ConfigurationProvider.DefaultProvider.ScriptManagerId))
        {
            BillFirstName.Text = string.Empty;
            BillLastName.Text = string.Empty;
            BillZip.Text = string.Empty;
            BillAddress1.Text = string.Empty;
            BillAddress2.Text = string.Empty;
            BillCounty.Text = string.Empty;
            BillCity.Text = string.Empty;
			BillPhone.Text = string.Empty;
        }
    }

    public override void Disable()
    {
        this.BillFirstName.Enabled = false;
		this.BillLastName.Enabled = false;
		this.BillZip.Enabled = false;
		this.BillCity.Enabled = false;
		this.BillCounty.Enabled = false;
		this.BillAddress1.Enabled = false;
		this.BillAddress2.Enabled = false;
		this.BillPhone.Enabled = false;

		PageUtility.AddClass(BillAddressTable, "disabled");

		this.UpdatePanelBillingAddressWrap.Update();
    }

    public override void Enable()
    {
		BillFirstName.Enabled = true;
		BillLastName.Enabled = true;
		BillZip.Enabled = true;
		BillCity.Enabled = true;
		BillCounty.Enabled = true;
		BillAddress1.Enabled = true;
		BillAddress2.Enabled = true;
		BillPhone.Enabled = true;

		PageUtility.RemoveClass(BillAddressTable, "disabled");
        
        this.UpdatePanelBillingAddressWrap.Update();
    }

    public override void Show()
    {
        this.Visible = true;
        this.UpdatePanelBillingAddressWrap.Visible = true;
        this.UpdatePanelBillingAddressWrap.Update();
    }

    public override void Hide()
    {
        this.Visible = false;
        this.UpdatePanelBillingAddressWrap.Visible = true;
        this.UpdatePanelBillingAddressWrap.Update();
    }

    public override void ShowError(string message)
    {
        PanelError.Visible = true;
        LabelError.Text = message;
    }

    #endregion

}
