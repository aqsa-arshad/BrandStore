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
    public static class MicropayController
    {
        public static String ProcessTransaction(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, String TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = AppLogic.ro_OK;

            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("x_type=" + CommonLogic.IIF(TransactionMode == AppLogic.ro_TXModeAuthOnly, "AUTH_ONLY", "AUTH_CAPTURE"));

            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_description=" + HttpContext.Current.Server.UrlEncode(AppLogic.AppConfig("StoreName") + " Order " + OrderNumber.ToString()));

            transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&x_card_num=" + UseBillingAddress.CardNumber);
            if (CardExtraCode.Length != 0)
            {
                transactionCommand.Append("&x_card_code=" + CardExtraCode.Trim());
            }

            transactionCommand.Append("&x_exp_date=" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + "/" + UseBillingAddress.CardExpirationYear);
            transactionCommand.Append("&x_phone=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Phone));
            transactionCommand.Append("&x_fax=");
            transactionCommand.Append("&x_customer_tax_id=");
            transactionCommand.Append("&x_cust_id=" + CustomerID.ToString());
            transactionCommand.Append("&x_invoice_num=" + OrderNumber.ToString());
            transactionCommand.Append("&x_email=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.EMail));
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());

            transactionCommand.Append("&x_first_name=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.FirstName));
            transactionCommand.Append("&x_last_name=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.LastName));
            transactionCommand.Append("&x_company=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Company));
            transactionCommand.Append("&x_address=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Address1));
            transactionCommand.Append("&x_city=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.City));
            transactionCommand.Append("&x_state=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.State));
            transactionCommand.Append("&x_zip=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Zip));
            transactionCommand.Append("&x_country=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Country));

            if (UseShippingAddress != null)
            {
                transactionCommand.Append("&x_ship_to_first_name=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.FirstName));
                transactionCommand.Append("&x_ship_to_last_name=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.LastName));
                transactionCommand.Append("&x_ship_to_company=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Company));
                transactionCommand.Append("&x_ship_to_address=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Address1));
                transactionCommand.Append("&x_ship_to_city=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.City));
                transactionCommand.Append("&x_ship_to_state=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.State));
                transactionCommand.Append("&x_ship_to_zip=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Zip));
                transactionCommand.Append("&x_ship_to_country=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Country));
            }

            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());

            if (CAVV.Length != 0 || ECI.Length != 0)
            {
                transactionCommand.Append("&x_authentication_indicator=" + ECI);
                transactionCommand.Append("&x_cardholder_authentication_value=" + CAVV);
            }

            //First do they have enough money?
            decimal mpBalance = AppLogic.GetMicroPayBalance(CustomerID);
            if (OrderTotal > mpBalance)
            {
                result = "INSUFFICIENT FUNDS";
                AuthorizationCode = "-1";
                AuthorizationResult = result;
                AuthorizationTransID = CommonLogic.GetNewGUID();
                AVSResult = String.Empty;
                TransactionCommandOut = transactionCommand.ToString();
                TransactionResponse = String.Empty;
            }
            else
            {
                // if we are capturing 
                if (AppLogic.TransactionMode() == AppLogic.ro_TXModeAuthCapture)
                {
                    DB.ExecuteSQL(String.Format("update customer set MicroPayBalance={0} where CustomerID={1}", Localization.CurrencyStringForDBWithoutExchangeRate(mpBalance - OrderTotal), CustomerID.ToString()));
                }
                AuthorizationCode = "0";
                AuthorizationResult = AppLogic.ro_OK;
                AuthorizationTransID = CommonLogic.GetNewGUID();
                AVSResult = AppLogic.ro_OK;
                TransactionCommandOut = transactionCommand.ToString();
                TransactionResponse = String.Empty;
            }

            return result;
        }
    }
}
