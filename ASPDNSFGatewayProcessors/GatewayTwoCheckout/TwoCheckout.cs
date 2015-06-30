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
	/// Summary description for TwoCheckout.
	/// </summary>
	public class TwoCheckout : GatewayProcessor
	{
		public TwoCheckout() {}

		public override String CaptureOrder(Order o)
		{
			String result = "CAPTURE METHOD NOT SUPPORTED FOR 2CHECKOUT";
			return result;
		}

		public override String VoidOrder(int OrderNumber)
		{
			String result = "VOID METHOD NOT SUPPORTED FOR 2CHECKOUT";
			return result;
		}

		// if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
			String result = "REFUND METHOD NOT SUPPORTED FOR 2CHECKOUT";
			return result;
		}

        public override string ProcessCard(int OrderNumber, int CustomerID, decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, string CardExtraCode, Address UseShippingAddress, string CAVV, string ECI, string XID, out string AVSResult, out string AuthorizationResult, out string AuthorizationCode, out string AuthorizationTransID, out string TransactionCommandOut, out string TransactionResponse)
        {
			String result = AppLogic.ro_OK;
			AuthorizationCode = String.Empty;
			AuthorizationResult = String.Empty;
			AuthorizationTransID = String.Empty;
			AVSResult = String.Empty;
			TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;
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
            return "~/twocheckoutpane.aspx";
        }

        public override string CreditCardPaneInfo(int SkinId, Customer ThisCustomer)
        {
            return AppLogic.GetString("checkouttwocheckout.aspx.2", SkinId, ThisCustomer.LocaleSetting);
        }
	}
}
