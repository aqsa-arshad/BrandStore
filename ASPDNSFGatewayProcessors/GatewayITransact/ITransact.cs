// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Security;
using System.Text;
using System.Web.SessionState;
using System.IO;
using System.Net;
using System.Data;
using System.Xml;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for ITransact.
    /// </summary>
    public class ITransact : GatewayProcessor
    {
        public ITransact() { }

        public override String CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;

            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";
            String TransID = o.AuthorizationPNREF;
            Decimal OrderTotal = o.OrderBalance;

            XmlDocument cmdXml = new XmlDocument();

            cmdXml.AppendChild(cmdXml.CreateXmlDeclaration("1.0", null, "yes"));

            XmlElement root = cmdXml.CreateElement("ITransactInterface");
            cmdXml.AppendChild(root);

            XmlElement vNode = cmdXml.CreateElement("VendorIdentification");
            root.AppendChild(vNode);

            XmlElement tNode = cmdXml.CreateElement("VendorId");
            vNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.AppConfig("ITransact.Vendor_ID").Trim();

            tNode = cmdXml.CreateElement("VendorPassword");
            vNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.AppConfig("ITransact.Password").Trim();

            tNode = cmdXml.CreateElement("HomePage");
            vNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.AppConfig("ITransact.Password").Trim();

            XmlElement pNode = cmdXml.CreateElement("PostAuthTransaction");
            root.AppendChild(pNode);

            tNode = cmdXml.CreateElement("OperationXID");
            pNode.AppendChild(tNode);
            tNode.InnerText = TransID;

            tNode = cmdXml.CreateElement("Total");
            pNode.AppendChild(tNode);
            tNode.InnerText = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);

            XmlElement tcNode = cmdXml.CreateElement("TransactionControl");
            pNode.AppendChild(tcNode);

            tNode = cmdXml.CreateElement("SendCustomerEmail");
            tcNode.AppendChild(tNode);
            tNode.InnerText = "TRUE";
            
            //  This is optional
            //tNode = cmdXml.CreateElement("EmailText");
            //tcNode.AppendChild(tNode);
            //tNode.InnerText = "";

            //tNode = cmdXml.CreateElement("EmailTextItem");
            //tcNode.AppendChild(tNode);
            //tNode.InnerText = "";

            //Have good Xml. Lets make it pretty
            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append(XmlCommon.FormatXml(cmdXml));

            o.CaptureTXCommand = transactionCommand.ToString();
          
            if (TransID.Length == 0) 
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = AppLogic.AppConfig("ITransact.VoidRefund_Server");
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                    myRequest.ContentType = "text/xml;charset=\"utf-8\"";
                    myRequest.Accept = "text/xml";
                    myRequest.Method = "POST";
                    //myRequest.Headers.Add ("MIME-Version", "1.0");
                    //myRequest.Headers.Add ("Request-number", "1");
                    //myRequest.Headers.Add ("Content-transfer-encoding", "text");
                    //myRequest.Headers.Add ("Document-type", "Request");
                    //myRequest.ContentType = "text/Xml";
                    myRequest.ContentLength = data.Length;
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

                    String replyCode = String.Empty;
                    String authResponse = String.Empty;

                    XmlDocument responseXml = new XmlDocument();
                    try
                    {
                        //Make sure it's good Xml
                        responseXml.LoadXml(rawResponseString.Trim());
                        //Have good Xml. Lets make it pretty
                        rawResponseString = XmlCommon.FormatXml(responseXml);

                        replyCode = responseXml.SelectSingleNode("//Status").InnerText;
                        authResponse = responseXml.SelectSingleNode("//ErrorCategory").InnerText + ": " + responseXml.SelectSingleNode("//ErrorMessage").InnerText;
                    }
                    catch
                    {
                        throw new ArgumentException("ITransact Unexpected Response: " + rawResponseString); // ITransact returns HTML sometimes, or other crap, so we just have to dump it out here
                        //authResponse = "GARBLED RESPONSE FROM THE GATEWAY";
                    }

                    o.CaptureTXResult = rawResponseString;
                  
                    if (replyCode.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase)) // They sometimes return OK or ok so ignore case
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = authResponse;
                    }
                }
                catch
                {
                    result = "NO RESPONSE FROM GATEWAY!";
                }
            }
            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());

            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            String CaptureTXResult = String.Empty;

            Decimal OrderTotal = System.Decimal.Zero;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders where OrderNumber=" + OrderNumber.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");

                        // You can only Void a PostAuth transaction so you may need the Transaction ID for the Capture Transaction 
                        CaptureTXResult = DB.RSField(rs, "CaptureTXResult");
                    }
                }
            }

            //If there was a Capture TXResult then Void the Capture TX instead
            if (CaptureTXResult.Length != 0)
            {
                try
                {
                    XmlDocument captureXml = new XmlDocument();
                    captureXml.LoadXml(CaptureTXResult);
                    // Get the Transaction ID of the Capture Transaction instead
                    TransID = captureXml.SelectSingleNode("//XID").InnerText;
                }
                catch
                {
                    result = String.Format("Invalid Capture Transaction ID: {0}", TransID);
                }
            }

            XmlDocument cmdXml = new XmlDocument();

            cmdXml.AppendChild(cmdXml.CreateXmlDeclaration("1.0", null, "yes"));

            XmlElement root = cmdXml.CreateElement("ITransactInterface");
            cmdXml.AppendChild(root);

            XmlElement vNode = cmdXml.CreateElement("VendorIdentification");
            root.AppendChild(vNode);

            XmlElement tNode = cmdXml.CreateElement("VendorId");
            vNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.AppConfig("ITransact.Vendor_ID").Trim();

            tNode = cmdXml.CreateElement("VendorPassword");
            vNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.AppConfig("ITransact.Password").Trim();

            tNode = cmdXml.CreateElement("HomePage");
            vNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.AppConfig("ITransact.Password").Trim();

            XmlElement pNode = cmdXml.CreateElement("VoidTransaction");
            root.AppendChild(pNode);

            tNode = cmdXml.CreateElement("OperationXID");
            pNode.AppendChild(tNode);
            tNode.InnerText = TransID;

            XmlElement tcNode = cmdXml.CreateElement("TransactionControl");
            pNode.AppendChild(tcNode);

            tNode = cmdXml.CreateElement("SendCustomerEmail");
            tcNode.AppendChild(tNode);
            tNode.InnerText = "TRUE";

            // this is optional
            //tNode = cmdXml.CreateElement("EmailText");
            //tcNode.AppendChild(tNode);
            //tNode.InnerText = "";

            //tNode = cmdXml.CreateElement("EmailTextItem");
            //tcNode.AppendChild(tNode);
            //tNode.InnerText = "";

            //Have good Xml. Lets make it pretty
            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append(XmlCommon.FormatXml(cmdXml));
            //Save reformatted version 

            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OrderNumber.ToString());

            if (TransID.Length == 0) 
            {
                result = String.Format("Invalid or Empty Transaction ID: {0}", TransID);
            }
            else
            {
                try
                {
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = AppLogic.AppConfig("ITransact.VoidRefund_Server");
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                    myRequest.ContentType = "text/xml;charset=\"utf-8\"";
                    myRequest.Accept = "text/xml";
                    myRequest.Method = "POST";
                    //myRequest.Headers.Add ("MIME-Version", "1.0");
                    //myRequest.Headers.Add ("Request-number", "1");
                    //myRequest.Headers.Add ("Content-transfer-encoding", "text");
                    //myRequest.Headers.Add ("Document-type", "Request");
                    //myRequest.ContentType = "text/Xml";
                    myRequest.ContentLength = data.Length;
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

                    String replyCode = String.Empty;
                    String authResponse = String.Empty;

                    XmlDocument responseXml = new XmlDocument();
                    try
                    {
                        //Make sure it's good Xml
                        responseXml.LoadXml(rawResponseString.Trim());
                        //Have good Xml. Lets make it pretty
                        rawResponseString = XmlCommon.FormatXml(responseXml);

                        replyCode = responseXml.SelectSingleNode("//Status").InnerText;
                        authResponse = responseXml.SelectSingleNode("//ErrorCategory").InnerText + " " + responseXml.SelectSingleNode("//ErrorMessage").InnerText;
                    }
                    catch
                    {
                        throw new ArgumentException("ITransact Unexpected Response: " + rawResponseString); // ITransact returns HTML sometimes, or other crap, so we just have to dump it out here
                        //authResponse = "GARBLED RESPONSE FROM THE GATEWAY";
                    }

                    DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OrderNumber.ToString());

                    if (replyCode.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase)) // They sometimes return OK or ok so ignore case
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = authResponse;
                    }
                }
                catch
                {
                    result = "NO RESPONSE FROM GATEWAY!";
                }
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
            Decimal OrderTotal = System.Decimal.Zero;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders where OrderNumber=" + OriginalOrderNumber.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }
                }
            }

            XmlDocument cmdXml = new XmlDocument();

            cmdXml.AppendChild(cmdXml.CreateXmlDeclaration("1.0", null, "yes"));

            XmlElement root = cmdXml.CreateElement("ITransactInterface");
            cmdXml.AppendChild(root);

            XmlElement vNode = cmdXml.CreateElement("VendorIdentification");
            root.AppendChild(vNode);

            XmlElement tNode = cmdXml.CreateElement("VendorId");
            vNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.AppConfig("ITransact.Vendor_ID").Trim();

            tNode = cmdXml.CreateElement("VendorPassword");
            vNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.AppConfig("ITransact.Password").Trim();

            tNode = cmdXml.CreateElement("HomePage");
            vNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.AppConfig("ITransact.Password").Trim();

            XmlElement pNode = cmdXml.CreateElement("TranCredTransaction");
            root.AppendChild(pNode);

            tNode = cmdXml.CreateElement("OperationXID");
            pNode.AppendChild(tNode);
            tNode.InnerText = TransID;

            if (RefundAmount == System.Decimal.Zero)
            {
              tNode = cmdXml.CreateElement("Total");
              pNode.AppendChild(tNode);
              tNode.InnerText = String.Format("{0:###0.00}", OrderTotal);
            }
            else
            {
              tNode = cmdXml.CreateElement("Total");
              pNode.AppendChild(tNode);
              tNode.InnerText = String.Format("{0:###0.00}", RefundAmount);
            }

            XmlElement tcNode = cmdXml.CreateElement("TransactionControl");
            pNode.AppendChild(tcNode);

            tNode = cmdXml.CreateElement("SendCustomerEmail");
            tcNode.AppendChild(tNode);
            tNode.InnerText = "TRUE";

            // this is optional
            //tNode = cmdXml.CreateElement("EmailText");
            //tcNode.AppendChild(tNode);
            //tNode.InnerText = "";

            //tNode = cmdXml.CreateElement("EmailTextItem");
            //tcNode.AppendChild(tNode);
            //tNode.InnerText = "";

            //Have good Xml. Lets make it pretty
            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append(XmlCommon.FormatXml(cmdXml));

            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            if (TransID.Length == 0) 
            {
                result = String.Format("Invalid or Empty Transaction ID: {0}", TransID);
            }
            else
            {
                try
                {
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = AppLogic.AppConfig("ITransact.VoidRefund_Server");
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                    myRequest.ContentType = "text/xml;charset=\"utf-8\"";
                    myRequest.Accept = "text/xml";
                    myRequest.Method = "POST";
                    //myRequest.Headers.Add ("MIME-Version", "1.0");
                    //myRequest.Headers.Add ("Request-number", "1");
                    //myRequest.Headers.Add ("Content-transfer-encoding", "text");
                    //myRequest.Headers.Add ("Document-type", "Request");
                    //myRequest.ContentType = "text/Xml";
                    myRequest.ContentLength = data.Length;
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

                    String replyCode = String.Empty;
                    String authResponse = String.Empty;

                    XmlDocument responseXml = new XmlDocument();
                    try
                    {
                        //Make sure it's good Xml
                        responseXml.LoadXml(rawResponseString.Trim());
                        //Have good Xml. Lets make it pretty
                        rawResponseString = XmlCommon.FormatXml(responseXml);
                        replyCode = responseXml.SelectSingleNode("//Status").InnerText;
                        authResponse = responseXml.SelectSingleNode("//ErrorCategory").InnerText + " " + responseXml.SelectSingleNode("//ErrorMessage").InnerText;
                    }
                    catch
                    {
                        throw new ArgumentException("ITransact Unexpected Response: " + rawResponseString); // ITransact returns HTML sometimes, or other crap, so we just have to dump it out here
                        //authResponse = "GARBLED RESPONSE FROM THE GATEWAY";
                    }

                    DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OriginalOrderNumber.ToString());
                    if (replyCode.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase)) // They sometimes return OK or ok so ignore case
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = authResponse;
                    }
                }
                catch
                {
                    result = "NO RESPONSE FROM GATEWAY!";
                }
            }
            return result;
        }

        public override string ProcessCard(int OrderNumber, int CustomerID, decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, string CardExtraCode, Address UseShippingAddress, string CAVV, string ECI, string XID, out string AVSResult, out string AuthorizationResult, out string AuthorizationCode, out string AuthorizationTransID, out string TransactionCommandOut, out string TransactionResponse)
        {
            Customer ThisCustomer = new Customer(CustomerID, true);
            ShoppingCart cart = new ShoppingCart(ThisCustomer.SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false, false);
            
            String result = AppLogic.ro_OK;

            XmlDocument cmdXml = new XmlDocument();

            cmdXml.AppendChild(cmdXml.CreateXmlDeclaration("1.0",null,"yes"));
          
            XmlElement root = cmdXml.CreateElement("SaleRequest");
            cmdXml.AppendChild(root);

            XmlElement cNode = cmdXml.CreateElement("CustomerData");
            root.AppendChild(cNode);
          
            if (UseBillingAddress.EMail.Length == 0)
            {
              UseBillingAddress.EMail = cart.EMail;
            }

            XmlElement tNode = cmdXml.CreateElement("Email");
            cNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.EMail;

            XmlElement bNode = cmdXml.CreateElement("BillingAddress");
            cNode.AppendChild(bNode);

            tNode = cmdXml.CreateElement("FirstName");
            bNode.AppendChild(tNode);
            //Substitute the ITransact.Test_FirstName if not a live transaction
            tNode.InnerText = CommonLogic.IIF(useLiveTransactions, UseBillingAddress.FirstName, AppLogic.AppConfig("ITransact.Test_FirstName"));

            tNode = cmdXml.CreateElement("LastName");
            bNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.LastName;

            tNode = cmdXml.CreateElement("Address1");
            bNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.Address1;

            if (UseBillingAddress.Address2.Trim().Length != 0)
            {
              tNode = cmdXml.CreateElement("Address2");
              bNode.AppendChild(tNode);
              tNode.InnerText = UseBillingAddress.Address2;
            }

            tNode = cmdXml.CreateElement("City");
            bNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.City;

            tNode = cmdXml.CreateElement("State");
            bNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.State;

            tNode = cmdXml.CreateElement("Zip");
            bNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.Zip;

            tNode = cmdXml.CreateElement("Country");
            bNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.Country;

            tNode = cmdXml.CreateElement("Phone");
            bNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.Phone;

            if (UseShippingAddress != null)
            {
              XmlElement sNode = cmdXml.CreateElement("ShippingAddress");
              cNode.AppendChild(sNode);

              tNode = cmdXml.CreateElement("FirstName");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseShippingAddress.FirstName;

              tNode = cmdXml.CreateElement("LastName");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseShippingAddress.LastName;

              tNode = cmdXml.CreateElement("Address1");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseShippingAddress.Address1;

              if (UseShippingAddress.Address2.Trim().Length != 0)
              {
                tNode = cmdXml.CreateElement("Address2");
                sNode.AppendChild(tNode);
                tNode.InnerText = UseShippingAddress.Address2;
              }

              tNode = cmdXml.CreateElement("City");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseShippingAddress.City;

              tNode = cmdXml.CreateElement("State");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseShippingAddress.State;

              tNode = cmdXml.CreateElement("Zip");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseShippingAddress.Zip;

              tNode = cmdXml.CreateElement("Country");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseShippingAddress.Country;

              tNode = cmdXml.CreateElement("Phone");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseShippingAddress.Phone;
            }

            XmlElement aNode = cmdXml.CreateElement("AccountInfo");
            cNode.AppendChild(aNode);

            XmlElement ccNode = cmdXml.CreateElement("CardInfo");
            aNode.AppendChild(ccNode);

            tNode = cmdXml.CreateElement("CCNum");
            ccNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.CardNumber;

            tNode = cmdXml.CreateElement("CCMo");
            ccNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.CardExpirationMonth.PadLeft(2, '0');

            tNode = cmdXml.CreateElement("CCYr");
            ccNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.CardExpirationYear.ToString();

            if (CardExtraCode.Trim().Length != 0)
            {
              tNode = cmdXml.CreateElement("CVV2Number");
              ccNode.AppendChild(tNode);
              tNode.InnerText = CardExtraCode;
            }
            else
            {
              tNode = cmdXml.CreateElement("CVV2Number");
              ccNode.AppendChild(tNode);

              tNode = cmdXml.CreateElement("CVV2Illegible");
              ccNode.AppendChild(tNode);
              tNode.InnerText = "1";
            }

            XmlElement dNode = cmdXml.CreateElement("TransactionData");
            root.AppendChild(dNode);

            tNode = cmdXml.CreateElement("VendorId");
            dNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.AppConfig("ITransact.Vendor_ID").Trim();

            tNode = cmdXml.CreateElement("VendorPassword");
            dNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.AppConfig("ITransact.Password").Trim();

            tNode = cmdXml.CreateElement("HomePage");
            dNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.LiveServer().Trim();

            // Add Preauth tag if auth only
            if (TransactionMode == TransactionModeEnum.auth)
            {
              tNode = cmdXml.CreateElement("Preauth");
              dNode.AppendChild(tNode);
            }

            // EMail message text for the ITransact receipt since it's only a total.
            XmlElement eNode = cmdXml.CreateElement("EmailText");
            dNode.AppendChild(eNode);

            tNode = cmdXml.CreateElement("EmailTextItem");
            eNode.AppendChild(tNode);
            tNode.InnerText = "Detailed receipt will be sent in a separate EMail.";

            XmlElement oNode = cmdXml.CreateElement("OrderItems");
            dNode.AppendChild(oNode);

            decimal productTotal = System.Decimal.Zero;

            foreach (CartItem item in cart.CartItems)
            {

              XmlElement iNode = cmdXml.CreateElement("Item");
              oNode.AppendChild(iNode);

              tNode = cmdXml.CreateElement("Description");
              iNode.AppendChild(tNode);
              tNode.InnerText = item.ProductName;

              tNode = cmdXml.CreateElement("Cost");
              iNode.AppendChild(tNode);
              tNode.InnerText = Localization.CurrencyStringForGatewayWithoutExchangeRate(item.Price);

              tNode = cmdXml.CreateElement("Qty");
              iNode.AppendChild(tNode);
              tNode.InnerText = item.Quantity.ToString();

              productTotal += (item.Price * item.Quantity);
            }
            productTotal += (cart.TaxTotal());
            productTotal += (cart.ShippingTotal(true,false));
            decimal discountAmt = productTotal - OrderTotal;

            if (discountAmt > 0)
            {
              XmlElement iNode = cmdXml.CreateElement("Item");
              oNode.AppendChild(iNode);

              tNode = cmdXml.CreateElement("Description");
              iNode.AppendChild(tNode);
              tNode.InnerText = "Discounts and Coupons - You saved";

              tNode = cmdXml.CreateElement("Cost");
              iNode.AppendChild(tNode);
              tNode.InnerText = Localization.CurrencyStringForGatewayWithoutExchangeRate(discountAmt);

              tNode = cmdXml.CreateElement("Qty");
              iNode.AppendChild(tNode);
              tNode.InnerText = "1";
            }

            if (cart.TaxTotal() != 0)
            {
              XmlElement iNode = cmdXml.CreateElement("Item");
              oNode.AppendChild(iNode);

              tNode = cmdXml.CreateElement("Description");
              iNode.AppendChild(tNode);
              tNode.InnerText = "Tax";

              tNode = cmdXml.CreateElement("Cost");
              iNode.AppendChild(tNode);
              tNode.InnerText = Localization.CurrencyStringForGatewayWithoutExchangeRate(cart.TaxTotal());

              tNode = cmdXml.CreateElement("Qty");
              iNode.AppendChild(tNode);
              tNode.InnerText = "1";
            }

            if (cart.ShippingTotal(true,true) != 0)
            {
              XmlElement iNode = cmdXml.CreateElement("Item");
              oNode.AppendChild(iNode);

              tNode = cmdXml.CreateElement("Description");
              iNode.AppendChild(tNode);
              tNode.InnerText = "Shipping";

              tNode = cmdXml.CreateElement("Cost");
              iNode.AppendChild(tNode);
              tNode.InnerText = Localization.CurrencyStringForGatewayWithoutExchangeRate(cart.ShippingTotal(true,true));

              tNode = cmdXml.CreateElement("Qty");
              iNode.AppendChild(tNode);
              tNode.InnerText = "1";
            }

            //Have good Xml. Lets make it pretty
            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append(XmlCommon.FormatXml(cmdXml));


            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            String AuthServer = AppLogic.AppConfig("ITransact.Sale_Server"); // Use the Sale Transaction Server

            WebResponse myResponse;
            String rawResponseString = String.Empty;
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.ContentType = "text/xml;charset=\"utf-8\"";
                myRequest.Accept = "text/xml";
                myRequest.Method = "POST";
                //myRequest.Headers.Add ("MIME-Version", "1.0");
                //myRequest.Headers.Add ("Request-number", "1");
                //myRequest.Headers.Add ("Content-transfer-encoding", "text");
                //myRequest.Headers.Add ("Document-type", "Request");
                //myRequest.ContentType = "text/Xml";
                myRequest.ContentLength = data.Length;
                Stream newStream = myRequest.GetRequestStream();
                // Send the data.
                newStream.Write(data, 0, data.Length);
                newStream.Close();
                // get the response
                myResponse = myRequest.GetResponse();
                StreamReader sr = new StreamReader(myResponse.GetResponseStream());
                rawResponseString = sr.ReadToEnd();
                // Close and clean up the StreamReader
                sr.Close();
                //Close the Response
                myResponse.Close();
            }
            catch
            {
                result = "NO RESPONSE FROM GATEWAY!";
            }

            // rawResponseString now has gateway response
            TransactionResponse = rawResponseString;

            AVSResult = String.Empty;

            String replyCode = String.Empty;
            String approvalCode = String.Empty;
            String authResponse = String.Empty;
            String TransID = String.Empty;

            XmlDocument responseXml = new XmlDocument();

            try
            {
                responseXml.LoadXml(rawResponseString);
                //Good Xml make it pretty
                rawResponseString = XmlCommon.FormatXml(responseXml);
                TransactionResponse = rawResponseString;

                XmlNode nNode = responseXml.SelectSingleNode("//Status");
                if (nNode != null) replyCode = nNode.InnerText;

                nNode = responseXml.SelectSingleNode("//ErrorCategory");
                if (nNode != null) authResponse = nNode.InnerText + ": " + responseXml.SelectSingleNode("//ErrorMessage").InnerText;

                nNode = responseXml.SelectSingleNode("//AuthCode");
                if (nNode != null) approvalCode = nNode.InnerText;

                nNode = responseXml.SelectSingleNode("//XID");
                if (nNode != null) TransID = nNode.InnerText;

                nNode = responseXml.SelectSingleNode("//AVSResponse");
                if (nNode != null) AVSResult = nNode.InnerText;
            }
            catch
            {
                //throw new ArgumentException("ITransact Unexpected Response: " + rawResponseString + transactionCommand.ToString()); // ITransact returns HTML sometimes, or other crap, so we just have to dump it out here
                authResponse = "GARBLED RESPONSE FROM THE GATEWAY. Please try later";
            }

            AuthorizationCode = approvalCode;
            AuthorizationResult = rawResponseString;
            AuthorizationTransID = TransID;
            TransactionCommandOut = transactionCommand.ToString();

            if (!replyCode.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase)) // They sometimes return OK or ok so ignore case
            {
                result = authResponse;
                if (result.Length == 0)
                {
                    result = "Unspecified Error";
                }
            }
            return result;
        }

        public override string ProcessECheck(int OrderNumber, int CustomerID, decimal OrderTotal, Address UseBillingAddress, Address UseShippingAddress, out string AVSResult, out string AuthorizationResult, out string AuthorizationCode, out string AuthorizationTransID, out string TransactionCommandOut, out string TransactionResponse)
        {
            String result = AppLogic.ro_OK;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            Customer ThisCustomer = new Customer(CustomerID, true);
            ShoppingCart cart = new ShoppingCart(ThisCustomer.SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false, false);

            XmlDocument cmdXml = new XmlDocument();

            cmdXml.AppendChild(cmdXml.CreateXmlDeclaration("1.0", null, "yes"));

            XmlElement root = cmdXml.CreateElement("SaleRequest");
            cmdXml.AppendChild(root);

            XmlElement cNode = cmdXml.CreateElement("CustomerData");
            root.AppendChild(cNode);

            if (UseBillingAddress.EMail.Length == 0)
            {
              UseBillingAddress.EMail = cart.EMail;
            }

            XmlElement tNode = cmdXml.CreateElement("Email");
            cNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.EMail;

            XmlElement bNode = cmdXml.CreateElement("BillingAddress");
            cNode.AppendChild(bNode);

            tNode = cmdXml.CreateElement("FirstName");
            bNode.AppendChild(tNode);
            //Substitute the ITransact.Test_FirstName if not a live transaction
            tNode.InnerText = CommonLogic.IIF(useLiveTransactions, UseBillingAddress.FirstName, AppLogic.AppConfig("ITransact.Test_FirstName"));

            tNode = cmdXml.CreateElement("LastName");
            bNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.LastName;

            tNode = cmdXml.CreateElement("Address1");
            bNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.Address1;

            if (UseBillingAddress.Address2.Trim().Length != 0)
            {
              tNode = cmdXml.CreateElement("Address2");
              bNode.AppendChild(tNode);
              tNode.InnerText = UseBillingAddress.Address2;
            }

            tNode = cmdXml.CreateElement("City");
            bNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.City;

            tNode = cmdXml.CreateElement("State");
            bNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.State;

            tNode = cmdXml.CreateElement("Zip");
            bNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.Zip;

            tNode = cmdXml.CreateElement("Country");
            bNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.Country;

            tNode = cmdXml.CreateElement("Phone");
            bNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.Phone;

            if (UseShippingAddress != null)
            {
              XmlElement sNode = cmdXml.CreateElement("ShippingAddress");
              cNode.AppendChild(sNode);

              tNode = cmdXml.CreateElement("FirstName");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseBillingAddress.FirstName;

              tNode = cmdXml.CreateElement("LastName");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseBillingAddress.LastName;

              tNode = cmdXml.CreateElement("Address1");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseBillingAddress.Address1;

              if (UseShippingAddress.Address2.Trim().Length != 0)
              {
                tNode = cmdXml.CreateElement("Address2");
                sNode.AppendChild(tNode);
                tNode.InnerText = UseBillingAddress.Address2;
              }

              tNode = cmdXml.CreateElement("City");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseBillingAddress.City;

              tNode = cmdXml.CreateElement("State");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseBillingAddress.State;

              tNode = cmdXml.CreateElement("Zip");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseBillingAddress.Zip;

              tNode = cmdXml.CreateElement("Country");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseBillingAddress.Country;

              tNode = cmdXml.CreateElement("Phone");
              sNode.AppendChild(tNode);
              tNode.InnerText = UseBillingAddress.Phone;
            }

            XmlElement aNode = cmdXml.CreateElement("AccountInfo");
            cNode.AppendChild(aNode);

            //Translate the account type
            string AccountType = String.Empty;
            switch (UseBillingAddress.ECheckBankAccountType)
            {
              case "CHECKING": AccountType = "personal"; break;
              case "SAVINGS": AccountType = "personal"; break;
              case "BUSINESS CHECKING": AccountType = "business"; break;
              default: AccountType = "personal"; break;
            }

            //Check information
            XmlElement ccNode = cmdXml.CreateElement("CheckInfo");
            aNode.AppendChild(ccNode);

            tNode = cmdXml.CreateElement("ABA");
            ccNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.ECheckBankABACode;

            tNode = cmdXml.CreateElement("Account");
            ccNode.AppendChild(tNode);
            tNode.InnerText = UseBillingAddress.ECheckBankAccountNumber;

            tNode = cmdXml.CreateElement("AccountType");
            ccNode.AppendChild(tNode);
            tNode.InnerText = AccountType;

            tNode = cmdXml.CreateElement("CheckMemo");
            ccNode.AppendChild(tNode);
            tNode.InnerText = String.Format("{0} - Order# {1}", AppLogic.AppConfig("StoreName"), OrderNumber);

          
            //Transaction Data
            XmlElement dNode = cmdXml.CreateElement("TransactionData");
            root.AppendChild(dNode);

            tNode = cmdXml.CreateElement("VendorId");
            dNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.AppConfig("ITransact.Vendor_ID").Trim();

            tNode = cmdXml.CreateElement("VendorPassword");
            dNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.AppConfig("ITransact.Password").Trim();

            tNode = cmdXml.CreateElement("HomePage");
            dNode.AppendChild(tNode);
            tNode.InnerText = AppLogic.LiveServer().Trim();

            // EMail message text for the ITransact receipt since it's only a total.
            XmlElement eNode = cmdXml.CreateElement("EmailText");
            dNode.AppendChild(eNode);

            tNode = cmdXml.CreateElement("EmailTextItem");
            eNode.AppendChild(tNode);
            tNode.InnerText = "Detailed receipt will be sent in a separate EMail.";

            XmlElement oNode = cmdXml.CreateElement("OrderItems");
            dNode.AppendChild(oNode);

            decimal productTotal = System.Decimal.Zero;

            foreach (CartItem item in cart.CartItems)
            {

              XmlElement iNode = cmdXml.CreateElement("Item");
              oNode.AppendChild(iNode);

              tNode = cmdXml.CreateElement("Description");
              iNode.AppendChild(tNode);
              tNode.InnerText = item.ProductName;

              tNode = cmdXml.CreateElement("Cost");
              iNode.AppendChild(tNode);
              tNode.InnerText = Localization.CurrencyStringForGatewayWithoutExchangeRate(item.Price);

              tNode = cmdXml.CreateElement("Qty");
              iNode.AppendChild(tNode);
              tNode.InnerText = item.Quantity.ToString();

              productTotal += (item.Price * item.Quantity);
            }

            productTotal += (cart.TaxTotal());
            productTotal += (cart.ShippingTotal(true,false));

            if (productTotal != OrderTotal)
            {
              XmlElement iNode = cmdXml.CreateElement("Item");
              oNode.AppendChild(iNode);

              tNode = cmdXml.CreateElement("Description");
              iNode.AppendChild(tNode);
              tNode.InnerText = "Discounts and Coupons - You saved";

              tNode = cmdXml.CreateElement("Cost");
              iNode.AppendChild(tNode);
              tNode.InnerText = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal - productTotal);

              tNode = cmdXml.CreateElement("Qty");
              iNode.AppendChild(tNode);
              tNode.InnerText = "1";
            }

            if (cart.TaxTotal() != 0)
            {
              XmlElement iNode = cmdXml.CreateElement("Item");
              oNode.AppendChild(iNode);

              tNode = cmdXml.CreateElement("Description");
              iNode.AppendChild(tNode);
              tNode.InnerText = "Tax";

              tNode = cmdXml.CreateElement("Cost");
              iNode.AppendChild(tNode);
              tNode.InnerText = Localization.CurrencyStringForGatewayWithoutExchangeRate(cart.TaxTotal());

              tNode = cmdXml.CreateElement("Qty");
              iNode.AppendChild(tNode);
              tNode.InnerText = "1";
            }

            if (cart.ShippingTotal(true,true) != 0)
            {
              XmlElement iNode = cmdXml.CreateElement("Item");
              oNode.AppendChild(iNode);

              tNode = cmdXml.CreateElement("Description");
              iNode.AppendChild(tNode);
              tNode.InnerText = "Shipping";

              tNode = cmdXml.CreateElement("Cost");
              iNode.AppendChild(tNode);
              tNode.InnerText = Localization.CurrencyStringForGatewayWithoutExchangeRate(cart.ShippingTotal(true,true));

              tNode = cmdXml.CreateElement("Qty");
              iNode.AppendChild(tNode);
              tNode.InnerText = "1";
            }

            //Have good Xml. Lets make it pretty
            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append(XmlCommon.FormatXml(cmdXml));


            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            String AuthServer = AppLogic.AppConfig("ITransact.Sale_Server"); // Use the Sale Transaction Server

            WebResponse myResponse;
            String rawResponseString = String.Empty;
            try
            {
              HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
              myRequest.ContentType = "text/xml;charset=\"utf-8\"";
              myRequest.Accept = "text/xml";
              myRequest.Method = "POST";
              //myRequest.Headers.Add ("MIME-Version", "1.0");
              //myRequest.Headers.Add ("Request-number", "1");
              //myRequest.Headers.Add ("Content-transfer-encoding", "text");
              //myRequest.Headers.Add ("Document-type", "Request");
              //myRequest.ContentType = "text/Xml";
              myRequest.ContentLength = data.Length;
              Stream newStream = myRequest.GetRequestStream();
              // Send the data.
              newStream.Write(data, 0, data.Length);
              newStream.Close();
              // get the response
              myResponse = myRequest.GetResponse();
              StreamReader sr = new StreamReader(myResponse.GetResponseStream());
              rawResponseString = sr.ReadToEnd();
              // Close and clean up the StreamReader
              sr.Close();
              //Close the Response
              myResponse.Close();
            }
            catch
            {
              result = "NO RESPONSE FROM GATEWAY!";
            }

            // rawResponseString now has gateway response
            TransactionResponse = rawResponseString;

            AVSResult = String.Empty;

            String replyCode = String.Empty;
            String approvalCode = String.Empty;
            String authResponse = String.Empty;
            String TransID = String.Empty;

            XmlDocument responseXml = new XmlDocument();

            try
            {
              responseXml.LoadXml(rawResponseString);
              //Good Xml make it pretty
              rawResponseString = XmlCommon.FormatXml(responseXml);
              TransactionResponse = rawResponseString;

              XmlNode nNode = responseXml.SelectSingleNode("//Status");
              if (nNode != null) replyCode = nNode.InnerText;

              nNode = responseXml.SelectSingleNode("//ErrorCategory");
              if (nNode != null) authResponse = nNode.InnerText + ": " + responseXml.SelectSingleNode("//ErrorMessage").InnerText;

              nNode = responseXml.SelectSingleNode("//AuthCode");
              if (nNode != null) approvalCode = nNode.InnerText;

              nNode = responseXml.SelectSingleNode("//XID");
              if (nNode != null) TransID = nNode.InnerText;

              nNode = responseXml.SelectSingleNode("//AVSResponse");
              if (nNode != null) AVSResult = nNode.InnerText;
            }
            catch
            {
              //throw new ArgumentException("ITransact Unexpected Response: " + rawResponseString + transactionCommand.ToString()); // ITransact returns HTML sometimes, or other crap, so we just have to dump it out here
              authResponse = "GARBLED RESPONSE FROM THE GATEWAY. Please try later";
            }

            AuthorizationCode = approvalCode;
            AuthorizationResult = rawResponseString;
            AuthorizationTransID = TransID;
            TransactionCommandOut = transactionCommand.ToString();

            if (!replyCode.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase)) // They sometimes return OK or ok so ignore case
            {
              result = authResponse;
              if (result.Length == 0)
              {
                result = "Unspecified Error";
              }
            }
            return result;
          }

        public override bool SupportsEChecks()
        {
            return true;
        }
    }
}
