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

[PaymentType(Vortx.OnePageCheckout.Models.PaymentType.COD)]
public partial class OPCControls_CodPaymentForm :
    OPCUserControl<PaymentMethodBaseModel>,
    IPaymentMethodView
{
	public override void BindView()
	{
	}

	public override void BindView(object identifier)
	{
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
    }	

    public override void Hide()
    {
		this.Visible = false;
    }

    public override void ShowError(string message)
    {
		PanelError.Visible = true;
		ErrorMessage.Text = message;
    }

    public void FirePaymentFormSubmitted()
    {
        if (PaymentFormSubmitted != null)
            PaymentFormSubmitted(this, null);
    }

    public event EventHandler PaymentFormSubmitted;
}
