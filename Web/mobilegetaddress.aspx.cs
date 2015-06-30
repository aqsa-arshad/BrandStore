// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;
using Vortx.MobileFramework;

namespace AspDotNetStorefront
{
    public partial class mobilegetaddress : SkinBase
    {
        bool Checkout = false;
        bool SkipRegistration = false;
        bool AllowShipToDifferentThanBillTo = false;
        String VerifyResult = String.Empty;
        String ReturnURL = String.Empty;
        String checkouttype = "";
        Address AddAddress = new AspDotNetStorefrontCore.Address(); // qualification needed for vb.net (not sure why)
        Address StandardizedAddress = new AspDotNetStorefrontCore.Address();
        AddressTypes AddType = AddressTypes.Billing;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            MobileHelper.RedirectPageWhenMobileIsDisabled("~/address.aspx", ThisCustomer);

            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            RequireSecurePage();
            ThisCustomer.RequireCustomerRecord();

            if (!ThisCustomer.IsRegistered)
            {
                Response.Redirect("mobilecreateaccount.aspx?" + BuildRedirectQuerystring());
            }

            SectionTitle = AppLogic.GetString("createaccount.aspx.1", SkinID, ThisCustomer.LocaleSetting);
            Checkout = CommonLogic.QueryStringBool("checkout");
            SkipRegistration = CommonLogic.QueryStringBool("skipreg");
            checkouttype = CommonLogic.QueryStringCanBeDangerousContent("checkouttype");

            if (CommonLogic.QueryStringCanBeDangerousContent("addresstype").ToLower() == "shipping")
            {
                AddType = AddressTypes.Shipping;
            }

            MPAddressHeader.Text = AddType.ToString() + " Info";

