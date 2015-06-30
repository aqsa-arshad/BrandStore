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
using Vortx.OnePageCheckout.WebUtility;
using Vortx.OnePageCheckout.Models.PaymentMethods;
using Vortx.OnePageCheckout.Settings;
using AspDotNetStorefront;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways.Processors;
using AspDotNetStorefrontGateways;

public partial class OPCControls_PaymentFormAndBillingAddress :
    OPCUserControl<IPaymentModel>,
    IPaymentView
{   
    #region Properties

    public OPCUserControl<PaymentMethodBaseModel> PaymentMethodView { get; set; }

    //Added so we can tell when to hide/show the first pay controls    
    private GatewayProcessor activeGateway;
    private GatewayProcessor ActiveGateway
    {
        get
        {
            if (activeGateway == null)
            {
                SkinBase page = HttpContext.Current.Handler as SkinBase;
                if (page != null)
                {
                    string gateway = AppLogic.ActivePaymentGatewayCleaned();
                    activeGateway = GatewayLoader.GetProcessor(gateway);
                }
            }
            return activeGateway;
        }
    }

    private bool IsFirstPay
    {
        get
        {
            if (ActiveGateway.GatewayIdentifier.ToUpper() == "FIRSTPAY")
                return true;
            else
                return false;
        }
    }

    #endregion

    #region Page Events

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        OrderDetailsChanged += (o, ea) => MoneybookersQuickCheckoutPaymentView.ResetCheckout();
    }

    protected void OPCControls_PaymentFormAndBillingAddress_PaymentFormSubmitted(object source, EventArgs args)
    {
        this.SaveViewToModel();
    }

    #endregion
    
    #region Protected Methods

    protected IDictionary<PaymentType, OPCUserControl<PaymentMethodBaseModel>> LoadPaymentViewTypes()
    {
        var controls = this.PanelPaymentMethod.Controls
          .OfType<OPCUserControl<PaymentMethodBaseModel>>()
          .Cast<OPCUserControl<PaymentMethodBaseModel>>();

        return (from paymentControl in controls
                let paymentTypeAttribute = paymentControl.GetType().GetCustomAttributes(typeof(PaymentTypeAttribute), true)
                .Cast<PaymentTypeAttribute>().FirstOrDefault()
                where paymentTypeAttribute != null
                select new { paymentTypeAttribute.PaymentType, paymentControl }).ToDictionary(f => f.PaymentType, f => f.paymentControl);
    }

    protected IDictionary<PaymentType, OPCUserControl<PaymentMethodBaseModel>> _PaymentViewTypes;
    protected IDictionary<PaymentType, OPCUserControl<PaymentMethodBaseModel>> PaymentViewTypes
    {
        get
        {
            if (_PaymentViewTypes == null)
            {
                _PaymentViewTypes = LoadPaymentViewTypes();
            }

            return _PaymentViewTypes;
        }
    }

    protected OPCUserControl<PaymentMethodBaseModel> FindPaymentMethodView(PaymentType paymentType)
    {
        var paymentViewControl = PaymentViewTypes.FirstOrDefault(p => p.Key == paymentType).Value;
        if (paymentViewControl != null)
        {
            return (OPCUserControl<PaymentMethodBaseModel>)paymentViewControl;
        }
        return null;
    }

    protected void ActivatePaymentView()
    {
        // hide the existing view
        if (this.PaymentMethodView != null)
        {
            this.PaymentMethodView.Hide();
            this.SecureNetPaymentView.Hide();
            this.TwoCheckoutPaymentView.Hide();
            this.WorldPayPaymentView.Hide();
        }

        if (Model.ActivePaymentMethod != null)
        {
            var paymentView = FindPaymentMethodView(Model.ActivePaymentMethod.PaymentType);
            if (paymentView != null)
            {
                this.PaymentMethodView = paymentView;
            }
        }
    }

    protected void BindPaymentView()
    {
        if (this.PaymentMethodView == null)
            return;

        bool showPaymentView = true;
        if (this.Model.ActivePaymentMethod != null && this.Model.ActivePaymentMethod.PaymentType == PaymentType.CreditCard)
        {
            if (AspDotNetStorefrontCore.AppLogic.ActivePaymentGatewayCleaned() ==
                AspDotNetStorefrontGateways.Gateway.ro_GWTWOCHECKOUT)
            {
                showPaymentView = false;

                TwoCheckoutPaymentView.Initialize();
                TwoCheckoutPaymentView.Show();
                TwoCheckoutPaymentView.BindView();
            }
            else if (AspDotNetStorefrontCore.AppLogic.ActivePaymentGatewayCleaned() ==
                (new AspDotNetStorefrontGateways.Processors.Worldpay()).GatewayIdentifier)
            {
                showPaymentView = false;

                WorldPayPaymentView.Initialize();
                WorldPayPaymentView.Show();
                WorldPayPaymentView.BindView();
            }
            else
            {
                if (SecureNetPaymentView.IsVaultPaymentSelected)
                {
                    showPaymentView = false;
                }

                SecureNetPaymentView.Initialize();
                SecureNetPaymentView.Show();
                SecureNetPaymentView.BindView();
            }
        }

        if (showPaymentView)
        {
            this.PaymentMethodView.Initialize();
            this.PaymentMethodView.Show();
            this.PaymentMethodView.BindView();
        }
    }
    
    protected void ShowPaymentView()
    {
        if (this.PaymentMethodView != null)
        {
            if (IsTwoCheckoutPayment())
            {
                this.TwoCheckoutPaymentView.Show();
                return;
            }
            else if (IsWorldPayPayment())
            {
                this.WorldPayPaymentView.Show();
                return;
            }
            else if (IsFirstPay)
            {
                FirstPayPaymentForm.gateway = ActiveGateway;
                FirstPayPaymentForm.thisCustomer = Vortx.OnePageCheckout.Models.Adnsf9200.AdnsfUtility.ContextCustomer;
                FirstPayPaymentForm.Show();
                FirstPayPaymentForm.Initialize();
                return;
            }
            this.PaymentMethodView.Show();
        }
    }

    protected void HidePaymentView()
    {
        if (this.PaymentMethodView != null)
        {
            if (IsTwoCheckoutPayment())
            {
                this.TwoCheckoutPaymentView.Hide();
            }
            else if (IsWorldPayPayment())
            {
                this.WorldPayPaymentView.Show();
                return;
            }
            this.PaymentMethodView.Hide();
        }
    }

    protected bool IsTwoCheckoutPayment()
    {
        if (this.Model.ActivePaymentMethod != null && this.Model.ActivePaymentMethod.PaymentType == PaymentType.CreditCard)
        {
            if (AspDotNetStorefrontCore.AppLogic.ActivePaymentGatewayCleaned() ==
                AspDotNetStorefrontGateways.Gateway.ro_GWTWOCHECKOUT)
            {
                return true;
            }
        }
        return false;
    }

    protected bool IsWorldPayPayment()
    {
        if (this.Model.ActivePaymentMethod != null && this.Model.ActivePaymentMethod.PaymentType == PaymentType.CreditCard)
        {
            if (AspDotNetStorefrontCore.AppLogic.ActivePaymentGatewayCleaned() ==
                (new AspDotNetStorefrontGateways.Processors.Worldpay()).GatewayIdentifier)
            {
                return true;
            }
        }
        return false;
    }


    protected bool AllowBillingAddressEdit()
    {
		if (!AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo"))
			return false;

        if (this.Model.ActivePaymentMethod != null && this.Model.ActivePaymentMethod.PaymentType == PaymentType.CreditCard)
            return true;

        if (Vortx.OnePageCheckout.Settings.AppSettingsProvider.DefaultProvider.AllowAlternativePaymentBillingAddressEdit)
        {
            return this.Model.ActivePaymentMethod != null &&
                this.Model.ActivePaymentMethod.AllowBillingAddressEdit;
        }

        return false;
    }

    #endregion

    #region IView Members

    public event EventHandler OrderDetailsChanged;

    public void NotifyOrderDetailsChanged()
    {
        FireOrderDetailsChanged();
    }

    private void FireOrderDetailsChanged()
    {
        if (OrderDetailsChanged != null)
            OrderDetailsChanged(this, EventArgs.Empty);
    }

    public override void SetModel(IPaymentModel model, IStringResourceProvider stringResourceProvider)
    {
        base.SetModel(model, stringResourceProvider);

        this.BillingAddressChoice.SetModel(model.AccountModel, this.StringResourceProvider);

        CodPaymentView.SetModel(null, this.StringResourceProvider);
        CreditCardPaymentForm.SetModel(null, this.StringResourceProvider);
        SecureNetPaymentView.SetModel(null, this.StringResourceProvider);
        TwoCheckoutPaymentView.SetModel(null, this.StringResourceProvider);
        CheckPaymentView.SetModel(null, this.StringResourceProvider);
        RequestQuoteView.SetModel(null, this.StringResourceProvider);
        PayPalPaymentView.SetModel(null, this.StringResourceProvider);
        PurchaseOrderPaymentView.SetModel(null, this.StringResourceProvider);
        PayPalExpressView.SetModel(null, this.StringResourceProvider);
        MicroPayPaymentView.SetModel(null, this.StringResourceProvider);
        CheckOutByAmazonPaymentView.SetModel(null, this.StringResourceProvider);
        PayPalEmbeddedCheckoutPaymentView.SetModel(null, this.StringResourceProvider);
        MoneybookersQuickCheckoutPaymentView.SetModel(null, this.StringResourceProvider);
        ECheckPaymentView.SetModel(null, this.StringResourceProvider);
        WorldPayPaymentView.SetModel(null, this.StringResourceProvider);

        foreach (var paymentModel in this.Model.PaymentMethods)
        {
            var paymentView = FindPaymentMethodView(paymentModel.Key);
            if (paymentView != null)
            {
                paymentView.SetModel(paymentModel.Value, this.StringResourceProvider);
                ((IPaymentMethodView)paymentView).PaymentFormSubmitted += OPCControls_PaymentFormAndBillingAddress_PaymentFormSubmitted;                
            }
            
            if (paymentModel.Value.PaymentType == PaymentType.SecureNet)
            {
                SecureNetPaymentView.ExistingCreditCardSelected += (o, e) => CreditCardPaymentForm.Hide();
                SecureNetPaymentView.NewCreditCardSelected += (o, e) =>
                    {
                        CreditCardPaymentForm.Initialize();
                        CreditCardPaymentForm.Show();
                        CreditCardPaymentForm.BindView();
                    };

                Model.ProcessingPayment += (o, e) =>
                {
                    if (!(e.PaymentMethod is CreditCardPaymentModel) && !(e.PaymentMethod is SecureNetPaymentModel))
                        return;

                    ((SecureNetPaymentModel)paymentModel.Value).SwitchPaymentMethod();
                };
            }            
        }

        this.ActivatePaymentView();
    }

	public override void BindView()
	{
        if (AllowBillingAddressEdit())
        {
            this.PnlBillingAddressChoice.Visible = true;
            this.BillingAddressChoice.BindView();
        }
        else
        {
            this.PnlBillingAddressChoice.Visible = false;
        }

        this.ActivatePaymentView();
        this.BindPaymentView();
    }

	public override void BindView(object id)
	{
        if (AllowBillingAddressEdit())
        {
            this.PnlBillingAddressChoice.Visible = true;
            this.BillingAddressChoice.BindView(id);
        }
        else
        {
            this.PnlBillingAddressChoice.Visible = false;
        }

        this.ActivatePaymentView();
        this.BindPaymentView();
    }

	public override void Initialize()
	{
        this.BillingAddressChoice.Initialize();

        if (this.PaymentMethodView != null)
        {
            this.PaymentMethodView.Initialize();
        }
	}

	public override void Disable()
	{
        this.BillingAddressChoice.Disable();
        if (this.PaymentMethodView != null)
        {
            CreditCardPaymentForm.Disable();
            SecureNetPaymentView.Disable();
            TwoCheckoutPaymentView.Disable();
            CheckPaymentView.Disable();
            RequestQuoteView.Disable();
            PayPalPaymentView.Disable();
            PurchaseOrderPaymentView.Disable();
            PayPalExpressView.Disable();
            MicroPayPaymentView.Disable();
            CheckOutByAmazonPaymentView.Disable();
            PayPalEmbeddedCheckoutPaymentView.Disable();
            MoneybookersQuickCheckoutPaymentView.Disable();
            ECheckPaymentView.Disable();
            WorldPayPaymentView.Disable();
            if (IsFirstPay)
                this.FirstPayPaymentForm.Disable();
        }
    }

	public override void Enable()
	{
        this.BillingAddressChoice.Enable();
        if (this.PaymentMethodView != null)
        {
            CreditCardPaymentForm.Enable();
            SecureNetPaymentView.Enable();
            TwoCheckoutPaymentView.Enable();
            CheckPaymentView.Enable();
            RequestQuoteView.Enable();
            PayPalPaymentView.Enable();
            PurchaseOrderPaymentView.Enable();
            PayPalExpressView.Enable();
            MicroPayPaymentView.Enable();
            CheckOutByAmazonPaymentView.Enable();
            PayPalEmbeddedCheckoutPaymentView.Enable();
            MoneybookersQuickCheckoutPaymentView.Enable();
            ECheckPaymentView.Enable();
            WorldPayPaymentView.Enable();
            if (IsFirstPay)
                this.FirstPayPaymentForm.Enable();
        }
    }

    public override void Show()
    {
        this.Visible = true;

        if (AllowBillingAddressEdit())
        {
            this.PnlBillingAddressChoice.Visible = true;
            this.BillingAddressChoice.Show();
        }
        else
        {
            this.PnlBillingAddressChoice.Visible = false;
        }

        this.ShowPaymentView();
        this.UpdatePanelPaymentFormAndBillingAddress.Visible = true;
            
    }

	public override void Hide()
	{
        this.Visible = false; 
        this.BillingAddressChoice.Hide();
        this.HidePaymentView();
        this.UpdatePanelPaymentFormAndBillingAddress.Visible = false;
        this.UpdatePanelPaymentFormAndBillingAddress.Update();
	}

	public override void ShowError(string message)
	{

	}

    public override void SaveViewToModel()
    {
        if (AllowBillingAddressEdit())
        {
            BillingAddressChoice.SaveViewToModel();
        }
        if (this.PaymentMethodView != null)
        {
            if (this.Model.ActivePaymentMethod != null && this.Model.ActivePaymentMethod.PaymentType == PaymentType.CreditCard)
            {
                if (AspDotNetStorefrontCore.AppLogic.ActivePaymentGatewayCleaned() ==
                    AspDotNetStorefrontGateways.Gateway.ro_GWTWOCHECKOUT)
                {
                    TwoCheckoutPaymentView.SaveViewToModel();
                    return;
                }
                else if (AspDotNetStorefrontCore.AppLogic.ActivePaymentGatewayCleaned() ==
                    (new AspDotNetStorefrontGateways.Processors.Worldpay()).GatewayIdentifier)
                {
                    WorldPayPaymentView.SaveViewToModel();
                    return;
                }
                else if (AspDotNetStorefrontCore.AppLogic.ActivePaymentGatewayCleaned() == (new AspDotNetStorefrontGateways.Processors.SecureNetV4()).GatewayIdentifier)
                {
                    SecureNetPaymentView.SaveViewToModel();
                }
            }
            PaymentMethodView.SaveViewToModel();
        }
    }

	#endregion
}
