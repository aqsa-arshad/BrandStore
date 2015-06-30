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
using Vortx.OnePageCheckout.WebUtility;
using Vortx.OnePageCheckout.Views;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.Models.PaymentMethods;

public partial class VortxControls_PaymentMethodSelector :
    OPCUserControl<IPaymentModel>,
	IPaymentView
{
	#region Page Events

	private void SetActivePaymentMethodFromForm(PaymentType paymentType)
	{
		PaymentMethodBaseModel method = this.Model.PaymentMethods
			.FirstOrDefault(pm => pm.Key == paymentType).Value;

		ToggleCheckoutByAmazonForm(false);

		this.Model.SetActivePaymentMethod(method.MethodId);
	}
	protected void RadioCreditCard_CheckedChanged(object sender, EventArgs e)
	{
        SetActivePaymentMethodFromForm(PaymentType.CreditCard);
	}
	protected void RadioPurchaseOrder_CheckedChanged(object sender, EventArgs e)
	{
		SetActivePaymentMethodFromForm(PaymentType.PurchaseOrder);
	}
	protected void RadioPayPal_CheckedChanged(object sender, EventArgs e)
	{
		SetActivePaymentMethodFromForm(PaymentType.PayPal);
	}
	protected void RadioCheckByMail_CheckedChanged(object sender, EventArgs e)
	{
		SetActivePaymentMethodFromForm(PaymentType.CheckByMail);
	}
	protected void RadioPayPalExpress_CheckedChanged(object sender, EventArgs e)
	{
		SetActivePaymentMethodFromForm(PaymentType.PayPalExpress);
	}
	protected void RadioRequestQuote_CheckedChanged(object sender, EventArgs e)
	{
		SetActivePaymentMethodFromForm(PaymentType.RequestAQuote);
	}
	protected void RadioMicroPay_CheckedChanged(object sender, EventArgs e)
	{
		SetActivePaymentMethodFromForm(PaymentType.MicroPay);
	}
	protected void RadioCod_CheckedChanged(object sender, EventArgs e)
	{
		SetActivePaymentMethodFromForm(PaymentType.COD);
	}
	protected void RadioCheckOutByAmazon_CheckedChanged(object sender, EventArgs e)
	{
		SetActivePaymentMethodFromForm(PaymentType.CheckoutByAmazon);
	}
	protected void RadioPayPalEmbeddedCheckout_CheckedChanged(object sender, EventArgs e)
	{
		SetActivePaymentMethodFromForm(PaymentType.PayPalEmbeddedCheckout);
	}
	protected void RadioMoneybookersQuickCheckout_CheckedChanged(object sender, EventArgs e)
	{
		SetActivePaymentMethodFromForm(PaymentType.MoneybookersQuickCheckout);
	}
    protected void RadioECheck_CheckedChanged(object sender, EventArgs e)
    {
        SetActivePaymentMethodFromForm(PaymentType.ECheck);
    }

	#endregion

	#region Private Methods

	private void HidePaymentForms()
	{
        ToggleForm(RadioCheckByMail, false);
        ToggleForm(RadioPayPal, false);
        ToggleForm(RadioCreditCard, false);
        ToggleForm(RadioPurchaseOrder, false);
        ToggleForm(RadioRequestQuote, false);
        ToggleForm(RadioMicroPay, false);
        ToggleForm(RadioCod, false);
        ToggleForm(RadioECheck, false);
        TogglePayPalExpressForm(false);
        ToggleCheckoutByAmazonForm(false);
        ToggleForm(RadioPayPalEmbeddedCheckout, false);
        ToggleForm(RadioMoneybookersQuickCheckout, false);
	}

	private void HidePaymentFormPanels()
	{
		PanelCreditCardMethod.Visible = false;
		RadioCreditCard.Visible = false;

		PanelCheckMoneyOrderMethod.Visible = false;
		RadioCheckByMail.Visible = false;

		PanelPayPalMethod.Visible = false;
		RadioPayPal.Visible = false;

		PanelPaypalExpressMethod.Visible = false;
		RadioPayPalExpress.Visible = false;

		PanelRequestQuote.Visible = false;
		RadioPurchaseOrder.Visible = false;

		PanelMicroPayMethod.Visible = false;
		RadioMicroPay.Visible = false;

		PanelCodMethod.Visible = false;
		RadioCod.Visible = false;

		PanelCheckoutByAmazon.Visible = false;
		RadioCheckoutByAmazon.Visible = false;

		PanelPayPalEmbeddedCheckout.Visible = false;
		RadioPayPalEmbeddedCheckout.Visible = false;

		PanelMoneybookersQuickCheckout.Visible = false;
		RadioMoneybookersQuickCheckout.Visible = false;

        PanelECheck.Visible = false;
        RadioECheck.Visible = false;
	}

    private void ActivatePaymentMethodChoice(Panel paymentPanel, RadioButton radioButton)
    {
        if (paymentPanel != null)
        {
            paymentPanel.Visible = true;
        }

        if (radioButton != null)
        {
            radioButton.Visible = true;
            radioButton.Enabled = true;
            radioButton.Checked = false;
        }
    }

	private void BindPaymentMethods()
	{
		if (Model != null)
		{
			var activePaymentMethod = this.Model.ActivePaymentMethod;
            PaymentFormAndBillingAddress.BindView();

            var havePayPalEmbedded = this.Model.PaymentMethods.Any(s => s.Value.PaymentType == PaymentType.PayPalEmbeddedCheckout);
            var havePayPalExpress = this.Model.PaymentMethods.Any(s => s.Value.PaymentType == PaymentType.PayPalExpress);
            var havePayPalStandard = this.Model.PaymentMethods.Any(s => s.Value.PaymentType == PaymentType.PayPal);

            bool payPalExpressCheckout = IsPayPalExpressCheckout(activePaymentMethod, havePayPalExpress);

            DefaultToPayPalEmbedded(activePaymentMethod, payPalExpressCheckout, havePayPalEmbedded);
            
            foreach (var kvp in this.Model.PaymentMethods)
            {
                PaymentMethodBaseModel method = kvp.Value;
                switch (method.PaymentType)
                {
                    case PaymentType.CreditCard:
                        {
                            // if we have Paypal Advanced, then let them handle CC Processing
                            if (!havePayPalEmbedded)
                            {
                                ActivatePaymentMethodChoice(PanelCreditCardMethod, RadioCreditCard);

                                IEnumerable<CreditCardType> cardTypes = ((CreditCardPaymentModel)method).AllowedCardTypes;

                                ImageCardTypeVisa.Visible = cardTypes.Any(f => f.CardType == CreditCardTypeEnum.Visa);
                                ImageCardTypeMastercard.Visible = cardTypes.Any(f => f.CardType == CreditCardTypeEnum.MasterCard);
                                ImageCardTypeAmex.Visible = cardTypes.Any(f => f.CardType == CreditCardTypeEnum.AmericanExpress);
                                ImageCardTypeDiscover.Visible = cardTypes.Any(f => f.CardType == CreditCardTypeEnum.Discover);
                                ImageCardTypeSolo.Visible = cardTypes.Any(f => f.CardType == CreditCardTypeEnum.Solo);
                                ImageCardTypeMaestro.Visible = cardTypes.Any(f => f.CardType == CreditCardTypeEnum.Maestro);
                            }
                        }; break;
                    case PaymentType.CheckByMail:
                        {
                            ActivatePaymentMethodChoice(PanelCheckMoneyOrderMethod, RadioCheckByMail);
                        }; break;
                    case PaymentType.RequestAQuote:
                        {
                            ActivatePaymentMethodChoice(PanelRequestQuote, RadioRequestQuote);
                        }; break;
                    case PaymentType.PurchaseOrder:
                        {
                            ActivatePaymentMethodChoice(PanelPurchaseOrderMethod, RadioPurchaseOrder);
                        }; break;
                    case PaymentType.MicroPay:
                        {
                            ActivatePaymentMethodChoice(PanelMicroPayMethod, RadioMicroPay);
                        }; break;
                    case PaymentType.COD:
                        {
                            ActivatePaymentMethodChoice(PanelCodMethod, RadioCod);
                        }; break;
                    case PaymentType.CheckoutByAmazon:
                        {
                            if (IsCheckoutByAmazonCheckingOut())
                            {
                                PanelCheckoutByAmazon.Visible = true;
                                RadioCheckoutByAmazon.Visible = true;
                                RadioCheckoutByAmazon.Enabled = true;
                                RadioCheckoutByAmazon.Checked = false;
                            }
                        }; break;

                    case PaymentType.MoneybookersQuickCheckout:
                        {
                            ActivatePaymentMethodChoice(PanelMoneybookersQuickCheckout, RadioMoneybookersQuickCheckout);
                        } break;
                    case PaymentType.PayPalEmbeddedCheckout:
                        {
                            // PayPal Advanced enables PayPal Express
                            // So make sure this isn't an express checkout originating from the cart page.
                            if (!payPalExpressCheckout)
                            {
                                ActivatePaymentMethodChoice(PanelPayPalEmbeddedCheckout, RadioPayPalEmbeddedCheckout);
                            }
                        } break;
                    case PaymentType.PayPalExpress:
                        {
                            // Show PayPal Express if Embedded is not enabled or we are using Express to Checkout.
                            if (payPalExpressCheckout || !havePayPalEmbedded)
                            {
                                ActivatePaymentMethodChoice(PanelPaypalExpressMethod, RadioPayPalExpress);
                            }
                        } break;
                    case PaymentType.PayPal:
                        {
                            // Only allow PayPal standard if not using express and embedded
                            if (!havePayPalExpress && !havePayPalEmbedded)
                            {
                                ActivatePaymentMethodChoice(PanelPayPalMethod, RadioPayPal);
                            }
                        } break;
                    case PaymentType.ECheck:
                        {
                            ActivatePaymentMethodChoice(PanelECheck, RadioECheck);
                        } break;
                }
            }
		}
	}
	
	private bool IsPayPalExpressCheckout(PaymentMethodBaseModel activePaymentMethod, bool havePayPalExpress)
	{
		if (activePaymentMethod != null && activePaymentMethod.PaymentType == PaymentType.PayPalExpress)
		{
			var paymentModel = (PaypalExpressPaymentModel)activePaymentMethod;
			return paymentModel.ExpressLoginComplete && havePayPalExpress;
		}
		return false;
	}

    protected bool IsCheckoutByAmazonCheckingOut()
    {
        if (this.Model.ActivePaymentMethod != null && this.Model.ActivePaymentMethod.PaymentType == PaymentType.CheckoutByAmazon)
        {
            return ((CheckOutByAmazonPaymentModel) this.Model.ActivePaymentMethod).IsCheckingOut;
        }
        return false;
    }

	private void DefaultToPayPalEmbedded(PaymentMethodBaseModel activePaymentMethod, bool expressLoginComplete, bool havePayPalEmbedded)
	{
		if (havePayPalEmbedded)
		{
			if (activePaymentMethod != null)
			{
				if (activePaymentMethod.PaymentType == PaymentType.PayPalExpress || activePaymentMethod.PaymentType == PaymentType.PayPal)
				{
					if (!expressLoginComplete)
					{
						var embeddedModel = this.Model.PaymentMethods.FirstOrDefault(s => s.Value.PaymentType == PaymentType.PayPalEmbeddedCheckout);
						Model.SetActivePaymentMethod(embeddedModel.Value.MethodId);
					}
				}
			}
		}
	}

    private void ToggleForm(RadioButton radioButton, bool show)
	{
		if (show)
		{
            HidePaymentForms();

            radioButton.Checked = true;
		}
		else
		{
			radioButton.Checked = false;
		}
	}

    private void TogglePayPalExpressForm(bool show)
    {
        if (show)
        {
            HidePaymentForms();
            
            RadioPayPalExpress.Checked = true;
            
            if (IsPayPalExpressCheckout(this.Model.ActivePaymentMethod, 
                this.Model.PaymentMethods.Any(s => s.Value.PaymentType == PaymentType.PayPalExpress)))
            {
                HidePaymentFormPanels();
                PanelPaypalExpressMethod.Visible = true;
                RadioPayPalExpress.Visible = true;
            }
        }
        else
        {
            RadioPayPalExpress.Checked = false;            
        }
    }

    private void ToggleCheckoutByAmazonForm(bool show)
    {
        if (show)
        {
            if (IsCheckoutByAmazonCheckingOut())
            {
                HidePaymentFormPanels();
                HidePaymentForms();

                PanelCheckoutByAmazon.Visible = true;
                RadioCheckoutByAmazon.Visible = true;
                RadioCheckoutByAmazon.Checked = true;
            }
        }
        else
        {
            RadioCheckoutByAmazon.Checked = false;
        }
    }
    
	private void SetSelectedPaymentMethod()
	{
		if (this.Model != null)
		{
			PaymentMethodBaseModel method = this.Model.ActivePaymentMethod;
			if (method != null)
			{
                switch (method.PaymentType)
                {
                    case PaymentType.CreditCard:                   
                    case PaymentType.SecureNet:
                    case PaymentType.TwoCheckout:
                    case PaymentType.WorldPay:
                        {
                            ToggleForm(RadioCreditCard, true);
                        }; break;
                    case PaymentType.CheckByMail:
                        {
                            ToggleForm(RadioCheckByMail, true);
                        }; break;
                    case PaymentType.PurchaseOrder:
                        {
                            ToggleForm(RadioPurchaseOrder, true);
                        }; break;
                    case PaymentType.RequestAQuote:
                        {
                            ToggleForm(RadioRequestQuote, true);
                        }; break;
                    case PaymentType.PayPal:
                        {
                            ToggleForm(RadioPayPal, true);
                        } break;
                    case PaymentType.PayPalExpress:
                        {
                            TogglePayPalExpressForm(true);
                        } break;
                    case PaymentType.MicroPay:
                        {
                            ToggleForm(RadioMicroPay, true);
                        } break;
                    case PaymentType.COD:
                        {
                            ToggleForm(RadioCod, true);
                        } break;
                    case PaymentType.CheckoutByAmazon:
                        {
                            ToggleCheckoutByAmazonForm(true);
                        } break;
                    case PaymentType.PayPalEmbeddedCheckout:
                        {
                            ToggleForm(RadioPayPalEmbeddedCheckout, true);
                        } break;
                    case PaymentType.MoneybookersQuickCheckout:
                        {
                            ToggleForm(RadioMoneybookersQuickCheckout, true);
                        } break;
                    case PaymentType.ECheck:
                        {
                            ToggleForm(RadioECheck, true);
                        } break;
                }
			}
		}
	}
	#endregion

	#region IView Members

    public override void SetModel(IPaymentModel model, IStringResourceProvider stringResourceProvider)
    {
        base.SetModel(model, stringResourceProvider);
        
        PaymentFormAndBillingAddress.SetModel(model, stringResourceProvider);
	}

	public override void Initialize()
	{
		HidePaymentForms();

		PanelCreditCardMethod.Visible = false;
		RadioCreditCard.Visible = false;

		PanelCheckMoneyOrderMethod.Visible = false;
		RadioCheckByMail.Visible = false;

		PanelPayPalMethod.Visible = false;
		RadioPayPal.Visible = false;

		PanelPaypalExpressMethod.Visible = false;
		RadioPayPalExpress.Visible = false;

		PanelRequestQuote.Visible = false;
		RadioPurchaseOrder.Visible = false;

		PanelMicroPayMethod.Visible = false;
		RadioMicroPay.Visible = false;

		PanelCodMethod.Visible = false;
		RadioCod.Visible = false;

		PanelCheckoutByAmazon.Visible = false;
		RadioCheckoutByAmazon.Visible = false;

		PanelPayPalEmbeddedCheckout.Visible = false;
		RadioPayPalEmbeddedCheckout.Visible = false;

		PanelMicroPayMethod.Visible = false;
		RadioMicroPay.Visible = false;

		PanelMoneybookersQuickCheckout.Visible = false;
		RadioMoneybookersQuickCheckout.Visible = false;
        
        PanelECheck.Visible = false;
        RadioECheck.Visible = false;
	}

	public override void Disable()
	{
		RadioCreditCard.Enabled = false;
		RadioCheckByMail.Enabled = false;
		RadioPayPal.Enabled = false;
		RadioPayPalExpress.Enabled = false;
		RadioPurchaseOrder.Enabled = false;
		RadioRequestQuote.Enabled = false;
		RadioMicroPay.Enabled = false;
		RadioCod.Enabled = false;
		RadioCheckoutByAmazon.Enabled = false;
		RadioPayPalEmbeddedCheckout.Enabled = false;
		RadioMoneybookersQuickCheckout.Enabled = false;        
        RadioECheck.Enabled = false;

        this.PaymentFormAndBillingAddress.Disable();
	}

	public override void Enable()
	{
		RadioCreditCard.Enabled = true;
		RadioCheckByMail.Enabled = true;
		RadioPayPal.Enabled = true;
		RadioPayPalExpress.Enabled = true;
		RadioPurchaseOrder.Enabled = true;
		RadioRequestQuote.Enabled = true;
		RadioMicroPay.Enabled = true;
		RadioCod.Enabled = true;
		RadioCheckoutByAmazon.Enabled = true;
		RadioPayPalEmbeddedCheckout.Enabled = true;
		RadioMoneybookersQuickCheckout.Enabled = true;
        RadioECheck.Enabled = true;

        this.PaymentFormAndBillingAddress.Enable();
    }

	public override void Show()
	{
		this.Visible = true;

        this.PaymentFormAndBillingAddress.Show();

		this.UpdatePanelPaymentInfo.Update();
	}

	public override void Hide()
	{
		HidePaymentForms();

		this.Visible = false;
		this.UpdatePanelPaymentInfo.Update();
	}

	public override void BindView()
	{
		BindPaymentMethods();

		PanelError.Visible = false;

		SetSelectedPaymentMethod();
	}

    public override void BindView(object orderNumber)
	{
		Response.Redirect("~/orderconfirmation.aspx?ordernumber=" + Convert.ToInt32(orderNumber).ToString() +
			"&paymentmethod=" + Server.UrlEncode(this.Model.ActivePaymentMethod.Name));
	}

	public override void SaveViewToModel()
	{
        this.PaymentFormAndBillingAddress.SaveViewToModel();
	}

	public override void ShowError(string message)
	{
		PanelError.Visible = true;
		ErrorMessage.Text = message;
	}

	#endregion

	public void NotifyOrderDetailsChanged()
	{
        this.PaymentFormAndBillingAddress.NotifyOrderDetailsChanged();
	}
}
