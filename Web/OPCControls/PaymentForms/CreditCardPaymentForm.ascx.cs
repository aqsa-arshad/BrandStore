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

[Vortx.OnePageCheckout.Views.PaymentType(Vortx.OnePageCheckout.Models.PaymentType.CreditCard)]
public partial class OPCControls_CreditCardPaymentForm :
    OPCUserControl<PaymentMethodBaseModel>,
    IPaymentMethodView
{
    public bool WalletSelected
    {
        get
        {
            if (Session["WalletSelected"] != null)
                return (bool)Session["WalletSelected"];
            return false;
        }
        set { Session["WalletSelected"] = value; }
    }

    private Int64 paymentProfileId
    {
        get
        {
            return Session["CCPaymentFormNewProfileId"] == null ? 0 : (Int64)Session["CCPaymentFormNewProfileId"];
        }
        set
        {
            Session["CCPaymentFormNewProfileId"] = value;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        AspDotNetStorefrontGateways.Processors.AuthorizeNet authorizeNet = new AspDotNetStorefrontGateways.Processors.AuthorizeNet();
        if (authorizeNet.IsCimEnabled)
        {
            var customer = AspDotNetStorefrontCore.Customer.Current;
            this.WalletSelector1.Visible = customer.IsRegistered;
            this.WalletSelector1.PaymentProfileSelected += new CIM_WalletSelector.PaymentProfileSelectedHandler(WalletSelector1_PaymentProfileSelected);
        }
        
        //Make sure our paymentProfileId is preserved during postbacks but
        //gets cleared out when a user first enters the page
        if (!IsPostBack)
        {
            paymentProfileId = 0;
        }
    }
    
    protected void WalletSelector1_PaymentProfileSelected(object sender, long paymentProfileId)
    {
        SaveViewToModel();
    }
    
    protected void RadioButtonWallet_CheckedChanged(object sender, EventArgs e)
    {
        if (RadioButtonWallet.Checked)
        {
            WalletSelected = true;

            PanelWallet.Visible = true;
            PanelCreditDetails.Visible = false;

            WalletSelector1.BindPage();
        }
    }

    protected void RadioButtonNewCard_CheckedChanged(object sender, EventArgs e)
    {
        if (RadioButtonNewCard.Checked)
        {
            WalletSelected = false;

            PanelWallet.Visible = false;
            PanelCreditDetails.Visible = true;

            AspDotNetStorefrontCore.Customer.Current.ThisCustomerSession["ActivePaymentProfileId"] = string.Empty;
        }
    }

    protected void SaveCreditCardPaymentForm_Click(object sender, EventArgs e)
    {
        FirePaymentFormSubmitted();
		
        if (Page.IsValid && !PanelError.Visible)
            ButtonSaveCreditCardPaymentForm.Visible = false;
	}

    protected void HFCreditCardTypeValidator_ServerValidate(object source, ServerValidateEventArgs args)
    {
        HFCreditCardTypeValidator.ErrorMessage = this.StringResourceProvider.GetString("checkoutcard_process.aspx.3");
        args.IsValid = false;

        var cardType = string.Empty;
        if (this.CCPaymentMethodModel.AllowedCardTypes.Any(f => f.CardType == CreditCardTypeEnum.Unknown))
        {
            cardType = DDLCreditCardType.SelectedValue;
        }
		else if (!String.IsNullOrEmpty(HFCreditCardType.Value))
        {
            cardType = HFCreditCardType.Value;
        }
		else
		{
			//not an accepted card type, reject it
			return;
		}

        var customer = AspDotNetStorefrontCore.Customer.Current;
        string cardNumberToUse = string.Empty;
        if (CCNumber.Text.StartsWith("****"))
            cardNumberToUse = customer.PrimaryBillingAddress.CardNumber;
        else
            cardNumberToUse = CCNumber.Text;

        args.IsValid = ValidateCard(cardNumberToUse.Replace("-", "").Replace(" ", ""), cardType);
    }

    protected void CVCreditCardType_ServerValidate(object source, ServerValidateEventArgs args)
    {
        args.IsValid = ValidateCard(CCNumber.Text, DDLCreditCardType.SelectedValue);
    }

    #region Private Methods

    private bool ValidateCard(string cardNumber, string cardType)
    {
        bool isValid = false;
        if (AspDotNetStorefrontCore.AppLogic.AppConfigBool("ValidateCreditCardNumbers"))
        {
            var actualCardType = AspDotNetStorefrontCore.CardType.Parse(cardType);
            var cardValidator = new AspDotNetStorefrontCore.CreditCardValidator(cardNumber, actualCardType);
            isValid = cardValidator.Validate();
        }
        else
        {
            isValid = true;
        }

        return isValid;
    }

    private void PopulateMonth()
    {
        this.CCExpiresMonth.DataSource = this.CCPaymentMethodModel.AvailableCCMonths;
        this.CCExpiresMonth.DataTextField = "Key";
        this.CCExpiresMonth.DataValueField = "Value";
        this.CCExpiresMonth.DataBind();
    }
    private void SetSelectedMonth()
    {
        PageUtility.SetSelectedValue(CCExpiresMonth, CCPaymentMethodModel.CCMonth);
    }
    private void PopulateYear()
    {
        this.CCExpiresYear.DataSource = this.CCPaymentMethodModel.AvailableCCYears;
        this.CCExpiresYear.DataTextField = "Value";
        this.CCExpiresYear.DataValueField = "Key";
        this.CCExpiresYear.DataBind();
    }
    private void SetSelectedYear()
    {
        PageUtility.SetSelectedValue(CCExpiresYear, CCPaymentMethodModel.CCYear);
    }

    private void SetCardTypeImageOpacity()
    {
        this.ImageCardTypeVisa.CssClass = "opacity25";
        this.ImageCardTypeMastercard.CssClass = "opacity25";
        this.ImageCardTypeDiscover.CssClass = "opacity25";
        this.ImageCardTypeAmex.CssClass = "opacity25";
        this.ImageCardTypeSolo.CssClass = "opacity25";
        this.ImageCardTypeMaestro.CssClass = "opacity25";

        if(!String.IsNullOrEmpty(this.CCPaymentMethodModel.CCNumber))
        {
            switch(this.CCPaymentMethodModel.CreditCardType.CardType)
            {
                case CreditCardTypeEnum.Visa:
                    this.ImageCardTypeVisa.CssClass = "opacity100"; 
                    break;
                case CreditCardTypeEnum.MasterCard:
                    this.ImageCardTypeMastercard.CssClass = "opacity100"; 
                    break;
                case CreditCardTypeEnum.Discover:
                    this.ImageCardTypeDiscover.CssClass = "opacity100";
                    break;
                case CreditCardTypeEnum.AmericanExpress:
                    this.ImageCardTypeAmex.CssClass = "opacity100"; 
                    break;
                case CreditCardTypeEnum.Solo:
                    this.ImageCardTypeSolo.CssClass = "opacity100"; 
                    break;
                case CreditCardTypeEnum.Maestro:
                    // enable issue number box if maestro card
                    this.CCIssueNumber.Enabled = true;
                    this.ImageCardTypeMaestro.CssClass = "opacity100"; 
                    break;
            }
        }
    }

    #endregion

    #region IPaymentMethod members

    private CreditCardPaymentModel CCPaymentMethodModel { get; set; }
    public override void SetModel(PaymentMethodBaseModel model, IStringResourceProvider stringResourceProvider)
    {
        base.SetModel(model, stringResourceProvider);
        this.CCPaymentMethodModel = (CreditCardPaymentModel)model;
    }

	public override void Initialize()
	{
		PanelError.Visible = false;

		if (this.CCPaymentMethodModel == null)
			return;

        PopulateMonth();
        PopulateYear();

		PanelError.Visible = false;

		this.CCNameOnCard.Text = string.Empty;
		this.CCExpiresMonth.ClearSelection();        
        this.CCExpiresYear.ClearSelection();
		this.CCNumber.Text = string.Empty;
		this.CCSecurityCode.Text = string.Empty;
        this.CCIssueNumber.Text = string.Empty;
		this.HFCreditCardType.Value = string.Empty;

        string javascript = String.Format("javascript:setCreditCardType(event,'{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}');",
                this.HFCreditCardType.ClientID,
                this.ImageCardTypeVisa.ClientID,
                this.ImageCardTypeMastercard.ClientID,
                this.ImageCardTypeDiscover.ClientID,
                this.ImageCardTypeAmex.ClientID,
                this.ImageCardTypeSolo.ClientID,
                this.ImageCardTypeMaestro.ClientID,
                this.CCIssueNumber.ClientID,
                this.CCSecurityCode.ClientID,
                this.CCNumber.ClientID);

        this.CCNumber.Attributes.Add("onkeydown", "javascript:captureKeyPress(event);");
		this.CCNumber.Attributes.Add("onkeyup", javascript);//IE7/8 does not support oninput

        SetCardTypeImageOpacity();

        if (!PageUtility.IsAsyncPostBackForControl(this, ConfigurationProvider.DefaultProvider.ScriptManagerId))
        {
            PanelChooseWalletOrNewCard.Visible = false;

            AspDotNetStorefrontGateways.Processors.AuthorizeNet authorizeNet = new AspDotNetStorefrontGateways.Processors.AuthorizeNet();
            if (authorizeNet.IsCimEnabled)
            {
                if (this.Model.AccountModel.IsRegistered &&
                    GatewayAuthorizeNet.DataUtility.GetPaymentProfiles(Int32.Parse(this.Model.AccountModel.AccountId), this.Model.AccountModel.Email).Any())
                {
                    PanelChooseWalletOrNewCard.Visible = true;

                    PanelWallet.Visible = WalletSelected;
                    PanelCreditDetails.Visible = !WalletSelected;

                    RadioButtonNewCard.Checked = !WalletSelected;
                    RadioButtonWallet.Checked = WalletSelected;
                }
                else
                {
                    PanelCreditDetails.Visible = true;
                    RadioButtonNewCard.Checked = true;
                    PanelSelectWallet.Visible = false;
                    RadioButtonWallet.Checked = false;
                }
            }
        }
	}

	public override void Disable()
	{
		if (this.CCPaymentMethodModel == null)
			return;

		this.CCNameOnCard.Enabled = false;
		this.CCExpiresMonth.Enabled = false;
		this.CCExpiresYear.Enabled = false;
		this.CCNumber.Enabled = false;
		this.CCSecurityCode.Enabled = false;
        this.CCIssueNumber.Enabled = false;
		this.ButtonSaveCreditCardPaymentForm.Enabled = false;
        this.ButtonSaveCreditCardPaymentForm.Visible = false;
        this.UpdatePanelCreditCard.Update();
	}

	public override void Enable()
	{
		if (this.CCPaymentMethodModel == null)
			return;
        
        this.CCNameOnCard.Enabled = true;
		this.CCExpiresMonth.Enabled = true;
		this.CCExpiresYear.Enabled = true;
		this.CCNumber.Enabled = true;
		this.CCSecurityCode.Enabled = true;
		this.CCIssueNumber.Enabled = true;
		this.ButtonSaveCreditCardPaymentForm.Enabled = !this.Model.HavePaymentData;
		this.ButtonSaveCreditCardPaymentForm.Visible = !this.Model.HavePaymentData;
		this.UpdatePanelCreditCard.Update();
	}

	public override void Show()
	{
		this.Visible = true;
		this.UpdatePanelCreditCard.Visible = true;
		this.UpdatePanelCreditCard.Update();
	}

	public override void Hide()
	{
		this.Visible = false;
		this.UpdatePanelCreditCard.Visible = false;
		this.UpdatePanelCreditCard.Update();
	}

    public override void ShowError(string message)
    {
        PanelError.Visible = true;
        ErrorMessage.Text = message;
    }
        
    public override void BindView()
	{
		if (this.CCPaymentMethodModel == null)
			return;

		this.CCNameOnCard.Text = this.CCPaymentMethodModel.NameOnCard;
		this.CCNumber.Text = this.CCPaymentMethodModel.CCNumber;
		this.CCSecurityCode.Text = this.CCPaymentMethodModel.CVV2;
        this.CCIssueNumber.Text = this.CCPaymentMethodModel.CCIssueNumber;
        this.SetSelectedMonth();
        this.SetSelectedYear();
		this.HFCreditCardType.Value = this.CCPaymentMethodModel.CreditCardType != null ? this.CCPaymentMethodModel.CreditCardType.StoredCardType : string.Empty;
        this.SetCardTypeImageOpacity();

		IEnumerable<CreditCardType> cardTypes = this.CCPaymentMethodModel.AllowedCardTypes;

		ImageCardTypeVisa.Visible = cardTypes.Any(f => f.CardType == CreditCardTypeEnum.Visa);
        ImageCardTypeMastercard.Visible = cardTypes.Any(f => f.CardType == CreditCardTypeEnum.MasterCard);
        ImageCardTypeAmex.Visible = cardTypes.Any(f => f.CardType == CreditCardTypeEnum.AmericanExpress);
        ImageCardTypeDiscover.Visible = cardTypes.Any(f => f.CardType == CreditCardTypeEnum.Discover);
        ImageCardTypeSolo.Visible = cardTypes.Any(f => f.CardType == CreditCardTypeEnum.Solo);
        ImageCardTypeMaestro.Visible = cardTypes.Any(f => f.CardType == CreditCardTypeEnum.Maestro);

		AspDotNetStorefrontGateways.Processors.AuthorizeNet authorizeNet = new AspDotNetStorefrontGateways.Processors.AuthorizeNet();
		RowSaveToWallet.Visible = authorizeNet.IsCimEnabled;

        if (cardTypes.Any(f => f.CardType == CreditCardTypeEnum.Solo) || cardTypes.Any(f => f.CardType == CreditCardTypeEnum.Maestro))
            this.CCIssueNumberRow.Visible = true;
        else
            this.CCIssueNumberRow.Visible = false;

        //Lets always enable the next button unless we have just pushed it.
        ButtonSaveCreditCardPaymentForm.Visible = !Model.HavePaymentData;

        RequiredFieldValidatorCCSecurityCode.Visible = !AspDotNetStorefrontCore.AppLogic.AppConfigBool("CardExtraCodeIsOptional");
        RequiredFieldValidatorCCSecurityCode.Enabled = !AspDotNetStorefrontCore.AppLogic.AppConfigBool("CardExtraCodeIsOptional");

        if (cardTypes.Any(f => f.CardType == CreditCardTypeEnum.Unknown))
        {
            PHCreditCardTypeDropDown.Visible = true;
            PHCreditCardTypeImages.Visible = false;

            var customer = ((AspDotNetStorefront.SkinBase)Page).OPCCustomer;
            DDLCreditCardType.DataSource = AspDotNetStorefrontCore.CardTypeDataSource.GetAcceptedCreditCardTypes(customer).Select(f => f.ToUpper());
            DDLCreditCardType.DataBind();

            Vortx.OnePageCheckout.WebUtility.PageUtility.SetSelectedValue(DDLCreditCardType, this.CCPaymentMethodModel.CreditCardType.StoredCardType);
        }
        else
        {
            PHCreditCardTypeDropDown.Visible = false;
            PHCreditCardTypeImages.Visible = true;
        }
    }

	public override void BindView(object id)
	{

	}

    public CreditCardType GetCardType(string cardTypeString)
    {
        var cardType = CreditCardTypeEnum.Unknown;
        try
        {
            cardType = (CreditCardTypeEnum)
                Enum.Parse(typeof(CreditCardTypeEnum), cardTypeString, true);
        }
        catch (ArgumentException)
        {
        }

        return new CreditCardType(cardType, cardTypeString);
    }

    public CreditCardType GetSelectedCardTypeFromForm()
    {
        if (this.CCPaymentMethodModel.AllowedCardTypes.Any(c => c.CardType == CreditCardTypeEnum.Unknown))
        {
            return GetCardType(DDLCreditCardType.SelectedValue);
        }
        else
        {
            if (!string.IsNullOrEmpty(HFCreditCardType.Value))
            {
                return GetCardType(HFCreditCardType.Value);
            }
            return null;
        }
    }

	public override void SaveViewToModel()
	{
		Page.Validate("VGCreditCardPayment");

		if (!Page.IsValid)
		{
			// find the validator that failed, and if it's the CCNumber, then nuke the stored CCNumber
			foreach (BaseValidator validator in Page.Validators)
			{
				if (!validator.IsValid && (validator.ControlToValidate == CCNumber.ID))
				{
					this.CCPaymentMethodModel.CCNumber = string.Empty;
					this.CCPaymentMethodModel.Save();
				}
			}			
			return;
		}

        CCNumber.Text = CCNumber.Text.Replace(" ", "").Replace("-", "");

		AspDotNetStorefrontGateways.Processors.AuthorizeNet authorizeNet = new AspDotNetStorefrontGateways.Processors.AuthorizeNet();
		if(authorizeNet.IsCimEnabled)
		{
			var customer = AspDotNetStorefrontCore.Customer.Current;
			if(CBSaveToAccount.Checked && paymentProfileId < 1)
			{
				string errorMsg, errorCode;

				string cardNumberToUse;
				if(CCNumber.Text.StartsWith("****"))
					cardNumberToUse = customer.PrimaryBillingAddress.CardNumber;
				else
					cardNumberToUse = CCNumber.Text;

                //set session var so that we don't try to create a new card multiple times
                paymentProfileId = GatewayAuthorizeNet.ProcessTools.SaveProfileAndPaymentProfile(customer.CustomerID, customer.EMail,
						AspDotNetStorefrontCore.AppLogic.AppConfig("StoreName"), paymentProfileId, customer.PrimaryBillingAddressID,
						cardNumberToUse, this.CCSecurityCode.Text, CCExpiresMonth.SelectedValue, CCExpiresYear.SelectedValue, out errorMsg, out errorCode);
				if(paymentProfileId <= 0 && errorCode != "E00039")
				{
					ShowError(errorMsg);
					return;
				}
			}
			if(!string.IsNullOrEmpty(customer.ThisCustomerSession["ActivePaymentProfileId"]))
			{
				long profileId = long.Parse(customer.ThisCustomerSession["ActivePaymentProfileId"]);
				var profile = GatewayAuthorizeNet.DataUtility.GetPaymentProfileWrapper(customer.CustomerID, customer.EMail, profileId);

				string profileCardType = profile.CardType == "AMEX" ? "AmericanExpress" : profile.CardType;

				this.CCPaymentMethodModel.CCMonth = profile.ExpirationMonth;
				this.CCPaymentMethodModel.CCYear = profile.ExpirationYear;
				this.CCPaymentMethodModel.CCNumber = profile.CreditCardNumberMasked;
				this.CCPaymentMethodModel.CreditCardType = GetCardType(profileCardType);

				this.CCPaymentMethodModel.Save();

				return;
			}
		}

		this.CCPaymentMethodModel.NameOnCard = this.CCNameOnCard.Text;
		this.CCPaymentMethodModel.CCMonth = this.CCExpiresMonth.SelectedValue;
		this.CCPaymentMethodModel.CCYear = this.CCExpiresYear.SelectedValue;
		this.CCPaymentMethodModel.CCIssueNumber = this.CCIssueNumber.Text;

		// Don't save out the number again if it has been hidden
		if(!this.CCNumber.Text.StartsWith("****"))
		{
			this.CCPaymentMethodModel.CCNumber = this.CCNumber.Text;
		}
		this.CCPaymentMethodModel.CVV2 = this.CCSecurityCode.Text;

        var selectedCardType = GetSelectedCardTypeFromForm();
        if (selectedCardType != null)
        {
            this.CCPaymentMethodModel.CreditCardType = selectedCardType;
        }

		this.CCPaymentMethodModel.Save();
	}

	#endregion    

    public void FirePaymentFormSubmitted()
    {
        if (PaymentFormSubmitted != null)
            PaymentFormSubmitted(this, null);
    }

    public event EventHandler PaymentFormSubmitted;
}
