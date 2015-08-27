// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Security;
using System.Text;
using System.Web.SessionState;
using System.IO;
using System.Xml;
using System.Net;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for QBMerchantServices.
    /// </summary>
    public class QBMerchantServices : GatewayProcessor
    {
        public QBMerchantServices() { }

        public override String CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;

            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";
            String TransID = o.AuthorizationPNREF;
            Decimal OrderTotal = o.OrderBalance;

            String EMail = o.BillingAddress.m_EMail;

            o.CaptureTXCommand = "Not Available";

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    BasicRequestSender requestSender = new BasicRequestSender();
                    AppRegistration appReg = new AppRegistration();
                    appReg.ApplicationType = QBMSApplicationType.Desktop;
                    appReg.ApplicationLogin = AppLogic.AppConfig("QBMERCHANTSERVICES_ApplicationLogin");
                    appReg.ApplicationId = AppLogic.AppConfig("QBMERCHANTSERVICES_ApplicationID");
                    appReg.ApplicationVersion = AppLogic.AppConfig("QBMERCHANTSERVICES_ApplicationVersion");
                    appReg.UseTestEnvironment = !AppLogic.AppConfigBool("UseLiveTransactions");
                    appReg.ConnectionTicket = AppLogic.AppConfig("QBMERCHANTSERVICES_ConnectionTicket");
                    appReg.InstallId = AppLogic.AppConfig("QBMERCHANTSERVICES_InstallID");
                    appReg.Language = AppLogic.AppConfig("QBMERCHANTSERVICES_Language");

                    QBMSGatewayRequest qbms = new QBMSGatewayRequest(appReg, requestSender);

                    String OrderTotalStr = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);
                    CaptureResponse rsp = qbms.SendCaptureRequest(TransID, OrderTotalStr);

                    String ResultCode = rsp.ResultCode.ToString();
                    String Description = rsp.ResultMessage;

                    o.CaptureTXResult = "Not Available";
                    
                    if (ResultCode == "0")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = Description;
                    }
                }
                catch (Exception ex)
                {
                    result = "Error calling QBMerchantServices [QUICKBOOKS] gateway. Please retry in a few minutes. Message=" + ex.Message;
                }
            }
            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());

            String TransID = String.Empty;
            Decimal OrderTotal = System.Decimal.Zero;
            String EMail = String.Empty;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                        EMail = DB.RSField(rs, "BillingEMail");
                    }
                }
            }

            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote("Not Available") + " where OrderNumber=" + OrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    BasicRequestSender requestSender = new BasicRequestSender();
                    AppRegistration appReg = new AppRegistration();
                    appReg.ApplicationType = QBMSApplicationType.Desktop;
                    appReg.ApplicationLogin = AppLogic.AppConfig("QBMERCHANTSERVICES_ApplicationLogin");
                    appReg.ApplicationId = AppLogic.AppConfig("QBMERCHANTSERVICES_ApplicationID");
                    appReg.ApplicationVersion = AppLogic.AppConfig("QBMERCHANTSERVICES_ApplicationVersion");
                    appReg.UseTestEnvironment = !AppLogic.AppConfigBool("UseLiveTransactions");
                    appReg.ConnectionTicket = AppLogic.AppConfig("QBMERCHANTSERVICES_ConnectionTicket");
                    appReg.InstallId = AppLogic.AppConfig("QBMERCHANTSERVICES_InstallID");
                    appReg.Language = AppLogic.AppConfig("QBMERCHANTSERVICES_Language");

                    QBMSGatewayRequest qbms = new QBMSGatewayRequest(appReg, requestSender);

                    VoidResponse rsp = qbms.SendVoidRequest(TransID);

                    String ResultCode = rsp.ResultCode.ToString();
                    String Description = rsp.ResultMessage;

                    DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote("Not Available") + " where OrderNumber=" + OrderNumber.ToString());
                    if (ResultCode == "0")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = Description;
                    }
                }
                catch (Exception ex)
                {
                    result = "Error calling QBMerchantServices [QUICKBOOKS] gateway. Please retry in a few minutes, Message=" + ex.Message;
                }
            }
            return result;
        }

        // if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());

            String TransID = String.Empty;
            Decimal OrderTotal = System.Decimal.Zero;
            String EMail = String.Empty;
            String CardNumber = String.Empty;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        if (UseBillingAddress == null)
                        {
                            UseBillingAddress = new Address();
                            Customer RefundCustomer = new Customer(DB.RSFieldInt(rs, "CustomerID"), true);
                            UseBillingAddress.LoadFromDB(RefundCustomer.PrimaryBillingAddressID);
                        }
                        
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                        EMail = DB.RSField(rs, "EMail");
                        if (RefundAmount != System.Decimal.Zero && UseBillingAddress != null)
                        {
                            CardNumber = UseBillingAddress.CardNumber;
                        }
                        else
                        {
                            CardNumber = Security.UnmungeString(DB.RSField(rs, "CardNumber"), rs[AppLogic.AppConfig("OrdersCCSaltField")].ToString());
                            if (CardNumber.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                            {
                                CardNumber = DB.RSField(rs, "CardNumber");
                            }
                        }
                    }
                }
            }

            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote("Not Available") + " where OrderNumber=" + OriginalOrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else if (CardNumber.Length == 0 || CardNumber == AppLogic.ro_CCNotStoredString)
            {
                result = "Credit Card Number Not Found or Empty";
            }
            else
            {
                try
                {
                    BasicRequestSender requestSender = new BasicRequestSender();
                    AppRegistration appReg = new AppRegistration();
                    appReg.ApplicationType = QBMSApplicationType.Desktop;
                    appReg.ApplicationLogin = AppLogic.AppConfig("QBMERCHANTSERVICES_ApplicationLogin");
                    appReg.ApplicationId = AppLogic.AppConfig("QBMERCHANTSERVICES_ApplicationID");
                    appReg.ApplicationVersion = AppLogic.AppConfig("QBMERCHANTSERVICES_ApplicationVersion");
                    appReg.UseTestEnvironment = !AppLogic.AppConfigBool("UseLiveTransactions");
                    appReg.ConnectionTicket = AppLogic.AppConfig("QBMERCHANTSERVICES_ConnectionTicket");
                    appReg.InstallId = AppLogic.AppConfig("QBMERCHANTSERVICES_InstallID");
                    appReg.Language = AppLogic.AppConfig("QBMERCHANTSERVICES_Language");
                    bool isEcommerce = true;

                    CreditCardInfo creditCard = new CreditCardInfo();
                    creditCard.CardNumber = CardNumber;
                    creditCard.NameOnCard = UseBillingAddress.CardName;
                    creditCard.BillingAddress = UseBillingAddress.Address1;

                    String zip = UseBillingAddress.Zip;
                    if (zip.Length > 9)
                        zip = zip.Replace("-", "");
                    if (zip.Length > 9)
                        zip = zip.Substring(0, 9);

                    creditCard.BillingPostalCode = zip;
                
                    creditCard.ExpireMonth = UseBillingAddress.CardExpirationMonth;
                    creditCard.ExpireYear = UseBillingAddress.CardExpirationYear;
                    creditCard.CommercialCardCode = String.Empty;
                    creditCard.SecurityCode = String.Empty;
					
                    String RefundAmountStr = Localization.CurrencyStringForGatewayWithoutExchangeRate(CommonLogic.IIF(RefundAmount == System.Decimal.Zero, OrderTotal, RefundAmount));

                    QBMSGatewayRequest qbms = new QBMSGatewayRequest(appReg, requestSender);

                    RefundResponse rsp = qbms.SendRefundRequest(creditCard, RefundAmountStr, Localization.CurrencyStringForGatewayWithoutExchangeRate(System.Decimal.Zero), isEcommerce);

                    String ResultCode = rsp.ResultCode.ToString();
                    String Description = rsp.ResultMessage;

                    DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote("Not Available") + " where OrderNumber=" + OriginalOrderNumber.ToString());
                    if (ResultCode == "0")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = Description;
                    }
                }
                catch (Exception ex)
                {
                    result = "Error calling QBMerchantServices [QUICKBOOKS] gateway. Please retry in a few minutes, Message=" + ex.Message;
                }
            }
            return result;
        }

        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = AppLogic.ro_OK;

            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            AVSResult = String.Empty;
            TransactionCommandOut = "Not Available With This Gateway";
            TransactionResponse = String.Empty;

            BasicRequestSender requestSender = new BasicRequestSender();
            AppRegistration appReg = new AppRegistration();
            appReg.ApplicationType = QBMSApplicationType.Desktop;
            appReg.ApplicationLogin = AppLogic.AppConfig("QBMERCHANTSERVICES_ApplicationLogin");
            appReg.ApplicationId = AppLogic.AppConfig("QBMERCHANTSERVICES_ApplicationID");
            appReg.ApplicationVersion = AppLogic.AppConfig("QBMERCHANTSERVICES_ApplicationVersion");
            appReg.UseTestEnvironment = !AppLogic.AppConfigBool("UseLiveTransactions");
            appReg.ConnectionTicket = AppLogic.AppConfig("QBMERCHANTSERVICES_ConnectionTicket");
            appReg.InstallId = AppLogic.AppConfig("QBMERCHANTSERVICES_InstallID");
            appReg.Language = AppLogic.AppConfig("QBMERCHANTSERVICES_Language");

            CreditCardInfo creditCard = new CreditCardInfo();
            creditCard.NameOnCard = UseBillingAddress.CardName;
            creditCard.CardNumber = UseBillingAddress.CardNumber;
            creditCard.ExpireMonth = UseBillingAddress.CardExpirationMonth.ToString().PadLeft(2, '0');
            creditCard.ExpireYear = UseBillingAddress.CardExpirationYear.ToString();
            creditCard.SecurityCode = CardExtraCode;
            creditCard.CommercialCardCode = String.Empty;
            creditCard.BillingAddress = UseBillingAddress.Address1;

            String zip = UseBillingAddress.Zip;
            if (zip.Length > 9)
                zip = zip.Replace("-", "");
            if (zip.Length > 9)
                zip = zip.Substring(0, 9);

            creditCard.BillingPostalCode = zip;

            string amount = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);
            string salesTaxAmount = String.Empty; // this one is a bit problematic to pass here
            bool isEcommerce = true;
            bool isRecurring = false;

            try
            {
                QBMSGatewayRequest qbms = new QBMSGatewayRequest(appReg, requestSender);

                String AVSStreet = String.Empty;
                String AVSZip = String.Empty;
                String CardSecurityCodeMatch = String.Empty;
                String CreditCardTransID = String.Empty;
                String MerchantAccountNumber = String.Empty;
                String PaymentGroupingCode = String.Empty;
                String PaymentStatus = String.Empty;
                String ReconBatchID = String.Empty;
                String ResultCode = String.Empty;
                String ResultMessage = String.Empty;
                String TxnAuthorizationStamp = String.Empty;
                String TxnAuthorizationTime = String.Empty;

                if (TransactionMode == TransactionModeEnum.auth)
                {
                    AuthResponse authResponse = qbms.SendAuthRequest(creditCard, amount, salesTaxAmount, isEcommerce, isRecurring);
                    AVSStreet = authResponse.AVSStreet;
                    AVSZip = authResponse.AVSZip;
                    CardSecurityCodeMatch = authResponse.CardSecurityCodeMatch;
                    CreditCardTransID = authResponse.CreditCardTransID;
                    MerchantAccountNumber = String.Empty; 
                    PaymentGroupingCode = String.Empty; 
                    PaymentStatus = String.Empty; 
                    ReconBatchID = String.Empty; 
                    ResultCode = authResponse.ResultCode.ToString();
                    ResultMessage = authResponse.ResultMessage;
                    TxnAuthorizationStamp = String.Empty; 
                    TxnAuthorizationTime = String.Empty; 
                    AuthorizationCode = authResponse.AuthorizationCode;
                }
                else
                {
                    ChargeResponse chargeResponse = qbms.SendChargeRequest(creditCard, amount, salesTaxAmount, isEcommerce, isRecurring);
                    AVSStreet = chargeResponse.AVSStreet;
                    AVSZip = chargeResponse.AVSZip;
                    CardSecurityCodeMatch = chargeResponse.CardSecurityCodeMatch;
                    CreditCardTransID = chargeResponse.CreditCardTransID;
                    MerchantAccountNumber = chargeResponse.MerchantAccountNumber;
                    PaymentGroupingCode = chargeResponse.PaymentGroupingCode;
                    PaymentStatus = chargeResponse.PaymentStatus;
                    ReconBatchID = chargeResponse.ReconBatchID;
                    ResultCode = chargeResponse.ResultCode.ToString();
                    ResultMessage = chargeResponse.ResultMessage;
                    TxnAuthorizationStamp = chargeResponse.TxnAuthorizationStamp;
                    TxnAuthorizationTime = chargeResponse.TxnAuthorizationTime;
                    AuthorizationCode = chargeResponse.AuthorizationCode;

                }

                result = ResultMessage;
                AuthorizationTransID = CreditCardTransID;
                AVSResult = AVSZip + "|" + AVSStreet + "|" + CardSecurityCodeMatch;

                // build up Xml AuthorizationResult:
                StringBuilder tmpS = new StringBuilder(4096);
                tmpS.Append("<CreditCardTxnResultInfo>");
                tmpS.Append("<ResultCode>" + ResultCode + "</ResultCode>");
                tmpS.Append("<ResultMessage>" + ResultMessage + "</ResultMessage>");
                tmpS.Append("<CreditCardTransID>" + AuthorizationTransID + "</CreditCardTransID>");
                tmpS.Append("<MerchantAccountNumber>" + MerchantAccountNumber + "</MerchantAccountNumber>");
                tmpS.Append("<AuthorizationCode>" + AuthorizationCode + "</AuthorizationCode>");
                tmpS.Append("<AVSStreet>" + AVSStreet + "</AVSStreet>");
                tmpS.Append("<AVSZip>" + AVSZip + "</AVSZip>");
                tmpS.Append("<ReconBatchID>" + ReconBatchID + "</ReconBatchID>");
                tmpS.Append("<PaymentGroupingCode>" + PaymentGroupingCode + "</PaymentGroupingCode>");
                tmpS.Append("<PaymentStatus>" + PaymentStatus + "</PaymentStatus>");
                tmpS.Append("<TxnAuthorizationTime>" + TxnAuthorizationTime + "</TxnAuthorizationTime>");
                tmpS.Append("<TxnAuthorizationStamp>" + TxnAuthorizationStamp + "</TxnAuthorizationStamp>");
                tmpS.Append("</CreditCardTxnResultInfo>");
                AuthorizationResult = tmpS.ToString();

                if (ResultCode == "0")
                {
                    result = AppLogic.ro_OK;
                }
            }
            catch (Exception ex)
            {
                result = "Error calling QBMerchantServices [QUICKBOOKS] gateway. Please retry your order in a few minutes or select another checkout payment option. Message=" + ex.Message;
            }
            return result;
        }

        public override string DisplayName(string LocaleSetting)
        {
            return "QuickBooks Merchant Services";
        }
    }

    /// <summary>
    /// Used in the QBMSRequestor constructor to indicate if the applicationlication using 
    /// QBMSRequestor is registered as a hosted or desktop applicationlication.
    /// </summary>
    public enum QBMSApplicationType
    {
        Desktop,
        Hosted
    }

    public class CreditCardInfo
    {
        private string m_cardnumber;
        private string m_nameoncard;
        private string m_expiremonth;
        private string m_expireyear;
        private string m_billingaddress;
        private string m_billingpostalcode;
        private string m_commercialcardcode;
        private string m_securitycode;

        public string CardNumber
        {
            get { return m_cardnumber; }
            set { m_cardnumber = value; }
        }

        public string NameOnCard
        {
            get { return m_nameoncard; }
            set { m_nameoncard = value; }
        }

        public string ExpireMonth
        {
            get { return m_expiremonth; }
            set { m_expiremonth = value; }
        }

        public string ExpireYear
        {
            get { return m_expireyear; }
            set { m_expireyear = value; }
        }

        public string BillingAddress
        {
            get { return m_billingaddress; }
            set { m_billingaddress = value; }
        }

        public string BillingPostalCode
        {
            get { return m_billingpostalcode; }
            set { m_billingpostalcode = value; }
        }

        public string CommercialCardCode
        {
            get { return m_commercialcardcode; }
            set { m_commercialcardcode = value; }
        }


        public string SecurityCode
        {
            get { return m_securitycode; }
            set { m_securitycode = value; }
        }

    }

    /// <summary>
    /// The main class for this library,  provide information regarding your
    /// application's registration with QBMS (from http://applicationreg.quickbooks.com) when you
    /// construct this class.  There are methods for sending each supported QBMS
    /// request with the response returned as a unique object type containing  
    /// properties corresponding to the data returned by QBMS.
    /// </summary>
    public class QBMSGatewayRequest
    {
        private QBMSApplicationType applicationType;
        private string applicationLogin;
        private string connectionTicket;
        private string installId;
        private string language;
        private string applicationId;
        private string applicationVersion;
        private string sessionTicket;
        private RequestSender sender;
        private string reqUrl;

        /// <summary>
        /// Create a QBMSRequestor with all the information about your applicationlication
        /// registration from http://applicationreg.quickbooks.com,  and other infor needed by the 
        /// QBMSXML Signon blocks.
        /// </summary>
        /// <param name="appRegistration">AppRegistration object containing information about app's registration with Intuit</param>
        /// <param name="sender">An object implementing the QBMSLib.RequestSender interface</param>
        public QBMSGatewayRequest(AppRegistration appRegistration, RequestSender sender)
        {
            this.applicationType = appRegistration.ApplicationType;
            string ptc = (appRegistration.UseTestEnvironment) ? ".ptc" : "";
            if (this.applicationType == QBMSApplicationType.Desktop)
            {
                reqUrl = "https://merchantaccount";
            }
            else
            {
                reqUrl = "https://webmerchantaccount";
            }
            reqUrl = reqUrl + ptc + ".quickbooks.com/j/AppGateway";
            this.applicationLogin = appRegistration.ApplicationLogin;
            this.connectionTicket = appRegistration.ConnectionTicket;
            this.installId = appRegistration.InstallId;
            this.language = appRegistration.Language;
            this.applicationId = appRegistration.ApplicationId;
            this.applicationVersion = appRegistration.ApplicationVersion;

            sessionTicket = "";
            if (sender == null)
            {
                throw new Exception("RequestSender is required");
            }
            this.sender = sender;
        }

        #region private utility functions.
        private string getClientDate()
        {
            System.DateTime today;
            today = System.DateTime.Now;
            return today.ToString("yyyy-MM-dd") + "T" + today.ToString("HH:mm:ss");
        }

        private XmlElement MakeSimpleElem(XmlDocument doc, string tagName, string tagVal)
        {
            XmlElement elem = doc.CreateElement(tagName);
            elem.InnerText = tagVal;
            return elem;
        }

        private XmlDocument InitRequest()
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));
            doc.AppendChild(doc.CreateProcessingInstruction("qbmsxml", "version=\"1.0\""));
            XmlNode QBMSXML = doc.CreateElement("QBMSXML");
            doc.AppendChild(QBMSXML);
            XmlNode SignonMsgs = doc.CreateElement("SignonMsgsRq");
            QBMSXML.AppendChild(SignonMsgs);
            XmlNode Signon;
            if (sessionTicket.Length > 0)
            {
                Signon = doc.CreateElement("SignonTicketRq");
                Signon.AppendChild(MakeSimpleElem(doc, "ClientDateTime", getClientDate()));
                Signon.AppendChild(MakeSimpleElem(doc, "SessionTicket", sessionTicket));
            }
            else
            {
                if (applicationType == QBMSApplicationType.Desktop)
                {
                    Signon = doc.CreateElement("SignonDesktopRq");
                }
                else
                {
                    Signon = doc.CreateElement("SignonAppCertRq");
                }
                Signon.AppendChild(MakeSimpleElem(doc, "ClientDateTime", getClientDate()));
                Signon.AppendChild(MakeSimpleElem(doc, "ApplicationLogin", applicationLogin));
                Signon.AppendChild(MakeSimpleElem(doc, "ConnectionTicket", connectionTicket));
            }
            if (installId.Length > 0)
            {
                Signon.AppendChild(MakeSimpleElem(doc, "InstallationID", installId));
            }
            Signon.AppendChild(MakeSimpleElem(doc, "Language", language));
            Signon.AppendChild(MakeSimpleElem(doc, "AppID", applicationId));
            Signon.AppendChild(MakeSimpleElem(doc, "AppVer", applicationVersion));
            SignonMsgs.AppendChild(Signon);
            return doc;
        }


        private void CheckSignonRs(XmlDocument doc)
        {
            string[] SignonRsTags = new string[] { "SignonTicketRs", "SignonDesktopRs", "SignonAppCertRs" };
            XmlNodeList nodeList;
            bool done = false;
            for (int i = 0; i < SignonRsTags.Length && !done; i++)
            {
                nodeList = doc.GetElementsByTagName(SignonRsTags[i]);
                if (nodeList.Count > 0)
                {
                    String statusCode = nodeList[0].Attributes["statusCode"].InnerText;
                    if (statusCode != "0")
                    {
                        throw new Exception("SignonResponse Error. Message=" + nodeList[0].Attributes["statusMessage"].InnerText);
                    }
                    done = true;
                }
            }
            if (!done)
            {
                throw new Exception("No SignonResponse found: " + doc.ToString());
            }
            nodeList = doc.GetElementsByTagName("SessionTicket");
            if (nodeList.Count > 0)
            {
                XmlNode tktnode = nodeList.Item(0);
                sessionTicket = tktnode.InnerText;
            }
        }

        private void AddCardInfo(XmlDocument doc, XmlElement request,
            CreditCardInfo creditCard, string amount, string salesTaxAmount,
            bool isEcommerce, bool isRecurring)
        {
            request.AppendChild(MakeSimpleElem(doc, "CreditCardNumber", creditCard.CardNumber));
            request.AppendChild(MakeSimpleElem(doc, "ExpirationMonth", creditCard.ExpireMonth));
            request.AppendChild(MakeSimpleElem(doc, "ExpirationYear", creditCard.ExpireYear));
            request.AppendChild(MakeSimpleElem(doc, "Amount", amount));
            if (creditCard.NameOnCard.Length > 0)
            {
                request.AppendChild(MakeSimpleElem(doc, "NameOnCard", creditCard.NameOnCard));
            }
            if (creditCard.BillingAddress.Length > 0)
            {
                request.AppendChild(MakeSimpleElem(doc, "CreditCardAddress", creditCard.BillingAddress));
            }
            if (creditCard.BillingPostalCode.Length > 0)
            {
                request.AppendChild(MakeSimpleElem(doc, "CreditCardPostalCode", creditCard.BillingPostalCode));
            }

            if (creditCard.CommercialCardCode != null && creditCard.CommercialCardCode.Length > 0)
            {
                request.AppendChild(MakeSimpleElem(doc, "CommercialCardCode", creditCard.CommercialCardCode));
            }
            else
            {
                request.AppendChild(MakeSimpleElem(doc, "CommercialCardCode", ""));
            }
            if (salesTaxAmount.Length > 0)
            {
                request.AppendChild(MakeSimpleElem(doc, "SalesTaxAmount", salesTaxAmount));
            }
            if (creditCard.SecurityCode != null && creditCard.SecurityCode.Length > 0)
            {
                request.AppendChild(MakeSimpleElem(doc, "CardSecurityCode", creditCard.SecurityCode));
            }
            else
            {
                request.AppendChild(MakeSimpleElem(doc, "CardSecurityCode", ""));
            }

            if (isEcommerce)
            {
                request.AppendChild(MakeSimpleElem(doc, "IsECommerce", "true"));
            }
            if (isRecurring)
            {
                request.AppendChild(MakeSimpleElem(doc, "IsRecurring", "true"));
            }
        }

        
        /// <summary>
        /// Add card information for a refund.  XmlNodes required are different then that of an Auth or an Auth Capture
        /// reference: http://developer.intuit.com/uploadedFiles/QuickBooks_SDK/QBSDK/Tecnical_Resources/QBMS_devguide.pdf
        /// pg. 94
        /// </summary>
        private void AddCardRefundInfo(XmlDocument doc, XmlElement request,
            CreditCardInfo creditCard, string amount, string salesTaxAmount,
            bool isEcommerce, bool isRecurring)
        {
            request.AppendChild(MakeSimpleElem(doc, "CreditCardNumber", creditCard.CardNumber));
            request.AppendChild(MakeSimpleElem(doc, "ExpirationMonth", creditCard.ExpireMonth));
            request.AppendChild(MakeSimpleElem(doc, "ExpirationYear", creditCard.ExpireYear));
            request.AppendChild(MakeSimpleElem(doc, "Amount", amount));
            if (creditCard.NameOnCard.Length > 0)
            {
                request.AppendChild(MakeSimpleElem(doc, "NameOnCard", creditCard.NameOnCard));
            }
            if (creditCard.CommercialCardCode != null && creditCard.CommercialCardCode.Length > 0)
            {
                request.AppendChild(MakeSimpleElem(doc, "CommercialCardCode", creditCard.CommercialCardCode));
            }
            else
            {
                request.AppendChild(MakeSimpleElem(doc, "CommercialCardCode", ""));
            }
            if (salesTaxAmount.Length > 0)
            {
                request.AppendChild(MakeSimpleElem(doc, "SalesTaxAmount", salesTaxAmount));
            }
            if (isEcommerce)
            {
                request.AppendChild(MakeSimpleElem(doc, "IsECommerce", "true"));
            }
        }

        #endregion

        /// <summary>
        /// Send a CustomerCreditCardAuthRq Request.
        /// </summary>
        /// <returns>an AuthResponse object with result of authorization</returns>
        public AuthResponse SendAuthRequest(
            CreditCardInfo creditCard,
            string amount, string salesTaxAmount,
            bool isEcommerce, bool isRecurring)
        {
            XmlDocument doc = InitRequest();
            XmlElement QBMSXMLMsgs = doc.CreateElement("QBMSXMLMsgsRq");
            doc.DocumentElement.AppendChild(QBMSXMLMsgs);
            XmlElement AuthRq = doc.CreateElement("CustomerCreditCardAuthRq");
            QBMSXMLMsgs.AppendChild(AuthRq);
            AddCardInfo(doc, AuthRq, creditCard, amount, salesTaxAmount, isEcommerce, isRecurring);
            String strResp = sender.SendRequest(reqUrl, doc.OuterXml);
            if (strResp == null)
            {
                throw new Exception("No response from SendRequest");
            }
            XmlDocument resp = new XmlDocument();
            resp.LoadXml(strResp);
            CheckSignonRs(resp);
            return new AuthResponse(resp);
        }


        /// <summary>
        /// Send a request to capture (charge) a prior authorization
        /// </summary>
        /// <param name="transID">the transaction ID from a previous Auth request</param>
        /// <param name="amount">amount to capture</param>
        /// <returns>a CaptureResponse object with the transaction information for the capture</returns>
        public CaptureResponse SendCaptureRequest(string transID, string amount)
        {
            XmlDocument doc = InitRequest();
            XmlElement QBMSXMLMsgs = doc.CreateElement("QBMSXMLMsgsRq");
            doc.DocumentElement.AppendChild(QBMSXMLMsgs);
            XmlElement CCapture = doc.CreateElement("CustomerCreditCardCaptureRq");
            QBMSXMLMsgs.AppendChild(CCapture);
            CCapture.AppendChild(MakeSimpleElem(doc, "CreditCardTransID", transID));
            CCapture.AppendChild(MakeSimpleElem(doc, "Amount", amount));
            String strResp = sender.SendRequest(reqUrl, doc.OuterXml);
            if (strResp == null)
            {
                throw new Exception("No response from SendRequest");
            }
            XmlDocument resp = new XmlDocument();
            resp.LoadXml(strResp);
            CheckSignonRs(resp);
            return new CaptureResponse(resp);
        }


        /// <summary>
        /// Authorize and charge a card in one swoop.
        /// </summary>
        /// <returns>a ChargeResponse object with the transaction info from the charge request</returns>
        public ChargeResponse SendChargeRequest(
            CreditCardInfo creditCard,
            string amount, string salesTaxAmount,
            bool isEcommerce, bool isRecurring
            )
        {
            XmlDocument doc = InitRequest();
            XmlElement QBMSXMLMsgs = doc.CreateElement("QBMSXMLMsgsRq");
            doc.DocumentElement.AppendChild(QBMSXMLMsgs);
            XmlElement CCCharge = doc.CreateElement("CustomerCreditCardChargeRq");
            QBMSXMLMsgs.AppendChild(CCCharge);
            AddCardInfo(doc, CCCharge, creditCard, amount, salesTaxAmount, isEcommerce, isRecurring);
          
            String strResp = sender.SendRequest(reqUrl, doc.OuterXml);
            if (strResp == null)
            {
                throw new Exception("No response from SendRequest");
            }
            XmlDocument resp = new XmlDocument();
            resp.LoadXml(strResp);
            CheckSignonRs(resp);
            return new ChargeResponse(resp);
        }

        /// <summary>
        /// Refund a customer credit card.
        /// </summary>
        /// <returns>a RefundResponse object with the transaction info from the refund request</returns>
        public RefundResponse SendRefundRequest(
            CreditCardInfo creditCard,
            string amount, string salesTaxAmount,
            bool isEcommerce
            )
        {
            XmlDocument doc = InitRequest();
            XmlElement QBMSXMLMsgs = doc.CreateElement("QBMSXMLMsgsRq");
            doc.DocumentElement.AppendChild(QBMSXMLMsgs);
            XmlElement CCRefund = doc.CreateElement("CustomerCreditCardRefundRq");
            QBMSXMLMsgs.AppendChild(CCRefund);
         
            AddCardRefundInfo(doc, CCRefund, creditCard, amount, salesTaxAmount, isEcommerce, false);
           
            String strResp = sender.SendRequest(reqUrl, doc.OuterXml);
            if (strResp == null)
            {
                throw new Exception("No response from SendRequest");
            }
            XmlDocument resp = new XmlDocument();
            resp.LoadXml(strResp);
            CheckSignonRs(resp);
            return new RefundResponse(resp);
        }


        /// <summary>
        /// Void a previous card transaction
        /// </summary>
        /// <param name="transID">the transaction ID to void</param>
        /// <returns>a VoidResponse object with the transaction info from the Void</returns>
        public VoidResponse SendVoidRequest(string transID)
        {
            XmlDocument doc = InitRequest();
            XmlElement QBMSXMLMsgs = doc.CreateElement("QBMSXMLMsgsRq");
            doc.DocumentElement.AppendChild(QBMSXMLMsgs);
            XmlElement VoidRq = doc.CreateElement("CustomerCreditCardTxnVoidRq");
            QBMSXMLMsgs.AppendChild(VoidRq);
            VoidRq.AppendChild(MakeSimpleElem(doc, "CreditCardTransID", transID));
            String strResp = sender.SendRequest(reqUrl, doc.OuterXml);
            if (strResp == null)
            {
                throw new Exception("No response from SendRequest");
            }
            XmlDocument resp = new XmlDocument();
            resp.LoadXml(strResp);
            CheckSignonRs(resp);
            return new VoidResponse(resp);
        }

        /// <summary>
        /// Authorize a card with a voice authorization code
        /// </summary>
        /// <param name="CCNum">Customer credit card number</param>
        /// <param name="ExpireMonth">Card expiration month</param>
        /// <param name="ExpireYear">Card expiration year</param>
        /// <param name="Amount">Amount of charge</param>
        /// <param name="AuthCode">voice authorization code</param>
        /// <param name="CCCCode">Commercial card code</param>
        /// <param name="STAmount">Sales tax amount</param>
        /// <returns></returns>
        public VoiceAuthResponse SendVoiceAuthRequest(
            CreditCardInfo creditCard,
            string amount, string salesTaxAmount,
            bool isEcommerce, string authorizationCode)
        {
            XmlDocument doc = InitRequest();
            XmlElement QBMSXMLMsgs = doc.CreateElement("QBMSXMLMsgsRq");
            doc.DocumentElement.AppendChild(QBMSXMLMsgs);
            XmlElement AuthRq = doc.CreateElement("CustomerCreditCardAuthRq");
            QBMSXMLMsgs.AppendChild(AuthRq);
            AuthRq.AppendChild(MakeSimpleElem(doc, "CreditCardNumber", creditCard.CardNumber));
            AuthRq.AppendChild(MakeSimpleElem(doc, "ExpirationMonth", creditCard.ExpireMonth));
            AuthRq.AppendChild(MakeSimpleElem(doc, "ExpirationYear", creditCard.ExpireYear));
            AuthRq.AppendChild(MakeSimpleElem(doc, "Amount", amount));
            AuthRq.AppendChild(MakeSimpleElem(doc, "AuthorizationCode", authorizationCode));
            if (creditCard.CommercialCardCode.Length > 0)
            {
                AuthRq.AppendChild(MakeSimpleElem(doc, "CommercialCardCode", creditCard.CommercialCardCode));
            }
            if (salesTaxAmount.Length > 0)
            {
                AuthRq.AppendChild(MakeSimpleElem(doc, "SalesTaxAmount", salesTaxAmount));
            }
            if (isEcommerce)
            {
                AuthRq.AppendChild(MakeSimpleElem(doc, "IsECommerce", "true"));
            }
            String strResp = sender.SendRequest(reqUrl, doc.OuterXml);
            if (strResp == null)
            {
                throw new Exception("No response from SendRequest");
            }
            XmlDocument resp = new XmlDocument();
            resp.LoadXml(strResp);
            CheckSignonRs(resp);
            return new VoiceAuthResponse(resp);

        }

    }

    /// <summary>
    /// Encapsulates the parsing of the XML response from QBMS and making that 
    /// data available to clients.  Contains the following properites for authorization transaction
    /// ID, authorization code, result of address validation for street and zip data
    /// result of security code checking.
    /// </summary>
    public class AppRegistration
    {
        private QBMSApplicationType m_applicationtype = QBMSApplicationType.Desktop;
        private string m_applicationlogin = "";
        private string m_connectionticket = "";
        private string m_installid = "";
        private string m_language = "English";
        private string m_applicationid = "";
        private string m_applicationversion = "";
        private bool m_usetestenvironment = false;

        public QBMSApplicationType ApplicationType
        {
            get { return m_applicationtype; }
            set { m_applicationtype = value; }
        }

        public string ApplicationLogin
        {
            get { return m_applicationlogin; }
            set { m_applicationlogin = value; }
        }

        public string ConnectionTicket
        {
            get { return m_connectionticket; }
            set { m_connectionticket = value; }
        }

        public string InstallId
        {
            get { return m_installid; }
            set { m_installid = value; }
        }

        public string Language
        {
            get { return m_language; }
            set { m_language = value; }
        }

        public string ApplicationId
        {
            get { return m_applicationid; }
            set { m_applicationid = value; }
        }

        public string ApplicationVersion
        {
            get { return m_applicationversion; }
            set { m_applicationversion = value; }
        }

        public bool UseTestEnvironment
        {
            get { return m_usetestenvironment; }
            set { m_usetestenvironment = value; }
        }


        public AppRegistration()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationType">hosted or desktop</param>
        /// <param name="applicationLogin">registered applicationlogin name</param>
        /// <param name="connectionTicket">the connection sessionTicket to use for the current user</param>
        /// <param name="installId">installation ID to use, can be empty string</param>
        /// <param name="language">English is only valid value at this time</param>
        /// <param name="applicationId">applicationID you got when you registered at applicationreg.quickbooks.com</param>
        /// <param name="applicationVersion">applicationlication version string</param>
        /// <param name="useTestEnvironment">boolean indicating whether to use IDNBeta or production environment</param>
        public AppRegistration
            (
                QBMSApplicationType applicationType,
                string applicationLogin,
                string connectionTicket,
                string installId,
                string language,
                string applicationId,
                string applicationVersion,
                bool useTestEnvironment
            )
        {
            ApplicationType = applicationType;
            ApplicationLogin = applicationLogin;
            ConnectionTicket = connectionTicket;
            InstallId = installId;
            Language = language;
            ApplicationId = applicationId;
            ApplicationVersion = applicationVersion;
            UseTestEnvironment = useTestEnvironment;
        }
    }

    /// <summary>
    /// The SimpleRequestSender class implements the RequestSender interface and simply
    /// does a POST of the request to the given URL, no client certificate set up, etc.
    /// This is fine for desktop-based applications.
    /// </summary>
    public class BasicRequestSender : RequestSender
    {
        #region RequestSender Members

        /// <summary>
        /// send an XML request to qbMS and return the XML response.
        /// </summary>
        /// <param name="URL">The URL to which the request should be POSTed</param>
        /// <param name="request">The XML string to POST</param>
        /// <returns>The XML response or an empty string if an error occurred</returns>
        public string SendRequest(string URL, string request)
        {
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] data = enc.GetBytes(request);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
            req.Method = "POST";
            req.ContentLength = data.Length;
            req.ContentType = "application/x-qbmsxml";
            Stream reqStream = req.GetRequestStream();
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            if (req.HaveResponse)
            {
                if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Accepted)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());
                    return reader.ReadToEnd();
                }
                else
                {
                    throw new Exception("Request failed: " + resp.StatusDescription);
                }
            }
            return null;
        }

        #endregion

    }

    /// <summary>
    /// The RequestSender interface is used by the QBMSRequestor to send qbmsXML
    /// requests to QuickBooks merchant services.  Any object which implements this
    /// interface will work with QBMSRequestor, including the 
    /// QBMSLib.SimpleRequestSender which just does a standard HTTPS POST, with no
    /// client certificate presentation.  
    /// 
    /// </summary>
    public interface RequestSender
    {
        /// <summary>
        /// send an XML request to qbMS and return the XML response.
        /// </summary>
        /// <param name="URL">The URL to which the request should be POSTed</param>
        /// <param name="request">The XML string to POST</param>
        /// <returns>The XML response or an empty string if an error occurred</returns>
        string SendRequest(string URL, string request);
    }

    /// <summary>
    /// Encapsulates the parsing of the XML response from QBMS and making that 
    /// data available to clients.  Contains the following properites for authorization transaction
    /// ID, authorization code, result of address validation for street and zip data
    /// result of security code checking.
    /// </summary>
    public class AuthResponse
    {
        private string m_creditcardtransid;
        private string m_authorizationcode;
        private string m_avsstreet;
        private string m_avszip;
        private string m_cardsecuritycodematch;
        private int m_resultcode;
        private string m_resultmessage;

        internal AuthResponse(XmlDocument doc)
        {
            XmlNodeList resp = doc.GetElementsByTagName("CustomerCreditCardAuthRs");
            if (resp.Count < 1)
            {
                String msg = String.Empty;
                XmlNode n = doc.SelectSingleNode("//statusMessage");
                if (n != null)
                {
                    msg = n.InnerText;
                }
                throw new Exception("AuthResponse: No CustomerCreditCardAuthRs found! Message=[" + msg + "]");
            }
            XmlAttributeCollection attrList = resp.Item(0).Attributes;
            for (int i = 0; i < attrList.Count; i++)
            {
                XmlAttribute curAttr = (XmlAttribute)attrList.Item(i);
                if ("statusCode".Equals(curAttr.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    m_resultcode = int.Parse(curAttr.Value);
                }
                else if (curAttr.Name == "statusMessage")
                {
                    m_resultmessage = curAttr.Value;
                }
            }
            XmlNode curnode = resp.Item(0).FirstChild;
            while (curnode != null)
            {
                switch (curnode.Name)
                {
                    case "CreditCardTransID":
                        m_creditcardtransid = curnode.InnerText;
                        break;
                    case "AuthorizationCode":
                        m_authorizationcode = curnode.InnerText;
                        break;
                    case "AVSStreet":
                        m_avsstreet = curnode.InnerText;
                        break;
                    case "AVSZip":
                        m_avszip = curnode.InnerText;
                        break;
                    case "CardSecurityCodeMatch":
                        m_cardsecuritycodematch = curnode.InnerText;
                        break;
                    default:
                        break;
                }
                curnode = curnode.NextSibling;
            }
        }

        public int ResultCode
        {
            get
            {
                return m_resultcode;
            }
        }

        public string ResultMessage
        {
            get
            {
                return m_resultmessage;
            }
        }


        public string CreditCardTransID
        {
            get
            {
                return m_creditcardtransid;
            }
        }
        public string AuthorizationCode
        {
            get
            {
                return m_authorizationcode;
            }
        }
        public string AVSStreet
        {
            get
            {
                return m_avsstreet;
            }
        }
        public string AVSZip
        {
            get
            {
                return m_avszip;
            }
        }
        public string CardSecurityCodeMatch
        {
            get
            {
                return m_cardsecuritycodematch;
            }
        }
    }

    /// <summary>
    /// Encapsulates the parsing of the XML response from QBMS and making that 
    /// data available to clients.  Contains the following properites for authorization transaction
    /// ID, authorization code, result of address validation for street and zip data
    /// result of security code checking.
    /// </summary>
    public class CaptureResponse
    {
        private string m_creditcardtransid;
        private string m_authorizationcode;
        private string m_merchantaccountnumber;
        private string m_reconbatchid;
        private string m_paymentgroupingcode;
        private string m_paymentstatus;
        private string m_txnauthorizationtime;
        private string m_txnauthorizationstamp;
        private int m_resultcode;
        private string m_resultmessage;

        internal CaptureResponse(XmlDocument doc)
        {
            XmlNodeList resp = doc.GetElementsByTagName("CustomerCreditCardCaptureRs");
            if (resp.Count < 1)
            {
                String msg = String.Empty;
                XmlNode n = doc.SelectSingleNode("//statusMessage");
                if (n != null)
                {
                    msg = n.InnerText;
                }
                throw new Exception("CaptureResponse: No CustomerCreditCardCaptureRs found! Message=[" + msg + "]");
            }
            XmlAttributeCollection attrList = resp.Item(0).Attributes;
            for (int i = 0; i < attrList.Count; i++)
            {
                XmlAttribute curAttr = (XmlAttribute)attrList.Item(i);
                if ("statusCode".Equals(curAttr.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    m_resultcode = int.Parse(curAttr.Value);
                }
                else if (curAttr.Name == "statusMessage")
                {
                    m_resultmessage = curAttr.Value;
                }
            }
            XmlNode curnode = resp.Item(0).FirstChild;
            while (curnode != null)
            {
                switch (curnode.Name)
                {
                    case "CreditCardTransID":
                        m_creditcardtransid = curnode.InnerText;
                        break;
                    case "AuthorizationCode":
                        m_authorizationcode = curnode.InnerText;
                        break;
                    case "MerchantAccountNumber":
                        m_merchantaccountnumber = curnode.InnerText;
                        break;
                    case "ReconBatchID":
                        m_reconbatchid = curnode.InnerText;
                        break;
                    case "PaymentGroupingCode":
                        m_paymentgroupingcode = curnode.InnerText;
                        break;
                    case "PaymentStatus":
                        m_paymentstatus = curnode.InnerText;
                        break;
                    case "TxnAuthorizationTime":
                        m_txnauthorizationtime = curnode.InnerText;
                        break;
                    case "TxnAuthorizationStamp":
                        m_txnauthorizationstamp = curnode.InnerText;
                        break;
                    default:
                        break;
                }
                curnode = curnode.NextSibling;
            }
        }

        public int ResultCode
        {
            get
            {
                return m_resultcode;
            }
        }

        public string ResultMessage
        {
            get
            {
                return m_resultmessage;
            }
        }

        public string CreditCardTransID
        {
            get
            {
                return m_creditcardtransid;
            }
        }
        public string AuthorizationCode
        {
            get
            {
                return m_authorizationcode;
            }
        }
        public string MerchantAccountNumber
        {
            get
            {
                return m_merchantaccountnumber;
            }
        }
        public string ReconBatchID
        {
            get
            {
                return m_reconbatchid;
            }
        }
        public string PaymentGroupingCode
        {
            get
            {
                return m_paymentgroupingcode;
            }
        }
        public string PaymentStatus
        {
            get
            {
                return m_paymentstatus;
            }
        }
        public string TxnAuthorizationTime
        {
            get
            {
                return m_txnauthorizationtime;
            }
        }
        public string TxnAuthorizationStamp
        {
            get
            {
                return m_txnauthorizationstamp;
            }
        }
    }

    /// <summary>
    /// Encapsulates the parsing of the XML response from QBMS and making that 
    /// data available to clients.  Contains the following properites for authorization transaction
    /// ID, authorization code, result of address validation for street and zip data
    /// result of security code checking.
    /// </summary>
    public class ChargeResponse
    {
        private string m_creditcardtransid;
        private string m_authorizationcode;
        private string m_avsstreet;
        private string m_avszip;
        private string m_cardsecuritycodematch;
        private string m_merchantaccountnumber;
        private string m_reconbatchid;
        private string m_paymentgroupingcode;
        private string m_paymentstatus;
        private string m_txnauthorizationtime;
        private string m_txnauthorizationstamp;
        private int m_resultcode;
        private string m_resultmessage;

        internal ChargeResponse(XmlDocument doc)
        {
            XmlNodeList resp = doc.GetElementsByTagName("CustomerCreditCardChargeRs");
            if (resp.Count < 1)
            {
                String msg = String.Empty;
                XmlNode n = doc.SelectSingleNode("//statusMessage");
                if (n != null)
                {
                    msg = n.InnerText;
                }
                throw new Exception("ChargeResponse: No CustomerCreditCardChargeRs found! Message=[" + msg + "]");
            }
            XmlAttributeCollection attrList = resp.Item(0).Attributes;
            for (int i = 0; i < attrList.Count; i++)
            {
                XmlAttribute curAttr = (XmlAttribute)attrList.Item(i);
                if ("statusCode".Equals(curAttr.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    m_resultcode = int.Parse(curAttr.Value);
                }
                else if (curAttr.Name == "statusMessage")
                {
                    m_resultmessage = curAttr.Value;
                }
            }
            XmlNode curnode = resp.Item(0).FirstChild;
            while (curnode != null)
            {
                switch (curnode.Name)
                {
                    case "CreditCardTransID":
                        m_creditcardtransid = curnode.InnerText;
                        break;
                    case "AuthorizationCode":
                        m_authorizationcode = curnode.InnerText;
                        break;
                    case "AVSStreet":
                        m_avsstreet = curnode.InnerText;
                        break;
                    case "AVSZip":
                        m_avszip = curnode.InnerText;
                        break;
                    case "CardSecurityCodeMatch":
                        m_cardsecuritycodematch = curnode.InnerText;
                        break;
                    case "MerchantAccountNumber":
                        m_merchantaccountnumber = curnode.InnerText;
                        break;
                    case "ReconBatchID":
                        m_reconbatchid = curnode.InnerText;
                        break;
                    case "PaymentGroupingCode":
                        m_paymentgroupingcode = curnode.InnerText;
                        break;
                    case "PaymentStatus":
                        m_paymentstatus = curnode.InnerText;
                        break;
                    case "TxnAuthorizationTime":
                        m_txnauthorizationtime = curnode.InnerText;
                        break;
                    case "TxnAuthorizationStamp":
                        m_txnauthorizationstamp = curnode.InnerText;
                        break;
                    default:
                        break;
                }
                curnode = curnode.NextSibling;
            }
        }

        public int ResultCode
        {
            get
            {
                return m_resultcode;
            }
        }

        public string ResultMessage
        {
            get
            {
                return m_resultmessage;
            }
        }

        public string CreditCardTransID
        {
            get
            {
                return m_creditcardtransid;
            }
        }
        public string AuthorizationCode
        {
            get
            {
                return m_authorizationcode;
            }
        }
        public string AVSStreet
        {
            get
            {
                return m_avsstreet;
            }
        }
        public string AVSZip
        {
            get
            {
                return m_avszip;
            }
        }
        public string CardSecurityCodeMatch
        {
            get
            {
                return m_cardsecuritycodematch;
            }
        }
        public string MerchantAccountNumber
        {
            get
            {
                return m_merchantaccountnumber;
            }
        }
        public string ReconBatchID
        {
            get
            {
                return m_reconbatchid;
            }
        }
        public string PaymentGroupingCode
        {
            get
            {
                return m_paymentgroupingcode;
            }
        }
        public string PaymentStatus
        {
            get
            {
                return m_paymentstatus;
            }
        }
        public string TxnAuthorizationTime
        {
            get
            {
                return m_txnauthorizationtime;
            }
        }
        public string TxnAuthorizationStamp
        {
            get
            {
                return m_txnauthorizationstamp;
            }
        }
    }

    /// <summary>
    /// Encapsulates the parsing of the XML response from QBMS and making that 
    /// data available to clients.  Contains the following properites for refund transaction
    /// ID, authorization code, result of address validation for street and zip data
    /// result of security code checking.
    /// </summary>
    public class RefundResponse
    {
        private string m_creditcardtransid;
        private string m_merchantaccountnumber;
        private string m_reconbatchid;
        private string m_paymentgroupingcode;
        private string m_paymentstatus;
        private string m_txnauthorizationtime;
        private string m_txnauthorizationstamp;
        private int m_resultcode;
        private string m_resultmessage;

        internal RefundResponse(XmlDocument doc)
        {
            XmlNodeList resp = doc.GetElementsByTagName("CustomerCreditCardRefundRs");
            if (resp.Count < 1)
            {
                String msg = String.Empty;
                XmlNode n = doc.SelectSingleNode("//statusMessage");
                if (n != null)
                {
                    msg = n.InnerText;
                }
                throw new Exception("ChargeResponse: No CustomerCreditCardChargeRs found! Message=[" + msg + "]");
            }
            XmlAttributeCollection attrList = resp.Item(0).Attributes;
            for (int i = 0; i < attrList.Count; i++)
            {
                XmlAttribute curAttr = (XmlAttribute)attrList.Item(i);
                if ("statusCode".Equals(curAttr.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    m_resultcode = int.Parse(curAttr.Value);
                }
                else if (curAttr.Name == "statusMessage")
                {
                    m_resultmessage = curAttr.Value;
                }
            }
            XmlNode curnode = resp.Item(0).FirstChild;
            while (curnode != null)
            {
                switch (curnode.Name)
                {
                    case "CreditCardTransID":
                        m_creditcardtransid = curnode.InnerText;
                        break;
                    case "MerchantAccountNumber":
                        m_merchantaccountnumber = curnode.InnerText;
                        break;
                    case "ReconBatchID":
                        m_reconbatchid = curnode.InnerText;
                        break;
                    case "PaymentGroupingCode":
                        m_paymentgroupingcode = curnode.InnerText;
                        break;
                    case "PaymentStatus":
                        m_paymentstatus = curnode.InnerText;
                        break;
                    case "TxnAuthorizationTime":
                        m_txnauthorizationtime = curnode.InnerText;
                        break;
                    case "TxnAuthorizationStamp":
                        m_txnauthorizationstamp = curnode.InnerText;
                        break;
                    default:
                        break;
                }
                curnode = curnode.NextSibling;
            }
        }

        public int ResultCode
        {
            get
            {
                return m_resultcode;
            }
        }

        public string ResultMessage
        {
            get
            {
                return m_resultmessage;
            }
        }

        public string CreditCardTransID
        {
            get
            {
                return m_creditcardtransid;
            }
        }
        public string MerchantAccountNumber
        {
            get
            {
                return m_merchantaccountnumber;
            }
        }
        public string ReconBatchID
        {
            get
            {
                return m_reconbatchid;
            }
        }
        public string PaymentGroupingCode
        {
            get
            {
                return m_paymentgroupingcode;
            }
        }
        public string PaymentStatus
        {
            get
            {
                return m_paymentstatus;
            }
        }
        public string TxnAuthorizationTime
        {
            get
            {
                return m_txnauthorizationtime;
            }
        }
        public string TxnAuthorizationStamp
        {
            get
            {
                return m_txnauthorizationstamp;
            }
        }
    }

    /// <summary>
    /// Encapsulates the parsing of the XML response from QBMS and making that 
    /// data available to clients.  Contains the following properites for authorization transaction
    /// ID, authorization code, result of address validation for street and zip data
    /// result of security code checking.
    /// </summary>
    public class VoiceAuthResponse
    {
        private string m_creditcardtransid;
        private string m_authorizationcode;
        private string m_merchantaccountnumber;
        private string m_reconbatchid;
        private string m_paymentgroupingcode;
        private string m_paymentstatus;
        private string m_txnauthorizationtime;
        private string m_txnauthorizationstamp;
        private int m_resultcode;
        private string m_resultmessage;

        internal VoiceAuthResponse(XmlDocument doc)
        {
            XmlNodeList resp = doc.GetElementsByTagName("CustomerCreditCardVoiceAuthRs");
            if (resp.Count < 1)
            {
                String msg = String.Empty;
                XmlNode n = doc.SelectSingleNode("//statusMessage");
                if (n != null)
                {
                    msg = n.InnerText;
                }
                throw new Exception("VoiceAuthResponse: No CustomerCreditCardVoiceAuthRs found! Message=[" + msg + "]");
            }
            XmlAttributeCollection attrList = resp.Item(0).Attributes;
            for (int i = 0; i < attrList.Count; i++)
            {
                XmlAttribute curAttr = (XmlAttribute)attrList.Item(i);
                if ("statusCode".Equals(curAttr.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    m_resultcode = int.Parse(curAttr.Value);
                }
                else if (curAttr.Name == "statusMessage")
                {
                    m_resultmessage = curAttr.Value;
                }
            }
            XmlNode curnode = resp.Item(0).FirstChild;
            while (curnode != null)
            {
                switch (curnode.Name)
                {
                    case "CreditCardTransID":
                        m_creditcardtransid = curnode.InnerText;
                        break;
                    case "AuthorizationCode":
                        m_authorizationcode = curnode.InnerText;
                        break;
                    case "MerchantAccountNumber":
                        m_merchantaccountnumber = curnode.InnerText;
                        break;
                    case "ReconBatchID":
                        m_reconbatchid = curnode.InnerText;
                        break;
                    case "PaymentGroupingCode":
                        m_paymentgroupingcode = curnode.InnerText;
                        break;
                    case "PaymentStatus":
                        m_paymentstatus = curnode.InnerText;
                        break;
                    case "TxnAuthorizationTime":
                        m_txnauthorizationtime = curnode.InnerText;
                        break;
                    case "TxnAuthorizationStamp":
                        m_txnauthorizationstamp = curnode.InnerText;
                        break;
                    default:
                        break;
                }
                curnode = curnode.NextSibling;
            }
        }

        public int ResultCode
        {
            get
            {
                return m_resultcode;
            }
        }

        public string ResultMessage
        {
            get
            {
                return m_resultmessage;
            }
        }

        public string CreditCardTransID
        {
            get
            {
                return m_creditcardtransid;
            }
        }
        public string AuthorizationCode
        {
            get
            {
                return m_authorizationcode;
            }
        }
        public string MerchantAccountNumber
        {
            get
            {
                return m_merchantaccountnumber;
            }
        }
        public string ReconBatchID
        {
            get
            {
                return m_reconbatchid;
            }
        }
        public string PaymentGroupingCode
        {
            get
            {
                return m_paymentgroupingcode;
            }
        }
        public string PaymentStatus
        {
            get
            {
                return m_paymentstatus;
            }
        }
        public string TxnAuthorizationTime
        {
            get
            {
                return m_txnauthorizationtime;
            }
        }
        public string TxnAuthorizationStamp
        {
            get
            {
                return m_txnauthorizationstamp;
            }
        }
    }

    /// <summary>
    /// Encapsulates the parsing of the XML response from QBMS and making that 
    /// data available to clients.  Contains the following properites for authorization transaction
    /// ID, authorization code, result of address validation for street and zip data
    /// result of security code checking.
    /// </summary>
    public class VoidResponse
    {
        private string m_creditcardtransid;
        private int m_resultcode;
        private string m_resultmessage;

        internal VoidResponse(XmlDocument doc)
        {
            XmlNodeList resp = doc.GetElementsByTagName("CustomerCreditCardTxnVoidRs");
            if (resp.Count < 1)
            {
                String msg = String.Empty;
                XmlNode n = doc.SelectSingleNode("//statusMessage");
                if (n != null)
                {
                    msg = n.InnerText;
                }
                throw new Exception("VoidResponse: No CustomerCreditTxnVoidRs found! Message=[" + msg + "]");
            }
            XmlAttributeCollection attrList = resp.Item(0).Attributes;
            for (int i = 0; i < attrList.Count; i++)
            {
                XmlAttribute curAttr = (XmlAttribute)attrList.Item(i);
                if ("statusCode".Equals(curAttr.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    m_resultcode = int.Parse(curAttr.Value);
                }
                else if (curAttr.Name == "statusMessage")
                {
                    m_resultmessage = curAttr.Value;
                }
            }
            XmlNode curnode = resp.Item(0).FirstChild;
            while (curnode != null)
            {
                switch (curnode.Name)
                {
                    case "CreditCardTransID":
                        m_creditcardtransid = curnode.InnerText;
                        break;
                    default:
                        break;
                }
                curnode = curnode.NextSibling;
            }
        }

        public int ResultCode
        {
            get
            {
                return m_resultcode;
            }
        }

        public string ResultMessage
        {
            get
            {
                return m_resultmessage;
            }
        }

        public string CreditCardTransID
        {
            get
            {
                return m_creditcardtransid;
            }
        }
    }
}
