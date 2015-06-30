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
using System.Net;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for PayJunction.
    /// </summary>
    public class PayJunction : GatewayProcessor
    {
        public PayJunction() { }

        public override String CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;

            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";

            String TransID = o.AuthorizationPNREF;

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            //Where to make the post
            transactionCommand.Append("quick_link?dc_test=" + CommonLogic.IIF(useLiveTransactions, "no", "yes"));

            //Username for the API request (mandatory)
            transactionCommand.Append("&dc_logon=" + AppLogic.AppConfig("PAYJUNCTION_LOGON"));

            //Password for the API request (mandatory)
            transactionCommand.Append("&dc_password=" + AppLogic.AppConfig("PAYJUNCTION_PASSWORD"));

            //Notes for the transaction. Maybe to put reasone when Refund or to put Stores order number 
            transactionCommand.Append("&dc_notes=");

            transactionCommand.Append("&dc_transaction_id=" + TransID.ToString());

            //“capture?”void?”hold?
            transactionCommand.Append("&dc_posture=capture");

            //Must be set to “update?
            transactionCommand.Append("&dc_transaction_type=update");

            //In order to get a full, proper HTTP/1.1 response, the value of this field should be "1.2".
            //Very important beacuse without this set to 1.2 the gateway will return an error like this
            //Error:
            //"transaction_id=response_code=FEapproval_code=response_message=dc_transaction_amount [1\r\n\r\n is invalid.  Currency amounts can only contain numbers or decimal numbers with 2 decimal places.], \n"
            transactionCommand.Append("&dc_version=" + AppLogic.AppConfig("PAYJUNCTION_HTTP_VERSION"));

            o.CaptureTXCommand = transactionCommand.ToString();


            try
            {
                byte[] data = encoding.GetBytes(transactionCommand.ToString());

                // Prepare web request...
                String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("PAYJUNCTION_LIVE_SERVER"), AppLogic.AppConfig("PAYJUNCTION_TEST_SERVER"));
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.ContentLength = data.Length;
                Stream newStream = myRequest.GetRequestStream();
                // Send the data.
                newStream.Write(data, 0, data.Length);
                newStream.Close();
                // get the response
                WebResponse myResponse;
                String rawResponseString = String.Empty;
                try
                {
                    myResponse = myRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                    {
                        rawResponseString = sr.ReadToEnd();
                        // Close and clean up the StreamReader
                        sr.Close();
                    }
                    myResponse.Close();
                }
                catch
                {
                    rawResponseString = "0|||Error Calling Pay Junction Payment Gateway||||||||";
                }

                // rawResponseString now has gateway response
                // rawResponseString now has gateway response
                byte[] byteToSplit = new byte[1];
                byteToSplit[0] = 0x1C; // == "?

                Encoding ascii = System.Text.Encoding.ASCII;

                string strAsciiFrombyteToSplit = ascii.GetString(byteToSplit);
                char[] charToSplit = strAsciiFrombyteToSplit.ToCharArray();

                //Split on char "?
                String[] statusArray = rawResponseString.Split(charToSplit[0]);

                String sql = String.Empty;
                String replyCode = statusArray[10].Split(new char[] { '=' }).GetValue(1).ToString();

                o.CaptureTXResult = rawResponseString;
               
                if (replyCode == "00" || replyCode == "85")
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    result = statusArray[11].Split(new char[] { '=' }).GetValue(1).ToString();
                }
            }
            catch
            {
                result = "NO RESPONSE FROM GATEWAY!";
            }
            
            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                    }                    
                }
            }

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            //Where to make the post
            transactionCommand.Append("quick_link?dc_test=" + CommonLogic.IIF(useLiveTransactions, "no", "yes"));

            //Username for the API request (mandatory)
            transactionCommand.Append("&dc_logon=" + AppLogic.AppConfig("PAYJUNCTION_LOGON"));

            //Password for the API request (mandatory)
            transactionCommand.Append("&dc_password=" + AppLogic.AppConfig("PAYJUNCTION_PASSWORD"));

            //Notes for the transaction.
            transactionCommand.Append("&dc_notes=");

            transactionCommand.Append("&dc_transaction_id=" + TransID.ToString());

            //“capture?”void?”hold?
            transactionCommand.Append("&dc_posture=void");

            //Must be set to “update?
            transactionCommand.Append("&dc_transaction_type=update");

            //In order to get a full, proper HTTP/1.1 response, the value of this field should be "1.2".
            //Very important beacuse without this set to 1.2 the gateway will return an error like this
            //Error:
            //"transaction_id=response_code=FEapproval_code=response_message=dc_transaction_amount [1\r\n\r\n is invalid.  Currency amounts can only contain numbers or decimal numbers with 2 decimal places.], \n"
            transactionCommand.Append("&dc_version=" + AppLogic.AppConfig("PAYJUNCTION_HTTP_VERSION"));

            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("PAYJUNCTION_LIVE_SERVER"), AppLogic.AppConfig("PAYJUNCTION_TEST_SERVER"));
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                    myRequest.Method = "POST";
                    myRequest.ContentType = "application/x-www-form-urlencoded";
                    myRequest.ContentLength = data.Length;
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;
                    String rawResponseString = String.Empty;
                    try
                    {
                        myResponse = myRequest.GetResponse();
                        using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                        {
                            rawResponseString = sr.ReadToEnd();
                            // Close and clean up the StreamReader
                            sr.Close();
                        }
                        myResponse.Close();
                    }
                    catch
                    {
                        rawResponseString = "0|||Error Calling Authorize.Net Payment Gateway||||||||";
                    }

                    // rawResponseString now has gateway response
                    byte[] byteToSplit = new byte[1];
                    byteToSplit[0] = 0x1C; // == "?

                    Encoding ascii = System.Text.Encoding.ASCII;

                    string strAsciiFrombyteToSplit = ascii.GetString(byteToSplit);
                    char[] charToSplit = strAsciiFrombyteToSplit.ToCharArray();

                    //Split on char "?
                    String[] statusArray = rawResponseString.Split(charToSplit[0]);


                    String sql = String.Empty;
                    String replyCode = statusArray[10].Split(new char[] { '=' }).GetValue(1).ToString();

                    DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OrderNumber.ToString());
                    if (replyCode == "00" || replyCode == "00")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = statusArray[11].Split(new char[] { '=' }).GetValue(1).ToString();
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

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }                    
                }
            }

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);
            //Where to make the post
            transactionCommand.Append("quick_link?dc_test=" + CommonLogic.IIF(useLiveTransactions, "no", "yes"));

            //Username for the API request (mandatory)
            transactionCommand.Append("&dc_logon=" + AppLogic.AppConfig("PAYJUNCTION_LOGON"));

            //Password for the API request (mandatory)
            transactionCommand.Append("&dc_password=" + AppLogic.AppConfig("PAYJUNCTION_PASSWORD"));

            //Notes for the transaction.
            transactionCommand.Append("&dc_notes=");

            //The action to take on the transaction. The following actions are possible:
            transactionCommand.Append("&dc_transaction_type=CREDIT");

            transactionCommand.Append("&dc_transaction_id=" + TransID.ToString());

            //TThe amount to transact (mandatory)
            //MAX in test mode is 1.99 Dollars
            if (useLiveTransactions)
            {
                if (RefundAmount == System.Decimal.Zero)
                {
                    transactionCommand.Append("&dc_transaction_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));

                }
                else
                {
                    transactionCommand.Append("&dc_transaction_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundAmount));
                }
            }
            else
            {
                transactionCommand.Append("&dc_transaction_amount=0.99");
            }



            //In order to get a full, proper HTTP/1.1 response, the value of this field should be "1.2".
            //Very important beacuse without this set to 1.2 the gateway will return an error like this
            //Error:
            //"transaction_id=response_code=FEapproval_code=response_message=dc_transaction_amount [1\r\n\r\n is invalid.  Currency amounts can only contain numbers or decimal numbers with 2 decimal places.], \n"
            transactionCommand.Append("&dc_version=" + AppLogic.AppConfig("PAYJUNCTION_HTTP_VERSION"));

            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("PAYJUNCTION_LIVE_SERVER"), AppLogic.AppConfig("PAYJUNCTION_TEST_SERVER"));
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                    myRequest.Method = "POST";
                    myRequest.ContentType = "application/x-www-form-urlencoded";
                    myRequest.ContentLength = data.Length;
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;
                    String rawResponseString = String.Empty;
                    try
                    {
                        myResponse = myRequest.GetResponse();
                        using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                        {
                            rawResponseString = sr.ReadToEnd();
                            // Close and clean up the StreamReader
                            sr.Close();
                        }
                        myResponse.Close();
                    }
                    catch
                    {
                        rawResponseString = "0|||Error Calling Payjunction Payment Gateway||||||||";
                    }

                    // rawResponseString now has gateway response
                    byte[] byteToSplit = new byte[1];
                    byteToSplit[0] = 0x1C; // == "?

                    Encoding ascii = System.Text.Encoding.ASCII;

                    string strAsciiFrombyteToSplit = ascii.GetString(byteToSplit);
                    char[] charToSplit = strAsciiFrombyteToSplit.ToCharArray();

                    //Split on char "?
                    String[] statusArray = rawResponseString.Split(charToSplit[0]);

                    /*
                     * EXAMPLE INFO WHEN REFUND
                     -	statusArray	{Length=4}	string[]
                    [0]	"transaction_id=345538"	string
                    [1]	"response_code=85"	string
                    [2]	"approval_code=null"	string
                    [3]	"response_message=null\n"	string

                     */

                    String sql = String.Empty;
                    String replyCode = statusArray[1].Split(new char[] { '=' }).GetValue(1).ToString();

                    DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OriginalOrderNumber.ToString());
                    if (replyCode == "00" || replyCode == "85")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = statusArray[3].Split(new char[] { '=' }).GetValue(1).ToString();
                    }
                }
                catch
                {
                    result = "NO RESPONSE FROM GATEWAY!";
                }
            }
            return result;
        }

        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = AppLogic.ro_OK;

            ASCIIEncoding encoding = new ASCIIEncoding();

            StringBuilder transactionCommand = new StringBuilder(4096);

            //Where to make the post
            transactionCommand.Append("quick_link?dc_test=" + CommonLogic.IIF(useLiveTransactions, "no", "yes"));

            //Username for the API request (mandatory)
            transactionCommand.Append("&dc_logon=" + AppLogic.AppConfig("PAYJUNCTION_LOGON"));

            //Password for the API request (mandatory)
            transactionCommand.Append("&dc_password=" + AppLogic.AppConfig("PAYJUNCTION_PASSWORD"));

            //Notes for the transaction.
            transactionCommand.Append("&dc_notes=");
            transactionCommand.Append("&dc_invoice_number=" + OrderNumber.ToString());

            //TThe amount to transact (mandatory)
            //MAX in test mode is 1.99 Dollars
            if (useLiveTransactions)
            {
                transactionCommand.Append("&dc_transaction_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            }
            else
            {
                transactionCommand.Append("&dc_transaction_amount=0.99");
            }

            //In order to get a full, proper HTTP/1.1 response, the value of this field should be "1.2".
            //Very important beacuse without this set to 1.2 the gateway will return an error like this
            //Error:
            //"transaction_id=response_code=FEapproval_code=response_message=dc_transaction_amount [1\r\n\r\n is invalid.  Currency amounts can only contain numbers or decimal numbers with 2 decimal places.], \n"
            transactionCommand.Append("&dc_version=" + AppLogic.AppConfig("PAYJUNCTION_HTTP_VERSION"));

            //The name of the cardholder. (Optional)
            //transactionCommand.Append("&dc_name=Mike Spike");

            //The first name of the cardholder. Be aware that dc_name will take precedence 
            //over this argument if it is given.
            transactionCommand.Append("&dc_first_name=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.FirstName));
            transactionCommand.Append("&dc_last_name=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.LastName));

            //The 13-16 digit credit card number.
            if (useLiveTransactions)
            {
                transactionCommand.Append("&dc_number=" + UseBillingAddress.CardNumber);
            }
            else
            {
                transactionCommand.Append("&dc_number=4433221111223344"); // payjunction test card
            }

            //The action to take on the transaction. The following actions are possible:
            //AUTHORIZATION || AUTHORIZATION_CAPTURE || CREDIT
            transactionCommand.Append("&dc_transaction_type=" + CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "AUTHORIZATION", "AUTHORIZATION_CAPTURE"));

            //The numeric (1 through 12) expiration month of the credit card.
            transactionCommand.Append("&dc_expiration_month=" + UseBillingAddress.CardExpirationMonth);

            //Seems that it´s not necessary to set this. The gateway seems to calculate which brand the credit card is
            //transactionCommand.Append("&dc_card_brand=VSA");

            //The two- or four-digit expiration year of the credit card.
            transactionCommand.Append("&dc_expiration_year=" + UseBillingAddress.CardExpirationYear);

            /*
             The card verification value (CVV2/CVC2) of the credit card. The CVV2/CVC2 code 
             should be a three- or four-digit number. This argument is only necessary if 
             card verification value security is active on your terminal.
            */
            if (useLiveTransactions)
            {
                transactionCommand.Append("&dc_verification_number=" + CardExtraCode);
            }
            else
            {
                transactionCommand.Append("&dc_verification_number=1234");
            }

            transactionCommand.Append("&dc_address=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Address1));

            transactionCommand.Append("&dc_city=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.City));

            transactionCommand.Append("&dc_state=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.State));

            transactionCommand.Append("&dc_zipcode=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Zip));

            //The country where the statement is sent. 
            //The value of this argument should be "US" or "United States" 
            //if the address is located in the United States.
            transactionCommand.Append("&dc_card_country=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Country));

            // cart does tax here, do not pass:
            //transactionCommand.Append("&dc_tax_amount=0.00");

            //EXTRA INFORMATION FROM THE API REFERENCE
            /*
             The Trinity Gateway Service (API) allows you to initiate settlement independent of 
             the web settings. Note: Most merchants will have automatic settlement configure via 
             the Trinity Point of Sale service. This feature should only be used by advanced clients 
             that need custom settlement behaviors.
			 
            settle = “capture?”void?”hold?
			
            When using this 
            dc_transaction_type Must be set to “update?
            */
            //transactionCommand.Append("&dc_transaction_type=settle");


            /*
            The Trinity Gateway Service (API) allows you to dynamically control the 
            AVS (Address Verification System) & CVV (Cardholder Verification Number) 
            independent of the setting in the web interface. You can override the 
            account setting and dynamically set the AVS and CVV properties. 
            This can be applied to:
                ?One Time Transactions
                ?Instant Transactions
                ?Recurring Transactions
                ?Recurring Instant Transactions
				
            Example
            To set the following:
                ?Match address
                ?Match CVS
                ?Approve the transaction even if AVS fails
                dc_security= A|M|false|true|false
			
            Value string is delimited by the “|?character and is in the format:
            AVS|CVS|PREAUTH|AVSFORCE|CVVFORCE
			
            see API spec page 19
					
            */
            transactionCommand.Append("&dc_security=" + AppLogic.AppConfig("PAYJUNCTION_SECURITY_DESCRIPTOR"));

            byte[] data = encoding.GetBytes(transactionCommand.ToString());
            string strTestData = data.ToString();

            // Prepare web request...
            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            AVSResult = String.Empty;
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;
            try
            {
                String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("PAYJUNCTION_LIVE_SERVER"), AppLogic.AppConfig("PAYJUNCTION_TEST_SERVER"));
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.ContentLength = data.Length;
                Stream newStream = myRequest.GetRequestStream();
                // Send the data.
                newStream.Write(data, 0, data.Length);
                newStream.Close();
                // get the response
                WebResponse myResponse;
                String rawResponseString = String.Empty;
                try
                {
                    myResponse = myRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                    {
                        rawResponseString = sr.ReadToEnd();
                        //The answer will look loke this
                        //Split on char "? taken care of later
                        //"dc_merchant_name=Payjunction - (demo)dc_merchant_address=3 W Carrillo St Ste 204dc_merchant_city=Santa Barbaradc_merchant_state=CAdc_merchant_zip=93101dc_merchant_phone=(805) 957-1520dc_device_id=1174dc_transaction_date=2005-10-14 02:46:32.625745dc_transaction_action=chargedc_approval_code=VITAL9dc_response_code=00dc_response_message=APPROVAL VITAL9 dc_transaction_id=345206dc_posture=capturedc_invoice_number=dc_notes= \r\n\r\ndc_card_name=Mike Spikedc_card_brand=VSAdc_card_exp=XX/XXdc_card_number=XXXX-XXXX-XXXX-3344dc_card_address=Skogaholmdc_card_city=Malmoedc_card_zipcode=21140dc_card_state=ZZdc_card_country=dc_base_amount=0.99dc_tax_amount=0.00dc_capture_amount=0.99\n"

                        // Close and clean up the StreamReader
                        sr.Close();
                    }
                    myResponse.Close();
                }
                catch
                {
                    rawResponseString = "0|||Error Calling Payjunction Payment Gateway||||||||";
                }

                byte[] byteToSplit = new byte[1];
                byteToSplit[0] = 0x1C; // == "?

                Encoding ascii = System.Text.Encoding.ASCII;

                string strAsciiFrombyteToSplit = ascii.GetString(byteToSplit);
                char[] charToSplit = strAsciiFrombyteToSplit.ToCharArray();

                //Split on char "?
                String[] statusArray = rawResponseString.Split(charToSplit[0]);

                /*
                 Will look something like this (Depending how the gatway is set up. 
                 I only got this ONE time (When you set up minimum of things to the gateway)
                 and the rest of the time the Array is 28 long. Se example in the API reference.
				 
                IF OK
				
                -	statusArray	{Length=4}	string[]
                [0]	"transaction_id=345265"	string
                [1]	"response_code=00"	string
                [2]	"approval_code=VITAL3"	string
                [3]	"response_message=APPROVAL VITAL3 \n"	string

				
				
                IF NOT OK
                (this example = Declined because this could be a duplicate transaction.  This card has been charged for similar details within the last 10 minutes.)
				
                -	statusArray	{Length=28}	string[]
                [0]	"dc_merchant_name=Payjunction - (demo)"	string
                [1]	"dc_merchant_address=3 W Carrillo St Ste xxx"	string
                [2]	"dc_merchant_city=Santa Barbara"	string
                [3]	"dc_merchant_state=CA"	string
                [4]	"dc_merchant_zip=9xx01"	string
                [5]	"dc_merchant_phone=(80x) 9x7-1xx0"	string
                [6]	"dc_device_id=1174"	string
                [7]	"dc_transaction_date=null"	string
                [8]	"dc_transaction_action=charge"	string
                [9]	"dc_approval_code="	string
                [10]	"dc_response_code=DT"	string
                [11]	"dc_response_message=Declined because this could be a duplicate transaction.  This card has been charged for similar details within the last 10 minutes."	string
                [12]	"dc_transaction_id=null"	string
                [13]	"dc_posture=capture"	string
                [14]	"dc_invoice_number="	string
                [15]	"dc_notes= \r\n\r\n"	string
                [16]	"dc_card_name=johnny1 doe1"	string
                [17]	"dc_card_brand=VSA"	string
                [18]	"dc_card_exp=XX/XX"	string
                [19]	"dc_card_number=XXXX-XXXX-XXXX-3344"	string
                [20]	"dc_card_address=Skogaholm"	string
                [21]	"dc_card_city=Malmoe"	string
                [22]	"dc_card_zipcode=21140"	string
                [23]	"dc_card_state=ZZ"	string
                [24]	"dc_card_country="	string
                [25]	"dc_base_amount=0.99"	string
                [26]	"dc_tax_amount=0.00"	string
                [27]	"dc_capture_amount=0.99\n"	string

				
				
                ANOTHER OK
				
                /*
				
                -	statusArray	{Length=28}	string[]
                [0]	"dc_merchant_name=Payjunction - (demo)"	string
                [1]	"dc_merchant_address=3 W Carrillo St Ste 2xx"	string
                [2]	"dc_merchant_city=Santa Barbara"	string
                [3]	"dc_merchant_state=CA"	string
                [4]	"dc_merchant_zip=x3x01"	string
                [5]	"dc_merchant_phone=(8x5) 9x7-1xx0"	string
                [6]	"dc_device_id=1174"	string
                [7]	"dc_transaction_date=2005-10-14 07:22:54.379777"	string
                [8]	"dc_transaction_action=charge"	string
                [9]	"dc_approval_code=VITAL5"	string
                [10]	"dc_response_code=00"	string
                [11]	"dc_response_message=APPROVAL VITAL5 "	string
                [12]	"dc_transaction_id=345307"	string
                [13]	"dc_posture=capture"	string
                [14]	"dc_invoice_number="	string
                [15]	"dc_notes=4024"	string
                [16]	"dc_card_name=mike spike"	string
                [17]	"dc_card_brand=VSA"	string
                [18]	"dc_card_exp=XX/XX"	string
                [19]	"dc_card_number=XXXX-XXXX-XXXX-3344"	string
                [20]	"dc_card_address=whatever"	string
                [21]	"dc_card_city=Malmoe"	string
                [22]	"dc_card_zipcode=20314"	string
                [23]	"dc_card_state=ZZ"	string
                [24]	"dc_card_country="	string
                [25]	"dc_base_amount=0.99"	string
                [26]	"dc_tax_amount=0.00"	string
                [27]	"dc_capture_amount=0.99\n"	string

                A response_code of "00" or "85" means that the transaction was a success. 
                Any other response_code value indicates failure.
				
                00 Transaction was approved.
                85 Transaction was approved.
                FE There was a format error with your Trinity Gateway Service (API) request.
                LE Could not log you in (problem with dc_logon and/or dc_password).
                AE Address verification failed because address did not match.
                ZE Address verification failed because zip did not match.
                XE Address verification failed because zip and address did not match.
                YE Address verification failed because zip and address did not match.
                OE Address verification failed because address or zip did not match..
                UE Address verification failed because cardholder address unavailable.
                RE Address verification failed because address verification system is not working.
                SE Address verification failed because address verification system is unavailable.
                EE Address verification failed because transaction is not a mail or phone order.
                GE Address verification failed because international support is unavailable.
                CE Declined because CVV2/CVC2 code did not match.
                NL Aborted because of a system error, please try again later.
                AB Aborted because of an upstream system error, please try again later.
                04 Declined. Pick up card.
                07 Declined. Pick up card (Special Condition).
                41 Declined. Pick up card (Lost).
                43 Declined. Pick up card (Stolen).
                13 Declined because of the amount is invalid.
                14 Declined because the card number is invalid.
                80 Declined because of an invalid date.
                05 Declined. Do not honor.
                51 Declined because of insufficient funds.
                N4 Declined because the amount exceeds issuer withdrawal limit.
                61 Declined because the amount exceeds withdrawal limit.
                62 Declined because of an invalid service code (restricted).
                65 Declined because the card activity limit exceeded.
                93 Declined because there a violation (the transaction could not be completed).
                06 Declined because address verification failed.
                54 Declined because the card has expired.
                15 Declined because there is no such issuer.
                96 Declined because of a system error.
                N7 Declined because of a CVV2/CVC2 mismatch.
                M4 Declined.
                 */

                //Tip!: It may be smarter to loop throug the Array (statusArray) and split them into another Array(SPLITTEDstatusArray). 
                //But lack of time == lack of functions :-)
                String sql = String.Empty;
                String replyCode = statusArray[10].Split(new char[] { '=' }).GetValue(1).ToString();
                String responseCode = statusArray[10].Split(new char[] { '=' }).GetValue(1).ToString();
                String approvalCode = statusArray[9].Split(new char[] { '=' }).GetValue(1).ToString();
                String authResponse = statusArray[11].Split(new char[] { '=' }).GetValue(1).ToString();
                String TransID = statusArray[12].Split(new char[] { '=' }).GetValue(1).ToString();

                AuthorizationCode = statusArray[12].Split(new char[] { '=' }).GetValue(1).ToString();
                AuthorizationResult = rawResponseString;
                AuthorizationTransID = statusArray[12].Split(new char[] { '=' }).GetValue(1).ToString();
                AVSResult = statusArray[9].Split(new char[] { '=' }).GetValue(1).ToString();
                TransactionCommandOut = transactionCommand.ToString();

                if (replyCode == "00" || replyCode == "85")
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    result = authResponse;
                    if (result.Length == 0)
                    {
                        result = "Unspecified Error";
                    }
                    else
                    {
                        result = result.Replace("account", "card");
                        result = result.Replace("Account", "Card");
                        result = result.Replace("ACCOUNT", "CARD");
                    }
                }
            }
            catch
            {
                result = "Error calling Payjunction gateway. Please retry your order in a few minutes or select another checkout payment option.";
            }
            return result;
        }

    }
}

