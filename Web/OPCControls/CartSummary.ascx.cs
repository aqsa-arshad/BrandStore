// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using AspDotNetStorefrontCore;
using Vortx.OnePageCheckout.UI;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.Views;

public partial class OPCControls_CartSummary :
    OPCUserControl<IShoppingCartModel>,
	IShoppingCartView
{
	public override void BindView()
	{
        var currentCustomer = AspDotNetStorefrontCore.Customer.Current;
        var shoppingCart = ((AspDotNetStorefront.SkinBase)Page).OPCShoppingCart;

		decimal shipTotal = Math.Round(Model.ShippingTotal, 2, MidpointRounding.AwayFromZero);
        decimal subTotal = Math.Round(Model.SubTotal, 2, MidpointRounding.AwayFromZero);
        decimal discount = Math.Round(Model.Discount1, 2, MidpointRounding.AwayFromZero);
        decimal total = Math.Round(Model.Total, 2, MidpointRounding.AwayFromZero);

		ShipMethodAmount.Text = string.Concat(Localization.CurrencyStringForDisplayWithExchangeRate(shipTotal, currentCustomer.CurrencySetting), this.Model.GetVatDetailsIfApplicable());
		TaxAmount.Text = Localization.CurrencyStringForDisplayWithExchangeRate(Model.TaxTotal, currentCustomer.CurrencySetting);
		SubTotal.Text = Localization.CurrencyStringForDisplayWithExchangeRate(subTotal, currentCustomer.CurrencySetting);
        
		QuantityDiscountRow.Visible = Model.Discount2 > 0m;
        LabelQuantityDiscountAmount.Text = Localization.CurrencyStringForDisplayWithExchangeRate(Model.Discount2 * -1, currentCustomer.CurrencySetting);

		LineItemDiscountRow.Visible = Model.LineItemPromotionDiscountTotal < 0m;
		OrderItemDiscountRow.Visible = Model.OrderPromotionDiscountTotal < 0m;

        LineItemDiscountLabel.Text = "shoppingcart.cs.200".StringResource();
        OrderItemDiscountLabel.Text = "shoppingcart.cs.201".StringResource();

		LineItemDiscount.Text = Localization.CurrencyStringForDisplayWithExchangeRate(Model.LineItemPromotionDiscountTotal, currentCustomer.CurrencySetting);
		OrderItemDiscount.Text = Localization.CurrencyStringForDisplayWithExchangeRate(Model.OrderPromotionDiscountTotal, currentCustomer.CurrencySetting);

        if (total < decimal.Zero)
            total = decimal.Zero;

        Total.Text = Localization.CurrencyStringForDisplayWithExchangeRate(total, currentCustomer.CurrencySetting);

        trTaxAmounts.Visible = !AppLogic.AppConfigBool("VAT.Enabled") || AspDotNetStorefrontCore.Customer.Current.VATSettingReconciled != VATSettingEnum.ShowPricesInclusiveOfVAT;

        GiftCardRow.Visible = !string.IsNullOrEmpty(this.Model.GiftCardCode);
        if (GiftCardRow.Visible)
        {
            GiftCardAmount.Text = Localization.CurrencyStringForDisplayWithExchangeRate(-shoppingCart.GiftCardTotal, currentCustomer.CurrencySetting);
        }
	}
	
	public override void BindView(object identifier)
	{
	}

	public override void SaveViewToModel()
	{
	}

	public override void Initialize()
	{
		ShipMethodAmount.Text = (0.0m).ToString("C");
		TaxAmount.Text = (0.0m).ToString("C");
		SubTotal.Text = (0.0m).ToString("C");
		Total.Text = (0.0m).ToString("C");
	}

	public override void Disable()
	{
	}

	public override void Enable()
	{
	}

	public override void Show()
	{
		Visible = true;
		UpdatePanelCartSummary.Visible = true;
		UpdatePanelCartSummary.Update();
	}

	public override void Hide()
	{
		Visible = false;
		UpdatePanelCartSummary.Visible = false;
		UpdatePanelCartSummary.Update();
	}

    public override void ShowError(string message)
    {
    }
}
