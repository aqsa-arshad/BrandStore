using AspDotNetStorefrontCore;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Class JeldWenMasterPage.
    /// </summary>
    public partial class JeldWenMasterPage : MasterPageBase
    {
        /// <summary>
        /// The m_ this customer
        /// </summary>
        private Customer m_ThisCustomer;
        /// <summary>
        /// Gets or sets the this customer.
        /// </summary>
        /// <value>The this customer.</value>
        public Customer ThisCustomer
        {
            get
            {
                if (m_ThisCustomer == null)
                    m_ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

                return m_ThisCustomer;
            }
            set
            {
                m_ThisCustomer = value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            if (this.RequireScriptManager)
            {
                // provide hookup for individual pages
                (this.Page as SkinBase).RegisterScriptAndServices(scrptMgr);
            }
            base.OnInit(e);
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            ShoppingCart Shoppingcart = new ShoppingCart(3, ThisCustomer, 0, 0, false);
            if (Shoppingcart != null)
            {
                if (Shoppingcart.CartItems.Count > 0)
                {
                    int quantity = 0;
                    foreach(CartItem citem in Shoppingcart.CartItems)
                    {
                        quantity = quantity + citem.Quantity;
                    }
                    shopping_cart.InnerText = "SHOPPING CART - " + quantity.ToString() + " -";
                }
                else
                    shopping_cart.InnerText = "SHOPPING CART";
            }

            if (!Page.IsPostBack)
            {
                SetPageHeading();
                if (ThisCustomer.IsRegistered)
                {
                    ShowPostLoginControls();
                    GetUnreadCustomerAlertCount();
                    this.hdnCustomerLevel.Text = ThisCustomer.CustomerLevelID.ToString();
                }
                else
                {
                    ShowPreLoginControls();
                    hdnCustomerLevel.Text = "-1";
                }
            }
        }
        private void GetUnreadCustomerAlertCount()
        {
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_CustomerAlertUnreadAlertCount", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustomerID", ThisCustomer.CustomerID);
                        cmd.Parameters.AddWithValue("@CustomerLevelID", ThisCustomer.CustomerLevelID);
                        cmd.Parameters.AddWithValue("@AlertDate", DateTime.Now);
                        Int32 AlertCount = (Int32)cmd.ExecuteScalar();

                        if (AlertCount > 0)
                        {
                            lnkAlertDesktop.Attributes.Add("class", "new-alerts");
                            lblAlertCount.InnerHtml = "ALERTS" + " - " + AlertCount.ToString() + " - ";
                        }
                        else
                        {
                            lnkAlertDesktop.Attributes.Add("class", "alerts-link");
                            lblAlertCount.InnerHtml = "ALERTS";
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }
        protected void ServerButton_Click(object sender, EventArgs e)
        {
            GetUnreadCustomerAlertCount();
            GetCustomerAlerts();
            popuppanel.Visible = true;
            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "preventLoad()", true);
        }
        private void GetCustomerAlerts()
        {
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_CustomerAlertStatusSelectByCustomerIDAlertDate", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustomerID", ThisCustomer.CustomerID);
                        cmd.Parameters.AddWithValue("@CustomerLevelID", ThisCustomer.CustomerLevelID);
                        cmd.Parameters.AddWithValue("@AlertDate", DateTime.Now);

                        IDataReader idr = cmd.ExecuteReader();
                        if ((((System.Data.Common.DbDataReader)idr).HasRows))
                        {
                            rptCustomerAlerts.DataSource = idr;
                            rptCustomerAlerts.DataBind();
                            ulCustomerAlertNotification.Visible = false;
                        }
                        else
                        {
                            rptCustomerAlerts.DataSource = null;
                            rptCustomerAlerts.DataBind();
                            ulCustomerAlertNotification.Visible = true;
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }

        /// <summary>
        /// Repeater ItemCommand Event
        /// </summary>
        protected void rptCustomerAlerts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            try
            {
                int customerAlertStatusID = Convert.ToInt32(e.CommandArgument);
                const string readSP = "aspdnsf_CustomerAlertStatusRead";
                const string deleteSP = "aspdnsf_CustomerAlertStatusDelete";

                if (e.CommandName == "Delete")
                {
                    UpdateCustomerAlert(customerAlertStatusID, deleteSP);
                }
                else if (e.CommandName == "Read")
                {
                    UpdateCustomerAlert(customerAlertStatusID, readSP);
                }
                GetCustomerAlerts();
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }

        /// <summary>
        /// Update Customer Alert
        /// </summary>
        /// <param name="customerAlertStatusID">customerAlertStatusID</param>
        /// <param name="spName">spName</param>
        private void UpdateCustomerAlert(int customerAlertStatusID, string spName)
        {
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(spName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustomerAlertStatusID", customerAlertStatusID);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }

        }

        /// <summary>
        /// Shows the post login controls.
        /// </summary>
        private void ShowPostLoginControls()
        {
            divbeforelogin.Visible = false;
            divafterlogin.Visible = true;
        }

        /// <summary>
        /// Shows the pre login controls.
        /// </summary>
        private void ShowPreLoginControls()
        {
            divbeforelogin.Visible = true;
            divafterlogin.Visible = false;
        }

        /// <summary>
        /// Sets the page heading.
        /// </summary>
        private void SetPageHeading()
        {
            var currentURL = Request.Url.AbsolutePath;
            if (currentURL.ToUpper().Contains("HOME"))
            {
                lblPageHeading.Text = string.Empty;
                pnlPageHeading.Visible = false;
            }
            else if (currentURL.ToUpper().Contains("SEARCH"))
            {
                lblPageHeading.Text = "SEARCH RESULTS";
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("JWMYACCOUNT"))
            {
                lblPageHeading.Text = "MY ACCOUNT: " + ThisCustomer.FullName();
                pnlPageHeading.Visible = true;

                lnkMyAccount.Attributes.Add("class", "active account-link");
                if (ThisCustomer.CustomerLevelID == 8)
                {
                    var newClassValue = JWBPublicUserAfterLoginControl.Attributes["class"].Replace("hide-element", "");
                    JWBPublicUserAfterLoginControl.Attributes.Remove("class");
                    JWBPublicUserAfterLoginControl.Attributes.Add("class", newClassValue);
                    JWBUserInfoAfterLoginControl.Visible = false;
                }
            }
            else if (currentURL.ToUpper().Contains("JWMYADDRESSES"))
            {
                // Label will be loaded from Content Page w.r.t AddressType in QueryString
                pnlPageHeading.Visible = true;
                lnkMyAccount.Attributes.Add("class", "active account-link");
            }
            else if (currentURL.ToUpper().Contains("JWADDADDRESSES"))
            {
                lblPageHeading.Text = "ADD/EDIT ADDRESS";
                pnlPageHeading.Visible = true;
                lnkMyAccount.Attributes.Add("class", "active account-link");
            }

            else if (currentURL.ToUpper().Contains("SIGNIN"))
            {
                divlogin.Visible = false;
                separatorafterlogin.Visible = false;
                pnlPageHeading.Attributes["class"] = "hide-element";
                pnlPageHeading.Visible = true;
                divSideBarBeforeLogin.Visible = false;
                divSideBarAfterLogin.Visible = false;
               // lnkShoppingCart.Attributes.Add("class", "active shopping-link");
                //divcontentarea.Attributes["class"] = "col-md-12";
            }

            else if (currentURL.ToUpper().Contains("CREATEACCOUNT"))
            {
                lblPageHeading.Text = "CREATE MY ACCOUNT";
                pnlPageHeading.Visible = true;
                //divSideBarBeforeLogin.Visible = false;
                //divSideBarAfterLogin.Visible = false;
            }
            else if (currentURL.ToUpper().Contains("MARKETINGSERVICESDETAIL"))
            {
                lblPageHeading.Text = "About Marketing Services";
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("C-"))
            {
                // Label & Back Link will be loaded from Content Page w.r.t Category in QueryString
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("P-"))
            {
                // Label & Back Link will be loaded from Content Page w.r.t Category &  Sub-Category in QueryString
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("JWSUPPORT"))
            {
                lblPageHeading.Text = "Support";
                pnlPageHeading.Visible = true;
                divbeforelogin.Visible = false;
                divafterlogin.Visible = false;
                divSideBarBeforeLogin.Visible = false;
                divSideBarAfterLogin.Visible = false;
                divJWsupport.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("JWABOUTTRUEBLU"))
            {
                lblPageHeading.Text = "ABOUT True BLU™";
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("DOWNLOADS"))
            {
                lblPageHeading.Text = AppLogic.GetString("download.aspx.1", 3, ThisCustomer.LocaleSetting);
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("JWTERMSANDCONDITIONS"))
            {
                lblPageHeading.Text = "Terms and Privacy Policy";
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("ORDERHISTORY"))
            {
                if (ThisCustomer.CustomerLevelID == 4 || ThisCustomer.CustomerLevelID == 5 ||
                    ThisCustomer.CustomerLevelID == 6)
                {
                    //For Dealer lblPageHeading will be set in OrderHistory Page
                }
                else
                {
                    lblPageHeading.Text = "ORDER HISTORY";
                }
                lnkMyAccount.Attributes.Add("class", "active account-link");
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("ORDERDETAIL"))
            {
                if (ThisCustomer.CustomerLevelID == 4 || ThisCustomer.CustomerLevelID == 5 ||
                    ThisCustomer.CustomerLevelID == 6)
                {
                    //For Dealer lblPageHeading will be set in OrderHistory Page
                }
                else
                {
                    lblPageHeading.Text = "ORDER DETAIL";
                }
                lnkMyAccount.Attributes.Add("class", "active account-link");
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("SHOPPINGCART"))
            {
                lblPageHeading.Text = "SHOPPING CART";

                pnlPageHeading.Visible = true;
                divSideBarBeforeLogin.Visible = false;
                divSideBarAfterLogin.Visible = false;
                lnkShoppingCart.Attributes.Add("class", "active shopping-link");
                divcontentarea.Attributes["class"] = "col-md-12";
            }
            else if (currentURL.ToUpper().Contains("CHECKOUTSHIPPING"))
            {
                lblPageHeading.Text = "SHIPPING OPTIONS";
                pnlPageHeading.Attributes["class"] = "hide-element";
                pnlPageHeading.Visible = true;
                divSideBarBeforeLogin.Visible = false;
                divSideBarAfterLogin.Visible = false;
                lnkShoppingCart.Attributes.Add("class", "active shopping-link");
                divcontentarea.Attributes["class"] = "col-md-12";
            }
            else if (currentURL.ToUpper().Contains("CHECKOUTPAYMENT"))
            {
                pnlPageHeading.Attributes["class"] = "hide-element";
                pnlPageHeading.Visible = true;
                divSideBarBeforeLogin.Visible = false;
                divSideBarAfterLogin.Visible = false;
                lnkShoppingCart.Attributes.Add("class", "active shopping-link");
                divcontentarea.Attributes["class"] = "col-md-12";
            }
            else if (currentURL.ToUpper().Contains("CHECKOUTREVIEW"))
            {
                pnlPageHeading.Attributes["class"] = "hide-element";
                pnlPageHeading.Visible = true;
                divSideBarBeforeLogin.Visible = false;
                divSideBarAfterLogin.Visible = false;
                lnkShoppingCart.Attributes.Add("class", "active shopping-link");
                divcontentarea.Attributes["class"] = "col-md-12";
            }
            else if (currentURL.ToUpper().Contains("ORDERCONFIRMATION"))
            {
                pnlPageHeading.Attributes["class"] = "hide-element";
                pnlPageHeading.Visible = true;
                divSideBarBeforeLogin.Visible = false;
                divSideBarAfterLogin.Visible = false;
                lnkShoppingCart.Attributes.Add("class", "active shopping-link");
                divcontentarea.Attributes["class"] = "col-md-12";
            }
            else if (currentURL.ToUpper().Contains("ACCOUNT"))
            {
                pnlPageHeading.Attributes["class"] = "hide-element";
                pnlPageHeading.Visible = true;
                divSideBarBeforeLogin.Visible = false;
                divSideBarAfterLogin.Visible = false;
                lnkShoppingCart.Attributes.Add("class", "active shopping-link");
                divcontentarea.Attributes["class"] = "col-md-12";
            }
            else
            {
                lblPageHeading.Text = string.Empty;
                pnlPageHeading.Visible = false;
            }
        }
    }
}