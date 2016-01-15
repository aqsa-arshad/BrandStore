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
                        cmd.Parameters.Add("@RecordCount", SqlDbType.Int, 4);
                        cmd.Parameters["@RecordCount"].Direction = ParameterDirection.Output;
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
                            bStatus.InnerText = GetShippingStatus(int.Parse(reader["OrderNumber"].ToString()), reader["ShippedOn"].ToString(), reader["ShippedVIA"].ToString(), reader["ShippingTrackingNumber"].ToString(), reader["TransactionState"].ToString(), reader["DownloadEMailSentOn"].ToString());
                            if (string.IsNullOrEmpty(SetTrackingPath(int.Parse(reader["OrderNumber"].ToString()))))
                            {
                                hlTrackItem.Visible = false;
                            }
                            else
                            {
                                hlTrackItem.NavigateUrl = SetTrackingPath(int.Parse(reader["OrderNumber"].ToString()));
                            }
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
        /// Set Tracking Path against Order
        /// </summary>
        /// <param name="orderNumber">orderNumber</param>
        /// <returns></returns>
        static string SetTrackingPath(int orderNumber)
        {
            List<TrackingInformation> lstTrackingInformation = TrackingInformationLogic.GetTrackingInformation(orderNumber);

            using (var conn = DB.dbConn())
            {
                conn.Open();
                using (var cmd = new SqlCommand("aspdnsf_GetOrderItemsDetail", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ORDERNUMBER", orderNumber);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (AppLogic.AppConfig("RTShipping.ActiveCarrier") != null)
                        {
                            var carrierList = AppLogic.AppConfig("RTShipping.ActiveCarrier").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var listItem in carrierList.Where(listItem => (reader["ShippingMethod"].ToString().ToUpper().Contains(listItem.ToUpper()) && reader["IsDownload"].ToString() != "1")))
                            {
                                if (!string.IsNullOrEmpty(reader["ShippingTrackingNumber"].ToString()))
                                {
                                    return
                                        string.Format(AppLogic.AppConfig("ShippingTrackingURL." + listItem),
                                            reader["ShippingTrackingNumber"]);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Get Order Shipping Status
        /// </summary>
        /// <param name="OrderNumber">OrderNumber</param>
        /// <param name="ShippedOn">ShippedOn</param>
        /// <param name="ShippedVIA">ShippedVIA</param>
        /// <param name="ShippingTrackingNumber">ShippingTrackingNumber</param>
        /// <param name="TransactionState">TransactionState</param>
        /// <param name="DownloadEMailSentOn">DownloadEMailSentOn</param>
        /// <returns>shippingStatus</returns>
        private string GetShippingStatus(int OrderNumber, string ShippedOn, string ShippedVIA, string ShippingTrackingNumber, string TransactionState, string DownloadEMailSentOn)
        {
            var shippingStatus = String.Empty;
            if (AppLogic.OrderHasShippableComponents(OrderNumber))
            {
                if (ShippedOn != "")
                {
                    shippingStatus = AppLogic.GetString("account.aspx.48", SkinID, ThisCustomer.LocaleSetting);
                    if (ShippedVIA.Length != 0)
                    {
                        shippingStatus += " " + AppLogic.GetString("account.aspx.49", SkinID, ThisCustomer.LocaleSetting) + " " + ShippedVIA;
                    }

                    shippingStatus += " " + AppLogic.GetString("account.aspx.50", SkinID, ThisCustomer.LocaleSetting) + " " + Localization.ParseNativeDateTime(ShippedOn).ToString(new CultureInfo(ThisCustomer.LocaleSetting));
                    if (ShippingTrackingNumber.Length != 0)
                    {
                        shippingStatus += " " + AppLogic.GetString("account.aspx.51", SkinID, ThisCustomer.LocaleSetting) + " ";

                        var trackUrl = Shipping.GetTrackingURL(ShippingTrackingNumber);
                        if (trackUrl.Length != 0)
                        {
                            shippingStatus += "<a href=\"" + trackUrl + "\" target=\"_blank\">" + ShippingTrackingNumber + "</a>";
                        }
                        else
                        {
                            shippingStatus += ShippingTrackingNumber;
                        }
                    }
                }
                else
                {
                    shippingStatus = AppLogic.GetString("account.aspx.52", SkinID, ThisCustomer.LocaleSetting);
                }
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
                hlTrackItem.Visible = false;
            }
            return shippingStatus;
        }
    }
}