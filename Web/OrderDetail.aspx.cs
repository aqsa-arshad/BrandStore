using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using AspDotNetStorefrontCore;


namespace AspDotNetStorefront
{
    public partial class OrderDetail : SkinBase
    {
        protected int OrderNumber;
        private IDataReader reader;
        protected string m_StoreLoc = AppLogic.GetStoreHTTPLocation(true);

        protected void Page_Load(object sender, EventArgs e)
        {
            RequireSecurePage();
            if (ThisCustomer.CustomerLevelID == 4 || ThisCustomer.CustomerLevelID == 5 || ThisCustomer.CustomerLevelID == 6)
            {
                ((System.Web.UI.WebControls.Label)Master.FindControl("lblPageHeading")).Text = "ORDER DETAILS FOR " + GetDealerName(ThisCustomer.CustomerID);
            }

            if (!Page.IsPostBack)
            {
                ClientScriptManager cs = Page.ClientScript;
                cs.RegisterClientScriptBlock(this.GetType(), Guid.NewGuid().ToString(), "function ReOrder(OrderNumber) {if(confirm('" + AppLogic.GetString("account.aspx.64", SkinID, ThisCustomer.LocaleSetting) + "')) {top.location.href='reorder.aspx?ordernumber='+OrderNumber;} }", true);
                OrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");
                GetOrderInfo();
                GetOrderItemsDetail();
                hplReOrder.NavigateUrl = "javascript: ReOrder(" + OrderNumber + ");";
            }
        }

        void GetOrderInfo()
        {
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_GetOrderDetail", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ORDERNUMBER", OrderNumber);

                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            lblOrderNumber.Text = reader["OrderNumber"].ToString();
                            lblOrderDate.Text = reader["OrderDate"].ToString();
                            //Billing Address
                            lblBAFullName.Text = reader["BillingFirstName"].ToString() + ' ' +
                                                 reader["BillingLastName"].ToString();
                            lblBACompany.Text = reader["BillingCompany"].ToString();
                            lblBAAddress1.Text = reader["BillingAddress1"].ToString();
                            lblBAAddress2.Text = reader["BillingAddress2"].ToString();
                            lblBASuite.Text = reader["BillingSuite"].ToString();
                            lblBACityStateZip.Text = reader["BillingCity"] + ", " + reader["BillingState"] + ' ' +
                                                     reader["BillingZip"];
                            lblBACountry.Text = reader["BillingCountry"].ToString();
                            lblBAPhone.Text = reader["BillingPhone"].ToString();
                            //Shipping Address
                            lblSAFullName.Text = reader["ShippingFirstName"].ToString() + ' ' +
                                                 reader["ShippingLastName"].ToString();
                            lblSACompany.Text = reader["ShippingCompany"].ToString();
                            lblSAAddress1.Text = reader["ShippingAddress1"].ToString();
                            lblSAAddress2.Text = reader["ShippingAddress2"].ToString();
                            lblSASuite.Text = reader["ShippingSuite"].ToString();
                            lblSACityStateZip.Text = reader["ShippingCity"] + ", " + reader["ShippingState"] + ' ' +
                                                     reader["ShippingZip"];
                            lblSACountry.Text = reader["ShippingCountry"].ToString();
                            lblSAPhone.Text = reader["ShippingPhone"].ToString();
                            //Payment Methods
                            lblPMCardInfo.Text = reader["CardType"].ToString() + ' ' +
                                                 (string.Concat("*********", reader["CardNumber"].ToString()));
                            lblPMExpireDate.Text = "Expires: " + reader["CardExpirationMonth"].ToString() + '/' +
                                                   reader["CardExpirationYear"].ToString();
                            lblPMCountry.Text = reader["BillingCountry"].ToString();
                            //Billing Amounts
                            lblSubTotal.Text = reader["OrderSubtotal"].ToString();
                            lblTax.Text = reader["OrderTax"].ToString();
                            lblShippingCost.Text = reader["OrderShippingCosts"].ToString();
                            lblTotalAmount.Text = reader["OrderTotal"].ToString();
                        }
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

        private void GetOrderItemsDetail()
        {
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_GetOrderItemsDetail", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ORDERNUMBER", OrderNumber);
                        reader = cmd.ExecuteReader();
                        rptOrderItemsDetail.DataSource = reader;
                        rptOrderItemsDetail.DataBind();
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

        protected override string OverrideTemplate()
        {
            var masterHome = AppLogic.HomeTemplate();
            if (masterHome.Trim().Length == 0)
            {
                masterHome = "JeldWenTemplate";
            }
            if (masterHome.EndsWith(".ascx"))
            {
                masterHome = masterHome.Replace(".ascx", ".master");
            }
            if (!masterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
            {
                masterHome = masterHome + ".master";
            }
            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + SkinID + "/" + masterHome)))
            {
                masterHome = "JeldWenTemplate";
            }
            return masterHome;
        }

        private static string GetDealerName(int customerId)
        {
            var customerName = string.Empty;
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    var query = "select FirstName + ', ' + LastName as CustomerName from Customer where CustomerID = " +
                                customerId;
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        IDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                            customerName = reader["CustomerName"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(
                    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " +
                    System.Reflection.MethodBase.GetCurrentMethod().Name,
                    ex.Message +
                    ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message))
                        ? " :: " + ex.InnerException.Message
                        : ""),
                    MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return customerName;
        }
    }
}