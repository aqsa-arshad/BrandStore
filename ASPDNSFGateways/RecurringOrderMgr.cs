// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for RecurringOrderMgr.
    /// </summary>
    public class RecurringOrderMgr
    {

        private Parser m_UseParser;
        private System.Collections.Generic.Dictionary<string, EntityHelper> m_EntityHelpers;

        public RecurringOrderMgr(System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers, Parser UseParser)
        {
            m_EntityHelpers = EntityHelpers;
            m_UseParser = UseParser;
        }

        public String ProcessRecurringOrder(int OriginalRecurringOrderNumber)
        {
            int NewOrderNumber = 0;
            return ProcessRecurringOrder(OriginalRecurringOrderNumber, String.Empty, DateTime.MinValue, out NewOrderNumber);
        }

        // main routine to process any active recurring order (can be subscription autobill or in-cart):
        public String ProcessRecurringOrder(int OriginalRecurringOrderNumber, string XID, DateTime PaymentTime, out int NewOrderNumber)
        {
            NewOrderNumber = 0;
            String status = AppLogic.ro_OK;
            String RecurringSubscriptionID = String.Empty;
            int ProcessCustomerID = Order.GetOrderCustomerID(OriginalRecurringOrderNumber);
            Customer ProcessCustomer = new Customer(ProcessCustomerID, true);
            ShoppingCart cart = new ShoppingCart(ProcessCustomer.SkinID, ProcessCustomer, CartTypeEnum.RecurringCart, OriginalRecurringOrderNumber, true);
            Order originalOrder = new Order(OriginalRecurringOrderNumber);

            if (cart.CartItems.Count != 0 && originalOrder.PaymentMethod != AppLogic.ro_PMPayPalExpress)
            {
                int OrderNumber = AppLogic.GetNextOrderNumber();
                RecurringSubscriptionID = AppLogic.GetRecurringSubscriptionIDFromOrder(OriginalRecurringOrderNumber);

                //we have to set the paymentmethod before processing a recurring order
                Address useBillingAddress = new Address();
                useBillingAddress.LoadByCustomer(cart.ThisCustomer.CustomerID, cart.ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                useBillingAddress.PaymentMethodLastUsed = originalOrder.PaymentMethod;
                useBillingAddress.UpdateDB();

                status = Gateway.MakeRecurringOrder(cart, OrderNumber, RecurringSubscriptionID, XID);

                if (status == AppLogic.ro_OK)
                {
                    NewOrderNumber = OrderNumber;
                    if (!PaymentTime.Equals(DateTime.MinValue))
                    {
                        // update order record with actual time of transaction
                        DB.ExecuteSQL("update orders set OrderDate=" + DB.DateQuote(PaymentTime)
                            + ", CapturedOn=" + DB.DateQuote(PaymentTime)
                            + " where OrderNumber=" + OrderNumber.ToString());
                    }

                    if (AppLogic.AppConfigBool("Recurring.ClearIsNewFlag"))
                    {
                        DB.ExecuteSQL("Update orders set IsNew=0 where OrderNumber=" + OrderNumber.ToString());
                    }

                    try
                    {
                        AppLogic.SendOrderEMail(ProcessCustomer, OrderNumber, true, AppLogic.ro_PMCreditCard, false, m_EntityHelpers, m_UseParser);
                    }
                    catch { }
                }
            }
            else
            {
                status = "Recurring Cart Was Empty (Prior Processed?)";
            }

            if (AppLogic.AppConfigBool("AuditLog.Enabled"))
            {
                StringBuilder sbDetails = new StringBuilder("Result=" + status);
                sbDetails.Append(", RecurringSubscriptionID=" + RecurringSubscriptionID);
                sbDetails.Append(", TransactionID=" + XID);
                sbDetails.Append(", OriginalOrderNumber=" + OriginalRecurringOrderNumber.ToString());
                AppLogic.AuditLogInsert(0, ProcessCustomerID, NewOrderNumber, "ProcessRecurringOrder", sbDetails.ToString(), CommonLogic.GetThisPageName(true), "RecurringOrderMgr");
            }
            return status;
        }

        //Need to create an order record without going through any gateway/payment or other recurring steps - the order has already been changed by PayPal.
        public void ProcessPPECRecurringOrder(int OriginalOrderNumber)
        {
            Order originalOrder = new Order(OriginalOrderNumber);
            Customer recurringCustomer = new Customer(originalOrder.CustomerID);
            ShoppingCart recurringCart = new ShoppingCart(recurringCustomer.SkinID, recurringCustomer, CartTypeEnum.RecurringCart, OriginalOrderNumber, false);

            Address billingAddress = new Address();
            billingAddress.LoadByCustomer(recurringCustomer.CustomerID, recurringCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
            billingAddress.PaymentMethodLastUsed = AppLogic.ro_PMPayPalExpress;

            int newOrderNumber = Gateway.CreateOrderRecord(recurringCart, 0, billingAddress);

            //Update the new order with some recurring information
            String orderUpdateSql = "UPDATE Orders SET TransactionState = @TransactionState, IsNew = @IsNew, ParentOrderNumber = @ParentOrderNumber, Notes = @Notes WHERE OrderNumber = @OrderNumber";

            SqlParameter[] orderUpdateParams = { new SqlParameter("@OrderNumber", newOrderNumber),
                                     new SqlParameter("@TransactionState", AppLogic.ro_TXStateCaptured),
                                     new SqlParameter("@IsNew", true),
                                     new SqlParameter("@ParentOrderNumber", originalOrder),
                                     new SqlParameter("@Notes", "This order was created automatically based on a PayPal Express Checkout notification that the recurring subscription had been charged.") };

            DB.ExecuteSQL(orderUpdateSql, orderUpdateParams);

            //Then update the ShoppingCart record, which is left behind for next time
            CartItem firstRecurringCartItem = recurringCart.CartItems[0];   //Safe to do as we currently only support one recurring schedule per order
            DateTime nextRecurringDate = firstRecurringCartItem.NextRecurringShipDate;
            if (nextRecurringDate.Equals(System.DateTime.MinValue))
            {
                // safety check:
                nextRecurringDate = System.DateTime.Now;
            }

            switch (firstRecurringCartItem.RecurringIntervalType)
            {
                case DateIntervalTypeEnum.Day:
                    nextRecurringDate = nextRecurringDate.AddDays(firstRecurringCartItem.RecurringInterval);
                    break;
                case DateIntervalTypeEnum.Week:
                    nextRecurringDate = nextRecurringDate.AddDays(7 * firstRecurringCartItem.RecurringInterval);
                    break;
                case DateIntervalTypeEnum.Month:
                    nextRecurringDate = nextRecurringDate.AddMonths(firstRecurringCartItem.RecurringInterval);
                    break;
                case DateIntervalTypeEnum.Year:
                    nextRecurringDate = nextRecurringDate.AddYears(firstRecurringCartItem.RecurringInterval);
                    break;
                case DateIntervalTypeEnum.Weekly:
                    nextRecurringDate = nextRecurringDate.AddDays(7);
                    break;
                case DateIntervalTypeEnum.BiWeekly:
                    nextRecurringDate = nextRecurringDate.AddDays(14);
                    break;
                case DateIntervalTypeEnum.EveryFourWeeks:
                    nextRecurringDate = nextRecurringDate.AddDays(28);
                    break;
                case DateIntervalTypeEnum.Monthly:
                    nextRecurringDate = nextRecurringDate.AddMonths(1);
                    break;
                case DateIntervalTypeEnum.Quarterly:
                    nextRecurringDate = nextRecurringDate.AddMonths(3);
                    break;
                case DateIntervalTypeEnum.SemiYearly:
                    nextRecurringDate = nextRecurringDate.AddMonths(6);
                    break;
                case DateIntervalTypeEnum.Yearly:
                    nextRecurringDate = nextRecurringDate.AddYears(1);
                    break;
                default:    //Default to monthly like we do elsewhere
                    nextRecurringDate = nextRecurringDate.AddMonths(1);
                    break;
            }

            String cartUpdateSql = "UPDATE ShoppingCart SET NextRecurringShipDate = @NextRecurringShipDate WHERE OriginalRecurringOrderNumber = @OriginalRecurringOrderNumber AND CartType = @CartType";

            SqlParameter[] cartUpdateParams = { new SqlParameter("@OriginalRecurringOrderNumber", OriginalOrderNumber),
                                                  new SqlParameter("@NextRecurringShipDate", nextRecurringDate),
                                                  new SqlParameter("@CartType", ((int)CartTypeEnum.RecurringCart).ToString()) };

            DB.ExecuteSQL(cartUpdateSql, cartUpdateParams);
        }

        // main routine to cancel any active recurring order (can be subscription autobill or in-cart):
        public String CancelRecurringOrder(int OriginalRecurringOrderNumber)
        {
            String Status = AppLogic.ro_OK;
            if (OriginalRecurringOrderNumber != 0)
            {
                String RecurringSubscriptionID = AppLogic.GetRecurringSubscriptionIDFromOrder(OriginalRecurringOrderNumber);

                if (RecurringSubscriptionID.Length != 0)
                {
                    // a Gateway AutoBill order, so cancel the gateway billing first:
                    String GW = AppLogic.ActivePaymentGatewayCleaned();
                    if (RecurringSubscriptionID.Length != 0)
                    {
                        // dynamically load the gateway processor class via the name
                        GatewayProcessor processor = GatewayLoader.GetProcessor(GW);

                        IDictionary<string, string> transactionContext = new Dictionary<string, string>();

                        if (RecurringSubscriptionID.ToUpper().StartsWith("B-"))
                            transactionContext.Add("TENDER", "P");

                        if (processor != null)
                        {
                            Status = processor.RecurringBillingCancelSubscription(RecurringSubscriptionID, OriginalRecurringOrderNumber, transactionContext);
                        }
                        else
                        {
                            if (GW == Gateway.ro_GWVERISIGN || GW == Gateway.ro_GWPAYFLOWPRO)
                            {
                                GatewayProcessor pfp = GatewayLoader.GetProcessor(Gateway.ro_GWPAYFLOWPRO);

                                Status = pfp.RecurringBillingCancelSubscription(RecurringSubscriptionID, OriginalRecurringOrderNumber, transactionContext);
                            }
                            else
                            {
                                Status = "Invalid Gateway";
                            }
                        }
                    }
                }

                int ProcessCustomerID = Order.GetOrderCustomerID(OriginalRecurringOrderNumber);

                if (Status == AppLogic.ro_OK)
                {
                    // now clean it up in the cart only if it cannot be restarted/reactivated
                    DB.ExecuteSQL(String.Format("delete from kitcart where OriginalRecurringOrderNumber={0}", OriginalRecurringOrderNumber.ToString()));
                    DB.ExecuteSQL(String.Format("delete from ShoppingCart where OriginalRecurringOrderNumber={0}", OriginalRecurringOrderNumber.ToString()));

                    // now notify customer of cancellation:
                    Customer ProcessCustomer = new Customer(ProcessCustomerID, true);

                    try
                    {
                        // send email notification to customer
                        string emailSubject = String.Format(AppLogic.GetString("recurringorder.canceled.subject", ProcessCustomer.SkinID, ProcessCustomer.LocaleSetting), AppLogic.AppConfig("StoreName"));
                        string emailBody = String.Format(AppLogic.GetString("recurringorder.canceled.body", ProcessCustomer.SkinID, ProcessCustomer.LocaleSetting), OriginalRecurringOrderNumber.ToString());
                        AppLogic.SendMail(emailSubject,
                            emailBody + AppLogic.AppConfig("MailFooter"),
                            true,
                            AppLogic.AppConfig("ReceiptEMailFrom"),
                            AppLogic.AppConfig("ReceiptEMailFromName"),
                            ProcessCustomer.EMail,
                            ProcessCustomer.EMail,
                            String.Empty,
                            AppLogic.MailServer());


                        // send email notification to admin
                        if (AppLogic.AppConfig("GotOrderEMailTo").Length != 0 && !AppLogic.AppConfigBool("TurnOffStoreAdminEMailNotifications"))
                        {
                            String SendToList = AppLogic.AppConfig("GotOrderEMailTo").Replace(",", ";");
                            if (SendToList.IndexOf(';') != -1)
                            {
                                foreach (String s in SendToList.Split(';'))
                                {
                                    AppLogic.SendMail(emailSubject,
                                        emailBody + AppLogic.AppConfig("MailFooter"),
                                        true,
                                        AppLogic.AppConfig("GotOrderEMailFrom"),
                                        AppLogic.AppConfig("GotOrderEMailFromName"),
                                        s.Trim(),
                                        s.Trim(),
                                        String.Empty,
                                        AppLogic.MailServer());
                                }
                            }
                            else
                            {
                                AppLogic.SendMail(emailSubject,
                                    emailBody + AppLogic.AppConfig("MailFooter"),
                                    true,
                                    AppLogic.AppConfig("GotOrderEMailFrom"),
                                    AppLogic.AppConfig("GotOrderEMailFromName"),
                                    SendToList,
                                    SendToList,
                                    String.Empty,
                                    AppLogic.MailServer());
                            }
                        }
                    }
                    catch { }
                }

                if (AppLogic.AppConfigBool("AuditLog.Enabled"))
                {
                    StringBuilder sbDetails = new StringBuilder("Result=" + Status);
                    sbDetails.Append(", RecurringSubscriptionID=" + RecurringSubscriptionID);
                    AppLogic.AuditLogInsert(0, ProcessCustomerID, OriginalRecurringOrderNumber, "CancelRecurringOrder", sbDetails.ToString(), CommonLogic.GetThisPageName(true), "RecurringOrderMgr");
                }
            }
            return Status;
        }

        public string CancelPPECRecurringOrder(int OriginalOrderNumber, bool TriggeredByPayPal)
        {
            String result = AppLogic.ro_OK;
            Order originalOrder = new Order(OriginalOrderNumber);
            Customer customerToNotify = new Customer(originalOrder.CustomerID, true);

            //Cancel the recurring profile on PayPal's end
            if (TriggeredByPayPal == false)
                result = PayPalController.CancelECRecurringProfile(OriginalOrderNumber);

            if (result == AppLogic.ro_OK)
            {
                //Cancel the recurring profile in the cart
                DB.ExecuteSQL(String.Format("delete from kitcart where OriginalRecurringOrderNumber={0}", OriginalOrderNumber.ToString()));
                DB.ExecuteSQL(String.Format("delete from ShoppingCart where OriginalRecurringOrderNumber={0}", OriginalOrderNumber.ToString()));

                // now notify people of cancellation:
                try
                {
                    // send email notification to customer
                    string emailSubject = String.Format(AppLogic.GetString("recurringorder.canceled.subject", customerToNotify.SkinID, customerToNotify.LocaleSetting), AppLogic.AppConfig("StoreName"));
                    string emailBody = String.Format(AppLogic.GetString("recurringorder.canceled.body", customerToNotify.SkinID, customerToNotify.LocaleSetting), OriginalOrderNumber.ToString());
                    AppLogic.SendMail(emailSubject,
                        emailBody + AppLogic.AppConfig("MailFooter"),
                        true,
                        AppLogic.AppConfig("ReceiptEMailFrom"),
                        AppLogic.AppConfig("ReceiptEMailFromName"),
                        customerToNotify.EMail,
                        customerToNotify.EMail,
                        String.Empty,
                        AppLogic.MailServer());


                    // send email notification to admin
                    if (AppLogic.AppConfig("GotOrderEMailTo").Length != 0 && !AppLogic.AppConfigBool("TurnOffStoreAdminEMailNotifications"))
                    {
                        String SendToList = AppLogic.AppConfig("GotOrderEMailTo").Replace(",", ";");
                        if (SendToList.IndexOf(';') != -1)
                        {
                            foreach (String s in SendToList.Split(';'))
                            {
                                AppLogic.SendMail(emailSubject,
                                    emailBody + AppLogic.AppConfig("MailFooter"),
                                    true,
                                    AppLogic.AppConfig("GotOrderEMailFrom"),
                                    AppLogic.AppConfig("GotOrderEMailFromName"),
                                    s.Trim(),
                                    s.Trim(),
                                    String.Empty,
                                    AppLogic.MailServer());
                            }
                        }
                        else
                        {
                            AppLogic.SendMail(emailSubject,
                                emailBody + AppLogic.AppConfig("MailFooter"),
                                true,
                                AppLogic.AppConfig("GotOrderEMailFrom"),
                                AppLogic.AppConfig("GotOrderEMailFromName"),
                                SendToList,
                                SendToList,
                                String.Empty,
                                AppLogic.MailServer());
                        }
                    }
                }
                catch
                {
                    SysLog.LogMessage(String.Format(AppLogic.GetStringForDefaultLocale("recurringorder.canceled.body"),
                                        OriginalOrderNumber.ToString()),
                                        result,
                                        MessageTypeEnum.Informational,
                                        MessageSeverityEnum.Message);
                }
            }

            return result;
        }

        // --------------------------------------------------------------------------------------
        // helper routines for Gateway AutoBill functions:
        // --------------------------------------------------------------------------------------

        public String ProcessAutoBillDeclined(int OriginalRecurringOrderNumber, String XID, DateTime PaymentTime, String RecurringSubscriptionID, String Reason)
        {
            int ProcessCustomerID = Order.GetOrderCustomerID(OriginalRecurringOrderNumber);
            Customer ProcessCustomer = new Customer(ProcessCustomerID, true);
            String GW = AppLogic.ActivePaymentGatewayCleaned();

            if (PaymentTime.Equals(DateTime.MinValue))
            {
                PaymentTime = DateTime.Now;
            }
            if (Reason.Length != 0)
            {
                Reason += ", ";
            }
            Reason += "OriginalRecurringOrderNumber=" + OriginalRecurringOrderNumber.ToString() + ", RecurringSubscriptionID=" + RecurringSubscriptionID;
            if (XID.Length != 0)
            {
                Reason += ", PNREF=" + XID;
            }
            // record in failed transactions, send e-mail to customer, etc.
            DB.ExecuteSQL("insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult,CustomerEMailed,RecurringSubscriptionID) values(" +
                ProcessCustomer.CustomerID.ToString() + "," + OriginalRecurringOrderNumber.ToString() + "," +
                DB.SQuote(ProcessCustomer.LastIPAddress) + "," + DB.DateQuote(PaymentTime) + "," + DB.SQuote(GW) + "," +
                DB.SQuote(AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()) + "," + DB.SQuote(AppLogic.ro_NotApplicable) + "," + DB.SQuote(Reason) + ",1," + DB.SQuote(RecurringSubscriptionID) + ")");

            try
            {
                // send email notification to customer
                string emailSubject = String.Format(AppLogic.GetString("recurringorder.declined.subject", ProcessCustomer.SkinID, ProcessCustomer.LocaleSetting), AppLogic.AppConfig("StoreName"));
                string emailBody = String.Format(AppLogic.GetString("recurringorder.declined.body", ProcessCustomer.SkinID, ProcessCustomer.LocaleSetting), OriginalRecurringOrderNumber.ToString(), RecurringSubscriptionID);
                AppLogic.SendMail(emailSubject,
                    emailBody + AppLogic.AppConfig("MailFooter"),
                    true,
                    AppLogic.AppConfig("ReceiptEMailFrom"),
                    AppLogic.AppConfig("ReceiptEMailFromName"),
                    ProcessCustomer.EMail,
                    ProcessCustomer.EMail,
                    String.Empty,
                    AppLogic.MailServer());

                // send email notification to admin
                if (AppLogic.AppConfig("GotOrderEMailTo").Length != 0 && !AppLogic.AppConfigBool("TurnOffStoreAdminEMailNotifications"))
                {
                    String SendToList = AppLogic.AppConfig("GotOrderEMailTo").Replace(",", ";");
                    if (SendToList.IndexOf(';') != -1)
                    {
                        foreach (String s in SendToList.Split(';'))
                        {
                            AppLogic.SendMail(emailSubject,
                                emailBody + AppLogic.AppConfig("MailFooter"),
                                true,
                                AppLogic.AppConfig("GotOrderEMailFrom"),
                                AppLogic.AppConfig("GotOrderEMailFromName"),
                                s.Trim(),
                                s.Trim(),
                                String.Empty,
                                AppLogic.MailServer());
                        }
                    }
                    else
                    {
                        AppLogic.SendMail(emailSubject,
                            emailBody + AppLogic.AppConfig("MailFooter"),
                            true,
                            AppLogic.AppConfig("GotOrderEMailFrom"),
                            AppLogic.AppConfig("GotOrderEMailFromName"),
                            SendToList,
                            SendToList,
                            String.Empty,
                            AppLogic.MailServer());
                    }
                }
            }
            catch { }

            if (AppLogic.AppConfigBool("AuditLog.Enabled"))
            {
                AppLogic.AuditLogInsert(0, ProcessCustomerID, OriginalRecurringOrderNumber, "ProcessAutoBillDeclined", Reason, CommonLogic.GetThisPageName(true), "RecurringOrderMgr");
            }
            return AppLogic.ro_OK;
        }

        public String ProcessAutoBillApproved(int OriginalRecurringOrderNumber, String XID, DateTime PaymentTime, out int NewOrderNumber)
        {
            return ProcessRecurringOrder(OriginalRecurringOrderNumber, XID, PaymentTime, out NewOrderNumber);
        }

        public String ProcessAutoBillGetAdminButtons(int OriginalRecurringOrderNumber, out bool ShowCancelButton, out bool ShowRetryButton, out bool ShowRestartButton, out String GatewayStatus)
        {
            String result = AppLogic.ro_OK;

            // These are the default values that will be returned for gateways 
            // that don't support getting realtime status.
            ShowCancelButton = true;
            ShowRetryButton = false;
            ShowRestartButton = false;
            GatewayStatus = String.Empty;

            DateTime StartDate = DateTime.MinValue;
            DateTime NextPaymentDate = DateTime.MinValue;
            Decimal AggregateAmount = 0.0M;
            String RecurringStatus = String.Empty;
            String LatestPaymentIdentifier = String.Empty;
            DateTime EndingDate = DateTime.MinValue;

            String GW = AppLogic.ActivePaymentGatewayCleaned();
            String RecurringSubscriptionID = AppLogic.GetRecurringSubscriptionIDFromOrder(OriginalRecurringOrderNumber);

            if (RecurringSubscriptionID.Length != 0)
            {
                if (GW == Gateway.ro_GWVERISIGN || GW == Gateway.ro_GWPAYFLOWPRO)
                {
                    result = PayFlowProController.RecurringBillingInquiry(RecurringSubscriptionID, out StartDate, out NextPaymentDate, out AggregateAmount, out RecurringStatus, out LatestPaymentIdentifier, out EndingDate);

                    if (result == AppLogic.ro_OK)
                    {
                        GatewayStatus = "Payflow Gateway Status: " + RecurringStatus;

                        switch (RecurringStatus)
                        {
                            case "VENDOR INACTIVE":
                                ShowCancelButton = false;
                                ShowRetryButton = false;
                                ShowRestartButton = false;
                                break;
                            case "DEACTIVATED BY MERCHANT":
                                ShowCancelButton = false;
                                ShowRetryButton = false;
                                ShowRestartButton = true;
                                break;
                            case "EXPIRED":
                                ShowCancelButton = false;
                                ShowRetryButton = false;
                                ShowRestartButton = true;
                                break;
                            case "TOO MANY FAILURES":
                                ShowCancelButton = false;
                                ShowRetryButton = true;
                                ShowRestartButton = true;
                                break;
                            case "ACTIVE":
                                ShowCancelButton = true;
                                ShowRetryButton = false;
                                ShowRestartButton = false;
                                break;
                            case "RETRYING CURRENT PAYMENT":
                                ShowCancelButton = true;
                                ShowRetryButton = true;
                                ShowRestartButton = false;
                                break;
                        }
                    }
                }
            }
            return result;

        }

        public String ProcessAutoBillGetGatewayStatus(int OriginalRecurringOrderNumber, out String RecurringSubscriptionID, out DateTime StartDate, out DateTime NextPaymentDate, out decimal AggregateAmount, out String RecurringStatus, out String LatestPaymentIdentifier, out DateTime EndingDate)
        {
            // retry a payment for the current billing period after the gateway auto-bill attempt failed
            String Status = AppLogic.ro_OK;
            String GW = AppLogic.ActivePaymentGatewayCleaned();

            RecurringSubscriptionID = AppLogic.GetRecurringSubscriptionIDFromOrder(OriginalRecurringOrderNumber);
            StartDate = DateTime.MinValue;
            NextPaymentDate = DateTime.MinValue;
            AggregateAmount = 0.0M;
            RecurringStatus = String.Empty;
            LatestPaymentIdentifier = String.Empty;
            EndingDate = DateTime.MinValue;

            if (RecurringSubscriptionID.Length != 0)
            {
                if (GW == Gateway.ro_GWVERISIGN || GW == Gateway.ro_GWPAYFLOWPRO)
                {
                    Status = PayFlowProController.RecurringBillingInquiry(RecurringSubscriptionID, out StartDate, out NextPaymentDate, out AggregateAmount, out RecurringStatus, out LatestPaymentIdentifier, out EndingDate);
                }
                else
                {
                    Status = "Invalid Gateway";
                }
            }
            return Status;

        }

        public String ProcessAutoBillRestartPayment(int OriginalRecurringOrderNumber)
        {
            // retry a payment for the current billing period after the gateway auto-bill attempt failed
            String Status = AppLogic.ro_OK;
            String GW = AppLogic.ActivePaymentGatewayCleaned();
            String RecurringSubscriptionID = AppLogic.GetRecurringSubscriptionIDFromOrder(OriginalRecurringOrderNumber);
            if (RecurringSubscriptionID.Length != 0)
            {
                if (GW == Gateway.ro_GWVERISIGN || GW == Gateway.ro_GWPAYFLOWPRO)
                {

                    if (Status == AppLogic.ro_OK)
                    {
                        Status = PayFlowProController.RecurringBillingRestartPayment(RecurringSubscriptionID, OriginalRecurringOrderNumber);
                    }
                }
                else
                {
                    Status = "Invalid Gateway";
                }
            }

            if (AppLogic.AppConfigBool("AuditLog.Enabled"))
            {
                StringBuilder sbDetails = new StringBuilder("Result=" + Status);
                sbDetails.Append(", RecurringSubscriptionID=" + RecurringSubscriptionID);
                int ProcessCustomerID = Order.GetOrderCustomerID(OriginalRecurringOrderNumber);
                AppLogic.AuditLogInsert(0, ProcessCustomerID, OriginalRecurringOrderNumber, "ProcessAutoBillRestartPayment", sbDetails.ToString(), CommonLogic.GetThisPageName(true), "RecurringOrderMgr");
            }
            return Status;

        }

        public String ProcessAutoBillRetryPayment(int OriginalRecurringOrderNumber)
        {
            // retry a payment for the current billing period after the gateway auto-bill attempt failed
            String Status = AppLogic.ro_OK;
            String GW = AppLogic.ActivePaymentGatewayCleaned();
            String RecurringSubscriptionID = AppLogic.GetRecurringSubscriptionIDFromOrder(OriginalRecurringOrderNumber);
            if (RecurringSubscriptionID.Length != 0)
            {
                if (GW == Gateway.ro_GWVERISIGN || GW == Gateway.ro_GWPAYFLOWPRO)
                {
                    DateTime StartDate = DateTime.MinValue;
                    DateTime NextPaymentDate = DateTime.MinValue;
                    Decimal AggregateAmount = 0.0M;
                    String RecurringStatus = String.Empty;
                    String LatestPaymentIdentifier = String.Empty;
                    DateTime EndingDate = DateTime.MinValue;
                    String tmpSubID = String.Empty;
                    // Need to get the Current Payment Identifier before we attemp the retry.
                    Status = ProcessAutoBillGetGatewayStatus(OriginalRecurringOrderNumber, out tmpSubID, out StartDate, out NextPaymentDate, out AggregateAmount, out RecurringStatus, out LatestPaymentIdentifier, out EndingDate);
                    if (Status == AppLogic.ro_OK)
                    {
                        Status = PayFlowProController.RecurringBillingRetryPayment(RecurringSubscriptionID, OriginalRecurringOrderNumber, LatestPaymentIdentifier);
                    }
                }
                else
                {
                    Status = "Invalid Gateway";
                }
            }

            if (AppLogic.AppConfigBool("AuditLog.Enabled"))
            {
                StringBuilder sbDetails = new StringBuilder("Result=" + Status);
                sbDetails.Append(", RecurringSubscriptionID=" + RecurringSubscriptionID);
                int ProcessCustomerID = Order.GetOrderCustomerID(OriginalRecurringOrderNumber);
                AppLogic.AuditLogInsert(0, ProcessCustomerID, OriginalRecurringOrderNumber, "ProcessAutoBillRetryPayment", sbDetails.ToString(), CommonLogic.GetThisPageName(true), "RecurringOrderMgr");
            }
            return Status;

        }

        /// <summary>
        /// This will check for an existing AutoBill order for the CustomerID and
        /// return the number of days left on that order's subscription, if any.
        /// The existing AutoBill order will be canceled and the items deleted
        /// from the cart.
        /// This should only be used with AppConfig Recurring.LimitCustomerToOneOrder=TRUE
        /// </summary>
        /// <param name="CustomerID"></param>
        /// <returns>Number of subscription days to migrate from existing order. If not a subscription we return zero.</returns>
        public static int ProcessAutoBillMigrateExisting(int CustomerID)
        {
            // This should only be used with AppConfig Recurring.LimitCustomerToOneOrder=TRUE

            int MigrateDays = 0;
            int OriginalRecurringOrderNumber = 0;
            bool IsSubscription = false;
            String Status = AppLogic.ro_OK;
            String RecurringSubscriptionID = String.Empty;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select top 1 OriginalRecurringOrderNumber, SubscriptionInterval from ShoppingCart  with (NOLOCK)  where RecurringSubscriptionID<>'' and CustomerID=" + CustomerID.ToString() + " order by OriginalRecurringOrderNumber desc", dbconn))
                {
                    if (rs.Read())
                    {
                        OriginalRecurringOrderNumber = DB.RSFieldInt(rs, "OriginalRecurringOrderNumber");
                        IsSubscription = (DB.RSFieldInt(rs, "SubscriptionInterval") > 0);
                    }
                }

            }

            if (OriginalRecurringOrderNumber != 0)
            {
                if (IsSubscription && !AppLogic.AppConfigBool("SubscriptionExtensionOccursFromOrderDate"))
                {
                    // get customer's current subscription expiration and compute days remaining
                    using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                    {
                        dbconn.Open();
                        using (IDataReader rsCust = DB.GetRS("Select SubscriptionExpiresOn from customer  with (NOLOCK)  where CustomerID=" + CustomerID.ToString(), dbconn))
                        {
                            if (rsCust.Read())
                            {
                                TimeSpan TimeRemaining = DB.RSFieldDateTime(rsCust, "SubscriptionExpiresOn").Subtract(DateTime.Today);
                                // Only carry forward if Expires in future
                                if (TimeRemaining.Days > 0)
                                {
                                    MigrateDays = TimeRemaining.Days;
                                }
                            }
                        }

                    }

                }

                RecurringSubscriptionID = AppLogic.GetRecurringSubscriptionIDFromOrder(OriginalRecurringOrderNumber);

                if (RecurringSubscriptionID.Length != 0)
                {
                    // cancel the existing gateway billing
                    String GW = AppLogic.ActivePaymentGatewayCleaned();
                    if (RecurringSubscriptionID.Length != 0)
                    {
                        if (GW == Gateway.ro_GWVERISIGN || GW == Gateway.ro_GWPAYFLOWPRO)
                        {
                            GatewayProcessor pfp = GatewayLoader.GetProcessor(Gateway.ro_GWPAYFLOWPRO);
                            IDictionary<string, string> transactionContext = new Dictionary<string, string>();

                            if (RecurringSubscriptionID.ToUpper().StartsWith("B-"))
                                transactionContext.Add("TENDER", "P");

                            Status = pfp.RecurringBillingCancelSubscription(RecurringSubscriptionID, OriginalRecurringOrderNumber, transactionContext);
                        }
                        else
                        {
                            Status = "Invalid Gateway";
                        }
                    }
                }

                // now clean up the original order from the cart
                DB.ExecuteSQL(String.Format("delete from kitcart where OriginalRecurringOrderNumber={0}", OriginalRecurringOrderNumber.ToString()));
                DB.ExecuteSQL(String.Format("delete from ShoppingCart where OriginalRecurringOrderNumber={0}", OriginalRecurringOrderNumber.ToString()));
            }
            else
            {
                Status = "OriginalRecurringOrderNumber Not Found.";
            }

            if (AppLogic.AppConfigBool("AuditLog.Enabled"))
            {
                StringBuilder sbDetails = new StringBuilder("Result=" + Status);
                sbDetails.Append(", RecurringSubscriptionID=" + RecurringSubscriptionID);
                sbDetails.Append(", MigrateDays=" + MigrateDays.ToString());
                AppLogic.AuditLogInsert(0, CustomerID, OriginalRecurringOrderNumber, "ProcessAutoBillMigrateExisting", sbDetails.ToString(), CommonLogic.GetThisPageName(true), "RecurringOrderMgr");
            }
            return MigrateDays;
        }

        public String ProcessAutoBillPartialRefund(int OriginalRecurringOrderNumber)
        {
            // determine the "remaining partial refund" on this subscription, and execute an ad hoc credit order for the customer

            String Status = AppLogic.ro_OK;

            // TBD COMPUTE AND EXECUTE PARTIAL REFUND HERE
            // ...
            // ...
            // tbd

            if (Status == AppLogic.ro_OK)
            {
                Status = ProcessAutoBillCancel(OriginalRecurringOrderNumber);
            }

            if (AppLogic.AppConfigBool("AuditLog.Enabled"))
            {
                StringBuilder sbDetails = new StringBuilder("Result=" + Status);
                AppLogic.AuditLogInsert(0, 0, OriginalRecurringOrderNumber, "ProcessAutoBillPartialRefund", sbDetails.ToString(), CommonLogic.GetThisPageName(true), "RecurringOrderMgr");
            }
            return Status;
        }

        public String ProcessAutoBillFullRefund(int OriginalRecurringOrderNumber)
        {
            // do a full refund on this subscription, and execute refund for the customer
            Order ord = new Order(OriginalRecurringOrderNumber);
            // TBD use string resource here:
            String Status = Gateway.OrderManagement_DoFullRefund(ord, ord.LocaleSetting, "AutoBill Recurring Order Canceled");
            if (Status == AppLogic.ro_OK)
            {
                Status = ProcessAutoBillCancel(OriginalRecurringOrderNumber);
            }

            if (AppLogic.AppConfigBool("AuditLog.Enabled"))
            {
                StringBuilder sbDetails = new StringBuilder("Result=" + Status);
                AppLogic.AuditLogInsert(0, 0, OriginalRecurringOrderNumber, "ProcessAutoBillFullRefund", sbDetails.ToString(), CommonLogic.GetThisPageName(true), "RecurringOrderMgr");
            }
            return Status;
        }

        public String ProcessAutoBillCancel(int OriginalRecurringOrderNumber)
        {
            // cancel this subscription, no money changes hands...it's just a cancel of all subsequent recurring events for this subscription
            return CancelRecurringOrder(OriginalRecurringOrderNumber);
        }

        public String ProcessAutoBillAddressUpdate(int OriginalRecurringOrderNumber, Address UseNewBillingInfo)
        {
            // update subscription to use new billing info
            String Status = AppLogic.ro_OK;
            String GW = AppLogic.ActivePaymentGatewayCleaned();
            String RecurringSubscriptionID = AppLogic.GetRecurringSubscriptionIDFromOrder(OriginalRecurringOrderNumber);
            if (RecurringSubscriptionID.Length != 0)
            {
                // dynamically load the gateway processor class via the name
                GatewayProcessor processor = GatewayLoader.GetProcessor(GW);
                if (processor != null)
                {
                    Status = processor.RecurringBillingAddressUpdate(RecurringSubscriptionID,
                                        OriginalRecurringOrderNumber,
                                        UseNewBillingInfo);
                }
                else
                {
                    if (GW == Gateway.ro_GWVERISIGN || GW == Gateway.ro_GWPAYFLOWPRO)
                    {
                        GatewayProcessor pfp = GatewayLoader.GetProcessor(Gateway.ro_GWPAYFLOWPRO);

                        Status = pfp.RecurringBillingAddressUpdate(RecurringSubscriptionID, OriginalRecurringOrderNumber, UseNewBillingInfo);
                    }
                    else
                    {
                        Status = "Invalid Gateway";
                    }
                }
            }

            if (AppLogic.AppConfigBool("AuditLog.Enabled"))
            {
                StringBuilder sbDetails = new StringBuilder("Result=" + Status);
                sbDetails.Append(", RecurringSubscriptionID=" + RecurringSubscriptionID);
                sbDetails.Append(", New Address=" + UseNewBillingInfo.DisplayHTML(true));
                AppLogic.AuditLogInsert(0, 0, OriginalRecurringOrderNumber, "ProcessAutoBillAddressUpdate", sbDetails.ToString(), CommonLogic.GetThisPageName(true), "RecurringOrderMgr");
            }
            return Status;
        }

        public String ProcessAutoBillStatusFile(String GW, String StatusFile, out String Results)
        {
            String Status = AppLogic.ro_OK;
            Results = String.Empty;
            StringBuilder tmpS = new StringBuilder(4096);

            GatewayProcessor GWActual = GatewayLoader.GetProcessor(GW);
            if (GWActual != null)
            {
                string gwresults;
                Status = GWActual.ProcessAutoBillStatusFile(GW, StatusFile, out gwresults, this);
                tmpS.Append(gwresults);
            }

            tmpS.Append("\nEND_OF_FILE");
            Results = tmpS.ToString();
            return Status;
        }

        public String GetAutoBillStatusFile(String GW, out String StatusFile)
        {
            String Status = AppLogic.ro_OK;
            StatusFile = String.Empty;

            if (GW == Gateway.ro_GWVERISIGN || GW == Gateway.ro_GWPAYFLOWPRO)
            {
                StatusFile = GatewayLoader.GetProcessor(Gateway.ro_GWPAYFLOWPRO).RecurringBillingGetStatusFile();
                return Status;
            }

            GatewayProcessor GWActual = GatewayLoader.GetProcessor(GW);
            if (GW != null)
            {
                StatusFile = GWActual.RecurringBillingGetStatusFile();
            }

            return Status;
        }

    }
}
