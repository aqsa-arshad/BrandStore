// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for paypalnotification.
    /// </summary>
    public partial class paypalnotification : SkinBase
    {
        string err_msg = String.Empty;
        string payment_status = String.Empty;
        string txn_id = String.Empty;
        string custom = String.Empty;
        string invoice = String.Empty;
        string pending_reason = String.Empty;
        string address_street = String.Empty;
        string address_name = String.Empty;
        string address_city = String.Empty;
        string address_state = String.Empty;
        string address_zip = String.Empty;
        string address_country = String.Empty;
        string payer_email = String.Empty;
        string parent_txn_id = String.Empty;
        string txn_type = String.Empty;
        string payerID = String.Empty;
        string profileID = String.Empty;
        string subscriptionID = String.Empty; // PayPal Standard

        private void Page_Load(object sender, System.EventArgs e)
        {
            //String postData = String.IsNullOrEmpty(Request.Form.ToString()) ? Request.QueryString.ToString() : Request.Form.ToString();
            //SysLog.LogMessage("Paypal notification posted to.", postData, MessageTypeEnum.Informational, MessageSeverityEnum.Alert);

            for (int i = 0; i < Request.Form.Count; i++)
            {
                string fValue = Server.UrlDecode(Request.Form[i]);

                switch (Request.Form.GetKey(i).ToLowerInvariant())
                {
                    // Customer Variables 
                    case "payment_status": payment_status = fValue; break;
                    case "txn_id": txn_id = fValue; break;
                    case "custom": custom = fValue; break;
                    case "invoice": invoice = fValue; break;
                    case "pending_reason": pending_reason = fValue; break;
                    case "address_name": address_name = fValue; break;
                    case "address_street": address_street = fValue; break;
                    case "address_city": address_city = fValue; break;
                    case "address_state": address_state = fValue; break;
                    case "address_zip": address_zip = fValue; break;
                    case "address_country": address_country = fValue; break;
                    case "payer_email": payer_email = fValue; break;
                    case "parent_txn_id": parent_txn_id = fValue; break;
                    case "txn_type": txn_type = fValue; break;
                    case "payer_id": payerID = fValue; break;
                    case "recurring_payment_id": profileID = fValue; break;
                    case "subscr_id": subscriptionID = fValue; break;
                }
            }

            // PayPal Express Checkout recurring notification
            if (txn_type.ToLowerInvariant().Contains("recurring") || txn_type.ToLowerInvariant().Contains("subscr_cancel") && PostIsValid())
            {
                HandlePayPalExpressCheckoutRecurringNotification();
            }

            // Non-recurring notification
            int CustomerID = Localization.ParseNativeInt(custom);
            if (CustomerID > 0 && PostIsValid())
            {
                String status = AppLogic.ro_OK;

                String TransactionState = AspDotNetStorefrontGateways.Processors.PayPalController.GetTransactionState(payment_status, pending_reason);

                int ExistingOrderNumber = Localization.ParseNativeInt(invoice);

                if (ExistingOrderNumber > 0 && !Order.OrderExists(ExistingOrderNumber))
                { // It only is existing if it exists.
                    ExistingOrderNumber = 0;
                }

                if (ExistingOrderNumber == 0)
                {
                    if (!String.IsNullOrEmpty(parent_txn_id))
                    {
                        ExistingOrderNumber = DB.GetSqlN("select min(ordernumber) N from orders where (paymentmethod = '" + AppLogic.ro_PMPayPal + "' OR paymentmethod = '" + AppLogic.ro_PMPayPalEmbeddedCheckout + "') AND charindex(" + DB.SQuote(parent_txn_id) + ",AuthorizationPNREF) > 0");
                    }
                    else
                    {
                        ExistingOrderNumber = DB.GetSqlN("select min(ordernumber) N from orders where (paymentmethod = '" + AppLogic.ro_PMPayPal + "' OR paymentmethod = '" + AppLogic.ro_PMPayPalEmbeddedCheckout + "') AND charindex(" + DB.SQuote(txn_id) + ",AuthorizationPNREF) > 0");
                    }
                }

                if (ExistingOrderNumber == 0) //last try - look up by paypal payments advanced checkout transaction
                {
                    if (!String.IsNullOrEmpty(parent_txn_id))
                        ExistingOrderNumber = OrderTransaction.LookupOrderNumber(null, null, null, null, parent_txn_id, null, null);
                    else if (!String.IsNullOrEmpty(txn_id))
                        ExistingOrderNumber = OrderTransaction.LookupOrderNumber(null, null, null, null, txn_id, null, null);
                }

                // Order won't exist yet if they never followed the link from paypal back to the store.
                if (ExistingOrderNumber == 0)
                {
                    if (TransactionState == AppLogic.ro_TXStateAuthorized
                        || TransactionState == AppLogic.ro_TXStatePending
                        || TransactionState == AppLogic.ro_TXStateCaptured)
                    {

                        Customer ThisCustomer = new Customer(CustomerID, true);
                        ShoppingCart cart = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);



                        // Cart will be empty if order already processed by paypalok.aspx
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
                            decimal CartTotal = cart.Total(true);
                            decimal NetTotal = CartTotal - CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotal < cart.Coupon.DiscountAmount, CartTotal, cart.Coupon.DiscountAmount), 0);
                            NetTotal = Localization.ParseNativeDecimal(Localization.CurrencyStringForGatewayWithoutExchangeRate(NetTotal));
                            decimal PaymentTotal = CommonLogic.FormNativeDecimal("mc_gross");

                            // Cart will be empty if order already processed by paypalok.aspx
                            if (!cart.IsEmpty() && NetTotal > 0.0M)
                            {
                                //Process as AuthOnly first
                                int OrderNumber = AppLogic.GetNextOrderNumber();
                                status = Gateway.MakeOrder(String.Empty, AppLogic.ro_TXModeAuthOnly, cart, OrderNumber, String.Empty, String.Empty, txn_id, String.Empty);

                                if (status == AppLogic.ro_OK)
                                {
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

                                    if (TransactionState == AppLogic.ro_TXStateCaptured)
                                    { // Now, if paid for, process as Captured
                                        Gateway.ProcessOrderAsCaptured(OrderNumber);
                                        DB.ExecuteSQL("update orders set AuthorizationPNREF=AuthorizationPNREF+" + DB.SQuote("|CAPTURE=" + txn_id) + " where OrderNumber=" + OrderNumber.ToString());
                                    }
                                    else if (TransactionState == AppLogic.ro_TXStatePending)
                                    {
                                        DB.ExecuteSQL("update orders set TransactionState=" + DB.SQuote(AppLogic.ro_TXStatePending) + " where OrderNumber=" + OrderNumber.ToString());
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
                                else
                                {
                                    // Finalize the order here since they may never click through to orderconfirmation.aspx
                                    Order ord = new Order(OrderNumber, ThisCustomer.LocaleSetting);
                                    String PM = AppLogic.CleanPaymentMethod(ord.PaymentMethod);
                                    if (!ord.AlreadyConfirmed)
                                    {
                                        DB.ExecuteSQL("update Customer set OrderOptions=NULL, OrderNotes=NULL, FinalizationData=NULL where CustomerID=" + CustomerID.ToString());

                                        if (ord.TransactionIsCaptured() && ord.HasGiftRegistryComponents())
                                        {
                                            ord.FinalizeGiftRegistryComponents();
                                        }
                                        AppLogic.SendOrderEMail(ThisCustomer, OrderNumber, false, PM, true, null, null);
                                        DB.ExecuteSQL("Update Orders set AlreadyConfirmed=1 where OrderNumber=" + OrderNumber.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
                else  // we have an existing order
                {
                    if (TransactionState == AppLogic.ro_TXStateVoided)
                    {
                        IPNVoid(ExistingOrderNumber);
                    }
                    else if (TransactionState == AppLogic.ro_TXStateCaptured)
                    {
                        IPNCapture(ExistingOrderNumber, txn_id, CommonLogic.FormNativeDecimal("mc_gross"));
                    }
                    else if (TransactionState == AppLogic.ro_TXStateRefunded)
                    {
                        IPNRefund(ExistingOrderNumber, txn_id, CommonLogic.FormNativeDecimal("mc_gross"));
                    }
                    else if (TransactionState == AppLogic.ro_TXStatePending)
                    { // eChecks could have had the order placed in Captured state with Express Checkout
                        DB.ExecuteSQL("update orders set CapturedOn=NULL, TransactionState=" + DB.SQuote(AppLogic.ro_TXStatePending) + " where OrderNumber=" + ExistingOrderNumber.ToString());
                    }

                    OrderTransactionCollection transactions = new OrderTransactionCollection(ExistingOrderNumber);
                    transactions.AddTransaction(TransactionState, null, null, null, txn_id, AppLogic.ro_PMPayPal + " IPN", null, CommonLogic.FormNativeDecimal("mc_gross"));
                }
            }
        }

        public Boolean PostIsValid()
        {
            //Validate the post by querying PayPal
            byte[] param = Request.BinaryRead(Request.ContentLength);
            string formStr = Encoding.ASCII.GetString(param);
            formStr += "&cmd=_notify-validate";

            string verify_url = String.Empty;

            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                verify_url = AppLogic.AppConfig("PayPal.LiveServer");
            }
            else
            {
                verify_url = AppLogic.AppConfig("PayPal.TestServer");
            }

            byte[] data = Encoding.ASCII.GetBytes(formStr);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(verify_url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = data.Length;

            Stream reqStream = webRequest.GetRequestStream();
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();

            WebResponse webResponse;
            string rawResponse = String.Empty;
            try
            {
                webResponse = webRequest.GetResponse();
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                rawResponse = sr.ReadToEnd();
                sr.Close();
                webResponse.Close();
            }
            catch (Exception exc)
            {
                Response.Write("Error connecting with gateway. Please try again later.");
                err_msg += exc.Message + "\n\n";
            }

            if (rawResponse.Equals("VERIFIED", StringComparison.InvariantCultureIgnoreCase))
                return true;
            else
                return false;
        }

        public void HandlePayPalExpressCheckoutRecurringNotification()
        {
            switch (txn_type.ToLowerInvariant())
            {
                case "recurring_payment":
                    CreateNewPPECRecurrence();
                    break;
                case "recurring_payment_expired":
                case "subscr_cancel":
                case "recurring_payment_profile_cancel":
                    CancelPPECRecurringSubscription();
                    break;
                default:
                    break;
            }
        }

        public void CreateNewPPECRecurrence()
        {
            int originalOrderNumber = GetPPECOriginalOrderNumber();

            if (originalOrderNumber != 0)
            {
                RecurringOrderMgr manager = new RecurringOrderMgr(base.EntityHelpers, base.GetParser);
                manager.ProcessPPECRecurringOrder(originalOrderNumber);
            }
            else
            {
                SysLog.LogMessage("A recurring payment notification came from PayPal Express that did not match an existing recurring order.",
                    "PayerID = " + payerID + ", ProfileID = " + profileID,
                    MessageTypeEnum.Informational,
                    MessageSeverityEnum.Alert);
            }
        }

        public void CancelPPECRecurringSubscription()
        {
            int originalOrderNumber = GetPPECOriginalOrderNumber();

            if (originalOrderNumber != 0)
            {
                // Cancelling through the API triggers a notification to this page.  Make sure we don't try to cancel the same order repeatedly.
                if (PPECRecurringOrderIsStillActive(originalOrderNumber))
                {
                    RecurringOrderMgr manager = new RecurringOrderMgr(base.EntityHelpers, base.GetParser);
                    manager.CancelPPECRecurringOrder(originalOrderNumber, true);
                }
            }
            else
            {
                SysLog.LogMessage("A recurring payment cancellation notification came from PayPal Express that did not match an existing recurring order.",
                    "PayerID = " + payerID + ", ProfileID = " + profileID,
                    MessageTypeEnum.Informational,
                    MessageSeverityEnum.Alert);
            }
        }

        public int GetPPECOriginalOrderNumber()
        {
            int originalOrderNumber = 0;

            if (profileID.Length == 0)
                profileID = subscriptionID;

            String orderNumberSql = "SELECT OrderNumber AS N FROM OrderTransaction WHERE Code=@ProfileID";
            SqlParameter[] orderNumberParams = { new SqlParameter("@ProfileID", profileID) };

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                originalOrderNumber = DB.GetSqlN(orderNumberSql, orderNumberParams);
            }

            return originalOrderNumber;
        }

        public Boolean PPECRecurringOrderIsStillActive(int OriginalOrderNumber)
        {
            Order orderToCheck = new Order(OriginalOrderNumber);

            Customer customerToCheck = new Customer(orderToCheck.CustomerID);

            ShoppingCart cartToCheck = new ShoppingCart(customerToCheck.SkinID, customerToCheck, CartTypeEnum.RecurringCart, OriginalOrderNumber, false);

            if (cartToCheck.CartItems.Count > 0)
                return true;
            else
                return false;
        }

        public void IPNCapture(int OrderNumber, String CaptureTransID, Decimal OrderTotal)
        {
            String result = "";

            int ONX = OrderNumber;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("Select * from orders   with (NOLOCK)  where ordernumber=" + ONX.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        String PM = AppLogic.CleanPaymentMethod(DB.RSField(rs, "PaymentMethod"));
                        if (DB.RSFieldDateTime(rs, "CapturedOn") == System.DateTime.MinValue)
                        {
                            if (DB.RSField(rs, "TransactionState") == AppLogic.ro_TXStateAuthorized
                                || DB.RSField(rs, "TransactionState") == AppLogic.ro_TXStatePending)
                            {
                                DB.ExecuteSQL(String.Format("update orders set OrderTotal={0} where OrderNumber={1}", Localization.CurrencyStringForDBWithoutExchangeRate(OrderTotal), ONX.ToString()));
                                Order o = new Order(ONX, Localization.GetDefaultLocale());
                                o.CaptureTXCommand = "Instant Payment Notification";
                                o.CaptureTXResult = AppLogic.ro_OK;
                                o.AuthorizationPNREF = o.AuthorizationPNREF + "|CAPTURE=" + CaptureTransID;
                                result = AppLogic.ro_OK;
                            }
                        }
                    }
                }
            }

            if (result == AppLogic.ro_OK)
            {
                Gateway.ProcessOrderAsCaptured(ONX);
            }
        }

        public void IPNRefund(int OrderNumber, String RefundTransID, Decimal RefundAmount)
        {
            String result = "";

            if (RefundAmount < 0)
            {
                RefundAmount = (decimal)(-1.0) * RefundAmount;
            }

            int ONX = OrderNumber;

            Order ord = new Order(ONX, Localization.GetDefaultLocale());
            Customer c = new Customer(ord.CustomerID);

            decimal OrderTotal = 0.0M;
            int CouponType = 0;
            string CouponCode = "";
            decimal CouponDiscountAmount = 0.0M;

            String RefundReason = "PayPal IPN Refund";

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("Select * from Orders  with (NOLOCK)  where OrderNumber=" + ONX.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        string PM = AppLogic.CleanPaymentMethod(DB.RSField(rs, "PaymentMethod"));
                        if (DB.RSFieldDateTime(rs, "RefundedOn") == System.DateTime.MinValue)
                        {
                            if (DB.RSFieldDateTime(rs, "CapturedOn") != System.DateTime.MinValue)
                            {
                                if (DB.RSField(rs, "TransactionState") == AppLogic.ro_TXStateCaptured)
                                {

                                    OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                                    CouponType = DB.RSFieldInt(rs, "CouponType");
                                    CouponCode = DB.RSField(rs, "CouponCode");
                                    CouponDiscountAmount = DB.RSFieldDecimal(rs, "CouponDiscountAmount");

                                    DB.ExecuteSQL("update orders set RefundTXCommand='Instant Payment Notification', RefundTXResult=" + DB.SQuote(AppLogic.ro_OK) + ", AuthorizationPNREF=AuthorizationPNREF+" + DB.SQuote("|REFUND=" + RefundTransID) + " where OrderNumber=" + ONX.ToString());

                                    result = AppLogic.ro_OK;

                                }
                            }
                        }
                    }
                }
            }

            if (result == AppLogic.ro_OK)
            {
                // was this a full refund
                // we can only properly handle IPN's for refunds of the full order amount
                if (RefundAmount == System.Decimal.Zero || RefundAmount == OrderTotal)
                {
                    // make sure inventory was restored. safe to call repeatedly. proc protects against deducting twice
                    DB.ExecuteSQL("aspdnsf_AdjustInventory " + ONX.ToString() + ",1");

                    Gateway.DecrementMicropayProductsInOrder(ONX);

                    // update transactionstate
                    DB.ExecuteSQL("update Orders set RefundReason=" + DB.SQuote(RefundReason) + ", TransactionState=" + DB.SQuote(AppLogic.ro_TXStateRefunded) + ", RefundedOn=getdate(), IsNew=0 where OrderNumber=" + ONX.ToString());

                    //Invalidate GiftCards ordered on this order
                    GiftCards GCs = new GiftCards(ONX, GiftCardCollectionFilterType.OrderNumber);
                    foreach (GiftCard gc in GCs)
                    {
                        gc.GiftCardTransactions.Add(GiftCardUsageTransaction.CreateTransaction(gc.GiftCardID, GiftCardUsageReasons.FundsRemovedByAdmin, 0, 0, gc.Balance, ""));
                        gc.UpdateCard(null, null, null, null, 1, null, null, null, null, null, null, null, null, null, null);
                    }

                    //Restore Amount to coupon used in paying for the order
                    if ((CouponTypeEnum)CouponType == CouponTypeEnum.GiftCard)
                    {
                        GiftCard gc = new GiftCard(CouponCode);
                        if (gc.GiftCardID != 0)
                        {
                            gc.GiftCardTransactions.Add(GiftCardUsageTransaction.CreateTransaction(gc.GiftCardID, GiftCardUsageReasons.FundsAddedByAdmin, 0, 0, CouponDiscountAmount, ""));
                        }
                    }
                }
            }
        }

        public void IPNVoid(int OrderNumber)
        {
            String result = "";

            int ONX = OrderNumber;

            Order ord = new Order(ONX, Localization.GetDefaultLocale());
            Customer c = new Customer(ord.CustomerID);

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("Select * from Orders  with (NOLOCK)  where OrderNumber=" + ONX.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        if (DB.RSFieldDateTime(rs, "VoidedOn") == System.DateTime.MinValue)
                        {
                            decimal OrderTotal = 0.0M;
                            int CouponType = 0;
                            string CouponCode = "";
                            decimal CouponDiscountAmount = 0.0M;

                            OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                            CouponType = DB.RSFieldInt(rs, "CouponType");
                            CouponCode = DB.RSField(rs, "CouponCode");
                            CouponDiscountAmount = DB.RSFieldDecimal(rs, "CouponDiscountAmount");

                            // make sure inventory was restored. safe to call repeatedly. proc protects against deducting twice
                            DB.ExecuteSQL("aspdnsf_AdjustInventory " + ONX.ToString() + ",1");

                            Gateway.DecrementMicropayProductsInOrder(ONX);

                            // update transactionstate
                            DB.ExecuteSQL("update Orders set VoidTXCommand='Instant Payment Notification', VoidTXResult=" + DB.SQuote(AppLogic.ro_OK) + ", TransactionState=" + DB.SQuote(AppLogic.ro_TXStateVoided) + ", VoidedOn=getdate(), IsNew=0 where ordernumber=" + ONX.ToString());

                            //Invalidate GiftCards ordered on this order
                            GiftCards GCs = new GiftCards(ONX, GiftCardCollectionFilterType.OrderNumber);
                            foreach (GiftCard gc in GCs)
                            {
                                gc.GiftCardTransactions.Add(GiftCardUsageTransaction.CreateTransaction(gc.GiftCardID, GiftCardUsageReasons.FundsRemovedByAdmin, 0, 0, gc.Balance, ""));
                                gc.UpdateCard(null, null, null, null, 1, null, null, null, null, null, null, null, null, null, null);
                            }

                            //Restore Amount to coupon used in paying for the order
                            if ((CouponTypeEnum)CouponType == CouponTypeEnum.GiftCard)
                            {
                                GiftCard gc = new GiftCard(CouponCode);
                                if (gc.GiftCardID != 0)
                                {
                                    gc.GiftCardTransactions.Add(GiftCardUsageTransaction.CreateTransaction(gc.GiftCardID, GiftCardUsageReasons.FundsAddedByAdmin, 0, 0, CouponDiscountAmount, ""));
                                }
                            }
                            result = AppLogic.ro_OK;
                        }
                    }
                }
            }
        }
    }

}
