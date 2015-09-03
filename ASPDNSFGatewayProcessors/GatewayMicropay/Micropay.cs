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
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for Micropay.
    /// </summary>
    public class Micropay : GatewayProcessor
    {
        public Micropay() { }

        public override String CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;

            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";
            String TransID = o.AuthorizationPNREF;
            Decimal OrderTotal = o.OrderBalance;
            String TransactionState = o.TransactionState;
            int CustomerID = o.CustomerID;

            Decimal mpBalance = AppLogic.GetMicroPayBalance(CustomerID);

            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("x_type=CAPTURE");
            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());
            transactionCommand.Append("&x_trans_id=" + TransID);
            transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));

            o.CaptureTXCommand = transactionCommand.ToString();

            if (TransID.Length == 0)
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                String rawResponseString = String.Empty;
                if (OrderTotal > mpBalance)
                {
                    rawResponseString = "INSUFFICIENT FUNDS";
                    result = rawResponseString;
                }
                else
                {
                    // withdrawl the funds:
                    DB.ExecuteSQL(String.Format("update customer set MicroPayBalance={0} where CustomerID={1}", Localization.CurrencyStringForDBWithoutExchangeRate(mpBalance - OrderTotal), CustomerID.ToString()));
                    rawResponseString = "MICROPAY GATEWAY SAID OK";
                }
                o.CaptureTXResult = rawResponseString;
              
            }

            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());

            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            Decimal OrderTotal = System.Decimal.Zero;
            String TransactionState = String.Empty;
            String TransID = String.Empty;
            int CustomerID = 0;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(String.Format("select AuthorizationPNREF,OrderTotal,CustomerID,TransactionState from orders  with (NOLOCK)  where OrderNumber={0}", OrderNumber), conn))
                {
                    if (rs.Read())
                    {

                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                        CustomerID = DB.RSFieldInt(rs, "CustomerID");
                        TransactionState = DB.RSField(rs, "TransactionState");
                    }                    
                }
            }
            
            Decimal mpBalance = AppLogic.GetMicroPayBalance(CustomerID);

            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("x_type=VOID");
            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());
            transactionCommand.Append("&x_trans_id=" + TransID);

            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OrderNumber.ToString());

            if (TransID.Length == 0)
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                String rawResponseString = String.Empty;
                if (TransactionState == AppLogic.ro_TXStateCaptured)
                {
                    // restore their balance if it was captured!
                    DB.ExecuteSQL(String.Format("update customer set MicroPayBalance={0} where CustomerID={1}", Localization.CurrencyStringForDBWithoutExchangeRate(mpBalance + OrderTotal), CustomerID.ToString()));
                    rawResponseString = String.Format("MicroPay Balance {0} => {1}", Localization.CurrencyStringForDBWithoutExchangeRate(mpBalance), Localization.CurrencyStringForDBWithoutExchangeRate(OrderTotal + mpBalance));
                }
                else
                {
                    rawResponseString = "MICROPAY GATEWAY SAID NO VOID ACTION NEEDED (WAS NOT IN CAPTURED STATE)";
                }
                DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OrderNumber.ToString());
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
            String TransactionState = String.Empty;
            int CustomerID = 0;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF,OrderTotal,TransactionState,CustomerID from orders   with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                        TransactionState = DB.RSField(rs, "TransactionState");
                        CustomerID = DB.RSFieldInt(rs, "CustomerID");
                    }                    
                }
            }
            
            Decimal mpBalance = AppLogic.GetMicroPayBalance(CustomerID);

            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("x_type=CREDIT");
            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_trans_id=" + TransID);
            if (RefundAmount == System.Decimal.Zero)
            {
                transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            }
            else
            {
                transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundAmount));
            }
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());

            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            if (TransID.Length == 0)
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                String rawResponseString = String.Empty;
                if (TransactionState == AppLogic.ro_TXStateCaptured)
                {
                    // restore their balance if it was captured!
                    DB.ExecuteSQL(String.Format("update customer set MicroPayBalance={0} where CustomerID={1}", Localization.CurrencyStringForDBWithoutExchangeRate(mpBalance + CommonLogic.IIF(RefundAmount == System.Decimal.Zero, OrderTotal, RefundAmount)), CustomerID.ToString()));
                    rawResponseString = String.Format("MicroPay Balance {0} => {1}", Localization.CurrencyStringForDBWithoutExchangeRate(mpBalance), Localization.CurrencyStringForDBWithoutExchangeRate(OrderTotal + mpBalance));
                }
                else
                {
                    rawResponseString = "MICROPAY GATEWAY SAID NO REFUND ACTION NEEDED (WAS NOT IN CAPTURED STATE)";
                }
                DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            }
            return result;
        }
    }
}
