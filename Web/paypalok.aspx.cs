// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for paypalok.
    /// </summary>
    public partial class paypalok : System.Web.UI.Page
    {
        string payer_business_name = String.Empty;
        string payer_email = String.Empty;
        string payer_id = String.Empty;
        string payer_status = String.Empty;
        string residence_country = String.Empty;
        string payment_date = String.Empty;
        string payment_status = String.Empty;
        string payment_gross = String.Empty;
        string auth_id = String.Empty;
        string first_name = String.Empty;
        string last_name = String.Empty;
        string txn_id = String.Empty;
        string receipt_id = String.Empty;
        string test_ipn = String.Empty;
        string custom = String.Empty;
        string invoice = String.Empty;
        string gift_email = String.Empty;
        string gift_message = String.Empty;
        string num_cart_items = String.Empty;
        string address_street = String.Empty;
        string address_name = String.Empty;
        string address_city = String.Empty;
        string address_state = String.Empty;
        string address_zip = String.Empty;
        string address_country = String.Empty;
        string address_status = String.Empty;
        string business = String.Empty;
        string receiver_id = String.Empty;
        string receiver_email = String.Empty;
        string memo = String.Empty;
        string pending_reason = String.Empty;
        string subscriptionID = String.Empty; // PayPal Standard

        protected void Page_Load(object sender, System.EventArgs e)
        {
            for (int i = 0; i < Request.Form.Count; i++)
            {
                string fValue = Server.UrlDecode(Request.Form[i]);

                switch (Request.Form.GetKey(i).ToLowerInvariant())
                {
                    // Customer Variables 
                    case "payer_business_name": payer_business_name = fValue; break;
                    case "residence_country": residence_country = fValue; break;
                    case "business": business = fValue; break;
                    case "receiver_id": receiver_id = fValue; break;
                    case "receiver_email": receiver_email = fValue; break;
                    case "payer_email": payer_email = fValue; break;
                    case "payer_id": payer_id = fValue; break;
                    case "payer_status": payer_status = fValue; break;
                    case "payment_date": payment_date = fValue; break;
                    case "payment_status": payment_status = fValue; break;
                    case "payment_gross": payment_gross = fValue; break;
                    case "auth_id": auth_id = fValue; break;
                    case "first_name": first_name = fValue; break;
                    case "last_name": last_name = fValue; break;
                    case "num_cart_items": num_cart_items = fValue; break;
                    case "txn_id": txn_id = fValue; break;
                    case "receipt_id": receipt_id = fValue; break;
                    case "test_ipn": test_ipn = fValue; break;
                    case "custom": custom = fValue; break;
                    case "invoice": invoice = fValue; break;
                    case "memo": memo = fValue; break;
                    case "address_name": address_name = fValue; break;
                    case "address_street": address_street = fValue; break;
                    case "address_city": address_city = fValue; break;
                    case "address_state": address_state = fValue; break;
                    case "address_zip": address_zip = fValue; break;
                    case "address_country": address_country = fValue; break;
                    case "address_status": address_status = fValue; break;
                    case "pending_reason": pending_reason = fValue; break;
                    case "subscr_id": subscriptionID = fValue; break; 
                }
            }

            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            String PM = AppLogic.CleanPaymentMethod(AppLogic.ro_PMPayPal);
            AppLogic.ValidatePM(PM); // this WILL throw a hard security exception on any problem!

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            ThisCustomer.RequireCustomerRecord();

            int CustomerID = 0;
            try
            {
                CustomerID = int.Parse(custom);
            }
            catch { }

            if (ThisCustomer.CustomerID != CustomerID)
            {
                ThisCustomer = new Customer(CustomerID, true);
            }

            int OrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");

            if (OrderNumber == 0)
            {
                OrderNumber = DB.GetSqlN("select max(ordernumber) N from orders where paymentmethod = '" + AppLogic.ro_PMPayPal + "' AND charindex(" + DB.SQuote(txn_id) + ",AuthorizationPNREF) > 0");
            }

            if (AppLogic.AppConfigBool("PayPal.UseInstantNotification") && OrderNumber == 0)
            { // try one more time after a pause to see if the IPN goes through
                Thread.Sleep(5000);
                OrderNumber = DB.GetSqlN("select max(ordernumber) N from orders where paymentmethod = '" + AppLogic.ro_PMPayPal + "' AND charindex(" + DB.SQuote(txn_id) + ",AuthorizationPNREF) > 0");
            }


            // The order could already exist if:
            // 1) AppConfigBool("PayPal.UseInstantNotification") = true
            // AND
            // 2) The Instant Payment Notification for this order already occurred
            if (OrderNumber == 0 || !Order.OrderExists(OrderNumber))
            {

                ShoppingCart cart = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);

                // Cart will be empty if order already processed by paypalnotification.aspx (Instant Payment Notification)
                if (!cart.IsEmpty())
                {
                    Address UseBillingAddress = new Address();
                    UseBillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                    UseBillingAddress.ClearCCInfo();
                    if (UseBillingAddress.PaymentMethodLastUsed != AppLogic.ro_PMPayPal && UseBillingAddress.PaymentMethodLastUsed != AppLogic.ro_PMPayPalEmbeddedCheckout)
                    {
                        try
                        {
                            AppLogic.ValidatePM(AppLogic.ro_PMPayPal);
                            UseBillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMPayPal;
                        }
                        catch (Exception)
                        {
                            AppLogic.ValidatePM(AppLogic.ro_PMPayPalEmbeddedCheckout);
                            UseBillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMPayPalEmbeddedCheckout;
                        }
                    }
                    UseBillingAddress.UpdateDB();

                    if (AppLogic.AppConfigBool("PayPal.RequireConfirmedAddress"))
                    {
                        Address ShippingAddress = new Address();

                        String[] StreetArray = address_street.Split(new string[1] { "\r\n" }, 2, StringSplitOptions.RemoveEmptyEntries);
                        String Address1 = String.Empty;
                        String Address2 = String.Empty;
                        if (StreetArray.Length > 1)
                        {
                            Address1 = StreetArray[0];
                            Address2 = StreetArray[1];
                        }
                        else
                        {
                            Address1 = address_street;
                        }
                        String[] NameArray = address_name.Split(new string[1] { " " }, 2, StringSplitOptions.RemoveEmptyEntries);
                        String FirstName = String.Empty;
                        String LastName = String.Empty;
                        if (NameArray.Length > 1)
                        {
                            FirstName = NameArray[0];
                            LastName = NameArray[1];
                        }
                        else
                        {
                            LastName = address_name;
                        }
                        string sql = String.Format("select top 1 AddressID as N from Address where Address1={0} and Address2={1} and City={2} and State={3} and Zip={4} and Country={5} and FirstName={6} and LastName={7} and CustomerID={8}",
                            DB.SQuote(Address1), DB.SQuote(Address2), DB.SQuote(address_city), DB.SQuote(address_state),
                            DB.SQuote(address_zip), DB.SQuote(address_country), DB.SQuote(FirstName), DB.SQuote(LastName), CustomerID);
                        int ExistingAddressID = DB.GetSqlN(sql);

                        if (ExistingAddressID == 0 || ThisCustomer.PrimaryShippingAddressID != ExistingAddressID)
                        {
                            string note = "Note: Customer selected Ship-To address at PayPal.com";
                            string ordernote = DB.GetSqlS("select OrderNotes S from Customer where CustomerID=" + ThisCustomer.CustomerID.ToString());
                            if (!ordernote.Contains(note))
                            {
                                ordernote += System.Environment.NewLine + note;
                                DB.ExecuteSQL("update Customer set OrderNotes=" + DB.SQuote(ordernote) + " where CustomerID=" + ThisCustomer.CustomerID.ToString());
                            }
                        }

                        if (ExistingAddressID == 0)
                        { // Does not exist
                            ShippingAddress.CustomerID = CustomerID;
                            ShippingAddress.FirstName = FirstName;
                            ShippingAddress.LastName = LastName;
                            ShippingAddress.Address1 = Address1;
                            ShippingAddress.Address2 = Address2;
                            ShippingAddress.City = address_city;
                            ShippingAddress.State = address_state;
                            ShippingAddress.Zip = address_zip;
                            ShippingAddress.Country = address_country;
                            ShippingAddress.EMail = payer_email;
                            ShippingAddress.InsertDB();

                            ShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
                        }
                        else
                        { // Exists already
                            ShippingAddress.LoadFromDB(ExistingAddressID);
                            ShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
                        }
                    }

                    // Reload customer and cart so that we have the addresses right
                    ThisCustomer = new Customer(CustomerID, true);
                    cart = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                    if (!cart.IsEmpty())
                    {
                        if (OrderNumber == 0)
                        {
                            OrderNumber = AppLogic.GetNextOrderNumber();
                        }

                        decimal CartTotal = cart.Total(true);
                        decimal NetTotal = CartTotal - CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotal < cart.Coupon.DiscountAmount, CartTotal, cart.Coupon.DiscountAmount), 0);
                        NetTotal = Localization.ParseNativeDecimal(Localization.CurrencyStringForGatewayWithoutExchangeRate(NetTotal));
                        decimal PaymentTotal = CommonLogic.FormNativeDecimal("mc_gross");

                        if(PaymentTotal == 0)
                            PaymentTotal = CommonLogic.FormNativeDecimal("mc_amount3");

                        try
                        {
                            //Process as AuthOnly first
                            String status = Gateway.MakeOrder(String.Empty, AppLogic.ro_TXModeAuthOnly, cart, OrderNumber, String.Empty, String.Empty, txn_id, string.Empty);

                            if (status == AppLogic.ro_OK)
                            { // Now, if paid for, process as Captured
                                String TransactionState = AspDotNetStorefrontGateways.Processors.PayPalController.GetTransactionState(payment_status, pending_reason);
                                if (TransactionState == AppLogic.ro_TXStateCaptured)
                                {
                                    Gateway.ProcessOrderAsCaptured(OrderNumber);
                                    DB.ExecuteSQL("update orders set AuthorizationPNREF=AuthorizationPNREF+" + DB.SQuote("|CAPTURE=" + txn_id)  + " where OrderNumber=" + OrderNumber.ToString());
                                }
                                else if (TransactionState == AppLogic.ro_TXStatePending)
                                {
                                    DB.ExecuteSQL("update orders set TransactionState=" + DB.SQuote(AppLogic.ro_TXStatePending) + " where OrderNumber=" + OrderNumber.ToString());
                                }

                                if (subscriptionID.Length > 0)
                                {
                                    String sql = "update orders set RecurringSubscriptionID = @SubscriptionID where OrderNumber = @OrderNumber";
                                    SqlParameter[] orderNumberParams = { new SqlParameter("@SubscriptionID", SqlDbType.NVarChar, 100) { Value = subscriptionID }, new SqlParameter("@OrderNumber", SqlDbType.Int) { Value = OrderNumber } };
                                    DB.ExecuteSQL(sql, orderNumberParams);

                                    OrderTransactionCollection ecRecurringOrderTransaction = new OrderTransactionCollection(OrderNumber);
                                    ecRecurringOrderTransaction.AddTransaction("PayPal Standard Checkout Subscription Profile Creation",
                                        string.Empty,
                                        string.Empty,
                                        string.Empty,
                                        subscriptionID,
                                        AppLogic.ro_PMPayPal,
                                        null,
                                        NetTotal);
                                }
                            }

                            // The incoming payment should match the cart total, if they don't
                            // the customer may have tampered with the cart to cheat, so flag as fraud
                            // but keep new so the admin will have to review the order.
                            if (Math.Abs(NetTotal - PaymentTotal) > 0.05M) // allow 0.05 descrepency to allow minor rounding errors
                            {
                                Order.MarkOrderAsFraud(OrderNumber, true);
                                DB.ExecuteSQL("update orders set FraudedOn=getdate(), IsNew=1 where OrderNumber=" + OrderNumber.ToString());
                            }
                        }
                        catch // if we failed, did the IPN come back at the same time?
                        {
                            cart = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                            if (cart.IsEmpty())
                            {
                                OrderNumber = DB.GetSqlN("select MAX(OrderNumber) N from dbo.orders where CustomerID = " + CustomerID.ToString());
                            }
                        }

                    }
                    else
                    {
                        OrderNumber = DB.GetSqlN("select MAX(OrderNumber) N from dbo.orders where CustomerID = " + CustomerID.ToString());
                    }
                }
                else
                {
                    OrderNumber = DB.GetSqlN("select MAX(OrderNumber) N from dbo.orders where CustomerID = " + CustomerID.ToString());
                }
            }

            Response.Redirect("orderconfirmation.aspx?ordernumber=" + OrderNumber.ToString() + "&paymentmethod=PayPal");
        }

    }
}
