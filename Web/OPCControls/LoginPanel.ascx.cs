// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore.Validation;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.Models.PaymentMethods;
using Vortx.OnePageCheckout.Settings;
using Vortx.OnePageCheckout.UI;
using Vortx.OnePageCheckout.Views;
using Vortx.OnePageCheckout.WebUtility;

public partial class LoginPanel :
    OPCUserControl<IAccountModel>,
    IAccountView
{
	#region Event Handlers

	protected void ButtonCreateNewPassword_Click(object sender, EventArgs e)
    {
        Page.Validate("VGCreateNewPassword");
        if (Page.IsValid)
        {
            this.Model.ChangePassword(TextBoxNewPassword1.Text);
        }
    }

    protected void btnEmailSubmit_Click(object sender, EventArgs e)
    {
        Page.Validate("VGAccount");
        if (Page.IsValid)
        {
            this.Model.Username = txtEmailAddress.Text.Trim();
			this.Model.HaveConfirmationEmail = txtConfirmEmailAddress.Text.Trim() == txtEmailAddress.Text.Trim();
            this.Model.FindAccount();
        }
    }

    protected void btnPasswordSubmit_Click(object sender, EventArgs e)
    {
        Page.Validate("VGAccount");
        if (Page.IsValid)
        {
            this.Model.Username = txtEmailAddress.Text;
            this.Model.Password = txtPassword.Text;

            this.Model.LogOn();
        }
    }

    protected void btnSkipLogin_Click(object sender, EventArgs e)
    {
        this.Model.Username = txtEmailAddress.Text;

        if (SkipLogin != null)
            SkipLogin(this, new EventArgs());
    }

    protected void ButtonForgotPassword_Click(object sender, EventArgs e)
    {
        this.PanelError.Visible = false;
        this.Model.SendLostPassword(txtEmailAddress.Text);
    }

    protected void linkSwitchUser_Click(object sender, EventArgs e)
    {
        this.Model.LogOut();
        Response.Redirect("~/smartcheckout.aspx?checkout=true");
    }

	#endregion

	#region ILoginView Members

	public event EventHandler SkipLogin;

	public IPaymentModel PaymentModel { get; set; }

    public override void SetModel(IAccountModel model, IStringResourceProvider stringResourceProvider)
    {
        base.SetModel(model, stringResourceProvider);

        this.Model.LostPasswordSent += new LostPasswordSentHandler(AccountModel_LostPasswordSent);
        this.Model.PasswordChanged += new PasswordChangedHandler(AccountModel_PasswordChanged);
    }

    protected void AccountModel_PasswordChanged(object sender, PasswordChangedEventArgs args)
    {
        if (args.Successful)
        {
            this.ShowAuthenticated();
            LabelCreateNewPasswordResults.Visible = false;
            PanelCreateNewPasswordResults.Visible = false;
        }
        else
        {
            PanelCreateNewPasswordResults.Visible = true;
            LabelCreateNewPasswordResults.Visible = true;
            LabelCreateNewPasswordResults.Text = args.Message;
        }

        this.UpdatePanelLogin.Update();
    }

    protected void AccountModel_LostPasswordSent(object sender, LostPasswordSentEventArgs args)
    {
        this.LostPasswordResults.Visible = true;
        this.LabelForgotPasswordResults.Text = args.Message;
        this.LabelForgotPasswordResults.Visible = true;
        this.UpdatePanelLogin.Update();
    }

    public override void Initialize()
    {
        PanelUsername.Visible = true;
        PanelPassword.Visible = false;
        PanelError.Visible = false;
        PanelNoAccount.Visible = false;

        txtEmailAddress.Visible = true;
        txtEmailAddress.Enabled = true;
        txtEmailAddress.Text = String.Empty;

        PanelCreateNewPassword.Visible = false;

        txtPassword.Visible = false;
        txtPassword.Enabled = false;
        PageUtility.AddClass(txtPassword, "disabled");
        PageUtility.RemoveClass(txtEmailAddress, "disabled");

        txtPassword.Text = String.Empty;

		trConfirmEmail.Visible = false;

        btnEmailSubmit.Visible = true;
        btnEmailSubmit.Enabled = true;

        btnPasswordSubmit.Visible = false;
        btnPasswordSubmit.Enabled = false;

        ButtonForgotPassword.Visible = false;
        ButtonForgotPassword.Enabled = false;

        LostPasswordResults.Visible = false;
        LabelForgotPasswordResults.Visible = false;

        linkSwitchUser.Visible = false;
        linkSwitchUser.Enabled = false;

        btnSkipLogin.Visible = false;
        btnSkipLogin.Enabled = false;

        EmailHelperText.Visible = true;

        if (!ConfigurationProvider.DefaultProvider.PasswordIsOptionalDuringCheckout)
        {
            lblNoAccount.Text = StringResourceProvider.GetString("smartcheckout.aspx.127");
        }
    }

    public override void Enable()
    {
    }

    public override void Disable()
    {
    }

    public override void Show()
    {
    }

    public override void Hide()
    {
    }

    public override void BindView()
    {
        if (this.Model.IsRegistered)
        {
            if (this.Model.PasswordChangeRequired)
            {
                this.ShowCreateNewPassword();
            }
            else
            {
                this.ShowAuthenticated();
            }
        }
        else if (this.Model.AccountFound)
        {
            this.ShowAccountFound();
        }
		else if (this.Model.HaveUsername && !this.Model.HaveConfirmationEmail && ConfigurationProvider.DefaultProvider.ReEnterEmail)
		{
			this.ShowReEnterEmail();
		}
        else if (this.Model.HaveUsername)
        {
            this.ShowAccountNotFound();
        }
        else
        {
            this.Initialize();
        }
    }

    public override void BindView(object identifier)
    {
        bool authenticated = (bool)identifier;
        if (authenticated)
        {
            if (this.Model.PasswordChangeRequired)
            {
                this.ShowCreateNewPassword();
            }
            else
            {
                this.ShowAuthenticated();
            }
        }
        else
        {
            this.ShowNotAuthenticated();
        }
    }

    public override void SaveViewToModel()
    {
    }
	
    public override void ShowError(string message)
    {
        this.PanelError.Visible = true;
        this.lblError.Visible = true;
        this.lblError.Text = message;
        this.PanelNoAccount.Visible = false;
    }
	
    #endregion

	#region Public Methods

	public void ShowCreateNewPassword()
	{
		PanelUsername.Visible = true;
		PanelPassword.Visible = true;
		PanelPassword.Enabled = false;

		PanelError.Visible = false;

		PanelCreateNewPassword.Visible = true;
		LabelCreateNewPasswordFirstName.Text = this.Model.FirstName;

		PanelNoAccount.Visible = false;

		txtEmailAddress.Visible = true;
		txtEmailAddress.Enabled = false;
		PageUtility.AddClass(txtEmailAddress, "disabled");

		btnEmailSubmit.Visible = false;
		btnEmailSubmit.Enabled = false;

		btnPasswordSubmit.Visible = false;
		btnPasswordSubmit.Enabled = true;

		ButtonForgotPassword.Visible = true;
		ButtonForgotPassword.Enabled = true;

		LabelForgotPasswordResults.Visible = false;
		LostPasswordResults.Visible = false;

		if (PaymentModel.ActivePaymentMethod != null &&
			PaymentModel.ActivePaymentMethod.PaymentType == PaymentType.PayPalExpress &&
			((PaypalExpressPaymentModel)this.PaymentModel.ActivePaymentMethod).ExpressLoginComplete)
		{
			linkSwitchUser.Visible = false;
			this.PanelNoAccount.Visible = false;
		}
		else
		{
			linkSwitchUser.Visible = true;
			linkSwitchUser.Enabled = true;
		}

		trConfirmEmail.Visible = false;

		txtPassword.Visible = true;
		txtPassword.Enabled = false;
		PageUtility.AddClass(txtPassword, "disabled");

		txtEmailAddress.Text = this.Model.Username;
		txtPassword.Text = "********";
		txtPassword.TextMode = TextBoxMode.SingleLine;
		EmailHelperText.Visible = false;
	}

	public void ShowAccountFound()
	{
		PanelUsername.Visible = true;
		PanelPassword.Visible = true;
		PanelError.Visible = false;

		PanelNoAccount.Visible = false;

		PanelCreateNewPassword.Visible = false;

		txtEmailAddress.Visible = true;
		txtEmailAddress.Enabled = false;
		PageUtility.AddClass(txtEmailAddress, "disabled");

		btnEmailSubmit.Visible = false;
		btnEmailSubmit.Enabled = false;

		btnPasswordSubmit.Visible = true;
		btnPasswordSubmit.Enabled = true;

		ButtonForgotPassword.Visible = true;
		ButtonForgotPassword.Enabled = true;

		LabelForgotPasswordResults.Visible = false;
		LostPasswordResults.Visible = false;

		if (PaymentModel.ActivePaymentMethod != null &&
			PaymentModel.ActivePaymentMethod.PaymentType == PaymentType.PayPalExpress &&
			((PaypalExpressPaymentModel)this.PaymentModel.ActivePaymentMethod).ExpressLoginComplete)
		{
			linkSwitchUser.Visible = false;
			this.PanelNoAccount.Visible = false;
		}
		else
		{
			linkSwitchUser.Visible = true;
			linkSwitchUser.Enabled = true;
		}

		trConfirmEmail.Visible = false;

		txtPassword.Visible = true;
		txtPassword.Enabled = true;
		txtPassword.TextMode = TextBoxMode.Password;
		PageUtility.RemoveClass(txtPassword, "disabled");

		txtEmailAddress.Text = this.Model.Username;
		EmailHelperText.Visible = false;

		btnSkipLogin.Visible = AspDotNetStorefrontCore.Customer.NewEmailPassesDuplicationRules(this.Model.Username, int.Parse(this.Model.AccountId), !this.Model.IsRegistered);
		btnSkipLogin.Enabled = AspDotNetStorefrontCore.Customer.NewEmailPassesDuplicationRules(this.Model.Username, int.Parse(this.Model.AccountId), !this.Model.IsRegistered);
	}

	public void ShowReEnterEmail()
	{
		PanelUsername.Visible = true;
		PanelPassword.Visible = false;

		PanelError.Visible = false;
		PanelNoAccount.Visible = true;

		PanelCreateNewPassword.Visible = false;

		txtEmailAddress.Visible = true;
		txtEmailAddress.Enabled = false;
		PageUtility.AddClass(txtEmailAddress, "disabled");

		trConfirmEmail.Visible = true;

		btnEmailSubmit.Visible = true;
		btnEmailSubmit.Enabled = true;

		btnPasswordSubmit.Visible = false;
		btnPasswordSubmit.Enabled = false;

		ButtonForgotPassword.Visible = false;
		ButtonForgotPassword.Enabled = false;

		LabelForgotPasswordResults.Visible = false;
		LostPasswordResults.Visible = false;

		if (PaymentModel.ActivePaymentMethod != null &&
			PaymentModel.ActivePaymentMethod.PaymentType == PaymentType.PayPalExpress &&
			 ((PaypalExpressPaymentModel)this.PaymentModel.ActivePaymentMethod).ExpressLoginComplete)
		{
			linkSwitchUser.Visible = false;
			this.PanelNoAccount.Visible = false;
		}
		else
		{
			PanelNoAccount.Visible = true;
			linkSwitchUser.Visible = true;
			linkSwitchUser.Enabled = true;
		}

		txtPassword.Visible = false;
		txtPassword.Enabled = false;
		PageUtility.AddClass(txtPassword, "disabled");

		txtEmailAddress.Text = this.Model.Username;
		EmailHelperText.Visible = false;
	}
	
	#endregion

	#region Private Methods

	void ShowAccountNotFound()
    {
        PanelUsername.Visible = true;
        PanelPassword.Visible = false;

        PanelError.Visible = false;
        PanelNoAccount.Visible = true;

        PanelCreateNewPassword.Visible = false;

        txtEmailAddress.Visible = true;
        txtEmailAddress.Enabled = false;
        PageUtility.AddClass(txtEmailAddress, "disabled");

		txtConfirmEmailAddress.Visible = true;
		txtConfirmEmailAddress.Enabled = false;
		PageUtility.AddClass(txtConfirmEmailAddress, "disabled");

        btnEmailSubmit.Visible = false;
        btnEmailSubmit.Enabled = false;

        btnPasswordSubmit.Visible = false;
        btnPasswordSubmit.Enabled = false;

        ButtonForgotPassword.Visible = false;
        ButtonForgotPassword.Enabled = false;

        LabelForgotPasswordResults.Visible = false;
        LostPasswordResults.Visible = false;

        if (PaymentModel.ActivePaymentMethod != null &&
            PaymentModel.ActivePaymentMethod.PaymentType == PaymentType.PayPalExpress &&
             ((PaypalExpressPaymentModel)this.PaymentModel.ActivePaymentMethod).ExpressLoginComplete)
        {
            linkSwitchUser.Visible = false;
            this.PanelNoAccount.Visible = false;
        }
        else
        {
            PanelNoAccount.Visible = true;
            linkSwitchUser.Visible = true;
            linkSwitchUser.Enabled = true;
        }

        txtPassword.Visible = false;
        txtPassword.Enabled = false;
        PageUtility.AddClass(txtPassword, "disabled");

        txtEmailAddress.Text = this.Model.Username;
        EmailHelperText.Visible = false;
    }

    void ShowNotAuthenticated()
    {
        PanelUsername.Visible = true;
        PanelPassword.Visible = true;

        PanelError.Visible = true;
        PanelNoAccount.Visible = false;

        PanelCreateNewPassword.Visible = false;

        txtEmailAddress.Visible = true;
        txtEmailAddress.Enabled = false;
        PageUtility.AddClass(txtEmailAddress, "disabled");

        btnEmailSubmit.Visible = false;
        btnEmailSubmit.Enabled = false;

        btnPasswordSubmit.Visible = true;
        btnPasswordSubmit.Enabled = true;

        ButtonForgotPassword.Visible = true;
        ButtonForgotPassword.Enabled = true;

        LabelForgotPasswordResults.Visible = false;
        LostPasswordResults.Visible = false;

        if (PaymentModel.ActivePaymentMethod != null &&
            PaymentModel.ActivePaymentMethod.PaymentType == PaymentType.PayPalExpress &&
             ((PaypalExpressPaymentModel)this.PaymentModel.ActivePaymentMethod).ExpressLoginComplete)
        {
            linkSwitchUser.Visible = false;
        }
        else
        {
            linkSwitchUser.Visible = true;
            linkSwitchUser.Enabled = true;
        }

        txtPassword.Visible = true;
        txtPassword.Enabled = true;
        txtPassword.TextMode = TextBoxMode.Password;
        PageUtility.RemoveClass(txtPassword, "disabled");

        txtEmailAddress.Text = this.Model.Username;
        lblError.Text = StringResourceProvider.GetString("smartcheckout.aspx.128");
        EmailHelperText.Visible = false;
    }

    void ShowAuthenticated()
    {
        PanelUsername.Visible = true;
        PanelPassword.Visible = true;

        PanelError.Visible = false;
        PanelNoAccount.Visible = false;

        txtEmailAddress.Visible = true;
        txtEmailAddress.Enabled = false;
        PageUtility.AddClass(txtEmailAddress, "disabled");

        trConfirmEmail.Visible = false;

        btnEmailSubmit.Visible = false;
        btnEmailSubmit.Enabled = false;

        PanelCreateNewPassword.Visible = false;

        btnPasswordSubmit.Visible = false;
        btnPasswordSubmit.Enabled = false;

        ButtonForgotPassword.Visible = false;
        ButtonForgotPassword.Enabled = false;

        LabelForgotPasswordResults.Visible = false;
        LostPasswordResults.Visible = false;

        linkSwitchUser.Visible = true;
        linkSwitchUser.Enabled = true;

        txtPassword.Visible = false;
        txtPassword.Enabled = false;
        lblPassword.Visible = false;
        PageUtility.AddClass(txtPassword, "disabled");

        txtEmailAddress.Text = this.Model.Username;
        txtPassword.Text = "********";
        txtPassword.TextMode = TextBoxMode.SingleLine;
        EmailHelperText.Visible = false;
    }

    #endregion
}
