// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.Settings;
using Vortx.OnePageCheckout.UI;
using Vortx.OnePageCheckout.Views;

public partial class OPCControls_Addresses_BillingAddressChoice :
    OPCUserControl<IAccountModel>,
    IAccountView
{
    public IAddressView BillingAddressUSEditView
    {
        get { return BillingAddressUSEditViewControl; }
    }
    public IAddressView BillingAddressStaticView
    {
        get { return BillingAddressStaticViewControl; }
    }

    public IAddressView BillingAddressUKEditView
    {
        get { return this.BillingAddressUKEditViewControl; }
    }

    public IAccountView AddressBookView
    {
        get { return this.AddressBook1; }
    }

    #region Private Methods

    private void ToggleShowBillSameAsShip(bool same)
    {
        if (same)
        {
            HyperLinkBillingAddressBook.Visible = false;

            if (ConfigurationProvider.DefaultProvider.AddressLocale.Equals("UK"))
            {
                BillingAddressUKEditView.Hide();
            }
            else
            {
                BillingAddressUSEditView.Hide();
            }

            PHBillingAddressStatic.Visible = true;
            PHBillingAddressEdit.Visible = false;
            BillingAddressStaticView.Show();
            BillingAddressStaticView.BindView();
        }
        else
        {
            HyperLinkBillingAddressBook.Visible = this.AddressBookView.Model.IsRegistered;

            if (ConfigurationProvider.DefaultProvider.AddressLocale.Equals("UK"))
            {
                BillingAddressUKEditView.Initialize();
                BillingAddressUKEditView.Show();
                BillingAddressUKEditView.Enable();
                BillingAddressUKEditView.BindView();
            }
            else
            {
                BillingAddressUSEditView.Initialize();
                BillingAddressUSEditView.Show();
                BillingAddressUSEditView.Enable();
                BillingAddressUSEditView.BindView();
            }
            PHBillingAddressStatic.Visible = false;
            PHBillingAddressEdit.Visible = true;
        }
    }

    #endregion

    #region Event Handlers
	
    protected void BillSameNo_CheckedChanged(object sender, EventArgs e)
    {
        this.Model.BillingEqualsShipping = false;
        ToggleShowBillSameAsShip(false);
    }

    protected void BillSameYes_CheckedChanged(object sender, EventArgs e)
    {
        this.Model.BillingEqualsShipping = true;
        ToggleShowBillSameAsShip(true);
    }

    #endregion

    #region IView Members
    
    public override void SetModel(IAccountModel model, IStringResourceProvider stringResourceProvider)
    {
        base.SetModel(model, stringResourceProvider);

        this.BillingAddressUKEditView.SetModel(this.Model.BillingAddress, this.StringResourceProvider);
        this.BillingAddressUSEditView.SetModel(this.Model.BillingAddress, this.StringResourceProvider);
        this.BillingAddressStaticView.SetModel(this.Model.BillingAddress, this.StringResourceProvider);
        this.AddressBookView.SetModel(this.Model, this.StringResourceProvider);
    }

    public override void BindView()
    {
        if (!Vortx.OnePageCheckout.Settings.ConfigurationProvider.DefaultProvider.AllowSeparateBillingAndShipping)
        {
            if (this.Model.ShippingRequired && !this.Model.BillingEqualsShipping)
            {
                this.Model.BillingEqualsShipping = true;
            }
        }
        PanelAddressBook.Visible = Vortx.OnePageCheckout.Settings.ConfigurationProvider.DefaultProvider.AllowSeparateBillingAndShipping;

		this.BillSameYes.Checked = this.Model.ShippingRequired && this.Model.BillingEqualsShipping;
        this.BillSameNo.Checked = !this.BillSameYes.Checked;

        this.PanelBillSame.Visible = this.Model.ShippingRequired &&
            Vortx.OnePageCheckout.Settings.ConfigurationProvider.DefaultProvider.AllowSeparateBillingAndShipping;

        this.AddressBookView.BindView();

        if (this.BillSameYes.Visible && this.BillSameYes.Checked)
        {
            this.BillingAddressStaticView.StringResourceProvider = this.StringResourceProvider;
            this.BillingAddressStaticView.SetModel(this.AddressBookView.Model.ShippingAddress, StringResourceProvider);
        }

        if (this.Model.ShippingRequired)
        {
            ToggleShowBillSameAsShip(this.BillSameYes.Checked);
        }
        else
        {
            ToggleShowBillSameAsShip(false);
        }

    }

    public override void BindView(object id)
    {
    }

    public override void Initialize()
    {
        if (ConfigurationProvider.DefaultProvider.AddressLocale.Equals("US"))
        {
            this.BillingAddressUSEditView.Initialize();
        }
        else
        {
            this.BillingAddressUKEditView.Initialize();
        }
        this.BillingAddressStaticView.Initialize();
    }

    public override void Disable()
    {
        if (ConfigurationProvider.DefaultProvider.AddressLocale.Equals("US"))
        {
            this.BillingAddressUSEditView.Disable();
        }
        else
        {
            this.BillingAddressUKEditView.Disable();

        }
        this.BillingAddressStaticView.Disable();
        this.BillSameNo.Enabled = false;
        this.BillSameYes.Enabled = false;
    }

    public override void Enable()
    {
        this.BillingAddressUSEditView.Enable();
        this.BillingAddressStaticView.Enable();
        this.BillSameNo.Enabled = true;
        this.BillSameYes.Enabled = true;
    }

    public override void Show()
    {
        this.Visible = true;

        this.PanelBillSame.Visible = this.Model.ShippingRequired;

        this.UpdateBillingAddressChoice.Visible = true;
        this.ToggleShowBillSameAsShip(BillSameYes.Visible && BillSameYes.Checked);
    }

    public override void Hide()
    {
        this.Visible = false;
        this.UpdateBillingAddressChoice.Visible = false;
        this.UpdateBillingAddressChoice.Update();
    }

    public override void ShowError(string message)
    {

    }

    public override void SaveViewToModel()
    {
        // don't save addresses if billing equals shipping
        if (this.Model.BillingEqualsShipping && this.BillSameYes.Visible)
            return;

        if (ConfigurationProvider.DefaultProvider.AddressLocale.Equals("UK"))
        {
            BillingAddressUKEditView.SaveViewToModel();
        }
        else
        {
            BillingAddressUSEditView.SaveViewToModel();
        }
    }

    #endregion
}
