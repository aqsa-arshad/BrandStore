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
using Vortx.OnePageCheckout.Settings;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.Views;
using AspDotNetStorefrontCore;

public partial class MiniCartControl :
    OPCUserControl<IShoppingCartModel>,
    IShoppingCartView
{
    private void PopulateCartItemRepeater()
    {
        RepeaterCartItems.DataSource = this.Model.ShoppingCartItems;
        RepeaterCartItems.DataBind();

        if (this.Model.OrderOptions.Count() > 0)
        {
            RepeaterOrderOptions.DataSource = this.Model.OrderOptions;
            RepeaterOrderOptions.DataBind();
        }
        else
        {
            RepeaterOrderOptions.Visible = false;
        }
    }

    #region IView Members

    public override void BindView()
    {
        PopulateCartItemRepeater();
    }

    public override void BindView(object id)
    {
        PopulateCartItemRepeater();
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
        this.UpdatePanelMiniCartControl.Visible = true;
        this.UpdatePanelMiniCartControl.Update();
    }

    public override void Hide()
    {
        this.Visible = false;
        this.UpdatePanelMiniCartControl.Visible = false;
        this.UpdatePanelMiniCartControl.Update();
    }

    public override void ShowError(string message)
    {
    }

    #endregion

    protected void RepeaterCartItems_OnItemDataBound(object source, RepeaterItemEventArgs e)
    {
        if ((e.Item.ItemType == ListItemType.AlternatingItem) ||
            (e.Item.ItemType == ListItemType.Item))
        {
            Image productImage = (Image)e.Item.FindControl("ProductImage");
            IShoppingCartItem cartItem = (IShoppingCartItem)e.Item.DataItem;
            if (productImage != null)
            {
                productImage.ImageUrl = cartItem.ImageUrl;
            }

            var currentCustomer = AspDotNetStorefrontCore.Customer.Current;

            Label CartItemPriceValue = e.Item.FindControl("CartItemPriceValue") as Label;
            CartItemPriceValue.Text = Localization.CurrencyStringForDisplayWithExchangeRate(cartItem.Price, currentCustomer.CurrencySetting);

            if (!String.IsNullOrEmpty(cartItem.Size))
            {
                if (cartItem.SizePrompt.Length > 0)
                {
                    Label LabelSizePrompt = (Label)e.Item.FindControl("ProductSizeLabel");
                    if (LabelSizePrompt != null)
                        LabelSizePrompt.Text = cartItem.SizePrompt + ":";
                }
                Panel panelSize = (Panel)e.Item.FindControl("PanelSize");
                panelSize.Visible = true;
            }

            if (!String.IsNullOrEmpty(cartItem.Color))
            {
                if (cartItem.ColorPrompt.Length > 0)
                {
                    Label LabelColorPrompt = (Label)e.Item.FindControl("ProductColorLabel");
                    if (LabelColorPrompt != null)
                        LabelColorPrompt.Text = cartItem.ColorPrompt + ":";
                }
                Panel panelColor = (Panel)e.Item.FindControl("PanelColor");
                panelColor.Visible = true;
            }

            if (cartItem.RestrictedQuantities.Count > 0)
            {
                DropDownList CartItemRestrictedQuantity = (DropDownList)e.Item.FindControl("CartItemRestrictedQuantity");
                CartItemRestrictedQuantity.DataSource = cartItem.RestrictedQuantities;
                CartItemRestrictedQuantity.DataBind();
                CartItemRestrictedQuantity.Visible = true;

                TextBox CartItemQuantityValue = (TextBox)e.Item.FindControl("CartItemQuantityValue");
                CartItemQuantityValue.Visible = false;

                CartItemRestrictedQuantity.SelectedValue = cartItem.Quantity.ToString();
            }

            // Show/hide VAT Inclusive information
            var label = (Label)e.Item.FindControl("CartItemPriceVat");
            var shoppingCart = ((AspDotNetStorefront.SkinBase)Page).OPCShoppingCart;
            label.Visible = shoppingCart.VatEnabled && shoppingCart.VatIsInclusive;
        }
    }

    protected void RepeaterCartItems_OnItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if ((e.Item.ItemType == ListItemType.AlternatingItem) ||
            (e.Item.ItemType == ListItemType.Item))
        {
            if (e.CommandName == "Remove")
            {
                this.Model.RemoveItem(Convert.ToString(e.CommandArgument));
            }
            else if (e.CommandName == "Update")
            {
                TextBox quantityField = (TextBox)e.Item.FindControl("CartItemQuantityValue");
                if (!quantityField.Visible)
                {
                    DropDownList CartItemRestrictedQuatity = (DropDownList)e.Item.FindControl("CartItemRestrictedQuantity");
                    quantityField.Text = CartItemRestrictedQuatity.SelectedValue;
                }

                Int32 quantity = Convert.ToInt32(quantityField.Text);
                this.Model.UpdateItemQuantity(Convert.ToString(e.CommandArgument), quantity);
            }

            this.UpdatePanelMiniCartControl.Update();
        }
    }

    protected void RepeaterOrderOptions_OnItemCommand(object source, RepeaterCommandEventArgs e)
    {

    }
}
