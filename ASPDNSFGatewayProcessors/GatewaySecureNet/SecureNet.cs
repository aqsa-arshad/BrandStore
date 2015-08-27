// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways.Processors.SecureNetSvc;

namespace AspDotNetStorefrontGateways.Processors
{
	/// <summary>
    /// Summary description for SecureNet.
	/// </summary>
	public class SecureNet : GatewayProcessor
	{

        public SecureNet()
        {
        }

        public override String CaptureOrder(Order o)
		{
            String result = AppLogic.ro_OK;
            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";

            String TransID = o.AuthorizationPNREF;

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    SecureNetSvc.SecureNetPayment oPG = new SecureNetPayment();
                    SecureNetSvc.TransactionInfo oTi = new TransactionInfo();
                    SecureNetSvc.TransactionResponse oTr = new TransactionResponse();

                    oPG.Url = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("SecureNet.LiveURL"), AppLogic.AppConfig("SecureNet.TestURL"));

                    // set oTi values here
                    oTi.SecurenetID = AppLogic.AppConfig("SecureNet.ID");
                    oTi.SecureKey = AppLogic.AppConfig("SecureNet.Key");

                    oTi.Test = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), "FALSE", "TRUE");

                    // AUTH_CAPTURE, AUTH_ONLY, CREDIT, VOID, PRIOR_AUTH_CAPTURE
                    oTi.Type = "PRIOR_AUTH_CAPTURE";

                    oTi.Amount = Localization.CurrencyStringForGatewayWithoutExchangeRate(o.OrderBalance);
                    oTi.Method = "CC"; // Credit Card

                    oTi.Trans_id = TransID;
                    oTi.OrderID = o.OrderNumber.ToString();

                    // o.Last4 is null, so let's just grab it directly
                    oTi.Card_num = DB.GetSqlS("select Last4 S from Orders  with (NOLOCK)  where OrderNumber=" + o.OrderNumber.ToString());
                    oTi.Exp_date = o.CardExpirationMonth.PadLeft(2, '0') + o.CardExpirationYear.ToString().Substring(2, 2); //MMYY

                    oTi.Invoice_num = o.OrderNumber.ToString();
                    oTi.Invoice_Description = AppLogic.AppConfig("StoreName");

                    oTr = oPG.Process(oTi); // Run transaction

                    if (oTr.Response_Code == "1") // 1=Approved, 2=Declined, 3=Error
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = "Error: [" + oTr.Response_Code + "] " + oTr.Response_Reason_Text;
                    }
                    o.CaptureTXCommand = XmlCommon.SerializeObject(oTi, oTi.GetType()).Replace(AppLogic.AppConfig("SecureNet.Key") + "</SecureKey>","*****</SecureKey>");
                    o.CaptureTXResult = XmlCommon.SerializeObject(oTr, oTr.GetType()).Replace(AppLogic.AppConfig("SecureNet.Key") + "</SecureKey>", "*****</SecureKey>"); ;
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
            String result = String.Empty;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            String TransID = String.Empty;
            decimal OrderTotal = 0.0M;
            string Last4 = string.Empty;
            string CardExpirationMonth = string.Empty;
            string CardExpirationYear = string.Empty;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using( IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(),dbconn))
                {
                    if (rs.Read())
                    {
                        // If you are voiding a transaction that has been reauthorized, 
                        // use the ID from the original authorization, and not the reauthorization.
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                        Last4 = DB.RSField(rs, "Last4");
                        CardExpirationMonth = DB.RSField(rs, "CardExpirationMonth");
                        CardExpirationYear = DB.RSField(rs, "CardExpirationYear");
                    }
                }
            
            }
            
            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {

                    SecureNetSvc.SecureNetPayment oPG = new SecureNetPayment();
                    SecureNetSvc.TransactionInfo oTi = new TransactionInfo();
                    SecureNetSvc.TransactionResponse oTr = new TransactionResponse();

                    oPG.Url = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("SecureNet.LiveURL"), AppLogic.AppConfig("SecureNet.TestURL"));

                    // set oTi values here
                    oTi.SecurenetID = AppLogic.AppConfig("SecureNet.ID");
                    oTi.SecureKey = AppLogic.AppConfig("SecureNet.Key");

                    oTi.Test = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), "FALSE", "TRUE");

                    // AUTH_CAPTURE, AUTH_ONLY, CREDIT, VOID, PRIOR_AUTH_CAPTURE
                    oTi.Type = "VOID";

                    oTi.Method = "CC"; // Credit Card

                    oTi.Trans_id = TransID;
                    oTi.OrderID = OrderNumber.ToString();
                    oTi.Amount = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);

                    oTi.Card_num = Last4;
                    oTi.Exp_date = CardExpirationMonth.PadLeft(2, '0') + CardExpirationYear.ToString().Substring(2, 2); //MMYY

                    oTr = oPG.Process(oTi); // Run transaction

                    if (oTr.Response_Code == "1") // 1=Approved, 2=Declined, 3=Error
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = "Error: [" + oTr.Response_Code + "] " + oTr.Response_Reason_Text;
                    }

                    DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(XmlCommon.SerializeObject(oTi, oTi.GetType()).Replace(AppLogic.AppConfig("SecureNet.Key") + "</SecureKey>", "*****</SecureKey>"))
                        + ", VoidTXResult=" + DB.SQuote(XmlCommon.SerializeObject(oTr, oTr.GetType()).Replace(AppLogic.AppConfig("SecureNet.Key") + "</SecureKey>", "*****</SecureKey>")) + " where OrderNumber=" + OrderNumber.ToString());

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
            String result = String.Empty;

            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());
            String TransID = String.Empty;
            Decimal OrderTotal = System.Decimal.Zero;
            string Last4 = string.Empty;
            string CardExpirationMonth = string.Empty;
            string CardExpirationYear = string.Empty;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using(IDataReader rs = DB.GetRS("select * from Orders  with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(),dbconn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                        Last4 = DB.RSField(rs, "Last4");
                        CardExpirationMonth = DB.RSField(rs, "CardExpirationMonth");
                        CardExpirationYear = DB.RSField(rs, "CardExpirationYear");
                    }
                }
            }
            
            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {

                    SecureNetSvc.SecureNetPayment oPG = new SecureNetPayment();
                    SecureNetSvc.TransactionInfo oTi = new TransactionInfo();
                    SecureNetSvc.TransactionResponse oTr = new TransactionResponse();

                    oPG.Url = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("SecureNet.LiveURL"), AppLogic.AppConfig("SecureNet.TestURL"));

                    // set oTi values here
                    oTi.SecurenetID = AppLogic.AppConfig("SecureNet.ID");
                    oTi.SecureKey = AppLogic.AppConfig("SecureNet.Key");

                    oTi.Test = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), "FALSE", "TRUE");

                    // AUTH_CAPTURE, AUTH_ONLY, CREDIT, VOID, PRIOR_AUTH_CAPTURE
                    oTi.Type = "CREDIT";

                    //If partial refund set value ( like 1.95). If FULL Refund leave it empty. The transactionID will take care of the amount
                    if (OrderTotal == RefundAmount || RefundAmount == 0.0M)
                    {
                        oTi.Amount = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);
                    }
                    else
                    {
                        oTi.Amount = Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundAmount);
                    }

                    oTi.Card_num = Last4;
                    oTi.Exp_date = CardExpirationMonth.PadLeft(2, '0') + CardExpirationYear.ToString().Substring(2, 2); //MMYY

                    if (!String.IsNullOrEmpty(RefundReason))
                    {
                        oTi.Notes = RefundReason;
                    }
                    
                    oTi.Method = "CC"; // Credit Card

                    oTi.Trans_id = TransID;
                    oTi.OrderID = OriginalOrderNumber.ToString() + "CREDIT" + CommonLogic.IIF(NewOrderNumber > 0, NewOrderNumber.ToString(), "");

                    oTr = oPG.Process(oTi); // Run transaction

                    if (oTr.Response_Code == "1") // 1=Approved, 2=Declined, 3=Error
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = "Error: [" + oTr.Response_Code + "] " + oTr.Response_Reason_Text;
                    }

                    DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(XmlCommon.SerializeObject(oTi, oTi.GetType()).Replace(AppLogic.AppConfig("SecureNet.Key") + "</SecureKey>", "*****</SecureKey>"))
                        + ", RefundTXResult=" + DB.SQuote(XmlCommon.SerializeObject(oTr, oTr.GetType()).Replace(AppLogic.AppConfig("SecureNet.Key") + "</SecureKey>", "*****</SecureKey>")) + " where OrderNumber=" + OriginalOrderNumber.ToString());

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

            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            AVSResult = String.Empty;
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;

            SecureNetSvc.SecureNetPayment oPG = new SecureNetPayment();
            SecureNetSvc.TransactionInfo oTi = new TransactionInfo();
            SecureNetSvc.TransactionResponse oTr = new TransactionResponse();

            // "https://gateway.securenet.com/payment.asmx"
            oPG.Url = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("SecureNet.LiveURL"), AppLogic.AppConfig("SecureNet.TestURL"));

            // set oTi values here
            oTi.SecurenetID = AppLogic.AppConfig("SecureNet.ID");
            oTi.SecureKey = AppLogic.AppConfig("SecureNet.Key");

            oTi.Test = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), "FALSE", "TRUE");
            oTi.OrderID = OrderNumber.ToString();
            oTi.Amount = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);
            oTi.Method = "CC"; // Credit Card

            // AUTH_CAPTURE, AUTH_ONLY, CREDIT, VOID, PRIOR_AUTH_CAPTURE
            oTi.Type = CommonLogic.IIF(AppLogic.TransactionModeIsAuthOnly(), "AUTH_ONLY", "AUTH_CAPTURE");

            oTi.Card_num = UseBillingAddress.CardNumber;
            oTi.Exp_date = UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + UseBillingAddress.CardExpirationYear.ToString().Substring(2, 2); //MMYY
            oTi.Card_code = CardExtraCode;

            oTi.Customer_id = CustomerID.ToString();
            oTi.First_name = UseBillingAddress.FirstName;
            oTi.Last_name = UseBillingAddress.LastName;
            oTi.Company = UseBillingAddress.Company;
            oTi.Address = UseBillingAddress.Address1;
            oTi.City = UseBillingAddress.City;
            oTi.State = UseBillingAddress.State;
            oTi.Zip = UseBillingAddress.Zip;
            oTi.Country = UseBillingAddress.Country;
            oTi.Phone = UseBillingAddress.Phone;
            oTi.Email = UseBillingAddress.EMail;

            oTi.Invoice_num = OrderNumber.ToString();
            oTi.Invoice_Description = AppLogic.AppConfig("StoreName");

            if (UseShippingAddress != null)
            {
                oTi.Shipping_First_name = UseShippingAddress.FirstName;
                oTi.Shipping_Last_name = UseShippingAddress.LastName;
                oTi.Shipping_Company = UseShippingAddress.Company;
                oTi.Shipping_Address = UseShippingAddress.Address1 + CommonLogic.IIF(UseShippingAddress.Address2 != "", " " + UseShippingAddress.Address2, "") + CommonLogic.IIF(UseShippingAddress.Suite != "", " Ste " + UseShippingAddress.Suite, "");
                oTi.Shipping_City = UseShippingAddress.City;
                oTi.Shipping_State = UseShippingAddress.State;
                oTi.Shipping_Zip = UseShippingAddress.Zip;
                oTi.Shipping_Country = UseShippingAddress.Country;
            }

            if (!String.IsNullOrEmpty(ECI))
            {
                oTi.MPI_Authentication_Indicator = ECI;
            }

            if (!String.IsNullOrEmpty(CAVV))
            {
                oTi.MPI_Authentication_Value = CAVV;
            }

            oTr = oPG.Process(oTi); // Run transaction

            if (oTr.Response_Code == "1") // 1=Approved, 2=Declined, 3=Error
            {
                AuthorizationTransID = oTr.Transaction_ID;
                AuthorizationCode = "Response Code: " + oTr.Response_Code + ", Reason Code: " + oTr.Response_Reason_Code;

                if (!String.IsNullOrEmpty(oTr.AVS_Result_Code))
                {
                    AVSResult = oTr.AVS_Result_Code;
                }

                if (!String.IsNullOrEmpty(oTr.Card_Code_Response_Code))
                {
                    if (AVSResult.Length > 0)
                    {
                        AVSResult += ", ";
                    }
                    AVSResult += "ExtraCode: " + oTr.Card_Code_Response_Code;
                }

                if (!String.IsNullOrEmpty(oTr.CAVV_Response_Code))
                {
                    if (AVSResult.Length > 0)
                    {
                        AVSResult += ", ";
                    }
                    AVSResult += "CAVV: " + oTr.CAVV_Response_Code;
                }

                AuthorizationResult = oTr.Response_Reason_Text + ", Approval Code: " + oTr.Approval_Code;
                result = AppLogic.ro_OK;
            }
            else
            {
                AuthorizationResult = "Error: [" + oTr.Response_Code + "] " + oTr.Response_Reason_Text;
                result = oTr.Response_Reason_Text;
            }

            TransactionCommandOut = XmlCommon.SerializeObject(oTi, oTi.GetType()).Replace(AppLogic.AppConfig("SecureNet.Key") + "</SecureKey>", "*****</SecureKey>");
            TransactionResponse = XmlCommon.SerializeObject(oTr, oTr.GetType()).Replace(AppLogic.AppConfig("SecureNet.Key") + "</SecureKey>", "*****</SecureKey>");

            return result;
        }

        public override String AdministratorSetupPrompt
        {
            get
            {
                return "Deprecated in favor of SecureNetV4.";
            }
        }
	}
}
