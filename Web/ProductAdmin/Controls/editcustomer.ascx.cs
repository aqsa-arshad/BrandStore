// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefrontControls
{
    public partial class Admin_Controls_editcustomer : BaseControl
    {
        #region Private Variables

        private int m_TargetCustomerID = 0;
        private bool m_IPIsBlocked = false;
        private bool m_VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
        private Customer m_TargetCustomer;

        private bool m_addmode;
        public bool AddMode
        {
            get { return m_addmode; }
            set { m_addmode = value; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Default Page_Load Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            this.adrlCustomerAddresses.DataChanged += new EventHandler(AddressEdit_DataChanged);
            this.adrlCustomerAddresses.CloseAddress += new EventHandler(AddressEdit_Closed);
            AddMode = false;
            
            if (!IsPostBack)
            {
                InitializeContent();
            }
            else if (ViewState["CustomerID"] != null && ViewState["CustomerID"].ToString() != "0")
            {
                m_TargetCustomerID = Convert.ToInt32(ViewState["CustomerID"]);
                adrlCustomerAddresses.CustomerID = m_TargetCustomerID;
            }
        }

        /// <summary>
        /// Method to catch bubbled event from the address control when data is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddressEdit_DataChanged(object sender, EventArgs e)
        {
            InitializeContent();

            UpdatePanel p = this.FindControl<UpdatePanel>("updAddressTable");

            if (p != null)
            {
                p.Update();
            }
        }

        protected void btnViewEditAddresses_Click(object sender, EventArgs e)
        {
            adrlCustomerAddresses.CustomerID = m_TargetCustomerID;

            if (m_TargetCustomer == null)
            {
                m_TargetCustomer = new Customer(m_TargetCustomerID);
            }

            if (m_TargetCustomer.HasAtLeastOneAddress())
            {
                adrlCustomerAddresses.CurrentDetailsViewMode = DetailsViewMode.ReadOnly;
            }
            else
            {
                adrlCustomerAddresses.CurrentDetailsViewMode = DetailsViewMode.Insert;
            }

            adrlCustomerAddresses.BindDetails();

            pnlCustomerEdit.Visible = false;
            pnlAddresses.Visible = true;

            UpdatePanel p = adrlCustomerAddresses.FindControl<UpdatePanel>("updAddressList");

            if (p != null)
            {
                p.Update();
            }
        }

        private void AddressEdit_Closed(object sender, EventArgs e)
        {
            pnlAddresses.Visible = false;
            pnlCustomerEdit.Visible = true;

            UpdatePanel p = this.FindControl<UpdatePanel>("updCustomerEdit");

            if (p != null)
            {
                p.Update();
            }
        }
        
        /// <summary>
        /// Manually resets the target customer's password to the value in the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnManualPassword_OnClick(object sender, EventArgs e)
        {
            if (ViewState["CustomerID"] != null && Convert.ToInt32(ViewState["CustomerID"].ToString()) > 0)
            {
                m_TargetCustomerID = Convert.ToInt32(ViewState["CustomerID"].ToString());
                m_TargetCustomer = new Customer(m_TargetCustomerID);
            }
            else
            {
                return;
            }

            bool emailSent = false;

            Password p = new Password(txtManualPassword.Text.Trim());

            //Try and email the new password to the target customer
            try
            {
                AppLogic.SendMail(AppLogic.AppConfig("StoreName") + " - " + AppLogic.GetString("cst_account_process.aspx.1", m_TargetCustomer.SkinID, m_TargetCustomer.LocaleSetting), AppLogic.RunXmlPackage("notification.lostpassword.xml.config", null, m_TargetCustomer, m_TargetCustomer.SkinID, "", "thiscustomerid=" + m_TargetCustomer.CustomerID.ToString() + "&newpwd=" + p.ClearPassword, false, false), true, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromName"), m_TargetCustomer.EMail, m_TargetCustomer.FullName(), "", "", AppLogic.MailServer());
                emailSent = true;
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Alert);
            }

            //Log the event
            Security.LogEvent(AppLogic.GetString("admin.cst_account_process.event.AdminResetCustomerPassword", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), "", m_TargetCustomer.CustomerID, ThisCustomer.CustomerID, Convert.ToInt32(ThisCustomer.CurrentSessionID));

            #region Update customer record
            //Update the customer record
            object lockeduntil = DateTime.Now.AddMinutes(-1);
            m_TargetCustomer.UpdateCustomer(
                /*CustomerLevelID*/ null,
                /*EMail*/ null,
                /*SaltedAndHashedPassword*/ p.SaltedPassword,
                /*SaltKey*/ p.Salt,
                /*DateOfBirth*/ null,
                /*Gender*/ null,
                /*FirstName*/ null,
                /*LastName*/ null,
                /*Notes*/ null,
                /*SkinID*/ null,
                /*Phone*/ null,
                /*AffiliateID*/ null,
                /*Referrer*/ null,
                /*CouponCode*/ null,
                /*OkToEmail*/ null,
                /*IsAdmin*/ null,
                /*BillingEqualsShipping*/ null,
                /*LastIPAddress*/ null,
                /*OrderNotes*/ null,
                /*SubscriptionExpiresOn*/ null,
                /*RTShipRequest*/ null,
                /*RTShipResponse*/ null,
                /*OrderOptions*/ null,
                /*LocaleSetting*/ null,
                /*MicroPayBalance*/ null,
                /*RecurringShippingMethodID*/ null,
                /*RecurringShippingMethod*/ null,
                /*BillingAddressID*/ null,
                /*ShippingAddressID*/ null,
                /*GiftRegistryGUID*/ null,
                /*GiftRegistryIsAnonymous*/ null,
                /*GiftRegistryAllowSearchByOthers*/ null,
                /*GiftRegistryNickName*/ null,
                /*GiftRegistryHideShippingAddresses*/ null,
                /*CODCompanyCheckAllowed*/ null,
                /*CODNet30Allowed*/ null,
                /*ExtensionData*/ null,
                /*FinalizationData*/ null,
                /*Deleted*/ null,
                /*Over13Checked*/ null,
                /*CurrencySetting*/ null,
                /*VATSetting*/ null,
                /*VATRegistrationID*/ null,
                /*StoreCCInDB*/ null,
                /*IsRegistered*/ null,
                /*LockedUntil*/ lockeduntil,
                /*AdminCanViewCC*/ null,
                /*BadLogin*/ -1,
                /*Active*/ null,
                /*PwdChangeRequired*/ 1,
                /*RegisterDate*/ null,
                /*StoreId*/null
             );
            #endregion

            if (emailSent)
            {
                DisplayMessage(false, AppLogic.GetString("admin.customer.passwordResetSuccessful", ThisCustomer.LocaleSetting));
            }
            else
            {
                DisplayMessage(true, AppLogic.GetString("admin.customer.passwordResetEmailFailed", ThisCustomer.LocaleSetting));
            }

            txtManualPassword.Text = String.Empty;

        }

        /// <summary>
        /// Resets the target customer's password to a random value and emails them
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnRandomPassword_OnClick(object sender, EventArgs e)
        {
            if (ViewState["CustomerID"] != null && Convert.ToInt32(ViewState["CustomerID"].ToString()) > 0)
            {
                m_TargetCustomerID = Convert.ToInt32(ViewState["CustomerID"].ToString());
                m_TargetCustomer = new Customer(m_TargetCustomerID);
            }
            else
            {
                return;
            }

            Password p = new Password(AspDotNetStorefrontEncrypt.Encrypt.CreateRandomStrongPassword(8));

            try
            {
                //Send the new password email
                AppLogic.SendMail(AppLogic.AppConfig("StoreName") + " - " + AppLogic.GetString("cst_account_process.aspx.1", m_TargetCustomer.SkinID, m_TargetCustomer.LocaleSetting), AppLogic.RunXmlPackage("notification.lostpassword.xml.config", null, m_TargetCustomer, m_TargetCustomer.SkinID, "", "thiscustomerid=" + m_TargetCustomer.CustomerID.ToString() + "&newpwd=" + p.ClearPassword, false, false), true, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromName"), m_TargetCustomer.EMail, m_TargetCustomer.FullName(), "", "", AppLogic.MailServer());
                Security.LogEvent(AppLogic.GetString("admin.cst_account_process.event.AdminResetCustomerPassword", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), "", m_TargetCustomer.CustomerID, ThisCustomer.CustomerID, Convert.ToInt32(ThisCustomer.CurrentSessionID));

                #region Update customer record
                //Update the customer record
                object lockeduntil = DateTime.Now.AddMinutes(-1);
                m_TargetCustomer.UpdateCustomer(
                    /*CustomerLevelID*/ null,
                    /*EMail*/ null,
                    /*SaltedAndHashedPassword*/ p.SaltedPassword,
                    /*SaltKey*/ p.Salt,
                    /*DateOfBirth*/ null,
                    /*Gender*/ null,
                    /*FirstName*/ null,
                    /*LastName*/ null,
                    /*Notes*/ null,
                    /*SkinID*/ null,
                    /*Phone*/ null,
                    /*AffiliateID*/ null,
                    /*Referrer*/ null,
                    /*CouponCode*/ null,
                    /*OkToEmail*/ null,
                    /*IsAdmin*/ null,
                    /*BillingEqualsShipping*/ null,
                    /*LastIPAddress*/ null,
                    /*OrderNotes*/ null,
                    /*SubscriptionExpiresOn*/ null,
                    /*RTShipRequest*/ null,
                    /*RTShipResponse*/ null,
                    /*OrderOptions*/ null,
                    /*LocaleSetting*/ null,
                    /*MicroPayBalance*/ null,
                    /*RecurringShippingMethodID*/ null,
                    /*RecurringShippingMethod*/ null,
                    /*BillingAddressID*/ null,
                    /*ShippingAddressID*/ null,
                    /*GiftRegistryGUID*/ null,
                    /*GiftRegistryIsAnonymous*/ null,
                    /*GiftRegistryAllowSearchByOthers*/ null,
                    /*GiftRegistryNickName*/ null,
                    /*GiftRegistryHideShippingAddresses*/ null,
                    /*CODCompanyCheckAllowed*/ null,
                    /*CODNet30Allowed*/ null,
                    /*ExtensionData*/ null,
                    /*FinalizationData*/ null,
                    /*Deleted*/ null,
                    /*Over13Checked*/ null,
                    /*CurrencySetting*/ null,
                    /*VATSetting*/ null,
                    /*VATRegistrationID*/ null,
                    /*StoreCCInDB*/ null,
                    /*IsRegistered*/ null,
                    /*LockedUntil*/ lockeduntil,
                    /*AdminCanViewCC*/ null,
                    /*BadLogin*/ -1,
                    /*Active*/ null,
                    /*PwdChangeRequired*/ 1,
                    /*RegisterDate*/ null,
                    /*StoreId*/null
                 );
                #endregion

                DisplayMessage(false, AppLogic.GetString("admin.customer.passwordResetSuccessful", ThisCustomer.LocaleSetting));

            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Alert);
                DisplayMessage(true, AppLogic.GetString("admin.customer.passwordResetError", ThisCustomer.LocaleSetting));
            }

        }

        /// <summary>
        /// Clears the target customer's session data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnClearSession_OnClick(object sender, EventArgs e)
        {
            if (ViewState["CustomerID"] != null && Convert.ToInt32(ViewState["CustomerID"].ToString()) > 0)
            {
                m_TargetCustomerID = Convert.ToInt32(ViewState["CustomerID"].ToString());
                CustomerSession.StaticClear(m_TargetCustomerID);
                DisplayMessage(false, AppLogic.GetString("admin.customer.CustomerSessionCleared", ThisCustomer.LocaleSetting));
            }
        }

        /// <summary>
        /// Erases all failed transactions linked to the target customer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnClearFailedTransactions_OnClick(object sender, EventArgs e)
        {
            if (ViewState["CustomerID"] != null && Convert.ToInt32(ViewState["CustomerID"].ToString()) > 0)
            {
                m_TargetCustomerID = Convert.ToInt32(ViewState["CustomerID"].ToString());
                Customer.ClearFailedTransactions(m_TargetCustomerID);
                m_TargetCustomer = new Customer(m_TargetCustomerID);
                ltlFailedTransactions.Text = (m_TargetCustomer.FailedTransactionCount <= 0 ? "0" : m_TargetCustomer.FailedTransactionCount.ToString());
                DisplayMessage(false, AppLogic.GetString("admin.customer.FailedTransactionsCleared", ThisCustomer.LocaleSetting));
            }
        }

        /// <summary>
        /// Adds or removes a ban on the target customer's last IP address
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnBlockIP_OnClick(object sender, EventArgs e)
        {
            if (ViewState["CustomerID"] != null && Convert.ToInt32(ViewState["CustomerID"].ToString()) > 0)
            {
                m_TargetCustomerID = Convert.ToInt32(ViewState["CustomerID"].ToString());
                m_TargetCustomer = new Customer(m_TargetCustomerID);
            }
            else
            {
                return;
            }

            if (String.IsNullOrEmpty(m_TargetCustomer.LastIPAddress))
            {
                return;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();

                    m_IPIsBlocked = (DB.GetSqlN("exec [dbo].[aspdnsf_getIPIsRestricted] @IPAddress=" + DB.SQuote(m_TargetCustomer.LastIPAddress), conn) > 0);

                    conn.Close();
                    conn.Dispose();
                }

                if (m_IPIsBlocked)
                {
                    //Removing the IP address ban
                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                    {
                        conn.Open();

                        DB.ExecuteSQL("exec [dbo].[aspdnsf_delRestrictedIP] @IPAddress=" + DB.SQuote(m_TargetCustomer.LastIPAddress));

                        conn.Close();
                        conn.Dispose();
                    }

                    DisplayMessage(false, AppLogic.GetString("admin.customer.IPAddressUnbanned", ThisCustomer.LocaleSetting));
                    btnBlockIP.Text = AppLogic.GetString("admin.customer.BanIP", ThisCustomer.LocaleSetting);
                }
                else
                {
                    //Banning the IP address
                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                    {
                        conn.Open();

                        DB.ExecuteSQL("exec [dbo].[aspdnsf_insRestrictedIP] @IPAddress=" + DB.SQuote(m_TargetCustomer.LastIPAddress), conn);

                        conn.Close();
                        conn.Dispose();
                    }

                    DisplayMessage(false, AppLogic.GetString("admin.customer.IPAddressBanned", ThisCustomer.LocaleSetting));
                    btnBlockIP.Text = AppLogic.GetString("admin.customer.UnBanIP", ThisCustomer.LocaleSetting);
                }
            }
        }

        /// <summary>
        /// Submits the entire form to add a new or update an existing customer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSubmit_OnClick(object sender, EventArgs e)
        {
            bool addingNew = (ViewState["CustomerID"] == null || Convert.ToInt32(ViewState["CustomerID"]) == 0);

            //Programmatically call the password validator since it is in a different validation group
            //Only call IF we are adding a new customer
            if (addingNew)
            {
                Page.Validate("vldPassword");
            }

            if (Page.IsValid)
            {
                if (!addingNew)
                {
                    m_TargetCustomerID = Localization.ParseNativeInt(ViewState["CustomerID"].ToString());
                }

                if (m_TargetCustomerID > 0)
                {
                    //Editing existing customer
                    m_TargetCustomer = new Customer(m_TargetCustomerID);
                    UpdateCustomer();
                }
                else
                {
                    //Adding a new customer
                    CreateNewCustomer();
                }
            }
        }

        #endregion

        #region EventHandlers

        public event EventHandler CustomerAdded;
		public event EventHandler CustomerUpdated;

        /// <summary>
        /// Raises an event that can be caught by a parent page or control when a new customer is created.  The
        /// customerID can be obtained from the sender object.
        /// </summary>
        private void RaiseNewCustomerEvent()
        {
            if (this.CustomerAdded != null)
            {
                this.CustomerAdded(m_TargetCustomerID, new EventArgs());
            }
        }

        /// <summary>
        /// Raises an event that can be caught by a parent page or control when a customer is updated.  The customerID
        /// can be obtained from the sender object
        /// </summary>
        private void RaiseUpdateCustomerEvent()
        {
            if (this.CustomerUpdated != null)
            {
                this.CustomerUpdated(m_TargetCustomerID, new EventArgs());
            }
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Public method that initialized the control for either adding new customers or
        /// editing an existing customer.
        /// Set the TargetCustomerID property to a customerID if you want to edit existing.
        /// </summary>
        public void InitializeControl()
        {
            pnlCustomerEdit.Visible = true;
            pnlAddresses.Visible = false;

            updCustomerEdit.Update();

            InitializeContent();
        }

        /// <summary>
        /// Initials page content, such as populating dropdown lists and setting default values
        /// All of your logic that you normally would expect to fire on page load should go here
        /// Since this is called often times from an AJAX-enabled page, any number of things
        /// could have changed since page load
        /// </summary>
        private void InitializeContent()
        {
            Page.Form.DefaultButton = btnSubmit.UniqueID;

            if (m_TargetCustomerID == 0 && !AddMode)
            {
                if (ViewState["CustomerID"] != null)
                {
                    m_TargetCustomerID = Localization.ParseNativeInt(ViewState["CustomerID"].ToString());
                }
            }
            else
            {
                ViewState.Add("CustomerID", m_TargetCustomerID);
            }

            //bool inEditMode = (m_TargetCustomerID > 0);
            bool inEditMode = !AddMode;

            pnlMessage.Visible = false;
            trVATRegID.Visible = m_VATEnabled;
            txtVATRegID.Visible = m_VATEnabled;

            ddlCustomerLocaleSetting.Items.Clear();
            ddlCustomerAffiliate.Items.Clear();
            ddlCustomerLevel.Items.Clear();

            //Security
            chkCanViewCC.Enabled = ThisCustomer.IsAdminSuperUser;
            txtManualPassword.Text = String.Empty;

            //Populate the affiliate dropdown
            ddlCustomerAffiliate.Items.Add(new ListItem(AppLogic.GetString("admin.common.DDNone", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), "0"));

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("exec [dbo].[aspdnsf_getAffiliateList]", conn))
                {
                    while (rs.Read())
                    {
                        string affiliateName = DB.RSField(rs, "Name");
                        string affiliateID = DB.RSFieldInt(rs, "AffiliateID").ToString();

                        ddlCustomerAffiliate.Items.Add(new ListItem(affiliateName, affiliateID));
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }
            //End Populating Affiliate DropDown

            //Populate the Locale Dropdown
            ddlCustomerLocaleSetting.Items.Add(new ListItem(AppLogic.GetString("admin.common.DDNone", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), "-1"));

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("exec [dbo].[aspdnsf_getLocales]", conn))
                {
                    while (rs.Read())
                    {
                        string localeName = DB.RSField(rs, "Name");
                        string localeID = DB.RSFieldInt(rs, "LocaleSettingID").ToString();

                        ddlCustomerLocaleSetting.Items.Add(new ListItem(localeName, localeID));
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            if (m_TargetCustomerID != 0)
 	 	 	{
                if (null!= ddlCustomerLocaleSetting.Items.FindByText(Localization.GetCustomerLocaleSettingID(m_TargetCustomerID)))
                {
                    ddlCustomerLocaleSetting.Items.FindByText(Localization.GetCustomerLocaleSettingID(m_TargetCustomerID)).Selected = true;
                }
            }
            //End Populating the Locale Dropdown

            //Populate the Customer Level Dropdown
            ddlCustomerLevel.Items.Add(new ListItem(AppLogic.GetString("admin.common.ddnone", ThisCustomer.LocaleSetting), "0"));

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("exec [dbo].[aspdnsf_getCustomerLevels]", conn))
                {
                    while (rs.Read())
                    {
                        string customerLevelID = DB.RSFieldInt(rs, "CustomerLevelID").ToString();
                        string customerLevelName = XmlCommon.GetLocaleEntry(DB.RSField(rs, "Name"), ThisCustomer.LocaleSetting, true);

                        ddlCustomerLevel.Items.Add(new ListItem(customerLevelName, customerLevelID));
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }
            //End Populating the Customer Level Dropdown

            if (inEditMode)
            {
                //Editing an existing customer, make sure all controls are unhidden
                trAssignedToStore.Visible = true;
                trClearSession.Visible = true;
                trCreatedOn.Visible = true;
                trCustomerID.Visible = true;
                trGuid.Visible = true;
                trIP.Visible = true;
                trFailedTransactions.Visible = true;
                trIsRegistered.Visible = true;

                btnRandomPassword.Visible = true;
                btnClearFailedTransactions.Visible = true;
                btnClearSession.Visible = true;
                btnBlockIP.Visible = true;
                btnManualPassword.Visible = true;

                ltlResetPasswordLabel.Text = AppLogic.GetString("admin.customer.ResetPassword", ThisCustomer.LocaleSetting);

                btnSubmit.Text = AppLogic.GetString("admin.customer.UpdateCustomer", ThisCustomer.LocaleSetting);

                pnlAddressEdit.Visible = true;
                //adrlCustomerAddresses.CustomerID = m_TargetCustomerID;

                //Setup the billing and shipping address...
                m_TargetCustomer = new Customer(m_TargetCustomerID);

                trCanViewCC.Visible = ThisCustomer.IsAdminSuperUser && m_TargetCustomer.IsAdminUser;

                Address bAddress = new Address();
                Address sAddress = new Address();

                int bAddressID = m_TargetCustomer.PrimaryBillingAddressID;
                int sAddressID = m_TargetCustomer.PrimaryShippingAddressID;

                if (bAddressID == 0 && sAddressID == 0)
                {
                    //See if the customer has any addresses
                    Addresses addressList = new Addresses();
                    addressList.LoadCustomer(m_TargetCustomer.CustomerID);

                    if (addressList.Count > 0)
                    {
                        bAddressID = addressList[0].AddressID;
                        sAddressID = addressList[0].AddressID;

                        m_TargetCustomer.PrimaryBillingAddressID = bAddressID;
                        m_TargetCustomer.PrimaryShippingAddressID = sAddressID;

                        bAddress = addressList[0];
                        sAddress = addressList[0];


                    }
               
                }

                if (bAddressID != 0)
                {
                    bAddress = m_TargetCustomer.PrimaryBillingAddress;

                    ltlBillingName.Text = String.Format("{0} {1}", bAddress.FirstName, bAddress.LastName);
                    ltlBillingCompany.Text = bAddress.Company;
                    ltlBillingAddress1.Text = bAddress.Address1;
                    ltlBillingAddress2.Text = bAddress.Address2;
                    ltlBillingSuite.Text = bAddress.Suite;
                    ltlBillingCityStateZip.Text = String.Format("{0}, {1} {2}", bAddress.City, bAddress.State, bAddress.Zip);
                    ltlBillingCountry.Text = bAddress.Country;
                    ltlBillingPhone.Text = bAddress.Phone;
                    ltlBillingEmail.Text = bAddress.EMail;

                    //Customer should always have a billing address, but may not have a shipping address
                    if (sAddressID != 0)
                    {
                        sAddress = m_TargetCustomer.PrimaryShippingAddress;
                    }
                    else
                    {
                        //No primary shipping address set.  Use billing.
                        sAddress = bAddress;
                    }

                    ltlShippingName.Text = String.Format("{0} {1}", sAddress.FirstName, sAddress.LastName);
                    ltlShippingCompany.Text = sAddress.Company;
                    ltlShippingAddress1.Text = sAddress.Address1;
                    ltlShippingAddress2.Text = sAddress.Address2;
                    ltlShippingSuite.Text = sAddress.Suite;
                    ltlShippingCityStateZip.Text = String.Format("{0}, {1} {2}", sAddress.City, sAddress.State, sAddress.Zip);
                    ltlShippingCountry.Text = sAddress.Country;
                    ltlShippingPhone.Text = sAddress.Phone;
                    ltlShippingEmail.Text = sAddress.EMail;

                    tblAddresses.Visible = true;
                }
                else
                {
                    //Hide the address table.  This customer has no addresses yet
                    tblAddresses.Visible = false;
                }

                //Load the customer
                LoadCustomer();
            }
            else
            {
                //Adding a new customer, so hide unneeded controls
                trAssignedToStore.Visible = false;
                trClearSession.Visible = false;
                trCreatedOn.Visible = false;
                trCustomerID.Visible = false;
                trGuid.Visible = false;
                trIP.Visible = false;
                trFailedTransactions.Visible = false;
                trIsRegistered.Visible = false;

                btnRandomPassword.Visible = false;
                btnClearFailedTransactions.Visible = false;
                btnClearSession.Visible = false;
                btnBlockIP.Visible = false;
                btnManualPassword.Visible = false;

                pnlAddressEdit.Visible = false;

                // reset the checkboxes to defaults
                chkOver13.Checked = false;
                chkOkToEmail.Checked = false;
                chkCODAllowed.Checked = false;
                chkNetTermsAllowed.Checked = false;

                // reset the text to defaults
                txtFirstName.Text = String.Empty;
                txtLastName.Text = String.Empty;
                txtEmail.Text = String.Empty;
                txtNotes.Text = String.Empty;
                txtPhone.Text = String.Empty;
                txtDOB.Text = String.Empty;
                txtMicroPay.Text = Localization.ParseNativeCurrency("0").ToString();
                

                // reset the dropdowns to defaults
                ddlCustomerAffiliate.ClearSelection();
                ddlCustomerLevel.ClearSelection();
                ddlCustomerLocaleSetting.ClearSelection();

                ltlResetPasswordLabel.Text = AppLogic.GetString("admin.customer.Password", ThisCustomer.LocaleSetting);

                btnSubmit.Text = AppLogic.GetString("admin.customer.CreateNew", ThisCustomer.LocaleSetting);

                try
                {
                    ddlCustomerLocaleSetting.SelectedValue = ddlCustomerLocaleSetting.Items.FindByText(Localization.GetDefaultLocale()).Value;
                    if (m_TargetCustomerID != 0)
 	 	 	        {
 	 	 	            ddlCustomerLocaleSetting.SelectedValue = ddlCustomerLocaleSetting.Items.FindByText(Localization.GetCustomerLocaleSettingID(m_TargetCustomerID)).Value;
 	 	 	        }
                }
                catch { }
            }
        }

        /// <summary>
        /// Displays an information message at the top of the control
        /// </summary>
        /// <param name="isError"></param>
        /// <param name="message"></param>
        private void DisplayMessage(bool isError, String message)
        {
            pnlMessage.Visible = true;

            if (isError)
            {
                pnlMessage.CssClass = "errorMsg";
            }
            else
            {
                pnlMessage.CssClass = "blueNoticeMsg";
            }

            ltlMessage.Text = message;
        }


        /// <summary>
        /// Creates a new customer record form the information supplied in the form
        /// </summary>
        private void CreateNewCustomer()
        {
            if (!Customer.NewEmailPassesDuplicationRules(txtEmail.Text.Trim(), 0, false))
            {
                DisplayMessage(true, AppLogic.GetString("admin.customer.EmailInUse", ThisCustomer.LocaleSetting));
                return;
            }

            bool over13 = chkOver13.Checked;
            bool okToEmail = chkOkToEmail.Checked;
            bool CODAllowed = chkCODAllowed.Checked;
            bool netTermsAllowed = chkNetTermsAllowed.Checked;

            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string localeSetting = String.Empty;
            string email = txtEmail.Text.Trim();
            string clearPassword = txtManualPassword.Text.Trim();
            string notes = txtNotes.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string VATRegID = null;
            string subscriptionExpires = null;
            string dob = null;
            Decimal MicroPayBalance = Decimal.Zero;

            object affiliateID = Convert.ToInt32(ddlCustomerAffiliate.SelectedValue);
            int customerLevelID = Convert.ToInt32(ddlCustomerLevel.SelectedValue);
            int VATSetting;

            bool emailSent = false;

            txtSubscriptionExpires.Culture = new System.Globalization.CultureInfo(ThisCustomer.LocaleSetting); 

            if (txtSubscriptionExpires.SelectedDate.ToString().Trim().Length > 0)
            {
                subscriptionExpires = Localization.DateStringForDB(Localization.ParseLocaleDateTime(txtSubscriptionExpires.SelectedDate.ToString().Trim(), ThisCustomer.LocaleSetting));
            }

            if (txtDOB.Text.Trim().Length > 0)
            {
                dob = Localization.DateStringForDB(Localization.ParseLocaleDateTime(txtDOB.Text.Trim(), ThisCustomer.LocaleSetting));
            }

            if (txtMicroPay.Text.Trim().Length > 0)
            {
                MicroPayBalance = Localization.ParseNativeDecimal(txtMicroPay.Text);
            }

            affiliateID = (affiliateID.ToString() == "0" ? null : affiliateID);
            localeSetting = (ddlCustomerLocaleSetting.SelectedValue != "-1" ? ddlCustomerLocaleSetting.SelectedItem.Text : String.Empty);
            VATSetting = AppLogic.AppConfigUSInt("VAT.DefaultSetting");

            //Create the new customer record and get back the customer ID to instantiate a customer object
            int newCustomerID = Customer.CreateCustomerRecord
                (
                /*email*/           email,
                /*password*/        clearPassword,
                /*SkinID*/          null,
                /*affiliateid*/     affiliateID,
                /*referrer*/        null,
                /*isadmin*/         false,
                /*lastipaddress*/   null,
                /*localesetting*/   localeSetting,
                /*over13*/          over13,
                /*currencysetting*/ null,
                /*VATSetting*/      VATSetting,
                /*vatregid*/        null,
                /*customerlevelid*/ customerLevelID
                );

            m_TargetCustomerID = newCustomerID;

            m_TargetCustomer = new Customer(m_TargetCustomerID);

            Password p = new Password(clearPassword);

            if (m_VATEnabled && txtVATRegID.Text.Trim().Length > 0)
            {
                try
                {
					Exception vatServiceException;
					if (AppLogic.VATRegistrationIDIsValid(m_TargetCustomer, txtVATRegID.Text.Trim(), out vatServiceException))
					{
						VATRegID = txtVATRegID.Text.Trim();
					}
					else
					{
						if (vatServiceException != null && vatServiceException.Message.Length > 0)
							if (vatServiceException.Message.Length > 255)
								DisplayMessage(true, Server.HtmlEncode(vatServiceException.Message.Substring(0, 255)));
							else
								DisplayMessage(true, Server.HtmlEncode(vatServiceException.Message));
						else
							DisplayMessage(true, "account.aspx.91".StringResource());
						
						return;
					}
                }
                catch { }
            }

            //Now update the record with whatever additional info we have
            #region Update customer record
            object lockeduntil = DateTime.Now.AddMinutes(-1);
            m_TargetCustomer.UpdateCustomer(
                /*CustomerLevelID*/ null,
                /*EMail*/ null,
                /*SaltedAndHashedPassword*/ p.SaltedPassword,
                /*SaltKey*/ p.Salt,
                /*DateOfBirth*/ dob,
                /*Gender*/ null,
                /*FirstName*/ firstName,
                /*LastName*/ lastName,
                /*Notes*/ notes,
                /*SkinID*/ null,
                /*Phone*/ phone,
                /*AffiliateID*/ null,
                /*Referrer*/ null,
                /*CouponCode*/ null,
                /*OkToEmail*/ okToEmail,
                /*IsAdmin*/ null,
                /*BillingEqualsShipping*/ null,
                /*LastIPAddress*/ null,
                /*OrderNotes*/ null,
                /*SubscriptionExpiresOn*/ subscriptionExpires,
                /*RTShipRequest*/ null,
                /*RTShipResponse*/ null,
                /*OrderOptions*/ null,
                /*LocaleSetting*/ null,
                /*MicroPayBalance*/ MicroPayBalance,
                /*RecurringShippingMethodID*/ null,
                /*RecurringShippingMethod*/ null,
                /*BillingAddressID*/ null,
                /*ShippingAddressID*/ null,
                /*GiftRegistryGUID*/ null,
                /*GiftRegistryIsAnonymous*/ null,
                /*GiftRegistryAllowSearchByOthers*/ null,
                /*GiftRegistryNickName*/ null,
                /*GiftRegistryHideShippingAddresses*/ null,
                /*CODCompanyCheckAllowed*/ CODAllowed,
                /*CODNet30Allowed*/ netTermsAllowed,
                /*ExtensionData*/ null,
                /*FinalizationData*/ null,
                /*Deleted*/ null,
                /*Over13Checked*/ over13,
                /*CurrencySetting*/ null,
                /*VATSetting*/ null,
                /*VATRegistrationID*/ VATRegID,
                /*StoreCCInDB*/ null,
                /*IsRegistered*/ true,
                /*LockedUntil*/ lockeduntil,
                /*AdminCanViewCC*/ null,
                /*BadLogin*/ -1,
                /*Active*/ null,
                /*PwdChangeRequired*/ 1,
                /*RegisterDate*/ null,
                /*StoreId*/null
             );
            #endregion

            Security.LogEvent(AppLogic.GetString("admin.cst_account_process.event.AdminResetCustomerPassword", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), "", m_TargetCustomer.CustomerID, ThisCustomer.CustomerID, Convert.ToInt32(ThisCustomer.CurrentSessionID));

            //Send the customer an email with their password
            try
            {
                AppLogic.SendMail(AppLogic.AppConfig("StoreName") + " - " + AppLogic.GetString("cst_account_process.aspx.1", m_TargetCustomer.SkinID, m_TargetCustomer.LocaleSetting), AppLogic.RunXmlPackage("notification.lostpassword.xml.config", null, m_TargetCustomer, m_TargetCustomer.SkinID, "", "thiscustomerid=" + m_TargetCustomer.CustomerID.ToString() + "&newpwd=" + p.ClearPassword, false, false), true, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromName"), m_TargetCustomer.EMail, m_TargetCustomer.FullName(), "", "", AppLogic.MailServer());
                emailSent = true;
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Alert);
            }

            //Reinitialize the content with the new customer
            //Stash the new customerID in admin page base so when the control posts back we can rebuild the customer
            ViewState["CustomerID"] = newCustomerID;

            // call InitializeContent before DisplayMessage to keep the message from being hidden
            InitializeContent();

            if (emailSent)
            {
                DisplayMessage(false, AppLogic.GetString("admin.customer.CustomerCreated", ThisCustomer.LocaleSetting));
            }
            else
            {
                DisplayMessage(false, AppLogic.GetString("admin.customer.CustomerCreatedEmailFailed", ThisCustomer.LocaleSetting));
            }

            RaiseNewCustomerEvent();

        }


        /// <summary>
        /// Updates an existing customer with the information supplied in the form
        /// </summary>
        private void UpdateCustomer()
        {
            bool over13 = chkOver13.Checked;
            bool okToEmail = chkOkToEmail.Checked;
            bool CODAllowed = chkCODAllowed.Checked;
            bool netTermsAllowed = chkNetTermsAllowed.Checked;

            int CurrentStore = ssOne.SelectedStoreID;
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string localeSetting = CommonLogic.IIF(ddlCustomerLocaleSetting.SelectedValue != "-1", ddlCustomerLocaleSetting.SelectedItem.Text, String.Empty);
            string email = txtEmail.Text.Trim();
            string notes = txtNotes.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string VATRegID = null;
            string subscriptionExpires = null;
            string dob = null;
            Decimal MicroPayBalance = Decimal.Zero;

            object affiliateID = Convert.ToInt32(ddlCustomerAffiliate.SelectedValue);
            int skinID = AppLogic.GetStoreSkinID(AppLogic.StoreID());
            int customerLevelID = Convert.ToInt32(ddlCustomerLevel.SelectedValue);

            object lockeduntil = null;

            affiliateID = (affiliateID.ToString() == "0" ? null : affiliateID);
            localeSetting = (ddlCustomerLocaleSetting.SelectedValue != "-1" ? ddlCustomerLocaleSetting.SelectedItem.Text : String.Empty);

            if (txtSubscriptionExpires.SelectedDate.ToString().Trim().Length > 0)
            {
                subscriptionExpires = Localization.DateStringForDB(Localization.ParseLocaleDateTime(txtSubscriptionExpires.SelectedDate.ToString().Trim(), ThisCustomer.LocaleSetting));
            }

            if (txtDOB.Text.Trim().Length > 0)
            {
                dob = Localization.DateStringForDB(Localization.ParseLocaleDateTime(txtDOB.Text.Trim(), ThisCustomer.LocaleSetting));
            }

            if (txtMicroPay.Text.Trim().Length > 0)
            {
                MicroPayBalance = Localization.ParseNativeDecimal(txtMicroPay.Text);
            }

			if (m_VATEnabled && txtVATRegID.Text.Trim().Length > 0)
			{
				try
				{
					Exception vatServiceException;
					if (AppLogic.VATRegistrationIDIsValid(m_TargetCustomer, txtVATRegID.Text.Trim(), out vatServiceException))
					{
						VATRegID = txtVATRegID.Text.Trim();
					}
					else
					{
						if (vatServiceException != null && vatServiceException.Message.Length > 0)
							if (vatServiceException.Message.Length > 255)
								DisplayMessage(true, Server.HtmlEncode(vatServiceException.Message.Substring(0, 255)));
							else
								DisplayMessage(true, Server.HtmlEncode(vatServiceException.Message));
						else
							DisplayMessage(true, "account.aspx.91".StringResource());

						return;
					}
				}
				catch { }
			}

            #region Update customer record
            if (!chkAccountLocked.Checked)
            {
                lockeduntil = DateTime.Now.AddMinutes(-1);
            }
            else
            {
                lockeduntil = DateTime.MaxValue;
            }

            m_TargetCustomer.UpdateCustomer(
                /*CustomerLevelID*/ ddlCustomerLevel.SelectedValue,
                /*EMail*/ email,
                /*SaltedAndHashedPassword*/ null,
                /*SaltKey*/ null,
                /*DateOfBirth*/ dob,
                /*Gender*/ null,
                /*FirstName*/ firstName,
                /*LastName*/ lastName,
                /*Notes*/ notes,
                /*SkinID*/ null,
                /*Phone*/ phone,
                /*AffiliateID*/ affiliateID,
                /*Referrer*/ null,
                /*CouponCode*/ null,
                /*OkToEmail*/ okToEmail,
                /*IsAdmin*/ null,
                /*BillingEqualsShipping*/ null,
                /*LastIPAddress*/ null,
                /*OrderNotes*/ null,
                /*SubscriptionExpiresOn*/ subscriptionExpires,
                /*RTShipRequest*/ null,
                /*RTShipResponse*/ null,
                /*OrderOptions*/ null,
                /*LocaleSetting*/ localeSetting,
                /*MicroPayBalance*/ MicroPayBalance,
                /*RecurringShippingMethodID*/ null,
                /*RecurringShippingMethod*/ null,
                /*BillingAddressID*/ null,
                /*ShippingAddressID*/ null,
                /*GiftRegistryGUID*/ null,
                /*GiftRegistryIsAnonymous*/ null,
                /*GiftRegistryAllowSearchByOthers*/ null,
                /*GiftRegistryNickName*/ null,
                /*GiftRegistryHideShippingAddresses*/ null,
                /*CODCompanyCheckAllowed*/ CODAllowed,
                /*CODNet30Allowed*/ netTermsAllowed,
                /*ExtensionData*/ null,
                /*FinalizationData*/ null,
                /*Deleted*/ null,
                /*Over13Checked*/ over13,
                /*CurrencySetting*/ null,
                /*VATSetting*/ null,
                /*VATRegistrationID*/ VATRegID,
                /*StoreCCInDB*/ null,
                /*IsRegistered*/ null,
                /*LockedUntil*/ lockeduntil,
                /*AdminCanViewCC*/ chkCanViewCC.Checked,
                /*BadLogin*/ -1,
                /*Active*/ null,
                /*PwdChangeRequired*/ null,
                /*RegisterDate*/ null,
                /*StoreId*/ CurrentStore
             );
            #endregion

            //Reload the customer after updating
            //LoadCustomer();

            RaiseUpdateCustomerEvent();
            DisplayMessage(false, AppLogic.GetString("admin.customer.CustomerUpdated", ThisCustomer.LocaleSetting));

        }


        /// <summary>
        /// If editing an existing customer, pre-populates all fields with the existing values for the target customer
        /// </summary>
        public void LoadCustomer()
        {
            ssOne.SelectedStoreID = m_TargetCustomer.StoreID;
            ltlCustomerID.Text = m_TargetCustomer.CustomerID.ToString();
            ltlCreatedOn.Text = Localization.ToThreadCultureShortDateString(m_TargetCustomer.CreatedOn);
            ltlCustomerGuid.Text = m_TargetCustomer.CustomerGUID;
            ltlIPAddress.Text = m_TargetCustomer.LastIPAddress;
            ltlFailedTransactions.Text = (m_TargetCustomer.FailedTransactionCount <= 0 ? "0" : m_TargetCustomer.FailedTransactionCount.ToString());
            //ltlCustomerStore.Text = m_TargetCustomer.StoreName;

            if (m_TargetCustomer.IsRegistered)
            {
                ltlIsRegistered.Text = AppLogic.GetString("admin.common.yes", ThisCustomer.LocaleSetting);
            }
            else
            {
                ltlIsRegistered.Text = AppLogic.GetString("admin.common.no", ThisCustomer.LocaleSetting);
            }

            txtFirstName.Text = m_TargetCustomer.FirstName;
            txtLastName.Text = m_TargetCustomer.LastName;
            txtEmail.Text = m_TargetCustomer.EMail;

            if (m_TargetCustomer.SubscriptionExpiresOn > System.DateTime.MinValue)
            {
                txtSubscriptionExpires.SelectedDate = m_TargetCustomer.SubscriptionExpiresOn;
            }
            else
            {
                txtSubscriptionExpires.SelectedDate = null;
            }

            txtPhone.Text = m_TargetCustomer.Phone;
            txtDOB.Text = Localization.ToThreadCultureShortDateString(m_TargetCustomer.DateOfBirth);
            txtMicroPay.Text = Localization.ParseNativeCurrency(m_TargetCustomer.MicroPayBalance.ToString()).ToString();
            txtNotes.Text = Security.HtmlEncode(m_TargetCustomer.Notes);

            chkAccountLocked.Checked = (m_TargetCustomer.LockedUntil > System.DateTime.Now);
            chkOver13.Checked = m_TargetCustomer.IsOver13;
            chkCanViewCC.Checked = (m_TargetCustomer.IsAdminUser && m_TargetCustomer.AdminCanViewCC);
            chkOkToEmail.Checked = m_TargetCustomer.OKToEMail;
            chkCODAllowed.Checked = m_TargetCustomer.CODCompanyCheckAllowed;
            chkNetTermsAllowed.Checked = m_TargetCustomer.CODNet30Allowed;

            if (m_TargetCustomer.AffiliateID != 0)
            {
                try
                {
                    ddlCustomerAffiliate.SelectedValue = ddlCustomerAffiliate.Items.FindByValue(m_TargetCustomer.AffiliateID.ToString()).Value;
                }
                catch
                {
                    //set to none - affiliate may have been deleted, etc.
                    ddlCustomerAffiliate.SelectedValue = "-1";
                }
            }

            if (!String.IsNullOrEmpty(m_TargetCustomer.LocaleSetting))
            {
                try
                {
                    ddlCustomerLocaleSetting.SelectedValue = ddlCustomerLocaleSetting.Items.FindByText(Localization.GetCustomerLocaleSettingID(m_TargetCustomerID)).Value;
                }
                catch
                {
                    //set to none - locale may have been deleted, etc.
                    ddlCustomerLocaleSetting.SelectedValue = "-1";
                }
            }

            if (m_TargetCustomer.CustomerLevelID != 0)
            {
                try
                {
                    ddlCustomerLevel.SelectedValue = m_TargetCustomer.CustomerLevelID.ToString();
                }
                catch
                {
                    //set to none - Customer level may have been deleted, etc.
                    ddlCustomerLevel.SelectedValue = "0";
                }
            }

            //See if the IP Is Blocked
            if (!String.IsNullOrEmpty(m_TargetCustomer.LastIPAddress))
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();

                    m_IPIsBlocked = (DB.GetSqlN("exec [dbo].[aspdnsf_getIPIsRestricted] @IPAddress=" + DB.SQuote(m_TargetCustomer.LastIPAddress), conn) > 0);

                    conn.Close();
                    conn.Dispose();
                }

                if (m_IPIsBlocked)
                {
                    btnBlockIP.Text = AppLogic.GetString("admin.customer.UnBanIP", ThisCustomer.LocaleSetting);
                    cbeBlockIP.ConfirmText = AppLogic.GetString("admin.customer.ConfirmUnBanIP", ThisCustomer.LocaleSetting);
                }
                else
                {
                    btnBlockIP.Text = AppLogic.GetString("admin.customer.BanIP", ThisCustomer.LocaleSetting);
                    cbeBlockIP.ConfirmText = AppLogic.GetString("admin.customer.ConfirmBanIP", ThisCustomer.LocaleSetting);
                }
            }
            else
            {
                btnBlockIP.Visible = false;
            }

        }



        #endregion

        #region Public Properties


        public int TargetCustomerID
        {
            get { return m_TargetCustomerID; }
            set { m_TargetCustomerID = value; }
        }

        #endregion


    }
}
