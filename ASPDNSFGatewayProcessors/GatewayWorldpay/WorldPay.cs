// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;
using System.Text;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for Worldpay.
    /// </summary>
    public class Worldpay : GatewayProcessor
    {
        public Worldpay() { }

        public override String CaptureOrder(Order o)
        {
            String result = "CAPTURE METHOD NOT SUPPORTED FOR WORLDPAY";
            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = "VOID METHOD NOT SUPPORTED FOR WORLDPAY";
            return result;
        }

        // if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            String result = "REFUND METHOD NOT SUPPORTED FOR WORLDPAY";
            return result;
        }


        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = AppLogic.ro_OK;

            Customer ThisCustomer = new Customer(CustomerID, true);

            String rawResponseString =
                "TransID=" + ThisCustomer.ThisCustomerSession["WorldPay_TransID"] +
                "&FuturePayID=" + ThisCustomer.ThisCustomerSession["WorldPay_FuturePayID"] +
                "&TransStatus=" + ThisCustomer.ThisCustomerSession["WorldPay_TransStatus"] +
                "&TransTime=" + ThisCustomer.ThisCustomerSession["WorldPay_TransTime"] +
                "&AuthAmount=" + ThisCustomer.ThisCustomerSession["WorldPay_AuthAmount"] +
                "&AuthCurrency=" + ThisCustomer.ThisCustomerSession["WorldPay_AuthCurrency"] +
                "&RawAuthMessage=" + ThisCustomer.ThisCustomerSession["WorldPay_RawAuthMessage"] +
                "&RawAuthCode=" + ThisCustomer.ThisCustomerSession["WorldPay_RawAuthCode"] +
                "&CallbackPW=" + ThisCustomer.ThisCustomerSession["WorldPay_CallbackPW"] +
                "&CardType=" + ThisCustomer.ThisCustomerSession["WorldPay_CardType"] +
                "&CountryMatch=" + ThisCustomer.ThisCustomerSession["WorldPay_CountryMatch"] +
                "&AVS=" + ThisCustomer.ThisCustomerSession["WorldPay_AVS"];

            String sql = String.Empty;
            String replyCode = ThisCustomer.ThisCustomerSession["WorldPay_TransStatus"];
            String responseCode = ThisCustomer.ThisCustomerSession["WorldPay_RawAuthCode"];
            String approvalCode = ThisCustomer.ThisCustomerSession["WorldPay_RawAuthCode"];
            String authResponse = ThisCustomer.ThisCustomerSession["WorldPay_RawAuthMessage"];
            String TransID = ThisCustomer.ThisCustomerSession["WorldPay_TransID"];

            AuthorizationCode = approvalCode;
            AuthorizationResult = rawResponseString;
            AuthorizationTransID = TransID;
            AVSResult = ThisCustomer.ThisCustomerSession["WorldPay_AVS"];
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;

            bool AVSOK = true;


            if (replyCode == "Y" && AVSOK)
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
                result = result.Replace("account", "card");
                result = result.Replace("Account", "Card");
                result = result.Replace("ACCOUNT", "CARD");
            }
            return result;
        }

        public override bool SupportsAdHocOrders()
        {
            return false;
        }

        public override bool SupportsPostProcessingEdits()
        {
            return false;
        }

        public override bool RequiresFinalization()
        {
            return true;
        }

        public override string ProcessingPageRedirect()
        {
            return "~/worldpaypane.aspx";
        }

        public override string CreditCardPaneInfo(int SkinId, Customer ThisCustomer)
        {
            return AppLogic.GetString("checkoutworldpay.aspx.3", SkinId, ThisCustomer.LocaleSetting);
        }

        public override string DisplayName(string LocaleSetting)
        {
            return "WorldPay Business Gateway";
        }
    }
}
