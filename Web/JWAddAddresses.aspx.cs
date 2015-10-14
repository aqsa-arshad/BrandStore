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
        protected override string OverrideTemplate()
        {
            String MasterHome = AppLogic.HomeTemplate();

            if (MasterHome.Trim().Length == 0)
            {

                MasterHome = "JeldWenTemplate";// "template";
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
                //Change template name to JELD-WEN template by Tayyab on 07-09-2015
                MasterHome = "JeldWenTemplate";// "template.master";
            }

            return MasterHome;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                GetCountryDropDownData();
                GetCustomerAddress(Request.QueryString["AddressID"]);
            }
        }

        private void GetCountryDropDownData()
        {
            ddlCountry.ClearSelection();
            ddlCountry.DataSource = Country.GetAll();
            ddlCountry.DataTextField = "Name";
            ddlCountry.DataValueField = "Name";
            ddlCountry.DataBind();
            ddlCountry.Items.Insert(0, "Please select");
            ddlState.Items.Insert(0, "Please select");
        }

        private void GetStateDropDownData()
        {
            ddlState.ClearSelection();
            ddlState.DataSource = State.GetAllStateForCountry(AppLogic.GetCountryID(ddlCountry.SelectedValue), ThisCustomer.LocaleSetting);
            ddlState.DataTextField = "Name";
            ddlState.DataValueField = "Name";
            ddlState.DataBind();
            ddlState.Items.Insert(0, "Please select");
        }

        protected void ddlCountry_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetStateDropDownData();
        }

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
                    Response.Write(ex.Message);
                }
            }
            else
            {
                btnSave.Visible = true;
            }
        }

        private void LoadFormData(Address anyAddress)
        {
            txtNickName.Text = anyAddress.NickName;
            txtFirstName.Text = anyAddress.FirstName;
            txtLastName.Text = anyAddress.LastName;
            txtPhoneNumber.Text = anyAddress.Phone;
            txtCompany.Text = anyAddress.Company;
            txtAddress1.Text = anyAddress.Address1;
            txtAddress2.Text = anyAddress.Address2;
            txtSuite.Text = anyAddress.Suite;
            txtCity.Text = anyAddress.City;
            ddlCountry.Items.FindByValue(anyAddress.Country).Selected = true;
            GetStateDropDownData();
            ddlState.Items.FindByValue(anyAddress.State).Selected = true;
            txtZip.Text = anyAddress.Zip;
        }

        private Address LoadClassData()
        {
            Address anyAddress = new Address();

            if (!string.IsNullOrEmpty(hfAddressID.Value))
                anyAddress.LoadFromDB(Convert.ToInt32(hfAddressID.Value));

            anyAddress.CustomerID = ThisCustomer.CustomerID;
            anyAddress.NickName = txtNickName.Text.Trim();
            anyAddress.FirstName = txtFirstName.Text.Trim();
            anyAddress.LastName = txtLastName.Text.Trim();
            anyAddress.Phone = txtPhoneNumber.Text.Trim();
            anyAddress.Company = txtCompany.Text.Trim();
            anyAddress.Address1 = txtAddress1.Text.Trim();
            anyAddress.Address2 = txtAddress2.Text.Trim();
            anyAddress.Suite = txtSuite.Text.Trim();
            anyAddress.City = txtCity.Text.Trim();
            anyAddress.Country = ddlCountry.SelectedItem.Text.Trim();
            anyAddress.State = ddlState.SelectedItem.Text.Trim();
            anyAddress.Zip = txtZip.Text.Trim();

            anyAddress.ResidenceType = (int)ResidenceTypes.Unknown;

            return anyAddress;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("JWMyAddresses.aspx");
        }

        protected void btnSave_Click(object sender, EventArgs e)
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

            Response.Redirect("JWMyAddresses.aspx");
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            Address anyAddress = LoadClassData();
            anyAddress.UpdateDB();
            Response.Redirect("JWMyAddresses.aspx");
        }    
    }
}