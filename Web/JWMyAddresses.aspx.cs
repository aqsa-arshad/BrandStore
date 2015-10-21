using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{
    public partial class JWMyAddresses : SkinBase
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
                LoadAddresses(GetAddressType(Request.QueryString["AddressType"]));
            }
        }

        /// <summary>
        /// Load All Customer Addresses
        /// </summary>
        private void LoadAddresses(int addressType)
        {
            if (addressType == (int)AddressTypes.Billing)
            {
                ((Label)this.Master.FindControl("lblPageHeading")).Text = "MY Billing ADDRESSES";
            }
            else if (addressType == (int)AddressTypes.Shipping)
            {
                ((Label)this.Master.FindControl("lblPageHeading")).Text = "MY Shipping ADDRESSES";
            }
            else
                return;

            Addresses allAddresses = new Addresses();
            allAddresses.LoadCustomer(ThisCustomer.CustomerID);

            rptAddresses.DataSource = allAddresses;
            rptAddresses.DataBind();
        }

        /// <summary>
        /// Get AddressType int from AddressType QueryString
        /// </summary>
        /// <param name="AddressType">AddressType</param>
        /// <returns>addressType</returns>
        private int GetAddressType(string AddressType)
        {
            int addressType = 0;

            if (!string.IsNullOrEmpty(AddressType))
            {
                if (Int32.TryParse(AddressType, out addressType))
                    return addressType;
            }

            return addressType;
        }

        /// <summary>
        /// Add New Address Event
        /// </summary>
        protected void btnAddAddresses_Click(object sender, EventArgs e)
        {
            Response.Redirect("JWAddAddresses.aspx");
        }

        /// <summary>
        /// Repeater ItemCommand Event
        /// </summary>
        protected void rptAddresses_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int addressID = 0;
            if (Int32.TryParse(e.CommandArgument.ToString(), out addressID))
            {
                if (e.CommandName == "Delete")
                {
                    DeleteAddress(addressID);
                }
                else if (e.CommandName == "Edit")
                {
                    EditAddress(addressID);
                }
                else if (e.CommandName == "Select")
                {
                    SelectPrimaryAddress(addressID);
                }
            }

        }

        /// <summary>
        /// Repeater Data Binding Event
        /// </summary>
        protected void rptAddresses_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                int addressType = GetAddressType(Request.QueryString["AddressType"]);
                int itemAddressID = Int32.Parse((e.Item.FindControl("hfAddressID") as HiddenField).Value);
                int primaryID = 0;

                if (addressType == (int)AddressTypes.Billing)
                {
                    primaryID = AppLogic.GetPrimaryBillingAddressID(ThisCustomer.CustomerID);
                    if (itemAddressID == primaryID)
                    {
                        Button btnSelect = (Button)e.Item.FindControl("btnSelect");
                        btnSelect.Enabled = false;
                        btnSelect.Text = "Primary Billing Address";
                    }
                }
                else if (addressType == (int)AddressTypes.Shipping)
                {
                    primaryID = AppLogic.GetPrimaryShippingAddressID(ThisCustomer.CustomerID);
                    if (itemAddressID == primaryID)
                    {
                        Button btnSelect = (Button)e.Item.FindControl("btnSelect");
                        btnSelect.Enabled = false;
                        btnSelect.Text = "Primary Shipping Address";
                    }
                }
            }
        }

        /// <summary>
        /// Delete Customer Address by addressID
        /// </summary>
        /// <param name="addressID">addressID</param>
        private void DeleteAddress(int addressID)
        {
            AspDotNetStorefrontCore.Address anyAddress = new AspDotNetStorefrontCore.Address();
            anyAddress.LoadFromDB(addressID);

            if (ThisCustomer.CustomerID == anyAddress.CustomerID || ThisCustomer.IsAdminSuperUser)
            {
                AspDotNetStorefrontCore.Address.DeleteFromDB(anyAddress.AddressID, ThisCustomer.CustomerID);
            }

            LoadAddresses(GetAddressType(Request.QueryString["AddressType"]));
        }

        /// <summary>
        /// Edit Customer Address by addressID
        /// </summary>
        /// <param name="addressID">addressID</param>
        private void EditAddress(int addressID)
        {
            Response.Redirect("JWAddAddresses.aspx?AddressID=" + addressID);
        }

        /// <summary>
        /// Select Primary Address
        /// </summary>
        /// <param name="addressID">addressID</param>
        private void SelectPrimaryAddress(int addressID)
        {
            int addressType = GetAddressType(Request.QueryString["AddressType"]);
            if (addressType == (int)AddressTypes.Billing)
                ThisCustomer.UpdateCustomer(new SqlParameter[] { new SqlParameter("BillingAddressID", addressID) });

            else if (addressType == (int)AddressTypes.Shipping)
                ThisCustomer.UpdateCustomer(new SqlParameter[] { new SqlParameter("ShippingAddressID", addressID) });

            LoadAddresses(addressType);
        }

        /// <summary>
        /// Back Button Event
        /// </summary>
        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("JWMyAccount.aspx");
        }

    }
}