            switch (AddType)
            {
                case AddressTypes.Shipping:
                    btnUseForBothAddressTypes.Visible = false;
                    btnUseForOneAddress.Text = AppLogic.GetString("Mobile.GetAddress.ShipTo", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    btnUseForOneAddress.CssClass = "fullwidthshortgreen action";
                    break;
                default:
                    btnUseForBothAddressTypes.Text = AppLogic.GetString("Mobile.GetAddress.Both", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    btnUseForOneAddress.Text = AppLogic.GetString("Mobile.GetAddress.NewShip", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    break;
            }

            if (Checkout)
            {
                SectionTitle = AppLogic.GetString("createaccount.aspx.2", SkinID, ThisCustomer.LocaleSetting) + SectionTitle;
                ShoppingCart cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                cart.ValidProceedCheckout();

            }

            AllowShipToDifferentThanBillTo = AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") &&
                                            !AppLogic.AppConfigBool("SkipShippingOnCheckout");
            ReturnURL = CommonLogic.QueryStringCanBeDangerousContent("ReturnURL");
            AppLogic.CheckForScriptTag(ReturnURL);
            ErrorMsgLabel.Text = "";

            if (!IsPostBack)
            {
                AddAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                InitializePageContent();
            }

            #region vortx init xmlpackage
            CheckoutHeader.Text = Vortx.MobileFramework.MobileXSLTExtensionBase.GetCheckoutHeader("address");
            #endregion
        }

        public void ValidateAddress(object source, ServerValidateEventArgs args)
        {
            String Adr1 = ctrlBillingAddress.Address1;
            Adr1 = Adr1.Replace(" ", "").Trim().Replace(".", "");
			bool IsPOBoxAddress = !(new POBoxAddressValidator()).IsValid(Adr1);
            bool RejectDueToPOBoxAddress = (IsPOBoxAddress && AppLogic.AppConfigBool("DisallowShippingToPOBoxes")); // undocumented feature
            args.IsValid = !RejectDueToPOBoxAddress;
        }

        #region Private Functions

        private void InitializePageContent()
        {
            if (Checkout)
            {
                pnlCheckoutImage.Visible = true;
            }
            if (CommonLogic.QueryStringCanBeDangerousContent("errormsg").Length > 0)
            {
                AppLogic.CheckForScriptTag(CommonLogic.QueryStringCanBeDangerousContent("errormsg"));
                ErrorMsgLabel.Text = Server.HtmlEncode(CommonLogic.QueryStringCanBeDangerousContent("ErrorMsg")).Replace("+", " ");
            }
            if (!IsPostBack)
            {
                ctrlBillingAddress.NickName = Server.HtmlEncode(ctrlBillingAddress.NickName);
                ctrlBillingAddress.FirstName = Server.HtmlEncode(CommonLogic.IIF(ThisCustomer.FirstName.Length != 0, ThisCustomer.FirstName, AddAddress.FirstName));
                ctrlBillingAddress.LastName = Server.HtmlEncode(CommonLogic.IIF(ThisCustomer.LastName.Length != 0, ThisCustomer.LastName, AddAddress.LastName));
                ctrlBillingAddress.PhoneNumber = Server.HtmlEncode(AddAddress.Phone);
                ctrlBillingAddress.Company = Server.HtmlEncode(AddAddress.Company);
                ctrlBillingAddress.ResidenceType = AddAddress.ResidenceType.ToString();
                ctrlBillingAddress.Address1 = Server.HtmlEncode(AddAddress.Address1);
                ctrlBillingAddress.Address2 = Server.HtmlEncode(AddAddress.Address2);
                ctrlBillingAddress.Suite = Server.HtmlEncode(AddAddress.Suite);
                ctrlBillingAddress.State = Server.HtmlEncode(AddAddress.State);
                ctrlBillingAddress.City = Server.HtmlEncode(AddAddress.City);
                ctrlBillingAddress.ZipCode = AddAddress.Zip;
                ctrlBillingAddress.ShowZip = AppLogic.GetCountryPostalCodeRequired(AppLogic.GetCountryID(ctrlBillingAddress.Country));
            }
            AppLogic.GetButtonDisable(this.btnUseForBothAddressTypes);
            AppLogic.GetButtonDisable(this.btnUseForOneAddress);
        }

        private void AddToNewsletterList(string firstName, string lastName, string emailAddress)
        {
            new NewsletterControlService().Subscribe(emailAddress, firstName, lastName);
        }
        private void CreateAccount(AddressTypes[] AddTypes)
        {
            if (SkipRegistration)
                Page.Validate("skipreg");
            else
                Page.Validate("registration");

            ctrlBillingAddress.CountryIDToValidateZipCode = AppLogic.GetCountryID(ctrlBillingAddress.Country);
            Page.Validate("createacccount");

            if (Page.IsValid)
            {
                String EMailField = ThisCustomer.EMail;
                AddAddress = new Address();

                AddAddress.NickName = ctrlBillingAddress.NickName;
                AddAddress.LastName = ctrlBillingAddress.LastName;
                AddAddress.FirstName = ctrlBillingAddress.FirstName;
                AddAddress.Phone = ctrlBillingAddress.PhoneNumber;
                AddAddress.Company = ctrlBillingAddress.Company;
                AddAddress.ResidenceType = (ResidenceTypes)Enum.Parse(typeof(ResidenceTypes), ctrlBillingAddress.ResidenceType);
                AddAddress.Address1 = ctrlBillingAddress.Address1;
                AddAddress.Address2 = ctrlBillingAddress.Address2;
                AddAddress.Suite = ctrlBillingAddress.Suite;
                AddAddress.City = ctrlBillingAddress.City;
                AddAddress.State = ctrlBillingAddress.State;
                AddAddress.Zip = ctrlBillingAddress.ZipCode;
                AddAddress.Country = ctrlBillingAddress.Country;

                AddAddress.InsertDB(ThisCustomer.CustomerID);
                foreach (AddressTypes type in AddTypes)
                {
                    AddAddress.MakeCustomersPrimaryAddress(type);
                }
            }
            else
            {
                ErrorMsgLabel.Text += AppLogic.GetString("createaccount.aspx.84", 1, Localization.GetDefaultLocale()) + "";
                foreach (IValidator aValidator in this.Validators)
                {
                    if (!aValidator.IsValid)
                    {
                        ErrorMsgLabel.Text += "&bull; " + aValidator.ErrorMessage + "";
                    }
                }
                pnlErrorMsg.Visible = true;
                ResetScrollPosition();
            }
        }

        private void ResetScrollPosition()
        {
            if (!ClientScript.IsClientScriptBlockRegistered(this.GetType(), "CreateResetScrollPosition"))
            {
                StringBuilder script = new StringBuilder();
                script.AppendLine();
                script.Append("<script type=\"text/javascript\">\n");
                script.Append("function ResetScrollPosition() {\n");
                script.Append("  var scrollX = document.getElementById('__SCROLLPOSITIONX');\n");
                script.Append("  var scrollY = document.getElementById('__SCROLLPOSITIONY');\n");
                script.Append("  if (scrollX != null && scrollY != null)\n");
                script.Append("     {\n");
                script.Append("         scrollX.value = 0;\n");
                script.Append("         scrollY.value = 0;\n");
                script.Append("     }\n");
                script.Append("}\n");
                script.Append("</script>\n");

                ClientScript.RegisterClientScriptBlock(this.GetType(), "CreateResetScrollPosition", script.ToString());
                ClientScript.RegisterStartupScript(this.GetType(), "CallResetScrollPosition", "ResetScrollPosition();", true);
            }

        }
        #endregion

        protected override void OnInit(EventArgs e)
        {
            InitializePageContent2();
            base.OnInit(e);
        }

        private void InitializePageContent2()
        {
            if (ctrlBillingAddress != null)
            {
                CountryDropdownData(ctrlBillingAddress);
                StateDropdownData(ctrlBillingAddress);
            }
        }

        protected void ctrlAddress_SelectedCountryChanged(object sender, EventArgs e)
        {
            AddressControl ctrlAddress = sender as AddressControl;

            if (ctrlAddress != null)
            {
                StateDropdownData(ctrlAddress);
                ctrlAddress.ShowZip = AppLogic.GetCountryPostalCodeRequired(AppLogic.GetCountryID(ctrlAddress.Country));
            }
        }

        private void CountryDropdownData(AddressControl ctrlAddress)
        {
            if (ctrlAddress != null)
            {
                //Billing Country DropDown
                ctrlAddress.CountryDataSource = Country.GetAll();
                ctrlAddress.CountryDataTextField = "Name";
                ctrlAddress.CountryDataValueField = "Name";

            }
        }

        private void StateDropdownData(AddressControl ctrlAddress)
        {
            ctrlAddress.StateDataSource = State.GetAllStateForCountry(AppLogic.GetCountryID(ctrlAddress.Country), ThisCustomer.LocaleSetting);
        }

        private void StateDropdownData(AddressControl ctrlBilling, AddressControl ctrlShipping)
        {
            ctrlShipping.StateDataSource = State.GetAllStateForCountry(AppLogic.GetCountryID(ctrlBilling.Country), ThisCustomer.LocaleSetting);
        }
        public void btnUseForBothAddressTypes_Click(object sender, EventArgs e)
        {
            CreateAccount(new AddressTypes[] { AddressTypes.Shipping, AddressTypes.Billing });
            Redirect("mobilecheckoutshipping.aspx");
        }
        private void Redirect(string checkoutRedirect)
        {
            Redirect(checkoutRedirect, true);
        }
        private void Redirect(string checkoutRedirect, bool prefixQueryString)
        {
            if (Page.IsValid)
            {
                if (!checkoutRedirect.EndsWith("?") && prefixQueryString)
                {
                    checkoutRedirect += "?";
                }

                if (Checkout)
                {
					if (checkouttype == "ppec" || checkouttype == "ppbml" || checkouttype == "gc")
                    {
                        Response.Redirect("shoppingcart.aspx" + BuildRedirectQuerystring());
                    }
                    else
                    {
                        Response.Redirect(checkoutRedirect + BuildRedirectQuerystring());
                    }
                }
                else
                {
                    Response.Redirect("account.aspx?newaccount=true");
                }
            }
        }

        public void btnUseForOneAddress_Click(object sender, EventArgs e)
        {
            switch (AddType)
            {
                case AddressTypes.Shipping:
                    CreateAccount(new AddressTypes[] { AddressTypes.Shipping });
                    Redirect("mobilecheckoutshipping.aspx");
                    break;
                default:
                    CreateAccount(new AddressTypes[] { AddressTypes.Billing });
                    Redirect("mobilegetaddress.aspx?addresstype=shipping&", false);
                    break;
            }
        }

        private string BuildRedirectQuerystring()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("checkout=" + Checkout.ToString() + "&");
            if (checkouttype != "")
            {
                sb.Append("checkouttype=" + checkouttype + "&");
            }
            if (ReturnURL != "")
            {
                sb.Append("returnurl=" + ReturnURL + "&");
            }
            return sb.ToString();
        }
    }

}
