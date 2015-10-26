using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{
    public partial class JWAddAddresses : SkinBase
    {
        /// <summary>
        /// Override JeldWen Master Template
        /// </summary>
        protected override string OverrideTemplate()
        {
            String MasterHome = AppLogic.HomeTemplate();

            if (MasterHome.Trim().Length == 0)
            {
                MasterHome = "JeldWenTemplate";
            }

            if (MasterHome.EndsWith(".ascx"))
            {
                MasterHome = MasterHome.Replace(".ascx", ".master");
            }

            if (!MasterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
            {
                MasterHome = MasterHome + ".master";
            }

            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + base.SkinID.ToString() + "/" + MasterHome)))
            {
                MasterHome = "JeldWenTemplate";
            }

            return MasterHome;
        }

        /// <summary>
        /// Page Load Event
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            RequireSecurePage();
            RequiresLogin(CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING"));

            if (!Page.IsPostBack)
            {
                hfPreviousURL.Value = Request.UrlReferrer.ToString();
                GetCountryDropDownData();
                GetCustomerAddress(Request.QueryString["AddressID"]);
            }
        }

        /// <summary>
        /// Get Country Data for DropDown List
        /// </summary>
        private void GetCountryDropDownData()
        {
            try
            {
                ddlCountry.ClearSelection();
                ddlCountry.DataSource = Country.GetAll();
                ddlCountry.DataTextField = "Name";
                ddlCountry.DataValueField = "Name";
                ddlCountry.DataBind();
                ddlCountry.Items.Insert(0, "Please select");
                ddlState.Items.Insert(0, "Please select");
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }

        /// <summary>
        /// Get Sate Data for DropDown List
        /// </summary>
        private void GetStateDropDownData()
        {
            try
            {
                ddlState.ClearSelection();
                ddlState.DataSource = State.GetAllStateForCountry(AppLogic.GetCountryID(ddlCountry.SelectedValue), ThisCustomer.LocaleSetting);
                ddlState.DataTextField = "Name";
                ddlState.DataValueField = "Name";
                ddlState.DataBind();
                ddlState.Items.Insert(0, "Please select");
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }

        /// <summary>
        /// Country DropDown List Selection Changed Event
        /// </summary>
        protected void ddlCountry_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetStateDropDownData();
        }

        /// <summary>
        /// Get Customer Address
        /// </summary>
        /// <param name="AddressID">AddressID</param>
        private void GetCustomerAddress(string AddressID)
        {
            if (!string.IsNullOrEmpty(AddressID))
            {
                try
                {
                    int addressID = int.Parse(AddressID);
                    hfAddressID.Value = addressID.ToString();
                    btnUpdate.Visible = true;
                    Address anyAddress = new Address();
                    anyAddress.LoadFromDB(addressID);

                    if (anyAddress != null)
                        LoadFormData(anyAddress);

                }
                catch (Exception ex)
                {
                    SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                    MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                }
            }
            else
            {
                btnSave.Visible = true;
            }
        }

        /// <summary>
        /// Load Form Fields from Address Class 
        /// </summary>
        /// <param name="anyAddress">anyAddress</param>
        private void LoadFormData(Address anyAddress)
        {
            try
            {
                txtNickName.Text = anyAddress.NickName;
                txtFirstName.Text = anyAddress.FirstName;
                txtLastName.Text = anyAddress.LastName;
                txtPhoneNumber.Text = anyAddress.Phone;
                txtCompany.Text = anyAddress.Company;
                txtAddress1.Text = anyAddress.Address1;
                txtAddress2.Text = anyAddress.Address2;
                txtSuite.Text = anyAddress.Suite.StartsWith("Suite ") ? anyAddress.Suite.Remove(0, 6) : anyAddress.Suite;
                txtCity.Text = anyAddress.City;
                if (!string.IsNullOrEmpty(anyAddress.Country))
                    ddlCountry.Items.FindByValue(anyAddress.Country).Selected = true;
                GetStateDropDownData();
                if (!string.IsNullOrEmpty(anyAddress.State))
                    ddlState.Items.FindByValue(anyAddress.State).Selected = true;
                txtZip.Text = anyAddress.Zip;
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }

        /// <summary>
        /// Load Address Class from Form Fields
        /// </summary>
        /// <returns>Address</returns>
        private Address LoadClassData()
        {
            Address anyAddress = new Address();
            try
            {
                if (!string.IsNullOrEmpty(hfAddressID.Value))
                {
                    anyAddress.LoadFromDB(Convert.ToInt32(hfAddressID.Value));
                }
                anyAddress.CustomerID = ThisCustomer.CustomerID;
                anyAddress.NickName = txtNickName.Text.Trim();
                anyAddress.FirstName = txtFirstName.Text.Trim();
                anyAddress.LastName = txtLastName.Text.Trim();
                anyAddress.Phone = txtPhoneNumber.Text.Trim();
                anyAddress.Company = txtCompany.Text.Trim();
                anyAddress.Address1 = txtAddress1.Text.Trim();
                anyAddress.Address2 = txtAddress2.Text.Trim();
                anyAddress.Suite = char.IsLetter(txtSuite.Text.Trim().FirstOrDefault()) ? txtSuite.Text.Trim() : "Suite " + txtSuite.Text.Trim();
                anyAddress.City = txtCity.Text.Trim();
                anyAddress.Country = ddlCountry.SelectedItem.Text.Trim();
                anyAddress.State = ddlState.SelectedItem.Text.Trim();
                anyAddress.Zip = txtZip.Text.Trim();

                anyAddress.ResidenceType = (int)ResidenceTypes.Unknown;
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return anyAddress;
        }

        /// <summary>
        /// Cancel Button Click Event
        /// </summary>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(hfPreviousURL.Value);
        }

        /// <summary>
        /// Save Form Data
        /// </summary>
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                Address anyAddress = LoadClassData();
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
                Response.Redirect(hfPreviousURL.Value);
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }

        /// <summary>
        /// Update Form Data
        /// </summary>
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                Address anyAddress = LoadClassData();
                anyAddress.UpdateDB();
                Response.Redirect(hfPreviousURL.Value);
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }
    }
}