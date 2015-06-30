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
    /// <summary>
    /// Summary description for lat_signup.
    /// </summary>
    public partial class lat_signup : SkinBase
    {
        Address BillingAddress;
        int AffiliateID = 0;
        private Boolean DisablePasswordAutocomplete
        {
            get { return AppLogic.AppConfigBool("DisablePasswordAutocomplete"); }
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            RequireSecurePage();
            AffiliateID = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(Profile.LATAffiliateID), Profile.LATAffiliateID, "0"));
            SectionTitle = "<a href=\"lat_account.aspx\">" + AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting) + "</a> - Sign Up";
            if (DisablePasswordAutocomplete)
            {
                AppLogic.DisableAutocomplete(AffPassword);
                AppLogic.DisableAutocomplete(AffPassword2);
            }
            if (!IsPostBack)
            {
                InitializePageContent();
            }
            else
            {
                btnJoin.Enabled = cbkAgreeToTermsAndConditions.Checked;
            }
            lblErrorMsg.Text = string.Empty;
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
            this.State.DataBound += new EventHandler(State_DataBound);
            this.Country.DataBound += new EventHandler(Country_DataBound);
        }

        #endregion

        void Country_DataBound(object sender, EventArgs e)
        {
            Country.Items.Insert(0, new ListItem(AppLogic.GetString("requestcatalog.aspx.20", SkinID, ThisCustomer.LocaleSetting), ""));
            if (BillingAddress != null)
            {
                int i = Country.Items.IndexOf(Country.Items.FindByValue(BillingAddress.Country));
                if (i == -1)
                {
                    Country.SelectedIndex = 0;
                }
                else
                {
                    Country.SelectedIndex = i;
                }
            }
        }
        void State_DataBound(object sender, EventArgs e)
        {
            State.Items.Insert(0, new ListItem(AppLogic.GetString("requestcatalog.aspx.20", SkinID, ThisCustomer.LocaleSetting), ""));
            if (BillingAddress != null)
            {
                int i = State.Items.IndexOf(State.Items.FindByValue(BillingAddress.State));
                if (i == -1)
                {
                    State.SelectedIndex = 0;
                }
                else
                {
                    State.SelectedIndex = i;
                }
            }

        }

        public void btnJoin_Click(object sender, EventArgs e)
        {
            ProcessSignup();
        }

        public void ValTerms_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = cbkAgreeToTermsAndConditions.Checked;
        }

        private void InitializePageContent()
        {
            GetJavaScriptFunctions();

            JSPopupRoutines.Text = AppLogic.GetJSPopupRoutines();

            AppConfigAffiliateProgramName3.Text = AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting) + " Sign-Up";
            AppConfigAffiliateProgramName4.Text = AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting);
            CustSvcEmailLink.NavigateUrl = "mailto:" + AppLogic.AppConfig("AffiliateEMailAddress");
            lnkAskAQuestion.NavigateUrl = "mailto:" + AppLogic.AppConfig("AffiliateEMailAddress");

            TermsLink.Text = "<a onClick=\"popuptopicwh('" + Server.UrlEncode(AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting).Replace("'", "")) + " Terms & Conditions','affiliate_terms',650,550,'yes')\" href=\"javascript:void(0);\">Terms & Conditions</a>";

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader dr = DB.GetRS("select * from State   with (NOLOCK)  order by DisplayOrder,Name", conn))
                {
                    State.DataSource = dr;
                    State.DataTextField = "Name";
                    State.DataValueField = "Abbreviation";
                    State.DataBind();
                }
            }


            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader dr2 = DB.GetRS("select * from Country   with (NOLOCK)  where Published = 1 order by DisplayOrder,Name", conn))
                {
                    Country.DataSource = dr2;
                    Country.DataTextField = "Name";
                    Country.DataValueField = "Name";
                    Country.DataBind();
                }
            }

            if (AffiliateID == 0)
            {
                pnlBeforeSignup.Visible = true;
                pnlAfterSignup.Visible = false;
                pnlSignedInMsg.Visible = false;
                pnlSignUpForm.Visible = true;
            }
            else
            {
                BillingAddress = new Address();
                BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                pnlBeforeSignup.Visible = false;
                pnlAfterSignup.Visible = true;
                pnlSignedInMsg.Visible = true;
                pnlSignUpForm.Visible = false;
                this.AppConfigAffiliateProgramName3.Text = "Your account information will be used to login to your " + AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting) + " account page later, so please save your password in a safe place.";
                FirstName.Text = ThisCustomer.FirstName;
                LastName.Text = ThisCustomer.LastName;
                EMail.Text = ThisCustomer.EMail.ToLowerInvariant().Trim();
                AffPassword.Text = String.Empty;
                AffPassword2.Text = String.Empty;
                Company.Text = Server.HtmlEncode(BillingAddress.Company);
                Address1.Text = Server.HtmlEncode(BillingAddress.Address1);
                Address2.Text = Server.HtmlEncode(BillingAddress.Address2);
                Suite.Text = Server.HtmlEncode(BillingAddress.Suite);
                City.Text = Server.HtmlEncode(BillingAddress.City);
                Zip.Text = Server.HtmlEncode(BillingAddress.Zip);
                Phone.Text = Server.HtmlEncode(BillingAddress.Phone);

                tblAffWebInfo.Attributes.Add("style", "border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor"));
                tblWebSiteInfoBox.Attributes.Add("style", AppLogic.AppConfig("BoxFrameStyle"));
            }

            AppLogic.GetButtonDisable(btnJoin);
        }

        private void ProcessSignup()
        {
            if (Page.IsValid)
            {
                int AffiliateID = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(Profile.LATAffiliateID), Profile.LATAffiliateID, "0"));

                String ErrorMsg = String.Empty;
                String EMailField = EMail.Text.ToLowerInvariant().Trim();
                bool Editing = false;
                if (Affiliate.EmailInUse(EMailField))
                {
                    ErrorMsg = "That email address has already been registered.  Please use another email.";
                }

                if (ErrorMsg.Length == 0)
                {
                    try
                    {
                        StringBuilder sql = new StringBuilder(2500);
                        String Name = CommonLogic.FormCanBeDangerousContent("Name");
                        if (Name.Length == 0)
                        {
                            if (FirstName.Text.Length != 0)
                            {
                                Name = (FirstName.Text + " " + LastName.Text).Trim();
                            }
                            else
                            {
                                Name = LastName.Text;
                            }
                        }
                        if (!Editing)
                        {
                            // ok to add them:

                            Password p = new Password(AffPassword.Text);
                            object dob = null;
                            if (Localization.ParseNativeDateTime(DateOfBirth.Text) != DateTime.MinValue)
                            {
                                dob = Localization.ParseNativeDateTime(DateOfBirth.Text);
                            }

                            // ok to add them:
                            Affiliate a = Affiliate.CreateAffiliate(CommonLogic.Left(EMailField, 100), p.SaltedPassword, dob, null, "", false, CommonLogic.Left(FirstName.Text, 50), CommonLogic.Left(LastName.Text, 50), CommonLogic.Left(Name, 100), CommonLogic.Left(Company.Text, 50), Address1.Text.Replace("\x0D\x0A", ""), Address2.Text.Replace("\x0D\x0A", ""), Suite.Text, City.Text, State.Text, Zip.Text, Country.Text, Phone.Text, WebSiteName.Text, WebSiteDescription.Text, CommonLogic.Left(URL.Text, 80), (CommonLogic.FormCanBeDangerousContent("TrackingOnly") == "1"), 1, 0, 1, null, null, null, null, null, null, null, false, p.Salt);
                            AffiliateID = a.AffiliateID;
                            if (a != null)
                            {
                                Editing = true;
                                lblErrorMsg.Visible = false;
                            }
                            else
                            {
                                Editing = false;
                                lblErrorMsg.Text = "Unable to create affiliate.";
                                lblErrorMsg.Visible = true;
                            }

                        }
                        else
                        {
                            // ok to update:
                            sql.Append("update Affiliate set ");
                            sql.Append("EMail=" + CommonLogic.SQuote(CommonLogic.Left(EMailField, 100)) + ",");
                            if (AffPassword.Text.Trim().Length != 0)
                            {
                                Password p = new Password(AffPassword.Text);
                                sql.Append("Password=" + DB.SQuote(p.SaltedPassword) + ",");
                                sql.Append("SaltKey=" + p.Salt.ToString() + ",");
                            }
                            sql.Append("IsOnline=" + CommonLogic.IIF(URL.Text.Length == 0, "0", "1") + ",");
                            sql.Append("FirstName=" + CommonLogic.SQuote(CommonLogic.Left(FirstName.Text, 50)) + ",");
                            sql.Append("LastName=" + CommonLogic.SQuote(CommonLogic.Left(LastName.Text, 50)) + ",");
                            sql.Append("Name=" + CommonLogic.SQuote(CommonLogic.Left(Name, 100)) + ",");
                            if (DateOfBirth.Text.Length != 0)
                            {
                                sql.Append("DateOfBirth=" + CommonLogic.SQuote(DateOfBirth.Text) + ",");
                            }
                            if (Company.Text.Length != 0)
                            {
                                sql.Append("Company=" + CommonLogic.SQuote(Company.Text) + ",");
                            }
                            else
                            {
                                sql.Append("Company=NULL,");
                            }
                            if (Address1.Text.Length != 0)
                            {
                                sql.Append("Address1=" + CommonLogic.SQuote(Address1.Text.Replace("\x0D\x0A", "")) + ",");
                            }
                            else
                            {
                                sql.Append("Address1=NULL,");
                            }
                            if (Address2.Text.Length != 0)
                            {
                                sql.Append("Address2=" + CommonLogic.SQuote(Address2.Text.Replace("\x0D\x0A", "")) + ",");
                            }
                            else
                            {
                                sql.Append("Address2=NULL,");
                            }
                            if (Suite.Text.Length != 0)
                            {
                                sql.Append("Suite=" + CommonLogic.SQuote(Suite.Text) + ",");
                            }
                            else
                            {
                                sql.Append("Suite=NULL,");
                            }
                            if (City.Text.Length != 0)
                            {
                                sql.Append("City=" + CommonLogic.SQuote(City.Text) + ",");
                            }
                            else
                            {
                                sql.Append("City=NULL,");
                            }
                            if (State.SelectedValue.Length != 0)
                            {
                                sql.Append("State=" + CommonLogic.SQuote(State.SelectedValue) + ",");
                            }
                            else
                            {
                                sql.Append("State=NULL,");
                            }
                            if (Zip.Text.Length != 0)
                            {
                                sql.Append("Zip=" + CommonLogic.SQuote(Zip.Text) + ",");
                            }
                            else
                            {
                                sql.Append("Zip=NULL,");
                            }
                            if (Country.SelectedValue.Length != 0)
                            {
                                sql.Append("Country=" + CommonLogic.SQuote(Country.SelectedValue) + ",");
                            }
                            else
                            {
                                sql.Append("Country=NULL,");
                            }
                            if (Phone.Text.Length != 0)
                            {
                                sql.Append("Phone=" + CommonLogic.SQuote(AppLogic.MakeProperPhoneFormat(Phone.Text)) + ",");
                            }
                            else
                            {
                                sql.Append("Phone=NULL,");
                            }
                            if (WebSiteName.Text.Length != 0)
                            {
                                sql.Append("WebSiteName=" + CommonLogic.SQuote(WebSiteName.Text) + ",");
                            }
                            else
                            {
                                sql.Append("WebSiteName=NULL,");
                            }
                            if (WebSiteDescription.Text.Length != 0)
                            {
                                sql.Append("WebSiteDescription=" + CommonLogic.SQuote(WebSiteDescription.Text) + ",");
                            }
                            else
                            {
                                sql.Append("WebSiteDescription=NULL,");
                            }
                            if (URL.Text.Length != 0)
                            {
                                String theUrl2 = CommonLogic.Left(URL.Text, 80);
                                if (theUrl2.IndexOf("http://") == -1 && theUrl2.Length != 0)
                                {
                                    theUrl2 = "http://" + theUrl2;
                                }
                                if (theUrl2.Length != 0)
                                {
                                    sql.Append("URL=" + CommonLogic.SQuote(theUrl2));
                                }
                                else
                                {
                                    sql.Append("URL=NULL");
                                }
                            }
                            else
                            {
                                sql.Append("URL=NULL");
                            }
                            sql.Append(" where AffiliateID=" + AffiliateID.ToString());
                            DB.ExecuteSQL(sql.ToString());
                            Editing = true;
                        }
                    }
                    catch
                    {
                        lblErrorMsg.Text = "<p><b>ERROR: There was an unknown error in adding your new account record. Please <a href=\"contactus.aspx\">contact a service representative</a> for assistance.</b></p>";
                    }

                }

                Profile.LATAffiliateID = AffiliateID.ToString();
                lblErrorMsg.Text = ErrorMsg;

                if (lblErrorMsg.Text.Length == 0)
                {
                    pnlSignedInMsg.Visible = false;
                    pnlSignUpForm.Visible = false;
                    pnlBeforeSignup.Visible = false;
                    pnlAfterSignup.Visible = true;
                    try
                    {
                        // send admin notification:
                        String FormContents = String.Empty;
                        for (int i = 0; i <= Request.Form.Count - 1; i++)
                        {
                            if (!Request.Form.Keys[i].StartsWith("__"))
                            {
                                FormContents += "<b>" + Request.Form.Keys[i] + "</b>=" + Request.Form[Request.Form.Keys[i]] + "";
                            }
                        }
                        AppLogic.SendMail("" + AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting) + " New Member Notification", FormContents, true, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromName"), AppLogic.AppConfig("AffiliateEMailAddress"), AppLogic.AppConfig("AffiliateEMailAddress"), AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.MailServer());
                    }
                    catch { }

                    lblSignupSuccess.Text = "CONGRATULATIONS AND WELCOME TO THE " + AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant() + " PROGRAM!Your sign-up was successful.<a href=\"lat_account.aspx\">Click here</a> to go to your " + AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting) + " Account Page.";
                    pnlSignupSuccess.Visible = true;

                }
            }
            else
            {
                lblErrorMsg.Text += " Some errors occurred trying to create your affiliate account.  Please correct them and try again.";
            }
            GetJavaScriptFunctions();
        }

        private void GetJavaScriptFunctions()
        {
            string strScript = "<script type=\"text/javascript\"> ";
            strScript += "function AgreeToTerms(sender, args){ ";
            strScript += "if (document.all) { \n ";
            strScript += "args.IsValid = document.all[\"" + cbkAgreeToTermsAndConditions.ClientID + "\"].checked;";
            strScript += "} else { \n";
            strScript += "args.IsValid = document.getElementById(\"" + cbkAgreeToTermsAndConditions.ClientID + "\").checked";
            strScript += "}\n} ";
            strScript += "</script> ";

            ClientScript.RegisterClientScriptBlock(this.GetType(), Guid.NewGuid().ToString(), strScript);

            cbkAgreeToTermsAndConditions.Attributes.Add("onclick", "return CheckStatus();");
            strScript = "<script type=\"text/javascript\">";
            strScript += "function CheckStatus() { ";
            strScript += "if(document.getElementById('" + cbkAgreeToTermsAndConditions.ClientID + "').checked) { ";
            strScript += "document.getElementById('" + btnJoin.ClientID + "').disabled = '';} ";
            strScript += "else { ";
            strScript += "document.getElementById('" + btnJoin.ClientID + "').disabled = 'true';} ";
            strScript += "} ";
            strScript += "</script> ";

            ClientScript.RegisterClientScriptBlock(this.GetType(), Guid.NewGuid().ToString(), strScript);

        }

        protected void ValidatePassword(object source, ServerValidateEventArgs args)
        {

            lblErrorMsg.Text = string.Empty;

            if (AffPassword.Text.Trim() == "")
            {
                args.IsValid = false;
                valPwd.ErrorMessage = AppLogic.GetString("account.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                return;
            }
            if (AffPassword.Text == AffPassword2.Text)
            {
                try
                {
                    if (AffPassword.Text.Length > 4)
                    {
                        args.IsValid = true;
                    }
                    else
                    {
                        args.IsValid = false;
                        valPwd.ErrorMessage = AppLogic.GetString("account.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }
                catch
                {
                    AppLogic.SendMail("Invalid Password Validation Pattern", "", false, AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToAddress"), "", "", AppLogic.MailServer());
                    throw new Exception("Password validation expression is invalid, please notify site administrator");
                }
            }
            else
            {
                args.IsValid = false;
                valPwd.ErrorMessage = AppLogic.GetString("account.aspx.68", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            }
        }
    }
}
