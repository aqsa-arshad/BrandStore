// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront
{
    public partial class lat_account : SkinBase
    {
        string AffState = string.Empty;
        string AffCountry = string.Empty;
        int AffiliateID = 0;
        private Boolean DisablePasswordAutocomplete
        {
            get { return AppLogic.AppConfigBool("DisablePasswordAutocomplete"); }
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            RequireSecurePage();

            if (DisablePasswordAutocomplete)
            {
                AppLogic.DisableAutocomplete(AffPassword);
                AppLogic.DisableAutocomplete(AffPassword2);
            }

            AffiliateID = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(Profile.LATAffiliateID), Profile.LATAffiliateID, "0"));

            if (AffiliateID == 0 || !AppLogic.IsValidAffiliate(AffiliateID))
            {
                Response.Redirect("lat_signin.aspx?returnurl=" + Server.UrlEncode(CommonLogic.GetThisPageName(true) + "?" + CommonLogic.ServerVariables("QUERY_STRING")));
            }

            SectionTitle = "<a href=\"lat_account.aspx\">" + AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting) + "</a> - Account Summary Page";

            lblErrorMsg.Text = "";
            lblNote.Text = "";

            if (!IsPostBack)
            {
                InitializePageContent();
            }
        }

        public void btnUpdate3_Click(object sender, EventArgs e)
        {
            ProcessAccountUpdate();
        }

        public void btnUpdate2_Click(object sender, EventArgs e)
        {
            ProcessAccountUpdate();
        }

        public void btnUpdate1_Click(object sender, EventArgs e)
        {
            ProcessAccountUpdate();
        }

        public void Country_DataBound(object sender, EventArgs e)
        {
            Country.Items.Insert(0, new ListItem(AppLogic.GetString("requestcatalog.aspx.20", SkinID, ThisCustomer.LocaleSetting), ""));
            int i = Country.Items.IndexOf(Country.Items.FindByValue(AffCountry));
            if (i == -1)
                Country.SelectedIndex = 0;
            else
                Country.SelectedIndex = i;
        }

        public void State_DataBound(object sender, EventArgs e)
        {
            State.Items.Insert(0, new ListItem(AppLogic.GetString("requestcatalog.aspx.20", SkinID, ThisCustomer.LocaleSetting), ""));
            int i = State.Items.IndexOf(State.Items.FindByValue(AffState));
            if (i == -1)
                State.SelectedIndex = 0;
            else
                State.SelectedIndex = i;
        }


        private void InitializePageContent()
        {
            AskAQuestion.NavigateUrl = "mailto:" + AppLogic.AppConfig("AffiliateEMailAddress");
            AppConfigAffiliateProgramName3.Text = AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting) + " Sign-Up";          
            AppConfig_AffiliateProgramName4.Text = String.Format(AppLogic.GetString("lataccount.aspx.30", SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting));

            Affiliate a = new Affiliate(AffiliateID);
            if (a.AffiliateID != -1)
            {
                //Fill Account data fields
                FirstName.Text = a.FirstName;
                LastName.Text = a.LastName;
                EMail.Text = a.EMail.ToLowerInvariant().Trim();
                AffPassword.Text = String.Empty;
                AffPassword2.Text = String.Empty;
                Company.Text = Server.HtmlEncode(a.Company);
                Address1.Text = Server.HtmlEncode(a.Address1);
                Address2.Text = Server.HtmlEncode(a.Address2);
                Suite.Text = Server.HtmlEncode(a.Suite);
                City.Text = Server.HtmlEncode(a.City);

                AffState = a.State;

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

                Zip.Text = Server.HtmlEncode(a.Zip);

                AffCountry = a.Country;

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

                Phone.Text = Server.HtmlEncode(a.Phone);
                DOBTxt.Text = Localization.ToThreadCultureShortDateString(a.DateOfBirth);

                //Website Data
                WebSiteName.Text = a.WebSiteName;
                WebSiteDescription.Text = a.WebSiteDescription;
                URL.Text = a.URL;
            }

            AppLogic.GetButtonDisable(btnUpdate1);
            AppLogic.GetButtonDisable(btnUpdate2);

        }

        private void ProcessAccountUpdate()
        {
            Affiliate a = new Affiliate(AffiliateID);
            Page.Validate();
            if (Page.IsValid && a.AffiliateID != -1)
            {
                try
                {
                    string pwd = null;
                    object saltkey = null;
                    if (AffPassword.Text.Trim().Length > 0)
                    {
                        Password p = new Password(AffPassword.Text, a.SaltKey);
                        pwd = p.SaltedPassword;
                        saltkey = p.Salt;
                    }

                    object dob = Localization.ParseNativeDateTime(DOBTxt.Text);
                    if ((DateTime)dob == DateTime.MinValue)
                    {
                        dob = null;
                    }


                    String theUrl2 = CommonLogic.Left(URL.Text, 80);
                    if (theUrl2.IndexOf("http://") == -1 && theUrl2.Length != 0)
                    {
                        theUrl2 = "http://" + theUrl2;
                    }

                    string Name = FirstName.Text + " " + LastName.Text;

                    a.Update(EMail.Text.ToLowerInvariant().Trim(), pwd, dob, null, null, CommonLogic.IIF(CommonLogic.FormCanBeDangerousContent("URL").Length == 0, 0, 1), CommonLogic.Left(FirstName.Text, 50), CommonLogic.Left(LastName.Text, 50), CommonLogic.Left(Name, 100), Company.Text, Address1.Text.Replace("\x0D\x0A", ""), Address2.Text.Replace("\x0D\x0A", ""), Suite.Text, City.Text, State.SelectedValue, Zip.Text, Country.SelectedValue, AppLogic.MakeProperPhoneFormat(Phone.Text), WebSiteName.Text, WebSiteDescription.Text, theUrl2, null, null, null, null, null, null, null, null, null, null, null, null, null, null, saltkey);

                    lblErrMsg.Text = "Account Updated";
                }
                catch
                {
                    lblErrMsg.Text = "<p><b>ERROR: There was an unknown error in updating your new account record. Please <a href=\"contactus.aspx\">contact a service representative</a> for assistance.</b></p>";
                }

                Profile.LATAffiliateID = a.AffiliateID.ToString();

                InitializePageContent();
            }
            else
            {
                lblErrorMsg.Text += " Some errors occurred trying to create your affiliate account.  Please correct them and try again.";
                foreach (IValidator aValidator in this.Validators)
                {
                    if (!aValidator.IsValid)
                    {
                        lblErrorMsg.Text += "&bull; " + aValidator.ErrorMessage + "";
                    }
                }
                lblErrorMsg.Text += "";
            }

        }

        protected void ValidatePassword(object source, ServerValidateEventArgs args)
        {
            if (AffPassword.Text.Trim() == "")
            {
                args.IsValid = true;
                return;
            }
            if (AffPassword.Text == AffPassword2.Text)
            {
                try
                {
                    valPwd.ErrorMessage = AppLogic.GetString("account.aspx.7", SkinID, ThisCustomer.LocaleSetting);

                    if (Regex.IsMatch(AffPassword.Text, AppLogic.AppConfig("CustomerPwdValidator"), RegexOptions.Compiled))
                    {
                        args.IsValid = true;
                    }
                    else
                    {
                        args.IsValid = false;
                        valPwd.ErrorMessage = AppLogic.GetString("account.aspx.69", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
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
