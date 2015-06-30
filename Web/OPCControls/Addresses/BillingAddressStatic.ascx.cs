// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.UI;
using Vortx.OnePageCheckout.Views;

public partial class OPCControls_BillingAddressStatic :
    OPCUserControl<IAddressModel>,
    IAddressView
{
	#region Page Events

	protected void EditAddress_Click(object sender, EventArgs e)
	{
		FireAddressEdit();
	}

	#endregion

	#region IAddressView Members

	public override void BindView()
	{
		this.FirstName.Text = this.Model.FirstName;
		this.LastName.Text = this.Model.LastName;
		this.Address1.Text = this.Model.Address1;
		this.Address2.Text = this.Model.Address2;
		this.Apartment.Text = this.Model.Apartment;
		this.City.Text = this.Model.City;
		this.State.Text = this.Model.State;
		this.Zip.Text = this.Model.PostalCode;
		this.Phone.Text = this.Model.Phone;
		this.Country.Text = this.Model.Country;
        this.Company.Text = this.Model.Company;

        if (String.IsNullOrEmpty(this.Company.Text))
        {
            PanelCompany.Visible = false;
        }
        else
            PanelCompany.Visible = true;

		if(String.IsNullOrEmpty(this.Address2.Text))
		{
			PanelAddressLine2.Visible = false;
		}
		else
		{
			PanelAddressLine2.Visible = true;
		}
		if (String.IsNullOrEmpty(this.Apartment.Text))
		{
			PanelApartment.Visible = false;
		}
		else
		{
			PanelApartment.Visible = true;
		}
		if(String.IsNullOrEmpty(this.Phone.Text))
		{
			PanelPhone.Visible = false;
		}
		else
		{
			PanelPhone.Visible = true;
		}		
		this.UpdatePanelStaticBillingAddress.Update();
	}

	public override void BindView(object id)
	{

	}

    public override void SaveViewToModel()
    {

    }

	public override void Initialize()
	{

	}

	public override void Disable()
	{

	}

	public override void Enable()
	{

	}

	public override void Show()
	{
		this.Visible = true;
		UpdatePanelStaticBillingAddress.Visible = true;
		UpdatePanelStaticBillingAddress.Update();
	}

	public override void Hide()
	{
		this.Visible = false;
		UpdatePanelStaticBillingAddress.Visible = false;
		UpdatePanelStaticBillingAddress.Update();
	}

	public override void ShowError(string message)
	{
	}

	public event AddressEditEventHandler AddressEdit;
	private void FireAddressEdit()
	{
		if(AddressEdit != null)
		{
			AddressEdit(this, new EventArgs());
		}
	}
	#endregion
}
