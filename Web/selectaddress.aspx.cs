// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront
{
    public partial class selectaddress : SkinBase
    {
        bool Checkout = false;
        public AddressTypes AddressType = AddressTypes.Unknown;
        public Addresses custAddresses;
        public String PaymentMethodPrompt = String.Empty;
        public String ReturnURL = String.Empty;
        public String ButtonImage = String.Empty;
        public String AddressTypeString = String.Empty;
        bool VerifyAddressPrompt = false;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            RequireSecurePage();

            ReturnURL = CommonLogic.QueryStringCanBeDangerousContent("ReturnURL");
            AppLogic.CheckForScriptTag(ReturnURL);
            AddressTypeString = CommonLogic.QueryStringCanBeDangerousContent("AddressType");
            AppLogic.CheckForScriptTag(AddressTypeString);
            ThisCustomer.RequireCustomerRecord();
            if (!Shipping.MultiShipEnabled())
            {
                RequiresLogin(CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING"));
            }
            SectionTitle = String.Format(AppLogic.GetString("selectaddress.aspx.1", SkinID, ThisCustomer.LocaleSetting), AddressTypeString);
            Checkout = CommonLogic.QueryStringBool("checkout");


            if (DB.GetSqlN("select count(*) as N from Address  with (NOLOCK)  where CustomerID=" + ThisCustomer.CustomerID.ToString()) == 0)
            {
                pnlAddressListBottom.Visible = false;
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("add").Equals("TRUE", StringComparison.InvariantCultureIgnoreCase))
            {
                liAdd.Visible = false;
            }

            if (AddressTypeString.Length != 0)
            {
                AddressType = (AddressTypes)Enum.Parse(typeof(AddressTypes), AddressTypeString, true);
            }
            if (AddressType == AddressTypes.Unknown)
            {
                AddressType = AddressTypes.Shipping;
                AddressTypeString = "Shipping";
            }

            if (AddressType == AddressTypes.Billing)
            {
                SectionTitle += " - " + AppLogic.GetString("order.cs.58", SkinID, ThisCustomer.LocaleSetting);
            }
            else
            {
                SectionTitle += " - " + AppLogic.GetString("order.cs.57", SkinID, ThisCustomer.LocaleSetting);
            }

            custAddresses = new Addresses();
            custAddresses.LoadCustomer(ThisCustomer.CustomerID);


            if (AddressType == AddressTypes.Shipping)
            {
                ButtonImage = AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/usethisshippingaddress.gif", ThisCustomer.LocaleSetting);
            }
            else
            {
                ButtonImage = AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/usethisbillingaddress.gif", ThisCustomer.LocaleSetting);
            }
            PaymentMethodPrompt = "<b>" + AppLogic.GetString("selectaddress.aspx.8", SkinID, ThisCustomer.LocaleSetting) + "</b>";

            if (!IsPostBack)
            {
                InitializePageContent();
            }
        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.AddressList.ItemDataBound += new RepeaterItemEventHandler(AddressList_ItemDataBound);
            this.AddressList.ItemCommand += new RepeaterCommandEventHandler(AddressList_ItemCommand);
            this.btnNewAddress.Click += new EventHandler(btnNewAddress_Click);

        }

        #endregion

        void AddressList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                ImageButton MakePrimaryBtn = (ImageButton)e.Item.FindControl("btnMakePrimary");
                ImageButton EditBtn = (ImageButton)e.Item.FindControl("btnEdit");
                ImageButton DeleteBtn = (ImageButton)e.Item.FindControl("btnDelete");

                MakePrimaryBtn.Visible = (((DbDataRecord)e.Item.DataItem)["PrimaryAddress"].ToString() == "0");

                MakePrimaryBtn.ImageUrl = ButtonImage;
                EditBtn.ImageUrl = AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/edit2.gif");
                DeleteBtn.ImageUrl = AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/delete.gif");
            }

        }

        void AddressList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "edit":
                    Response.Redirect(String.Format("editaddress.aspx?Checkout={0}&AddressType={1}&AddressID={2}&ReturnURL={3}", Checkout.ToString(), AddressType, e.CommandArgument, ReturnURL));
                    break;
                case "delete":
                    DB.ExecuteSQL("exec aspdnsf_DeleteAddress " + e.CommandArgument + ", " + ThisCustomer.CustomerID.ToString());
                    InitializePageContent();
                    break;
                case "makeprimary":
                    int AddressID = System.Int32.Parse(e.CommandArgument.ToString());
                    DB.ExecuteSQL("update customer set " + CommonLogic.IIF(AddressType == AddressTypes.Billing || AddressType == AddressTypes.Unknown, "BillingAddressID=", "ShippingAddressID=") + AddressID.ToString() + " where customerid = " + ThisCustomer.CustomerID.ToString());
                    // make sure cart is now updated to use this shipping address id, but not if multi-ship is enabled
                    if (AddressType == AddressTypes.Shipping)
                    {
                        ThisCustomer.SetPrimaryShippingAddressForShoppingCart(ThisCustomer.PrimaryShippingAddressID,AddressID);
                    }
                    InitializePageContent();
                    break;
            }
        }

        void btnNewAddress_Click(object sender, EventArgs e)
        {
            lblErrMsg.Text = "";
            AddressTypes AddressType = (AddressTypes)Enum.Parse(typeof(AddressTypes), AddressTypeString, true);
            int OriginalRecurringOrderNumber = CommonLogic.QueryStringUSInt("OriginalRecurringOrderNumber");
            bool AllowShipToDifferentThanBillTo = AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") && !AppLogic.AppConfigBool("SkipShippingOnCheckout");

            if (!AllowShipToDifferentThanBillTo)
            {
                //Shipping and Billing address nust be the same so save both
                AddressType = AddressTypes.Billing | AddressTypes.Shipping;
            }

            if (CommonLogic.FormCanBeDangerousContent("AddressFirstName") == "")
            {
                lblErrMsg.Text += "First Name is required";
            }
            if (CommonLogic.FormCanBeDangerousContent("AddressLastName") == "")
            {
                lblErrMsg.Text += "Last Name is required";
            }
            if (CommonLogic.FormCanBeDangerousContent("AddressPhone") == "")
            {
                lblErrMsg.Text += "Phone is required";
            }
            if (CommonLogic.FormCanBeDangerousContent("AddressAddress1") == "")
            {
                lblErrMsg.Text += "Address1 is required";
            }
            if (CommonLogic.FormCanBeDangerousContent("AddressCity") == "")
            {
                lblErrMsg.Text += "City is required";
            }
            if (CommonLogic.FormCanBeDangerousContent("AddressState") == "")
            {
                lblErrMsg.Text += "State is required";
            }
            if (CommonLogic.FormCanBeDangerousContent("AddressZip") == "")
            {
                lblErrMsg.Text += "ZIP is required";
            }
            if (ValidateAddress(CommonLogic.FormCanBeDangerousContent("AddressAddress1")))
            {
                lblErrMsg.Text += AppLogic.GetString("createaccount_process.aspx.3", SkinID, ThisCustomer.LocaleSetting) + "";
            }

            Address thisAddress = new Address();

            thisAddress.CustomerID = ThisCustomer.CustomerID;
            thisAddress.NickName = CommonLogic.FormCanBeDangerousContent("AddressNickName");
            thisAddress.FirstName = CommonLogic.FormCanBeDangerousContent("AddressFirstName");
            thisAddress.LastName = CommonLogic.FormCanBeDangerousContent("AddressLastName");
            thisAddress.Company = CommonLogic.FormCanBeDangerousContent("AddressCompany");
            thisAddress.Address1 = CommonLogic.FormCanBeDangerousContent("AddressAddress1");
            thisAddress.Address2 = CommonLogic.FormCanBeDangerousContent("AddressAddress2");
            thisAddress.Suite = CommonLogic.FormCanBeDangerousContent("AddressSuite");
            thisAddress.City = CommonLogic.FormCanBeDangerousContent("AddressCity");
            thisAddress.State = CommonLogic.FormCanBeDangerousContent("AddressState");
            thisAddress.Zip = CommonLogic.FormCanBeDangerousContent("AddressZip");
            thisAddress.Country = CommonLogic.FormCanBeDangerousContent("AddressCountry");
            thisAddress.Phone = CommonLogic.FormCanBeDangerousContent("AddressPhone");
            thisAddress.EMail = ThisCustomer.EMail;
            thisAddress.ResidenceType = (ResidenceTypes)CommonLogic.FormNativeInt("ResidenceType");

            if (lblErrMsg.Text == "")
            {
                thisAddress.InsertDB();
                int AddressID = thisAddress.AddressID;

                if (ThisCustomer.PrimaryBillingAddressID == 0)
                {
                    DB.ExecuteSQL("Update Customer set BillingAddressID=" + AddressID.ToString() + " where CustomerID=" + ThisCustomer.CustomerID.ToString());
                }
                if (ThisCustomer.PrimaryShippingAddressID == 0)
                {
                    DB.ExecuteSQL("Update Customer set ShippingAddressID=" + AddressID.ToString() + " where CustomerID=" + ThisCustomer.CustomerID.ToString());
                    ThisCustomer.SetPrimaryShippingAddressForShoppingCart(ThisCustomer.PrimaryShippingAddressID, AddressID);
                }

                if (OriginalRecurringOrderNumber != 0)
                {
                    //put it in the ShoppingCart record
                    string sql = String.Empty;
                    if ((AddressType & AddressTypes.Billing) != 0)
                    {
                        sql = String.Format("BillingAddressID={0}", AddressID);
                    }
                    if ((AddressType & AddressTypes.Shipping) != 0)
                    {
                        if (sql.Length != 0)
                        {
                            sql += ",";
                        }
                        sql += String.Format("ShippingAddressID={0}", AddressID);
                    }
                    sql = String.Format("update ShoppingCart set " + sql + " where OriginalRecurringOrderNumber={0}", OriginalRecurringOrderNumber.ToString());
                    DB.ExecuteSQL(sql);
                }

                if (AppLogic.AppConfig("VerifyAddressesProvider") != "")
                {
                    Address StandardizedAddress = new Address();
                    String VerifyResult = AddressValidation.RunValidate(thisAddress, out StandardizedAddress);
                    VerifyAddressPrompt = (VerifyResult != AppLogic.ro_OK);
                    if (VerifyAddressPrompt)
                    {
                        thisAddress = StandardizedAddress;
                        thisAddress.UpdateDB();
                        Response.Redirect(String.Format("editaddress.aspx?Checkout={0}&AddressType={1}&ReturnURL={2}&AddressID={3}&Prompt={4}", Checkout.ToString(), AddressTypeString, Server.UrlEncode(ReturnURL), thisAddress.AddressID, VerifyResult));
                    }
                    else
                    {
                        Response.Redirect(String.Format("selectaddress.aspx?Checkout={0}&AddressType={1}&ReturnURL={2}", Checkout.ToString(), AddressTypeString, Server.UrlEncode(ReturnURL)));
                    }
                }
                else
                {
                    Response.Redirect(String.Format("selectaddress.aspx?Checkout={0}&AddressType={1}&ReturnURL={2}", Checkout.ToString(), AddressTypeString, Server.UrlEncode(ReturnURL)));
                }
            }
            else
            {
                // Redisplay the info they gave us so they don't have to re-enter it.
                litNewAddressForm.Text = thisAddress.InputHTML();
            }
        }

        private bool ValidateAddress(string address)
        {
            address = address.Replace(" ", "").Trim().Replace(".", "");
			bool IsPOBoxAddress = !(new POBoxAddressValidator()).IsValid(address);
            bool RejectDueToPOBoxAddress = (IsPOBoxAddress && AppLogic.AppConfigBool("DisallowShippingToPOBoxes")); // undocumented feature

            return RejectDueToPOBoxAddress;
        }

        private void InitializePageContent()
        {
            pnlAddressList.Visible = (custAddresses.Count > 0 || CommonLogic.QueryStringCanBeDangerousContent("add").Length != 0);
            tblAddressList.Attributes.Add("style", "border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor"));
            tblAddressListBox.Attributes.Add("style", AppLogic.AppConfig("BoxFrameStyle"));
            lnkAddAddress.Text = AppLogic.GetString("selectaddress.aspx.6", SkinID, ThisCustomer.LocaleSetting);
            lnkAddAddress.NavigateUrl = "selectaddress.aspx?add=true&checkout=" + Checkout.ToString() + "&addressType=" + AddressType.ToString() + "&returnURL=" + Server.UrlEncode(ReturnURL);
            lnkAddAddress.Visible = (CommonLogic.QueryStringCanBeDangerousContent("add").Length == 0);
            if (CommonLogic.QueryStringCanBeDangerousContent("add").Length != 0)
            {
                pnlNewAddress.Visible = true;
                Address newAddress = new Address();
                newAddress.AddressType = AddressType;
                litNewAddressForm.Text = newAddress.InputHTML();
                litNewAddressForm.Visible = true;
                btnNewAddress.Text = AppLogic.GetString("selectaddress.aspx.5", SkinID, ThisCustomer.LocaleSetting);
                AppLogic.GetButtonDisable(btnNewAddress);
            }

            addressbook_gif.ImageUrl = AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/addressbook.gif", ThisCustomer.LocaleSetting);
            string sql = String.Empty;
            if (AddressType == AddressTypes.Shipping)
            {
                sql = "select a.*, isnull(City + ', ', '') + isnull(State,'') + isnull(' ' + ZIP, '') CityStateZip, case when c.customerid is null then 0 else 1 end PrimaryAddress from Address a left join Customer c on a.AddressID = c.ShippingAddressID where a.customerid = " + ThisCustomer.CustomerID.ToString();
            }
            else
            {
                sql = "select a.*, isnull(City + ', ', '') + isnull(State,'') + isnull(' ' + ZIP, '') CityStateZip, case when c.customerid is null then 0 else 1 end PrimaryAddress from Address a left join Customer c on a.AddressID = c.BillingAddressID where a.customerid = " + ThisCustomer.CustomerID.ToString();
            }

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader dr = DB.GetRS(sql, dbconn))
                {
                    AddressList.DataSource = dr;
                    AddressList.DataBind();
                }
            }
           
            btnReturn.Text = AppLogic.GetString("account.aspx.61", SkinID, ThisCustomer.LocaleSetting);
            btnReturn.OnClientClick = "self.location='account.aspx?checkout=" + Checkout.ToString() + "';return false";
            btnCheckOut.Visible = Checkout;
            btnCheckOut.Text = AppLogic.GetString("account.aspx.60", SkinID, ThisCustomer.LocaleSetting);
            btnCheckOut.OnClientClick = "self.location='checkoutshipping.aspx';return false;";

        }

        public String DisplayPaymentMethod(Customer ViewingCustomer, String PaymentMethod, int AddrCustomerID, int AddressID)
        {
            if (ViewingCustomer.MasterShouldWeStoreCreditCardInfo)
            {
                Address addr = new Address();
                addr.LoadFromDB(AddressID);
                String PMCleaned = AppLogic.CleanPaymentMethod(PaymentMethod);
                if (PMCleaned == AppLogic.ro_PMMicropay)
                {
                    return String.Format(AppLogic.GetString("account.aspx.11", SkinID, ThisCustomer.LocaleSetting) + " - {0}", ViewingCustomer.CurrencyString(AppLogic.GetMicroPayBalance(AddrCustomerID))) + "";
                }
                if (PMCleaned == AppLogic.ro_PMECheck)
                {
                    return String.Format("ECheck - {0}: {1} {2}", addr.ECheckBankName, addr.ECheckBankABACodeMasked, addr.ECheckBankAccountNumberMasked) + "";
                }
                if (PMCleaned == AppLogic.ro_PMCreditCard)
                {
                    return String.Format("{0} - {1}: {2} {3}/{4}", AppLogic.GetString("address.cs.54", SkinID, ThisCustomer.LocaleSetting), addr.CardType, AppLogic.SafeDisplayCardNumber(addr.CardNumber, "Address", addr.AddressID), addr.CardExpirationMonth, addr.CardExpirationYear) + "";
                }
            }
            return String.Empty;
        }
    }
}
