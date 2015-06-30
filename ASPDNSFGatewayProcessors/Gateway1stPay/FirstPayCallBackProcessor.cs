// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontGateways.Processors
{
    public class FirstPayCallBackProcessor
    {
        #region Instance Variables
        private Customer ThisCustomer;
        private Dictionary<string, string> Params;
        private string ParamString;
        private FirstPay gateway;
        private ShoppingCart cart;

        public bool Success 
        {
            get
            {
                bool success;
                bool.TryParse(GetParameter("success"), out success);
                return success;
            }
        }

        private string ReferenceNumber
        {
            get
            {
                return GetParameter("reference_number");
            }
        }

        public string AuthResponse
        {
            get
            {
                return GetParameter("auth_response");
            }
        }
     
        private string AuthCode
        {
            get
            {
                return GetParameter("auth_code");
            }
        }

        private string AVSResponse
        {
            get
            {
                return GetParameter("avs_response");
            }
        }

        private string CVV2Response
        {
            get
            {
                return GetParameter("cvv2_response").Trim();
            }
        }

        public string ErrorMessage
        {
            get
            {
                return GetParameter("error_message");
            }
        }

        private decimal Total
        {
            get
            {
                decimal total;
                decimal.TryParse(GetParameter("total"), out total);

                return total;
            }
        }

        public int OrderNumber
        {
            get
            {               
                return GetOrderNumber();
            }
        }

        private string CimRefNumber
        {
            get
            {
                return GetParameter("cim_ref_num");
            }
        }

        private string CCExpMonth
        {
            get
            {
                return GetParameter("card_exp_mm");
            }
        }

        private string CCExpYear
        {
            get
            {
                return GetParameter("card_exp_yy");
            }
        }

        private string CCType
        {
            get
            {
                return GetParameter("card_type");
            }
        }

        private string CCNumberLast4
        {
            get
            {
                return GetParameter("card_num4");
            }
        }

        private string ADNSFTransactionState
        {
            get
            {
                if (AuthResponse != null && AuthResponse.ToLower().Contains("approved"))
                {
                    if (AppLogic.TransactionModeIsAuthCapture())
                        return AppLogic.ro_TXStateCaptured;
                    else if (AppLogic.TransactionModeIsAuthOnly())
                        return AppLogic.ro_TXStateAuthorized;                        
                }

                return AppLogic.ro_TXStateError;
            }
        }

        #endregion

        public FirstPayCallBackProcessor(Customer customer, string returnedParameters)
        {
            gateway = new FirstPay();
            ThisCustomer = customer;
            ParamString = returnedParameters;
            Params = GetParameterStringAsDictionary(returnedParameters);
            cart = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
        }

        public string ProcessCallBack()
        {
            string PM = AppLogic.CleanPaymentMethod(AppLogic.ro_PMCreditCard);
            AppLogic.ValidatePM(PM); // this WILL throw a hard security exception on any problem!
                        
            //recalculate total for verification
            decimal cartTotal = cart.Total(true);
            decimal orderTotal = cartTotal - CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(cartTotal < cart.Coupon.DiscountAmount, cartTotal, cart.Coupon.DiscountAmount), 0);
            orderTotal = Localization.ParseNativeDecimal(Localization.CurrencyStringForGatewayWithoutExchangeRate(orderTotal));

            if (!ThisCustomer.HasCustomerRecord)
            {
                FirstPay.order_id = 0;
                throw new System.Security.SecurityException("Customer not signed in to complete transaction.");
            }

            if (!Success)
            {
                string IP = "";
                if (cart != null)
                {
                    IP = cart.ThisCustomer.LastIPAddress;
                }

                string sql = "insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult) values(" + ThisCustomer.CustomerID.ToString() + "," + OrderNumber.ToString() + "," + DB.SQuote(IP) + ",getdate()," + DB.SQuote("1stPay") + "," + DB.SQuote(AppLogic.ro_PMCreditCard) + "," + DB.SQuote("") + "," + DB.SQuote(ParamString) + ")";
                DB.ExecuteSQL(sql);
                return ReturnFirstPayError();
            }

            //Need to add this to check that the transaction processed through the gateway and that the charged amount matches the orderTotal.
            ConfirmTransaction(orderTotal); // this WILL throw a hard security exception on any problem!


            if (cart.IsEmpty())
            {
                ErrorMessage er = new ErrorMessage("Could not complete the transaction because the shopping cart was empty.");
                var checkoutController = CheckOutPageControllerFactory.CreateCheckOutPageController(ThisCustomer, cart);
                return checkoutController.GetCheckoutPaymentPage() + "?errormsg=" + er.MessageId;
            }

            //the callback is valid. make the order.
            int orderNumber = OrderNumber;

            //Setup param list
            List<SqlParameter> sqlParams = new List<SqlParameter>();            

            try
            {
                ThisCustomer.PrimaryBillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCreditCard;
                ThisCustomer.PrimaryBillingAddress.UpdateDB();
                //Process as AuthOnly first
                string status = Gateway.MakeOrder(AppLogic.ro_PMCreditCard, AppLogic.TransactionModeIsAuthOnly() ? AppLogic.ro_TXModeAuthOnly : AppLogic.ro_TXModeAuthCapture, cart, orderNumber, "", "", ReferenceNumber, "");

                if (status == AppLogic.ro_OK)
                {
                    string AVSResult = AVSResponse;
                    string AuthorizationCode = AuthCode;
                    string AuthorizationTransID = ReferenceNumber;

                    //Add all the params needed.
                    sqlParams.Add(new SqlParameter("@AuthorizationTransId", AuthorizationTransID));
                    sqlParams.Add(new SqlParameter("@AVSResult", AVSResult));
                    sqlParams.Add(new SqlParameter("@AuthorizationCode", AuthorizationCode));
                    sqlParams.Add(new SqlParameter("@CCType", CCType));
                    sqlParams.Add(new SqlParameter("@CCExpMonth", CCExpMonth));
                    sqlParams.Add(new SqlParameter("@CCExpYear", CCExpYear));
                    sqlParams.Add(new SqlParameter("@CCNumberLast4", CCNumberLast4));
                    sqlParams.Add(new SqlParameter("@OrderNumber", orderNumber));
                    sqlParams.Add(new SqlParameter("@TransactionState", ADNSFTransactionState));

                    if (CVV2Response.Length > 0)
                    {
                        AVSResult += ", CV Result: " + CVV2Response;
                    }

                    // Now, if paid for, process as Captured
                    if (ADNSFTransactionState == AppLogic.ro_TXStateAuthorized)
                    {
                        string sql = "Update Orders Set AuthorizationPNREF=@AuthorizationTransId"
                                                    + ", AVSResult=@AVSResult"
                                                    + ", AuthorizationCode=@AuthorizationCode"
                                                    + ", CardType=@CCType"
                                                    + ", CardExpirationMonth=@CCExpMonth"
                                                    + ", CardExpirationYear=@CCExpYear"
                                                    + ", Last4=@CCNumberLast4"
                                                    + " Where OrderNumber=@OrderNumber;";


                        DB.ExecuteSQL(sql, sqlParams.ToArray());
                    }
                    if (ADNSFTransactionState == AppLogic.ro_TXStateCaptured)
                    {
                        string sql = "Update Orders Set AuthorizationPNREF=@AuthorizationTransId + '|CAPTURE=' + @AuthorizationTransId"
                                                    + ", AVSResult=@AVSResult"
                                                    + ", AuthorizationCode=@AuthorizationCode"
                                                    + ", CardType=@CCType"
                                                    + ", CardExpirationMonth=@CCExpMonth"
                                                    + ", CardExpirationYear=@CCExpYear"
                                                    + ", Last4=@CCNumberLast4"
                                                    + ", CapturedOn=getdate()"
                                                    + " Where OrderNumber=@OrderNumber;";

                        Gateway.ProcessOrderAsCaptured(orderNumber);
                        DB.ExecuteSQL(sql, sqlParams.ToArray());
                    }
                    else if (ADNSFTransactionState == AppLogic.ro_TXStateError)
                    {
                        DB.ExecuteSQL("update orders set TransactionState=@TransactionState where OrderNumber=@OrderNumber;", sqlParams.ToArray());
                    }

                    if (!string.IsNullOrEmpty(ReferenceNumber))
                    {
                        OrderTransactionCollection transactions = new OrderTransactionCollection(orderNumber);
                        transactions.AddTransaction(ADNSFTransactionState, null, ParamString, AuthorizationTransID, AuthorizationCode, AppLogic.ro_PMCreditCard, gateway.DisplayName(ThisCustomer.LocaleSetting), orderTotal);
                    }
                }
                else if(status != AppLogic.ro_3DSecure) // If the status is anything but Ok or 3DSecure then send them back to the checkout process and display the error status
                {
                    ErrorMessage er = new ErrorMessage(status);
                    var checkoutController = CheckOutPageControllerFactory.CreateCheckOutPageController(ThisCustomer, cart);
                    return checkoutController.GetCheckoutPaymentPage() + "?errormsg=" + er.MessageId;
                }

                if (Math.Abs(orderTotal - Total) > 0.05M) // allow 0.05 descrepency to allow minor rounding errors
                {
                    Order.MarkOrderAsFraud(orderNumber, true);
                    DB.ExecuteSQL("update orders set FraudedOn=getdate(), IsNew=1 where OrderNumber=@OrderNumber;", sqlParams.ToArray());
                }
            }
            catch // if we failed, did the IPN come back at the same time?
            {          

                cart = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                if (cart.IsEmpty())
                {
                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using(SqlCommand cmd = new SqlCommand("select MAX(OrderNumber) N from dbo.orders where CustomerID = @CustomerId;", dbconn))
                        {
                            cmd.Parameters.Add(new SqlParameter("@CustomerId", ThisCustomer.CustomerID));
                            orderNumber = cmd.ExecuteScalar() as int? ?? 0;
                        }
                    }
                }
            }
            return "orderconfirmation.aspx?ordernumber=" + orderNumber + "&paymentmethod=CreditCard";
        }

        #region Helper Methods

        private string GetParameter(string key)
        {
            string value = "";
            try { value = Params == null ? "" : HttpUtility.UrlDecode(Params[key]); }
            catch { }

            return value;
        }

        private int GetOrderNumber()
        {
            int OrderId_Param;
            int OrderId_Session;
            int.TryParse(GetParameter("order_id"), out OrderId_Param);

            try
            {
                OrderId_Session = HttpContext.Current.Session["firstPayOrderNumber"] == null ? 0 : (int)HttpContext.Current.Session["firstPayOrderNumber"];
            }
            catch
            {
                OrderId_Session = 0;
            }

            if (OrderId_Param == OrderId_Session)
                return OrderId_Param;
            else
                return 0;
        }

        private string ReturnFirstPayError()
        {
            ErrorMessage er;
            if (!string.IsNullOrEmpty(ErrorMessage))
                er = new ErrorMessage("Transaction Error: " + HttpUtility.UrlDecode(ErrorMessage));
            else if (!string.IsNullOrEmpty(AuthResponse) && !AuthResponse.ToLower().Contains("approved"))
                er = new ErrorMessage("Transaction Declined: " + HttpUtility.UrlDecode(AuthResponse));
            else
                er = new ErrorMessage("The transaction was declined.");

            var checkoutController = CheckOutPageControllerFactory.CreateCheckOutPageController(ThisCustomer, cart);
            return checkoutController.GetCheckoutPaymentPage() + "?errormsg=" + er.MessageId;
        }

        private void ConfirmTransaction(decimal orderTotal)
        {
            if (!gateway.ConfirmTransaction(OrderNumber, orderTotal, ReferenceNumber))
            {
                FirstPay.order_id = 0;
                throw new System.Security.SecurityException("The 1stPay transaction did not exist for that order total.");
            }
        }        

        private Dictionary<string, string> GetParameterStringAsDictionary(string paramString)
        {
            Dictionary<string, string> returnedParameters = new Dictionary<string, string>();
            string[] pairs = paramString.Split('&');
            foreach (string param in pairs)
            {
                string[] kvp = param.Split('=');
                if (kvp.Length != 2)
                    continue;

                if (!returnedParameters.ContainsKey(kvp[0]))
                    returnedParameters.Add(kvp[0], (HttpContext.Current.Server.UrlDecode(kvp[1])));
            }
            return returnedParameters;
        }

        #endregion
    }

}
