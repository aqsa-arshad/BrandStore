﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Handle the Order Receipt
    /// </summary>
    public partial class OrderReceipt : SkinBase
    {
        /// <summary>
        /// Used for the total of blu bucks used
        /// </summary>
        private decimal totalBluBucks;
        /// <summary>
        /// List for the used funds
        /// </summary>
        List<double> lstFund = Enumerable.Repeat(0.0, 7).ToList();
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
                masterHome = "JeldWenEmptyTemplate";
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
                masterHome = "JeldWenEmptyTemplate";
            }
            return masterHome;
        }

        /// <summary>
        /// Gets the order information.
        /// </summary>
        private void GetOrderInfo()
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
                            lblCustomerID.Text = reader["CustomerID"].ToString();

                            //Billing Address
                            lblBAFullName.Text = reader["BillingFirstName"].ToString() + ' ' +
                                                 reader["BillingLastName"];
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
                                                 reader["ShippingLastName"];
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
                            lblPMExpireDate.Text = "Expires: " + reader["CardExpirationMonth"] + '/' +
                                                   reader["CardExpirationYear"];
                            lblPMCountry.Text = reader["BillingCountry"].ToString();
                            //Billing Amounts
                            lblSubTotal.Text = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C}", Convert.ToDecimal(reader["OrderSubtotal"]));
                            lblTax.Text = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C}", Convert.ToDecimal(reader["OrderTax"]));
                            lblShippingCost.Text = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C}", Convert.ToDecimal(reader["OrderShippingCosts"]));
                            lblTotalAmount.Text = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C}", Convert.ToDecimal(reader["OrderTotal"]));
                            for (var i = 2; i < 7; i++)
                            {
                                if (Convert.ToDecimal(reader[i.ToString()].ToString()) != 0)
                                {
                                    lstFund[i] =
                                        lstFund[i] + Convert.ToDouble(reader[i.ToString()].ToString());

                                    if (lstFund[i] != 0 && i == (int)FundType.SOFFunds)
                                    {
                                        lblSOFFundsTotal.Text = lstFund[i].ToString();
                                        lblSOFFundsTotal.Visible = true;
                                        lblSOFFundsTotalCaption.Visible = true;
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.DirectMailFunds)
                                    {
                                        lblDirectMailFundsTotal.Text = lstFund[i].ToString();
                                        lblDirectMailFundsTotal.Visible = true;
                                        lblDirectMailFundsTotalCaption.Visible = true;
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.DisplayFunds)
                                    {
                                        lblDisplayFundsTotal.Text = lstFund[i].ToString();
                                        lblDisplayFundsTotal.Visible = true;
                                        lblDisplayFundsTotalCaption.Visible = true;
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.LiteratureFunds)
                                    {
                                        lblLiteratureFundsTotal.Text = lstFund[i].ToString();
                                        lblLiteratureFundsTotal.Visible = true;
                                        lblLiteratureFundsTotalCaption.Visible = true;
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.POPFunds)
                                    {
                                        lblPOPFundsTotal.Text = lstFund[i].ToString();
                                        lblPOPFundsTotal.Visible = true;
                                        lblPOPFundsTotalCaption.Visible = true;
                                    }
                                }
                            }
                            if (lstFund.Sum(x => Convert.ToDouble(x)) <= 0)
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
                SysLog.LogMessage(
                    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " +
                    System.Reflection.MethodBase.GetCurrentMethod().Name,
                    ex.Message +
                    ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message))
                        ? " :: " + ex.InnerException.Message
                        : ""),
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
                SysLog.LogMessage(
                    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " +
                    System.Reflection.MethodBase.GetCurrentMethod().Name,
                    ex.Message +
                    ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message))
                        ? " :: " + ex.InnerException.Message
                        : ""),
                    MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptOrderItemsDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs" /> instance containing the event data.</param>
        protected void rptOrderItemsDetail_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                (e.Item.FindControl("lblRegularPrice") as Label).Text = Math.Round(Convert.ToDecimal((e.Item.FindControl("hfRegularPrice") as HiddenField).Value), 2).ToString();
                (e.Item.FindControl("ImgProduct") as Image).ImageUrl = !string.IsNullOrEmpty((e.Item.FindControl("hfChosenColor") as HiddenField).Value) ? AppLogic.LookupProductImageByNumberAndColor(int.Parse((e.Item.FindControl("hfProductID") as HiddenField).Value), ThisCustomer.SkinID, (e.Item.FindControl("hfImageFileNameOverride") as HiddenField).Value, (e.Item.FindControl("hfSKU") as HiddenField).Value, ThisCustomer.LocaleSetting, 1, (e.Item.FindControl("hfChosenColor") as HiddenField).Value, "icon") : AppLogic.LookupImage("Product", int.Parse((e.Item.FindControl("hfProductID") as HiddenField).Value), (e.Item.FindControl("hfImageFileNameOverride") as HiddenField).Value, (e.Item.FindControl("hfSKU") as HiddenField).Value, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                if (!string.IsNullOrEmpty((e.Item.FindControl("hfSKU") as HiddenField).Value))
                {
                    (e.Item.FindControl("lblProductSKU") as Label).Text = "SKU: " +
                                                                          (e.Item.FindControl("hfSKU") as HiddenField)
                                                                              .Value;
                }                
                if (!(string.IsNullOrEmpty((e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value)) && !(string.IsNullOrEmpty((e.Item.FindControl("hfBluBucksUsed") as HiddenField).Value)))
                {
                    (e.Item.FindControl("lblCategoryFundCredit") as Label).Text = Math.Round(Convert.ToDecimal((e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value), 2).ToString();
                    (e.Item.FindControl("lblBluBuck") as Label).Text = Math.Round(Convert.ToDecimal((e.Item.FindControl("hfBluBucksUsed") as HiddenField).Value), 2).ToString();
                }
                else if ((string.IsNullOrEmpty((e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value)) && (string.IsNullOrEmpty((e.Item.FindControl("hfBluBucksUsed") as HiddenField).Value)))
                {
                    (e.Item.FindControl("lblCategoryFundCreditCaption") as Label).Visible = false;
                    (e.Item.FindControl("lblBluBucksCaption") as Label).Visible = false;
                }
                else if (string.IsNullOrEmpty((e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value))
                {
                    (e.Item.FindControl("lblCategoryFundCreditCaption") as Label).Visible = false;
                    (e.Item.FindControl("lblBluBuck") as Label).Text = Math.Round(Convert.ToDecimal((e.Item.FindControl("hfBluBucksUsed") as HiddenField).Value), 2).ToString();
                }
                else
                {
                    (e.Item.FindControl("lblBluBucksCaption") as Label).Visible = false;
                    (e.Item.FindControl("lblCategoryFundCredit") as Label).Text = Math.Round(Convert.ToDecimal((e.Item.FindControl("hfCategoryFundUsed") as HiddenField).Value), 2).ToString();
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
                if (Math.Round(Convert.ToDecimal((e.Item.FindControl("hfBluBucks") as HiddenField).Value), 2) != 0)
                {
                    totalBluBucks = totalBluBucks +
                                    Math.Round(
                                        Convert.ToDecimal((e.Item.FindControl("hfBluBucks") as HiddenField).Value), 2);                    
                    lblBluBucksTotal.Text = totalBluBucks.ToString();
                    lblBluBucksTotal.Visible = true;
                    lblBluBucksTotalCaption.Visible = true;
                }
            }
        }
    }
}