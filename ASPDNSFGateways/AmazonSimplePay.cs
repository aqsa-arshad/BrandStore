// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Serialization;
using System.Security.Cryptography;
using AspDotNetStorefrontCommon;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontGateways
{
    public class AmazonSimplePay
    {
        public enum StatusTypes { A, PF, PI, PR, PS, RF, RS, SE }
        public enum Operation { Pay, Reserve, Settle, Refund, Cancel }

        public static string CreateFormPayment(ShoppingCart cart)
        {
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;           
            SortedDictionary<String, String> formHiddentInputs = new SortedDictionary<string, string>();

            decimal cartTotal = cart.Total(true);         

            if (cart.HasCoupon())
            {
                cartTotal = cartTotal - CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(cartTotal < cart.Coupon.DiscountAmount, cartTotal, cart.Coupon.DiscountAmount), 0);
            }

            string returnUrl = String.Format("{0}{1}", AppLogic.GetStoreHTTPLocation(true), "amazon_PostSale.aspx");
            string ipnUrl = String.Format("{0}{1}", AppLogic.GetStoreHTTPLocation(true), "amazoncallback.aspx");
            string abandonUrl = String.Format("{0}{1}", AppLogic.GetStoreHTTPLocation(true), "checkoutpayment.aspx");

            formHiddentInputs.Add("accessKey", AppLogic.AppConfig("AMAZON.AccessKey"));
            formHiddentInputs.Add("amount", String.Format("{0} {1}", ThisCustomer.CurrencySetting, decimal.Round(cartTotal, 2).ToString()));
            formHiddentInputs.Add("description", AppLogic.AppConfig("AMAZON.Description"));
            formHiddentInputs.Add("referenceId", String.Format("{0}:{1}", ThisCustomer.CustomerID, AppLogic.GetNextOrderNumber().ToString()));
            formHiddentInputs.Add("immediateReturn", AppLogic.AppConfigUSInt("AMAZON.ImmediateReturn").ToString());
            formHiddentInputs.Add("returnUrl", returnUrl);            
            formHiddentInputs.Add("abandonUrl", abandonUrl);
            formHiddentInputs.Add("ipnUrl", ipnUrl);
            formHiddentInputs.Add("collectShippingAddress", AppLogic.AppConfigUSInt("AMAZON.CollectShippingAddress").ToString());            
            if (AppLogic.TransactionMode() == AppLogic.ro_TXModeAuthOnly)
            {
                formHiddentInputs.Add("processImmediate", "FALSE");
            }
            else
            {
                formHiddentInputs.Add("processImmediate", "TRUE");
            }

            string signedString = GetStringToSign(formHiddentInputs);
            string signature = GetSignature(signedString, AppLogic.AppConfig("AMAZON.SecretKey"));

            formHiddentInputs.Add("signature", signature);

            return CreateHTMLForm(formHiddentInputs);

        }

        public static void ValidateShippingAddress()
        {
            if (AppLogic.AppConfigUSInt("AMAZON.CollectShippingAddress").ToString().Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                SortedDictionary<string, string> formKeys = GetCurrentRequest();
                char[] splitter = { ':' };
                string[] referenceId = formKeys["referenceId"].Split(splitter, StringSplitOptions.RemoveEmptyEntries);

                Customer ThisCustomer = new Customer(Convert.ToInt32(referenceId[0]));

                string sql = String.Format("select top 1 addressid N from dbo.address with (NOLOCK) where CustomerID = {0} and ", ThisCustomer.CustomerID);
                sql += String.Format("isnull(City, '') = {0} and ", DB.SQuote(formKeys["city"].ToString().Trim()));
                sql += String.Format("isnull(State, '') = {0} and ", DB.SQuote(formKeys["state"].ToString().Trim()));
                sql += String.Format("isnull(Zip, '') = {0} and ", DB.SQuote(formKeys["zip"].ToString().Trim()));
                sql += String.Format("isnull(Country, '') = {0}", DB.SQuote(AppLogic.GetCountryNameFromThreeLetterISOCode(formKeys["country"].ToString()).Trim()));
                sql += String.Format(" order by addressid desc");

                int addressId = DB.GetSqlN(sql);

                if (addressId > 0)
                {
                    DB.ExecuteSQL(String.Format("update dbo.ShoppingCart set ShippingAddressID = {0} where CustomerID = {1}", addressId, ThisCustomer.CustomerID));
                    DB.ExecuteSQL(String.Format("update dbo.Customer set ShippingAddressID={0} where CustomerID={1}", addressId, ThisCustomer.CustomerID));
                }
                else
                {
                    Address shippingAddress = new Address();

                    shippingAddress.CustomerID = ThisCustomer.CustomerID;
                    shippingAddress.PaymentMethodLastUsed = "";
                    shippingAddress.FirstName = formKeys["addressName"].ToString();
                    shippingAddress.LastName = "";
                    shippingAddress.Address1 = formKeys["addressLine1"].ToString();
                    if (formKeys.ContainsKey("addressLine2"))
                    {
                        shippingAddress.Address2 = formKeys["addressLine2"].ToString();
                    }
                    shippingAddress.EMail = formKeys["buyerEmail"].ToString();
                    shippingAddress.Phone = formKeys["phoneNumber"].ToString();
                    shippingAddress.City = formKeys["city"].ToString();
                    shippingAddress.State = formKeys["state"].ToString();
                    shippingAddress.Zip = formKeys["zip"].ToString();
                    shippingAddress.Country = AppLogic.GetCountryNameFromThreeLetterISOCode(formKeys["country"].ToString());
                    shippingAddress.ResidenceType = ResidenceTypes.Residential;
                    shippingAddress.InsertDB();

                    DB.ExecuteSQL(String.Format("update dbo.ShoppingCart set ShippingAddressID={0} where CustomerID={1} and CartType={2}", shippingAddress.AddressID.ToString(), ThisCustomer.CustomerID, ((int)CartTypeEnum.ShoppingCart).ToString()));
                    DB.ExecuteSQL(String.Format("update dbo.Customer set ShippingAddressID={0} where CustomerID={1}", shippingAddress.AddressID, ThisCustomer.CustomerID));
                }

            }
        }

        public static string MakeOrder()
        {
            SortedDictionary<string, string> formKeys = GetCurrentRequest();
            char[] splitter = { ':' };
            string[] referenceId = formKeys["referenceId"].Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            Customer ThisCustomer = new Customer(Convert.ToInt32(referenceId[0]));
            int orderNumber = 0;
            ShoppingCart cart = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            string status = string.Empty;
            if (Int32.TryParse(referenceId[1], out orderNumber))
            {
                status = Gateway.MakeOrder(Gateway.ro_GWAMAZONSIMPLEPAY, AppLogic.TransactionMode(), cart, orderNumber, string.Empty, string.Empty, string.Empty, string.Empty);                
            }

            return status;
        }

        public static bool CurrentRequestIsCallBack()
        {
            return HttpContext.Current.Request.RequestType.EqualsIgnoreCase("post");
        }

        public static void UpdateOrder(int orderNumber, string operation)
        {
            if (CurrentRequestIsCallBack())
            {
                Order ord = new Order(orderNumber);
                if (operation.Equals("pay"))
                {
                    if (!ord.TransactionIsCaptured())
                    {
                        Gateway.DispatchCapture(Gateway.ro_GWAMAZONSIMPLEPAY, orderNumber);
                    }
                }
                else if (operation.Equals("reserve"))
                {
                    if (!ord.TransactionIsAuth())
                    {
                        MakeForceAuthorize(orderNumber);
                    }
                }
            }
            else
            {
                HttpContext.Current.Response.Redirect(String.Format("orderconfirmation.aspx?ordernumber={0}", orderNumber));
            }
        }

        public static string ProcessCard(int orderNumber, int customerID, decimal orderTotal, bool useLiveTransactions, string transactionMode, Address useBillingAddress, string cardExtraCode, Address useShippingAddress, string cavv, string eci, string transactionID, out string avsResult, out string authorizationResult, out string authorizationCode, out string authorizationTransID, out string transactionCommandOut, out string transactionResponse)
        {
            SortedDictionary<string, string> results = GetCurrentRequest();
            Customer ThisCustomer = new Customer(customerID);
            string status = string.Empty;
            avsResult = "N/A";
            authorizationResult = "N/A";
            authorizationCode = "N/A";

            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (KeyValuePair<string, string> list in results)
            {
                if (!first)
                {
                    sb.Append("&");
                }
                if (list.Key.Equals("signature", StringComparison.InvariantCultureIgnoreCase))
                {
                    string mungeString = list.Value.Replace(list.Value, "*".PadLeft(list.Value.Length));
                    sb.AppendFormat("{0}={1}", list.Key, mungeString);
                }
                else
                {                    
                    sb.AppendFormat("{0}={1}", list.Key, list.Value);
                }
                first = false;
            }

            transactionCommandOut = sb.ToString();
            transactionResponse = results["operation"].ToString();
            authorizationTransID = results["transactionId"].ToString();

            StatusTypes statTypes = (StatusTypes)Enum.Parse(typeof(StatusTypes), results["status"].ToString(), true);

            switch (statTypes)
            {
                case StatusTypes.PS:
                case StatusTypes.PI:
                case StatusTypes.PR:
                    status = AppLogic.ro_OK;
                    break;
                case StatusTypes.A:
                    status = AppLogic.GetString("AmazonSimplePay.Status.A", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    break;
                case StatusTypes.PF:
                    status = AppLogic.GetString("AmazonSimplePay.Status.PF", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    break;
                case StatusTypes.SE:
                    status = AppLogic.GetString("AmazonSimplePay.Status.SE", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    break;
                default:
                    break;
            }

            return status;
        }

        public static void MakeForceTransaction(string transType)
        {
            SortedDictionary<string, string> formKeys = GetCurrentRequest();
            char[] splitter = { ':' };
            string[] referenceId = formKeys["referenceId"].Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            Operation ops = (Operation)Enum.Parse(typeof(Operation), transType);

            string custId = referenceId[0];
            if (ops == Operation.Refund)
            {
                custId = custId.ToLower().Replace("refund for", "").Trim();
            }
            if (!CommonLogic.IsStringNullOrEmpty(custId))
            {
                int orderNumber = 0;
                if (int.TryParse(referenceId[1], out orderNumber))
                {
                    if (Order.OrderExists(orderNumber))
                    {
                        switch (ops)
                        {
                            case Operation.Settle:
                                Gateway.ForceCapture(orderNumber);
                                break;
                            case Operation.Refund:
                                Gateway.ForceRefundStatus(orderNumber);
                                break;
                            case Operation.Cancel:
                                Gateway.ForceVoidStatus(orderNumber);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        public static string CaptureOrder(Order o)
        {           
            AmazonFPS.Request request = new AmazonFPS.Request();
            string status = string.Empty;

            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                request.url = AppLogic.AppConfig("AMAZON.FPS_LiveServer");
            }
            else
            {
                request.url = AppLogic.AppConfig("AMAZON.FPS_TestServer");
            }

            request.ord = o;
            request.accessKey = AppLogic.AppConfig("AMAZON.AccessKey");
            request.type = AmazonFPS.Request.ActionType.Settle;
            request.secretKey = AppLogic.AppConfig("AMAZON.SecretKey");
            request.timeStamp = string.Concat(DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture), "Z");

            string response = CreateWebRequest(request);
            if (response.Contains("<Errors"))
            {
                status = GetErrorMessages(response);
            }
            else
            {
                AmazonFPS.ResponseDeserialization.SettleResponse deserializedResponse =
                    (AmazonFPS.ResponseDeserialization.SettleResponse)DeserializeResponse(response, typeof(AmazonFPS.ResponseDeserialization.SettleResponse));

                if (deserializedResponse.settle.tranStatus == AmazonFPS.ResponseDeserialization.TransactionStatus.Pending ||
                    deserializedResponse.settle.tranStatus == AmazonFPS.ResponseDeserialization.TransactionStatus.Success)
                {
                    AmazonFPS.ResponseDeserialization.TransactionStatus tranStatus = deserializedResponse.settle.tranStatus;

                    StringBuilder txResult = new StringBuilder();
                    txResult.AppendFormat("TransactionId={0}", deserializedResponse.settle.tranId);
                    txResult.AppendFormat("&TransactionStatus={0}", deserializedResponse.settle.tranStatus);
                    txResult.AppendFormat("&RequestId={0}", deserializedResponse.metadata.reqId);
                    o.CaptureTXResult = txResult.ToString();

                    StringBuilder txCommand = new StringBuilder();
                    txCommand.AppendFormat("Action={0}", tranStatus.ToString());
                    txCommand.AppendFormat("&AWSAccessKeyId={0}", request.accessKey);
                    txCommand.AppendFormat("&ReserveTransactionId={0}", request.ord.AuthorizationPNREF);
                    txCommand.AppendFormat("&SignatureVersion={0}", request.signatureVersion);
                    txCommand.AppendFormat("&TimeStamp={0}", request.timeStamp);
                    txCommand.AppendFormat("&Version={0}", request.schemaVersion);
                    o.CaptureTXCommand = txCommand.ToString();

                    DB.ExecuteSQL(String.Format("update orders set CaptureTXCommand={0}, CaptureTXResult={1} where OrderNumber={2}", DB.SQuote(txCommand.ToString()), DB.SQuote(txResult.ToString()), o.OrderNumber));
                    
                    status = AppLogic.ro_OK;
                }
            }
            
            return status;
        }

        public static string VoidOrder(int orderNumber)
        {
            string transactionID = string.Empty, transactionState = string.Empty, status = string.Empty;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(String.Format("SELECT AuthorizationPNREF, TransactionState FROM Orders   with (NOLOCK)  where OrderNumber={0}", orderNumber.ToString()), conn))
                {
                    if (rs.Read())
                    {
                        transactionState = DB.RSField(rs, "TransactionState");
                        transactionID = DB.RSField(rs, "AuthorizationPNREF");
                    }
                }
            }

            if (transactionState.Equals("CAPTURED", StringComparison.InvariantCultureIgnoreCase))
            {
                return AppLogic.GetString("AmazonSimplePay.VoidedCaptureError", Customer.Current.SkinID, Customer.Current.LocaleSetting);
            }

            AmazonFPS.Request request = new AmazonFPS.Request();

            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                request.url = AppLogic.AppConfig("AMAZON.FPS_LiveServer");
            }
            else
            {
                request.url = AppLogic.AppConfig("AMAZON.FPS_TestServer");
            }

            request.type = AmazonFPS.Request.ActionType.Cancel;
            request.accessKey = AppLogic.AppConfig("AMAZON.AccessKey");
            request.secretKey = AppLogic.AppConfig("AMAZON.SecretKey");
            request.timeStamp = string.Concat(DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture), "Z");
            request.ord = new Order(orderNumber);

            string response = CreateWebRequest(request);
            if (response.Contains("<Errors"))
            {
                status = GetErrorMessages(response);
            }
            else
            {
                AmazonFPS.ResponseDeserialization.CancelResponse deserializedResponse =
                    (AmazonFPS.ResponseDeserialization.CancelResponse)DeserializeResponse(response, typeof(AmazonFPS.ResponseDeserialization.CancelResponse));

                if (deserializedResponse.cancel.tranStatus == AmazonFPS.ResponseDeserialization.TransactionStatus.Pending ||
                    deserializedResponse.cancel.tranStatus == AmazonFPS.ResponseDeserialization.TransactionStatus.Cancelled)
                {
                    AmazonFPS.ResponseDeserialization.TransactionStatus tranStatus = deserializedResponse.cancel.tranStatus;

                    StringBuilder txResult = new StringBuilder();
                    txResult.AppendFormat("TransactionId={0}", deserializedResponse.cancel.tranId);
                    txResult.AppendFormat("&TransactionStatus={0}", deserializedResponse.cancel.tranStatus);
                    txResult.AppendFormat("&RequestId={0}", deserializedResponse.metadata.reqId);

                    StringBuilder txCommand = new StringBuilder();
                    txCommand.AppendFormat("Action={0}", tranStatus.ToString());
                    txCommand.AppendFormat("&AWSAccessKeyId={0}", request.accessKey);
                    txCommand.AppendFormat("&TransactionId={0}", request.ord.AuthorizationPNREF);
                    txCommand.AppendFormat("&SignatureVersion={0}", request.signatureVersion);
                    txCommand.AppendFormat("&TimeStamp={0}", request.timeStamp);
                    txCommand.AppendFormat("&Version={0}", request.schemaVersion);

                    DB.ExecuteSQL(String.Format("update orders set VoidTXCommand={0}, VoidTXResult={1} where OrderNumber={2}", DB.SQuote(txCommand.ToString()), DB.SQuote(txResult.ToString()), orderNumber));

                    status = AppLogic.ro_OK;
                }
            }

            return status;
        }

        public static string RefundOrder(int originalOrderNumber, int newOrderNumber, decimal refundAmount, string refundReason, Address useBillingAddress)
        {
            string transactionId = string.Empty, status = string.Empty;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(String.Format("SELECT AuthorizationPNREF, OrderTotal FROM Orders   with (NOLOCK)  where OrderNumber={0}", originalOrderNumber.ToString()), conn))
                {
                    if (rs.Read())
                    {
                        transactionId = DB.RSField(rs, "AuthorizationPNREF");

                        if (refundAmount == 0M)
                        {
                            refundAmount = DB.RSFieldDecimal(rs, "OrderTotal");
                        }
                    }
                }
            }

            AmazonFPS.Request request = new AmazonFPS.Request();

            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                request.url = AppLogic.AppConfig("AMAZON.FPS_LiveServer");
            }
            else
            {
                request.url = AppLogic.AppConfig("AMAZON.FPS_TestServer");
            }

            request.type = AmazonFPS.Request.ActionType.Refund;
            request.accessKey = AppLogic.AppConfig("AMAZON.AccessKey");
            request.secretKey = AppLogic.AppConfig("AMAZON.SecretKey");
            request.timeStamp = string.Concat(DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture), "Z");
            request.ord = new Order(originalOrderNumber);
            request.description = refundReason;

            string response = CreateWebRequest(request);
            if (response.Contains("<Errors"))
            {
                status = GetErrorMessages(response);
            }
            else
            {
                AmazonFPS.ResponseDeserialization.RefundResponse deserializedResponse =
                    (AmazonFPS.ResponseDeserialization.RefundResponse)DeserializeResponse(response, typeof(AmazonFPS.ResponseDeserialization.RefundResponse));

                if (deserializedResponse.refund.tranStatus == AmazonFPS.ResponseDeserialization.TransactionStatus.Pending ||
                    deserializedResponse.refund.tranStatus == AmazonFPS.ResponseDeserialization.TransactionStatus.Success)
                {
                    AmazonFPS.ResponseDeserialization.TransactionStatus tranStatus = deserializedResponse.refund.tranStatus;

                    StringBuilder txResult = new StringBuilder();
                    txResult.AppendFormat("TransactionId={0}", deserializedResponse.refund.tranId);
                    txResult.AppendFormat("&TransactionStatus={0}", deserializedResponse.refund.tranStatus);
                    txResult.AppendFormat("&RequestId={0}", deserializedResponse.metadata.reqId);

                    StringBuilder txCommand = new StringBuilder();
                    txCommand.AppendFormat("Action={0}", tranStatus.ToString());
                    txCommand.AppendFormat("&AWSAccessKeyId={0}", request.accessKey);
                    txCommand.AppendFormat("&CallerDescription={0}", request.GetCallerDescription());
                    txCommand.AppendFormat("&CallerReference={0}", request.GetCallerReference());
                    txCommand.AppendFormat("&TransactionId={0}", request.ord.AuthorizationPNREF);
                    txCommand.AppendFormat("&SignatureVersion={0}", request.signatureVersion);
                    txCommand.AppendFormat("&TimeStamp={0}", request.timeStamp);
                    txCommand.AppendFormat("&Version={0}", request.schemaVersion);

                    DB.ExecuteSQL(String.Format("update orders set RefundTXCommand={0}, RefundTXResult={1} where OrderNumber={2}", DB.SQuote(txCommand.ToString()), DB.SQuote(txResult.ToString()), originalOrderNumber.ToString()));

                    status = AppLogic.ro_OK;
                }
            }

            return status;
        }

        public static string UpdateTransaction(Order o)
        {
            string status = AppLogic.ro_OK;
            AmazonFPS.Request request = new AmazonFPS.Request();

            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                request.url = AppLogic.AppConfig("AMAZON.FPS_LiveServer");
            }
            else
            {
                request.url = AppLogic.AppConfig("AMAZON.FPS_TestServer");
            }

            request.type = AmazonFPS.Request.ActionType.GetTransactionStatus;
            request.accessKey = AppLogic.AppConfig("AMAZON.AccessKey");
            request.secretKey = AppLogic.AppConfig("AMAZON.SecretKey");
            request.timeStamp = string.Concat(DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture), "Z");
            request.ord = o;

            string response = CreateWebRequest(request);

            if (response.Contains("<Errors"))
            {
                status = GetErrorMessages(response);
            }
            else
            {
                AmazonFPS.ResponseDeserialization.UpdateTransaction deserializedResponse =
                    (AmazonFPS.ResponseDeserialization.UpdateTransaction)DeserializeResponse(response, typeof(AmazonFPS.ResponseDeserialization.UpdateTransaction));

                switch (deserializedResponse.transResult.StatusCode)
                {
                    case AmazonFPS.ResponseDeserialization.StatusCodes.Success:
                        {
                            if ((o.TransactionState.Equals(AppLogic.ro_TXStateAuthorized) || o.TransactionState.Equals(AppLogic.ro_TXStatePending)) &&
                                deserializedResponse.transResult.TransactionStatus == AmazonFPS.ResponseDeserialization.TransactionStatus.Success)
                            {
                                status = Gateway.ForceCapture(o.OrderNumber);
                            }
                            else if (o.TransactionState.Equals(AppLogic.ro_TXStatePending) &&
                                deserializedResponse.transResult.TransactionStatus == AmazonFPS.ResponseDeserialization.TransactionStatus.Reserved)
                            {
                                MakeForceAuthorize(o.OrderNumber);
                            }

                            break;
                        }
                    case AmazonFPS.ResponseDeserialization.StatusCodes.Cancelled:
                        {
                            status = "Order Transaction Updated";
                            if (o.TransactionState.Equals("AUTHORIZED") &&
                                deserializedResponse.transResult.TransactionStatus == AmazonFPS.ResponseDeserialization.TransactionStatus.Cancelled)
                            {
                                status = Gateway.ForceVoidStatus(o.OrderNumber);
                            }
                            break;
                        }
                    case AmazonFPS.ResponseDeserialization.StatusCodes.PendingVerification:
                        {
                            status = "Order is still in pending status";
                            break;
                        }
                    case AmazonFPS.ResponseDeserialization.StatusCodes.PendingNetworkResponse:
                        {
                            status = "Order is still in pending network response";
                            break;
                        }
                    default:
                        break;
                }
            }

            return status;
        }

        public static bool IsSignatureValid()
        {
            SortedDictionary<string, string> formKeys = GetCurrentRequest();
            string suppliedSignature = "";

            if (formKeys.TryGetValue("signature", out suppliedSignature))
            {
                formKeys.Remove("signature");
                string signedString = GetStringToSign(formKeys);
                string signature = GetSignature(signedString, AppLogic.AppConfig("AMAZON.SecretKey"));

                return signature.Equals(suppliedSignature);
            }

            return false;
        }

        public static string GetStringToSign(SortedDictionary<string, string> formHiddentInputs)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> list in formHiddentInputs)
            {
                sb.AppendFormat("{0}{1}", list.Key, list.Value);
            }

            return sb.ToString();
        }

        public static string GetSignature(string signedString, string secretKey)
        {
            HMACSHA1 mac = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey));
            byte[] data = mac.ComputeHash(Encoding.UTF8.GetBytes(signedString));

            return Convert.ToBase64String(data);
        }

        private static void MakeForceAuthorize(int orderNumber)
        {
            DB.ExecuteSQL(String.Format("update orders set transactionstate={0} where ordernumber={1}", DB.SQuote(AppLogic.ro_TXStateAuthorized), orderNumber));
        }

        private static string GetErrorMessages(string response)
        {
            AmazonFPS.ResponseDeserialization.ErrorResponse errorResponse =
                    (AmazonFPS.ResponseDeserialization.ErrorResponse)DeserializeResponse(response, typeof(AmazonFPS.ResponseDeserialization.ErrorResponse));

            StringBuilder sb = new StringBuilder();
            foreach (AmazonFPS.ResponseDeserialization.Error er in errorResponse.errors)
            {
                sb.Append(er.errormessage);
            }

            return sb.ToString();
        }

        private static string CreateWebRequest(AmazonFPS.Request request)
        {
            string response = string.Empty;
            Uri url = new Uri(request.ToString());
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = WebRequestMethods.Http.Post;            

            try
            {
                WebResponse webResponse = webRequest.GetResponse();
                using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    response = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch (WebException e)
            {
                WebResponse webException = e.Response;
                using (StreamReader sr = new StreamReader(webException.GetResponseStream()))
                {
                    response = sr.ReadToEnd();
                    sr.Close();
                }
            }

            return response;
        }

        private static object DeserializeResponse(string xmlData, Type oType)
        {
            XmlSerializer serRequest = new XmlSerializer(oType);
            Object oRequest;

            using (StringReader strReader = new StringReader(xmlData))
            {
                oRequest = serRequest.Deserialize(strReader);
                strReader.Close();
            }

            return oRequest;
        }

        private static SortedDictionary<string, string> GetCurrentRequest()
        {
            SortedDictionary<string, string> formKeys = new SortedDictionary<string, string>();
            int size =
                CommonLogic.IIF(CurrentRequestIsCallBack(), HttpContext.Current.Request.Form.AllKeys.Length, HttpContext.Current.Request.QueryString.AllKeys.Length);

            string[] allKeys = new string[size];

            if (CurrentRequestIsCallBack())
            {
                allKeys = HttpContext.Current.Request.Form.AllKeys;
            }
            else
            {
                allKeys = HttpContext.Current.Request.QueryString.AllKeys;
            }

            foreach (string key in allKeys)
            {
                string value = CommonLogic.IIF(CurrentRequestIsCallBack(), 
                                                HttpUtility.HtmlDecode(CommonLogic.FormCanBeDangerousContent(key)), 
                                                HttpUtility.UrlDecode(CommonLogic.QueryStringCanBeDangerousContent(key)));
                                                formKeys.Add(key, value);
            }

            return formKeys;
        }

        private static string CreateHTMLForm(SortedDictionary<string, string> formHiddentInputs)
        {
            StringBuilder form = new StringBuilder();
            string url = string.Empty;

            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                url = AppLogic.AppConfig("AMAZON.LiveServer");
            }
            else
            {
                url = AppLogic.AppConfig("AMAZON.TestServer");
            }

            form.AppendFormat("<form id=\"AmazonSimplePay\" name=\"AmazonSimplePay\" target=\"_top\" action=\"{0}\" method=\"post\">\n", url);

            foreach (KeyValuePair<string, string> list in formHiddentInputs)
            {
                form.AppendFormat("<input type=\"hidden\" name=\"{0}\" value=\"{1}\"/>\n", list.Key, list.Value);
            }

            form.Append("</form>");

            return form.ToString();

        }

    }

    namespace AmazonFPS
    {
        public class BaseRequest : IConvertible
        {
            public virtual string ToString(IFormatProvider provider)
            {
                return "";
            }

            public TypeCode GetTypeCode()
            {
                throw new NotImplementedException();
            }

            public bool ToBoolean(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public byte ToByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public char ToChar(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public DateTime ToDateTime(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public decimal ToDecimal(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public double ToDouble(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public short ToInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public int ToInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public long ToInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            [CLSCompliant(false)]
            public sbyte ToSByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public float ToSingle(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public object ToType(Type conversionType, IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            [CLSCompliant(false)]
            public ushort ToUInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            [CLSCompliant(false)]
            public uint ToUInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            [CLSCompliant(false)]
            public ulong ToUInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }
        }

        public partial class Request : BaseRequest
        {
            internal enum ActionType
            {
                GetTransactionStatus,
                Settle,
                Refund,
                Cancel
            }

            internal string url;
            internal ActionType type;
            internal string accessKey;
            internal int signatureVersion = 1;
            internal Order ord;
            internal string schemaVersion = "2008-09-17";
            internal string timeStamp;
            internal string secretKey;
            internal string description = string.Empty;

            private string GetSignedUrl()
            {
                SortedDictionary<string, string> urlKeys = new SortedDictionary<string, string>();

                //Default Request Parameters
                urlKeys.Add("Action", type.ToString());
                urlKeys.Add("AWSAccessKeyId", accessKey);
                urlKeys.Add("SignatureVersion", signatureVersion.ToString());
                urlKeys.Add("Timestamp", timeStamp);
                urlKeys.Add("Version", schemaVersion);

                if (type == ActionType.Settle)
                {
                    urlKeys.Add("ReserveTransactionId", ord.AuthorizationPNREF);
                }
                else
                {
                    urlKeys.Add("TransactionId", ord.AuthorizationPNREF);
                }

                if (type == ActionType.Refund)
                {
                    urlKeys.Add("CallerDescription", GetCallerDescription());
                    urlKeys.Add("CallerReference", GetCallerReference());
                }

                return AmazonSimplePay.GetStringToSign(urlKeys);
            }

            internal string GetCallerDescription()
            {
                if (description.Length >= 160)
                {
                    return description.Substring(0, 150) + "...";
                }
                return description;
            }

            internal string GetCallerReference()
            {
                int customerId = ord.CustomerID, orderNumber = ord.OrderNumber;
                return String.Format("{0}:{1}", customerId.ToString(), orderNumber.ToString());
            }

            public override string ToString()
            {
                string signature = AmazonSimplePay.GetSignature(GetSignedUrl(), secretKey);

                return url +
                    "?Action=" + type.ToString() +
                    "&AWSAccessKeyId=" + accessKey +
                    "&Signature=" + signature +
                    "&SignatureVersion=" + signatureVersion.ToString() +
                    "&Timestamp=" + timeStamp +
                    "&Version=" + schemaVersion +
                    CommonLogic.IIF(type == ActionType.Settle, "&ReserveTransactionId=", "&TransactionId=") + ord.AuthorizationPNREF +
                    CommonLogic.IIF(type == ActionType.Refund, String.Format("&CallerReference={0}", GetCallerReference()), "") +
                    CommonLogic.IIF(type == ActionType.Refund, String.Format("&CallerDescription={0}", GetCallerDescription()), "");

            }
        }

        namespace ResponseDeserialization
        {
            public enum TransactionStatus { Reserved, Success, Failure, Pending, Cancelled };
            public enum StatusCodes { Cancelled, Expired, PendingNetworkResponse, PendingVerification, Success, TransactionDenied }

            [XmlRoot(ElementName = "GetTransactionStatusResponse", Namespace = "http://fps.amazonaws.com/doc/2008-09-17/")]
            public class UpdateTransaction
            {
                [XmlElement("GetTransactionStatusResult")]
                public GetTransactionStatusResult transResult { get; set; }

                [XmlElement("ResponseMetadata")]
                public ResponseMetadata metaData { get; set; }
            }

            public class GetTransactionStatusResult
            {
                [XmlElement("TransactionId")]
                public string TransactionId { get; set; }

                [XmlElement("TransactionStatus")]
                public TransactionStatus TransactionStatus { get; set; }

                [XmlElement("CallerReference")]
                public string CallerReference { get; set; }

                [XmlElement("StatusCode")]
                public StatusCodes StatusCode { get; set; }

                [XmlElement("StatusMessage")]
                public string StatusMessage { get; set; }
            }

            [XmlRoot(ElementName = "SettleResponse", Namespace = "http://fps.amazonaws.com/doc/2008-09-17/")]
            public class SettleResponse
            {
                [XmlElement("SettleResult")]
                public Result settle { get; set; }

                [XmlElement("ResponseMetadata")]
                public ResponseMetadata metadata { get; set; }
            }

            [XmlRoot(ElementName = "CancelResponse", Namespace = "http://fps.amazonaws.com/doc/2008-09-17/")]
            public class CancelResponse
            {

                [XmlElement("CancelResult")]
                public Result cancel { get; set; }

                [XmlElement("ResponseMetadata")]
                public ResponseMetadata metadata { get; set; }
            }

            [XmlRoot(ElementName = "RefundResponse", Namespace = "http://fps.amazonaws.com/doc/2008-09-17/")]
            public class RefundResponse
            {

                [XmlElement("RefundResult")]
                public Result refund { get; set; }

                [XmlElement("ResponseMetadata")]
                public ResponseMetadata metadata { get; set; }
            }

            public class Result
            {
                [XmlElement("TransactionId")]
                public string tranId { get; set; }

                [XmlElement("TransactionStatus")]
                public TransactionStatus tranStatus { get; set; }
            }

            public class ResponseMetadata
            {
                [XmlElement("RequestId")]
                public string reqId { get; set; }
            }

            [XmlRoot(ElementName = "Response")]
            public class ErrorResponse
            {
                [XmlArray("Errors")]
                [XmlArrayItem("Error", typeof(Error))]
                public Error[] errors { get; set; }

                [XmlElement("RequestId")]
                public string reqId { get; set; }
            }

            public class Error
            {
                [XmlElement("Code")]
                public string errorcode { get; set; }

                [XmlElement("Message")]
                public string errormessage { get; set; }
            }


        }
    }

}


