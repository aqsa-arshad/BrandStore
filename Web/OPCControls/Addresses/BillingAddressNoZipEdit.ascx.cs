// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using Vortx.OnePageCheckout;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.Settings;
using Vortx.OnePageCheckout.UI;
using Vortx.OnePageCheckout.Views;
using Vortx.OnePageCheckout.WebUtility;

public partial class VortxControls_BillingAddressNoZipEdit :
    OPCUserControl<IAddressModel>,
    IAddressView
{
	#region Public Properties

	public IAddressUtility AddressUtility { get; set; }

	#endregion
	
	#region Page Events

	protected void BillOtherCountry_OnDataBound(object sender, EventArgs e)
	{
		SetSelectedOtherCountry();
	}

	protected void BillOtherCountry_OnChanged(object sender, EventArgs e)
	{
		this.Model.Country = BillOtherCountry.SelectedValue;
		this.PopulateOtherStates();
	}

	protected void BillOtherState_OnDataBound(object sender, EventArgs e)
	{
		SetSelectedOtherState();
	}

	#endregion

	#region Private Methods

	void PopulateOtherStates()
	{
		PopulateOtherStates(this.Model.CountryCode);
	}

	void PopulateOtherStates(string Country)
	{
		this.BillOtherState.Items.Clear();
		this.BillOtherState.DataSource = AddressUtility.GetStates(Country);
		this.BillOtherState.DataTextField = "Key";
		this.BillOtherState.DataValueField = "Value";
		this.BillOtherState.DataBind();
	}

	void SetSelectedOtherState()
	{
		PageUtility.SetSelectedValue(BillOtherState, this.Model.StateCode);
	}

	void PopulateOtherCountries()
	{
		this.BillOtherCountry.Items.Clear();
		this.BillOtherCountry.DataSource = AddressUtility.GetCountries();
		this.BillOtherCountry.DataTextField = "Key";
		this.BillOtherCountry.DataValueField = "Value";
		this.BillOtherCountry.DataBind();
	}

	void SetSelectedOtherCountry()
	{
		PageUtility.SetSelectedValue(BillOtherCountry, Model.Country);
	}

	void ToggleOtherCityState(bool showOtherStatePanel, bool showZipCityStateDropDown)
	{
		this.BillZip.Attributes.Remove("onkeyup");
		this.BillZip.Style.Remove("display");
		if (showOtherStatePanel)
		{
			this.LabelBillZip.Visible = true;
			this.BillZip.Visible = true;
		}
		else
		{
			this.LabelBillZip.Visible = true;
			this.BillZip.Visible = true;
		}
	}

	#endregion

	#region IView Members

	public override void SetModel(IAddressModel model, IStringResourceProvider stringResourceProvider)
	{
		base.SetModel(model, stringResourceProvider);

		this.AddressUtility = ObjectFactory.CreateModelFactory((AspDotNetStorefront.SkinBase)Page).CreateAddressUtility();
	}
	
	public override void BindView()
	{
		if (!PageUtility.IsAsyncPostBackForControl(this, ConfigurationProvider.DefaultProvider.ScriptManagerId))
		{
			if (!string.IsNullOrEmpty(this.Model.Country))
			{
				PopulateOtherCountries();
				SetSelectedOtherCountry();
				PopulateOtherStates(this.Model.CountryCode);
				SetSelectedOtherState();

				BillFirstName.Text = Model.FirstName;
				BillLastName.Text = Model.LastName;
				BillZip.Text = Model.PostalCode;
				BillOtherCity.Text = Model.City;
				BillAddress1.Text = Model.Address1;
				BillAddress2.Text = Model.Address2;
				BillCompany.Text = Model.Company;
				BillPhone.Text = Model.Phone ?? string.Empty;

				ToggleOtherCityState(true, false);

				this.UpdatePanelBillingAddressWrap.Update();
			}
		}
	}

	public override void BindView(object id)
	{
	}

	public override void SaveViewToModel()
	{
        if (!string.IsNullOrEmpty(this.BillOtherCountry.SelectedValue))
            ZipCodeValidator.CountryID = AspDotNetStorefrontCore.AppLogic.GetCountryID(this.BillOtherCountry.SelectedValue);

        Page.Validate("VGBillingAddress");
		Page.Validate("VGBillingOtherAddress");

		if (Page.IsValid)
		{
			this.Model.FirstName = this.BillFirstName.Text;
			this.Model.LastName = this.BillLastName.Text;
			this.Model.Company = this.BillCompany.Text;
			this.Model.Address1 = this.BillAddress1.Text;
			this.Model.Address2 = this.BillAddress2.Text;
			this.Model.City = this.BillOtherCity.Text;
			this.Model.State = this.BillOtherState.SelectedValue ?? string.Empty;
			this.Model.PostalCode = this.BillZip.Text;
			this.Model.Country = this.BillOtherCountry.SelectedValue ?? string.Empty;
			this.Model.Phone = this.BillPhone.Text;
            this.Model.Email = AspDotNetStorefrontCore.Customer.Current.EMail;
			this.Model.Save();
		}
	}

	public override void Initialize()
	{
		if (!PageUtility.IsAsyncPostBackForControl(this, ConfigurationProvider.DefaultProvider.ScriptManagerId))
		{
			BillFirstName.Text = string.Empty;
			BillLastName.Text = string.Empty;
			BillCompany.Text = string.Empty;
			BillZip.Text = string.Empty;
			BillOtherCity.Text = string.Empty;
			BillAddress1.Text = string.Empty;
			BillAddress2.Text = string.Empty;
			BillPhone.Text = string.Empty;

			if (this.BillOtherCountry.SelectedValue == "")
			{
				PopulateOtherCountries();
				PopulateOtherStates(this.BillOtherCountry.SelectedValue);
			}
            
			ToggleOtherCityState(false, false);
		}
	}

	public override void Disable()
	{
		BillFirstName.Enabled = false;
		BillLastName.Enabled = false;
		BillCompany.Enabled = false;
		BillZip.Enabled = false;
		BillOtherCity.Enabled = false;
		BillAddress1.Enabled = false;
		BillAddress2.Enabled = false;
		BillPhone.Enabled = false;

		PageUtility.AddClass(this.BillAddressTable, "disabled");

		this.UpdatePanelBillingAddressWrap.Update();
	}

	public override void Enable()
	{
		BillFirstName.Enabled = true;
		BillLastName.Enabled = true;
		BillCompany.Enabled = true;
		BillZip.Enabled = true;
		BillOtherCity.Enabled = true;
		BillAddress1.Enabled = true;
		BillAddress2.Enabled = true;
		BillPhone.Enabled = true;

		PageUtility.RemoveClass(this.BillAddressTable, "disabled");

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
