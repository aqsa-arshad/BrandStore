﻿using System;
using System.Data;
using System.Data.SqlClient;
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
            RequireSecurePage();
            if (ThisCustomer.CustomerLevelID == 4 || ThisCustomer.CustomerLevelID == 5 || ThisCustomer.CustomerLevelID == 6)
            {
                ((Label)Master.FindControl("lblPageHeading")).Text = "ORDER DETAILS FOR " + GetDealerName(ThisCustomer.CustomerID);
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

        /// <summary>
        /// Gets the name of the dealer.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        private static string GetDealerName(int customerId)
        {
            var customerName = string.Empty;
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    var query = "select FirstName + ' ' + LastName as CustomerName from Customer where CustomerID = " +
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

        /// <summary>
        /// Handles the ItemDataBound event of the rptAddresses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptAddresses_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (AppLogic.AppConfig("RTShipping.ActiveCarrier") != null)
            {
                var carrierList = AppLogic.AppConfig("RTShipping.ActiveCarrier").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var listItem in carrierList.Where(listItem => (e.Item.FindControl("hfShippingMethod") as HiddenField).Value.Contains(listItem)))
                {
                    (e.Item.FindControl("hlTrackItem") as HyperLink).NavigateUrl =
                        string.Format(AppLogic.AppConfig("ShippingTrackingURL." + listItem), (e.Item.FindControl("hfShippingTrackingNumber") as HiddenField).Value);
                }
            }
            if ((e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
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
                    (e.Item.FindControl("hlTrackItem") as HyperLink).Visible = false;
                    (e.Item.FindControl("lblDelivery") as Label).Visible = false;
                }
                else
                {
                    if ((e.Item.FindControl("hfShippingMethod") as HiddenField).Value.Contains("|"))
                    {
                        var firstOccuranceOfPipeSign = (e.Item.FindControl("hfShippingMethod") as HiddenField).Value.IndexOf("|", StringComparison.Ordinal);
                        (e.Item.FindControl("lblDelivery") as Label).Text = (e.Item.FindControl("hfShippingMethod") as HiddenField).Value.Substring(0, firstOccuranceOfPipeSign);
                    }
                    else
                    {
                        (e.Item.FindControl("lblDelivery") as Label).Text =
                            (e.Item.FindControl("hfShippingMethod") as HiddenField).Value;
                    }
                }
                if (!string.IsNullOrEmpty((e.Item.FindControl("hfSKU") as HiddenField).Value))
                {
                    (e.Item.FindControl("lblProductSKU") as Label).Text = "SKU: " +
                                                                          (e.Item.FindControl("hfSKU") as HiddenField)
                                                                              .Value;
                }
                if (!string.IsNullOrEmpty((e.Item.FindControl("hfDescription") as HiddenField).Value))
                {
                    if ((e.Item.FindControl("hfDescription") as HiddenField).Value.Length > 60)
                        (e.Item.FindControl("lblDescription") as Label).Text = (e.Item.FindControl("hfDescription") as HiddenField)
                                                                              .Value.Take(60).Aggregate("", (x, y) => x + y) + " ...";
                    else
                    {
                        (e.Item.FindControl("lblDescription") as Label).Text =
                            (e.Item.FindControl("hfDescription") as HiddenField).Value;
                    }
                }
                if (!(string.IsNullOrEmpty((e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value)) && !(string.IsNullOrEmpty((e.Item.FindControl("hfBluBucksUsed") as HiddenField).Value)))
                {
                    (e.Item.FindControl("lblCategoryFundCredit") as Label).Text = (e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value;
                    (e.Item.FindControl("lblBluBuck") as Label).Text = (e.Item.FindControl("hfBluBucksUsed") as HiddenField).Value;
                }
                else if ((string.IsNullOrEmpty((e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value)) && (string.IsNullOrEmpty((e.Item.FindControl("hfBluBucksUsed") as HiddenField).Value)))
                {
                    (e.Item.FindControl("lblCategoryFundCreditCaption") as Label).Visible = false;
                    (e.Item.FindControl("lblBluBucksCaption") as Label).Visible = false;
                }
                else if (string.IsNullOrEmpty((e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value))
                {
                    (e.Item.FindControl("lblCategoryFundCreditCaption") as Label).Visible = false;
                    (e.Item.FindControl("lblBluBuck") as Label).Text = (e.Item.FindControl("hfBluBucksUsed") as HiddenField).Value;
                }
                else
                {
                    (e.Item.FindControl("lblBluBucksCaption") as Label).Visible = false;
                    (e.Item.FindControl("lblCategoryFundCredit") as Label).Text = (e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value;
                }
            }
        }
    }
}