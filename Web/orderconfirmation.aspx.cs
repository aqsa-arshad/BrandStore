using System;
using System.Data;
using System.Data.SqlClient;
using AspDotNetStorefrontCore;
using System.Net;
using System.Configuration;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{
    public partial class orderconfirmation : SkinBase
    {
        protected int OrderNumber;
        private IDataReader reader;
        private IDataReader reader2;
        protected string m_StoreLoc = AppLogic.GetStoreHTTPLocation(true);
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            SkinBase.RequireSecurePage();

            OrderNumber = CommonLogic.QueryStringUSInt("ordernumber");
            int OrderCustomerID = Order.GetOrderCustomerID(OrderNumber);

            lnkreceipt.HRef = "OrderReceipt.aspx?ordernumber=" + OrderNumber.ToString() + "&customerid=" + OrderCustomerID.ToString();

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            // who is logged in now viewing this page:

            // currently viewing user must be logged in to view receipts:
            if (!ThisCustomer.IsRegistered)
            {
                Response.Redirect("signin.aspx?returnurl=receipt.aspx?" +
                                  Server.UrlEncode(CommonLogic.ServerVariables("QUERY_STRING")));
            }

            // are we allowed to view?
            // if currently logged in user is not the one who owns the order, and this is not an admin user who is logged in, reject the view:
            if (ThisCustomer.CustomerID != OrderCustomerID && !ThisCustomer.IsAdminUser)
            {
                Response.Redirect("OrderNotFound.aspx");
            }

            //For multi store checking
            //Determine if customer is allowed to view orders from other store.
            if (!ThisCustomer.IsAdminUser && AppLogic.StoreID() != AppLogic.GetOrdersStoreID(OrderNumber) &&
                AppLogic.GlobalConfigBool("AllowCustomerFiltering") == true)
            {
                Response.Redirect("OrderNotFound.aspx");
            }
            if (!Page.IsPostBack)
            {
                GetOrderInfo();
                GetOrderItemsDetail();
                SendOrderinfotoRRD();
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
                            lblOrderNumber.Text = reader["OrderNumber"].ToString() + ".";
                            //lblOrderDate.Text = reader["OrderDate"].ToString();
                            // lblCustomerID.Text = reader["CustomerID"].ToString();

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
                            lblSubTotal.Text = Math.Round(Convert.ToDecimal(reader["OrderSubtotal"]), 2).ToString();
                            lblTax.Text = Math.Round(Convert.ToDecimal(reader["OrderTax"]), 2).ToString();
                            lblShippingCost.Text = Math.Round(Convert.ToDecimal(reader["OrderShippingCosts"]), 2).ToString();
                            lblTotalAmount.Text = Math.Round(Convert.ToDecimal(reader["OrderTotal"]), 2).ToString();
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

        private void lblreceipt_click()
        {
            // String ReceiptURL = "receipt.aspx?ordernumber=" + OrderNumber.ToString() + "&customerid=" + CustomerID.ToString();
        }
        #region "Send order to RRD"
        private void SendOrderinfotoRRD()
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
                        reader2 = cmd.ExecuteReader();
                        int totalRRDRow = 0;
                        while (reader2.Read())
                        {
                            if ((reader2["DistributorName"].ToString() == AppLogic.GetString("Fullfilment Vendor RRD", SkinID, ThisCustomer.LocaleSetting)) 
                                    || (reader2["DistributorName"].ToString() == AppLogic.GetString("Fullfilment Vendor CDS Publications", SkinID, ThisCustomer.LocaleSetting)) 
                                    || (reader2["DistributorName"].ToString() == AppLogic.GetString("Fullfilment Vendor Wetzel Brothers", SkinID, ThisCustomer.LocaleSetting)))
                                totalRRDRow++;
                        }
                        reader2.Close();
                        reader = cmd.ExecuteReader();
                        orderService.brandstore.ws.orderService os = new orderService.brandstore.ws.orderService();
                        orderService.brandstore.ws.Credentials c = new orderService.brandstore.ws.Credentials();
                        orderService.brandstore.ws.BillingAddress Ba = new orderService.brandstore.ws.BillingAddress();
                        orderService.brandstore.ws.ShippingAddress Sa = new orderService.brandstore.ws.ShippingAddress();
                        orderService.brandstore.ws.Product p;
                        orderService.brandstore.ws.Product[] pa = new orderService.brandstore.ws.Product[totalRRDRow];

                        // Set the authentication
                        c.Username = AppLogic.AppConfig("fullfillmentapi_username");
                        c.Token = AppLogic.AppConfig("fullfillmentapi_password");
                        SetBillingAndShippingAddresses(ref Ba, ref Sa, OrderNumber);
                        int index = 0;
                        bool hasproducts = false;
                        while (reader.Read())
                        {
                            if ((reader["DistributorName"].ToString() == AppLogic.GetString("Fullfilment Vendor RRD", SkinID, ThisCustomer.LocaleSetting)) 
                                    || (reader["DistributorName"].ToString() == AppLogic.GetString("Fullfilment Vendor CDS Publications", SkinID, ThisCustomer.LocaleSetting)) 
                                    || (reader["DistributorName"].ToString() == AppLogic.GetString("Fullfilment Vendor Wetzel Brothers", SkinID, ThisCustomer.LocaleSetting)))
                            {
                                p = new orderService.brandstore.ws.Product();
                                // set the product
                                p.ID = reader["ProductID"].ToString();
                                p.Quantity = reader["Quantity"].ToString();
                                p.SKU = reader["SKU"].ToString();
                                p.Description = reader["OrderedProductName"].ToString();
                                pa[index] = p;
                                index++;
                                hasproducts = true;
                            }
                        }

                        // call the service
                        if (hasproducts)
                        {
                            orderService.brandstore.ws.ReturnStatus rs = os.processOrder(c, OrderNumber.ToString(), OrderNumber.ToString(), Ba, Sa, DateTime.Now, pa, "RRD");
                            bool isok = rs.status.Equals(0) ? false : true;
                        }
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

        private void SetBillingAndShippingAddresses(ref orderService.brandstore.ws.BillingAddress Ba, ref orderService.brandstore.ws.ShippingAddress Sa, int OrderNumber)
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
                        reader2 = cmd.ExecuteReader();
                        if (reader2.Read())
                        {
                            //Set Billing address
                            Ba.Name1 = reader2["BillingFirstName"].ToString() + ' ' + reader2["BillingLastName"].ToString();
                            Ba.Name2 = "";
                            Ba.Email = String.IsNullOrEmpty(reader2["Email"].ToString()) ? String.Empty : reader2["Email"].ToString();
                            Ba.Address1 = String.IsNullOrEmpty(reader2["BillingAddress1"].ToString())? String.Empty :reader2["BillingAddress1"].ToString() ;
                            Ba.Address2 = String.IsNullOrEmpty(reader2["BillingAddress2"].ToString()) ? String.Empty : reader2["BillingAddress2"].ToString() + " "+(String.IsNullOrEmpty(reader2["BillingSuite"].ToString()) ? String.Empty : reader2["BillingSuite"].ToString());
                            Ba.City = String.IsNullOrEmpty(reader2["BillingCity"].ToString()) ? String.Empty : reader2["BillingCity"].ToString();
                            Ba.Locale = String.IsNullOrEmpty(reader2["BillingState"].ToString()) ? String.Empty : reader2["BillingState"].ToString();
                            Ba.Country = String.IsNullOrEmpty(reader2["BillingCountryCode"].ToString()) ? String.Empty : reader2["BillingCountryCode"].ToString();
                            Ba.PostalCode = String.IsNullOrEmpty(reader2["BillingZip"].ToString()) ? String.Empty : reader2["BillingZip"].ToString();
                                                       
                            //Set Shipping Address                       

                            Sa.Name1 = reader2["ShippingFirstName"].ToString() + ' ' + reader2["ShippingLastName"].ToString();
                            Sa.Name2 = "";
                            Sa.Email = String.IsNullOrEmpty(reader2["Email"].ToString()) ? String.Empty : reader2["Email"].ToString();
                            Sa.Address1 = String.IsNullOrEmpty(reader2["ShippingAddress1"].ToString()) ? String.Empty : reader2["ShippingAddress1"].ToString();
                            Sa.Address2 = String.IsNullOrEmpty(reader2["ShippingAddress2"].ToString()) ? String.Empty : reader2["ShippingAddress2"].ToString() + " " + (String.IsNullOrEmpty(reader2["ShippingSuite"].ToString()) ? String.Empty : reader2["BillingSuite"].ToString());
                            Sa.City = String.IsNullOrEmpty(reader2["ShippingCity"].ToString()) ? String.Empty : reader2["ShippingCity"].ToString();
                            Sa.Locale = String.IsNullOrEmpty(reader2["ShippingState"].ToString()) ? String.Empty : reader2["ShippingState"].ToString();
                            Sa.Country = String.IsNullOrEmpty(reader2["ShippingCountryCode"].ToString()) ? String.Empty : reader2["ShippingCountryCode"].ToString();
                            Sa.PostalCode = String.IsNullOrEmpty(reader2["ShippingZip"].ToString()) ? String.Empty : reader2["ShippingZip"].ToString();



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
        #endregion

        protected void rptAddresses_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                if ((e.Item.FindControl("hfIsDownload") as HiddenField).Value != "0")
                {
                    (e.Item.FindControl("hlDelivery") as HyperLink).NavigateUrl = (e.Item.FindControl("hfDownloadLocation") as HiddenField).Value;
                    (e.Item.FindControl("hlDelivery") as HyperLink).Text = "Download";
                    (e.Item.FindControl("lblDelivery") as Label).Visible = false;
                }
                else
                {
                    (e.Item.FindControl("lblDelivery") as Label).Visible = false;

                }

            }
        }

    }
}