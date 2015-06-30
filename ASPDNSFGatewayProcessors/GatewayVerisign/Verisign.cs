// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------

//#define VERISIGN
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
using AspDotNetStorefrontGateways;
using System.Xml;

namespace AspDotNetStorefrontGateways.Processors
{


    /// <summary>
    /// Summary description for Verisign.
    /// </summary>
    public class Verisign : GatewayProcessor
    {
        public Verisign() { }

        public override String CaptureOrder(Order o)
        {
            String result = "VERISIGN COM COMPONENTS NOT INSTALLED ON SERVER OR STOREFRONT NOT COMPILED WITH VERISIGN CODE TURNED ON";
#if VERISIGN
			result = AppLogic.ro_OK;
			PFProCOMLib.PNComClass vsnGate = new PFProCOMLib.PNComClass();
			int TO = AppLogic.AppConfigUSInt("Verisign_Timeout");
			if(TO == 0)
			{
				TO = 60;
			}
			vsnGate.TimeOut = TO.ToString();

			bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";

            String TransID = o.AuthorizationPNREF;
            Decimal OrderTotal = o.OrderBalance;
	
		
			StringBuilder transactionCommand = new StringBuilder(4096);

			transactionCommand.Append("TRXTYPE=D&TENDER=C&COMMENT1=" + AppLogic.AppConfig("StoreName").Replace("&","").Replace("=","") + " Capture");
			transactionCommand.Append("&PWD=" + AppLogic.AppConfig("Verisign_PWD"));
			transactionCommand.Append("&USER=" + AppLogic.AppConfig("Verisign_USER"));
			transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("Verisign_VENDOR"));
			transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("Verisign_PARTNER"));
			transactionCommand.Append("&ORIGID=" + TransID);

            o.CaptureTXCommand = transactionCommand.ToString();

			try
			{
				int Ctx1;
				if(AppLogic.AppConfigBool("UseLiveTransactions"))
				{
					Ctx1 = vsnGate.CreateContext("payflow.verisign.com",443,30,String.Empty,0,String.Empty,String.Empty);
				}
				else
				{
					Ctx1 = vsnGate.CreateContext("test-payflow.verisign.com",443,30,String.Empty,0,String.Empty,String.Empty);
				}
				String rawResponseString = vsnGate.SubmitTransaction(Ctx1, transactionCommand.ToString(), transactionCommand.Length);
				vsnGate.DestroyContext (Ctx1);

				String[] statusArray = rawResponseString.Split('&');
				String replyCode = String.Empty;
				String replyMsg = String.Empty;
				String PNREF = String.Empty;
				for(int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
				{
					String[] lasKeyPair = statusArray[i].Split('=');
					switch(lasKeyPair[0].ToLowerInvariant())
					{
						case "result":
							replyCode = lasKeyPair[1];
							break;
						case "pnref":
							PNREF = lasKeyPair[1];
							break;
						case "respmsg":
							replyMsg = lasKeyPair[1];
							break;
					}
				}

                o.CaptureTXResult = rawResponseString;
                //DB.ExecuteSQL("update orders set CaptureTXResult=" + DB.SQuote(rawResponseString)  + " where OrderNumber=" + OrderNumber.ToString());
				if(replyCode == "0")
				{
					result = AppLogic.ro_OK;
				}
				else
				{
					result = replyMsg;
				}
			}
			catch
			{
				result = "NO RESPONSE FROM GATEWAY!";
			}
			//}
#endif
            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = "VERISIGN COM COMPONENTS NOT INSTALLED ON SERVER OR STOREFRONT NOT COMPILED WITH VERISIGN CODE TURNED ON";
#if VERISIGN
			result = AppLogic.ro_OK;
			PFProCOMLib.PNComClass vsnGate = new PFProCOMLib.PNComClass();
			int TO = AppLogic.AppConfigUSInt("Verisign_Timeout");
			if(TO == 0)
			{
				TO = 60;
			}
			vsnGate.TimeOut = TO.ToString();

			DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
			bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
			String TransID = String.Empty;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                    }
                    rs.Close();
                }
            }            

			StringBuilder transactionCommand = new StringBuilder(4096);

			transactionCommand.Append("TRXTYPE=V&TENDER=C&COMMENT1=" + AppLogic.AppConfig("StoreName").Replace("&","").Replace("=","") + " Void");
			transactionCommand.Append("&PWD=" + AppLogic.AppConfig("Verisign_PWD"));
			transactionCommand.Append("&USER=" + AppLogic.AppConfig("Verisign_USER"));
			transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("Verisign_VENDOR"));
			transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("Verisign_PARTNER"));
			transactionCommand.Append("&ORIGID=" + TransID);

			DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OrderNumber.ToString());

			if(TransID.Length == 0 || TransID == "0")
			{
				result = "Invalid or Empty Transaction ID";
			}
			else
			{
				try
				{
					int Ctx1;
					if(AppLogic.AppConfigBool("UseLiveTransactions"))
					{
						Ctx1 = vsnGate.CreateContext("payflow.verisign.com",443,30,String.Empty,0,String.Empty,String.Empty);
					}
					else
					{
						Ctx1 = vsnGate.CreateContext("test-payflow.verisign.com",443,30,String.Empty,0,String.Empty,String.Empty);
					}
					String rawResponseString = vsnGate.SubmitTransaction(Ctx1, transactionCommand.ToString(), transactionCommand.Length);
					vsnGate.DestroyContext (Ctx1);

					String[] statusArray = rawResponseString.Split('&');
					String replyCode = String.Empty;
					String replyMsg = String.Empty;
					String PNREF = String.Empty;
					for(int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
					{
						String[] lasKeyPair = statusArray[i].Split('=');
						switch(lasKeyPair[0].ToLowerInvariant())
						{
							case "result":
								replyCode = lasKeyPair[1];
								break;
							case "pnref":
								PNREF = lasKeyPair[1];
								break;
							case "respmsg":
								replyMsg = lasKeyPair[1];
								break;
						}
					}
					
					DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString)  + " where OrderNumber=" + OrderNumber.ToString());
					if(replyCode == "0")
					{
						result = AppLogic.ro_OK;
					}
					else
					{
						result = replyMsg;
					}
				}
				catch
				{
					result = "NO RESPONSE FROM GATEWAY!";
				}

			}
