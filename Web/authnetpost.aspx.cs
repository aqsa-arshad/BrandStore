// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text;
using System.IO;
using System.Data;
using System.Net;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for authnetpost.
    /// </summary>
    public partial class authnetpost : System.Web.UI.Page
    {

        private void Page_Load(object sender, System.EventArgs e)
        {

            // This file is only implemented for Authorize.net Automated Recurring Billing
            if (CommonLogic.FormCanBeDangerousContent("x_subscription_id").Length != 0)
            {
                String TxSubID = CommonLogic.FormCanBeDangerousContent("x_subscription_id");
                String TxStatus = CommonLogic.FormCanBeDangerousContent("x_response_code");
                String TxMsg = CommonLogic.FormCanBeDangerousContent("x_response_reason_text");
                String TxID = CommonLogic.FormCanBeDangerousContent("x_trans_id");
                String TxAmount = CommonLogic.FormCanBeDangerousContent("x_amount");
                DateTime dtTx = System.DateTime.Now;

                String tmpStatus = String.Empty;

                int OrigOrdNumber = AppLogic.GetOriginalRecurringOrderNumberFromSubscriptionID(TxSubID);

                if (OrigOrdNumber == 0)
                {
                    tmpStatus = "Silent Post: No Original Order Found";
                    if (TxID.Length != 0)
                    {
                        tmpStatus += ", PNREF=" + TxID;
                    }
                    DB.ExecuteSQL("insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult,CustomerEMailed,RecurringSubscriptionID) values(" +
                        "0,0,'0.0.0.0'," + DB.DateQuote(dtTx) + "," + DB.SQuote("AUTHORIZENET") + "," +
                        DB.SQuote(AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()) + "," + DB.SQuote(AppLogic.ro_NotApplicable) + "," + DB.SQuote(tmpStatus) + ",0," + DB.SQuote(TxSubID) + ")");
                }
                else
                {
                    if (TxStatus == "1") // Approved
                    {
                        int NewOrderNumber = 0;
                        RecurringOrderMgr rmgr = new RecurringOrderMgr(AppLogic.MakeEntityHelpers(), null);
                        tmpStatus = rmgr.ProcessAutoBillApproved(OrigOrdNumber, TxID, dtTx, out NewOrderNumber);
                    }
                    else
                    {
                        RecurringOrderMgr rmgr = new RecurringOrderMgr(AppLogic.MakeEntityHelpers(), null);
                        tmpStatus = rmgr.ProcessAutoBillDeclined(OrigOrdNumber, TxID, dtTx, TxSubID, TxMsg);
                    }

                    if (tmpStatus != AppLogic.ro_OK)
                    {
                        int ProcessCustomerID = Order.GetOrderCustomerID(OrigOrdNumber);
                        Customer ProcessCustomer = new Customer(ProcessCustomerID, true);

                        if (TxID.Length != 0)
                        {
                            tmpStatus += ", PNREF=" + TxID;
                        }
                        DB.ExecuteSQL("insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult,CustomerEMailed,RecurringSubscriptionID) values(" +
                            ProcessCustomer.CustomerID.ToString() + "," + OrigOrdNumber.ToString() + "," +
                            DB.SQuote("0.0.0.0") + "," + DB.DateQuote(dtTx) + "," + DB.SQuote("AUTHORIZENET") + "," +
                            DB.SQuote(AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()) + "," + DB.SQuote(AppLogic.ro_NotApplicable) + "," + DB.SQuote(tmpStatus) + ",0," + DB.SQuote(TxSubID) + ")");
                    }
                }
            }
            Response.Write("OK");
        }
    }
}
