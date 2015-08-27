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
    /// Summary description for PinnaclePayments.
    /// </summary>
    public class PinnaclePayments : GatewayProcessor
    {
        public PinnaclePayments() { }

        public override String CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;

            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = o.AuthorizationPNREF;
            Decimal OrderTotal = o.OrderBalance;

            if (TransID.Length == 0)
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                PPSCommerce.SmartPayments svc = new PPSCommerce.SmartPayments();
                svc.Url = AppLogic.AppConfig("PinnaclePayments.SOAPURL");
                PPSCommerce.Response rsp = null;

                try
                {
                    int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
                    int CurrentTry = 0;
                    bool CallSuccessful = false;
                    do
                    {
                        try
                        {
                            rsp = svc.ProcessCreditCard(AppLogic.AppConfig("PinnaclePayments.UserName"),AppLogic.AppConfig("PinnaclePayments.Password"),"Capture",String.Empty,String.Empty,String.Empty,String.Empty,Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal),String.Empty,TransID.ToString(),String.Empty,String.Empty,String.Empty,String.Empty);
                            CurrentTry++;
                            CallSuccessful = true;
                        }
                        catch (Exception ex)
                        {
                            CallSuccessful = false;
                            result = "Error Calling PinnaclePayments SmartPayments Payment Gateway: " + ex.Message;
                        }
                    }
                    while (!CallSuccessful && CurrentTry < MaxTries);

                    if (CallSuccessful && rsp != null)
                    {
                        if (rsp.Result == 0)
                        {
                            result = AppLogic.ro_OK;
                        }
                        else
                        {
                            result = rsp.RespMSG;
                        }
                    }
                }
                catch
                {
                    result = "Error calling PinnaclePayments SmartPayments gateway for Capture. Please retry your order in a few minutes or select another checkout payment option.";
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

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                    }
                }
            }

            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(AppLogic.ro_NotApplicable) + " where OrderNumber=" + OrderNumber.ToString());

            if (TransID.Length == 0)
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                PPSCommerce.SmartPayments svc = new PPSCommerce.SmartPayments();
                svc.Url = AppLogic.AppConfig("PinnaclePayments.SOAPURL");
                PPSCommerce.Response rsp = null;

                try
                {
                    int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
                    int CurrentTry = 0;
                    bool CallSuccessful = false;
                    do
                    {
                        try
                        {
                            rsp = svc.ProcessCreditCard(AppLogic.AppConfig("PinnaclePayments.UserName"),AppLogic.AppConfig("PinnaclePayments.Password"),"Void",String.Empty,String.Empty,String.Empty,String.Empty,String.Empty,String.Empty,TransID.ToString(),String.Empty,String.Empty,String.Empty,String.Empty);
                            CurrentTry++;
                            CallSuccessful = true;
                        }
                        catch (Exception ex)
                        {
                            CallSuccessful = false;
                            result = "Error Calling PinnaclePayments SmartPayments Payment Gateway: " + ex.Message;
                        }
                    }
                    while (!CallSuccessful && CurrentTry < MaxTries);

                    if (CallSuccessful && rsp != null)
                    {
                        if (rsp.Result == 0)
                        {
                            result = AppLogic.ro_OK;
                        }
                        else
                        {
                            result = rsp.RespMSG;
                        }
                    }
                }
                catch
                {
                    result = "Error calling PinnaclePayments SmartPayments gateway for Void. Please retry your order in a few minutes or select another checkout payment option.";
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
            String Last4 = String.Empty;
            int CustomerID = 0;
            Decimal OrderTotal = System.Decimal.Zero;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        Last4 = DB.RSField(rs, "Last4");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                        CustomerID = DB.RSFieldInt(rs, "CustomerID");
                    }
                }
            }

            RefundAmount = CommonLogic.IIF(RefundAmount == System.Decimal.Zero, OrderTotal, RefundAmount);

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                PPSCommerce.SmartPayments svc = new PPSCommerce.SmartPayments();
                svc.Url = AppLogic.AppConfig("PinnaclePayments.SOAPURL");
                PPSCommerce.Response rsp = null;

                try
                {
                    int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
                    int CurrentTry = 0;
                    bool CallSuccessful = false;
                    do
                    {
                        try
                        {
                            rsp = svc.ProcessCreditCard(AppLogic.AppConfig("PinnaclePayments.UserName"),AppLogic.AppConfig("PinnaclePayments.Password"),"Return",String.Empty,String.Empty,String.Empty,String.Empty,Localization.DecimalStringForDB(RefundAmount),OriginalOrderNumber.ToString(),TransID.ToString(),String.Empty,String.Empty,String.Empty,String.Empty);
                            CurrentTry++;
                            CallSuccessful = true;
                        }
                        catch (Exception ex)
                        {
                            CallSuccessful = false;
                            result = "Error Calling PinnaclePayments SmartPayments Payment Gateway: " + ex.Message;
                        }
                    }
                    while (!CallSuccessful && CurrentTry < MaxTries);

                    if (CallSuccessful && rsp != null)
                    {
                        if (rsp.Result == 0)
                        {
                            result = AppLogic.ro_OK;
                        }
                        else
                        {
                            result = rsp.RespMSG;
                        }
                    }
                }
                catch
                {
                    result = "Error calling PinnaclePayments SmartPayments gateway for Void. Please retry your order in a few minutes or select another checkout payment option.";
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

            String ExtData = String.Format("<CustomerID>{0}</CustomerID><City>{1}</City><BillToState>{2}</BillToState><CVPresence>{3}</CVPresence>", 
                CustomerID.ToString(), 
                XmlCommon.XmlEncode(UseBillingAddress.City), 
                XmlCommon.XmlEncode(UseBillingAddress.State),
                CommonLogic.IIF(CardExtraCode.Length == 0, "None","Submitted"));

            if (ECI.Length != 0)
            {
                // Verified By Visa/MasterCare Secure (TBD):
                ExtData += String.Format("Authentication=<XID>{0}</XID><CAVV>{1}</CAVV>", XID, CAVV);
            }

            PPSCommerce.SmartPayments svc = new PPSCommerce.SmartPayments();
            svc.Url = AppLogic.AppConfig("PinnaclePayments.SOAPURL");
            PPSCommerce.Response rsp = null;

            try
            {
                int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
                int CurrentTry = 0;
                bool CallSuccessful = false;
                do
                {
                    try
                    {

                    rsp = svc.ProcessCreditCard(
                        AppLogic.AppConfig("PinnaclePayments.UserName"), 
                        AppLogic.AppConfig("PinnaclePayments.Password"), 
                        CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "Auth", "Sale"), 
                        UseBillingAddress.CardNumber, 
                        UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + UseBillingAddress.CardExpirationYear.PadLeft(4, '0').Substring(2, 2), 
                        String.Empty, 
                        UseBillingAddress.CardName,
                        Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal), 
                        OrderNumber.ToString(), 
                        String.Empty, 
                        UseBillingAddress.Zip, 
                        UseBillingAddress.Address1, 
                        CardExtraCode, 
                        ExtData);

                        CurrentTry++;

                        CallSuccessful = true;
                    }
                    catch(Exception ex)
                    {
                        CallSuccessful = false;
                        TransactionResponse = "Error Calling PinnaclePayments SmartPayments Payment Gateway: " + ex.Message;
                    }
                }
                while (!CallSuccessful && CurrentTry < MaxTries);

                if (CallSuccessful && rsp != null)
                {
                    AuthorizationCode = rsp.AuthCode;
                    AuthorizationResult = rsp.Message;
                    AuthorizationTransID = rsp.PNRef;
                    AVSResult = rsp.GetAVSResultTXT;

                    if (rsp.Result == 0)
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = rsp.RespMSG;
                    }
                }
            }
            catch
            {
                result = "Error calling PinnaclePayments SmartPayments gateway. Please retry your order in a few minutes or select another checkout payment option.";
            }
            return result;
        }


    }
}
