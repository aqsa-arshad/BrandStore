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

[PaymentType(Vortx.OnePageCheckout.Models.PaymentType.PurchaseOrder)]
public partial class OPCControls_PurchaseOrderPaymentForm :
    OPCUserControl<PaymentMethodBaseModel>,
    IPaymentMethodView
{
    protected PurchaseOrderPaymentModel PurchaseOrderModel;
    public override void SetModel(PaymentMethodBaseModel model, IStringResourceProvider stringResourceProvider)
    {
        base.SetModel(model, stringResourceProvider);

        PurchaseOrderModel = (PurchaseOrderPaymentModel)model;
    }

	public override void BindView()
	{
        this.TextBoxPONumber.Text = this.PurchaseOrderModel.PONumber;
	}

	public override void BindView(object identifier)
	{
	}

	public override void SaveViewToModel()
	{
		Page.Validate("VGPOPayment");
		if (Page.IsValid)
		{
            this.PurchaseOrderModel.PONumber = this.TextBoxPONumber.Text;
		}
	}

	public override void Initialize()
	{
	}

	public override void Disable()
	{
		this.TextBoxPONumber.Enabled = false;
	}

	public override void Enable()
	{
		this.TextBoxPONumber.Enabled = true;
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

	protected void ButtonPONext_Click(object sender, EventArgs e)
	{
        FirePaymentFormSubmitted();
    }

    public void FirePaymentFormSubmitted()
    {
        if (PaymentFormSubmitted != null)
            PaymentFormSubmitted(this, null);
    }

    public event EventHandler PaymentFormSubmitted;
}
