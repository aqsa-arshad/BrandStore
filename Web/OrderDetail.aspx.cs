using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Handle the Order Detail 
    /// </summary>
    public partial class OrderDetail : SkinBase
    {
        private string trackingNumber = string.Empty;
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
                ClientScriptManager cs = Page.ClientScript;
                cs.RegisterClientScriptBlock(this.GetType(), Guid.NewGuid().ToString(), "function ReOrder(OrderNumber) {if(confirm('" + AppLogic.GetString("account.aspx.64", SkinID, ThisCustomer.LocaleSetting) + "')) {top.location.href='reorder.aspx?ordernumber='+OrderNumber;} }", true);
                OrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");
                GetOrderInfo();
                GetOrderItemsDetail();
                SetTrackingInfo();
                hplReOrder.NavigateUrl = "javascript: ReOrder(" + OrderNumber + ");";
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
                            lblOrderNumber.Text = reader["OrderNumber"].ToString();
                            lblOrderDate.Text = Localization.ConvertLocaleDate(reader["OrderDate"].ToString(), Localization.GetDefaultLocale(), ThisCustomer.LocaleSetting);
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

                            SetPageHeading(int.Parse(reader["CustomerID"].ToString()), reader["FirstName"].ToString(), reader["LastName"].ToString());
                            
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
            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + SkinID + "/" + masterHome)))
            {
                masterHome = "JeldWenTemplate";
            }
            return masterHome;
        }

        private void SetPageHeading(int customerID, string firstName, string lastName)
        {
            if (ThisCustomer.CustomerID == customerID)
            {
                ((System.Web.UI.WebControls.Label)Master.FindControl("lblPageHeading")).Text = "ORDER DETAIL";
            }
            else
            {
                ((System.Web.UI.WebControls.Label)Master.FindControl("lblPageHeading")).Text = "ORDER DETAIL FOR " + firstName + " " + lastName;
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptAddresses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptOrderItemsDetail_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                if (!string.IsNullOrEmpty((e.Item.FindControl("hfShippingTrackingNumber") as HiddenField).Value))
                {
                    trackingNumber = (e.Item.FindControl("hfShippingTrackingNumber") as HiddenField).Value;
                }
                (e.Item.FindControl("lblRegularPrice") as Label).Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), (Convert.ToDecimal((e.Item.FindControl("hfRegularPrice") as HiddenField).Value)));
                (e.Item.FindControl("lblCreditPrice") as Label).Text = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), (Convert.ToDecimal((e.Item.FindControl("hfCreditPrice") as HiddenField).Value)));

                if (!string.IsNullOrEmpty((e.Item.FindControl("hfChosenColor") as HiddenField).Value))
                {
                    (e.Item.FindControl("ImgProduct") as Image).ImageUrl = AppLogic.LookupProductImageByNumberAndColor(int.Parse((e.Item.FindControl("hfProductID") as HiddenField).Value), ThisCustomer.SkinID, (e.Item.FindControl("hfImageFileNameOverride") as HiddenField).Value, (e.Item.FindControl("hfSKU") as HiddenField).Value, ThisCustomer.LocaleSetting, 1, (e.Item.FindControl("hfChosenColor") as HiddenField).Value, "icon");
                }
                else
                {
                    (e.Item.FindControl("ImgProduct") as Image).ImageUrl = AppLogic.LookupImage("Product", int.Parse((e.Item.FindControl("hfProductID") as HiddenField).Value), (e.Item.FindControl("hfImageFileNameOverride") as HiddenField).Value, (e.Item.FindControl("hfSKU") as HiddenField).Value, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }

                if ((e.Item.FindControl("hfIsDownload") as HiddenField).Value != "0")
                {
                    (e.Item.FindControl("hlDelivery") as HyperLink).NavigateUrl = (e.Item.FindControl("hfDownloadLocation") as HiddenField).Value;
                    (e.Item.FindControl("hlDelivery") as HyperLink).Text = "Download";
                    (e.Item.FindControl("lblDelivery") as Label).Visible = false;
                }
                if (!string.IsNullOrEmpty((e.Item.FindControl("hfSKU") as HiddenField).Value))
                {
                    (e.Item.FindControl("lblProductSKU") as Label).Text = "SKU: " +
                                                                          (e.Item.FindControl("hfSKU") as HiddenField).Value;
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

        void SetTrackingInfo()
        {
            var lstTrackingInformation = TrackingInformationLogic.GetTrackingInformation(OrderNumber);

            rptTrackingInformation.DataSource = lstTrackingInformation;
            rptTrackingInformation.DataBind();
        }

        protected void rptTrackingInformation_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                (e.Item.FindControl("lblPackageNumber") as Label).Text = (e.Item.ItemIndex + 1).ToString() + ": ";               
                if (!string.IsNullOrEmpty((e.Item.FindControl("hfShippingStatus") as HiddenField).Value) && (string.IsNullOrEmpty((e.Item.FindControl("hfTrackingNumber") as HiddenField).Value) && string.IsNullOrEmpty(trackingNumber)))
                {
                    (e.Item.FindControl("lblShippingStatus") as Label).Text =
                        (e.Item.FindControl("hfShippingStatus") as HiddenField).Value;
                }

                if (!string.IsNullOrEmpty((e.Item.FindControl("hfTrackingNumber") as HiddenField).Value))
                {
                    (e.Item.FindControl("hlTrackItem") as HyperLink).Text =
                        (e.Item.FindControl("hfTrackingNumber") as HiddenField).Value;
                    if (!string.IsNullOrEmpty((e.Item.FindControl("hfTrackingURL") as HiddenField).Value))
                    {
                        (e.Item.FindControl("hlTrackItem") as HyperLink).NavigateUrl =
                            (e.Item.FindControl("hfTrackingURL") as HiddenField).Value;
                    }
                    else
                    {
                        (e.Item.FindControl("hlTrackItem") as HyperLink).Font.Underline = false;
                    }
                }                
            }
        }
    }
}