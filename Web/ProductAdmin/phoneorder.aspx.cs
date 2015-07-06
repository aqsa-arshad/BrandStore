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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{

    public partial class phoneorder : AdminPageBase
    {
        private int m_OrderNumber = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            m_OrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");
            GetJavaScriptFunctions();

            if (!IsPostBack)
            {
                saveOrderNumber.Text = m_OrderNumber.ToString();
                if (AppLogic.AppConfigBool("PhoneOrder.EMailIsOptional"))
                {
                    valRegExValEmail.Enabled = false;
                }
                BillingState.ClearSelection();
                BillingState.Items.Clear();
                ShippingState.ClearSelection();
                ShippingState.Items.Clear();

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("select * from State  with (NOLOCK)  order by DisplayOrder,Name", conn))
                    {
                        ShippingState.DataValueField = "Abbreviation";
                        ShippingState.DataTextField = "Name";
                        ShippingState.DataSource = rs;
                        ShippingState.DataBind();
                    }
                }

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("select * from State  with (NOLOCK)  order by DisplayOrder,Name", conn))
                    {
                        BillingState.DataValueField = "Abbreviation";
                        BillingState.DataTextField = "Name";
                        BillingState.DataSource = rs;
                        BillingState.DataBind();
                    }
                }

                ShippingCountry.ClearSelection();
                ShippingCountry.Items.Clear();
                BillingCountry.ClearSelection();
                BillingCountry.Items.Clear();

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("select * from Country  with (NOLOCK)  order by DisplayOrder,Name", conn))
                    {
                        ShippingCountry.DataValueField = "Name";
                        ShippingCountry.DataTextField = "Name";
                        ShippingCountry.DataSource = rs;
                        ShippingCountry.DataBind();
                    }
                }

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("select * from Country  with (NOLOCK)  order by DisplayOrder,Name", conn))
                    {
                        BillingCountry.DataValueField = "Name";
                        BillingCountry.DataTextField = "Name";
                        BillingCountry.DataSource = rs;
                        BillingCountry.DataBind();
                    }
                }

                AffiliateList.ClearSelection();
                AffiliateList.Items.Clear();

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("select AffiliateID,Name,DisplayOrder from Affiliate  with (NOLOCK)  union select 0,'--N/A--', 0 from Affiliate  with (NOLOCK)  order by DisplayOrder,Name", conn))
                    {
                        AffiliateList.DataSource = rs;
                        AffiliateList.DataBind();
                        if (AffiliateList.Items.Count == 0)
                        {
                            AffiliateList.Visible = false;
                            AffiliatePrompt.Visible = false;
                        }
                    }
                }

                CustomerLevelList.ClearSelection();
                CustomerLevelList.Items.Clear();

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader dr = DB.GetRS("select CustomerLevelID,Name,DisplayOrder from CustomerLevel  with (NOLOCK)  union select 0,'--N/A--', 0 from CustomerLevel  with (NOLOCK)  order by DisplayOrder,Name", conn))
                    {
                        while (dr.Read())
                        {
                            CustomerLevelList.Items.Add(new ListItem(DB.RSFieldByLocale(dr, "Name", LocaleSetting), DB.RSFieldInt(dr, "CustomerLevelID").ToString()));
                        }
                    }
                }

                ThisCustomer.ThisCustomerSession.ClearVal("IGD_EDITINGORDER");
                // are we order editing?
                if (m_OrderNumber != 0)
                {
                    ThisCustomer.ThisCustomerSession["IGD_EDITINGORDER"] = m_OrderNumber.ToString();

                    // setup cart to match order:
                    DB.ExecuteSQL("aspdnsf_EditOrder " + m_OrderNumber.ToString());

                    Button89.Visible = true;
                    Button90.Visible = true;
                    Button91.Visible = true;
                    helpphone.Visible = false;
                    helporderedit.Visible = true;
                    backtoorderlink.NavigateUrl = AppLogic.AdminLinkUrl("orders.aspx") + "?ordernumber=" + m_OrderNumber.ToString();
                    content.Visible = false;
                    SearchCustomerPanel.Visible = false;
                    txtordernumber.Text = m_OrderNumber.ToString();
                    Order ord = new Order(m_OrderNumber);

                    // use customer as is:
                    TopPanel.Visible = false;
                    SearchCustomerPanel.Visible = false;
                    CreateNewCustomerPanel.Visible = false;
                    CustomerStatsPanel.Visible = true;
                    CreateCustomer.Visible = false;
                    UseCustomer.Visible = false;
                    UpdateCustomer.Visible = false;

                    SetContextToCustomerID(ord.CustomerID);

                    UsingCustomerID.Text = CustomerID.Text;
                    UsingFirstName.Text = FirstName.Text;
                    UsingLastName.Text = LastName.Text;
                    UsingEMail.Text = EMail.Text.ToLowerInvariant().Trim();

                    SetToImpersonationPageContext(ord.CustomerID, "../shoppingcart.aspx", false);
                }
            }
            else
            {
                m_OrderNumber = System.Int32.Parse(saveOrderNumber.Text);
            }

            bool RequireEmail = !(AppLogic.AppConfigBool("PasswordIsOptionalDuringCheckout") && !AppLogic.AppConfigBool("AnonCheckoutReqEmail"));
            RequiredEmailValidator.Enabled = RequireEmail;
            lblRequiredEmailAsterix.Visible = RequireEmail;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            // create new customer
            CreateNewCustomerPanel.Visible = true;
            SearchCustomerPanel.Visible = false;

            SetContextToNewCustomer();
        }

        protected void Button2_Click1(object sender, EventArgs e)
        {
            SearchCustomerPanel.Visible = true;
            CreateNewCustomerPanel.Visible = false;

            String sql = "select top 50 CustomerID, FirstName, LastName, EMail from Customer  with (NOLOCK)  where deleted=0 and IsAdmin=0 and (convert(nvarchar(10),CustomerID)=" + DB.SQuote(TextBox1.Text.Trim()) + " or FirstName like " + DB.SQuote("%" + TextBox1.Text.Trim() + "%") + " or LastName like " + DB.SQuote("%" + TextBox1.Text.Trim() + "%") + " or CustomerID in (select CustomerID from Orders where convert(nvarchar(10),OrderNumber)=" + DB.SQuote(TextBox1.Text.Trim()) + ") or EMail like " + DB.SQuote("%" + TextBox1.Text.Trim() + "%") + ")";
            SQLText.Text = sql;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(sql, conn))
                {
                    GridView1.DataSource = rs;
                    GridView1.DataBind();
                }
            }
        }

        protected void SetContextToCustomer(int RowID)
        {
            SetContextToNewCustomer();
            DataKey data = GridView1.DataKeys[RowID];
            int iCustomerID = (int)data.Values["CustomerID"];
            EMailAlreadyTaken.Visible = false;
            SearchCustomerPanel.Visible = false;
            CustomerIDPanel.Visible = true;
            CreateNewCustomerPanel.Visible = true;
            SetContextToCustomerID(iCustomerID);
            CreateCustomer.Visible = false;
            UseCustomer.Visible = true;
            UpdateCustomer.Visible = true;
        }

        protected void SetContextToCustomerID(int iCustomerID)
        {
            CustomerID.Text = iCustomerID.ToString();
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from Customer  with (NOLOCK)  where deleted=0 and IsAdmin=0 and CustomerID=" + iCustomerID.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        FirstName.Text = DB.RSField(rs, "FirstName");
                        LastName.Text = DB.RSField(rs, "LastName");
                        EMail.Text = DB.RSField(rs, "EMail").ToLowerInvariant().Trim();
                        Phone.Text = DB.RSField(rs, "Phone");
                        Over13.Checked = DB.RSFieldBool(rs, "Over13Checked");
                        RadioButtonList1.SelectedIndex = CommonLogic.IIF(DB.RSFieldBool(rs, "OkToEMail"), 0, 1);

                        Address BillingAddress = new Address();
                        BillingAddress.LoadByCustomer(iCustomerID, AddressTypes.Billing);
                        BillingFirstName.Text = BillingAddress.FirstName;
                        BillingLastName.Text = BillingAddress.LastName;
                        BillingPhone.Text = BillingAddress.Phone;
                        BillingCompany.Text = BillingAddress.Company;
                        try
                        {
                            BillingResidenceType.ClearSelection();
                            if (BillingAddress.ResidenceType != ResidenceTypes.Unknown)
                            {
                                BillingResidenceType.Items.FindByValue(((int)BillingAddress.ResidenceType).ToString()).Selected = true;
                            }
                        }
                        catch { }
                        BillingAddress1.Text = BillingAddress.Address1;
                        BillingAddress2.Text = BillingAddress.Address2;
                        BillingSuite.Text = BillingAddress.Suite;
                        BillingCity.Text = BillingAddress.City;
                        try
                        {
                            BillingState.SelectedIndex = -1;
                            BillingState.ClearSelection();
                            BillingState.Items.FindByValue(BillingAddress.State).Selected = true;
                        }
                        catch { }
                        BillingZip.Text = BillingAddress.Zip;
                        try
                        {
                            BillingCountry.SelectedIndex = -1;
                            BillingCountry.ClearSelection();
                            BillingCountry.Items.FindByValue(BillingAddress.Country).Selected = true;
                        }
                        catch { }

                        Address ShippingAddress = new Address();
                        ShippingAddress.LoadByCustomer(iCustomerID, AddressTypes.Shipping);
                        ShippingFirstName.Text = ShippingAddress.FirstName;
                        ShippingLastName.Text = ShippingAddress.LastName;
                        ShippingPhone.Text = ShippingAddress.Phone;
                        ShippingCompany.Text = ShippingAddress.Company;
                        try
                        {
                            ShippingResidenceType.ClearSelection();
                            if (ShippingAddress.ResidenceType != ResidenceTypes.Unknown)
                            {
                                ShippingResidenceType.Items.FindByValue(((int)ShippingAddress.ResidenceType).ToString()).Selected = true;
                            }
                        }
                        catch { }
                        ShippingAddress1.Text = ShippingAddress.Address1;
                        ShippingAddress2.Text = ShippingAddress.Address2;
                        ShippingSuite.Text = ShippingAddress.Suite;
                        ShippingCity.Text = ShippingAddress.City;
                        try
                        {
                            ShippingState.SelectedIndex = -1;
                            ShippingState.ClearSelection();
                            ShippingState.Items.FindByValue(ShippingAddress.State).Selected = true;
                        }
                        catch { }
                        ShippingZip.Text = ShippingAddress.Zip;
                        try
                        {
                            ShippingCountry.SelectedIndex = -1;
                            ShippingCountry.ClearSelection();
                            ShippingCountry.Items.FindByValue(ShippingAddress.Country).Selected = true;
                        }
                        catch { }
                        try
                        {
                            AffiliateList.SelectedIndex = -1;
                            AffiliateList.ClearSelection();
                            AffiliateList.Items.FindByValue(DB.RSFieldInt(rs, "AffiliateID").ToString()).Selected = true;
                        }
                        catch { }
                        try
                        {
                            CustomerLevelList.SelectedIndex = -1;
                            CustomerLevelList.ClearSelection();
                            CustomerLevelList.Items.FindByValue(DB.RSFieldInt(rs, "CustomerLevelID").ToString()).Selected = true;
                        }
                        catch { }

                    }
                }
            }
        }

        protected void SetContextToNewCustomer()
        {
            SearchCustomerPanel.Visible = false;
            CustomerIDPanel.Visible = false;

            CustomerID.Text = String.Empty;
            FirstName.Text = String.Empty;
            LastName.Text = String.Empty;
            EMail.Text = String.Empty;
            Phone.Text = String.Empty;
            RadioButtonList1.SelectedIndex = 0;
            Over13.Checked = true;

            BillingFirstName.Text = String.Empty;
            BillingLastName.Text = String.Empty;
            BillingPhone.Text = String.Empty;
            BillingCompany.Text = String.Empty;
            BillingResidenceType.ClearSelection();
            BillingResidenceType.SelectedIndex = 1;
            BillingAddress1.Text = String.Empty;
            BillingAddress2.Text = String.Empty;
            BillingSuite.Text = String.Empty;
            BillingCity.Text = String.Empty;
            BillingState.ClearSelection();
            BillingState.SelectedIndex = 0;
            BillingZip.Text = String.Empty;
            BillingCountry.ClearSelection();
            BillingCountry.SelectedIndex = 0;

            ShippingFirstName.Text = String.Empty;
            ShippingLastName.Text = String.Empty;
            ShippingPhone.Text = String.Empty;
            ShippingCompany.Text = String.Empty;
            ShippingResidenceType.ClearSelection();
            ShippingResidenceType.SelectedIndex = 1;
            ShippingAddress1.Text = String.Empty;
            ShippingAddress2.Text = String.Empty;
            ShippingSuite.Text = String.Empty;
            ShippingCity.Text = String.Empty;
            ShippingCountry.ClearSelection();
            ShippingCountry.SelectedIndex = -1;
            ShippingState.ClearSelection();
            ShippingState.SelectedIndex = -1;
            ShippingZip.Text = String.Empty;

            try
            {
                AffiliateList.SelectedIndex = -1;
                AffiliateList.ClearSelection();
            }
            catch { }
            try
            {
                CustomerLevelList.SelectedIndex = -1;
                CustomerLevelList.ClearSelection();
            }
            catch { }

            CreateCustomer.Visible = true;
            UseCustomer.Visible = false;
            UpdateCustomer.Visible = false;
            CreateNewCustomerPanel.Visible = true;
            CustomerStatsPanel.Visible = false;
            ImpersonationPanel.Visible = false;
            Panel3.Visible = false;
            Panel2.Visible = false;
        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName.Equals("select", StringComparison.InvariantCultureIgnoreCase))
            {
                int index = Convert.ToInt32(e.CommandArgument);
                SetContextToCustomer(index);
            }
        }

        private void SetFramePage(String FrameName, String Url)
        {
            HtmlControl frame1 = (HtmlControl)FindControl(FrameName);
            frame1.Attributes["src"] = Url;
            if (FrameName.IndexOf("Impersonation") != -1)
            {
                ImpersonationPanel.Visible = true;
                Panel3.Visible = false;
                Panel2.Visible = false;
            }
            else if (FrameName.IndexOf("Panel2") != -1)
            {
                ImpersonationPanel.Visible = false;
                Panel2.Visible = true;
                Panel3.Visible = false;
            }
            else if (FrameName.IndexOf("Panel3") != -1)
            {
                ImpersonationPanel.Visible = false;
                Panel2.Visible = false;
                Panel3.Visible = true;
            }
        }

        private void SetToImpersonationPageContext(int CustomerID, String PageName, bool UseEmptyTemplate)
        {
            String IGD = String.Empty;
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("Select CustomerGUID from Customer  with (NOLOCK)  where deleted=0 and IsAdmin=0 and CustomerID=" + CustomerID.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        IGD = DB.RSFieldGUID(rs, "CustomerGUID").ToString();
                    }
                }
            }

            String Url = PageName + CommonLogic.IIF(PageName.IndexOf("?") == -1, "?", "&") + "IGD=" + IGD.ToString();
            SetFramePage(ImpersonationFrame.UniqueID, Url);
        }

        private void SetToPanel2Page(int CustomerID, String PageName, bool UseEmptyTemplate)
        {
            String IGD = String.Empty;
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("Select CustomerGUID from Customer  with (NOLOCK)  where deleted=0 and IsAdmin=0 and CustomerID=" + CustomerID.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        IGD = DB.RSFieldGUID(rs, "CustomerGUID").ToString();
                    }
                }
            }

            String Url = PageName + CommonLogic.IIF(PageName.IndexOf("?") == -1, "?", "&") + "IGD=" + IGD.ToString();
            SetFramePage(LeftPanel2Frame.UniqueID, Url);
        }

        private void SetToPanel3Page(int CustomerID, String PageName, bool UseEmptyTemplate)
        {
            String IGD = String.Empty;
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("Select CustomerGUID from Customer  with (NOLOCK)  where deleted=0 and IsAdmin=0 and CustomerID=" + CustomerID.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        IGD = DB.RSFieldGUID(rs, "CustomerGUID").ToString();
                    }
                }
            }
            String Url = PageName + CommonLogic.IIF(PageName.IndexOf("?") == -1, "?", "&") + "IGD=" + IGD.ToString();
            SetFramePage(LeftPanel3Frame.UniqueID, Url);
        }

        protected bool EmailTaken(string Email)
        {
            bool emailIsOptional = AppLogic.AppConfigBool("PhoneOrder.EMailIsOptional");
            bool allowDuplicateEmails = AppLogic.GlobalConfigBool("AllowCustomerDuplicateEMailAddresses");
            bool allowAlreadyRegisteredEmails = AppLogic.GlobalConfigBool("Anonymous.AllowAlreadyRegisteredEmail");
            bool updating = !String.IsNullOrEmpty(CustomerID.Text); //Only populated when editing an existing customer - if empty, we're making a new one

            if (!emailIsOptional && Email.Length != 0 && !allowDuplicateEmails && !allowAlreadyRegisteredEmails)
            {
                int NN = DB.GetSqlN("select count(*) as N from customer  with (NOLOCK)  where deleted=0 and EMail=" + 
                    DB.SQuote(Email.ToLowerInvariant().Trim()) +
                    (updating ? " and CustomerID <> " + CustomerID.Text : string.Empty));
                if (NN > 0)
                {
                    return true;
                }
            }

            return false;
        }

        protected void CreateCustomer_Click(object sender, EventArgs e)
        {
            bool ErrorsFound = false;
            EMailAlreadyTaken.Visible = ErrorsFound = EmailTaken(EMail.Text.Trim());

            valBillingZip.CountryID = AppLogic.GetCountryID(BillingCountry.SelectedItem.Text);
            valShippingZip.CountryID = AppLogic.GetCountryID(ShippingCountry.SelectedItem.Text);

            valShippingZip.Validate();
            valBillingZip.Validate();

            if (!valShippingZip.IsValid || !valBillingZip.IsValid)
                ErrorsFound = true;


            if (!ErrorsFound)
            {
                int m_CustomerID = 0;
                String m_CustomerGUID = String.Empty;
                Customer.MakeAnonCustomerRecord(out m_CustomerID, out m_CustomerGUID);
                if (EMail.Text.Trim().Length == 0)
                {
                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS("select EMail from Customer  with (NOLOCK)  where deleted=0 and IsAdmin=0 and CustomerID=" + m_CustomerID.ToString(), conn))
                        {
                            if (rs.Read())
                            {
                                EMail.Text = DB.RSField(rs, "EMail").ToLowerInvariant().Trim();
                            }
                        }
                    }
                }

                // when first created, we create a random password for the user, and e-mail it to them!
                Password p = new RandomPassword();

                if (EMail.Text.Trim().Length != 0 && AppLogic.MailServer().Length != 0 && AppLogic.MailServer() != AppLogic.ro_TBD)
                {
                    try
                    {
                        AppLogic.SendMail(AppLogic.AppConfig("StoreName") + " - " + AppLogic.GetString("cst_account_process.aspx.1", SkinID, LocaleSetting), AppLogic.RunXmlPackage("notification.lostpassword.xml.config", null, ThisCustomer, SkinID, "", "thiscustomerid=" + m_CustomerID.ToString() + "&newpwd=" + p.ClearPassword.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;").Replace("&", "&amp;"), false, false), true, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromName"), EMail.Text.ToLowerInvariant().Trim(), EMail.Text.ToLowerInvariant().Trim(), "", "", AppLogic.MailServer());
                        Security.LogEvent("Admin Create Phone Order Customer Random Password", "", m_CustomerID, ThisCustomer.CustomerID, Convert.ToInt32(ThisCustomer.CurrentSessionID));
                    }
                    catch
                    {
                        // we have NO way of handling a failure to send the customer their pwd here!!!
                    }
                }

                StringBuilder sql = new StringBuilder(1024);
                sql.Append("update customer set ");
                sql.Append("RegisterDate=getdate(),");
                sql.Append("FirstName=" + DB.SQuote(FirstName.Text.Trim()) + ",");
                sql.Append("LastName=" + DB.SQuote(LastName.Text.Trim()) + ",");
                sql.Append("EMail=" + DB.SQuote(EMail.Text.ToLowerInvariant().Trim()) + ",");
                sql.Append("IsRegistered=" + CommonLogic.IIF(EMail.Text.Trim().Length != 0 && (FirstName.Text.Trim() + LastName.Text.Trim()).Trim().Length != 0, "1", "0") + ",");

                // set their pwd to the new random password, must be changed on first login:
                sql.Append("Password=" + DB.SQuote(p.SaltedPassword) + ",");
                sql.Append("SaltKey=" + p.Salt.ToString() + ",");
                sql.Append("PwdChangeRequired=1,");

                sql.Append("Phone=" + DB.SQuote(Phone.Text) + ",");
                if (AffiliateList.SelectedIndex > 0)
                {
                    sql.Append("AffiliateID=" + AffiliateList.SelectedValue + ",");
                }
                if (CustomerLevelList.SelectedIndex > 0)
                {
                    sql.Append("CustomerLevelID=" + CustomerLevelList.SelectedValue + ",");
                }
                sql.Append("Over13Checked=" + CommonLogic.IIF(Over13.Checked, "1", "0") + ",");
                sql.Append("OKToEMail=" + CommonLogic.IIF(RadioButtonList1.SelectedValue == "Yes", "1", "0"));
                sql.Append(" where CustomerID=" + m_CustomerID.ToString());
                DB.ExecuteSQL(sql.ToString());

                Address BillingAddress = new Address();
                Address ShippingAddress = new Address();

                BillingAddress.LastName = BillingLastName.Text;
                BillingAddress.FirstName = BillingFirstName.Text;
                BillingAddress.Phone = BillingPhone.Text;
                BillingAddress.Company = BillingCompany.Text;
                BillingAddress.ResidenceType = (ResidenceTypes)Convert.ToInt32(BillingResidenceType.SelectedValue);
                BillingAddress.Address1 = BillingAddress1.Text;
                BillingAddress.Address2 = BillingAddress2.Text;
                BillingAddress.Suite = BillingSuite.Text;
                BillingAddress.City = BillingCity.Text;
                BillingAddress.State = BillingState.SelectedValue;
                BillingAddress.Zip = BillingZip.Text;
                BillingAddress.Country = BillingCountry.SelectedValue;
                BillingAddress.EMail = EMail.Text.ToLowerInvariant().Trim();

                BillingAddress.InsertDB(m_CustomerID);
                BillingAddress.MakeCustomersPrimaryAddress(AddressTypes.Billing);

                ShippingAddress.LastName = ShippingLastName.Text;
                ShippingAddress.FirstName = ShippingFirstName.Text;
                ShippingAddress.Phone = ShippingPhone.Text;
                ShippingAddress.Company = ShippingCompany.Text;
                ShippingAddress.ResidenceType = (ResidenceTypes)Convert.ToInt32(ShippingResidenceType.SelectedValue);
                ShippingAddress.Address1 = ShippingAddress1.Text;
                ShippingAddress.Address2 = ShippingAddress2.Text;
                ShippingAddress.Suite = ShippingSuite.Text;
                ShippingAddress.City = ShippingCity.Text;
                ShippingAddress.State = ShippingState.SelectedValue;
                ShippingAddress.Zip = ShippingZip.Text;
                ShippingAddress.Country = ShippingCountry.SelectedValue;
                ShippingAddress.EMail = EMail.Text.ToLowerInvariant().Trim();

                ShippingAddress.InsertDB(m_CustomerID);
                ShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);

                CustomerID.Text = m_CustomerID.ToString();
            }

            if (!ErrorsFound)
            {
                CreateCustomer.Visible = false;
                UpdateCustomer.Visible = true;
                UseCustomer.Visible = true;
            }
        }

        protected void UpdateCustomer_Click(object sender, EventArgs e)
        {
            bool errorsFound = false;
            EMailAlreadyTaken.Visible = errorsFound = EmailTaken(EMail.Text.Trim());            

            valBillingZip.CountryID = AppLogic.GetCountryID(BillingCountry.SelectedItem.Text);
            valShippingZip.CountryID = AppLogic.GetCountryID(ShippingCountry.SelectedItem.Text);

            valShippingZip.Validate();
            valBillingZip.Validate();

            if (!valShippingZip.IsValid || !valBillingZip.IsValid)
                errorsFound = true;

            if (!errorsFound)
            {
                int m_CustomerID = System.Int32.Parse(CustomerID.Text);
                StringBuilder sql = new StringBuilder(1024);
                sql.Append("update customer set ");
                sql.Append("FirstName=" + DB.SQuote(FirstName.Text.Trim()) + ",");
                sql.Append("LastName=" + DB.SQuote(LastName.Text.Trim()) + ",");
                sql.Append("EMail=" + DB.SQuote(EMail.Text.ToLowerInvariant().Trim()) + ",");
                sql.Append("Phone=" + DB.SQuote(Phone.Text.Trim()) + ",");
                if (AffiliateList.SelectedIndex > 0)
                {
                    sql.Append("AffiliateID=" + AffiliateList.SelectedValue + ",");
                }
                if (CustomerLevelList.SelectedIndex > 0)
                {
                    sql.Append("CustomerLevelID=" + CustomerLevelList.SelectedValue + ",");
                }
                sql.Append("Over13Checked=" + CommonLogic.IIF(Over13.Checked, "1", "0") + ",");
                sql.Append("OKToEMail=" + CommonLogic.IIF(RadioButtonList1.SelectedValue == "Yes", "1", "0"));
                sql.Append(" where CustomerID=" + m_CustomerID.ToString());
                DB.ExecuteSQL(sql.ToString());

                Address BillingAddress = new Address();
                Address ShippingAddress = new Address();

                BillingAddress.LoadByCustomer(m_CustomerID, AddressTypes.Billing);
                BillingAddress.LastName = BillingLastName.Text;
                BillingAddress.FirstName = BillingFirstName.Text;
                BillingAddress.Phone = BillingPhone.Text;
                BillingAddress.Company = BillingCompany.Text;
                BillingAddress.ResidenceType = (ResidenceTypes)Convert.ToInt32(BillingResidenceType.SelectedValue);
                BillingAddress.Address1 = BillingAddress1.Text;
                BillingAddress.Address2 = BillingAddress2.Text;
                BillingAddress.Suite = BillingSuite.Text;
                BillingAddress.City = BillingCity.Text;
                BillingAddress.State = BillingState.SelectedValue;
                BillingAddress.Zip = BillingZip.Text;
                BillingAddress.Country = BillingCountry.SelectedValue;
                BillingAddress.EMail = EMail.Text.ToLowerInvariant().Trim();
                if (BillingAddress.AddressID == 0)
                {
                    BillingAddress.InsertDB(m_CustomerID);
                }
                else
                {
                    BillingAddress.UpdateDB();
                }
                BillingAddress.MakeCustomersPrimaryAddress(AddressTypes.Billing);

                ShippingAddress.LoadByCustomer(m_CustomerID, AddressTypes.Shipping);
                ShippingAddress.LastName = ShippingLastName.Text;
                ShippingAddress.FirstName = ShippingFirstName.Text;
                ShippingAddress.Phone = ShippingPhone.Text;
                ShippingAddress.Company = ShippingCompany.Text;
                ShippingAddress.ResidenceType = (ResidenceTypes)Convert.ToInt32(ShippingResidenceType.SelectedValue);
                ShippingAddress.Address1 = ShippingAddress1.Text;
                ShippingAddress.Address2 = ShippingAddress2.Text;
                ShippingAddress.Suite = ShippingSuite.Text;
                ShippingAddress.City = ShippingCity.Text;
                ShippingAddress.State = ShippingState.SelectedValue;
                ShippingAddress.Zip = ShippingZip.Text;
                ShippingAddress.Country = ShippingCountry.SelectedValue;
                ShippingAddress.EMail = EMail.Text.ToLowerInvariant().Trim();
                if (ShippingAddress.AddressID == 0)
                {
                    ShippingAddress.InsertDB(m_CustomerID);
                }
                else
                {
                    ShippingAddress.UpdateDB();
                }
                ShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
            }

            if (!errorsFound)
            {
                CreateCustomer.Visible = false;
                UpdateCustomer.Visible = true;
                UseCustomer.Visible = true;
            }
        }

        protected void UseCustomer_Click(object sender, EventArgs e)
        {
            UpdateCustomer_Click(sender, e);
            if (EMailAlreadyTaken.Visible == false)
            {

                // use customer as is:
                TopPanel.Visible = false;
                SearchCustomerPanel.Visible = false;
                CreateNewCustomerPanel.Visible = false;
                CustomerStatsPanel.Visible = true;
                CreateCustomer.Visible = false;
                UseCustomer.Visible = false;
                UpdateCustomer.Visible = false;

                UsingCustomerID.Text = CustomerID.Text;
                UsingFirstName.Text = FirstName.Text;
                UsingLastName.Text = LastName.Text;
                UsingEMail.Text = EMail.Text.ToLowerInvariant().Trim();

                SetToImpersonationPageContext(System.Int32.Parse(CustomerID.Text), "../default.aspx", false);
            }
        }

        protected void Button6_Click(object sender, EventArgs e)
        {
            // cancel order
            ThisCustomer.ThisCustomerSession["IGD"] = "";
            ThisCustomer.ThisCustomerSession["IGDEDITINGORDER"] = "";
            if (m_OrderNumber == 0)
            {
                ShoppingCart.StaticClearContents(System.Int32.Parse(CustomerID.Text), CartTypeEnum.ShoppingCart, null);
                Response.Redirect(AppLogic.AdminLinkUrl("phoneorder.aspx"));
            }
            else
            {
                try
                {
                    ShoppingCart.StaticClearContents(System.Int32.Parse(CustomerID.Text), CartTypeEnum.ShoppingCart, null);
                }
                catch { }
                Response.Redirect(AppLogic.AdminLinkUrl("orders.aspx")+"?ordernumber=" + m_OrderNumber.ToString());
            }
        }

        protected void Button88_Click(object sender, EventArgs e)
        {
            // cancel order
            ThisCustomer.ThisCustomerSession["IGD"] = "";
            ThisCustomer.ThisCustomerSession["IGDEDITINGORDER"] = "";
            if (m_OrderNumber == 0)
            {
                Response.Redirect(AppLogic.AdminLinkUrl("phoneorder.aspx"));
            }
            else
            {
                try
                {
                    ShoppingCart.StaticClearContents(System.Int32.Parse(CustomerID.Text), CartTypeEnum.ShoppingCart, null);
                }
                catch { }
                Response.Redirect(AppLogic.AdminLinkUrl("orders.aspx") + "?ordernumber=" + m_OrderNumber.ToString());
            }
        }

        protected void Button89_Click(object sender, EventArgs e)
        {
            // Edit: Reset To Match Original Order
            if (m_OrderNumber != 0)
            {
                DB.ExecuteSQL("aspdnsf_EditOrder " + m_OrderNumber.ToString());
                SetToImpersonationPageContext(System.Int32.Parse(CustomerID.Text), "../shoppingcart.aspx", false);
            }
        }

        protected void Button90_Click(object sender, EventArgs e)
        {
            // Edit: Clear Cart
            if (m_OrderNumber != 0)
            {
                ShoppingCart.StaticClearContents(System.Int32.Parse(CustomerID.Text), CartTypeEnum.ShoppingCart, null);
                SetToImpersonationPageContext(System.Int32.Parse(CustomerID.Text), "../shoppingcart.aspx", false);
            }
            else
            {
                ShoppingCart.StaticClearContents(System.Int32.Parse(CustomerID.Text), CartTypeEnum.ShoppingCart, null);
            }
        }

        protected void Button91_Click(object sender, EventArgs e)
        {
            // Edit: View Original Receipt
            if (m_OrderNumber != 0)
            {
                SetToImpersonationPageContext(System.Int32.Parse(CustomerID.Text), "../receipt.aspx?ordernumber=" + m_OrderNumber.ToString(), false);
            }
        }

        protected void Button43_Click(object sender, EventArgs e)
        {
            DB.ExecuteSQL("delete from FailedTransaction where CustomerID=" + CustomerID.Text);
        }

        protected void Button12_Click(object sender, EventArgs e)
        {
            // re-edit customer
            CreateNewCustomerPanel.Visible = true;
            CustomerStatsPanel.Visible = false;
            ImpersonationPanel.Visible = false;
            CreateCustomer.Visible = false;
            UpdateCustomer.Visible = true;
            UseCustomer.Visible = true;
            Panel3.Visible = false;
            Panel2.Visible = false;
        }
        protected void Button13_Click(object sender, EventArgs e)
        {
            // restart impersonation
            UseCustomer_Click(sender, e);
        }

        private void GetJavaScriptFunctions()
        {
            btnCopyAccount.Attributes.Add("onclick", "copyaccount(this.form);");
            string strScript = "<script type=\"text/javascript\">";
            strScript += "function copyaccount(theForm){ ";
            strScript += "theForm." + BillingFirstName.ClientID + ".value = theForm." + FirstName.ClientID + ".value;";
            strScript += "theForm." + BillingLastName.ClientID + ".value = theForm." + LastName.ClientID + ".value;";
            strScript += "theForm." + BillingPhone.ClientID + ".value = theForm." + Phone.ClientID + ".value;";
            strScript += "return true; }  ";
            strScript += "</script> ";

            ClientScript.RegisterClientScriptBlock(GetType(), Guid.NewGuid().ToString(), strScript);

            btnCopyBilling.Attributes.Add("onclick", "copybilling(this.form);");
            strScript = string.Empty;
            strScript = "<script type=\"text/javascript\">";
            strScript += "function copybilling(theForm){";
            strScript += "theForm." + ShippingFirstName.ClientID + ".value = theForm." + BillingFirstName.ClientID + ".value;";
            strScript += "theForm." + ShippingLastName.ClientID + ".value = theForm." + BillingLastName.ClientID + ".value;";
            strScript += "theForm." + ShippingPhone.ClientID + ".value = theForm." + BillingPhone.ClientID + ".value;";
            strScript += "theForm." + ShippingCompany.ClientID + ".value = theForm." + BillingCompany.ClientID + ".value;";
            strScript += "theForm." + ShippingCompany.ClientID + ".value = theForm." + BillingCompany.ClientID + ".value;";
            strScript += "theForm." + ShippingResidenceType.ClientID + "selectedIndex =  theForm." + BillingResidenceType.ClientID + ".selectedIndex;";
            strScript += "theForm." + ShippingAddress1.ClientID + ".value = theForm." + BillingAddress1.ClientID + ".value;";
            strScript += "theForm." + ShippingAddress2.ClientID + ".value = theForm." + BillingAddress2.ClientID + ".value;";
            strScript += "theForm." + ShippingSuite.ClientID + ".value = theForm." + BillingSuite.ClientID + ".value;";
            strScript += "theForm." + ShippingCity.ClientID + ".value = theForm." + BillingCity.ClientID + ".value;";
            strScript += "theForm." + ShippingState.ClientID + ".value = theForm." + BillingState.ClientID + ".value;";
            strScript += "theForm." + ShippingZip.ClientID + ".value = theForm." + BillingZip.ClientID + ".value;";
            strScript += "theForm." + ShippingCountry.ClientID + ".selectedIndex =  theForm." + BillingCountry.ClientID + ".selectedIndex;";
            strScript += "return true; }  ";
            strScript += "</script> ";

            ClientScript.RegisterClientScriptBlock(GetType(), Guid.NewGuid().ToString(), strScript);
        }

    }
}
