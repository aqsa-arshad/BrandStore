// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;
using Vortx.MobileFramework;

namespace AspDotNetStorefront
{
    public partial class _mobileaddress : SkinBase
    {
        private String AddressTypeString = String.Empty;
        private const String BILLING = "BILLING";
        private const String SHIPPING = "SHIPPING";
        private readonly bool ShowEcheck = AppLogic.AppConfig("PaymentMethods").ToUpperInvariant().Contains(AppLogic.ro_PMECheck);

        private AddressTypes AddressMode
        {
            get
            {
                // default to shipping address
                AddressTypes addressType = AddressTypes.Shipping;

                string addressTypeQueryString = CommonLogic.QueryStringCanBeDangerousContent("AddressType");
                if (!CommonLogic.IsStringNullOrEmpty(addressTypeQueryString))
                {
                    if (addressTypeQueryString.Trim().ToUpper() == BILLING ||
                        addressTypeQueryString.Trim().ToUpper() == SHIPPING)
                    {
                        addressType = (AddressTypes)Enum.Parse(typeof(AddressTypes), addressTypeQueryString, true);
                    }
                }

                return addressType;
            }
        }

        private Boolean CustomerCCRequired
        {
            get
            {
                return ThisCustomer.MasterShouldWeStoreCreditCardInfo && AppLogic.AppConfigBool("StoreCCInDB");
            }
        }

        protected override void OnInit(EventArgs e)
        {
            MobileHelper.RedirectPageWhenMobileIsDisabled("~/address.aspx", ThisCustomer);

            if (ThisCustomer.IsRegistered == false)
            {
                Response.Redirect("~/signin.aspx");
            }
            InitializePageContent();
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                String sessionErrorMsg = Session["ErrorMsgLabelText"] != null ? Session["ErrorMsgLabelText"].ToString() : String.Empty; //already encoded
                String addressErrorPrompt = CommonLogic.QueryStringCanBeDangerousContent("prompt");
                if (addressErrorPrompt.Length > 0 || sessionErrorMsg.Length > 0)
                {
                    ErrorMsgLabel.Visible = true;
                    ErrorMsgLabel.Text = String.IsNullOrEmpty(addressErrorPrompt) ? sessionErrorMsg : addressErrorPrompt;
                    Session["ErrorMsgLabelText"] = String.Empty;
                }
            }
            else ErrorMsgLabel.Visible = false;
        }

        private void InitializePageContent()
        {
            //Gets the datasource for the datalist control
            if (dlAddress != null)
            {
                if (!this.IsPostBack)
                {
                    LoadData();
                }
            }
           

            if (ctrlNewAddress != null)
            {
                CountryDropDownData(ctrlNewAddress);
                StateDropDownData(ctrlNewAddress, ThisCustomer.LocaleSetting);
            }

			if(CommonLogic.QueryStringCanBeDangerousContent("Checkout").EqualsIgnoreCase(Boolean.TrueString))
				btnReturnUrl.Text = "account.aspx.60".StringResource();
        }

        private void LoadData()
        {
            dlAddress.DataSource = GetAddresses();
            dlAddress.DataBind();
        }

        private Addresses GetAddresses()
        {
            Addresses allAddresses = new Addresses();
            allAddresses.LoadCustomer(ThisCustomer.CustomerID);

            return allAddresses;
        }

        protected void dlAddress_EditCommand(object sender, DataListCommandEventArgs e)
        {
            int editIndex = e.Item.ItemIndex;

            DataList dlAddress = pnlContent.FindControl("dlAddress") as DataList;

            if (dlAddress != null)
            {
                dlAddress.EditItemIndex = editIndex;
                LoadData();

                //hide the add new address panel if visible
                pnlNewAddress.Visible = false;
                dlAddress.ShowFooter = !pnlNewAddress.Visible;
            }
        }

