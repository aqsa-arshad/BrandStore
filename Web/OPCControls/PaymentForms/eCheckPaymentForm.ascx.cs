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

[PaymentType(Vortx.OnePageCheckout.Models.PaymentType.ECheck)]
public partial class OPCControls_eCheckPaymentForm :
    OPCUserControl<PaymentMethodBaseModel>,
    IPaymentMethodView
{
    public ECheckPaymentModel ECheckPaymentMethodModel { get; private set; }

    public override void SetModel(PaymentMethodBaseModel model, IStringResourceProvider stringResourceProvider)
    {
        base.SetModel(model, stringResourceProvider);

        this.ECheckPaymentMethodModel = (ECheckPaymentModel)model;
    }

    protected void BtnSaveECheckPaymentForm_Click(object sender, EventArgs e)
    {
        FirePaymentFormSubmitted();
    }

	public override void BindView()
	{
        var skinId = ((AspDotNetStorefront.SkinBase)Page).OPCCustomer.SkinID;
        ImgECheckBankABAImage1.ImageUrl = AspDotNetStorefrontCore.AppLogic.LocateImageURL(string.Format("~/App_Themes/skin_{0}/images/check_aba.gif", skinId));
        ImgECheckBankABAImage2.ImageUrl = AspDotNetStorefrontCore.AppLogic.LocateImageURL(string.Format("~/App_Themes/skin_{0}/images/check_aba.gif", skinId));
        ImgECheckBankAccountImage.ImageUrl = AspDotNetStorefrontCore.AppLogic.LocateImageURL(string.Format("~/App_Themes/skin_{0}/images/check_account.gif", skinId));

        LabelNotes.Text = string.Format(this.StringResourceProvider.GetString("address.cs.48"),
            AspDotNetStorefrontCore.AppLogic.LocateImageURL(string.Format("~/App_Themes/skin_{0}/images/check_micr.gif", skinId)));

        AccountNumber.Text = ECheckPaymentMethodModel.AccountNumber;
        BankABA.Text = ECheckPaymentMethodModel.BankABANumber;
        BankName.Text = ECheckPaymentMethodModel.BankName;
        NameOnAccount.Text = ECheckPaymentMethodModel.NameOnAccount;

        Vortx.OnePageCheckout.WebUtility.PageUtility.SetSelectedValue(DDLAccountType, ECheckPaymentMethodModel.AccountType);
	}

	public override void BindView(object identifier)
	{
	}

	public override void SaveViewToModel()
	{
        Page.Validate("VGECheckPayment");
        if (Page.IsValid)
        {
            ECheckPaymentMethodModel.AccountNumber = AccountNumber.Text;
            ECheckPaymentMethodModel.AccountType = DDLAccountType.SelectedValue;
            ECheckPaymentMethodModel.BankABANumber = BankABA.Text;
            ECheckPaymentMethodModel.BankName = BankName.Text;
            ECheckPaymentMethodModel.NameOnAccount = NameOnAccount.Text;
            ECheckPaymentMethodModel.Save();
        }
	}

    public override void Initialize()
    {
        NameOnAccount.Text = string.Empty;
        AccountNumber.Text = string.Empty;
        BankABA.Text = string.Empty;
        BankName.Text = string.Empty;
    }

    public override void Disable()
    {
        NameOnAccount.Enabled = false;
        AccountNumber.Enabled = false;
        BankABA.Enabled = false;
        BankName.Enabled = false;
    }

    public override void Enable()
    {
        NameOnAccount.Enabled = true;
        AccountNumber.Enabled = true;
        BankABA.Enabled = true;
        BankName.Enabled = true;
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
