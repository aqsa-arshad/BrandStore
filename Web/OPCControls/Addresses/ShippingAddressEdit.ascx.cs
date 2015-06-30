// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Vortx.OnePageCheckout;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.Settings;
using Vortx.OnePageCheckout.UI;
using Vortx.OnePageCheckout.Views;
using Vortx.OnePageCheckout.WebUtility;

public partial class VortxControls_ShippingAddressEdit :
	OPCUserControl<IAddressModel>,
	IAddressView
{
	#region Public Properties

	public IAddressUtility AddressUtility { get; set; }

	#endregion

	#region Page Events

	protected void ShipOtherCountry_OnDataBound(object sender, EventArgs e)
	{
		SetSelectedOtherCountry();
	}

	protected void ShipOtherCountry_OnChanged(object sender, EventArgs e)
	{
		this.Model.Country = ShipOtherCountry.SelectedValue;
		this.PopulateOtherStates();
	}

	protected void ShipOtherState_OnDataBound(object sender, EventArgs e)
	{
		SetSelectedOtherState();
	}

	protected void ShipZip_OnTextChanged(object sender, EventArgs e)
	{
		string eventTarget = Request.Params["__EVENTTARGET"];
		// when we programmatically set the password value it causes this event to fire
		// even when a different control triggers the postback.
		// SO, make sure the text box triggers the postback
		if (eventTarget.Contains("ShipZip"))
		{
			PopulateZipCityState();
			this.UpdatePanelShippingAddressWrap.Update();

			if (ShipZipCityState.Visible)
				ShipZipCityState.Focus();

			if (ShipOtherCity.Visible)
				ShipOtherCity.Focus();
		}
	}

	protected void ShipZipCityState_OnChanged(object sender, EventArgs e)
	{
		if (ShipZipCityState.SelectedValue.Equals("Other", StringComparison.InvariantCultureIgnoreCase))
		{
			PopulateOtherCountries();
			SetSelectedOtherCountry();

			PopulateOtherStates(ShipOtherCountry.SelectedValue);
			SetSelectedOtherState();

			ToggleOtherCityState(true, false);
		}
	}

	protected void SaveAddress_Click(object sender, EventArgs e)
	{
		this.SaveViewToModel();
	}

	protected void Address1POBoxNotAllowed_ServerValidate(object source, ServerValidateEventArgs args)
	{
		//first use standard logic to check, then use better regex parsing.
		if (args.Value.StartsWith("pobox", StringComparison.InvariantCultureIgnoreCase) ||
									args.Value.StartsWith("box ", StringComparison.InvariantCultureIgnoreCase) ||
									args.Value.IndexOf("postoffice") != -1)
		{
			args.IsValid = false;
			return;
		}

		args.IsValid = new AspDotNetStorefrontCore.Validation.POBoxAddressValidator().IsValid(args.Value);
	}

	protected void LnkBtnNonUsAddress_Click(object sender, EventArgs e)
	{
		PopulateOtherCountries();
		PopulateOtherStates();
		ToggleOtherCityState(true, false);
		LnkBtnNonUsAddress.Visible = false;

		ShipZip.Attributes.Remove("onkeyup");
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
		this.Address1POBoxNotAllowed.Enabled = Address2POBoxNotAllowed.Enabled = Vortx.OnePageCheckout.Settings.ConfigurationProvider.DefaultProvider.DisallowShippingToPOBoxes;

		if (!PageUtility.IsAsyncPostBackForControl(this, ConfigurationProvider.DefaultProvider.ScriptManagerId))
		{
			if (!string.IsNullOrEmpty(this.Model.Country))
			{

				PopulateOtherCountries();
				PopulateOtherStates();

				ShipFirstName.Text = Model.FirstName;
				ShipLastName.Text = Model.LastName;
				Company.Text = Model.Company;
				ShipZip.Text = Model.PostalCode;
				ShipOtherCity.Text = Model.City;
				ShipAddress1.Text = Model.Address1;
				ShipAddress2.Text = Model.Address2;
				ShipPhone.Text = Model.Phone ?? string.Empty;
				CheckBoxCommercial.Checked = !Model.Residential;
				SetSelectedOtherCountry();
				SetSelectedOtherState();

				ToggleOtherCityState(true, false);

				this.UpdatePanelShippingAddressWrap.Update();
			}

			// Notes do not require an address record, so display them even if we don't have address data.
			if (this.Model.ShowAddressNotes && string.IsNullOrEmpty(ShipComments.Text))
				ShipComments.Text = Model.Notes;

			this.PanelShipComments.Visible = this.Model.ShowAddressNotes;
		}
	}

	public override void BindView(object id)
	{
	}

	public override void SaveViewToModel()
	{
		Page.Validate("VGShippingAddress");
		if (!IsDynamicZipEntry())
		{
			ZipCodeValidator.CountryID = AspDotNetStorefrontCore.AppLogic.GetCountryID(this.ShipOtherCountry.SelectedValue);

			Page.Validate("VGShippingOtherAddress");
		}

		if (Page.IsValid)
		{
			String city = GetSelectedCity();
			String state = GetSelectedState();
			String country = GetSelectedCountry();

			if (city.Length > 0 && state.Length > 0 && country.Length > 0)
			{
				SaveAddress(ShipAddress1.Text, ShipAddress2.Text, city, state, ShipZip.Text, country, !CheckBoxCommercial.Checked);
			}
			else
			{
				PopulateZipCityState();

				this.UpdatePanelShippingAddressWrap.Update();

				if (ShipZipCityState.Visible)
					ShipZipCityState.Focus();

				if (ShipOtherCity.Visible)
					ShipOtherCity.Focus();
			}
		}
	}

	public override void Initialize()
	{
		this.Address1POBoxNotAllowed.Enabled = this.Address2POBoxNotAllowed.Enabled = Vortx.OnePageCheckout.Settings.ConfigurationProvider.DefaultProvider.DisallowShippingToPOBoxes;

		if (!PageUtility.IsAsyncPostBackForControl(this, ConfigurationProvider.DefaultProvider.ScriptManagerId))
		{
			if (this.ShipOtherCountry.SelectedValue == "")
			{
				PopulateOtherCountries();
				PopulateOtherStates(this.ShipOtherCountry.SelectedValue);
			}
			ToggleOtherCityState(false, false);
		}
	}

	public override void Disable()
	{
		this.ShipFirstName.Enabled = false;
		this.ShipLastName.Enabled = false;
		this.ShipPhone.Enabled = false;
		this.Company.Enabled = false;
		this.ShipZip.Enabled = false;
		this.ShipOtherCity.Enabled = false;
		this.Company.Enabled = false;
		this.ShipAddress1.Enabled = false;
		this.ShipAddress2.Enabled = false;
		this.ShipComments.Enabled = false;
		this.ButtonSaveAddress.Enabled = false;
		this.ButtonSaveAddress.Visible = false;
		this.CheckBoxCommercial.Enabled = false;
		this.ShipOtherState.Enabled = false;
		this.ShipOtherCountry.Enabled = false;
		this.ShipZipCityState.Enabled = false;

		PageUtility.AddClass(ShipAddressTable, "disabled");

		this.UpdatePanelShippingAddressWrap.Update();
	}

	public override void Enable()
	{
		ShipFirstName.Enabled = true;
		ShipLastName.Enabled = true;
		this.ShipPhone.Enabled = true;
		this.Company.Enabled = true;
		ShipZip.Enabled = true;
		ShipOtherCity.Enabled = true;
		ShipAddress1.Enabled = true;
		ShipAddress2.Enabled = true;
		ShipComments.Enabled = true;
		this.ShipOtherState.Enabled = true;
		this.ShipOtherCountry.Enabled = true;
		this.ShipZipCityState.Enabled = true;

		ButtonSaveAddress.Enabled = true;
		this.ButtonSaveAddress.Visible = true;
		this.CheckBoxCommercial.Enabled = true;
		PageUtility.RemoveClass(ShipAddressTable, "disabled");

		this.UpdatePanelShippingAddressWrap.Update();
	}

	public override void Show()
	{
		this.Visible = true;
		this.UpdatePanelShippingAddressWrap.Visible = true;
		this.UpdatePanelShippingAddressWrap.Update();
	}

	public override void Hide()
	{
		this.Visible = false;
		this.UpdatePanelShippingAddressWrap.Visible = true;
		this.UpdatePanelShippingAddressWrap.Update();
	}

	public override void ShowError(string message)
	{
		PanelError.Visible = true;
		LabelError.Text = message;
	}

	#endregion

	#region Private Methods

	void PopulateZipCityState()
	{
		this.ShipZipCityState.Items.Clear();
		this.ShipZipCityState.DataSource = Vortx.OnePageCheckout.WebService.ZipServiceUtility.GetInfoByZip(ShipZip.Text);
		this.ShipZipCityState.DataTextField = "DisplayValue";
		this.ShipZipCityState.DataValueField = "DisplayValue";
		this.ShipZipCityState.DataBind();

		if (ShipZipCityState.Items.Count <= 0)
		{
			PopulateOtherCountries();
			PopulateOtherStates();
			ToggleOtherCityState(true, false);
		}
		else
		{
			// add other option
			this.ShipZipCityState.Items.Add(new ListItem(StringResourceProvider.GetString("smartcheckout.aspx.122"), "Other"));

			// toggle dynamic city/state display
			ToggleOtherCityState(false, true);

			// hide the 'Enter zip for City and State.' message
			this.EnterZip.Visible = false;
		}
	}

	void PopulateOtherStates()
	{
		PopulateOtherStates(this.Model.CountryCode);
	}

	void PopulateOtherStates(string Country)
	{
		this.ShipOtherState.Items.Clear();
		this.ShipOtherState.DataSource = AddressUtility.GetStates(Country);
		this.ShipOtherState.DataTextField = "Key";
		this.ShipOtherState.DataValueField = "Value";
		this.ShipOtherState.DataBind();
	}

	void SetSelectedOtherState()
	{
		PageUtility.SetSelectedValue(ShipOtherState, Model.StateCode);
	}

	void PopulateOtherCountries()
	{
		this.ShipOtherCountry.Items.Clear();
		this.ShipOtherCountry.DataSource = AddressUtility.GetCountries();
		this.ShipOtherCountry.DataTextField = "Key";
		this.ShipOtherCountry.DataValueField = "Value";
		this.ShipOtherCountry.DataBind();
	}

	void SetSelectedOtherCountry()
	{
		PageUtility.SetSelectedValue(ShipOtherCountry, Model.Country);
	}

	void ToggleOtherCityState(bool showOtherStatePanel, bool showZipCityStateDropDown)
	{
		this.ShipZip.Attributes.Remove("onkeyup");
		this.ShipZip.Style.Remove("display");
		if (showOtherStatePanel)
		{
			this.LabelShipZip.Visible = true;
			this.ShipZip.Visible = true;
			this.EnterZip.Visible = false;
			this.PanelOtherCityState.Visible = true;
			this.PanelDynamicCityAndState.Visible = false;
		}
		else
		{
			this.LabelShipZip.Visible = true;
			this.ShipZip.Visible = true;
			this.EnterZip.Visible = true;
			this.PanelDynamicCityAndState.Visible = true;
			this.PanelOtherCityState.Visible = false;
		}
		string display = "none";
		if (showZipCityStateDropDown)
		{
			display = "block";
		}
		this.ShipZipCityState.Style.Add("display", display);
		this.ShipZip.Attributes.Add("onkeyup", "javascript:onZipKeyUp(event);");
	}

	void SaveAddress(string address1, string address2, string city,
		string state, string postalCode, string country, bool residential)
	{
		this.Model.FirstName = this.ShipFirstName.Text;
		this.Model.LastName = this.ShipLastName.Text;
		this.Model.Address1 = this.ShipAddress1.Text;
		this.Model.Address2 = this.ShipAddress2.Text;
		this.Model.Company = this.Company.Text;
		this.Model.City = city;
		this.Model.State = state;
		this.Model.PostalCode = this.ShipZip.Text;
		if (this.Model.ShowAddressNotes)
		{
			this.Model.Notes = this.ShipComments.Text;
		}
		this.Model.Phone = this.ShipPhone.Text;

		if (!string.IsNullOrEmpty(country))
		{
			this.Model.Country = country;
		}
		this.Model.Residential = !CheckBoxCommercial.Checked;

		this.Model.Email = AspDotNetStorefrontCore.Customer.Current.EMail;

		this.Model.Save();
	}

	string GetSelectedCity()
	{
		string city = this.ShipOtherCity.Text;

		if (IsDynamicZipEntry())
		{
			String[] splitCity = this.ShipZipCityState.SelectedValue.Split(',');
			if (splitCity.Length >= 2)
			{
				city = splitCity[0].Trim();
			}
		}

		return city;
	}

	string GetSelectedState()
	{
		string state = this.ShipOtherState.Text;

		if (IsDynamicZipEntry())
		{
			String[] splitCity = this.ShipZipCityState.SelectedValue.Split(',');
			if (splitCity.Length >= 2)
			{
				state = splitCity[1].Trim();
			}
		}

		return state;
	}

	string GetSelectedCountry()
	{
		string country = this.ShipOtherCountry.SelectedValue;

		if (IsDynamicZipEntry())
		{
			country = "United States";
		}

		return country;
	}

	bool IsDynamicZipEntry()
	{
		bool isDynamic = this.ShipZipCityState.Visible;
		isDynamic = isDynamic && !String.IsNullOrEmpty(this.ShipZipCityState.SelectedValue);
		isDynamic = isDynamic && !this.ShipZipCityState.SelectedValue.Equals("Other", StringComparison.InvariantCulture);
		return isDynamic;
	}

	#endregion
}