        protected void dlAddress_UpdateCommand(object sender, DataListCommandEventArgs e)
        {
            CreditCardPanel ctrlCreditCard = e.Item.FindControl("ctrlCreditCard") as CreditCardPanel;
            Panel pnlCCData = e.Item.FindControl("pnlCCData") as Panel;
            Panel pnlECData = e.Item.FindControl("pnlECData") as Panel;

            AddressControl ctrlAddress = e.Item.FindControl("ctrlAddress") as AddressControl;
            if (ctrlAddress != null)
            {
                ctrlAddress.CountryIDToValidateZipCode = AppLogic.GetCountryID(ctrlAddress.Country);
            }

            Page.Validate("EditAddress");

            if (AddressMode == AddressTypes.Billing && pnlCCData.Visible)
            {               
                if (ctrlCreditCard.CreditCardType == AppLogic.GetString("address.cs.32", SkinID, ThisCustomer.LocaleSetting))
                {
                    pnlCCTypeErrorMsg.Visible = true;
                }
                else { pnlCCTypeErrorMsg.Visible = false; }
                if (ctrlCreditCard.CardExpMonth == AppLogic.GetString("address.cs.34", SkinID, ThisCustomer.LocaleSetting))
                {
                    pnlCCExpMonthErrorMsg.Visible = true;
                }
                else { pnlCCExpMonthErrorMsg.Visible = false; }
                if (ctrlCreditCard.CardExpYr == AppLogic.GetString("address.cs.35", 1, ThisCustomer.LocaleSetting))
                {
                    pnlCCExpYrErrorMsg.Visible = true;
                }
                else { pnlCCExpYrErrorMsg.Visible = false; }

                CardType Type = CardType.Parse(ctrlCreditCard.CreditCardType);
                CreditCardValidator validator = new CreditCardValidator(ctrlCreditCard.CreditCardNumber, Type);
                bool isValid = validator.Validate();

                if (!isValid && AppLogic.AppConfigBool("ValidateCreditCardNumbers"))
                {
                    ctrlCreditCard.CreditCardNumber = string.Empty;
                    // clear the card extra code
                    AppLogic.StoreCardExtraCodeInSession(ThisCustomer, string.Empty);
                    pnlCCNumberErrorMsg.Visible = true;
                }
                else { pnlCCNumberErrorMsg.Visible = false; }
            }

            bool isValidCCDropdown = !(pnlCCTypeErrorMsg.Visible || pnlCCExpMonthErrorMsg.Visible ||
                    pnlCCExpYrErrorMsg.Visible || pnlCCNumberErrorMsg.Visible);

            if (dlAddress != null && Page.IsValid && isValidCCDropdown)
            {
                AspDotNetStorefrontCore.Address anyAddress = new AspDotNetStorefrontCore.Address();
                Echeck ctrlECheck = e.Item.FindControl("ctrlECheck") as Echeck;

                if (ctrlAddress != null)
                {
                    anyAddress.AddressID = int.Parse((e.Item.FindControl("hfAddressID") as HiddenField).Value);
                    anyAddress.CustomerID = ThisCustomer.CustomerID;
                    anyAddress.NickName = ctrlAddress.NickName;
                    anyAddress.FirstName = ctrlAddress.FirstName;
                    anyAddress.LastName = ctrlAddress.LastName;
                    anyAddress.Phone = ctrlAddress.PhoneNumber;
                    anyAddress.Company = ctrlAddress.Company;
                    anyAddress.AddressType = AddressMode;
                    anyAddress.ResidenceType = (ResidenceTypes)Enum.Parse(typeof(ResidenceTypes),ctrlAddress.ResidenceType, true);
                    anyAddress.Address1 = ctrlAddress.Address1;
                    anyAddress.Address2 = ctrlAddress.Address2;
                    anyAddress.City = ctrlAddress.City;
                    anyAddress.Suite = ctrlAddress.Suite;
                    anyAddress.Zip = ctrlAddress.ZipCode;
                    anyAddress.Country = ctrlAddress.Country;
                    anyAddress.State = ctrlAddress.State;

                    if (CustomerCCRequired && AddressMode == AddressTypes.Billing)
                    {
                        Address BillingAddress = new Address();
                        BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);

                        if (ctrlCreditCard != null)
                        {
                            anyAddress.CardName = ctrlCreditCard.CreditCardName;

                            if (!ctrlCreditCard.CreditCardNumber.StartsWith("*"))
                            {
                                anyAddress.CardNumber = ctrlCreditCard.CreditCardNumber;
                            }
                            else
                            {
                                anyAddress.CardNumber = BillingAddress.CardNumber;
                            }

                            anyAddress.CardType = ctrlCreditCard.CreditCardType;
                            anyAddress.CardExpirationMonth = ctrlCreditCard.CardExpMonth;
                            anyAddress.CardExpirationYear = ctrlCreditCard.CardExpYr;

                            if (AppLogic.AppConfigBool("ShowCardStartDateFields"))
                            {
                                string cardStartDate = "";
                                if (ctrlCreditCard.CardExpMonth != AppLogic.GetString("address.cs.34", SkinID, ThisCustomer.LocaleSetting))
                                {
                                    cardStartDate = ctrlCreditCard.CardStartMonth;
                                }
                                if (ctrlCreditCard.CardExpYr != AppLogic.GetString("address.cs.35", SkinID, ThisCustomer.LocaleSetting))
                                {
                                    cardStartDate += ctrlCreditCard.CardStartYear;
                                }
                                anyAddress.CardStartDate = cardStartDate;
                            }
                            if (AppLogic.AppConfigBool("CardExtraCodeIsOptional"))
                            {
                                anyAddress.CardIssueNumber = ctrlCreditCard.CreditCardIssueNumber;
                            }
                        }

                        if (ShowEcheck && ctrlECheck != null)
                        {
                            anyAddress.ECheckBankAccountName = ctrlECheck.ECheckBankAccountName;
                            anyAddress.ECheckBankName = ctrlECheck.ECheckBankName;

                            if (!ctrlECheck.ECheckBankABACode.StartsWith("*"))
                            {
                                anyAddress.ECheckBankABACode = ctrlECheck.ECheckBankABACode;
                            }
                            else
                            {
                                anyAddress.ECheckBankABACode = BillingAddress.ECheckBankABACode;
                            }

                            if (!ctrlECheck.ECheckBankAccountNumber.StartsWith("*"))
                            {
                                anyAddress.ECheckBankAccountNumber = ctrlECheck.ECheckBankAccountNumber;
                            }
                            else
                            {
                                anyAddress.ECheckBankAccountNumber = BillingAddress.ECheckBankAccountNumber;
                            }

                            anyAddress.ECheckBankAccountType = ctrlECheck.ECheckBankAccountType;
                        }

                        if (pnlCCData.Visible)
                        {
                            anyAddress.PaymentMethodLastUsed = AppLogic.ro_PMCreditCard;
                        }
                        else if (pnlECData.Visible)
                        {
                            anyAddress.PaymentMethodLastUsed = AppLogic.ro_PMECheck;
                        }
                        else
                        {
                            anyAddress.PaymentMethodLastUsed = BillingAddress.PaymentMethodLastUsed;
                        }
                    }

                    anyAddress.UpdateDB();

                    if (AppLogic.AppConfig("VerifyAddressesProvider").Length > 0)
                    {
                        AspDotNetStorefrontCore.Address standardizedAddress = new AspDotNetStorefrontCore.Address();
                        string validateResult = AddressValidation.RunValidate(anyAddress, out standardizedAddress);

                        if (validateResult != AppLogic.ro_OK)
                        {
                            var err = new CustomValidator();
                            err.ValidationGroup = "EditAddress";
                            err.IsValid = false;
                            err.ErrorMessage = "address.validation.errormsg".StringResource() + validateResult;
                            Page.Validators.Add(err);
                        }
                        else
                        {
                            anyAddress = standardizedAddress;
                            anyAddress.UpdateDB();
                        }
                    }

                    dlAddress.EditItemIndex = -1;
                    LoadData();
                }
            }
        }

        protected void dlAddress_DeleteCommand(object sender, DataListCommandEventArgs e)
        {
            int addressID = 0;
            HiddenField hfAddressID = e.Item.FindControl("hfAddressID") as HiddenField;
            if (hfAddressID != null && Int32.TryParse(hfAddressID.Value, out addressID))
            {
                AspDotNetStorefrontCore.Address anyAddress = new AspDotNetStorefrontCore.Address();

                anyAddress.LoadFromDB(addressID);

                if (ThisCustomer.CustomerID == anyAddress.CustomerID || ThisCustomer.IsAdminSuperUser)
                {
                    AspDotNetStorefrontCore.Address.DeleteFromDB(anyAddress.AddressID, ThisCustomer.CustomerID);
                }
            }
            dlAddress.EditItemIndex = -1;
            LoadData();

        }

        protected void dlAddress_CancelCommand(object sender, DataListCommandEventArgs e)
        {
            pnlCCTypeErrorMsg.Visible = false;
            pnlCCExpMonthErrorMsg.Visible = false;
            pnlCCExpYrErrorMsg.Visible = false;
            dlAddress.EditItemIndex = -1;
            LoadData();
        }

        protected void dlAddress_ItemCommand(object sender, DataListCommandEventArgs e)
        {
            if (e.CommandArgument.Equals("MakePrimaryAddress"))
            {
                int addressID = 0;
                StringBuilder sSql = new StringBuilder(5000);
                string sColumn = string.Empty;
                if (Int32.TryParse((e.Item.FindControl("hfAddressID") as HiddenField).Value, out addressID))
                {
                    
                    if (AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo"))
                    {
                        sColumn = CommonLogic.IIF(AddressMode == AddressTypes.Billing || AddressMode == AddressTypes.Unknown, "BillingAddressID", "ShippingAddressID");
                        sSql.AppendFormat("UPDATE Customer SET {0}={1} WHERE CustomerID={2}", sColumn, addressID, ThisCustomer.CustomerID);
                        if (sColumn.ContainsIgnoreCase("BillingAddressID"))
                        {
                            sSql.AppendFormat("UPDATE ShoppingCart SET BillingAddressID = {0} WHERE CustomerID={1} AND CartType IN (0,1)", addressID, ThisCustomer.CustomerID);
                        }
                        if (!AppLogic.AppConfigBool("AllowMultipleShippingAddressPerOrder") && sColumn.ContainsIgnoreCase("ShippingAddressID"))
                        {
                            sSql.AppendFormat("UPDATE ShoppingCart SET ShippingAddressID = {0} WHERE CustomerID={1} AND CartType IN (0,1)", addressID, ThisCustomer.CustomerID);
                        }
                    }
                    else
                    {
                        sSql.AppendFormat("UPDATE Customer SET BillingAddressID={0}, ShippingAddressID={1} WHERE CustomerID={2}", addressID, addressID, ThisCustomer.CustomerID);
                        sSql.AppendFormat("UPDATE ShoppingCart SET ShippingAddressID = {0}, BillingAddressID = {0} WHERE CustomerID = {1} AND CartType IN (0,1)", addressID, ThisCustomer.CustomerID);
                    }

                    using (SqlConnection conn = DB.dbConn())
                    {
                        try
                        {
                            conn.Open();
                            DB.ExecuteSQL(sSql.ToString(), conn);
                        }
                        finally
                        {
                            conn.Close();
                            conn.Dispose();
                        }
                    }

                    LoadData();
                }
            }
        }

        protected void dlAddress_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.EditItem)
            {
                AddressControl ctrlAddress = e.Item.FindControl("ctrlAddress") as AddressControl;
                CreditCardPanel ctrlCreditCard = e.Item.FindControl("ctrlCreditCard") as CreditCardPanel;
                Echeck ctrlECheck = e.Item.FindControl("ctrlECheck") as Echeck;
                PopulateAddressControlValues(ctrlAddress, ctrlCreditCard, ctrlECheck, e.Item.ItemIndex);

                if (CustomerCCRequired)
                {
                    TableRow trCCInformation = e.Item.FindControl("trCCInformation") as TableRow;
                    if (trCCInformation != null)
                    {
                        if (AddressMode == AddressTypes.Billing)
                        {
                            RadioButtonList rblPaymentMethodInfo = e.Item.FindControl("rblPaymentMethodInfo") as RadioButtonList;
                            Panel pnlCCData = e.Item.FindControl("pnlCCData") as Panel;

                            if (rblPaymentMethodInfo.SelectedValue.Equals(AppLogic.ro_PMCreditCard,
                                StringComparison.InvariantCultureIgnoreCase))
                            {
                                trCCInformation.Visible = true;
                                rblPaymentMethodInfo.Items[0].Enabled = true;
                                pnlCCData.Visible = true;
                            }
                            if (!ShowEcheck)
                            {
                                rblPaymentMethodInfo.Items.Remove(rblPaymentMethodInfo.Items[1]);
                            }

                            //Image for eCheck
                            if (ShowEcheck && ctrlECheck != null)
                            {
                                ctrlECheck = e.Item.FindControl("ctrlECheck") as Echeck;
                                ctrlECheck.ECheckBankABAImage1 = AppLogic.LocateImageURL(String.Format("~/App_Themes/skin_{0}/images/check_aba.gif", SkinID.ToString()));
                                ctrlECheck.ECheckBankABAImage2 = AppLogic.LocateImageURL(String.Format("~/App_Themes/skin_{0}/images/check_aba.gif", SkinID.ToString()));
                                ctrlECheck.ECheckBankAccountImage = AppLogic.LocateImageURL(String.Format("~/App_Themes/skin_{0}/images/check_account.gif", SkinID.ToString()));
                                ctrlECheck.ECheckNoteLabel = string.Format(AppLogic.GetString("address.cs.48", SkinID, ThisCustomer.LocaleSetting), AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/check_micr.gif"));
                            }

                            //hide payment methods if storeccindb = false                                
                        }
                        else if (AddressMode == AddressTypes.Shipping)
                        {
                            trCCInformation.Visible = false;
                        }
                    }
                }
            }

            if (e.Item.ItemType == ListItemType.Footer)
            {
                //LinkButton lbAddNewAddress = e.Item.FindControl("lbAddNewAddress") as LinkButton;
                Button ibAddNewAddress = e.Item.FindControl("ibAddNewAddress") as Button;

                if (ibAddNewAddress != null)
                {
                    if (AddressMode == AddressTypes.Billing)
                    {
                        string billingText = AppLogic.GetString("address.cs.70", SkinID, ThisCustomer.LocaleSetting);
                        if (ibAddNewAddress != null)
                        {
                            ibAddNewAddress.Text = billingText;
                        }
                    }
                    else if (AddressMode == AddressTypes.Shipping)
                    {
                        string shippingText = AppLogic.GetString("address.cs.71", SkinID, ThisCustomer.LocaleSetting);
                        if (ibAddNewAddress != null)
                        {
                            ibAddNewAddress.Text = shippingText;
                        }
                    }
                }
            }

            if ((e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem))
            {
                //Assign numbering for individual address
                //(e.Item.FindControl("lblIndexOrder") as Label).Text = String.Format("{0}.", (e.Item.ItemIndex + 1).ToString());
                int itemAddressID = Int32.Parse((e.Item.FindControl("hfAddressID") as HiddenField).Value);
                int primaryID = 0;

                Button ibDelete = e.Item.FindControl("ibDelete") as Button;
                Button ibEdit = e.Item.FindControl("ibEdit") as Button;

                DisableEditButtonsForAddressWithOpenOrder(ibDelete, ibEdit, itemAddressID);


                Button ibMakePrimaryAddress = e.Item.FindControl("ibMakePrimary") as Button;
                //Check if the address mode from the querystring to know what will be the primary address
                if (AddressMode == AddressTypes.Billing)
                {
                    primaryID = AppLogic.GetPrimaryBillingAddressID(ThisCustomer.CustomerID);
                    ibMakePrimaryAddress.ToolTip = AppLogic.GetString("account.aspx.87", SkinID, ThisCustomer.LocaleSetting);
                    //ibMakePrimaryAddress.ImageUrl = String.Format("~/App_Themes/Skin_{0}/images/icons/check_disabled.png", SkinID);
                }
                else if (AddressMode == AddressTypes.Shipping)
                {
                    primaryID = AppLogic.GetPrimaryShippingAddressID(ThisCustomer.CustomerID);
                    ibMakePrimaryAddress.ToolTip = AppLogic.GetString("account.aspx.88", SkinID, ThisCustomer.LocaleSetting);
                    //ibMakePrimaryAddress.ImageUrl = String.Format("~/App_Themes/Skin_{0}/images/icons/check_disabled.png", SkinID);
                }

                if (itemAddressID == primaryID)
                {
                    Label AddressHTML = e.Item.FindControl("lblAddressHTML") as Label;

                    //Display the last payment method used
                    if (CustomerCCRequired && AddressMode == AddressTypes.Billing)
                    {
                        string paymentMethodDisplay = DisplayPaymentMethod(primaryID);
                        if (!CommonLogic.IsStringNullOrEmpty(paymentMethodDisplay))
                        {
                            AddressHTML.Text += paymentMethodDisplay;
                        }
                    }

                    AddressHTML.Style["font-weight"] = "bold";
                    if (AddressMode == AddressTypes.Billing)
                    {
                        ibMakePrimaryAddress.ToolTip = AppLogic.GetString("account.aspx.89", SkinID, ThisCustomer.LocaleSetting);
                    }
                    else if (AddressMode == AddressTypes.Shipping)
                    {
                        ibMakePrimaryAddress.ToolTip = AppLogic.GetString("account.aspx.90", SkinID, ThisCustomer.LocaleSetting);
                    }
                    ibMakePrimaryAddress.Visible = false;
                }

                //shows the footer where you can click add
                dlAddress.ShowFooter = !pnlNewAddress.Visible;
            }
        }

        private String DisplayPaymentMethod(int primaryID)
        {
            Address primaryAddress = new Address();
            primaryAddress.LoadFromDB(primaryID);
            String pmCleaned = AppLogic.CleanPaymentMethod(primaryAddress.PaymentMethodLastUsed);
            String paymentMethodDisplay = "";

            if (pmCleaned == AppLogic.ro_PMCreditCard)
            {
                paymentMethodDisplay = String.Format("<nobr>{0} - {1}: {2} {3}/{4}", AppLogic.GetString("address.cs.54", SkinID, ThisCustomer.LocaleSetting), primaryAddress.CardType, AppLogic.SafeDisplayCardNumber(primaryAddress.CardNumber, "Address", primaryAddress.AddressID), primaryAddress.CardExpirationMonth, primaryAddress.CardExpirationYear) + "</nobr>";
            }
            else if (pmCleaned == AppLogic.ro_PMECheck)
            {
                paymentMethodDisplay = String.Format("<nobr>ECheck - {0}: {1} {2}", primaryAddress.ECheckBankName, primaryAddress.ECheckBankABACodeMasked, primaryAddress.ECheckBankAccountNumberMasked) + "</nobr>";
            }


            return paymentMethodDisplay;
        }

        protected void btnNewAddress_Click(object sender, EventArgs e)
        {

            AddressControl ctrlNewAddress = pnlContent.FindControl("ctrlNewAddress") as AddressControl;
            if (ctrlNewAddress != null)
            {
                ctrlNewAddress.CountryIDToValidateZipCode = AppLogic.GetCountryID(ctrlNewAddress.Country);
            }

            Page.Validate("AddAddress");

            if (Page.IsValid)
            {
                AddressTypes addressType = AddressMode;
                bool AllowShipToDifferentThanBillTo = AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") 
                                                    && !AppLogic.AppConfigBool("SkipShippingOnCheckout");

                if (!AllowShipToDifferentThanBillTo)
                {
                    //Shipping and Billing address must be the same so save both
                    addressType = AddressTypes.Billing | AddressTypes.Shipping;
                }

                AspDotNetStorefrontCore.Address anyAddress = new AspDotNetStorefrontCore.Address();

                if (ctrlNewAddress != null)
                {
                    anyAddress.CustomerID = ThisCustomer.CustomerID;
                    anyAddress.NickName = ctrlNewAddress.NickName;
                    anyAddress.FirstName = ctrlNewAddress.FirstName;
                    anyAddress.LastName = ctrlNewAddress.LastName;
                    anyAddress.Company = ctrlNewAddress.Company;
                    anyAddress.Address1 = ctrlNewAddress.Address1;
                    anyAddress.Address2 = ctrlNewAddress.Address2;
                    anyAddress.Suite = ctrlNewAddress.Suite;
                    anyAddress.City = ctrlNewAddress.City;
                    anyAddress.State = ctrlNewAddress.State;
                    anyAddress.Zip = ctrlNewAddress.ZipCode;
                    anyAddress.Country = ctrlNewAddress.Country;
                    anyAddress.Phone = ctrlNewAddress.PhoneNumber;
                    anyAddress.ResidenceType = (ResidenceTypes)addressType;

                    anyAddress.InsertDB();

                    int addressID = anyAddress.AddressID;

                    if (ThisCustomer.PrimaryBillingAddressID == 0)
                    {
                        DB.ExecuteSQL("Update Customer set BillingAddressID=" + addressID + " where CustomerID=" + ThisCustomer.CustomerID.ToString());
                    }
                    if (ThisCustomer.PrimaryShippingAddressID == 0)
                    {
                        DB.ExecuteSQL("Update Customer set ShippingAddressID=" + addressID + " where CustomerID=" + ThisCustomer.CustomerID.ToString());
                        ThisCustomer.SetPrimaryShippingAddressForShoppingCart(ThisCustomer.PrimaryShippingAddressID, addressID);
                    }

                    if (AppLogic.AppConfig("VerifyAddressesProvider") != "")
                    {
                        AspDotNetStorefrontCore.Address standardizedAddress = new AspDotNetStorefrontCore.Address();
                        String validateResult = AddressValidation.RunValidate(anyAddress, out standardizedAddress);
                        validateResult = "address.validation.errormsg".StringResource() + validateResult;

                        if (validateResult != AppLogic.ro_OK)
                        {
                            Session["ErrorMsgLabelText"] = System.Web.HttpUtility.HtmlEncode(validateResult);

                        }
                        else
                        {
                            anyAddress = standardizedAddress;
                            anyAddress.UpdateDB();
                        }
                    }

                    String sURL = CommonLogic.ServerVariables("URL") + CommonLogic.IIF(CommonLogic.ServerVariables("QUERY_STRING") != "", "?" + CommonLogic.ServerVariables("QUERY_STRING"), "");

                    if (!CommonLogic.IsStringNullOrEmpty(sURL))
                    {
                        Response.Redirect(sURL);
                    }
                }
            }
        }

        protected void AddNewAddress(object sender, EventArgs e)
        {
            pnlNewAddress.Visible = true;
			dlAddress.ShowFooter = !pnlNewAddress.Visible;

            if (ctrlNewAddress != null)
            {
                CountryDropDownData(ctrlNewAddress);
                StateDropDownData(ctrlNewAddress, ThisCustomer.LocaleSetting);
            }

            dlAddress.EditItemIndex = -1;
            LoadData();
        }

        protected void btnCancelAddNew_OnClick(object sender, EventArgs e)
        {
            pnlNewAddress.Visible = false;
			dlAddress.ShowFooter = !pnlNewAddress.Visible;
			
			dlAddress.EditItemIndex = -1;
            LoadData();
        }

        private void PopulateAddressControlValues(AddressControl ctrlAddress, CreditCardPanel ctrlCreditCard, Echeck ctrlEcheck, int Index)
        {
            Addresses allAddress = GetAddresses();
            AspDotNetStorefrontCore.Address anyAddress = allAddress[Index];

            if (ctrlAddress != null)
            {
                ctrlAddress.NickName = anyAddress.NickName;
                ctrlAddress.FirstName = anyAddress.FirstName;
                ctrlAddress.LastName = anyAddress.LastName;
                ctrlAddress.PhoneNumber = anyAddress.Phone;
                ctrlAddress.Company = anyAddress.Company;
                ctrlAddress.ResidenceType = anyAddress.ResidenceType.ToString();
                ctrlAddress.Address1 = anyAddress.Address1;
                ctrlAddress.Address2 = anyAddress.Address2;
                ctrlAddress.Suite = anyAddress.Suite;
                ctrlAddress.City = anyAddress.City;
                ctrlAddress.ZipCode = anyAddress.Zip;
                CountryDropDownData(ctrlAddress);
                ctrlAddress.Country = anyAddress.Country;
                StateDropDownData(ctrlAddress, ThisCustomer.LocaleSetting);
                ctrlAddress.State = anyAddress.State;
                ctrlAddress.ShowZip = AppLogic.GetCountryPostalCodeRequired(AppLogic.GetCountryID(ctrlAddress.Country));
            }

            if (CustomerCCRequired)
            {
                if (ctrlCreditCard != null)
                {
                    ctrlCreditCard.CreditCardName = anyAddress.CardName;
                    ctrlCreditCard.CreditCardNumber = AppLogic.SafeDisplayCardNumber(anyAddress.CardNumber, "Address", anyAddress.AddressID);
                    ctrlCreditCard.CreditCardType = anyAddress.CardType;
                    ctrlCreditCard.CardExpMonth = anyAddress.CardExpirationMonth;
                    ctrlCreditCard.CardExpYr = anyAddress.CardExpirationYear;
                    if (AppLogic.AppConfigBool("Misc.ShowCardStartDateFields"))
                    {
                        if (!CommonLogic.IsStringNullOrEmpty(anyAddress.CardStartDate))
                        {
                            if (anyAddress.CardStartDate.Length >= 6)
                            {
                                ctrlCreditCard.CardStartMonth = anyAddress.CardStartDate.Substring(0, 2);
                                ctrlCreditCard.CardStartYear = anyAddress.CardStartDate.Substring(2, 4);
                            }
                        }
                    }
                    if (AppLogic.AppConfigBool("CardExtraCodeIsOptional"))
                    {
                        ctrlCreditCard.CreditCardIssueNumber = anyAddress.CardIssueNumber;
                    }
                }
            }

            if (ShowEcheck)
            {
                if (ctrlEcheck != null)
                {
                    ctrlEcheck.ECheckBankAccountName = anyAddress.ECheckBankAccountName;
                    ctrlEcheck.ECheckBankName = anyAddress.ECheckBankName;
                    ctrlEcheck.ECheckBankABACode = AppLogic.SafeDisplayCardNumber(anyAddress.ECheckBankABACode, "Address", anyAddress.AddressID);
                    ctrlEcheck.ECheckBankAccountNumber = anyAddress.ECheckBankAccountNumberMasked;
                    ctrlEcheck.ECheckBankAccountType = anyAddress.ECheckBankAccountType;
                }
            }
        }

        private void CountryDropDownData(AddressControl ctrlAddress)
        {
            //Assign Datasource for the country dropdown
            ctrlAddress.CountryDataSource = Country.GetAll();
            ctrlAddress.CountryDataTextField = "Name";
            ctrlAddress.CountryDataValueField = "Name";
        }

        protected void ctrlAddress_SelectedCountryChanged(object sender, EventArgs e)
        {
            AddressControl ctrlAddress = sender as AddressControl;

            if (ctrlAddress != null)
            {
                StateDropDownData(ctrlAddress, ThisCustomer.LocaleSetting);
                ctrlAddress.ShowZip = AppLogic.GetCountryPostalCodeRequired(AppLogic.GetCountryID(ctrlAddress.Country));
            }
        }

        private void StateDropDownData(AddressControl ctrlAddress, String LocaleSetting)
        {
            //Assign Datasource for the state dropdown
            ctrlAddress.StateDataSource = State.GetAllStateForCountry(AppLogic.GetCountryID(ctrlAddress.Country), LocaleSetting);            
        }

        protected void rblSelectedIndexChanged(object sender, EventArgs e)
        {
            RadioButtonList rblPaymentMethodInfo = sender as RadioButtonList;

            if (rblPaymentMethodInfo != null)
            {
                int EditIndex = dlAddress.EditItemIndex;
                Panel pnlCCData = dlAddress.Items[EditIndex].FindControl("pnlCCData") as Panel;
                Panel pnlECData = dlAddress.Items[EditIndex].FindControl("pnlECData") as Panel;

                if (rblPaymentMethodInfo.SelectedValue.Equals(AppLogic.ro_PMCreditCard, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (pnlCCData != null)
                    {
                        pnlCCData.Visible = true;
                        pnlECData.Visible = false;
                    }
                }
                else if (rblPaymentMethodInfo.SelectedValue.Equals(AppLogic.ro_PMECheck, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (pnlECData != null)
                    {
                        pnlCCData.Visible = false;
                        pnlECData.Visible = true;
                    }
                }
            }
        }

        protected void btnReturnUrlClick(object sender, EventArgs e)
        {
            string returnUrl = CommonLogic.QueryStringCanBeDangerousContent("returnURL");
            if (!CommonLogic.IsStringNullOrEmpty(returnUrl))
            {
                Response.Redirect(returnUrl);
            }
        }

        //JH 10.18.2010 - make sure the address is not attached to any open orders
        private void DisableEditButtonsForAddressWithOpenOrder(Button deleteButton, Button editButton, int addressID)
        {
            if (deleteButton != null && editButton != null)
            {
                if (AddressOpenOrderCount(addressID) > 0)
                {
                    deleteButton.Visible = editButton.Visible = false;
                }
            }
        }

        protected int AddressOpenOrderCount(int addressID)
        {
            int count = 0;
            String sql = @"
                select COUNT(*) as countOpenOrders from Orders o
                JOIN (SELECT OrderNumber, ShippingAddressID FROM dbo.orders_shoppingcart with (nolock) GROUP BY OrderNumber, ShippingAddressID HAVING COUNT(DISTINCT ShippingAddressID) = 1 ) a ON O.OrderNumber = A.OrderNumber 
                WHERE o.ReadyToShip = 1 AND o.ShippedOn IS NULL AND TransactionState IN ('AUTHORIZED', 'CAPTURED')
                and a.ShippingAddressID = {0}
            ";
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(String.Format(sql, addressID.ToString()), dbconn))
                {
                    if (rs.Read())
                    {
                        count = DB.RSFieldInt(rs, "countOpenOrders");
                    }
                }
            }
            return count;
        }

    }
}



