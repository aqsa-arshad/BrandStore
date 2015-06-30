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

public partial class OPCControls_AddressStatic :
    OPCUserControl<IAddressModel>,
    IAddressView
{
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

		if(String.IsNullOrEmpty(this.Address2.Text))
		{
			PanelAddressLine2.Visible = false;
		}
		if(String.IsNullOrEmpty(this.Apartment.Text))
		{
			PanelApartment.Visible = false;
		}
		if(String.IsNullOrEmpty(this.Phone.Text))
		{
			PanelPhone.Visible = false;
		}
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
	}

	public override void Hide()
	{
		this.Visible = false;	
	}

    public override void ShowError(string message)
    {
    }

	#endregion
}
