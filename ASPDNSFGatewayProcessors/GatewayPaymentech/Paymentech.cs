// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Web;
using System.Security;
using System.Text;
using System.Xml;
using System.Web.SessionState;
using System.IO;
using System.Net;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for Paymentech.
    /// </summary>
    public class Paymentech : GatewayProcessor
    {
        public Paymentech() { }

        public override string AdministratorSetupPrompt
        {
            get
            {
                return "<a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=paymentech&type=learnmore' target='_blank'>Learn More...</a>";
            }
        }

        public override String CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;

            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";

            String TransID = o.AuthorizationPNREF;
            Decimal TotalAmount = o.OrderBalance;

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(5000);

            transactionCommand.Append("<Request>");
            transactionCommand.Append("<AC>");
            transactionCommand.Append("<CommonData>");
            transactionCommand.Append("<CommonMandatory MessageType=\"C\">");
            transactionCommand.Append("<MerchantID>" + AppLogic.AppConfig("PAYMENTECH_MERCHANT_ID") + "</MerchantID>");
            transactionCommand.Append("<TerminalID TermEntCapInd=\"05\" CATInfoInd=\"06\" TermLocInd=\"01\" CardPresentInd=\"N\" POSConditionCode=\"59\" AttendedTermDataInd=\"01\">" + AppLogic.AppConfig("PAYMENTECH_MERCHANT_TERMINAL_ID") + "</TerminalID>");
            transactionCommand.Append("<BIN>" + AppLogic.AppConfig("PAYMENTECH_BIN") + "</BIN>");
            transactionCommand.Append("<OrderID>" + o.OrderNumber.ToString().PadRight(16, '0') + "</OrderID>");
            transactionCommand.Append("<AmountDetails>");
            transactionCommand.Append("<Amount>" + Localization.CurrencyStringForGatewayWithoutExchangeRate(TotalAmount).Replace(",", "").Replace(".", "").PadLeft(12, '0') + "</Amount>");
            transactionCommand.Append("</AmountDetails>");
            transactionCommand.Append("</CommonMandatory>");
            transactionCommand.Append("<CommonOptional>");
            transactionCommand.Append("<TxRefNum>" + TransID + "</TxRefNum>");
            transactionCommand.Append("</CommonOptional>");
            transactionCommand.Append("</CommonData>");
            transactionCommand.Append("</AC>");
            transactionCommand.Append("</Request>");

            o.CaptureTXCommand = transactionCommand.ToString();

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("PAYMENTECH_LIVE_SERVER"), AppLogic.AppConfig("PAYMENTECH_TEST_SERVER"));
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
            myRequest.Headers.Add("MIME-Version", "1.0");
            myRequest.ContentType = "application/PTI34";
            myRequest.ContentLength = data.Length;
            myRequest.Headers.Add("Content-transfer-encoding", "text");
            myRequest.Headers.Add("Request-number", "1");
            myRequest.Headers.Add("Document-type", "Request");
            myRequest.Method = "POST";
            Stream newStream = myRequest.GetRequestStream();
            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            // get the response
            WebResponse myResponse;
            myResponse = myRequest.GetResponse();
            String rawResponseString = String.Empty;
            using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
            {
                rawResponseString = sr.ReadToEnd();
                // Close and clean up the StreamReader
                sr.Close();
            }
            myResponse.Close();

            // rawResponseString now has gateway response

            XmlDocument Doc = new XmlDocument();
            // Zap the DOCTYPE so we don't try to find a corresponding DTD.
            String t1 = "<!DOCTYPE Response SYSTEM";
            String t2 = ">";
            String doctype = t1 + CommonLogic.ExtractToken(rawResponseString, t1, t2) + t2;
            Doc.LoadXml(rawResponseString.Replace(doctype, ""));

            XmlNode Node = Doc.SelectSingleNode("Response/ACResponse");

            String replyCode = String.Empty;
            String responseCode = String.Empty;
            String authResponse = String.Empty;

            if (Node != null)
            {
                replyCode = XmlCommon.XmlField(Node, "CommonDataResponse/CommonMandatoryResponse/ProcStatus");
                authResponse = XmlCommon.XmlField(Node, "CommonDataResponse/CommonMandatoryResponse/StatusMsg");
                responseCode = XmlCommon.XmlAttribute(Node.SelectSingleNode("CapResponse/CapMandatoryResponse"), "CapStatus");
            }
            else
            {
                Node = Doc.SelectSingleNode("Response/QuickResponse");
                if (Node != null)
                {
                    authResponse = XmlCommon.XmlField(Node, "ProcStatus")
                        + " - " + XmlCommon.XmlField(Node, "StatusMsg");
                }
            }

            if (rawResponseString.Contains("<AccountNum>"))
            {
                rawResponseString = rawResponseString.Replace(CommonLogic.ExtractToken(rawResponseString, "<AccountNum>", "</AccountNum>"), "****");
            }
            o.CaptureTXResult = rawResponseString;

            if (replyCode == "0" && responseCode == "1")
            {
                result = AppLogic.ro_OK;
            }
            else if (replyCode.Length > 0)
            {
                result = "Transaction was not marked for capture. " + replyCode + " - " + authResponse;
            }
            else if (authResponse.Length > 0)
            {
                result = "Error: " + authResponse;
            }
            else
            {
                result = "System Error: " + rawResponseString;
            }


            return result.Trim();
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            String TxRefIdx = String.Empty;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");

                        // For captured orders we need to look up the TxRefIdx from the original CaptureTXResult.
                        if (DB.RSFieldDateTime(rs, "CapturedOn") > System.DateTime.MinValue)
                        {
                            String captureResult = DB.RSField(rs, "CaptureTXResult");
                            // If we were running in AUTH CAPTURE mode then CaptureTXResult will be empty and our
                            // value will be in AuthorizationResult.
                            if (String.IsNullOrEmpty(captureResult))
                            {
                                captureResult = DB.RSField(rs, "AuthorizationResult");
                            }
                            XmlDocument Doc2 = new XmlDocument();
                            // Zap the DOCTYPE so we don't try to find a corresponding DTD.
                            String t21 = "<!DOCTYPE Response SYSTEM";
                            String t22 = ">";
                            String doctype2 = t21 + CommonLogic.ExtractToken(captureResult, t21, t22) + t22;
                            try
                            {
                                Doc2.LoadXml(captureResult.Replace(doctype2, ""));
                                XmlNode Node2 = Doc2.SelectSingleNode("Response/ACResponse/CommonDataResponse/CommonMandatoryResponse");
                                if (Node2 != null)
                                {
                                    TxRefIdx = XmlCommon.XmlField(Node2, "TxRefIdx");
                                }
                            }
                            catch { }
                        }
                    }                    
                }
            }           

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(5000);
            transactionCommand.Append("<Request>");
            transactionCommand.Append("<Void MessageType=\"V\">");
            transactionCommand.Append("<MerchantID>" + AppLogic.AppConfig("PAYMENTECH_MERCHANT_ID") + "</MerchantID>");
            transactionCommand.Append("<TerminalID>" + AppLogic.AppConfig("PAYMENTECH_MERCHANT_TERMINAL_ID") + "</TerminalID>");
            transactionCommand.Append("<BIN>" + AppLogic.AppConfig("PAYMENTECH_BIN") + "</BIN>");
            transactionCommand.Append("<TxRefNum>" + TransID + "</TxRefNum>");
            if (TxRefIdx.Length > 0)
            {
                transactionCommand.Append("<TxRefIdx>" + TxRefIdx + "</TxRefIdx>");
            }
            else
            {
                transactionCommand.Append("<TxRefIdx />");
            }
            transactionCommand.Append("</Void>");
            transactionCommand.Append("</Request>");

            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OrderNumber.ToString());

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("PAYMENTECH_LIVE_SERVER"), AppLogic.AppConfig("PAYMENTECH_TEST_SERVER"));
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
            myRequest.Headers.Add("MIME-Version", "1.0");
            myRequest.Headers.Add("Request-number", "1");
            myRequest.Headers.Add("Content-transfer-encoding", "text");
            myRequest.Headers.Add("Document-type", "Request");
            myRequest.ContentType = "application/PTI34";
            myRequest.ContentLength = data.Length;
            myRequest.Method = "POST";
            Stream newStream = myRequest.GetRequestStream();
            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            // get the response
            WebResponse myResponse;
            myResponse = myRequest.GetResponse();
            String rawResponseString = String.Empty;
            using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
            {
                rawResponseString = sr.ReadToEnd();
                // Close and clean up the StreamReader
                sr.Close();
            }
            myResponse.Close();

            // rawResponseString now has gateway response
            XmlDocument Doc = new XmlDocument();
            // Zap the DOCTYPE so we don't try to find a corresponding DTD.
            String t1 = "<!DOCTYPE Response SYSTEM";
            String t2 = ">";
            String doctype = t1 + CommonLogic.ExtractToken(rawResponseString, t1, t2) + t2;
            Doc.LoadXml(rawResponseString.Replace(doctype, ""));

            XmlNode Node = Doc.SelectSingleNode("Response/VoidResponse");

            String replyCode = String.Empty;
            String responseCode = String.Empty;
            String authResponse = String.Empty;

            if (Node != null)
            {
                replyCode = XmlCommon.XmlField(Node, "ProcStatus");
                authResponse = replyCode + " - " + XmlCommon.XmlField(Node, "StatusMsg");
            }
            else
            {
                Node = Doc.SelectSingleNode("Response/QuickResponse");
                if (Node != null)
                {
                    authResponse = XmlCommon.XmlField(Node, "ProcStatus")
                        + " - " + XmlCommon.XmlField(Node, "StatusMsg");
                }
            }

            if (rawResponseString.Contains("<AccountNum>"))
            {
                rawResponseString = rawResponseString.Replace(CommonLogic.ExtractToken(rawResponseString, "<AccountNum>", "</AccountNum>"), "****");
            }

            DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OrderNumber.ToString());

            if (replyCode == "0")
            {
                result = AppLogic.ro_OK;
            }
            else if (authResponse.Length > 0)
            {
                result = "Error: " + authResponse;
            }
            else
            {
                result = "System Error: " + rawResponseString;
            }

            return result;
        }

        // if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());

            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            String CardNumber = String.Empty;
            String CardExpM = String.Empty;
            String CardExpY = String.Empty;
            String ApprvCode = String.Empty;
            decimal TotalAmount = 0;
            decimal RefundTotal = 0;
            int OrdNo = CommonLogic.IIF(NewOrderNumber == 0, OriginalOrderNumber, NewOrderNumber);

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        CardNumber = Security.UnmungeString(DB.RSField(rs, "CardNumber"), rs[AppLogic.AppConfig("OrdersCCSaltField")].ToString());
                        if (CardNumber.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                        {
                            CardNumber = DB.RSField(rs, "CardNumber");
                        }
                        CardExpM = DB.RSField(rs, "CardExpirationMonth");
                        CardExpY = DB.RSField(rs, "CardExpirationYear").Substring(2, 2);
                        ApprvCode = DB.RSField(rs, "AuthorizationCode");
                        TotalAmount = DB.RSFieldDecimal(rs, "OrderTotal");
                        //For full refunds a value of zero is passed, so use the order total
                        RefundTotal = CommonLogic.IIF(RefundAmount > 0, RefundAmount, TotalAmount);
                    }
                }
            }
            
            if (CardNumber == AppLogic.ro_CCNotStoredString || CardNumber.Length == 0)
            {
                return "Failed. The credit card number is required and is not stored for this purchase.";
            }

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(5000);

            transactionCommand.Append("<Request>");
            transactionCommand.Append("<Refund POSEntryMode=\"01\" AccountTypeInd=\"91\" TermEntCapInd=\"05\" CATInfoInd=\"06\" TermLocInd=\"01\" CardHolderAttendanceInd=\"01\" CardPresentInd=\"N\" POSConditionCode=\"59\" AttendedTermDataInd=\"01\" FormatInd=\"N\" HcsTcsInd=\"T\" TxCatg=\"7\" MessageType=\"FR\" Version=\"2\" TzCode=\"" + AppLogic.AppConfig("PAYMENTECH_MERCHANT_TZCODE") + "\">");
            transactionCommand.Append("<TxRefIdx />");
            transactionCommand.Append("<Comments>" + XmlCommon.XmlEncode(RefundReason) + "</Comments>");
            transactionCommand.Append("<AccountNum AccountTypeInd=\"91\">" + CardNumber + "</AccountNum>");
            transactionCommand.Append("<MerchantID>" + AppLogic.AppConfig("PAYMENTECH_MERCHANT_ID") + "</MerchantID>");
            transactionCommand.Append("<TerminalID TermEntCapInd=\"05\" CATInfoInd=\"06\" TermLocInd=\"01\" CardPresentInd=\"N\" POSConditionCode=\"59\" AttendedTermDataInd=\"01\">" + AppLogic.AppConfig("PAYMENTECH_MERCHANT_TERMINAL_ID") + "</TerminalID>");
            transactionCommand.Append("<BIN>" + AppLogic.AppConfig("PAYMENTECH_BIN") + "</BIN>");
            transactionCommand.Append("<OrderID>" + OrdNo.ToString().PadRight(16, '0') + "</OrderID>");
            transactionCommand.Append("<Amount>" + Localization.CurrencyStringForGatewayWithoutExchangeRate(CommonLogic.IIF(RefundTotal > TotalAmount, TotalAmount, RefundTotal)).Replace(",", "").Replace(".", "").PadLeft(12, '0') + "</Amount>");
            transactionCommand.Append("<Currency CurrencyCode=\"" + Localization.StoreCurrencyNumericCode() + "\" CurrencyExponent=\"" + CommonLogic.IIF(Localization.StoreCurrencyNumericCode() == "392", "0", "2") + "\"/>");
            transactionCommand.Append("<TxDateTime>" + DateTime.Now.ToString("hhmmssMMyyyy") + "</TxDateTime>");
            transactionCommand.Append("<CardPresence>");
            transactionCommand.Append("<CardNP>");
            transactionCommand.Append("<Exp>" + CardExpM + CardExpY + "</Exp>");
            transactionCommand.Append("</CardNP>");
            transactionCommand.Append("</CardPresence>");
            transactionCommand.Append("<POScardID>4</POScardID>");
            transactionCommand.Append("<EntryDataSrc>2</EntryDataSrc>");
            transactionCommand.Append("</Refund>");
            transactionCommand.Append("</Request>");

            String CardToken = String.Format("<AccountNum AccountTypeInd=\"91\">{0}</AccountNum>", CardNumber);
            String CardTokenReplacement = String.Format("<AccountNum AccountTypeInd=\"91\">{0}</AccountNum>", "x".PadLeft(CardNumber.Length, 'x'));
            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(transactionCommand.ToString().Replace(CardToken, CardTokenReplacement)) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("PAYMENTECH_LIVE_SERVER"), AppLogic.AppConfig("PAYMENTECH_TEST_SERVER"));
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
            myRequest.Headers.Add("MIME-Version", "1.0");
            myRequest.Headers.Add("Request-number", "1");
            myRequest.Headers.Add("Content-transfer-encoding", "text");
            myRequest.Headers.Add("Document-type", "Request");
            myRequest.ContentType = "application/PTI34";
            myRequest.ContentLength = data.Length;
            myRequest.Method = "POST";
            Stream newStream = myRequest.GetRequestStream();
            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            // get the response
            WebResponse myResponse;
            myResponse = myRequest.GetResponse();
            String rawResponseString = String.Empty;
            using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
            {
                rawResponseString = sr.ReadToEnd();
                // Close and clean up the StreamReader
                sr.Close();
            }
            myResponse.Close();

            // rawResponseString now has gateway response
            rawResponseString = rawResponseString.Replace(CardNumber, String.Empty);

            XmlDocument Doc = new XmlDocument();
            // Zap the DOCTYPE so we don't try to find a corresponding DTD.
            String t1 = "<!DOCTYPE Response SYSTEM";
            String t2 = ">";
            String doctype = t1 + CommonLogic.ExtractToken(rawResponseString, t1, t2) + t2;
            Doc.LoadXml(rawResponseString.Replace(doctype, ""));

            XmlNode Node = Doc.SelectSingleNode("Response/RefundResponse");

            String replyCode = String.Empty;
            String responseCode = String.Empty;
            String authResponse = String.Empty;

            if (Node != null)
            {
                replyCode = XmlCommon.XmlField(Node, "ProcStatus");
                authResponse = replyCode + " - " + XmlCommon.XmlField(Node, "StatusMsg");
            }
            else
            {
                Node = Doc.SelectSingleNode("Response/QuickResponse");
                if (Node != null)
                {
                    authResponse = XmlCommon.XmlField(Node, "ProcStatus")
                        + " - " + XmlCommon.XmlField(Node, "StatusMsg");
                }
            }

            DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            if (replyCode == "0")
            {
                result = AppLogic.ro_OK;
            }
            else if (authResponse.Length > 0)
            {
                result = "Error: " + authResponse;
            }
            else
            {
                result = "System Error: " + rawResponseString;
            }

            return result;

        }

        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = AppLogic.ro_OK;

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("<Request>");
            transactionCommand.Append("<AC>");
            transactionCommand.Append("<CommonData>");
            transactionCommand.Append("<CommonMandatory AuthOverrideInd=\"N\" LangInd=\"00\" CardHolderAttendanceInd=\"01\" HcsTcsInd=\"T\" TxCatg=\"7\" MessageType=\"" + CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "A", "AC") + "\" Version=\"2\" TzCode=\"" + AppLogic.AppConfig("PAYMENTECH_MERCHANT_TZCODE") + "\">");
            transactionCommand.Append("<AccountNum AccountTypeInd=\"91\">" + UseBillingAddress.CardNumber + "</AccountNum>");
            transactionCommand.Append("<POSDetails POSEntryMode=\"01\"/>");
            transactionCommand.Append("<MerchantID>" + AppLogic.AppConfig("PAYMENTECH_MERCHANT_ID") + "</MerchantID>");
            transactionCommand.Append("<TerminalID TermEntCapInd=\"05\" CATInfoInd=\"06\" TermLocInd=\"01\" CardPresentInd=\"N\" POSConditionCode=\"59\" AttendedTermDataInd=\"01\">" + AppLogic.AppConfig("PAYMENTECH_MERCHANT_TERMINAL_ID") + "</TerminalID>");
            transactionCommand.Append("<BIN>" + AppLogic.AppConfig("PAYMENTECH_BIN") + "</BIN>");
            transactionCommand.Append("<OrderID>" + OrderNumber.ToString().PadRight(16, '0') + "</OrderID>");
            transactionCommand.Append("<AmountDetails>");
            transactionCommand.Append("<Amount>" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal).Replace(",", "").Replace(".", "").PadLeft(12, '0') + "</Amount>");
            transactionCommand.Append("</AmountDetails>");
            transactionCommand.Append("<TxTypeCommon TxTypeID=\"G\"/>");
            transactionCommand.Append("<Currency CurrencyCode=\"" + Localization.StoreCurrencyNumericCode() + "\" CurrencyExponent=\"" + CommonLogic.IIF(Localization.StoreCurrencyNumericCode() == "392", "0", "2") + "\"/>"); // 392 is Japanese Yen
            transactionCommand.Append("<CardPresence>");
            transactionCommand.Append("<CardNP>");
            transactionCommand.Append("<Exp>" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + UseBillingAddress.CardExpirationYear.ToString().Substring(2, 2) + "</Exp>");
            transactionCommand.Append("</CardNP>");
            transactionCommand.Append("</CardPresence>");
            transactionCommand.Append("<TxDateTime/>");
            transactionCommand.Append("</CommonMandatory>");
            transactionCommand.Append("<CommonOptional>");
            transactionCommand.Append("<Comments>" + XmlCommon.XmlEncode("CustomerID: " + CustomerID.ToString()) + "</Comments>");
            if (CardExtraCode.Trim().Length != 0)
            {
                // The handling of CardSecVal does not match the documentation per request from Paymentech.
                // From Paymentech... November 21, 2006 (Reported under bug 213)
                // "The field is CardSecInd and the value is null. There is an error in the wording 
                // of the interface specification. There are 4 choices for this field 1, 2, 9, and null. 
                // The 1 says value deliberately bypassed or not provided but it really should say value 
                // is present. The 1 is sent for VI and DI only it should be null for AX and MC. So it 
                // is only an issue for the VI transaction. If you have the gateway message templates 
                // the visa/discover credit card ecommerce template shows the correct formatting."
                transactionCommand.Append(CommonLogic.IIF(UseBillingAddress.CardType.Equals("VISA", StringComparison.InvariantCultureIgnoreCase) || UseBillingAddress.CardType.Equals("DISCOVER", StringComparison.InvariantCultureIgnoreCase), "<CardSecVal CardSecInd=\"1\">" + CardExtraCode + "</CardSecVal>", "<CardSecVal CardSecInd=\"\">" + CardExtraCode + "</CardSecVal>"));
            }

            transactionCommand.Append("<ECommerceData ECSecurityInd=\"07\">");
            transactionCommand.Append("<ECOrderNum>" + OrderNumber.ToString().PadRight(16, '0') + "</ECOrderNum>");
            transactionCommand.Append("</ECommerceData>");
            transactionCommand.Append("</CommonOptional>");
            transactionCommand.Append("</CommonData>");

            string xmlName = (UseBillingAddress.FirstName + " " + UseBillingAddress.LastName).Trim();



            transactionCommand.Append("<Auth>");
            transactionCommand.Append("<AuthMandatory FormatInd=\"H\"/>");
            transactionCommand.Append("<AuthOptional>");

            if (AppLogic.AppConfigBool("PAYMENTECH_Verify_Addresses") || AppLogic.AppConfig("PAYMENTECH_Verify_Addresses").Equals("full", StringComparison.InvariantCultureIgnoreCase))
            {
                transactionCommand.Append("<AVSextended>");
                transactionCommand.Append("<AVSname>" + XmlCommon.XmlEncodeMaxLength(xmlName, 30) + "</AVSname>"); // max 30 chars
                transactionCommand.Append("<AVSaddress1>" + XmlCommon.XmlEncodeMaxLength(UseBillingAddress.Address1, 30) + "</AVSaddress1>"); // max 30 chars
                transactionCommand.Append("<AVSaddress2>" + XmlCommon.XmlEncodeMaxLength(UseBillingAddress.Address2, 30) + "</AVSaddress2>"); // max 30 chars
                transactionCommand.Append("<AVScity>" + XmlCommon.XmlEncodeMaxLength(UseBillingAddress.City, 20) + "</AVScity>"); // max 20 chars
                transactionCommand.Append("<AVSstate>" + XmlCommon.XmlEncode(UseBillingAddress.State.Replace("--","")) + "</AVSstate>");
                transactionCommand.Append("<AVSzip>" + XmlCommon.XmlEncode(UseBillingAddress.Zip) + "</AVSzip>");
                transactionCommand.Append("<AVScountryCode>" + AppLogic.GetCountryTwoLetterISOCode(UseBillingAddress.Country) + "</AVScountryCode>");
                transactionCommand.Append("</AVSextended>");
            }
            else
            {
                if (AppLogic.AppConfig("PAYMENTECH_Verify_Addresses").Equals("zip", StringComparison.InvariantCultureIgnoreCase))
                {
                    transactionCommand.Append("<AVSextended>");
                    transactionCommand.Append("<AVSzip>" + XmlCommon.XmlEncode(UseBillingAddress.Zip) + "</AVSzip>");
                    transactionCommand.Append("<AVScountryCode>" + AppLogic.GetCountryTwoLetterISOCode(UseBillingAddress.Country) + "</AVScountryCode>");
                    transactionCommand.Append("</AVSextended>");
                }
            }

            if (CAVV.Trim().Length != 0)
            {
                if (UseBillingAddress.CardType.Equals("VISA", StringComparison.InvariantCultureIgnoreCase) && AppLogic.AppConfigBool("Paymentech.UseVerifiedByVisa"))
                {
                    transactionCommand.Append("<VerifiedByVisa>");
                    transactionCommand.Append("<CAVV>" + CAVV + "</CAVV>");
                    transactionCommand.Append("<XID>" + XID + "</XID>");
                    transactionCommand.Append("</VerifiedByVisa>");
                }
                else if (UseBillingAddress.CardType.Equals("MASTERCARD", StringComparison.InvariantCultureIgnoreCase))
                {
                    transactionCommand.Append("<MCSecureCode><AAV>" + CAVV + "</AAV></MCSecureCode>");
                }
            }

            transactionCommand.Append("</AuthOptional>");
            transactionCommand.Append("</Auth>");

            transactionCommand.Append("<Cap>");
            transactionCommand.Append("<CapMandatory>");
            transactionCommand.Append("<EntryDataSrc>02</EntryDataSrc>");
            transactionCommand.Append("</CapMandatory>");
            transactionCommand.Append("<CapOptional/>");
            transactionCommand.Append("</Cap>");
            transactionCommand.Append("</AC>");
            transactionCommand.Append("</Request>");

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("PAYMENTECH_LIVE_SERVER"), AppLogic.AppConfig("PAYMENTECH_TEST_SERVER"));
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
            myRequest.Headers.Add("MIME-Version", "1.0");
            myRequest.ContentType = "application/PTI34";
            myRequest.ContentLength = data.Length;
            myRequest.Headers.Add("Content-transfer-encoding", "text");
            myRequest.Headers.Add("Request-number", "1");
            myRequest.Headers.Add("Document-type", "Request");
            myRequest.Method = "POST";
            string temp = myRequest.ToString();
            Stream newStream = myRequest.GetRequestStream();
            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            // get the response
            WebResponse myResponse;
            myResponse = myRequest.GetResponse();
            String rawResponseString = String.Empty;
            using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
            {
                rawResponseString = sr.ReadToEnd();
                // Close and clean up the StreamReader
                sr.Close();
            }
            myResponse.Close();

            // rawResponseString now has gateway response
            TransactionResponse = rawResponseString;

            XmlDocument Doc = new XmlDocument();
            // Zap the DOCTYPE so we don't try to find a corresponding DTD.
            String t1 = "<!DOCTYPE Response SYSTEM";
            String t2 = ">";
            String doctype = t1 + CommonLogic.ExtractToken(rawResponseString, t1, t2) + t2;
            Doc.LoadXml(rawResponseString.Replace(doctype, ""));

            XmlNode Node = Doc.SelectSingleNode("Response/ACResponse/CommonDataResponse/CommonMandatoryResponse");

            String replyCode = String.Empty;
            String responseCode = String.Empty;
            String approvalCode = String.Empty;
            String AVSCode = String.Empty;
            String CVCode = String.Empty;
            String authResponse = String.Empty;
            String TransID = String.Empty;

            if (Node != null)
            {
                replyCode = XmlCommon.XmlField(Node, "ApprovalStatus");
                responseCode = XmlCommon.XmlField(Node, "ResponseCodes/RespCode");
                approvalCode = XmlCommon.XmlField(Node, "ResponseCodes/AuthCode");
                AVSCode = XmlCommon.XmlField(Node, "ResponseCodes/AVSRespCode");
                CVCode = XmlCommon.XmlField(Node, "ResponseCodes/CVV2RespCode");
                authResponse = XmlCommon.XmlField(Node, "StatusMsg");
                TransID = XmlCommon.XmlField(Node, "TxRefNum");
            }
            else
            {
                Node = Doc.SelectSingleNode("Response/QuickResponse");
                if (Node != null)
                {
                    authResponse = XmlCommon.XmlField(Node, "ProcStatus") 
                        + " - " + XmlCommon.XmlField(Node, "StatusMsg");
                }
            }

            AuthorizationCode = approvalCode;
            AuthorizationResult = rawResponseString;
            AuthorizationTransID = TransID;
            AVSResult = AVSCode;
            if (CVCode.Length > 0)
            {
                AVSResult += ", CV Result: " + CVCode;
            }
            TransactionCommandOut = transactionCommand.ToString();

            if (replyCode == "1")
            {
                result = AppLogic.ro_OK;
            }
            else if (replyCode == "0")
            {
                result = "DECLINED";
                if (authResponse.Length > 0)
                {
                    result += ". " + authResponse;
                }
            }
            else if (authResponse.Length > 0)
            {
                result = "Error: " + authResponse;
            }
            else
            {
                result = "System Error: " + rawResponseString;
            }

            return result;
        }

        public override bool RequiresCCForFurtherProcessing()
        {
            return true;
        }
    }
}
