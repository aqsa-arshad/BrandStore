// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways.Processors;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.Models.PaymentMethods;
using Vortx.OnePageCheckout.Views;
using Vortx.OnePageCheckout.UI;

[PaymentType(Vortx.OnePageCheckout.Models.PaymentType.FirstPay)]
public partial class OPCControls_PaymentForms_FirstPayEmbeddedCheckoutPaymentForm :
    OPCUserControl<PaymentMethodBaseModel>,
    IPaymentMethodView
{
    public GatewayProcessor gateway { get; set; }
    public Customer thisCustomer { get; set; }

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
        if (gateway == null || thisCustomer == null)
            return;

        string ccPane = gateway.CreditCardPaneInfo(thisCustomer.SkinID, thisCustomer);

        litFirstPayEmbeddedCheckoutFrame.Text = ccPane;

        if (CommonLogic.QueryStringNativeInt("ErrorMsg") > 0)
        {
            ErrorMessage e = new ErrorMessage(CommonLogic.QueryStringNativeInt("ErrorMsg"));
            ShowError(e.Message);
        }
    }

    public override void Disable()
    {
        this.Visible = false;
    }

    public override void Enable()
    {
        this.Visible = true;
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
