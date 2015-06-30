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
using Vortx.OnePageCheckout.Views;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.Models.PaymentMethods;

[PaymentType(Vortx.OnePageCheckout.Models.PaymentType.CheckoutByAmazon)]
public partial class OPCControls_PaymentForms_CheckOutByAmazonPaymentForm :
    OPCUserControl<PaymentMethodBaseModel>,
    IPaymentMethodView
{
	#region IView Members

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

	public override void BindView()
	{
	}

	public override void BindView(object identifier)
	{
	}

	public override void SaveViewToModel()
	{
		CheckOutByAmazonPaymentModel cbaModel = (CheckOutByAmazonPaymentModel)this.Model;
		if (!cbaModel.IsCheckingOut)
		{			
			Response.Redirect("shoppingcart.aspx");
		}
	}

	public override void ShowError(string message)
	{
	}

	#endregion

    public void FirePaymentFormSubmitted()
    {
        if (PaymentFormSubmitted != null)
            PaymentFormSubmitted(this, null);
    }

    public event EventHandler PaymentFormSubmitted;
}
