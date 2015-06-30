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
using System.Xml.Serialization;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontGateways.Processors
{
    public class PlugNPay : GatewayProcessor
    {
        public PlugNPay()
        { }

        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            string result = string.Empty;
            AuthorizationCode = string.Empty;
            AuthorizationResult = string.Empty;
            AuthorizationTransID = string.Empty;
            AVSResult = string.Empty;
            TransactionCommandOut = string.Empty;
            TransactionResponse = string.Empty;

            // create payment request
            var request = CreatePaymentRequest(OrderNumber, OrderTotal, Localization.StoreCurrency(), UseBillingAddress);

            // request details
            var details = request.Request.TransactionRequest;
            details.Mode = PlugNPayApi.RequestMode.Auth;
            details.AuthType = AppLogic.TransactionModeIsAuthOnly() ? PlugNPayApi.AuthType.AuthOnly : PlugNPayApi.AuthType.AuthPostAuth;

            // 3D Secure details
            if (CAVV.Length > 0) details.cavv = CAVV;
            if (ECI.Length > 0) details.eci = ECI;
            if (XID.Length > 0) details.xid = XID;

            // payment information
            var payment = details.PaymentDetails;
            payment.PaymentMethod = PlugNPayApi.PaymentMethod.Credit;
            payment.CardNumber = UseBillingAddress.CardNumber;
            payment.CardExp = UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + "/" + UseBillingAddress.CardExpirationYear.Substring(2, 2);
            payment.CardCVV = CardExtraCode;

            // transmit request to PlugNPay and receive response
            PlugNPayApi.PNP response;

            try
            {
                response = TransmitRequest(request, out TransactionCommandOut);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            // examine the response
            var responseDetails = response.Response.TransactionResponse;
            AuthorizationResult = responseDetails.FinalStatus;
            AuthorizationTransID = responseDetails.TransactionID;
            AuthorizationCode = responseDetails.AuthCode;
            AVSResult = responseDetails.AVSResp;

            if (AuthorizationResult == PlugNPayApi.ResponseFinalStatus.Success)
            {
                result = AppLogic.ro_OK;
            }
            else
            {
                result = responseDetails.MErrMsg;
                if (result == null || result.Length == 0) result = "Unspecified Error";

                result = result.Replace("account", "card");
                result = result.Replace("Account", "Card");
                result = result.Replace("ACCOUNT", "CARD");
            }

            return result;
        }

        public override String ProcessECheck(int OrderNumber, int CustomerID, Decimal OrderTotal, Address UseBillingAddress, Address UseShippingAddress, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            string result = string.Empty;
            AuthorizationCode = string.Empty;
            AuthorizationResult = string.Empty;
            AuthorizationTransID = string.Empty;
            AVSResult = string.Empty;
            TransactionCommandOut = string.Empty;
            TransactionResponse = string.Empty;

            // create payment request
            var request = CreatePaymentRequest(OrderNumber, OrderTotal, Localization.StoreCurrency(), UseBillingAddress);

            // payment information
            var payment = request.Request.TransactionRequest.PaymentDetails;
            payment.PaymentMethod = PlugNPayApi.PaymentMethod.OnlineCheck;
            payment.RoutingNum = UseBillingAddress.ECheckBankABACode;
            payment.AccountNum = UseBillingAddress.ECheckBankAccountNumber;
            payment.AcctClass = UseBillingAddress.ECheckBankAccountType == "BUSINESS CHECKING" ? PlugNPayApi.AcctClass.Business : PlugNPayApi.AcctClass.Personal;
            payment.AcctType = UseBillingAddress.ECheckBankAccountType == "SAVINGS" ? PlugNPayApi.AcctType.Savings : PlugNPayApi.AcctType.Checking;
            payment.CheckType = PlugNPayApi.CheckType.ViaTheInternet;

            // transmit request to PlugNPay and receive response
            PlugNPayApi.PNP response;

            try
            {
                response = TransmitRequest(request, out TransactionCommandOut);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            // examine the response
            var responseDetails = response.Response.TransactionResponse;
            AuthorizationResult = responseDetails.FinalStatus;
            AuthorizationTransID = responseDetails.TransactionID;
            AuthorizationCode = responseDetails.AuthCode;
            AVSResult = responseDetails.AVSResp;

            if (AuthorizationResult == PlugNPayApi.ResponseFinalStatus.Success)
            {
                result = AppLogic.ro_OK;
            }
            else
            {
                result = responseDetails.MErrMsg;
                if (result == null || result.Length == 0) result = "Unspecified Error";

                result = result.Replace("account", "card");
                result = result.Replace("Account", "Card");
                result = result.Replace("ACCOUNT", "CARD");
            }

            return result;
        }

        public override String CaptureOrder(Order o)
        {
            string result = string.Empty;

            // create payment request
            var request = new PlugNPayApi.PNP();

            // request details
            var details = request.Request.TransactionRequest;
            details.Mode = PlugNPayApi.RequestMode.Mark;
            details.TransactionID = o.OrderNumber.ToString();
            details.Order.CardAmount = Localization.CurrencyStringForGatewayWithoutExchangeRate(o.Total(true));
            details.Order.Currency = Localization.StoreCurrency();

            // transmit request to PlugNPay and receive response
            PlugNPayApi.PNP response;
            string transactionCommandOut = string.Empty;

            try
            {
                response = TransmitRequest(request, out transactionCommandOut);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            // examine the response
            var responseDetails = response.Response.TransactionResponse;

            if (responseDetails.FinalStatus == PlugNPayApi.ResponseFinalStatus.Pending || responseDetails.FinalStatus == PlugNPayApi.ResponseFinalStatus.Success)
            {
                result = AppLogic.ro_OK;
            }
            else
            {
                result = responseDetails.MErrMsg;
                if (result == null || result.Length == 0) result = "Unspecified Error";
            }

            return result;
        }

        public override String RefundOrder(int originalOrderNumber, int newOrderNumber, decimal refundAmount, string refundReason, Address billingAddress)
        {
            string result = string.Empty;

            // create payment request
            var request = new PlugNPayApi.PNP();

            // request details
            var details = request.Request.TransactionRequest;
            details.Mode = PlugNPayApi.RequestMode.Return;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF, OrderTotal from orders with (NOLOCK) where OrderNumber = " + originalOrderNumber.ToString(), dbconn))
                {
                    rs.Read();
                    details.TransactionID = DB.RSField(rs, "AuthorizationPNREF");
                    details.Order.CardAmount = Localization.CurrencyStringForGatewayWithoutExchangeRate(refundAmount == 0 ? DB.RSFieldDecimal(rs, "OrderTotal") : refundAmount);
                    details.Order.Currency = Localization.StoreCurrency();
                }
            }

            // transmit request to PlugNPay and receive response
            PlugNPayApi.PNP response;
            string transactionCommandOut = string.Empty;

            try
            {
                response = TransmitRequest(request, out transactionCommandOut);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            // examine the response
            var responseDetails = response.Response.TransactionResponse;

            if (responseDetails.FinalStatus == PlugNPayApi.ResponseFinalStatus.Success)
            {
                result = AppLogic.ro_OK;
            }
            else
            {
                result = responseDetails.MErrMsg;
                if (result == null || result.Length == 0) result = "Unspecified Error";
            }

            return result;
        }

        public override String VoidOrder(int orderNumber)
        {
            string result = string.Empty;

            // create payment request
            var request = new PlugNPayApi.PNP();

            // request details
            var details = request.Request.TransactionRequest;
            details.Mode = PlugNPayApi.RequestMode.Void;
            details.TxnType = PlugNPayApi.TxnType.Auth;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF, OrderTotal from orders with (NOLOCK) where OrderNumber = " + orderNumber.ToString(), dbconn))
                {
                    rs.Read();
                    details.TransactionID = DB.RSField(rs, "AuthorizationPNREF");
                    details.Order.CardAmount = Localization.CurrencyStringForGatewayWithoutExchangeRate(DB.RSFieldDecimal(rs, "OrderTotal"));
                    details.Order.Currency = Localization.StoreCurrency();
                }
            }

            // transmit request to PlugNPay and receive response
            PlugNPayApi.PNP response;
            string transactionCommandOut = string.Empty;

            try
            {
                response = TransmitRequest(request, out transactionCommandOut);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            // examine the response
            var responseDetails = response.Response.TransactionResponse;

            if (responseDetails.FinalStatus == PlugNPayApi.ResponseFinalStatus.Success)
            {
                result = AppLogic.ro_OK;
            }
            else
            {
                result = responseDetails.MErrMsg;
                if (result == null || result.Length == 0) result = "Unspecified Error";
            }

            return result;
        }

        static private PlugNPayApi.PNP CreatePaymentRequest(int orderNumber, decimal orderTotal, string currencyCode, Address billingAddress)
        {
            // instantiate new request object
            var request = new PlugNPayApi.PNP();
            var details = request.Request.TransactionRequest;
            var billing = details.BillDetails;
            var order = details.Order;

            // request details
            details.TransactionID = orderNumber.ToString();
            details.IPaddress = CommonLogic.CustomerIpAddress();

            // billing information
            billing.CardName = billingAddress.CardName;
            billing.CardAddress1 = billingAddress.Address1;
            billing.CardAddress2 = billingAddress.Address2;
            billing.CardCity = billingAddress.City;
            billing.CardState = billingAddress.State;
            billing.CardZip = billingAddress.Zip;
            billing.CardCountry = AppLogic.GetCountryTwoLetterISOCode(billingAddress.Country);
            billing.Email = billingAddress.EMail;

            // order information
            order.CardAmount = Localization.CurrencyStringForGatewayWithoutExchangeRate(orderTotal);
            order.Currency = currencyCode;

            return request;
        }

        static private PlugNPayApi.PNP TransmitRequest(PlugNPayApi.PNP request, out string req)
        {
            string plugNPayServer = "https://pay1.plugnpay.com/payment/xml.cgi";
            PlugNPayApi.PNP response;

            // serialize Request
            XmlSerializer serRequest = new XmlSerializer(typeof(PlugNPayApi.PNP));
            StringWriter swRequest = new StringWriter();

            serRequest.Serialize(swRequest, request);
            req = swRequest.ToString();

            // Send request to PlugNPay
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(plugNPayServer);
            webRequest.Method = "POST";

            // Transmit the request to PlugNPay
            byte[] data = System.Text.Encoding.ASCII.GetBytes(req);
            webRequest.ContentLength = data.Length;
            Stream requestStream;

            try
            {
                requestStream = webRequest.GetRequestStream();
            }
            catch (WebException e)  // could not connect to PlugNPay endpoint
            {
                throw new Exception("Tried to reach PlugNPay Server (" + plugNPayServer + "): " + e.Message);
            }

            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            // get the response from PlugNPay
            WebResponse webResponse = null;
            string resp;
            try
            {
                webResponse = webRequest.GetResponse();
            }
            catch (WebException e)  // could not receive a response from PlugNPay endpoint
            {
                throw new Exception("No response from PlugNPay Server (" + plugNPayServer + "): " + e.Message);
            }

            using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
            {
                resp = sr.ReadToEnd();
                sr.Close();
            }
            webResponse.Close();

            // check for non-Xml response (indicates a problem)
            if (!resp.StartsWith("<")) throw new Exception("PlugNPay server returned this message: " + resp.Replace('\n', ' '));

            // deserialize the xml response into a Response object
            XmlSerializer serResponse = new XmlSerializer(typeof(PlugNPayApi.PNP));
            StringReader srResponse = new StringReader(resp);

            try
            {
                response = (PlugNPayApi.PNP)serResponse.Deserialize(srResponse);
            }
            catch (InvalidOperationException e)  // invalid xml, or no reply received from PlugNPay
            {
                throw new Exception("Could not parse response from PlugNPay server: " + e.Message + " Response received: " + resp.Replace('\n', ' '));
            }

            srResponse.Close();

            return response;
        }

        public override bool SupportsEChecks()
        {
            return true;
        }
    }

    namespace PlugNPayApi
    {
        [XmlRoot("PNPxml")]
        public class PNP
        {
            [XmlAttribute()]
            public string version = "2.0";

            [XmlAttribute()]
            public string timeStamp = DateTime.UtcNow.ToString("s");

            public PNPHeader Header = new PNPHeader();
            public PNPRequest Request = new PNPRequest();
            public PNPResponse Response;
        }

        public class PNPHeader
        {
            public PNPHeaderMerchant Merchant = new PNPHeaderMerchant();
            public string Version = "2.0";
            public int TranCount = 1;
        }

        public class PNPHeaderMerchant
        {
            public string AcctName = AppLogic.AppConfig("PlugNPay_Username");
            public string Password = AppLogic.AppConfig("PlugNPay_Password");
            public string Email;
        }

        public class PNPRequest
        {
            public PNPRequestTransactionRequest TransactionRequest = new PNPRequestTransactionRequest();
        }

        public class PNPResponse
        {
            public PNPResponseTransactionResponse TransactionResponse = new PNPResponseTransactionResponse();
        }

        public class PNPRequestTransactionRequest
        {
            public string Mode;
            public string AuthType;
            public string TxnType;
            public PNPRequestTransactionRequestInstructions Instructions = new PNPRequestTransactionRequestInstructions();
            public string IPaddress;
            public string TransactionID;
            public string CustRef2;
            public string CustRef3;
            public string CustRef4;
            public string CustRef5;
            public string cavv;
            public string eci;
            public string xid;
            public PNPRequestTransactionRequestBillDetails BillDetails = new PNPRequestTransactionRequestBillDetails();
            public PNPRequestTransactionRequestShipDetails ShipDetails;
            public PNPRequestTransactionRequestPaymentDetails PaymentDetails = new PNPRequestTransactionRequestPaymentDetails();
            public PNPRequestTransactionRequestOrder Order = new PNPRequestTransactionRequestOrder();
        }

        public class PNPResponseTransactionResponse
        {
            public string Mode;
            public string AVSResp;
            public string AuthCode;
            public string CVVResp;
            public string Duplicate;
            public string FinalStatus;
            public string MErrMsg;
            public string RespCode;
            public string SRespCode;
            public string TransactionID;
            public string CustRef2;
            public string CustRef3;
            public string CustRef4;
            public string CustRef5;
            public PNPRequestTransactionRequestBillDetails BillDetails = new PNPRequestTransactionRequestBillDetails();
            public PNPRequestTransactionRequestShipDetails ShipDetails;
            public PNPRequestTransactionRequestOrder Order = new PNPRequestTransactionRequestOrder();
        }

        public class PNPRequestTransactionRequestInstructions
        {
            public string DontSndMail = "yes";
            public string AppLevel;
        }

        public class PNPRequestTransactionRequestBillDetails
        {
            public string CardName;
            public string CardCompany;
            public string CardAddress1;
            public string CardAddress2;
            public string CardCity;
            public string CardState;
            public string CardProv;
            public string CardZip;
            public string CardCountry;
            public string Email;
        }

        public class PNPRequestTransactionRequestShipDetails
        {
            public string ShipName;
            public string Company;
            public string Address1;
            public string Address2;
            public string City;
            public string State;
            public string Province;
            public string Zip;
            public string Country;
            public string ShipInfo;
        }

        public class PNPRequestTransactionRequestPaymentDetails
        {
            public string PaymentMethod;
            public string CardNumber;
            public string CardExp;
            public string CardType;
            public string CardCVV;
            public string MagStripe;
            public string AccountNum;
            public string RoutingNum;
            public string AcctType;
            public string AcctClass;
            public string CheckType;
            public string CommCardType;
            public string Phone;
        }

        public class PNPRequestTransactionRequestOrder
        {
            public string EasyCart;
            public string CardAmount;
            public string Currency;
            public string Tax;
            public string Shipping;
            public int ItemCount;
            public PNPRequestTransactionRequestOrderOrderDetails[] OrderDetails;
        }

        public class PNPRequestTransactionRequestOrderOrderDetails
        {
            public string Sku;
            public int Qty;
            public string Desc;
            public string Cost;
        }

        public class RequestMode
        {
            public const string Auth = "auth";
            public const string CheckCard = "checkcard";
            public const string Return = "return";
            public const string Mark = "mark";
            public const string Void = "void";
        }

        public class AuthType
        {
            public const string AuthOnly = "authonly";
            public const string AuthPostAuth = "authpostauth";
        }

        public class TxnType
        {
            public const string Auth = "auth";
            public const string ForceAuth = "forceauth";
            public const string PostAuth = "postauth";
            public const string Return = "return";
            public const string Void = "void";
        }

        public class ResponseFinalStatus
        {
            public const string BadCard = "badcard";
            public const string Pending = "pending";
            public const string Fraud = "fraud";
            public const string Problem = "problem";
            public const string Success = "success";
        }

        public class PaymentMethod
        {
            public const string Credit = "credit";
            public const string OnlineCheck = "onlinecheck";
        }

        public class AcctType
        {
            public const string Checking = "checking";
            public const string Savings = "savings";
        }

        public class AcctClass
        {
            public const string Personal = "personal";
            public const string Business = "business";
        }

        public class CheckType
        {
            public const string DocumentSignedByIndividual = "PPD";
            public const string DocumentSignedByCompany = "CCD";
            public const string ViaTheInternet = "WEB";
        }

        public class CommCardType
        {
            public const string Business = "business";
        }
    }
}
