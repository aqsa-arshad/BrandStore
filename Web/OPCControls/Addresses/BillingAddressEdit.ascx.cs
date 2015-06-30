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

public partial class VortxControls_BillingAddressEdit :
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

	protected void BillZip_OnTextChanged(object sender, EventArgs e)
	{
		PopulateZipCityState();
		this.UpdatePanelBillingAddressWrap.Update();
	}

	protected void BillZipCityState_OnChanged(object sender, EventArgs e)
	{
		if (BillZipCityState.SelectedValue.Equals("Other", StringComparison.InvariantCultureIgnoreCase))
		{
			PopulateOtherCountries();
			PopulateOtherStates();

			SetSelectedOtherCountry();
			SetSelectedOtherState();

			ToggleOtherCityState(true, false);
		}
	}

	#endregion

	#region Private Methods

	void PopulateZipCityState()
	{
		this.BillZipCityState.Items.Clear();
		this.BillZipCityState.DataSource = Vortx.OnePageCheckout.WebService.ZipServiceUtility.GetInfoByZip(BillZip.Text);
		this.BillZipCityState.DataTextField = "DisplayValue";
		this.BillZipCityState.DataValueField = "DisplayValue";
		this.BillZipCityState.DataBind();
		if (BillZipCityState.Items.Count <= 0)
		{
			PopulateOtherCountries();
			PopulateOtherStates();
			ToggleOtherCityState(true, false);
		}
		else
		{
            this.BillZipCityState.Items.Add(new ListItem(StringResourceProvider.GetString("smartcheckout.aspx.122"), "Other"));
			ToggleOtherCityState(false, true);
			this.EnterZip.Visible = false;
		}
	}

	void PopulateOtherStates()
	{
		this.Model.Country = BillOtherCountry.SelectedValue;

		this.BillOtherState.Items.Clear();
		this.BillOtherState.DataSource = AddressUtility.GetStates(this.Model.Country);
		this.BillOtherState.DataTextField = "Key";
		this.BillOtherState.DataValueField = "Value";
		this.BillOtherState.DataBind();
	}

	void SetSelectedOtherState()
	{
		PageUtility.SetSelectedValue(BillOtherState, Model.State);
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
			LabelCityAndState.Visible = false;
			this.LabelBillZip.Visible = true;
			this.BillZip.Visible = true;
			this.EnterZip.Visible = false;
			this.PanelOtherCityState.Visible = true;
		}
		else
		{
			LabelCityAndState.Visible = true;
			this.LabelBillZip.Visible = true;
			this.BillZip.Visible = true;
			this.EnterZip.Visible = true;
			this.PanelOtherCityState.Visible = false;
		}
		string display = "none";
		if (showZipCityStateDropDown)
		{
			display = "block";
		}
		this.BillZipCityState.Style.Add("display", display); //this.BillZipCityState.Visible = false;
		this.BillZip.Attributes.Add("onkeyup", "javascript:onZipKeyUp(event);");
	}

	string GetCityFromDynamicDropDown()
	{
		String[] splitCity = this.BillZipCityState.SelectedValue.Split(',');
		if (splitCity.Length >= 2)
		{
			return splitCity[0].Trim();
		}

		return string.Empty;
	}

	string GetStateFromDynamicDropDown()
	{
		String[] splitCity = this.BillZipCityState.SelectedValue.Split(',');
		if (splitCity.Length >= 2)
		{
			return splitCity[1].Trim();
		}

		return string.Empty;
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
				BillFirstName.Text = Model.FirstName;
				BillLastName.Text = Model.LastName;
				BillZip.Text = Model.PostalCode;
				BillOtherCity.Text = Model.City;
				BillAddress1.Text = Model.Address1;
				BillAddress2.Text = Model.Address2;
				BillCompany.Text = Model.Company;
				BillPhone.Text = Model.Phone ?? string.Empty;

				SetSelectedOtherCountry();
				SetSelectedOtherState();

				ToggleOtherCityState(true, false);

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

		// Validate the 'Other' Ship city/state/zip if selected
		if (String.IsNullOrEmpty(this.BillZipCityState.SelectedValue))
		{
			Page.Validate("VGBillingOtherAddress");
		}
		if (Page.IsValid)
		{
			String city = String.Empty;
			String state = String.Empty;
			String country = String.Empty;

			if (!String.IsNullOrEmpty(this.BillZipCityState.SelectedValue))
			{
				city = GetCityFromDynamicDropDown();
				state = GetStateFromDynamicDropDown();
				country = "United States";
			}
			else
			{
				city = this.BillOtherCity.Text;
				state = this.BillOtherState.SelectedValue;
				country = this.BillOtherCountry.SelectedValue;
			}

			this.Model.FirstName = this.BillFirstName.Text;
			this.Model.LastName = this.BillLastName.Text;
			this.Model.Company = this.BillCompany.Text;
			this.Model.Address1 = this.BillAddress1.Text;
			this.Model.Address2 = this.BillAddress2.Text;
			this.Model.City = city;
			this.Model.State = state;
			this.Model.PostalCode = this.BillZip.Text;

			if (!string.IsNullOrEmpty(country))
			{
				this.Model.Country = country;
			}
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

			PopulateOtherCountries();
			PopulateOtherStates();

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
		BillOtherState.Enabled = false;
		BillOtherCountry.Enabled = false;
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
		BillOtherState.Enabled = true;
		BillOtherCountry.Enabled = true;
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