#endif
            return result;
        }

        // if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            String result = "VERISIGN COM COMPONENTS NOT INSTALLED ON SERVER OR STOREFRONT NOT COMPILED WITH VERISIGN CODE TURNED ON";
#if VERISIGN
			result = AppLogic.ro_OK;
			PFProCOMLib.PNComClass vsnGate = new PFProCOMLib.PNComClass();
			int TO = AppLogic.AppConfigUSInt("Verisign_Timeout");
			if(TO == 0)
			{
				TO = 60;
			}
			vsnGate.TimeOut = TO.ToString();

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
            
			StringBuilder transactionCommand = new StringBuilder(4096);

			if(RefundAmount != System.Decimal.Zero)
			{
				return "Partial refunds not supported in Verisign PayFlow PRO Gateway";
			}

			transactionCommand.Append("TRXTYPE=C&TENDER=C&COMMENT1=" + AppLogic.AppConfig("StoreName").Replace("&","").Replace("=","") + " Refund");
			transactionCommand.Append("&PWD=" + AppLogic.AppConfig("Verisign_PWD"));
			transactionCommand.Append("&USER=" + AppLogic.AppConfig("Verisign_USER"));
			transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("Verisign_VENDOR"));
			transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("Verisign_PARTNER"));
			transactionCommand.Append("&ORIGID=" + TransID);

			DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OriginalOrderNumber.ToString());

			if(TransID.Length == 0 || TransID == "0")
			{
				result = "Invalid or Empty Transaction ID";
			}
			else
			{
				try
				{
					int Ctx1;
					if(AppLogic.AppConfigBool("UseLiveTransactions"))
					{
						Ctx1 = vsnGate.CreateContext("payflow.verisign.com",443,30,String.Empty,0,String.Empty,String.Empty);
					}
					else
					{
						Ctx1 = vsnGate.CreateContext("test-payflow.verisign.com",443,30,String.Empty,0,String.Empty,String.Empty);
					}
					String rawResponseString = vsnGate.SubmitTransaction(Ctx1, transactionCommand.ToString(), transactionCommand.Length);
					vsnGate.DestroyContext (Ctx1);

					String[] statusArray = rawResponseString.Split('&');
					String replyCode = String.Empty;
					String replyMsg = String.Empty;
					String PNREF = String.Empty;
					for(int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
					{
						String[] lasKeyPair = statusArray[i].Split('=');
						switch(lasKeyPair[0].ToLowerInvariant())
						{
							case "result":
								replyCode = lasKeyPair[1];
								break;
							case "pnref":
								PNREF = lasKeyPair[1];
								break;
							case "respmsg":
								replyMsg = lasKeyPair[1];
								break;
						}
					}

					DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString)  + " where OrderNumber=" + OriginalOrderNumber.ToString());
					if(replyCode == "0")
					{
						result = AppLogic.ro_OK;
					}
					else
					{
						result = replyMsg;
					}
				}
				catch
				{
					result = "NO RESPONSE FROM GATEWAY!";
				}
			}
