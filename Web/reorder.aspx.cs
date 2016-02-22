// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Globalization;
using System.Text;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for reorder.
    /// </summary>
    public partial class reorder : System.Web.UI.Page
    {
        private Customer ThisCustomer
        {
            get { return Customer.Current; }
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            // currently viewing user must be logged in to view receipts:
            if (!ThisCustomer.IsRegistered)
            {
                Response.Redirect("signin.aspx?returnurl=reorder.aspx?" + Server.UrlEncode(CommonLogic.ServerVariables("QUERY_STRING")));
            }

            this.Title = AppLogic.GetString("reorder.aspx.1", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

            int OrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");

            // are we allowed to view?
            // if currently logged in user is not the one who owns the order, and this is not an admin user who is logged in, reject the reorder:
            if (ThisCustomer.CustomerID != Order.GetOrderCustomerID(OrderNumber) && !ThisCustomer.IsAdminUser)
            {
                Response.Redirect(SE.MakeDriverLink("ordernotfound"));
            }

            StringBuilder output = new StringBuilder();

            if (OrderNumber == 0)
            {
                output.Append("<p>" + String.Format(AppLogic.GetString("reorder.aspx.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), "account.aspx") + "</p>");
            }
            String StatusMsg = String.Empty;
            if (Order.BuildReOrder(null, ThisCustomer, OrderNumber, out StatusMsg))
            {
                CalculateFundsForReOrder();
                Response.Redirect("shoppingcart.aspx");
            }
            else
            {
                output.Append("<p>" + AppLogic.GetString("reorder.aspx.6", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</p>");
                output.Append("<p>Error: " + StatusMsg + "</p>");
                output.Append("<p>" + String.Format(AppLogic.GetString("reorder.aspx.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), "account.aspx", AppLogic.GetString("AppConfig.CartPrompt", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)) + "</p>");
            }

            litOutput.Text = output.ToString();
        }

        private void CalculateFundsForReOrder()
        {
            ShoppingCart cart = new ShoppingCart(ThisCustomer.SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            System.Collections.Generic.List<CustomerFund> CustomerFunds = AuthenticationSSO.GetCustomerFund(ThisCustomer.CustomerID, true);

            Decimal BluBucksPercentage = AuthenticationSSO.GetBudgetPercentageRatio(ThisCustomer.CustomerLevelID, Convert.ToInt32(FundType.BLUBucks)).BudgetPercentageValue;
            CustomerFund BluBucksFund = CustomerFunds.Find(x => x.FundID == Convert.ToInt32(FundType.BLUBucks));
            Decimal BluBucksAvailable = 0;
            if (BluBucksFund != null)
            {
                 BluBucksAvailable = BluBucksFund.AmountAvailable;
            }

            foreach (CartItem cItem in cart.CartItems.ToArrayList())
            {
                String RecordID = cItem.ShoppingCartRecordID.ToString();
                int FundID = GetProductFundID(cItem.ProductID);//Get latest Fund ID of product , dont use fund id already assigned it may change
                Decimal Productprice = cItem.Price;
                int Quantity = cItem.Quantity;
                Decimal TotalPrice = Convert.ToDecimal(Productprice * Quantity);

                

                //Apply Product Category Fund
                CustomerFund CategoryFund = CustomerFunds.Find(x => x.FundID == FundID);
                if (CategoryFund != null)
                {
                    Decimal CategoryFundAmountAvailable = CategoryFund.AmountAvailable;
                    if (CategoryFundAmountAvailable < TotalPrice)
                    {
                        TotalPrice = TotalPrice - CategoryFundAmountAvailable;
                        cItem.CategoryFundUsed = CategoryFundAmountAvailable;

                    }
                    else
                    {
                        CategoryFundAmountAvailable = CategoryFundAmountAvailable - TotalPrice;
                        cItem.CategoryFundUsed = TotalPrice;
                        TotalPrice = 0;
                       
                    }
                    CustomerFunds.Find(x => x.FundID == FundID).AmountUsed =   CustomerFunds.Find(x => x.FundID == FundID).AmountUsed + cItem.CategoryFundUsed;
                }
                else
                {
                    cItem.CategoryFundUsed = 0;
                    cItem.FundID = 0;

                }
                //End Apply Product Category Fund

                //Apply BluBucks to this item based on available bucks and percentage ratio
                //CustomerFund BluBucksFund = CustomerFunds.Find(x => x.FundID == Convert.ToInt32(FundType.BLUBucks));
                cItem.BluBucksPercentageUsed = BluBucksPercentage;
                if (BluBucksAvailable >0 )
                {                   
                    Decimal amountTopaidbyBluBucks = Math.Round((TotalPrice * (BluBucksPercentage / 100)), 2);

                    if (BluBucksAvailable < amountTopaidbyBluBucks)
                    {

                        cItem.BluBuksUsed = BluBucksAvailable;
                        BluBucksAvailable = 0;
                    }
                    else
                    {
                        cItem.BluBuksUsed = amountTopaidbyBluBucks;
                        BluBucksAvailable = BluBucksAvailable - amountTopaidbyBluBucks;
                    }

                }
                else
                {
                    cItem.BluBuksUsed = 0;

                }
                //End Apply BluBucks

                cart.SetItemFundsUsed(cItem.ShoppingCartRecordID, cItem.CategoryFundUsed, cItem.BluBuksUsed, cItem.GLcode, BluBucksPercentage);
            }
        }

        private int GetProductFundID(int ProductID)
        {
            int FundID = 0;
            if (ThisCustomer.CustomerLevelID == 3 || ThisCustomer.CustomerLevelID == 7)
                FundID = (int)FundType.SOFFunds;
            else
            {
                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS(string.Format("select FundID from Product a with (NOLOCK) inner join (select a.ProductID, b.StoreID from Product a with (nolock) left join ProductStore b " +
                                              "with (NOLOCK) on a.ProductID = b.ProductID) b on a.ProductID = b.ProductID where Deleted=0 and a.ProductID={0} and ({1}=0 or StoreID={2})", +
                                              ProductID, CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowProductFiltering") == true, 1, 0), AppLogic.StoreID()), dbconn))
                    {
                        if (rs.Read())
                            FundID = DB.RSFieldInt(rs, "FundID");
                    }
                }
            }
            return FundID;


        }

    }
}
