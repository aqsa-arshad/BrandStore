// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Moneris eSELECTplus
    /// http://www.moneris.com/
    /// Supports Canadian and United States merchants using the XML API. (AppConfig eSelectPlus.country)
    /// Supports 3D Secure Payer Authentication using the MPI API. (AppConfig 3DSECURE.CreditCardTypeIDs)
    /// </summary>
    public class Moneris : GatewayProcessor
    {
        protected const string cryptLabel = "Crypt: ";

        public Moneris() { }

        public override String CaptureOrder(Order o)
        {
            String result = "error";

            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";

            string txn_number = o.AuthorizationPNREF;
            Decimal OrderTotal = o.OrderBalance;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            string crypt = AppLogic.AppConfig("eSelectPlus.crypt");
            if (o.AuthorizationCode.Contains(cryptLabel))
            {
                crypt = o.AuthorizationCode.Substring(o.AuthorizationCode.IndexOf(cryptLabel) + cryptLabel.Length, 1);
            }
            StringBuilder transactionCommand = new StringBuilder(4096);

            String sAmount = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);
            if (sAmount.StartsWith("."))
            {
                sAmount = "0" + sAmount;
            }

            transactionCommand.Append("x_type=PRIOR_AUTH_CAPTURE");
            transactionCommand.Append("&x_amount=" + sAmount);

            esp.completion reqCompletion = new esp.completion();
            reqCompletion.order_id = o.OrderNumber.ToString();
            reqCompletion.comp_amount = sAmount;
            reqCompletion.txn_number = txn_number;
            reqCompletion.crypt_type = crypt;

            try
            {
                string sResponse = sendRequest(useLiveTransactions, reqCompletion);

                esp.response resp = null;
                if (sResponse != null)
                {
                    resp = DeserializeResponse(sResponse);
                }

                if (resp != null)
                {
                    esp.receipt respReceipt = (esp.receipt)resp.receipt[0];
                    o.CaptureTXCommand = transactionCommand.ToString();
                    o.CaptureTXResult = respReceipt.Complete;
                    if (respReceipt.Complete == "true")
                    {
                        o.AuthorizationPNREF = respReceipt.TransID;
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = respReceipt.Message;
                    }
                }
            }
            catch
            {
                result = "failed";
            }
            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = "error";

            string crypt = AppLogic.AppConfig("eSelectPlus.crypt");
            string txn_number = string.Empty;
            string authCode = string.Empty;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());

            using (SqlConnection connn = DB.dbConn())
            {
                connn.Open();
                using (IDataReader rsv = DB.GetRS("select AuthorizationPNREF, AuthorizationCode from Orders where OrderNumber=" + OrderNumber.ToString(), connn))
                {
                    if (rsv.Read())
                    {
                        txn_number = DB.RSField(rsv, "AuthorizationPNREF");
                        authCode = DB.RSField(rsv, "AuthorizationCode");
                    }
                }
            }

            if (authCode.Contains(cryptLabel))
            {
                crypt = authCode.Substring(authCode.IndexOf(cryptLabel) + cryptLabel.Length, 1);
            }

            esp.purchasecorrection reqCorrection = new esp.purchasecorrection();
            reqCorrection.order_id = OrderNumber.ToString();
            reqCorrection.txn_number = txn_number;
            reqCorrection.crypt_type = crypt;

            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("x_type=VOID");
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());
            transactionCommand.Append("&x_trans_id=" + txn_number);

            try
            {
                string sResponse = sendRequest(useLiveTransactions, reqCorrection);

                esp.response resp = null;
                if (sResponse != null)
                {
                    resp = DeserializeResponse(sResponse);
                }

                if (resp != null)
                {
                    esp.receipt respReceipt = (esp.receipt)resp.receipt[0];
                    StringBuilder tps = new StringBuilder("");
                    tps.Append("update orders set ");
                    tps.Append("VoidTXResult=" + DB.SQuote(respReceipt.Complete) + ",");
                    tps.Append("VoidTXCommand=" + DB.SQuote(transactionCommand.ToString()));
                    tps.Append(" where ordernumber=" + OrderNumber.ToString());
                    DB.ExecuteSQL(tps.ToString());
                    if (respReceipt.Complete == "true")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = respReceipt.Message;
                    }
                }
            }
            catch
            {
                result = "failed";
            }
            return result;
        }

        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundOrderTotal, String RefundReason, Address UseBillingAddress)
        {
            String result = "error";
            string crypt = AppLogic.AppConfig("eSelectPlus.crypt");
            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            String Last4 = String.Empty;
            int CustomerID = 0;
            Decimal OrderTotal = System.Decimal.Zero;
            string authCode = string.Empty;

            string theOrderTotal = string.Empty;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        Last4 = DB.RSField(rs, "Last4");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                        CustomerID = DB.RSFieldInt(rs, "CustomerID");
                        authCode = DB.RSField(rs, "AuthorizationCode");
                    }
                }
            }

            if (authCode.Contains(cryptLabel))
            {
                crypt = authCode.Substring(authCode.IndexOf(cryptLabel) + cryptLabel.Length, 1);
            }

            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("x_type=CREDIT");
            transactionCommand.Append("&x_trans_id=" + TransID);
            if (RefundOrderTotal == System.Decimal.Zero)
            {
                theOrderTotal = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);
            }
            else
            {
                theOrderTotal = Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundOrderTotal);
            }

            if (theOrderTotal.StartsWith("."))
            {
                theOrderTotal = "0" + theOrderTotal;
            }

            transactionCommand.Append("&x_amount=" + theOrderTotal);
            transactionCommand.Append("&x_cust_id=" + CustomerID.ToString());
            transactionCommand.Append("&x_invoice_num=" + OriginalOrderNumber.ToString());
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());
            transactionCommand.Append("&x_card_num=" + Last4);
            transactionCommand.Append("&x_description=" + HttpContext.Current.Server.UrlEncode(RefundReason));

            esp.refundTxn reqRefund = new esp.refundTxn();
            reqRefund.order_id = OriginalOrderNumber.ToString();
            reqRefund.amount = theOrderTotal;
            reqRefund.txn_number = TransID;
            reqRefund.crypt_type = crypt;

            try
            {
                string sResponse = sendRequest(useLiveTransactions, reqRefund);

                esp.response resp = null;
                if (sResponse != null)
                {
                    resp = DeserializeResponse(sResponse);
                }

                if (resp != null)
                {
                    esp.receipt respReceipt = (esp.receipt)resp.receipt[0];
                    StringBuilder tps = new StringBuilder("");
                    tps.Append("update orders set ");
                    tps.Append("RefundTXCommand=" + DB.SQuote(transactionCommand.ToString()) + ",");
                    tps.Append("RefundTXResult=" + DB.SQuote(respReceipt.Complete) + ",");
                    tps.Append("RefundReason=" + DB.SQuote(RefundReason.ToString()));
                    tps.Append(" where ordernumber=" + OriginalOrderNumber.ToString());
                    DB.ExecuteSQL(tps.ToString());
                    if (respReceipt.Complete == "true")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = respReceipt.Message;
                    }
                }
            }
            catch
            {
                result = "failed";
            }
            return result;
        }

        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = "Error";
            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            AVSResult = String.Empty;
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;

            String sOrderNumber = OrderNumber.ToString();
            if (AppLogic.AppConfigBool("eSelectPlus.randomizeOrderNumberForTesting") && !useLiveTransactions)
            {
                Random r = new Random();
                sOrderNumber = "r" + r.Next(0, 9999999);
            }

            String crypt = AppLogic.AppConfig("eSelectPlus.crypt");
            String sAmount = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);
            if (sAmount.StartsWith("."))
            {
                sAmount = "0" + sAmount;
            }

            String signedPARes = String.Empty;
            CustomerSession cSession = new CustomerSession(CustomerID);

            if (cSession["3Dsecure.PaRes"].Length != 0)
            {
                signedPARes = cSession["3Dsecure.PaRes"];
                // After grabbing it, clear out the session PaRes so it won't be re-used ever again.
                cSession["3Dsecure.PaRes"] = String.Empty;
            }

            String CardNumber = UseBillingAddress.CardNumber.Trim();
            String expire_date = CommonLogic.IIF(UseBillingAddress.CardExpirationYear.PadLeft(2, '0').Length > 2, UseBillingAddress.CardExpirationYear.PadLeft(2, '0').Substring(2, 2), UseBillingAddress.CardExpirationYear.PadLeft(2, '0')) + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0');

            int CardTypeID = DB.GetSqlN("select CardTypeID N from CreditCardType where CardType = " + DB.SQuote(UseBillingAddress.CardType));
            bool Try3DSecure = CommonLogic.IntegerIsInIntegerList(CardTypeID, AppLogic.AppConfig("3DSECURE.CreditCardTypeIDs"));

            if (Try3DSecure)
            {
                if (signedPARes == String.Empty)
                {
                    // check for enrollment
                    string reqXID = sOrderNumber.PadLeft(20, '0'); // must be 20 chars
                    esp.txn reqTxn = new esp.txn();
                    reqTxn.xid = reqXID;
                    reqTxn.amount = sAmount;
                    reqTxn.pan = CardNumber;
                    reqTxn.expdate = expire_date;
                    reqTxn.MD = sOrderNumber;
                    reqTxn.merchantUrl = AppLogic.GetStoreHTTPLocation(true) + "secureprocess.aspx";
                    reqTxn.accept = CommonLogic.ServerVariables("HTTP_ACCEPT");
                    reqTxn.userAgent = CommonLogic.ServerVariables("HTTP_USER_AGENT");

                    string resultMPI = sendRequestMPI(useLiveTransactions, reqTxn);

                    esp.MpiResponse respMPI = null;
                    if (resultMPI != null)
                    {
                        respMPI = DeserializeResponseMPI(resultMPI);
                    }

                    if (respMPI != null)
                    {
                        TransactionResponse = XmlCommon.PrettyPrintXml(resultMPI);
                        if (respMPI.message == "Y")
                        {
                            // enrolled, must authenticate
                            cSession["3DSecure.CustomerID"] = CustomerID.ToString();
                            cSession["3DSecure.OrderNumber"] = sOrderNumber;
                            cSession["3DSecure.MD"] = sOrderNumber;
                            cSession["3DSecure.ACSUrl"] = respMPI.ACSUrl;
                            cSession["3DSecure.paReq"] = respMPI.PaReq;
                            cSession["3DSecure.XID"] = reqXID;
                            cSession.UpdateCustomerSession(null, null);
                            result = AppLogic.ro_3DSecure; // This is what triggers the 3D Secure IFRAME to be used.
                            return result;
                        }
                        else if (respMPI.message == "N")
                        {
                            // not enrolled
                            crypt = "6";
                        }
                        else if (respMPI.message == "U")
                        {
                            // non-participating card type
                            crypt = "7";
                        }
                    }
                }
                else
                {
                    // this is round two for authenticated buyers
                    esp.acs reqACS = new esp.acs();
                    reqACS.MD = sOrderNumber;
                    reqACS.PaRes = signedPARes;

                    string resultMPI = sendRequestMPI(useLiveTransactions, reqACS);

                    esp.MpiResponse respMPI = null;
                    if (resultMPI != null)
                    {
                        respMPI = DeserializeResponseMPI(resultMPI);
                    }

                    if (respMPI != null)
                    {
                        TransactionResponse = XmlCommon.PrettyPrintXml(resultMPI);
                        if (respMPI.message == "Y")
                        {
                            // fully authenticated
                            CAVV = respMPI.cavv;
                            crypt = "5";
                        }
                        else if (respMPI.message == "A")
                        {
                            // attempted to verify
                            CAVV = respMPI.cavv;
                            crypt = "6";
                        }
                        else if (respMPI.message == "N")
                        {
                            // failed to verify
                            return AppLogic.GetString("gw.moneris.3DSfailed", UseBillingAddress.SkinID, UseBillingAddress.LocaleSetting);
                        }

                        if (CAVV.Length != 0)
                        {
                            // encode it to store in the session, it will be decoded before being saved to the database
                            byte[] str = Encoding.UTF8.GetBytes("MPI=" + respMPI.message + ", CAVV=" + CAVV);
                            cSession["3DSecure.LookupResult"] = Convert.ToBase64String(str);
                        }
                    }
                }
            }

            esp.billing bill = new AspDotNetStorefrontGateways.Processors.esp.billing();
            bill.first_name = UseBillingAddress.FirstName;
            bill.last_name = UseBillingAddress.LastName;
            bill.company_name = UseBillingAddress.Company;
            bill.address = UseBillingAddress.Address1;
            bill.city = UseBillingAddress.City;
            bill.province = UseBillingAddress.State;
            bill.postal_code = UseBillingAddress.Zip;
            bill.country = UseBillingAddress.Country;
            bill.phone_number = UseBillingAddress.Phone;

            esp.shipping ship = null;
            if (UseShippingAddress != null)
            {
                ship = new esp.shipping();
                ship.first_name = UseShippingAddress.FirstName;
                ship.last_name = UseShippingAddress.LastName;
                ship.company_name = UseShippingAddress.Company;
                ship.address = UseShippingAddress.Address1;
                ship.city = UseShippingAddress.City;
                ship.province = UseShippingAddress.State;
                ship.postal_code = UseShippingAddress.Zip;
                ship.country = UseShippingAddress.Country;
                ship.phone_number = UseShippingAddress.Phone;
            }

            esp.cust_info cst = new esp.cust_info();
            cst.billing = bill;
            cst.shipping = ship;
            cst.email = UseBillingAddress.EMail;

            esp.avs_info avs = null;
            if (AppLogic.AppConfigBool("eSelectPlus.includeAVS"))
            {
                avs = new esp.avs_info();
                int iSpace = UseBillingAddress.Address1.IndexOf(" ");
                if (iSpace > 0)
                {
                    avs.avs_street_number = UseBillingAddress.Address1.Substring(0, iSpace);
                    avs.avs_street_name = UseBillingAddress.Address1.Substring(iSpace + 1);
                }
                else
                {
                    avs.avs_street_name = UseBillingAddress.Address1;
                }
                avs.avs_zipcode = UseBillingAddress.Zip;
            }

            esp.cvd_info cvd = null;
            if (CardExtraCode.Trim().Length != 0)
            {
                cvd = new esp.cvd_info();
                cvd.cvd_indicator = "1";
                cvd.cvd_value = CardExtraCode.Trim();
            }

            object oReqItem = null;



            if (TransactionMode == TransactionModeEnum.auth)
            {
                if (CAVV.Length != 0)
                {
                    esp.cavv_preauth cavvPreAuth = new esp.cavv_preauth();
                    cavvPreAuth.order_id = sOrderNumber;
                    cavvPreAuth.cust_id = CustomerID.ToString();
                    cavvPreAuth.amount = sAmount;
                    cavvPreAuth.pan = CardNumber;
                    cavvPreAuth.expdate = expire_date;
                    cavvPreAuth.cavv = CAVV;
                    cavvPreAuth.avs_info = avs;
                    cavvPreAuth.cvd_info = cvd;
                    cavvPreAuth.cust_info = cst;

                    oReqItem = cavvPreAuth;
                }
                else
                {
                    esp.preauth preAuthTxn = new esp.preauth();
                    preAuthTxn.order_id = sOrderNumber;
                    preAuthTxn.cust_id = CustomerID.ToString();
                    preAuthTxn.amount = sAmount;
                    preAuthTxn.pan = CardNumber;
                    preAuthTxn.expdate = expire_date;
                    preAuthTxn.avs_info = avs;
                    preAuthTxn.cvd_info = cvd;
                    preAuthTxn.cust_info = cst;
                    preAuthTxn.crypt_type = crypt;

                    oReqItem = preAuthTxn;
                }
            }
            else
            {
                if (CAVV.Length != 0)
                {
                    esp.cavv_purchase cavvPurchase = new esp.cavv_purchase();
                    cavvPurchase.order_id = sOrderNumber;
                    cavvPurchase.cust_id = CustomerID.ToString();
                    cavvPurchase.amount = sAmount;
                    cavvPurchase.pan = CardNumber;
                    cavvPurchase.expdate = expire_date;
                    cavvPurchase.cavv = CAVV;
                    cavvPurchase.avs_info = avs;
                    cavvPurchase.cvd_info = cvd;
                    cavvPurchase.cust_info = cst;

                    oReqItem = cavvPurchase;
                }
                else
                {
                    esp.purchaseTxn purchaseTxn = new esp.purchaseTxn();
                    purchaseTxn.order_id = sOrderNumber;
                    purchaseTxn.cust_id = CustomerID.ToString();
                    purchaseTxn.amount = sAmount;
                    purchaseTxn.pan = CardNumber;
                    purchaseTxn.expdate = expire_date;
                    purchaseTxn.crypt_type = crypt;
                    purchaseTxn.avs_info = avs;
                    purchaseTxn.cvd_info = cvd;
                    purchaseTxn.cust_info = cst;

                    oReqItem = purchaseTxn;
                }
            }

            try
            {
                string sResponse = sendRequest(useLiveTransactions, oReqItem);

                esp.response resp = null;
                if (sResponse != null)
                {
                    resp = DeserializeResponse(sResponse);

                    if (resp != null)
                    {
                        TransactionResponse = XmlCommon.PrettyPrintXml(sResponse);
                        esp.receipt respReceipt = (esp.receipt)resp.receipt[0];
                        result = respReceipt.Message;
                        AuthorizationCode = respReceipt.AuthCode;
                        if (AuthorizationCode == null)
                        {
                            AuthorizationCode = String.Empty;
                        }
                        try
                        {
                            AuthorizationResult = (System.Int32.Parse(respReceipt.ResponseCode).ToString());
                            if (AuthorizationResult == null)
                            {
                                AuthorizationResult = String.Empty;
                            }
                        }
                        catch
                        {
                            AuthorizationResult = String.Empty;
                        }
                        AuthorizationTransID = respReceipt.TransID;
                        if (AuthorizationTransID == null || AuthorizationTransID == "null")
                        {
                            AuthorizationTransID = String.Empty;
                        }
                        AVSResult = respReceipt.AvsResultCode;
                        if (AVSResult == null || AVSResult == "null")
                        {
                            AVSResult = String.Empty;
                        }
                        if (respReceipt.CvdResultCode != null && respReceipt.CvdResultCode != "null")
                        {
                            AVSResult += ", CVD Result: " + respReceipt.CvdResultCode;
                        }
                        if (AuthorizationResult != String.Empty && System.Int32.Parse(AuthorizationResult) < 50)
                        {
                            result = AppLogic.ro_OK;

                            if (crypt != AppLogic.AppConfig("eSelectPlus.crypt"))
                            {
                                // we need to store the crypt if it is not the default value
                                // this gets parsed elsewhere so don't modify unless you know what you are doing
                                AuthorizationCode += ", " + cryptLabel + crypt;
                            }
                        }
                        else
                        {
                            result = AppLogic.GetString("gw.moneris.code." + AuthorizationResult, UseBillingAddress.SkinID, UseBillingAddress.LocaleSetting);
                            if (result == "gw.moneris.code." + AuthorizationResult)
                            {
                                result = AppLogic.GetString("gw.moneris.cardfailed", UseBillingAddress.SkinID, UseBillingAddress.LocaleSetting);
                            }
                        }
                    }
                    else
                    {
                        // failed to Deserialize
                        result = AppLogic.GetString("gw.moneris.parsefailure", UseBillingAddress.SkinID, UseBillingAddress.LocaleSetting);
                    }
                }
                else
                {
                    // failed to communicate
                    result = AppLogic.GetString("gw.moneris.commsfailure", UseBillingAddress.SkinID, UseBillingAddress.LocaleSetting);
                    return result;
                }
            }
            catch
            {
                result = AppLogic.GetString("gw.moneris.exception", UseBillingAddress.SkinID, UseBillingAddress.LocaleSetting);
            }

            if (result != AppLogic.ro_OK && TransactionResponse.Length == 0)
            {
                TransactionResponse = result;
            }
            return result;
        }

        private static string sendRequestMPI(bool live, object oReqItem)
        {
            string cntry = AppLogic.AppConfig("eSelectPlus.country").ToUpperInvariant().Trim();
            string mode = CommonLogic.IIF(live, "Live", "Test");
            string url = AppLogic.AppConfig("eSelectPlus.URL." + cntry + "." + mode + "MPI"); //eSelectPlus.URL.CA.TestMPI

            esp.MpiRequest req = new esp.MpiRequest();
            req.store_id = AppLogic.AppConfig("eSelectPlus.store_id." + mode);
            req.api_token = AppLogic.AppConfig("eSelectPlus.api_token." + mode);

            object[] reqItems = new object[1];
            reqItems[0] = oReqItem;
            req.Items = reqItems;

            return sendXMLRequest(req, url, cntry);
        }
        private static String sendRequest(bool live, object oReqItem)
        {
            string cntry = AppLogic.AppConfig("eSelectPlus.country").ToUpperInvariant().Trim();
            string mode = CommonLogic.IIF(live, "Live", "Test");
            string url = AppLogic.AppConfig("eSelectPlus.URL." + cntry + "." + mode); //eSelectPlus.URL.CA.Test

            esp.request req = new esp.request();
            req.store_id = AppLogic.AppConfig("eSelectPlus.store_id." + mode);
            req.api_token = AppLogic.AppConfig("eSelectPlus.api_token." + mode);

            object[] reqItems = new object[1];
            reqItems[0] = oReqItem;
            req.Items = reqItems;

            return sendXMLRequest(req, url, cntry);
        }
        private static String sendXMLRequest(Object oRequest, string url, string Country)
        {
            MemoryStream memoryStream = new MemoryStream();
            byte[] data = null;

            String XmlizedString = String.Empty;
            XmlSerializer xs;
            if ("US" == Country)
            {
                // Set up US ElementName overrides
                XmlAttributeOverrides myOverrides = new XmlAttributeOverrides();
                XmlAttributes myAttributes = new XmlAttributes();

                myAttributes.XmlElements.Add(new XmlElementAttribute("us_cavv_preauth", typeof(esp.cavv_preauth)));
                myAttributes.XmlElements.Add(new XmlElementAttribute("us_cavv_purchase", typeof(esp.cavv_purchase)));
                myAttributes.XmlElements.Add(new XmlElementAttribute("us_completion", typeof(esp.completion)));
                myAttributes.XmlElements.Add(new XmlElementAttribute("us_preauth", typeof(esp.preauth)));
                myAttributes.XmlElements.Add(new XmlElementAttribute("us_purchase", typeof(esp.purchaseTxn)));
                myAttributes.XmlElements.Add(new XmlElementAttribute("us_purchasecorrection", typeof(esp.purchasecorrection)));
                myAttributes.XmlElements.Add(new XmlElementAttribute("us_refund", typeof(esp.refundTxn)));

                myOverrides.Add(typeof(esp.request), "Items", myAttributes);
                xs = new XmlSerializer(oRequest.GetType(), myOverrides);
            }
            else
            {
                xs = new XmlSerializer(oRequest.GetType());
            }
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, new UTF8Encoding(false));
            xs.Serialize(xmlTextWriter, oRequest);
            memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
            UTF8Encoding encoding = new UTF8Encoding(false);
            XmlizedString = encoding.GetString(memoryStream.ToArray());
            memoryStream.Dispose();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(XmlizedString);
            foreach (XmlNode child in xmlDoc.ChildNodes)
            {
                CleanXmlNode(child);
            }
            XmlizedString = XmlCommon.FormatXml(xmlDoc);
            data = encoding.GetBytes(XmlizedString);

            // Set-up the HttpWebRequest object
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/xml; charset=UTF-8";
            webRequest.Accept = "application/xml; charset=UTF-8";

            string strResponse = null;

            // Transmit the request
            try
            {
                webRequest.ContentLength = data.Length;
                Stream requestStream = webRequest.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();

                // get the response
                WebResponse webResponse = null;
                webResponse = webRequest.GetResponse();
                using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    strResponse = sr.ReadToEnd();
                    sr.Close();
                }
                webResponse.Close();
            }
            catch { }
            return strResponse;
        }

        private static void CleanXmlNode(XmlNode parent)
        {
            // We need to clear the IsEmpty flag on each element so that we get a separate
            // closing tag instead of a self-closed tag.
            if (parent.HasChildNodes)
            {
                foreach (XmlNode child in parent.ChildNodes)
                {
                    CleanXmlNode(child);
                }
            }
            else
            {
                if (parent.NodeType == XmlNodeType.Element)
                {
                    (parent as XmlElement).IsEmpty = false;
                }
            }
        }

        private static esp.MpiResponse DeserializeResponseMPI(String xmlData)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(esp.MpiResponse));
                UTF8Encoding encoding = new UTF8Encoding(false);
                Byte[] byteArray = encoding.GetBytes(xmlData);
                MemoryStream memoryStream = new MemoryStream(byteArray);
                XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
                return (esp.MpiResponse)xs.Deserialize(memoryStream);
            }
            catch { }
            return null;

        }

        private static esp.response DeserializeResponse(String xmlData)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(esp.response));
                UTF8Encoding enc = new UTF8Encoding(false);
                Byte[] byteArray = enc.GetBytes(xmlData);
                MemoryStream memoryStream = new MemoryStream(byteArray);
                XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
                return (esp.response)xs.Deserialize(memoryStream);
            }
            catch { }
            return null;
        }
    }

    namespace esp
    {
        #region Request Objects

        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute("request", Namespace = "", IsNullable = false)]
        public partial class request
        {
            [XmlElement("store_id")]
            public string store_id;
            [XmlElement("api_token")]
            public string api_token;

            private object[] itemsField;

            [XmlElement("cavv_preauth", typeof(cavv_preauth))]
            [XmlElement("cavv_purchase", typeof(cavv_purchase))]
            [XmlElement("completion", typeof(completion))]
            [XmlElement("preauth", typeof(preauth))]
            [XmlElement("purchase", typeof(purchaseTxn))]
            [XmlElement("purchasecorrection", typeof(purchasecorrection))]
            [XmlElement("refund", typeof(refundTxn))]
            public object[] Items
            {
                get
                {
                    return this.itemsField;
                }
                set
                {
                    this.itemsField = value;
                }
            }
        }

        public partial class cust_info
        {
            [XmlElement("billing")]
            public billing billing;
            [XmlElement("shipping")]
            public shipping shipping;
            [XmlElement("email")]
            public string email = "";
            [XmlElement("instructions")]
            public string instructions = "";
            [XmlElement("item")]
            public item[] item = new item[1];

            public cust_info()
            {
                item[0] = new item();
            }
        }

        public partial class billing
        {
            [XmlElement("first_name")]
            public string first_name = "";
            [XmlElement("last_name")]
            public string last_name = "";
            [XmlElement("company_name")]
            public string company_name = "";
            [XmlElement("address")]
            public string address = "";
            [XmlElement("city")]
            public string city = "";
            [XmlElement("province")]
            public string province = "";
            [XmlElement("postal_code")]
            public string postal_code = "";
            [XmlElement("country")]
            public string country = "";
            [XmlElement("phone_number")]
            public string phone_number = "";
            [XmlElement("fax")]
            public string fax = "";
            [XmlElement("tax1")]
            public string tax1 = "";
            [XmlElement("tax2")]
            public string tax2 = "";
            [XmlElement("tax3")]
            public string tax3 = "";
            [XmlElement("shipping_cost")]
            public string shipping_cost = "";
        }

        public partial class shipping
        {
            [XmlElement("first_name")]
            public string first_name = "";
            [XmlElement("last_name")]
            public string last_name = "";
            [XmlElement("company_name")]
            public string company_name = "";
            [XmlElement("address")]
            public string address = "";
            [XmlElement("city")]
            public string city = "";
            [XmlElement("province")]
            public string province = "";
            [XmlElement("postal_code")]
            public string postal_code = "";
            [XmlElement("country")]
            public string country = "";
            [XmlElement("phone_number")]
            public string phone_number = "";
            [XmlElement("fax")]
            public string fax = "";
            [XmlElement("tax1")]
            public string tax1 = "";
            [XmlElement("tax2")]
            public string tax2 = "";
            [XmlElement("tax3")]
            public string tax3 = "";
            [XmlElement("shipping_cost")]
            public string shipping_cost = "";
        }

        public partial class purchaseTxn
        {
            [XmlElement("order_id")]
            public string order_id;
            [XmlElement("cust_id")]
            public string cust_id;
            [XmlElement("amount")]
            public string amount;
            [XmlElement("pan")]
            public string pan;
            [XmlElement("expdate")]
            public string expdate;
            [XmlElement("crypt_type")]
            public string crypt_type;
            [XmlElement("cust_info")]
            public cust_info cust_info;
            [XmlElement("avs_info")]
            public avs_info avs_info;
            [XmlElement("cvd_info")]
            public cvd_info cvd_info;
        }

        public partial class item
        {
            [XmlElement("name")]
            public string name = "";
            [XmlElement("quantity")]
            public string quantity = "";
            [XmlElement("product_code")]
            public string product_code = "";
            [XmlElement("extended_amount")]
            public string extended_amount = "";
        }

        public partial class avs_info
        {
            [XmlElement("avs_street_number")]
            public string avs_street_number;
            [XmlElement("avs_street_name")]
            public string avs_street_name;
            [XmlElement("avs_zipcode")]
            public string avs_zipcode;
        }

        public partial class cvd_info
        {
            [XmlElement("cvd_indicator")]
            public string cvd_indicator;
            [XmlElement("cvd_value")]
            public string cvd_value;
        }

        public partial class cavv_preauth
        {
            [XmlElement("order_id")]
            public string order_id;
            [XmlElement("cust_id")]
            public string cust_id;
            [XmlElement("amount")]
            public string amount;
            [XmlElement("pan")]
            public string pan;
            [XmlElement("expdate")]
            public string expdate;
            [XmlElement("cavv")]
            public string cavv;
            [XmlElement("cust_info")]
            public cust_info cust_info;
            [XmlElement("avs_info")]
            public avs_info avs_info;
            [XmlElement("cvd_info")]
            public cvd_info cvd_info;
        }

        public partial class cavv_purchase
        {
            [XmlElement("order_id")]
            public string order_id;
            [XmlElement("cust_id")]
            public string cust_id;
            [XmlElement("amount")]
            public string amount;
            [XmlElement("pan")]
            public string pan;
            [XmlElement("expdate")]
            public string expdate;
            [XmlElement("cavv")]
            public string cavv;
            [XmlElement("cust_info")]
            public cust_info cust_info;
            [XmlElement("avs_info")]
            public avs_info avs_info;
            [XmlElement("cvd_info")]
            public cvd_info cvd_info;
        }

        public partial class completion
        {
            [XmlElement("order_id")]
            public string order_id;
            [XmlElement("comp_amount")]
            public string comp_amount;
            [XmlElement("txn_number")]
            public string txn_number;
            [XmlElement("crypt_type")]
            public string crypt_type;
        }

        public partial class preauth
        {
            [XmlElement("order_id")]
            public string order_id;
            [XmlElement("cust_id")]
            public string cust_id;
            [XmlElement("amount")]
            public string amount;
            [XmlElement("pan")]
            public string pan;
            [XmlElement("expdate")]
            public string expdate;
            [XmlElement("crypt_type")]
            public string crypt_type;
            [XmlElement("cust_info")]
            public cust_info cust_info;
            [XmlElement("avs_info")]
            public avs_info avs_info;
            [XmlElement("cvd_info")]
            public cvd_info cvd_info;
        }

        public partial class purchasecorrection
        {
            [XmlElement("order_id")]
            public string order_id;
            [XmlElement("txn_number")]
            public string txn_number;
            [XmlElement("crypt_type")]
            public string crypt_type;
        }

        public partial class refundTxn
        {
            [XmlElement("order_id")]
            public string order_id;
            [XmlElement("amount")]
            public string amount;
            [XmlElement("txn_number")]
            public string txn_number;
            [XmlElement("crypt_type")]
            public string crypt_type;
        }

        #endregion

        #region Response Objects

        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute("response", Namespace = "", IsNullable = false)]
        public partial class response
        {

            private receipt[] receiptField;

            [XmlElement("receipt")]
            public receipt[] receipt
            {
                get
                {
                    return this.receiptField;
                }
                set
                {
                    this.receiptField = value;
                }
            }
        }

        public partial class receipt
        {
            [XmlElement("ReceiptId")]
            public string ReceiptId;
            [XmlElement("ReferenceNum")]
            public string ReferenceNum;
            [XmlElement("ResponseCode")]
            public string ResponseCode;
            [XmlElement("ISO")]
            public string ISO;
            [XmlElement("AuthCode")]
            public string AuthCode;
            [XmlElement("TransTime")]
            public string TransTime;
            [XmlElement("TransDate")]
            public string TransDate;
            [XmlElement("TransType")]
            public string TransType;
            [XmlElement("Complete")]
            public string Complete;
            [XmlElement("Message")]
            public string Message;
            [XmlElement("TransAmount")]
            public string TransAmount;
            [XmlElement("CardType")]
            public string CardType;
            [XmlElement("TransID")]
            public string TransID;
            [XmlElement("TimedOut")]
            public string TimedOut;
            [XmlElement("BankTotals")]
            public BankTotals BankTotals;
            [XmlElement("Ticket")]
            public string Ticket;
            [XmlElement("CvdResultCode")]
            public string CvdResultCode;
            [XmlElement("AvsResultCode")]
            public string AvsResultCode;
            [XmlElement("RecurSuccess")]
            public string RecurSuccess;

        }

        public partial class BankTotals
        {
            [XmlElement("ECR")]
            public ECR ECR;
        }

        public partial class ECR
        {
            [XmlElement("term_id")]
            public string term_id;
            [XmlElement("closed")]
            public string closed;

            private Card[] cardField;

            [XmlElement("Card")]
            public Card[] Card
            {
                get
                {
                    return this.cardField;
                }
                set
                {
                    this.cardField = value;
                }
            }
        }

        public partial class Card
        {
            [XmlElement("cardType")]
            public string cardType;

            private object[] itemsField;

            [XmlElement("Correction", typeof(Correction))]
            [XmlElement("Purchase", typeof(Purchase))]
            [XmlElement("Refund", typeof(Refund))]
            public object[] Items
            {
                get
                {
                    return this.itemsField;
                }
                set
                {
                    this.itemsField = value;
                }
            }
        }

        public partial class Correction
        {
            [XmlElement("count")]
            public string count;
            [XmlElement("amount")]
            public string amount;
        }

        public partial class Purchase
        {
            [XmlElement("count")]
            public string count;
            [XmlElement("amount")]
            public string amount;
        }

        public partial class Refund
        {

            [XmlElement("count")]
            public string count;
            [XmlElement("amount")]
            public string amount;

        }

        public partial class CvdResultCode
        {
            [XmlElement("ECR")]
            public ECR ECR;
        }
        #endregion

        #region MPI Request Objects

        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute("MpiRequest", Namespace = "", IsNullable = false)]
        public partial class MpiRequest
        {
            [XmlElement("store_id")]
            public string store_id;
            [XmlElement("api_token")]
            public string api_token;

            private object[] itemsField;

            [XmlElement("acs", typeof(acs))]
            [XmlElement("txn", typeof(txn))]
            public object[] Items
            {
                get
                {
                    return this.itemsField;
                }
                set
                {
                    this.itemsField = value;
                }
            }
        }

        public partial class acs
        {
            [XmlElement("PaRes")]
            public string PaRes;
            [XmlElement("MD")]
            public string MD;
        }

        public partial class txn
        {
            [XmlElement("xid")]
            public string xid;
            [XmlElement("amount")]
            public string amount;
            [XmlElement("pan")]
            public string pan;
            [XmlElement("expdate")]
            public string expdate;
            [XmlElement("MD")]
            public string MD;
            [XmlElement("merchantUrl")]
            public string merchantUrl;
            [XmlElement("accept")]
            public string accept;
            [XmlElement("userAgent")]
            public string userAgent;
            [XmlElement("currency")]
            public string currency;
            [XmlElement("recurFreq")]
            public string recurFreq;
            [XmlElement("recurEnd")]
            public string recurEnd;
            [XmlElement("install")]
            public string install;

        }
        #endregion

        #region MPI Response Objects

        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute("MpiResponse", Namespace = "", IsNullable = false)]
        public partial class MpiResponse
        {
            [XmlElement("type")]
            public string type;
            [XmlElement("success")]
            public string success;
            [XmlElement("message")]
            public string message;
            [XmlElement("PaReq")]
            public string PaReq;
            [XmlElement("TermUrl")]
            public string TermUrl;
            [XmlElement("MD")]
            public string MD;
            [XmlElement("ACSUrl")]
            public string ACSUrl;
            [XmlElement("cavv")]
            public string cavv;
            [XmlElement("PAResVerified")]
            public string PAResVerified;
        }
        #endregion
    }
}
