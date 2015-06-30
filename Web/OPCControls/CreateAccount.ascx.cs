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
using Vortx.OnePageCheckout.Settings;

public partial class VortxControls_CreateAccount : 
    OPCUserControl<IAccountModel>,
    IAccountView   
{
	private string _AccountCreatedDisplayStringResource = null;


	public string AccountCreatedDisplayStringResource
	{
		get { return _AccountCreatedDisplayStringResource ?? "smartcheckout.aspx.126"; }
		set { _AccountCreatedDisplayStringResource = value; }
	}


    protected void RadioCreateAccount_CheckedChanged(object sender, EventArgs e)
    {
        this.PanelCreateAccount.Visible = RadioCreateAccountYes.Checked;
        this.RadioCreateAccountYes.Enabled = true;

        if (ConfigurationProvider.DefaultProvider.PasswordIsOptionalDuringCheckout)
            this.RadioCreateAccountNo.Enabled = true;
        else
        {
            this.RadioCreateAccountYes.Enabled = false;
            this.RadioCreateAccountNo.Enabled = false;
            this.RadioCreateAccountNo.Visible = false;
            this.RadioCreateAccountYes.Visible = false;
            LabelOptional.Visible = false;
        }

        this.CreateAccountPassword.Enabled = true;
        this.CreateAccountPasswordConfirm.Enabled = true;

		if (RadioCreateAccountNo.Checked)
			this.Model.RegisterAccountSelected = true;

		this.Model.RegisterAccount = RadioCreateAccountYes.Checked;
		this.Model.Save();
    }

    protected void CreateAccountButton_Click(object sender, EventArgs e)
    {
        SaveViewToModel();
    }

	protected void CustomValidatorCreateAccount_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = RadioCreateAccountYes.Checked || RadioCreateAccountNo.Checked;		
	}

    public Boolean IsRecurringOrder { get; set; }

	public override void BindView()
	{
		if (this.Model.RegisterAccountSelected)
		{
			this.PanelCreateAccount.Visible = this.Model.RegisterAccount;
			this.CreateAccountPassword.Enabled = this.Model.RegisterAccount;
			this.CreateAccountPasswordConfirm.Enabled = this.Model.RegisterAccount;

			this.RadioCreateAccountYes.Checked = this.Model.RegisterAccount;
			this.RadioCreateAccountNo.Checked = !this.Model.RegisterAccount;
            
			if (!string.IsNullOrEmpty(this.Model.Password))
			{
				this.CreateAccountPassword.Attributes.Add("value", this.Model.Password);
				this.CreateAccountPasswordConfirm.Attributes.Add("value", this.Model.Password);
			}

            if (this.Model.IsRegistered)
            {
                this.PanelCreateAccount.Visible = true;
                this.RadioCreateAccountYes.Enabled = false;
                this.CreateAccountPassword.Enabled = false;
                this.CreateAccountPasswordConfirm.Enabled = false;
                this.CreateAccountButton.Enabled = false;

                this.CreateAccountPassword.Text = "********";
                this.CreateAccountPassword.TextMode = TextBoxMode.SingleLine;
                this.CreateAccountPasswordConfirm.Text = "********";
                this.CreateAccountPasswordConfirm.TextMode = TextBoxMode.SingleLine;

                ShowError(StringResourceProvider.GetString("smartcheckout.aspx.126"));
            }
		}
	}

	public override void BindView(object identifier)
	{
	}

	public override void SaveViewToModel()
	{
		Page.Validate("VGCreateAccount");
        if (Page.IsValid)
        {
            if (!this.Model.IsRegistered)
            {
                if (!ConfigurationProvider.DefaultProvider.ShowEmailPreferencesOnCheckout)
                {
                    this.Model.AllowEmail = false;
                }
                this.Model.FirstName = this.Model.BillingAddress.FirstName;
                this.Model.LastName = this.Model.BillingAddress.LastName;
                this.Model.Phone = this.Model.ShippingAddress.Phone;

                if (this.RadioCreateAccountYes.Checked) 
                {
                    if (string.IsNullOrEmpty(this.Model.Password))
                    {
                        this.Model.Password = this.CreateAccountPassword.Text;
                    }
                    this.Model.Register();
                }
                else
                {
                    this.Model.Save();
                }
            }
        }
	}

    public override void ShowError(string message)
    {
        PanelError.Visible = true;
        LabelError.Text = message;
    }

    public override void Initialize()
    {
        this.RadioCreateAccountYes.Enabled = true;
        this.RadioCreateAccountNo.Enabled = false;
        this.RadioCreateAccountYes.Checked = false;
        this.RadioCreateAccountNo.Checked = false;
        this.PanelCreateAccount.Visible = false;

        if (!ConfigurationProvider.DefaultProvider.PasswordIsOptionalDuringCheckout || IsRecurringOrder)
        {
            this.RadioCreateAccountNo.Visible = false;
            this.RadioCreateAccountYes.Visible = false;
            this.RadioCreateAccountYes.Checked = true;
            this.Model.RegisterAccount = true;
        }

        LabelOptional.Visible = (!IsRecurringOrder && ConfigurationProvider.DefaultProvider.PasswordIsOptionalDuringCheckout);
	}

    public override void Disable()
    {
        this.RadioCreateAccountYes.Enabled = false;
		this.RadioCreateAccountNo.Enabled = false;
        this.CreateAccountPassword.Enabled = false;
        this.CreateAccountPasswordConfirm.Enabled = false;
    }

    public override void Enable()
    {
        this.RadioCreateAccountYes.Enabled = true;

        if (ConfigurationProvider.DefaultProvider.PasswordIsOptionalDuringCheckout)
        {
            this.RadioCreateAccountNo.Enabled = true;
			this.CreateAccountPassword.Enabled = true;
			this.CreateAccountPasswordConfirm.Enabled = true;
        }
        else
        {
            this.RadioCreateAccountYes.Enabled = false;
            this.RadioCreateAccountNo.Enabled = false;
            this.RadioCreateAccountNo.Visible = false;
            this.RadioCreateAccountYes.Visible = false;
            this.LabelOptional.Visible = false;
            this.CreateAccountPassword.Enabled = true;
            this.CreateAccountPasswordConfirm.Enabled = true;
		
            this.PanelCreateAccount.Visible = true;
        }
	}

    public override void Show()
    {
        this.Visible = true;
        this.UpdatePanelCreateAccount.Visible = true;
        this.UpdatePanelCreateAccount.Update();
    }

    public override void Hide()
    {
        this.Visible = false;
        this.UpdatePanelCreateAccount.Visible = false;
        this.UpdatePanelCreateAccount.Update();
    }
}
