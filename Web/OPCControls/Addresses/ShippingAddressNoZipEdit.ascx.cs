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

public partial class VortxControls_ShippingAddressNoZipEdit :
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
		this.UpdatePanelShippingAddressWrap.Update();
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

	#endregion

	#region Private Methods

	private void PopulateOtherStates()
	{
		PopulateOtherStates(this.Model.CountryCode);
	}

	private void PopulateOtherStates(string Country)
	{
		this.ShipOtherState.Items.Clear();
		this.ShipOtherState.DataSource = AddressUtility.GetStates(Country);
		this.ShipOtherState.DataTextField = "Key";
		this.ShipOtherState.DataValueField = "Value";
		this.ShipOtherState.DataBind();
	}


	private void SetSelectedOtherState()
	{
		PageUtility.SetSelectedValue(ShipOtherState, Model.StateCode);
	}

	private void PopulateOtherCountries()
	{
		this.ShipOtherCountry.Items.Clear();
		this.ShipOtherCountry.DataSource = AddressUtility.GetCountries();
		this.ShipOtherCountry.DataTextField = "Key";
		this.ShipOtherCountry.DataValueField = "Value";
		this.ShipOtherCountry.DataBind();
	}

	private void SetSelectedOtherCountry()
	{
		PageUtility.SetSelectedValue(ShipOtherCountry, Model.Country);
	}

	private void ToggleOtherCityState(bool showOtherStatePanel, bool showZipCityStateDropDown)
	{
		this.ShipZip.Attributes.Remove("onkeyup");
		this.ShipZip.Style.Remove("display");
		if (showOtherStatePanel)
		{
			this.LabelShipZip.Visible = true;
			this.ShipZip.Visible = true;
		}
		else
		{
			this.LabelShipZip.Visible = true;
			this.ShipZip.Visible = true;
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
		this.Address1POBoxNotAllowed.Enabled = this.Address2POBoxNotAllowed.Enabled = Vortx.OnePageCheckout.Settings.ConfigurationProvider.DefaultProvider.DisallowShippingToPOBoxes;

		if (!PageUtility.IsAsyncPostBackForControl(this, ConfigurationProvider.DefaultProvider.ScriptManagerId))
		{
			if (!string.IsNullOrEmpty(this.Model.Country))
			{
				PopulateOtherCountries();
				SetSelectedOtherCountry();
				PopulateOtherStates(this.Model.CountryCode);
				SetSelectedOtherState();

				ShipFirstName.Text = Model.FirstName;
				ShipLastName.Text = Model.LastName;
				Company.Text = Model.Company;
				ShipZip.Text = Model.PostalCode;
				ShipOtherCity.Text = Model.City;
				ShipAddress1.Text = Model.Address1;
				ShipAddress2.Text = Model.Address2;
				ShipComments.Text = Model.Notes;
				ShipPhone.Text = Model.Phone ?? string.Empty;
				CheckBoxCommercial.Checked = !Model.Residential;

				if (!IsPostBack && string.IsNullOrEmpty(ShipComments.Text))
					ShipComments.Text = Model.Notes;

				ToggleOtherCityState(true, false);

				this.UpdatePanelShippingAddressWrap.Update();
			}

			this.PanelShipComments.Visible = this.Model.ShowAddressNotes;
		}
	}

	public override void BindView(object id)
	{
	}

	public override void SaveViewToModel()
	{
		if (!string.IsNullOrEmpty(this.ShipOtherCountry.SelectedValue))
			ZipCodeValidator.CountryID = AspDotNetStorefrontCore.AppLogic.GetCountryID(this.ShipOtherCountry.SelectedValue);

		Page.Validate("VGShippingAddress");

		Page.Validate("VGShippingOtherAddress");

		if (Page.IsValid)
		{
			String city = String.Empty;
			String state = String.Empty;
			String country = String.Empty;

			city = this.ShipOtherCity.Text;
			state = this.ShipOtherState.SelectedValue;
			country = this.ShipOtherCountry.SelectedValue;

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
			this.Hide();
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
		this.ShipOtherCountry.Enabled = false;
		this.ShipOtherState.Enabled = false;
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
		ButtonSaveAddress.Enabled = true;
		this.ButtonSaveAddress.Visible = true;
		this.CheckBoxCommercial.Enabled = true;
		this.ShipOtherCountry.Enabled = true;
		this.ShipOtherState.Enabled = true;
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
}
