using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{
    /// <summary>
    /// View User Account Information - code behind cs file
    /// </summary>
    public partial class JWMyAccount : SkinBase
    {
        /// <summary>
        /// The m_ store loc
        /// </summary>
        public string m_StoreLoc = AppLogic.GetStoreHTTPLocation(true);
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
                LoadAddresses();
                CurrentOrderStatus();
                LoadAccountInformation();
            }
            if (ThisCustomer == null)
                ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            if(ThisCustomer.CustomerLevelID==(int)UserType.PUBLIC)
              lnkEditAccountInfo.Visible=true;
            
        }
        private void LoadAccountInformation()
        {
            if (ThisCustomer != null)
            {
                lblName.Text = string.IsNullOrEmpty(ThisCustomer.FirstName) ? "" : ThisCustomer.FirstName + " " + ThisCustomer.LastName;
                lblmailId.Text = string.IsNullOrEmpty(ThisCustomer.EMail) ? "" : ThisCustomer.EMail;
                lblPhoneNumber.Text = string.IsNullOrEmpty(ThisCustomer.Phone) ? "" : ThisCustomer.Phone;
            }
        }

        /// <summary>
        /// Load Primary Billing & Shipping Addresses
        /// </summary>
        private void LoadAddresses()
        {
            try
            {
                if (ThisCustomer.PrimaryBillingAddressID == 0)
                    lblBANA.Text = "N/A";
                else if (ThisCustomer.PrimaryBillingAddress != null)
                {
                    lblBAFullName.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.FirstName) ? "" : ThisCustomer.PrimaryBillingAddress.FirstName + " " + ThisCustomer.PrimaryBillingAddress.LastName;
                    lblBAAddress1.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.Address1) ? "" : ThisCustomer.PrimaryBillingAddress.Address1;
                    lblBAAddress2.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.Address2) ? "" : ThisCustomer.PrimaryBillingAddress.Address2;
                    lblBASuite.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.Suite) ? "" : ThisCustomer.PrimaryBillingAddress.Suite;

                    lblBACityStateZip.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.City) ? "" : ThisCustomer.PrimaryBillingAddress.City;
                    lblBACityStateZip.Text += ", ";
                    lblBACityStateZip.Text += string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.State) ? "" : ThisCustomer.PrimaryBillingAddress.State;
                    lblBACityStateZip.Text += " ";
                    lblBACityStateZip.Text += string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.Zip) ? "" : ThisCustomer.PrimaryBillingAddress.Zip;

                    if (string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.City))
                        lblBACityStateZip.Text.Replace(", ", "");
                    if (string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.State))
                    {
                        lblBACityStateZip.Text.Replace(" ", "");
                        lblBACityStateZip.Text.Replace(",", ", ");
                    }

                    lblBACountry.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.Country) ? "" : ThisCustomer.PrimaryBillingAddress.Country;
                    lblBAPhone.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryBillingAddress.Phone) ? "" : ThisCustomer.PrimaryBillingAddress.Phone;
                }

                if (ThisCustomer.PrimaryShippingAddressID == 0)
                    lblSANA.Text = "N/A";
                else if (ThisCustomer.PrimaryShippingAddress != null)
                {
                    lblSAFullName.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.FirstName) ? "" : ThisCustomer.PrimaryShippingAddress.FirstName + " " + ThisCustomer.PrimaryShippingAddress.LastName;
                    lblSAAddress1.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.Address1) ? "" : ThisCustomer.PrimaryShippingAddress.Address1;
                    lblSAAddress2.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.Address2) ? "" : ThisCustomer.PrimaryShippingAddress.Address2;
                    lblSASuite.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.Suite) ? "" : ThisCustomer.PrimaryShippingAddress.Suite;

                    lblSACityStateZip.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.City) ? "" : ThisCustomer.PrimaryShippingAddress.City;
                    lblSACityStateZip.Text += ", ";
                    lblSACityStateZip.Text += string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.State) ? "" : ThisCustomer.PrimaryShippingAddress.State;
                    lblSACityStateZip.Text += " ";
                    lblSACityStateZip.Text += string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.Zip) ? "" : ThisCustomer.PrimaryShippingAddress.Zip;

                    if (string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.City))
                        lblSACityStateZip.Text.Replace(", ", "");
                    if (string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.State))
                    {
                        lblSACityStateZip.Text.Replace(" ", "");
                        lblSACityStateZip.Text.Replace(",", ", ");
                    }

                    lblSACountry.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.Country) ? "" : ThisCustomer.PrimaryShippingAddress.Country;
                    lblSAPhone.Text = string.IsNullOrEmpty(ThisCustomer.PrimaryShippingAddress.Phone) ? "" : ThisCustomer.PrimaryShippingAddress.Phone;
                }
            }
            catch (Exception ex)
            {

                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }
        protected void UpdateAccountInfo_Click(object sender, EventArgs e)
        {
            if (ThisCustomer == null)
                ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            Response.Redirect("JWUpdateAccount.aspx");
        }
        /// <summary>
        /// View All Billing Addresses
        /// </summary>
        protected void btnChangeBillingAddress_Click(object sender, EventArgs e)
        {
            Response.Redirect("JWMyAddresses.aspx?AddressType=" + (int)AddressTypes.Billing, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// View All Shipping Addresses
        /// </summary>
        protected void btnChangeShippingAddress_Click(object sender, EventArgs e)
        {
            Response.Redirect("JWMyAddresses.aspx?AddressType=" + (int)AddressTypes.Shipping, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Set latest Order Status
        /// </summary>
        void CurrentOrderStatus()
        {
            string[] trxStates = { AppLogic.ro_TXStateAuthorized, AppLogic.ro_TXStateCaptured, AppLogic.ro_TXStatePending };
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_GetAllOrders", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PageIndex", 1);
                        cmd.Parameters.AddWithValue("@PageSize", 4);
                        cmd.Parameters.AddWithValue("@TransactionState", String.Join(",", trxStates));
                        cmd.Parameters.AddWithValue("@CustomerID", ThisCustomer.CustomerID);
                        cmd.Parameters.AddWithValue("@AllowCustomerFiltering",
                            CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowCustomerFiltering") == true, 1, 0));
                        cmd.Parameters.AddWithValue("@StoreID", AppLogic.StoreID());

                        IDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            accountaspx55.Visible = false;
                            bOrderNumber.InnerText = reader["OrderNumber"].ToString();
                            aOrderDetail.HRef = "OrderDetail.aspx?ordernumber=" + reader["OrderNumber"].ToString();
                            bStatus.InnerHtml = GetShippingStatus(int.Parse(reader["OrderNumber"].ToString()), reader["ShippedOn"].ToString());                            
                        }
                        else
                        {
                            ulLatestOrderStatus.Visible = false;
                            accountaspx55.Visible = true;
                        }
                        reader.Close();
                        conn.Close();
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
        /// Get Order Shipping Status
        /// </summary>
        /// <param name="OrderNumber">OrderNumber</param>
        /// <param name="ShippedOn">ShippedOn</param>       
        /// <returns>shippingStatus</returns>
        private string GetShippingStatus(int OrderNumber, string ShippedOn)
        {
            var shippingStatus = String.Empty;
            if (AppLogic.OrderHasShippableComponents(OrderNumber))
            {
                shippingStatus = AppLogic.GetString(ShippedOn != "" ? "account.aspx.48" : "account.aspx.52", SkinID, ThisCustomer.LocaleSetting);
            }
            if (AppLogic.OrderHasDownloadComponents(OrderNumber, true))
            {
                shippingStatus += string.Format("<div><a href=\"downloads.aspx\">{0}</a></div>", AppLogic.GetString("download.aspx.1", SkinID, ThisCustomer.LocaleSetting));
            }
            if (shippingStatus.Contains("downloads.aspx") && shippingStatus.Contains("Not Yet Shipped"))
                shippingStatus = "Not Yet Shipped, Downloadable";
            else if (shippingStatus.Contains("downloads.aspx"))
            {
                shippingStatus = "Downloadable";
            }
            return shippingStatus;
        }
    }
}