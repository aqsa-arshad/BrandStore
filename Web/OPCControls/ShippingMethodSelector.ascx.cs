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
using Vortx.OnePageCheckout.UI;
using Vortx.OnePageCheckout.Settings;
using Vortx.OnePageCheckout.Views;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.WebUtility;
using AspDotNetStorefront.Promotions;
using AspDotNetStorefrontCore;

public partial class VortxControls_ShippingMethodSelector :
    OPCUserControl<IShoppingCartModel>,
    IShoppingCartView
{
	public bool CartContentsChanged { get; set; }

	#region Page Events

	protected void ShippingMethods_SelectedIndexChanged(object sender, EventArgs e)
	{
		this.Model.ShippingMethodId = ShippingMethods.SelectedValue.Trim();
        var customer = AspDotNetStorefrontCore.Customer.Current;
	}

	#endregion

	#region Private Methods

	private void PopulateShippingMethods()
	{
		IEnumerable<IShippingMethod> shippingMethods = this.Model.ShippingMethods;
        var customer = AspDotNetStorefrontCore.Customer.Current;

		if (!customer.HasAtLeastOneAddress() ||
			(string.IsNullOrEmpty(customer.PrimaryShippingAddress.City) &&
			 string.IsNullOrEmpty(customer.PrimaryShippingAddress.State) &&			 
			 string.IsNullOrEmpty(customer.PrimaryShippingAddress.Country)))
        {
            ShowError(StringResourceProvider.GetString("smartcheckout.aspx.153"));
            ShippingMethods.Items.Clear();
            this.Disable();
            return;
        }

        if(shippingMethods.Count() <= 0)
        {
            if (!string.IsNullOrEmpty(this.Model.LastShippingMethodError))
            {
				if(AppLogic.AppConfigBool("Vortx.OnePageCheckout.AllowRTShipping.NoMethodSelected"))
				{
					ShowError(StringResourceProvider.GetString("smartcheckout.aspx.165")); 
				}
				else
				{
					ShowError(this.Model.LastShippingMethodError);
				}
            }
            else
            {
                ShowError(StringResourceProvider.GetString("smartcheckout.aspx.131"));
            }
            ShippingMethods.Items.Clear();
            this.Disable();
            return;
        }

		if (CartContentsChanged && shippingMethods.Count() > 0)
		{
			ShowErrorWithCss(StringResourceProvider.GetString("smartcheckout.aspx.166"), "error-wrap");
		}
		else
		{
			PanelError.CssClass = string.Empty;
			PanelError.Visible = false;
		}

        ShippingMethods.DataSource = shippingMethods;
        ShippingMethods.DataTextField = "DisplayName";
        ShippingMethods.DataValueField = "Identifier";
        ShippingMethods.DataBind();
	}

    private void PopulateFreeShippingThreshold()
    {
        PanelFreeShippingThreshold.Visible = false;
        var cart = ((AspDotNetStorefront.SkinBase)this.Page).OPCShoppingCart;
        var customer = ((AspDotNetStorefront.SkinBase)this.Page).OPCCustomer;
        if (!cart.ShippingIsFree && cart.MoreNeededToReachFreeShipping != 0.0M)
        {
            PanelFreeShippingThreshold.Visible = true;
            LblFreeShippingThreshold.Text = String.Format(this.StringResourceProvider.GetString("checkoutshipping.aspx.2"),
                customer.CurrencyString(cart.FreeShippingThreshold), CommonLogic.Capitalize(cart.FreeShippingMethod));
        }
    }

	private void SetSelectedShippingMethod()
	{        
        PageUtility.SetSelectedValue(this.ShippingMethods, this.Model.ShippingMethodId);
	}

	#endregion	

	#region IView Members

	public override void Initialize()
	{
		ShippingMethods.ClearSelection();
		ShippingMethods.Items.Clear();

        SetMultiShipVisibility();
    }

    private void SetMultiShipVisibility()
    {
        GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
        PanelMultiShip.Visible = ConfigurationProvider.DefaultProvider.AllowMultipleShippingAddressPerOrder &&
            (this.Model.ShoppingCartItems.Count() > 1 || this.Model.ShoppingCartItems.FirstOrDefault().Quantity > 1) && !(checkoutByAmazon.IsEnabled && checkoutByAmazon.IsCheckingOut);
    }

	public override void Disable()
	{
		ShippingMethods.Enabled = false;
	}

	public override void Enable()
	{
		ShippingMethods.Enabled = true;
	}

	public override void Show()
	{
		this.Visible = true;
		this.UpdatePanelShipMethod.Visible = true;
		this.UpdatePanelShipMethod.Update();
	}

	public override void Hide()
	{
		this.Visible = false;
		this.UpdatePanelShipMethod.Visible = false;
		this.UpdatePanelShipMethod.Update();
	}

	public override void BindView()
	{
		PopulateShippingMethods();
		SetSelectedShippingMethod();
        SetMultiShipVisibility();
        PopulateFreeShippingThreshold();
	}

	public override void BindView(object id)
	{
		PopulateShippingMethods();
		SetSelectedShippingMethod();
        SetMultiShipVisibility();
        PopulateFreeShippingThreshold();
    }

	public override void SaveViewToModel()
	{

	}

	public override void ShowError(string message)
	{
		PanelError.Visible = true;
		LabelError.Visible = true;
		LabelError.Text = message;
	}

	public void ShowErrorWithCss(string message, string cssClass)
	{
		PanelError.Visible = true;
		LabelError.Visible = true;
		LabelError.Text = message;
		PanelError.CssClass = cssClass;
	}

	#endregion
}
