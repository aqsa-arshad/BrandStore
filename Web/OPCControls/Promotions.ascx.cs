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
using AspDotNetStorefront;
using AspDotNetStorefront.Promotions;
using AspDotNetStorefrontCore;

public partial class OPCControls_Promotions : System.Web.UI.UserControl
{
    public Customer ThisCustomer
    {
        get
        {
            return ((SkinBase)Page).OPCCustomer;
        }
    }

    public ShoppingCart ShoppingCart
    {
        get
        {
            return ((SkinBase)Page).OPCShoppingCart;
        }
    }

    public event EventHandler PromotionsChanged;

    public void Initialize()
    {
        lblPromotionError.Text = string.Empty;
        BindPromotions();
    }

    #region Private Methods

    private void FirePromotionsChanged()
    {
        if (PromotionsChanged != null)
        {
            PromotionsChanged(this, null);
        }
    }

    private void BindPromotions()
    {
        repeatPromotions.DataSource = this.ShoppingCart.DiscountResults.Select(dr => dr.Promotion);
        repeatPromotions.DataBind();
    }

    #endregion

    #region Event Handlers

    protected void repeatPromotions_ItemCommand(Object sender, RepeaterCommandEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            return;

        PromotionManager.ClearPromotionUsages(ThisCustomer.CustomerID, e.CommandArgument.ToString(), true);

        FirePromotionsChanged();
    }

    protected void btnAddPromotion_Click(Object sender, EventArgs e)
    {
        Page.Validate("AddPromotion");
        if (Page.IsValid)
        {
            String promotionCode = txtPromotionCode.Text.ToLower().Trim();
            txtPromotionCode.Text = String.Empty;
            lblPromotionError.Text = String.Empty;

            IEnumerable<IPromotionValidationResult> validationResults = PromotionManager.ValidatePromotion(promotionCode,
                PromotionManager.CreateRuleContext(this.ShoppingCart));
            if (validationResults.Count() > 0 && validationResults.Any(vr => !vr.IsValid))
            {
                foreach (var reason in validationResults.Where(vr => !vr.IsValid).SelectMany(vr => vr.Reasons))
                {
                    String message = reason.MessageKey.StringResource();
                    if (reason.ContextItems != null && reason.ContextItems.Any())
                        foreach (var item in reason.ContextItems)
                            message = message.Replace(String.Format("{{{0}}}", item.Key), item.Value.ToString());

					lblPromotionError.Text += String.Format("<div class='promotion-reason error-wrap'>{0}</div>", message);
                }
                return;
            }
            else
            {
                PromotionManager.AssignPromotion(ThisCustomer.CustomerID, promotionCode);
            }
        }
        FirePromotionsChanged();
    }

    #endregion
}
