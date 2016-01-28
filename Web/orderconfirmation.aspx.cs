using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Net;
using System.Linq;
using System.Web.UI;
using System.Configuration;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Handle the Order confirmation
    /// </summary>
    [PageType("orderconfirmation")] //added in by Mauricio to fix Analytics Ecommerce problem

    public partial class orderconfirmation : SkinBase
    {
        /// <summary>
        /// Used for the total of blu bucks used
        /// </summary>
        private decimal totalBluBucks;
        /// <summary>
        /// List for the used funds
        /// </summary>
        List<decimal> lstFund = Enumerable.Repeat(0m, 7).ToList();
        /// <summary>
        /// The order number
        /// </summary>
        protected int OrderNumber;

        /// <summary>
        /// The Data reader for reading Data from SQL
        /// </summary>
        private IDataReader reader;

        /// <summary>
        /// The reader2
        /// </summary>
        private IDataReader reader2;

        /// <summary>
        /// The m_ store loc
        /// </summary>
        protected string m_StoreLoc = AppLogic.GetStoreHTTPLocation(true);

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Gets the order information.
        /// </summary>
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
                            lblSubTotal.Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), Convert.ToDecimal(reader["OrderSubtotal"]));
                            lblTax.Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), Convert.ToDecimal(reader["OrderTax"]));
                            lblShippingCost.Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), Convert.ToDecimal(reader["OrderShippingCosts"]));
                            lblTotalAmount.Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), Convert.ToDecimal(reader["OrderTotal"]));
                            for (var i = 2; i < 7; i++)
                            {
                                if (Convert.ToDecimal(reader[i.ToString()].ToString()) != 0)
                                {
                                    lstFund[i] =
                                        lstFund[i] + Convert.ToDecimal(reader[i.ToString()].ToString());

                                    if (lstFund[i] != 0 && i == (int)FundType.SOFFunds)
                                    {
                                        lblSOFFundsTotal.Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                        lblSOFFundsTotal.Visible = true;
                                        lblSOFFundsTotalCaption.Visible = true;
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.DirectMailFunds)
                                    {
                                        lblDirectMailFundsTotal.Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                        lblDirectMailFundsTotal.Visible = true;
                                        lblDirectMailFundsTotalCaption.Visible = true;
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.DisplayFunds)
                                    {
                                        lblDisplayFundsTotal.Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                        lblDisplayFundsTotal.Visible = true;
                                        lblDisplayFundsTotalCaption.Visible = true;
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.LiteratureFunds)
                                    {
                                        lblLiteratureFundsTotal.Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                        lblLiteratureFundsTotal.Visible = true;
                                        lblLiteratureFundsTotalCaption.Visible = true;
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.POPFunds)
                                    {
                                        lblPOPFundsTotal.Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                        lblPOPFundsTotal.Visible = true;
                                        lblPOPFundsTotalCaption.Visible = true;
                                    }
                                }
                            }
                            if (lstFund.Sum(x => Convert.ToDecimal(x)) <= 0)
                            {
                                lblCreditsUsedCaption.Visible = false;
                            }
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

        /// <summary>
        /// Gets the order items detail.
        /// </summary>
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

        /// <summary>
        /// Used to set the master page when using template switching or page-based templates
        /// </summary>
        /// <returns>
        /// The name of the template to use.  To utilize this you must override OverrideTemplate
        /// in a page that inherits from SkinBase where you're trying to change the master page
        /// </returns>
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
            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + base.SkinID.ToString() + "/" + masterHome)))
            {
                masterHome = "JeldWenTemplate";
            }
            return masterHome;
        }

        #region "Send order to RRD"
        /// <summary>
        /// Sends the orderinfoto RRD.
        /// </summary>
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
                        string shippingMethodCode = string.Empty;
                        string shippingMethod = string.Empty;

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
                                shippingMethodCode = reader["ShippingMethodCode"].ToString();
                                shippingMethod = reader["ShippingMethod"].ToString();
                            }
                        }

                        // call the service after verification if the shopping cart has RRD Product & UseFulfillmentAPI flag is true
                        if (hasproducts && AppLogic.AppConfig("UseFulfillmentAPI").ToBool())
                        {
                            orderService.brandstore.ws.ReturnStatus rs = os.processOrder(c, OrderNumber.ToString(), OrderNumber.ToString(), Ba, Sa, DateTime.Now, pa, AppLogic.GetString("Fullfilment Vendor RRDParam", SkinID, ThisCustomer.LocaleSetting), shippingMethodCode, shippingMethod);
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

        /// <summary>
        /// Sets the billing and shipping addresses.
        /// </summary>
        /// <param name="Ba">The ba.</param>
        /// <param name="Sa">The sa.</param>
        /// <param name="OrderNumber">The order number.</param>
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
                            Ba.Address1 = String.IsNullOrEmpty(reader2["BillingAddress1"].ToString()) ? String.Empty : reader2["BillingAddress1"].ToString();
                            Ba.Address2 = String.IsNullOrEmpty(reader2["BillingAddress2"].ToString()) ? String.Empty : reader2["BillingAddress2"].ToString() + " " + (String.IsNullOrEmpty(reader2["BillingSuite"].ToString()) ? String.Empty : reader2["BillingSuite"].ToString());
                            Ba.City = String.IsNullOrEmpty(reader2["BillingCity"].ToString()) ? String.Empty : reader2["BillingCity"].ToString();
                            Ba.Locale = String.IsNullOrEmpty(reader2["BillingStateName"].ToString()) ? String.Empty : reader2["BillingStateName"].ToString();
                            Ba.Country = String.IsNullOrEmpty(reader2["BillingCountryCode"].ToString()) ? String.Empty : reader2["BillingCountryCode"].ToString();
                            Ba.PostalCode = String.IsNullOrEmpty(reader2["BillingZip"].ToString()) ? String.Empty : reader2["BillingZip"].ToString();

                            //Set Shipping Address                       

                            Sa.Name1 = reader2["ShippingFirstName"].ToString() + ' ' + reader2["ShippingLastName"].ToString();
                            Sa.Name2 = "";
                            Sa.Email = String.IsNullOrEmpty(reader2["Email"].ToString()) ? String.Empty : reader2["Email"].ToString();
                            Sa.Address1 = String.IsNullOrEmpty(reader2["ShippingAddress1"].ToString()) ? String.Empty : reader2["ShippingAddress1"].ToString();
                            Sa.Address2 = String.IsNullOrEmpty(reader2["ShippingAddress2"].ToString()) ? String.Empty : reader2["ShippingAddress2"].ToString() + " " + (String.IsNullOrEmpty(reader2["ShippingSuite"].ToString()) ? String.Empty : reader2["BillingSuite"].ToString());
                            Sa.City = String.IsNullOrEmpty(reader2["ShippingCity"].ToString()) ? String.Empty : reader2["ShippingCity"].ToString();
                            Sa.Locale = String.IsNullOrEmpty(reader2["ShippingStateName"].ToString()) ? String.Empty : reader2["ShippingStateName"].ToString();
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

        /// <summary>
        /// Handles the ItemDataBound event of the rptAddresses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptOrderItemsDetail_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                (e.Item.FindControl("lblRegularPrice") as Label).Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), (Convert.ToDecimal((e.Item.FindControl("hfRegularPrice") as HiddenField).Value)));
                (e.Item.FindControl("lblCreditPrice") as Label).Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), (Convert.ToDecimal((e.Item.FindControl("hfCreditPrice") as HiddenField).Value)));

                (e.Item.FindControl("ImgProduct") as Image).ImageUrl = !string.IsNullOrEmpty((e.Item.FindControl("hfChosenColor") as HiddenField).Value) ? AppLogic.LookupProductImageByNumberAndColor(int.Parse((e.Item.FindControl("hfProductID") as HiddenField).Value), ThisCustomer.SkinID, (e.Item.FindControl("hfImageFileNameOverride") as HiddenField).Value, (e.Item.FindControl("hfSKU") as HiddenField).Value, ThisCustomer.LocaleSetting, 1, (e.Item.FindControl("hfChosenColor") as HiddenField).Value, "icon") : AppLogic.LookupImage("Product", int.Parse((e.Item.FindControl("hfProductID") as HiddenField).Value), (e.Item.FindControl("hfImageFileNameOverride") as HiddenField).Value, (e.Item.FindControl("hfSKU") as HiddenField).Value, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
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
                if (!string.IsNullOrEmpty((e.Item.FindControl("hfSKU") as HiddenField).Value))
                {
                    (e.Item.FindControl("lblProductSKU") as Label).Text = "SKU: " +
                                                                          (e.Item.FindControl("hfSKU") as HiddenField)
                                                                              .Value;
                }
                if (string.IsNullOrEmpty((e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value) && string.IsNullOrEmpty((e.Item.FindControl("hfBluBucksUsed") as HiddenField).Value))
                {
                    (e.Item.FindControl("lblCategoryFundCreditCaption") as Label).Visible = false;
                    (e.Item.FindControl("lblBluBucksCaption") as Label).Visible = false;
                }
                else if (((e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value.CompareTo("0.0000") != 0) && ((e.Item.FindControl("hfBluBucksUsed") as HiddenField).Value.CompareTo("0.0000") != 0))
                {
                    (e.Item.FindControl("lblCategoryFundCredit") as Label).Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), Convert.ToDecimal((e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value));
                    (e.Item.FindControl("lblBluBuck") as Label).Text = Math.Round(Convert.ToDecimal((e.Item.FindControl("hfBluBucksUsed") as HiddenField).Value), 2).ToString();
                }
                else if (((e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value.CompareTo("0.0000") == 0) && ((e.Item.FindControl("hfBluBucksUsed") as HiddenField).Value.CompareTo("0.0000") == 0))
                {
                    (e.Item.FindControl("lblCategoryFundCreditCaption") as Label).Visible = false;
                    (e.Item.FindControl("lblBluBucksCaption") as Label).Visible = false;
                }
                else if ((e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value.CompareTo("0.0000") == 0)
                {
                    (e.Item.FindControl("lblCategoryFundCreditCaption") as Label).Visible = false;
                    (e.Item.FindControl("lblBluBuck") as Label).Text = Math.Round(Convert.ToDecimal((e.Item.FindControl("hfBluBucksUsed") as HiddenField).Value), 2).ToString();
                }
                else
                {
                    (e.Item.FindControl("lblBluBucksCaption") as Label).Visible = false;
                    (e.Item.FindControl("lblCategoryFundCredit") as Label).Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), Convert.ToDecimal((e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value));
                }
                if (ThisCustomer.CustomerLevelID == 1 || ThisCustomer.CustomerLevelID == 8 || ThisCustomer.CustomerLevelID == 2)
                {
                    (e.Item.FindControl("lblCategoryFundCreditCaption") as Label).Visible = false;
                    (e.Item.FindControl("lblBluBucksCaption") as Label).Visible = false;
                    (e.Item.FindControl("lblCategoryFundCredit") as Label).Visible = false;
                    (e.Item.FindControl("lblBluBuck") as Label).Visible = false;
                    (e.Item.FindControl("lblRegularPriceCaption") as Label).Visible = false;
                    (e.Item.FindControl("lblRegularPrice") as Label).Visible = false;
                    lblCreditsUsedCaption.Visible = false;
                }
                if (ThisCustomer.CustomerLevelID == 3 || ThisCustomer.CustomerLevelID == 7 || ThisCustomer.CustomerLevelID == 9 || ThisCustomer.CustomerLevelID == 10 || ThisCustomer.CustomerLevelID == 11 || ThisCustomer.CustomerLevelID == 12)
                {
                    (e.Item.FindControl("lblBluBucksCaption") as Label).Visible = false;
                    (e.Item.FindControl("lblBluBuck") as Label).Visible = false;
                }
                if (!string.IsNullOrEmpty((e.Item.FindControl("hfBluBucks") as HiddenField).Value))
                {
                    if (Math.Round(Convert.ToDecimal((e.Item.FindControl("hfBluBucks") as HiddenField).Value), 2) != 0)
                    {
                        totalBluBucks = totalBluBucks +
                                        Math.Round(
                                            Convert.ToDecimal((e.Item.FindControl("hfBluBucks") as HiddenField).Value),
                                            2);
                        lblBluBucksTotal.Text = Math.Round(totalBluBucks, 2).ToString();
                        lblBluBucksTotal.Visible = true;
                        lblCreditsUsedCaption.Visible = true;
                        lblBluBucksTotalCaption.Visible = true;
                    }
                }

                if (!string.IsNullOrEmpty((e.Item.FindControl("hfFundName") as HiddenField).Value))
                {
                    (e.Item.FindControl("lblCategoryFundCredit") as Label).Visible = true;
                    (e.Item.FindControl("lblCategoryFundCreditCaption") as Label).Text =
                        (e.Item.FindControl("hfFundName") as HiddenField).Value + " Discount: ";
                }
            }
        }
    }
}