#endif
            return result;
        }

        // processes card in real time:
        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = "VERISIGN COM COMPONENTS NOT INSTALLED ON SERVER OR STOREFRONT NOT COMPILED WITH VERISIGN CODE TURNED ON";
            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            AVSResult = String.Empty;
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;
#if VERISIGN
			PFProCOMLib.PNComClass vsnGate = new PFProCOMLib.PNComClass();
			int TO = AppLogic.AppConfigUSInt("Verisign_Timeout");
			if(TO == 0)
			{
				TO = 60;
			}
			vsnGate.TimeOut = TO.ToString();

			StringBuilder transactionCommand = new StringBuilder(4096);
			String rawResponseString;
			String replyCode = String.Empty;
			String responseCode = String.Empty;
			String authResponse = String.Empty;
			String approvalCode = String.Empty;
			String orderTotalString;

			if(!useLiveTransactions)
			{
				OrderTotal = 1.0M;
			}
			orderTotalString = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);

			transactionCommand.Append("TRXTYPE=" + CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth , "A", "S") + "&TENDER=C&ZIP=" + UseBillingAddress.Zip + "&COMMENT1=Order " + OrderNumber + "&COMMENT2=CustomerID " + CustomerID.ToString());
			transactionCommand.Append("&PWD=" + AppLogic.AppConfig("Verisign_PWD"));
			transactionCommand.Append("&USER=" + AppLogic.AppConfig("Verisign_USER"));
			transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("Verisign_VENDOR"));
			transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("Verisign_PARTNER"));

			//set the amount 
			transactionCommand.Append("&AMT=" + orderTotalString);
			
			transactionCommand.Append("&ACCT=" + UseBillingAddress.CardNumber);
			//set the expiration date form the HTML form
			transactionCommand.Append("&EXPDATE=" + UseBillingAddress.CardExpirationMonth.PadLeft(2,'0') + UseBillingAddress.CardExpirationYear.Substring(2,2));

			//set the CSC code:
			if (CardExtraCode.Trim().Length != 0)
			{
				transactionCommand.Append("&CSC2MATCH=" + CardExtraCode);
				transactionCommand.Append("&CVV2=" + CardExtraCode);
			}

			if(UseShippingAddress != null)
			{
				transactionCommand.Append("&SHIPTOSTREET=" + UseShippingAddress.Address1.Replace("&","").Replace("=",""));
				transactionCommand.Append("&SHIPTOCITY=" + UseShippingAddress.City.Replace("&","").Replace("=",""));
				transactionCommand.Append("&SHIPTOSTATE=" + UseShippingAddress.State.Replace("&","").Replace("=",""));
				transactionCommand.Append("&SHIPTOZIP=" + UseShippingAddress.Zip.Replace("&","").Replace("=",""));
				transactionCommand.Append("&SHIPTOCOUNTRY=" + UseShippingAddress.Country.Replace("&","").Replace("=","")); //Verisign documentation says it's SHIPTOCOUNTRY but support says it's COUNTRYCODE which is the one that worked for me
				transactionCommand.Append("&COUNTRYCODE=" + UseShippingAddress.Country.Replace("&","").Replace("=","")); //Verisign documentation says it's SHIPTOCOUNTRY but support says it's COUNTRYCODE which is the one that worked for me
			}

			transactionCommand.Append("&STREET=" + UseBillingAddress.Address1.Replace("&","").Replace("=",""));
			transactionCommand.Append("&CITY=" + UseBillingAddress.City.Replace("&","").Replace("=",""));
			transactionCommand.Append("&STATE=" + UseBillingAddress.State.Replace("&","").Replace("=",""));
			transactionCommand.Append("&ZIP=" + UseBillingAddress.Zip.Replace("&","").Replace("=",""));
			transactionCommand.Append("&COUNTRY=" + UseBillingAddress.Country.Replace("&","").Replace("=",""));
			transactionCommand.Append("&CUSTIP=" + CommonLogic.CustomerIpAddress().Replace("&","").Replace("=","")); //cart.ThisCustomer.LastIPAddress);
			transactionCommand.Append("&EMAIL=" + UseBillingAddress.EMail.Replace("&","").Replace("=",""));

			if(CAVV.Length != 0)
			{
				transactionCommand.Append("&CAVV[" + CAVV.Length.ToString() + "]=" + CAVV);
		 		transactionCommand.Append("&ECI=" + ECI);
 				//transactionCommand.Append("&XID=" + XID);
			}

			int Ctx1;
			if(AppLogic.AppConfigBool("UseLiveTransactions"))
			{
				Ctx1 = vsnGate.CreateContext("payflow.verisign.com",443,30,String.Empty,0,String.Empty,String.Empty);
			}
			else
			{
				Ctx1 = vsnGate.CreateContext("test-payflow.verisign.com",443,30,String.Empty,0,String.Empty,String.Empty);
			}
			String curString = vsnGate.SubmitTransaction(Ctx1, transactionCommand.ToString(), transactionCommand.Length);
			rawResponseString = curString;
			vsnGate.DestroyContext (Ctx1);

			bool AVSOK = true;
			String AVSAddr = String.Empty;
			String AVSZip = String.Empty;

			String[] statusArray = curString.Split('&');
			for(int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
			{
				String[] lasKeyPair = statusArray[i].Split('=');
				switch(lasKeyPair[0].ToLowerInvariant())
				{
					case "result":
						replyCode = lasKeyPair[1];
						break;
					case "pnref":
						responseCode = lasKeyPair[1];
						break;
					case "respmsg":
						authResponse = lasKeyPair[1];
						break;
					case "authcode":
						approvalCode = lasKeyPair[1];
						break;
					case "avsaddr":
						AVSAddr = lasKeyPair[1];
						break;
					case "avszip":
						AVSZip = lasKeyPair[1];
						break;
				}
			}

			// ok, how to handle this? Bank doesn't decline based on AVS info, so we can't either...as the card has already been charged!

//			if(AppLogic.AppConfigBool("Verisign_Verify_Addresses"))
//			{
//				AVSOK = false;
//				if(AVSAddr == "Y" || AVSZip == "Y")
//				{
//					AVSOK = true;
//				}
//			}
			
			AuthorizationCode = approvalCode;
			AuthorizationResult = rawResponseString;
			AuthorizationTransID = responseCode;
			AVSResult =  String.Empty;
			TransactionCommandOut = transactionCommand.ToString();
			
			if(replyCode == "0" && AVSOK)
			{
				result = AppLogic.ro_OK;
			}
			else
			{
				result = authResponse;
				if(result.Length == 0)
				{
					result = "Unspecified Error";
				}
				result = result.Replace("account","card");
				result = result.Replace("Account","Card");
				result = result.Replace("ACCOUNT","CARD");
			}
#endif
            return result;
        }

        public override string ProcessAutoBillStatusFile(String GW, string StatusFile, out string Results, RecurringOrderMgr OrderManager)
        {
            String Status = AppLogic.ro_OK;
            StringBuilder tmpS = new StringBuilder();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(StatusFile);
                String Separator = String.Empty;
                foreach (XmlNode n in doc.SelectNodes("/RecurringBillingReport/TX"))
                {
                    tmpS.Append(Separator);
                    String TxSubID = XmlCommon.XmlAttribute(n, "RecurringSubscriptionID");
                    String TxStatus = XmlCommon.XmlAttribute(n, "Status").ToUpperInvariant();
                    String TxMsg = XmlCommon.XmlAttribute(n, "Message");
                    String TxID = XmlCommon.XmlAttribute(n, "PNREF");
                    String TxTime = XmlCommon.XmlAttribute(n, "Time");

                    DateTime dtTx = Localization.ParseNativeDateTime(TxTime);

                    int OrigOrdNumber = AppLogic.GetOriginalRecurringOrderNumberFromSubscriptionID(TxSubID);
                    tmpS.Append("Processing ID ");
                    tmpS.Append(TxSubID);
                    tmpS.Append(",");
                    tmpS.Append(TxStatus);
                    tmpS.Append("=");
                    try
                    {
                        String tmpStatus = String.Empty;

                        if (OrigOrdNumber == 0)
                        {
                            if (dtTx.Equals(DateTime.MinValue))
                            {
                                dtTx = DateTime.Now;
                            }
                            tmpStatus = "No Original Order Found";
                            if (TxID.Length != 0)
                            {
                                tmpStatus += ", PNREF=" + TxID;
                            }
                            DB.ExecuteSQL("insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult,CustomerEMailed,RecurringSubscriptionID) values(" +
                                "0,0,'0.0.0.0'," + DB.DateQuote(dtTx) + "," + DB.SQuote(GW) + "," +
                                DB.SQuote(AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()) + "," + DB.SQuote(AppLogic.ro_NotApplicable) + "," + DB.SQuote(tmpStatus) + ",0," + DB.SQuote(TxSubID) + ")");
                        }
                        else
                        {
                            if (TxStatus == "APPROVED")
                            {
                                int NewOrderNumber = 0;
                                tmpStatus = OrderManager.ProcessAutoBillApproved(OrigOrdNumber, TxID, dtTx, out NewOrderNumber);
                            }
                            else
                            {
                                tmpStatus = OrderManager.ProcessAutoBillDeclined(OrigOrdNumber, TxID, dtTx, TxSubID, TxMsg);
                            }
                            if (tmpStatus == AppLogic.ro_OK)
                            {
                                // mark this one as processed ok
                                // TBD
                            }
                            else
                            {
                                // mark this record as not processed
                                // TBD

                                int ProcessCustomerID = Order.GetOrderCustomerID(OrigOrdNumber);
                                Customer ProcessCustomer = new Customer(ProcessCustomerID, true);

                                if (dtTx.Equals(DateTime.MinValue))
                                {
                                    dtTx = DateTime.Now;
                                }
                                if (TxID.Length != 0)
                                {
                                    tmpStatus += ", PNREF=" + TxID;
                                }
                                DB.ExecuteSQL("insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult,CustomerEMailed,RecurringSubscriptionID) values(" +
                                    ProcessCustomer.CustomerID.ToString() + "," + OrigOrdNumber.ToString() + "," +
                                    DB.SQuote(ProcessCustomer.LastIPAddress) + "," + DB.DateQuote(dtTx) + "," + DB.SQuote(GW) + "," +
                                    DB.SQuote(AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()) + "," + DB.SQuote(AppLogic.ro_NotApplicable) + "," + DB.SQuote(tmpStatus) + ",0," + DB.SQuote(TxSubID) + ")");
                            }
                        }
                        tmpS.Append(tmpStatus);
                    }
                    catch (Exception ex)
                    {
                        tmpS.Append(ex.Message);
                    }
                    Separator = "\n";
                }
                Status = AppLogic.ro_OK;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
            Results = tmpS.ToString();
            return Status;
        }

        public override RecurringSupportType RecurringSupportType()
        {
            return Processors.RecurringSupportType.Normal;
        }
    }
}
