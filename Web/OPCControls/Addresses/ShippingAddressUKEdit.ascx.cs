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
using Vortx.OnePageCheckout;
using Vortx.OnePageCheckout.UI;
using Vortx.OnePageCheckout.Views;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.WebUtility;
using Vortx.OnePageCheckout.Settings;
using Vortx.OnePageCheckout.WebService;
using System.Collections;

public partial class VortxControls_ShippingAddressUKEdit :
    OPCUserControl<IAddressModel>,
    IAddressView
{
	#region Page Events
	
	protected void FindAddress_Click(object sender, EventArgs e)
	{
		// Postcode formatting
		ShipZip.Text = ShipZip.Text.Replace(" ", "");

		if (ShipZip.Text.Length == 5)
		{
			ShipZip.Text = ShipZip.Text.Insert(2, " ");
		}
		else if (ShipZip.Text.Length == 6)
		{
			ShipZip.Text = ShipZip.Text.Insert(3, " ");
		}
		else if (ShipZip.Text.Length == 7)
		{
			ShipZip.Text = ShipZip.Text.Insert(4, " ");
		}

		ShipZip.Text = ShipZip.Text.ToUpper();
		this.UpdatePanelShippingAddressWrap.Update();
	}

	protected void SaveAddress_Click(object sender, EventArgs e)
	{
		this.SaveViewToModel();
	}

	#endregion

	#region IView Members

	public override void BindView()
	{
		if (!PageUtility.IsAsyncPostBackForControl(this, ConfigurationProvider.DefaultProvider.ScriptManagerId))
		{
			if (!string.IsNullOrEmpty(this.Model.Country))
			{
				ShipFirstName.Text = Model.FirstName;
				ShipLastName.Text = Model.LastName;
				ShipZip.Text = Model.PostalCode;
				ShipAddress1.Text = Model.Address1;
				ShipAddress2.Text = Model.Address2;
				ShipComments.Text = Model.Notes;
				ShipPhone.Text = Model.Phone ?? string.Empty;

                if (!IsPostBack && string.IsNullOrEmpty(ShipComments.Text))
                    ShipComments.Text = Model.Notes;
                
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
		Page.Validate("VGShippingAddress");

		if (Page.IsValid)
		{
			this.Model.FirstName = this.ShipFirstName.Text;
			this.Model.LastName = this.ShipLastName.Text;
			this.Model.Address1 = this.ShipAddress1.Text;
			this.Model.Address2 = this.ShipAddress2.Text;
			this.Model.City = this.ShipCity.Text;
			this.Model.State = this.ShipCounty.Text;
			this.Model.PostalCode = this.ShipZip.Text;
			this.Model.Notes = this.ShipComments.Text;
			this.Model.Phone = this.ShipPhone.Text;
            this.Model.Country = "United Kingdom";
            this.Model.Email = AspDotNetStorefrontCore.Customer.Current.EMail;

			this.Model.Save();
		}
	}

	public override void Initialize()
	{
	}

	public override void Disable()
	{
		this.ShipFirstName.Enabled = false;
		this.ShipLastName.Enabled = false;
		this.ShipZip.Enabled = false;
		this.ShipCity.Enabled = false;
		this.ShipCounty.Enabled = false;
		this.ShipAddress1.Enabled = false;
		this.ShipAddress2.Enabled = false;
		this.ShipComments.Enabled = false;
		this.ShipPhone.Enabled = false;
		this.ButtonSaveAddress.Enabled = false;
		this.ButtonSaveAddress.Visible = false;
		PageUtility.AddClass(ShipAddressTable, "disabled");

		this.UpdatePanelShippingAddressWrap.Update();
	}

	public override void Enable()
	{
		ShipFirstName.Enabled = true;
		ShipLastName.Enabled = true;
		ShipZip.Enabled = true;
		ShipCity.Enabled = true;
		ShipCounty.Enabled = true;
		ShipAddress1.Enabled = true;
		ShipAddress2.Enabled = true;
		ShipComments.Enabled = true;
		ShipPhone.Enabled = true;
		ButtonSaveAddress.Enabled = true;
		ButtonSaveAddress.Visible = true;

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
