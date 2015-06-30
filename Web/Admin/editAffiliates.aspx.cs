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
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefrontAdmin
{

    public partial class editAffiliates : AdminPageBase
    {
        protected string affiliateCode;
        protected int affiliateID;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            affiliateID = CommonLogic.QueryStringNativeInt("iden");

            ResetPasswordError.Visible = false;
            ResetPasswordOk.Visible = false;

            if (!IsPostBack)
            {
                ResetPasswordLink.Attributes.Add("onClick", "javascript: return confirm('Are you sure you want to reset the password for this customer?');");
                ResetPasswordError.Text = AppLogic.GetString("cst_account_process.aspx.2", 1, Localization.GetDefaultLocale());
                ResetPasswordOk.Text = AppLogic.GetString("cst_account_process.aspx.3", 1, Localization.GetDefaultLocale());
                ddParent.Items.Clear();
                ddParent.Items.Add(new ListItem("--NONE--", "0"));
                string affRaw = AppLogic.RunXmlPackage("AffiliateArrayList.xml.config", null, null, 1, "", "", false, false);
                affRaw = affRaw.Replace("&gt;", " -> ").Replace("\n", "").Replace("\r", "");
                string[] affList = affRaw.Split('~');
                foreach (string aff in affList)
                {
                    string[] affItem = aff.Split('|');
                    if (affItem.Length > 1)
                    {
                        ddParent.Items.Add(new ListItem(affItem[1].ToString(), affItem[0].ToString().Replace(" ", "")));
                    }
                }

                ddState.Items.Clear();
                ddState.Items.Add(new ListItem("SELECT ONE", "0"));

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select * from state   with (NOLOCK)  order by DisplayOrder,Name", dbconn))
                    {
                        while (rs.Read())
                        {
                            ddState.Items.Add(new ListItem(HttpContext.Current.Server.HtmlEncode(DB.RSField(rs, "Name")), HttpContext.Current.Server.HtmlEncode(DB.RSField(rs, "Abbreviation"))));
                        }
                    }
                }

                ddCountry.Items.Clear();
                ddCountry.Items.Add(new ListItem("SELECT ONE", "0"));

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select * from country   with (NOLOCK)  order by DisplayOrder,Name", dbconn))
                    {
                        while (rs.Read())
                        {
                            ddCountry.Items.Add(new ListItem(DB.RSField(rs, "Name"), DB.RSField(rs, "Name")));
                        }
                    }
                }


                ViewState["EditingAffiliate"] = false;
                ViewState["EditingAffiliateID"] = "0";

                reqValPassword.ErrorMessage = AppLogic.GetString("createaccount.aspx.20", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

                resetForm();
                loadScript(true);

                if (affiliateID > 0)
                {
                    ResetPasswordRow.Visible = true;
                    CreatePasswordRow.Visible = false;
                    ViewState["EditingAffiliate"] = true;
                    ViewState["EditingAffiliateID"] = affiliateID;

                    etsMapper.ObjectID = Localization.ParseNativeInt(ViewState["EditingAffiliateID"].ToString());
                    etsMapper.DataBind();
                    litStoreMapper.Visible = etsMapper.StoreCount > 1;
                    litStoreMapperHdr.Visible = etsMapper.StoreCount > 1;

                    getAffiliateDetails();
                }
                else
                {
                    ResetPasswordRow.Visible = false;
                    CreatePasswordRow.Visible = true;
                    ltAffiliate.Text = "NEW Affiliate";
                    btnSubmit.Text = btnSubmit1.Text = "Add Affiliate";
                }
            }
        }

        protected void getAffiliateDetails()
        {

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from Affiliate   with (NOLOCK)  where AffiliateID=" + ViewState["EditingAffiliateID"].ToString(), dbconn))
                {
                    if (!rs.Read())
                    {
                        rs.Close();
                        resetError(AppLogic.GetString("admin.common.UnableToRetrieveData", SkinID, LocaleSetting), true);
                        return;
                    }

                    //editing affiliate
                    btnSubmit1.Text = btnSubmit.Text = AppLogic.GetString("admin.editAffiliates.UpdateAffiliate", SkinID, LocaleSetting);

                    ltAffiliate.Text = txtNickName.Text = Server.HtmlEncode(DB.RSField(rs, "Name"));
                    ltAffiliate.Text += " (" + DB.RSFieldInt(rs, "AffiliateID") + ")";
                    txtFirstName.Text = Server.HtmlEncode(DB.RSField(rs, "FirstName"));
                    txtLastName.Text = Server.HtmlEncode(DB.RSField(rs, "LastName"));

                    ddCountry.ClearSelection();
                    ddParent.SelectedValue = DB.RSFieldInt(rs, "ParentAffiliateID").ToString();

                    txtEmail.Text = Server.HtmlEncode(DB.RSField(rs, "EMail"));

                    txtSkin.Text = Server.HtmlEncode(DB.RSFieldInt(rs, "DefaultSkinID").ToString());
                    txtCompany.Text = Server.HtmlEncode(DB.RSField(rs, "Company"));
                    txtAddress1.Text = Server.HtmlEncode(DB.RSField(rs, "Address1"));
                    txtAddress2.Text = Server.HtmlEncode(DB.RSField(rs, "Address2"));
                    txtSuite.Text = Server.HtmlEncode(DB.RSField(rs, "Suite"));
                    txtCity.Text = Server.HtmlEncode(DB.RSField(rs, "City"));

                    ddState.ClearSelection();
                    try
                    {
                        foreach (ListItem i in ddState.Items)
                        {
                            if (i.Value.Equals(DB.RSField(rs, "State")))
                            {
                                ddState.ClearSelection();
                                ddState.SelectedValue = DB.RSField(rs, "State");
                                break;
                            }
                        }
                    }
                    catch { }

                    txtZip.Text = Server.HtmlEncode(DB.RSField(rs, "Zip"));

                    ddCountry.ClearSelection();
                    try
                    {
                        foreach (ListItem i in ddCountry.Items)
                        {
                            if (i.Value.Equals(DB.RSField(rs, "Country")))
                            {
                                ddCountry.ClearSelection();
                                ddCountry.SelectedValue = DB.RSField(rs, "Country");
                                break;
                            }
                        }
                    }
                    catch { }

                    txtPhone.Text = Server.HtmlEncode(DB.RSField(rs, "Phone"));
                    txtBirthdate.Text = CommonLogic.IIF(DB.RSFieldDateTime(rs, "DateOfBirth") != System.DateTime.MinValue, Localization.ToThreadCultureShortDateString(DB.RSFieldDateTime(rs, "DateOfBirth")), "");

                    rblAdTracking.ClearSelection();
                    rblAdTracking.Items[0].Selected = (DB.RSFieldBool(rs, "TrackingOnly") ? false : true);
                    rblAdTracking.Items[1].Selected = (DB.RSFieldBool(rs, "TrackingOnly") ? true : false);

                    //WEBSITE INFORMATION
                    txtWebName.Text = Server.HtmlEncode(DB.RSField(rs, "WebSiteName"));
                    txtWebDescription.Text = Server.HtmlEncode(DB.RSField(rs, "WebSiteDescription"));
                    txtWebURL.Text = Server.HtmlEncode(DB.RSField(rs, "URL"));
                }
            }
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">" + AppLogic.GetString("admin.common.Notice", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

            if (error.Length > 0)
                str += error + "";
            else
                str = "";

            ((Literal)Form.FindControl(ltError.UniqueID)).Text = str;
        }

        protected void loadScript(bool load)
        {
            if (!load)
            {
                ltScript.Text = "";
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                UpdateInformation();
                etsMapper.ObjectID = Localization.ParseNativeInt(ViewState["EditingAffiliateID"].ToString());
                etsMapper.Save();
            }
        }

        protected void resetForm()
        {

        }

        protected void btnSubmit1_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                UpdateInformation();
                etsMapper.Save();
            }
        }

        protected void UpdateInformation()
        {
            bool Editing = Localization.ParseBoolean(ViewState["EditingAffiliate"].ToString());
            int AffiliateID = Localization.ParseNativeInt(ViewState["EditingAffiliateID"].ToString());

            StringBuilder sql = new StringBuilder();
            String Name = txtNickName.Text;
            if (Name.Length == 0)
            {
                if (txtFirstName.Text.Length != 0)
                {
                    Name = (txtFirstName.Text + " " + txtLastName.Text).Trim();
                }
                else
                {
                    Name = txtLastName.Text;
                }
            }
            int ParID = Localization.ParseNativeInt(ddParent.SelectedValue);
            if (ParID == AffiliateID)  // prevent case which causes endless recursion
            {
                ParID = 0;
            }

            if (txtEmail.Text.Trim().Length > 0 && !(new EmailAddressValidator()).IsValidEmailAddress(txtEmail.Text.Trim()))
            {
                resetError(AppLogic.GetString("admin.editAffiliates.InvalidEmailFormat", SkinID, LocaleSetting), true);
                return;
            }

            if (txtEmail.Text.Trim().Length > 0 && Affiliate.EmailInUse(txtEmail.Text.Trim(), AffiliateID))
            {
                resetError(AppLogic.GetString("admin.editAffiliates.TakenEmailAddress", SkinID, LocaleSetting), true);
                return;
            }



            if (!Editing)
            {

                // ok to add them:
                String NewGUID = DB.GetNewGUID();
                sql.Append("insert Affiliate(AffiliateGUID,EMail,Password,SaltKey,DateOfBirth,TrackingOnly,IsOnline,DefaultSkinID,FirstName,LastName,[Name],ParentAffiliateID,[Company],Address1,Address2,Suite,City,State,Zip,Country,Phone,WebSiteName,WebSiteDescription,URL) values(");
                sql.Append(CommonLogic.SQuote(NewGUID) + ",");
                sql.Append(CommonLogic.SQuote(CommonLogic.Left(txtEmail.Text.Trim(), 100)) + ",");

                Password p = new Password(CommonLogic.IsNull(ViewState["affpwd"], "").ToString());
                sql.Append(CommonLogic.SQuote(p.SaltedPassword) + ",");
                sql.Append(p.Salt.ToString() + ",");


                try
                {
                    if (txtBirthdate.Text.Length != 0)
                    {
                        DateTime dob = Localization.ParseNativeDateTime(txtBirthdate.Text);
                        sql.Append(DB.SQuote(Localization.ToDBShortDateString(dob)) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                }
                catch
                {
                    sql.Append("NULL,");
                }
                if (rblAdTracking.SelectedValue == "0")
                {
                    sql.Append("0,");
                }
                else
                {
                    sql.Append("1,");
                }
                if (txtWebURL.Text.Length != 0)
                {
                    sql.Append("1,");
                }
                else
                {
                    sql.Append("0,");
                }
                sql.Append(Localization.ParseNativeInt(txtSkin.Text) + ",");
                sql.Append(CommonLogic.SQuote(CommonLogic.Left(txtFirstName.Text, 100)) + ",");
                sql.Append(CommonLogic.SQuote(CommonLogic.Left(txtLastName.Text, 100)) + ",");
                sql.Append(CommonLogic.SQuote(CommonLogic.Left(Name, 100)) + ",");
                sql.Append(ParID.ToString() + ",");
                sql.Append(CommonLogic.SQuote(CommonLogic.Left(txtCompany.Text, 100)) + ",");
                if (txtAddress1.Text.Length != 0)
                {
                    sql.Append(CommonLogic.SQuote(txtAddress1.Text.Replace("\x0D\x0A", "")) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtAddress2.Text.Length != 0)
                {
                    sql.Append(CommonLogic.SQuote(txtAddress2.Text.Replace("\x0D\x0A", "")) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtSuite.Text.Length != 0)
                {
                    sql.Append(CommonLogic.SQuote(txtSuite.Text) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtCity.Text.Length != 0)
                {
                    sql.Append(CommonLogic.SQuote(txtCity.Text) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (ddState.SelectedValue != "0")
                {
                    sql.Append(CommonLogic.SQuote(ddState.SelectedValue) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtZip.Text.Length != 0)
                {
                    sql.Append(CommonLogic.SQuote(txtZip.Text) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (ddCountry.SelectedValue != "0")
                {
                    sql.Append(CommonLogic.SQuote(ddCountry.SelectedValue) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtPhone.Text.Length != 0)
                {
                    sql.Append(CommonLogic.SQuote(txtPhone.Text) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtWebName.Text.Length != 0)
                {
                    sql.Append(CommonLogic.SQuote(txtWebName.Text) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtWebDescription.Text.Length != 0)
                {
                    sql.Append(CommonLogic.SQuote(txtWebDescription.Text) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtWebURL.Text.Length != 0)
                {
                    String theUrl = CommonLogic.Left(txtWebURL.Text, 80);
                    if (theUrl.IndexOf("http://") == -1 && theUrl.Length != 0)
                    {
                        theUrl = "http://" + theUrl;
                    }
                    if (theUrl.Length == 0)
                    {
                        sql.Append("NULL");
                    }
                    else
                    {
                        sql.Append(CommonLogic.SQuote(theUrl) + " ");
                    }
                }
                else
                {
                    sql.Append("NULL");
                }

                sql.Append(")");

                DB.ExecuteSQL(sql.ToString());

                resetError("Affiliate added.", false);

                ResetPasswordRow.Visible = true;
                CreatePasswordRow.Visible = false;

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select AffiliateID from Affiliate where deleted=0 and AffiliateGUID=" + CommonLogic.SQuote(NewGUID), dbconn))
                    {
                        rs.Read();
                        AffiliateID = DB.RSFieldInt(rs, "AffiliateID");
                        ViewState["EditingAffiliate"] = true;
                        ViewState["EditingAffiliateID"] = AffiliateID.ToString();
                    }
                }

                getAffiliateDetails();
            }
            else
            {
                // ok to update:
                sql.Append("update Affiliate set ");
                sql.Append("EMail=" + CommonLogic.SQuote(CommonLogic.Left(txtEmail.Text, 100)) + ",");

                try
                {
                    if (txtBirthdate.Text.Length != 0)
                    {
                        DateTime dob = Localization.ParseNativeDateTime(txtBirthdate.Text);
                        sql.Append("DateOfBirth=" + DB.SQuote(Localization.ToDBShortDateString(dob)) + ",");
                    }
                }
                catch { }
                sql.Append("TrackingOnly=" + CommonLogic.IIF(rblAdTracking.SelectedValue == "1", "1", "0") + ",");
                sql.Append("IsOnline=" + CommonLogic.IIF(txtWebURL.Text.Length == 0, "0", "1") + ",");
                sql.Append("DefaultSkinID=" + Localization.ParseNativeInt(txtSkin.Text) + ",");
                sql.Append("FirstName=" + CommonLogic.SQuote(CommonLogic.Left(txtFirstName.Text, 100)) + ",");
                sql.Append("LastName=" + CommonLogic.SQuote(CommonLogic.Left(txtLastName.Text, 100)) + ",");
                sql.Append("Name=" + CommonLogic.SQuote(CommonLogic.Left(Name, 100)) + ",");
                sql.Append("ParentAffiliateID=" + ParID + ",");
                if (txtCompany.Text.Length != 0)
                {
                    sql.Append("Company=" + CommonLogic.SQuote(txtCompany.Text) + ",");
                }
                else
                {
                    sql.Append("Company=NULL,");
                }
                if (txtAddress1.Text.Length != 0)
                {
                    sql.Append("Address1=" + CommonLogic.SQuote(txtAddress1.Text.Replace("\x0D\x0A", "")) + ",");
                }
                else
                {
                    sql.Append("Address1=NULL,");
                }
                if (txtAddress2.Text.Length != 0)
                {
                    sql.Append("Address2=" + CommonLogic.SQuote(txtAddress2.Text.Replace("\x0D\x0A", "")) + ",");
                }
                else
                {
                    sql.Append("Address2=NULL,");
                }
                if (txtSuite.Text.Length != 0)
                {
                    sql.Append("Suite=" + CommonLogic.SQuote(txtSuite.Text) + ",");
                }
                else
                {
                    sql.Append("Suite=NULL,");
                }
                if (txtCity.Text.Length != 0)
                {
                    sql.Append("City=" + CommonLogic.SQuote(txtCity.Text) + ",");
                }
                else
                {
                    sql.Append("City=NULL,");
                }
                if (ddState.SelectedValue != "0")
                {
                    sql.Append("State=" + CommonLogic.SQuote(ddState.SelectedValue) + ",");
                }
                else
                {
                    sql.Append("State=NULL,");
                }
                if (txtZip.Text.Length != 0)
                {
                    sql.Append("Zip=" + CommonLogic.SQuote(txtZip.Text) + ",");
                }
                else
                {
                    sql.Append("Zip=NULL,");
                }
                if (ddCountry.SelectedValue != "0")
                {
                    sql.Append("Country=" + CommonLogic.SQuote(ddCountry.SelectedValue) + ",");
                }
                else
                {
                    sql.Append("Country=NULL,");
                }
                if (txtPhone.Text.Length != 0)
                {
                    sql.Append("Phone=" + CommonLogic.SQuote(AppLogic.MakeProperPhoneFormat(txtPhone.Text)) + ",");
                }
                else
                {
                    sql.Append("Phone=NULL,");
                }
                if (txtWebName.Text.Length != 0)
                {
                    sql.Append("WebSiteName=" + CommonLogic.SQuote(txtWebName.Text) + ",");
                }
                else
                {
                    sql.Append("WebSiteName=NULL,");
                }
                if (txtWebDescription.Text.Length != 0)
                {
                    sql.Append("WebSiteDescription=" + CommonLogic.SQuote(txtWebDescription.Text) + ",");
                }
                else
                {
                    sql.Append("WebSiteDescription=NULL,");
                }
                if (txtWebURL.Text.Length != 0)
                {
                    String theUrl2 = CommonLogic.Left(txtWebURL.Text, 80);
                    if (theUrl2.IndexOf("http://") == -1 && theUrl2.Length != 0)
                    {
                        theUrl2 = "http://" + theUrl2;
                    }
                    if (theUrl2.Length != 0)
                    {
                        sql.Append("URL=" + CommonLogic.SQuote(theUrl2) + " ");
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

                resetError("Affiliate updated.", false);

                getAffiliateDetails();
            }
        }

        protected void btnSubmit2_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                UpdateInformation();
            }
        }

        public void ValidatePassword(object source, ServerValidateEventArgs args)
        {
            SetPasswordFields();
            string pwd1 = ViewState["affpwd"].ToString();
            string pwd2 = ViewState["affpwd2"].ToString();

            if (pwd1.Length == 0)
            {
                args.IsValid = true;
            }
            else if (pwd1.Trim().Length == 0)
            {
                args.IsValid = false;
                valPassword.ErrorMessage = AppLogic.GetString("account.aspx.74", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            }
            else if (pwd1 == pwd2)
            {
                try
                {
                    valPassword.ErrorMessage = AppLogic.GetString("account.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    if (AppLogic.AppConfigBool("UseStrongPwd"))
                    {

                        if (Regex.IsMatch(pwd1, AppLogic.AppConfig("CustomerPwdValidator"), RegexOptions.Compiled))
                        {
                            args.IsValid = true;
                        }
                        else
                        {
                            args.IsValid = false;
                            valPassword.ErrorMessage = AppLogic.GetString("account.aspx.69", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                        }
                    }
                    else
                    {
                        args.IsValid = (pwd1.Length > 4);
                    }
                }
                catch
                {
                    AppLogic.SendMail(AppLogic.GetString("admin.editAffiliates.InvalidPasswordValidation", SkinID, LocaleSetting), "", false, AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToAddress"), "", "", AppLogic.MailServer());
                    throw new Exception(AppLogic.GetString("admin.editAffiliates.InvalidPasswordExpression", SkinID, LocaleSetting));
                }
            }
            else
            {
                args.IsValid = false;
                valPassword.ErrorMessage = AppLogic.GetString("createaccount.aspx.80", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            }

            if (!args.IsValid)
            {
                ViewState["affpwd"] = "";
                ViewState["affpwd2"] = "";
            }


        }

        private void SetPasswordFields()
        {
            if (ViewState["affpwd"] == null)
            {
                ViewState["affpwd"] = "";
            }
            if (AffPassword.Text.Trim() != "")
            {
                ViewState["affpwd"] = AffPassword.Text;
                reqValPassword.Enabled = false;
            }

            if (ViewState["affpwd2"] == null)
            {
                ViewState["affpwd2"] = "";
            }
            if (AffPassword2.Text != "")
            {
                ViewState["affpwd2"] = AffPassword2.Text;
            }
        }


        protected void ResetPasswordLink_Click(object sender, EventArgs e)
        {
            if (AppLogic.MailServer().Length == 0 || AppLogic.MailServer() == AppLogic.ro_TBD)
            {
                ResetPasswordError.Visible = true;
            }
            else
            {
                Password p = new Password(AspDotNetStorefrontEncrypt.Encrypt.CreateRandomStrongPassword(8));
                Affiliate a = new Affiliate(affiliateID);
                try
                {
                    String Subject = AppLogic.AppConfig("StoreName") + " - " + AppLogic.AppConfig("AppConfig.AffiliateProgramName") + " - " + AppLogic.GetString("cst_account_process.aspx.1", 1, Localization.GetDefaultLocale());
                    String Body = AppLogic.RunXmlPackage("notification.lostpassword.xml.config", null, null, 1, "", "thisaffiliateid=" + a.AffiliateID.ToString() + "&newpwd=" + p.ClearPassword, false, false);
                    AppLogic.SendMail(Subject, Body, true, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromName"), a.EMail, (a.FirstName + " " + a.LastName).Trim(), "", "", AppLogic.MailServer());
                    ResetPasswordOk.Visible = true;
                    a.Update(null, p.SaltedPassword, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, p.Salt);
                }
                catch
                {
                    ResetPasswordOk.Visible = false;
                    ResetPasswordError.Visible = true;
                }
            }

        }
    }
}
