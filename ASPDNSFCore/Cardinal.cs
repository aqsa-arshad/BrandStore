// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for Cardinal.
	/// </summary>
	public class Cardinal
	{
		public Cardinal() {}

		static public bool EnabledForCheckout(decimal cartTotal, string cardType)
		{
			var CardinalAllowed = AppLogic.AppConfigBool("CardinalCommerce.Centinel.Enabled") && 
				!(cartTotal == System.Decimal.Zero && 
				AppLogic.AppConfigBool("SkipPaymentEntryOnZeroDollarCheckout"));

			return CardinalAllowed && (cardType.Trim().Equals("VISA", StringComparison.InvariantCultureIgnoreCase) ||
				cardType.Trim().Equals("MASTERCARD", StringComparison.InvariantCultureIgnoreCase) ||
				cardType.Trim().Equals("JCB", StringComparison.InvariantCultureIgnoreCase));
		}

		static public bool PreChargeLookupAndStoreSession(Customer customer, int orderNumber, decimal cartTotal, string cardNumber, string expMonth, string expYear)
		{
			// use cardinal pre-auth fraud screening:
			String ACSUrl = String.Empty;
			String Payload = String.Empty;
			String TransactionID = String.Empty;
			String CardinalLookupResult = String.Empty;

			if (Cardinal.PreChargeLookup(cardNumber, Localization.ParseUSInt(expYear), Localization.ParseUSInt(expMonth), orderNumber, cartTotal, "",
				out ACSUrl, out Payload, out TransactionID, out CardinalLookupResult))
			{
				// redirect to intermediary page which gets card password from user:
				customer.ThisCustomerSession["Cardinal.LookupResult"] = CardinalLookupResult;
				customer.ThisCustomerSession["Cardinal.ACSUrl"] = ACSUrl;
				customer.ThisCustomerSession["Cardinal.Payload"] = Payload;
				customer.ThisCustomerSession["Cardinal.TransactionID"] = TransactionID;
				customer.ThisCustomerSession["Cardinal.OrderNumber"] = orderNumber.ToString();

				return true;
			}
			else
			{
				customer.ThisCustomerSession["Cardinal.LookupResult"] = CardinalLookupResult;
			}

			return false;
		}

		static public string GetECIFlag(string cardType)
		{
			// set the ECIFlag for an 'N' Enrollment response, so the merchant receives Liability Shift Protection
			string ECIFlag;
			if (cardType.Trim().Equals("VISA", StringComparison.InvariantCultureIgnoreCase))
			{
				ECIFlag = "06";  // Visa Card Issuer Liability
			}
			else if (cardType.Trim().Equals("JCB", StringComparison.InvariantCultureIgnoreCase))
			{
				ECIFlag = "07";  // Indicates Merchant Liability 
			}
			else
			{
				ECIFlag = "01";  // MasterCard Merchant Liability for non-enrolled card (rules differ between MC and Visa in the regard)
			}

			return ECIFlag;
		}

		// returns true if no errors, and card is enrolled:
		static public bool PreChargeLookup(String CardNumber, int CardExpirationYear, int CardExpirationMonth, int OrderNumber, decimal OrderTotal, String OrderDescription, out String ACSUrl, out String Payload, out String TransactionID, out String CardinalLookupResult)
		{
			CardinalCommerce.CentinelRequest ccRequest = new CardinalCommerce.CentinelRequest();
			CardinalCommerce.CentinelResponse ccResponse = new CardinalCommerce.CentinelResponse();

			// ==================================================================================
			// Construct the cmpi_lookup message
			// ==================================================================================

			ccRequest.add("MsgType", AppLogic.AppConfig("CardinalCommerce.Centinel.MsgType.Lookup"));
            ccRequest.add("Version", "1.7");
            ccRequest.add("ProcessorId", AppLogic.AppConfig("CardinalCommerce.Centinel.ProcessorID"));
			ccRequest.add("MerchantId", AppLogic.AppConfig("CardinalCommerce.Centinel.MerchantID"));
            ccRequest.add("TransactionPwd", AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionPwd"));
            ccRequest.add("TransactionType", "C"); //C = Credit Card / Debit Card Authentication.
            ccRequest.add("Amount", Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal).Replace(",", "").Replace(".", ""));
            ccRequest.add("CurrencyCode", Localization.StoreCurrencyNumericCode());
            ccRequest.add("CardNumber", CardNumber);
            ccRequest.add("CardExpMonth", CardExpirationMonth.ToString().PadLeft(2, '0'));
            ccRequest.add("CardExpYear", CardExpirationYear.ToString().PadLeft(4, '0'));
            ccRequest.add("OrderNumber", OrderNumber.ToString());

            // Optional fields
            ccRequest.add("OrderDescription", OrderDescription);
            ccRequest.add("UserAgent", CommonLogic.ServerVariables("HTTP_USER_AGENT"));
            ccRequest.add("Recurring", "N");
			
			int NumAttempts = AppLogic.AppConfigUSInt("CardinalCommerce.Centinel.NumRetries");
			if(NumAttempts == 0)
			{
				NumAttempts = 1;
			}
			bool CallWasOK = false;
			for(int i = 1; i <= NumAttempts; i++)
			{
				CallWasOK = true;
				try
				{
					String URL = AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionUrl.Test");
					if(AppLogic.AppConfigBool("CardinalCommerce.Centinel.IsLive"))
					{
						URL = AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionUrl.Live");
					}
					ccResponse = ccRequest.sendHTTP(URL, AppLogic.AppConfigUSInt("CardinalCommerce.Centinel.MapsTimeout"));
				}
				catch
				{
					CallWasOK = false;
				}
				if(CallWasOK)
				{
					break;
				}
			}

			Payload = String.Empty;
			ACSUrl = String.Empty;
			TransactionID = String.Empty;
			if(CallWasOK)
			{
				String errorNo = ccResponse.getValue("ErrorNo");
				String errorDesc = ccResponse.getValue("ErrorDesc");
				String enrolled = ccResponse.getValue("Enrolled");
				Payload = ccResponse.getValue("Payload");
				ACSUrl = ccResponse.getValue("ACSUrl");
				TransactionID = ccResponse.getValue("TransactionId");

				CardinalLookupResult = ccResponse.getUnparsedResponse();

				ccRequest = null;
				ccResponse = null;

				//======================================================================================
				// Assert that there was no error code returned and the Cardholder is enrolled in the
				// Payment Authentication Program prior to starting the Authentication process.
				//======================================================================================

				if(errorNo == "0" && enrolled == "Y")
				{
					return true;
				}
				return false;
			}
			ccRequest = null;
			ccResponse = null;
			CardinalLookupResult = AppLogic.GetString("cardinal.cs.1",1,Localization.GetDefaultLocale());
			return false;
		}

		static public String PreChargeAuthenticate(int OrderNumber, String PaRes, String TransactionID, out String PAResStatus, out String SignatureVerification, out String ErrorNo, out String ErrorDesc, out String CardinalAuthenticateResult)
		{
			CardinalCommerce.CentinelRequest ccRequest = new CardinalCommerce.CentinelRequest();
			CardinalCommerce.CentinelResponse ccResponse = new CardinalCommerce.CentinelResponse();

			ErrorNo = String.Empty;
			ErrorDesc = String.Empty;
			PAResStatus = String.Empty;
			SignatureVerification = String.Empty;


			if(PaRes.Length == 0 || TransactionID.Length == 0)
			{
				CardinalAuthenticateResult = AppLogic.GetString("cardinal.cs.3",1,Localization.GetDefaultLocale());
				return AppLogic.GetString("cardinal.cs.2",1,Localization.GetDefaultLocale());
			}
			else
			{

				// ==================================================================================
				// Construct the cmpi_authenticate message
				// ==================================================================================

				ccRequest.add("MsgType", AppLogic.AppConfig("CardinalCommerce.Centinel.MsgType.Authenticate")); //cmpi_authenticate
                ccRequest.add("Version", "1.7");
                ccRequest.add("ProcessorId", AppLogic.AppConfig("CardinalCommerce.Centinel.ProcessorID")); 
				ccRequest.add("MerchantId", AppLogic.AppConfig("CardinalCommerce.Centinel.MerchantID"));
                ccRequest.add("TransactionType", "C");
                ccRequest.add("TransactionPwd", AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionPwd"));
				ccRequest.add("TransactionId", TransactionID);
				ccRequest.add("PAResPayload", HttpContext.Current.Server.HtmlEncode(PaRes));

				int NumAttempts = AppLogic.AppConfigUSInt("CardinalCommerce.Centinel.NumRetries");
				if(NumAttempts == 0)
				{
					NumAttempts = 1;
				}
				bool CallWasOK = false;
				for(int i = 1; i <= NumAttempts; i++)
				{
					CallWasOK = true;
					try
					{
						String URL = AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionUrl.Test");
						if(AppLogic.AppConfigBool("CardinalCommerce.Centinel.IsLive"))
						{
							URL = AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionUrl.Live");
						}
						ccResponse = ccRequest.sendHTTP(URL, AppLogic.AppConfigUSInt("CardinalCommerce.Centinel.MapsTimeout"));
					}
					catch
					{
						CallWasOK = false;
					}
					if(CallWasOK)
					{
						break;
					}
				}

				if(CallWasOK)
				{
					ErrorNo = ccResponse.getValue("ErrorNo");
					ErrorDesc = ccResponse.getValue("ErrorDesc");
					String cavv = ccResponse.getValue("Cavv");
					String xid = ccResponse.getValue("Xid");
					PAResStatus = ccResponse.getValue("PAResStatus");
					SignatureVerification = ccResponse.getValue("SignatureVerification");
					String eciflag = ccResponse.getValue("EciFlag");

					//=====================================================================================
					// ************************************************************************************
					//				** Important Note **
					// ************************************************************************************
					//
					// Here you should persist the authentication results to your commerce system. A production
					// integration should, at a minimum, write the PAResStatus, Cavv, EciFlag, Xid to a database
					// for use when sending the authorization message to your gateway provider.
					//
					// Be sure not to simply //pass// the authentication results around from page to page, since
					// the values could be easily spoofed if that technique is used.
					//
					//=====================================================================================

					CardinalAuthenticateResult = ccResponse.getUnparsedResponse();
					String tmpS = ccResponse.getUnparsedResponse();
					ccRequest = null;
					ccResponse = null;
					return tmpS;
				}
				ccRequest = null;
				ccResponse = null;
				CardinalAuthenticateResult = AppLogic.GetString("cardinal.cs.4",1,Localization.GetDefaultLocale());
				return AppLogic.GetString("cardinal.cs.5",1,Localization.GetDefaultLocale());
			}
		}

        static public bool MyECheckLookup(ShoppingCart cart, int OrderNumber, decimal OrderTotal, String OrderDescription, out String ACSUrl, out String Payload, out String TransactionID, out String CardinalLookupResult)
        {
            String MerchantID = AppLogic.AppConfig("CardinalCommerce.Centinel.MerchantID");
            String ProcessorID = AppLogic.AppConfig("CardinalCommerce.Centinel.ProcessorID");
            String TransactionPwd = AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionPwd");
            if (MerchantID.Length == 0)
            {
                throw new ArgumentException("You MUST set AppConfig:CardinalCommerce.Centinel.MerchantID to a valid value.");
            }
            if (ProcessorID.Length == 0)
            {
                throw new ArgumentException("You MUST set AppConfig:CardinalCommerce.Centinel.ProcessorID to a valid value.");
            }
            if (TransactionPwd.Length == 0)
            {
                throw new ArgumentException("You MUST set AppConfig:CardinalCommerce.Centinel.TransactionPwd to a valid value.");
            }

            CardinalCommerce.CentinelRequest ccRequest = new CardinalCommerce.CentinelRequest();
            CardinalCommerce.CentinelResponse ccResponse = new CardinalCommerce.CentinelResponse();

            // ==================================================================================
            // Construct the cmpi_lookup message
            // ==================================================================================

            String IPAddress = cart.ThisCustomer.LastIPAddress;

            ccRequest.add("MsgType", "cmpi_lookup");
            ccRequest.add("Version", "1.7");
            ccRequest.add("MerchantId", MerchantID);
            ccRequest.add("ProcessorId", ProcessorID);
            ccRequest.add("TransactionPwd", TransactionPwd);
            ccRequest.add("TransactionType", "ME");
            ccRequest.add("Amount", Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal).Replace(",", "").Replace(".", ""));
            ccRequest.add("CurrencyCode", Localization.StoreCurrencyNumericCode());
            ccRequest.add("IPAddress", IPAddress);
            ccRequest.add("EMail", cart.ThisCustomer.EMail);
            ccRequest.add("OrderNumber", OrderNumber.ToString());
            ccRequest.add("OrderDescription", AppLogic.AppConfig("StoreName") + " Purchase");            
            ccRequest.add("BillingFirstName", cart.ThisCustomer.PrimaryBillingAddress.FirstName);
            ccRequest.add("BillingLastName", cart.ThisCustomer.PrimaryBillingAddress.LastName);
            ccRequest.add("BillingAddress1", cart.ThisCustomer.PrimaryBillingAddress.Address1);
            ccRequest.add("BillingAddress2", cart.ThisCustomer.PrimaryBillingAddress.Address2);
            ccRequest.add("BillingCity", cart.ThisCustomer.PrimaryBillingAddress.City);
            ccRequest.add("BillingState", cart.ThisCustomer.PrimaryBillingAddress.State);
            ccRequest.add("BillingPostalCode", cart.ThisCustomer.PrimaryBillingAddress.Zip);
            ccRequest.add("BillingCountryCode", AppLogic.GetCountryTwoLetterISOCode(cart.ThisCustomer.PrimaryBillingAddress.Country));
            ccRequest.add("BillingPhone", cart.ThisCustomer.PrimaryBillingAddress.Phone.Replace("(","").Replace(")","").Replace("-","").Replace(" ",""));


            int NumAttempts = AppLogic.AppConfigUSInt("CardinalCommerce.Centinel.NumRetries");
            if (NumAttempts == 0)
            {
                NumAttempts = 1;
            }
            bool CallWasOK = false;
            for (int i = 1; i <= NumAttempts; i++)
            {
                CallWasOK = true;
                try
                {
                    String URL = AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionUrl.Test");
                    if (AppLogic.AppConfigBool("CardinalCommerce.Centinel.IsLive"))
                    {
                        URL = AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionUrl.Live");
                    }
                    ccResponse = ccRequest.sendHTTP(URL, AppLogic.AppConfigUSInt("CardinalCommerce.Centinel.MapsTimeout"));
                }
                catch
                {
                    CallWasOK = false;
                }
                if (CallWasOK)
                {
                    break;
                }
            }

            Payload = String.Empty;
            ACSUrl = String.Empty;
            TransactionID = String.Empty;
            if (CallWasOK)
            {
                String errorNo = ccResponse.getValue("ErrorNo");
                String errorDesc = ccResponse.getValue("ErrorDesc");
                String enrolled = ccResponse.getValue("Enrolled");
                Payload = ccResponse.getValue("Payload");
                ACSUrl = ccResponse.getValue("ACSUrl");
                TransactionID = ccResponse.getValue("TransactionId");

                CardinalLookupResult = ccResponse.getUnparsedResponse();

                //======================================================================================
                // Assert that there was no error code returned and the Cardholder is enrolled in the
                // Payment Authentication Program prior to starting the Authentication process.
                //======================================================================================

                if (enrolled == "Y")
                {
                    return true;
                }
                // write to Failed Transaction table
                String sql = "insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult) values(" + cart.ThisCustomer.CustomerID.ToString() + "," + OrderNumber.ToString() + "," + DB.SQuote(IPAddress) + ",getdate(),'Cardinal'," + DB.SQuote(AppLogic.ro_PMCardinalMyECheck) + "," + DB.SQuote(ccRequest.getUnparsedRequest()) + "," + DB.SQuote(CardinalLookupResult) + ")";
                DB.ExecuteSQL(sql);
                ccRequest = null;
                ccResponse = null;
                return false;
            }
            CardinalLookupResult = AppLogic.GetString("cardinal.cs.6", 1, Localization.GetDefaultLocale());
            String sql2 = "insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult) values(" + cart.ThisCustomer.CustomerID.ToString() + "," + OrderNumber.ToString() + "," + DB.SQuote(IPAddress) + ",getdate(),'Cardinal'," + DB.SQuote(AppLogic.ro_PMCardinalMyECheck) + "," + DB.SQuote(ccRequest.getUnparsedRequest()) + "," + DB.SQuote(CardinalLookupResult) + ")";
            DB.ExecuteSQL(sql2);
            ccRequest = null;
            ccResponse = null;
            return false;
        }

        static public String MyECheckAuthenticate(int OrderNumber, String PaRes, String TransactionID, out String OrderId, out String PAResStatus, out String SignatureVerification, out String ErrorNo, out String ErrorDesc, out String CardinalAuthenticateResult)
        {
            CardinalCommerce.CentinelRequest ccRequest = new CardinalCommerce.CentinelRequest();
            CardinalCommerce.CentinelResponse ccResponse = new CardinalCommerce.CentinelResponse();

            ErrorNo = String.Empty;
            ErrorDesc = String.Empty;
            PAResStatus = String.Empty;
            OrderId = String.Empty;
            SignatureVerification = String.Empty;


            if (PaRes.Length == 0 || TransactionID.Length == 0)
            {
                CardinalAuthenticateResult = AppLogic.GetString("cardinal.cs.8", 1, Localization.GetDefaultLocale());
                return AppLogic.GetString("cardinal.cs.7", 1, Localization.GetDefaultLocale());
            }
            else
            {

                // ==================================================================================
                // Construct the cmpi_authenticate message
                // ==================================================================================

                ccRequest.add("MsgType", "cmpi_authenticate");
                ccRequest.add("Version", "1.7");
                ccRequest.add("MerchantId", AppLogic.AppConfig("CardinalCommerce.Centinel.MerchantID"));
                ccRequest.add("ProcessorId", AppLogic.AppConfig("CardinalCommerce.Centinel.ProcessorID"));
                ccRequest.add("TransactionPwd", AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionPwd"));
                ccRequest.add("TransactionType", "ME");
                ccRequest.add("TransactionId", TransactionID);
                ccRequest.add("PAResPayload", HttpContext.Current.Server.HtmlEncode(PaRes));

                int NumAttempts = AppLogic.AppConfigUSInt("CardinalCommerce.Centinel.NumRetries");
                if (NumAttempts == 0)
                {
                    NumAttempts = 1;
                }
                bool CallWasOK = false;
                for (int i = 1; i <= NumAttempts; i++)
                {
                    CallWasOK = true;
                    try
                    {
                        String URL = AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionUrl.Test");
                        if (AppLogic.AppConfigBool("CardinalCommerce.Centinel.IsLive"))
                        {
                            URL = AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionUrl.Live");
                        }
                        ccResponse = ccRequest.sendHTTP(URL, AppLogic.AppConfigUSInt("CardinalCommerce.Centinel.MapsTimeout"));
                    }
                    catch
                    {
                        CallWasOK = false;
                    }
                    if (CallWasOK)
                    {
                        break;
                    }
                }

                if (CallWasOK)
                {
                    ErrorNo = ccResponse.getValue("ErrorNo");
                    ErrorDesc = ccResponse.getValue("ErrorDesc");
                    OrderId = ccResponse.getValue("OrderId");
                    PAResStatus = ccResponse.getValue("PAResStatus");
                    SignatureVerification = ccResponse.getValue("SignatureVerification");

                    CardinalAuthenticateResult = ccResponse.getUnparsedResponse();
                    String tmpS = ccResponse.getUnparsedResponse();
                    ccRequest = null;
                    ccResponse = null;
                    return tmpS;
                }
                ccRequest = null;
                ccResponse = null;
                CardinalAuthenticateResult = AppLogic.GetString("cardinal.cs.9", 1, Localization.GetDefaultLocale());
                return AppLogic.GetString("cardinal.cs.10", 1, Localization.GetDefaultLocale());
            }
        }

        /// <summary>
        /// This is not used for getting a follow up status on check clears, but is only a backup measure if
        /// the browser does not return to the storefront and complete the order properly.
        /// </summary>
        static public String MyECheckStatus(String NotificationID, out String OrderId, out String PAResStatus, out String SignatureVerification, out String ErrorNo, out String ErrorDesc, out String CardinalAuthenticateResult)
        {
            CardinalCommerce.CentinelRequest ccRequest = new CardinalCommerce.CentinelRequest();
            CardinalCommerce.CentinelResponse ccResponse = new CardinalCommerce.CentinelResponse();

            ErrorNo = String.Empty;
            ErrorDesc = String.Empty;
            PAResStatus = String.Empty;
            OrderId = String.Empty;
            SignatureVerification = String.Empty;
            CardinalAuthenticateResult = String.Empty;


            // ==================================================================================
            // Construct the cmpi_payment_status message
            // ==================================================================================

            ccRequest.add("MsgType", "cmpi_payment_status");
            ccRequest.add("Version", "1.7");
            ccRequest.add("MerchantId", AppLogic.AppConfig("CardinalCommerce.Centinel.MerchantID"));
            ccRequest.add("ProcessorId", AppLogic.AppConfig("CardinalCommerce.Centinel.ProcessorID"));
            ccRequest.add("TransactionPwd", AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionPwd"));
            ccRequest.add("TransactionType", "ME");
            ccRequest.add("NotificationId", NotificationID);


            int NumAttempts = AppLogic.AppConfigUSInt("CardinalCommerce.Centinel.NumRetries");
            if (NumAttempts == 0)
            {
                NumAttempts = 1;
            }
            bool CallWasOK = false;
            for (int i = 1; i <= NumAttempts; i++)
            {
                CallWasOK = true;
                try
                {
                    String URL = AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionUrl.Test");
                    if (AppLogic.AppConfigBool("CardinalCommerce.Centinel.IsLive"))
                    {
                        URL = AppLogic.AppConfig("CardinalCommerce.Centinel.TransactionUrl.Live");
                    }
                    ccResponse = ccRequest.sendHTTP(URL, AppLogic.AppConfigUSInt("CardinalCommerce.Centinel.MapsTimeout"));
                }
                catch
                {
                    CallWasOK = false;
                }
                if (CallWasOK)
                {
                    break;
                }
            }

            if (CallWasOK)
            {
                string OrderNumber = ccResponse.getValue("OrderNumber");
                ErrorNo = ccResponse.getValue("ErrorNo");
                ErrorDesc = ccResponse.getValue("ErrorDesc");
                OrderId = ccResponse.getValue("OrderId");
                PAResStatus = ccResponse.getValue("PAResStatus");
                SignatureVerification = ccResponse.getValue("SignatureVerification");

                CardinalAuthenticateResult = ccResponse.getUnparsedResponse();
                String tmpS = ccResponse.getUnparsedResponse();
                ccRequest = null;
                ccResponse = null;

                String msg = "Failed to complete order.";
                if (PAResStatus.Trim().Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    msg += " Customer was CHARGED but no order was created.";
                }
                String sql = "insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult) values(0," + OrderNumber.ToString() + ",'',getdate(),'Cardinal'," + DB.SQuote(AppLogic.ro_PMCardinalMyECheck) + "," + DB.SQuote(msg) + "," + DB.SQuote(CardinalAuthenticateResult) + ")";
                DB.ExecuteSQL(sql);
                return tmpS;
            }
            else
            {
                String msg = "MyECheck notification failed.";
                String sql = "insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult) values(0,0,'',getdate(),'Cardinal'," + DB.SQuote(AppLogic.ro_PMCardinalMyECheck) + "," + DB.SQuote(msg) + "," + DB.SQuote("NotificationId=" + NotificationID) + ")";
                DB.ExecuteSQL(sql);
            }
            ccRequest = null;
            ccResponse = null;
            return "MyECheck Status Error";
        }
	}
}
