// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront
{
    public partial class editaddress : SkinBase
    {

        bool Checkout = false;
        int AddressID = 0;
        AddressTypes AddressType = AddressTypes.Unknown;
        bool CanDelete = false;
        Address theAddress = new Address();
        readonly bool ValidateAddress = (AppLogic.AppConfig("VerifyAddressesProvider").Length > 0);
        string Prompt = string.Empty;

        private readonly bool ShowEcheck =
            AppLogic.AppConfig("PaymentMethods").ToUpperInvariant().Contains(AppLogic.ro_PMECheck);

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            RequireSecurePage();

            Checkout = CommonLogic.QueryStringBool("checkout");
            
            AddressTypes ats = DetermineAddressType(CommonLogic.QueryStringCanBeDangerousContent("AddressType").ToLowerInvariant());
            String AddressTypeString = ats.ToString();

            AddressID = CommonLogic.QueryStringUSInt("AddressID");
            theAddress.LoadFromDB(AddressID);
            Prompt = CommonLogic.QueryStringCanBeDangerousContent("Prompt");
            if (CommonLogic.QueryStringCanBeDangerousContent("RETURNURL") != "")
            {
                ViewState["RETURNURL"] = CommonLogic.QueryStringCanBeDangerousContent("RETURNURL");
            }

            if (!ThisCustomer.OwnsThisAddress(AddressID))
            {
                Response.Redirect("default.aspx");
            }

            AppLogic.CheckForScriptTag(AddressTypeString);

            SectionTitle = "<a href=\"selectaddress.aspx?checkout=" + Checkout.ToString() + "&AddressType=" + AddressTypeString + "\">" + String.Format(AppLogic.GetString("selectaddress.aspx.1", SkinID, ThisCustomer.LocaleSetting), AddressTypeString) + "</a> &rarr; ";
            SectionTitle += AppLogic.GetString("editaddress.aspx.1", SkinID, ThisCustomer.LocaleSetting);

            AddressType = DetermineAddressType(AddressTypeString);
            CanDelete = (0 == DB.GetSqlN(String.Format("select count(0) as N from ShoppingCart   with (NOLOCK)  where (ShippingAddressID={0} or BillingAddressID={0}) and CartType={1}", AddressID, (int)CartTypeEnum.RecurringCart)));

            if (!IsPostBack)
            {
                InitializePageContent();
            }
        }

        private AddressTypes DetermineAddressType(string addressTypeString)
        {
            AddressTypes type = AddressTypes.Billing;
            string compareString = addressTypeString.ToString();

            if (compareString.Equals(AddressTypes.Billing.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                type = AddressTypes.Billing;
            }
            else if (compareString.Equals(AddressTypes.Shipping.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                type = AddressTypes.Shipping;
            }
            else if (compareString.Equals(AddressTypes.Account.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                type = AddressTypes.Account;
            }
            else if (compareString.Equals(AddressTypes.Unknown.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                type = AddressTypes.Unknown;
            }
            else
            {
                // should default it to something...
                type = AddressTypes.Billing;
            }

            return type;
        }

        public void btnValidateAddress_Click(object sender, EventArgs e)
        {
            ProcessForm(true, Convert.ToInt32(((Button)sender).CommandArgument));
        }

        public void btnSaveAddress_Click(object sender, EventArgs e)
        {
            ProcessForm(false, Convert.ToInt32(((Button)sender).CommandArgument));
        }

        private void ProcessForm(bool UseValidationService, int AddressID)
        {
            ThisCustomer.RequireCustomerRecord();
            string ResidenceType = ddlResidenceType.SelectedValue;
            bool valid = true;
            string errormsg = string.Empty;

            // Payment method validations
            if (AddressType == AddressTypes.Billing)
            {
                string paymentMethodLastUsed = AppLogic.CleanPaymentMethod(CommonLogic.FormCanBeDangerousContent("PaymentMethod"));
                if (paymentMethodLastUsed == AppLogic.ro_PMECheck && ShowEcheck)
                {
                    if (string.IsNullOrEmpty(CommonLogic.FormCanBeDangerousContent("ECheckBankABACode")))
                    {
                        valid = false;
                        errormsg += "&bull;Bank ABA Code is required";
                    }
                    if (string.IsNullOrEmpty(CommonLogic.FormCanBeDangerousContent("ECheckBankAccountNumber")))
                    {
                        valid = false;
                        errormsg += "&bull;Bank Account Number is required";
                    }
                    if (string.IsNullOrEmpty(CommonLogic.FormCanBeDangerousContent("ECheckBankName")))
                    {
                        valid = false;
                        errormsg += "&bull;Bank Account Name is required";
                    }
                    if (string.IsNullOrEmpty(CommonLogic.FormCanBeDangerousContent("ECheckBankAccountName")))
                    {
                        valid = false;
                        errormsg += "&bull;Bank Account Name is required";
                    }
                }
                if (paymentMethodLastUsed == AppLogic.ro_PMCreditCard)
                {
                    if (string.IsNullOrEmpty(CommonLogic.FormCanBeDangerousContent("CardName")))
                    {
                        valid = false;
                        errormsg += "&bull;Card Name is required";
                    }
                    if (string.IsNullOrEmpty(CommonLogic.FormCanBeDangerousContent("CardType")))
                    {
                        valid = false;
                        errormsg += "&bull;Card Type is required";
                    }
                    if (string.IsNullOrEmpty(CommonLogic.FormCanBeDangerousContent("CardNumber")))
                    {
                        valid = false;
                        errormsg += "&bull;Card Number is required";
                    }

                    int iexpMonth = 0;
                    int iexpYear = 0;
                    string expMonth = CommonLogic.FormCanBeDangerousContent("CardExpirationMonth");
                    string expYear = CommonLogic.FormCanBeDangerousContent("CardExpirationYear");

                    if (string.IsNullOrEmpty(expMonth) ||
                        !int.TryParse(expMonth, out iexpMonth) ||
                        !(iexpMonth > 0))
                    {
                        valid = false;
                        errormsg += "&bull;Please select the Card Expiration Month";
                    }
                    if (string.IsNullOrEmpty(expYear) ||
                        !int.TryParse(expYear, out iexpYear) ||
                        !(iexpYear > 0))
                    {
                        valid = false;
                        errormsg += "&bull;Please select the Card Expiration Year";
                    }
                }
            }

            if (!Page.IsValid || !valid)
            {
                ErrorMsgLabel.Text = "" + AppLogic.GetString("editaddress.aspx.15", SkinID, ThisCustomer.LocaleSetting) + "";
                foreach (IValidator aValidator in this.Validators)
                {
                    if (!aValidator.IsValid)
                    {
                        ErrorMsgLabel.Text += "&bull; " + aValidator.ErrorMessage + "";
                    }
                }
                ErrorMsgLabel.Text += "";
                ErrorMsgLabel.Text += errormsg;
                InitializePageContent();
                return;
            }

            theAddress.AddressType = AddressType;
            theAddress.NickName = txtAddressNickName.Text;
            theAddress.FirstName = txtFirstName.Text;
            theAddress.LastName = txtLastName.Text;
            theAddress.Company = txtCompany.Text;
            theAddress.Address1 = txtAddress1.Text;
            theAddress.Address2 = txtAddress2.Text;
            theAddress.Suite = txtSuite.Text;
            theAddress.City = txtCity.Text;
            theAddress.State = ddlState.SelectedValue;
            theAddress.Zip = txtZip.Text;
            theAddress.Country = ddlCountry.SelectedValue;
            theAddress.Phone = txtPhone.Text;
            if (ResidenceType == "2")
            {
                theAddress.ResidenceType = ResidenceTypes.Commercial;
            }
            else if (ResidenceType == "1")
            {
                theAddress.ResidenceType = ResidenceTypes.Residential;
            }
            else
            {
                theAddress.ResidenceType = ResidenceTypes.Unknown;
            }
            if (theAddress.AddressType == AddressTypes.Billing)
            {
                theAddress.PaymentMethodLastUsed = AppLogic.CleanPaymentMethod(CommonLogic.FormCanBeDangerousContent("PaymentMethod"));
                if (theAddress.PaymentMethodLastUsed == AppLogic.ro_PMECheck && ShowEcheck)
                {
                    string eCheckABACode = CommonLogic.FormCanBeDangerousContent("ECheckBankABACode");
                    if (!eCheckABACode.StartsWith("*"))
                    {
                        theAddress.ECheckBankABACode = CommonLogic.FormCanBeDangerousContent("ECheckBankABACode");
                    }

                    string eCheckBankAccountNumber = CommonLogic.FormCanBeDangerousContent("ECheckBankAccountNumber");
                    if (!eCheckBankAccountNumber.StartsWith("*"))
                    {
                        theAddress.ECheckBankAccountNumber = CommonLogic.FormCanBeDangerousContent("ECheckBankAccountNumber");
                    }

                    theAddress.ECheckBankName = CommonLogic.FormCanBeDangerousContent("ECheckBankName");
                    theAddress.ECheckBankAccountName = CommonLogic.FormCanBeDangerousContent("ECheckBankAccountName");
                    theAddress.ECheckBankAccountType = CommonLogic.FormCanBeDangerousContent("ECheckBankAccountType");
                }
                if (theAddress.PaymentMethodLastUsed == AppLogic.ro_PMCreditCard)
                {
                    theAddress.CardName = CommonLogic.FormCanBeDangerousContent("CardName");
                    theAddress.CardType = CommonLogic.FormCanBeDangerousContent("CardType");

                    string tmpS = CommonLogic.FormCanBeDangerousContent("CardNumber");
                    if (!tmpS.StartsWith("*"))
                    {
                        theAddress.CardNumber = tmpS;
                    }
                    theAddress.CardExpirationMonth = CommonLogic.FormCanBeDangerousContent("CardExpirationMonth");
                    theAddress.CardExpirationYear = CommonLogic.FormCanBeDangerousContent("CardExpirationYear");
                }
            }
            theAddress.UpdateDB();

            string RETURNURL = "";
            if (ViewState["RETURNURL"] != null)
            {
                RETURNURL = "&ReturnUrl=" + ViewState["RETURNURL"].ToString();
            }
            if (UseValidationService)
            {

                Address StandardizedAddress = new Address();
                String validateResult = AddressValidation.RunValidate(theAddress, out StandardizedAddress);
                theAddress = StandardizedAddress;
                theAddress.UpdateDB();

                if (validateResult != AppLogic.ro_OK)
                {
                    validateResult = "address.validation.errormsg".StringResource() + validateResult;
                    Session["ErrorMsgLabelText"] = System.Web.HttpUtility.HtmlEncode(validateResult);
                    Response.Redirect("editaddress.aspx?Checkout=" + Checkout.ToString() + "&AddressType=" + AddressType.ToString() + "&AddressID=" + AddressID.ToString() + RETURNURL);
                }
            }

            Response.Redirect(String.Format("selectaddress.aspx?Checkout={0}&AddressType={1}" + RETURNURL, Checkout.ToString(), AddressType));
        }

        public void btnDeleteAddress_Click(object sender, EventArgs e)
        {
            int DeleteAddressID = Convert.ToInt32(((Button)sender).CommandArgument);
            if (DeleteAddressID != 0)
            {
                Address adr = new Address();
                adr.LoadFromDB(DeleteAddressID);
                // make sure ok to delete:
                if (ThisCustomer.CustomerID == adr.CustomerID || ThisCustomer.IsAdminUser)
                {
                    Address.DeleteFromDB(DeleteAddressID, ThisCustomer.CustomerID);
                }
                Response.Redirect(String.Format("selectaddress.aspx?Checkout={0}&AddressType={1}", Checkout.ToString(), AddressType));
            }
        }

        public void ddlCountry_OnChange(object sender, EventArgs e)
        {
            SetStateList(ddlCountry.SelectedValue);
        }

        private void InitializePageContent()
        {
            lblError.Visible = (lblError.Text.Trim() != "");
            valAddressIsPOBox.ErrorMessage = AppLogic.GetString("createaccount_process.aspx.3", SkinID, ThisCustomer.LocaleSetting);
            tblAddressList.Attributes.Add("style", "border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor"));
            tblAddressListBox.Attributes.Add("style", AppLogic.AppConfig("BoxFrameStyle"));
            editaddress_gif.ImageUrl = AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/editaddress.gif");
            litAddressPrompt.Text = CommonLogic.IIF(AddressType == AddressTypes.Shipping, AppLogic.GetString("editaddress.aspx.2", SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("editaddress.aspx.12", SkinID, ThisCustomer.LocaleSetting));
            if (Prompt.Length > 0)
            {
                litAddressPrompt.Text += "<strong><font color=\"red\">" + Prompt + "</font></strong>";
            }
            bool CustCCRequired = ThisCustomer.MasterShouldWeStoreCreditCardInfo;
            pnlBillingData.Visible = (AddressType == AddressTypes.Billing && CustCCRequired);
            editaddress_aspx_4.Text = AppLogic.GetString("editaddress.aspx.4", SkinID, ThisCustomer.LocaleSetting);
            editaddress_aspx_5.Text = AppLogic.GetString("editaddress.aspx.5", SkinID, ThisCustomer.LocaleSetting);
            editaddress_aspx_6.Text = AppLogic.GetString("editaddress.aspx.6", SkinID, ThisCustomer.LocaleSetting);
            editaddress_aspx_7.Text = AppLogic.GetString("editaddress.aspx.7", SkinID, ThisCustomer.LocaleSetting);
            litCCForm.Text = theAddress.InputCardHTML(ThisCustomer, false, false);
            editaddress_aspx_8.Text = AppLogic.GetString("editaddress.aspx.8", SkinID, ThisCustomer.LocaleSetting);
            litECheckForm.Text = theAddress.InputECheckHTML(false);
            btnSaveAddress.Text = AppLogic.GetString("editaddress.aspx.9", SkinID, ThisCustomer.LocaleSetting);
            btnSaveAddress.CommandArgument = AddressID.ToString();
            btnDeleteAddress.Visible = CanDelete;
            btnDeleteAddress.CommandArgument = AddressID.ToString();
            btnDeleteAddress.Text = AppLogic.GetString("editaddress.aspx.10", SkinID, ThisCustomer.LocaleSetting);
            pnlEcheckData.Attributes.Add("style", "display:none;");
            pnlCCData.Attributes.Add("style", "display:none;");
            btnValidateAddress.Text = AppLogic.GetString("editaddress.aspx.14", SkinID, ThisCustomer.LocaleSetting);
            btnValidateAddress.CommandArgument = AddressID.ToString();
            btnValidateAddress.Visible = ValidateAddress;
            lblValidateAddressSpacer.Visible = ValidateAddress;

            if (!IsPostBack)
            {
                if (CustCCRequired)
                {
                    CreditCard.Checked = true;
                }

                if (!ShowEcheck)
                {
                    editaddress_aspx_6.Visible = false;
                    ECheck.Visible = false;
                }
            }

            if (CreditCard.Checked || ECheck.Checked)
            {
                if (CreditCard.Checked)
                {
                    CreditCard.Checked = true;
                    pnlCCData.Attributes.Add("style", "display:block;");
                    pnlEcheckData.Attributes.Add("style", "display:none;");
                }
                else if (ECheck.Checked)
                {
                    ECheck.Checked = true;
                    pnlCCData.Attributes.Add("style", "display:none;");
                    pnlEcheckData.Attributes.Add("style", "display:block;");
                }
            }
            else
            {
                if (theAddress.PaymentMethodLastUsed == AppLogic.ro_PMCreditCard)
                {
                    CreditCard.Checked = true;
                    pnlCCData.Attributes.Add("style", "display:block;");
                }
                else if (theAddress.PaymentMethodLastUsed == AppLogic.ro_PMECheck && ShowEcheck)
                {
                    ECheck.Checked = true;
                    pnlEcheckData.Attributes.Add("style", "display:block;");
                }
            }

            txtAddressNickName.Text = theAddress.NickName;
            txtFirstName.Text = theAddress.FirstName;
            txtLastName.Text = theAddress.LastName;
            txtPhone.Text = theAddress.Phone;
            txtCompany.Text = theAddress.Company;
            ddlResidenceType.Items.Add(new ListItem(AppLogic.GetString("address.cs.55", SkinID, ThisCustomer.LocaleSetting), ((int)ResidenceTypes.Unknown).ToString()));
            ddlResidenceType.Items.Add(new ListItem(AppLogic.GetString("address.cs.56", SkinID, ThisCustomer.LocaleSetting), ((int)ResidenceTypes.Residential).ToString()));
            ddlResidenceType.Items.Add(new ListItem(AppLogic.GetString("address.cs.57", SkinID, ThisCustomer.LocaleSetting), ((int)ResidenceTypes.Commercial).ToString()));
            ddlResidenceType.SelectedValue = ((int)theAddress.ResidenceType).ToString();
            txtAddress1.Text = theAddress.Address1;
            txtAddress2.Text = theAddress.Address2;
            txtSuite.Text = theAddress.Suite;
            txtCity.Text = theAddress.City;
            txtZip.Text = theAddress.Zip;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader dr = DB.GetRS("select * from Country   with (NOLOCK)  where Published = 1 order by DisplayOrder,Name", conn))
                {
                    ddlCountry.DataSource = dr;
                    ddlCountry.DataTextField = "Name";
                    ddlCountry.DataValueField = "Name";
                    ddlCountry.DataBind();
                }
            }

            ddlCountry.SelectedValue = theAddress.Country;
            SetStateList(theAddress.Country);
            if (ddlState.Items.FindByValue(theAddress.State) != null)
            {
                ddlState.SelectedValue = theAddress.State;
            }

            GetJS();
        }

        private void GetJS()
        {
            StringBuilder js = new StringBuilder("<script type=\"text/javascript\">");
            js.Append("function ShowPaymentInput(theRadio)");
            js.Append("{");
            js.Append("if (theRadio.value == 'none')");
            js.Append("{");
            js.Append("document.getElementById('" + pnlEcheckData.ClientID + "').style.display = 'none';");
            js.Append("document.getElementById('" + pnlCCData.ClientID + "').style.display = 'none';");
            js.Append("}");
            js.Append("else if (theRadio.value == 'ECheck')");
            js.Append("{");
            js.Append("document.getElementById('" + pnlEcheckData.ClientID + "').style.display = '';");
            js.Append("document.getElementById('" + pnlCCData.ClientID + "').style.display = 'none';");
            js.Append("}");
            js.Append("else");
            js.Append("{");
            js.Append("document.getElementById('" + pnlEcheckData.ClientID + "').style.display = 'none';");
            js.Append("document.getElementById('" + pnlCCData.ClientID + "').style.display = '';");
            js.Append("}");
            js.Append("return true;");
            js.Append("}");
            js.Append("</script> ");
            ClientScriptManager cs = this.ClientScript;
            cs.RegisterClientScriptBlock(this.GetType(), Guid.NewGuid().ToString(), js.ToString());
        }

        private void SetStateList(string Country)
        {
            string sql = String.Empty;
            if (Country.Length > 0)
            {
                sql = "select s.* from dbo.State s  with (NOLOCK)  join dbo.country c on s.countryid = c.countryid where c.name = " + DB.SQuote(Country) + " order by s.DisplayOrder,s.Name";
            }

            ddlState.ClearSelection();

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader dr = DB.GetRS(sql, conn))
                {
                    ddlState.DataSource = dr;
                    ddlState.DataTextField = "Name";
                    ddlState.DataValueField = "Abbreviation";
                    ddlState.DataBind();
                }
            }

            if (ddlState.Items.Count == 0)
            {
                using (SqlConnection conn2 = DB.dbConn())
                {
                    conn2.Open();
                    using (IDataReader dr2 = DB.GetRS("select * from State  with (NOLOCK)  where countryid is null", conn2))
                    {
                        if (dr2.Read())
                        {
                            ddlState.Items.Insert(0, new ListItem(DB.RSField(dr2, "Name"), DB.RSField(dr2, "Abbreviation")));
                        }
                        else
                        {
                            ddlState.Items.Insert(0, new ListItem("state.countrywithoutstates".StringResource(), "--"));
                        }
                    }
                }
                ddlState.SelectedIndex = 0;
            }
        }

        protected override string OverrideTemplate()
        {
            if (CommonLogic.QueryStringBool("recurringedit"))
            {
                return "empty.ascx";
            }

            return base.OverrideTemplate();
        }

        public void ValidateAddress1(object source, ServerValidateEventArgs args)
        {
            String Adr1 = txtAddress1.Text;
            Adr1 = Adr1.Replace(" ", "").Trim().Replace(".", "");
			bool IsPOBoxAddress = !(new POBoxAddressValidator()).IsValid(Adr1);
            bool RejectDueToPOBoxAddress = (IsPOBoxAddress && AppLogic.AppConfigBool("DisallowShippingToPOBoxes")); // undocumented feature 
            args.IsValid = !RejectDueToPOBoxAddress;
        }

    }
}
