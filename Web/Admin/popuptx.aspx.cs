// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for popuptx.
    /// </summary>
    public partial class popuptx : System.Web.UI.Page
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            StringBuilder writer = new StringBuilder();
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            bool IsEcheck = false;
            bool IsMicroPay = false;
            bool IsCard = false;

            if (!ThisCustomer.IsAdminUser)
            {
                writer.Append("<b><font color=red>" + AppLogic.GetString("admin.common.PermissionDeniedUC", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</b></font>");
            }
            else
            {
                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("Select * from orders   with (NOLOCK)  where ordernumber=" + CommonLogic.QueryStringUSInt("OrderNumber").ToString(), conn))
                    {
                        if (rs.Read())
                        {
                            IsEcheck = (DB.RSField(rs, "PaymentMethod").Trim().Equals(AppLogic.ro_PMECheck, StringComparison.InvariantCultureIgnoreCase));
                            //V3_9
                            IsMicroPay = (AppLogic.CleanPaymentMethod(DB.RSField(rs, "PaymentMethod")) == AppLogic.ro_PMMicropay);
                            IsCard = (AppLogic.CleanPaymentMethod(DB.RSField(rs, "PaymentMethod")) == AppLogic.ro_PMCreditCard);
                            //V3_9
                            if (IsEcheck || IsMicroPay || IsCard)
                            {
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.OrderNumber", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),CommonLogic.QueryStringUSInt("OrderNumber").ToString()));
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.CustomerID", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),DB.RSFieldInt(rs, "CustomerID").ToString()));
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.OrderDate", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),DB.RSFieldDateTime(rs, "OrderDate").ToString()));
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.OrderTotal", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),ThisCustomer.CurrencyString(DB.RSFieldDecimal(rs, "OrderTotal"))));
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.CardType", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),DB.RSField(rs, "CardType")));
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.PaymentGateway", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),DB.RSField(rs, "PaymentGateway")));
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.TransactionState", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),DB.RSField(rs, "TransactionState")));

                                String _cardNumber = AppLogic.SafeDisplayCardNumber(DB.RSField(rs, "CardNumber"), "Orders", CommonLogic.QueryStringUSInt("OrderNumber"));
                                String _cardType = DB.RSField(rs, "CardType");

                                if (IsEcheck)
                                {
                                    writer.Append(String.Format(AppLogic.GetString("admin.popuptx.ECheckBank", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), DB.RSField(rs, "ECheckBankName")));
                                    writer.Append(String.Format(AppLogic.GetString("admin.popuptx.ECheckABA", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), DB.RSField(rs, "ECheckBankABACode")));
                                    writer.Append(String.Format(AppLogic.GetString("admin.popuptx.Account", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), DB.RSField(rs, "ECheckBankAccountNumber")));
                                    writer.Append(String.Format(AppLogic.GetString("admin.popuptx.AccountName", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), DB.RSField(rs, "ECheckBankAccountName")));
                                    writer.Append(String.Format(AppLogic.GetString("admin.popuptx.AccountType", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), DB.RSField(rs, "ECheckBankAccountType")));
                                }
                                //V3_9
                                if (IsMicroPay)
                                {
                                    writer.Append("<b>" + AppLogic.GetString("admin.popuptx.Micropay", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + ":</b>");
                                }
                                //V3_9
                                else
                                {
                                    if (_cardType.StartsWith(AppLogic.ro_PMPayPal, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        writer.Append("<b>" + AppLogic.GetString("admin.orderframe.CardNumber", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " </b>");
                                    }
                                    else
                                    {
                                        writer.Append(String.Format(AppLogic.GetString("admin.popuptx.CardNumber", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),_cardNumber));
                                    }
                                    if (_cardNumber.Length == 0 || _cardNumber == AppLogic.ro_CCNotStoredString)
                                    {
                                        writer.Append(AppLogic.GetString("admin.popuptx.CardExpirationNotAvailable", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                                    }
                                    else
                                    {
                                        if (_cardType.StartsWith(AppLogic.ro_PMPayPal, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            writer.Append(AppLogic.GetString("admin.popuptx.CardExpirationNotAvailable", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                                        }
                                        else
                                        {
                                            writer.Append(String.Format(AppLogic.GetString("admin.popuptx.CardExpiration", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),DB.RSField(rs, "CardExpirationMonth"),DB.RSField(rs, "cardExpirationYear")));
                                        }
                                    }
                                }

                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.TransactionCommand", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Server.HtmlEncode(DB.RSField(rs, "TransactionCommand")).Replace(" ", "&nbsp;").Replace("\r\n", "<br/>")));
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.AuthorizationResult", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Server.HtmlEncode(DB.RSField(rs, "AuthorizationResult")).Replace(" ", "&nbsp;").Replace("\r\n", "<br/>")));
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.AuthorizationCode", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Server.HtmlEncode(DB.RSField(rs, "AuthorizationCode")) + "<br/>"));
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.TransactionID", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),DB.RSField(rs, "AuthorizationPNREF") + "<br/>"));
                                writer.Append("<hr size=\"1\"/>");
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.CaptureTXCommand", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Server.HtmlEncode(CommonLogic.IIF(DB.RSField(rs, "CaptureTXCommand").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs, "CaptureTXCommand"))).Replace(" ", "&nbsp;").Replace("\r\n", "<br/>")));
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.CaptureTXResult", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Server.HtmlEncode(CommonLogic.IIF(DB.RSField(rs, "CaptureTXResult").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs, "CaptureTXResult"))).Replace(" ", "&nbsp;").Replace("\r\n", "<br/>")));
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.VoidTXCommand", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Server.HtmlEncode(CommonLogic.IIF(DB.RSField(rs, "VoidTXCommand").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs, "VoidTXCommand"))).Replace(" ", "&nbsp;").Replace("\r\n", "<br/>")));
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.VoidTXResult", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Server.HtmlEncode(CommonLogic.IIF(DB.RSField(rs, "VoidTXResult").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs, "VoidTXResult"))).Replace(" ", "&nbsp;").Replace("\r\n", "<br/>")));
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.RefundTXCommand", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Server.HtmlEncode(CommonLogic.IIF(DB.RSField(rs, "RefundTXCommand").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs, "RefundTXCommand"))).Replace(" ", "&nbsp;").Replace("\r\n", "<br/>")));
                                writer.Append(String.Format(AppLogic.GetString("admin.popuptx.RefundTXResult", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Server.HtmlEncode(CommonLogic.IIF(DB.RSField(rs, "RefundTXResult").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs, "RefundTXResult"))).Replace(" ", "&nbsp;").Replace("\r\n", "<br/>")));

                                if (AppLogic.AppConfigBool("CardinalCommerce.Centinel.Enabled"))
                                {
                                    writer.Append(String.Format(AppLogic.GetString("admin.popuptx.CardinalLookupResult", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Server.HtmlEncode(CommonLogic.IIF(DB.RSField(rs, "CardinalLookupResult").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs, "CardinalLookupResult")))));
                                    writer.Append(String.Format(AppLogic.GetString("admin.popuptx.CardinalAuthenticateResult", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Server.HtmlEncode(CommonLogic.IIF(DB.RSField(rs, "CardinalAuthenticateResult").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs, "CardinalAuthenticateResult")))));
                                    writer.Append(String.Format(AppLogic.GetString("admin.popuptx.CardinalGatewayParams", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Server.HtmlEncode(CommonLogic.IIF(DB.RSField(rs, "CardinalGatewayParms").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs, "CardinalGatewayParms")))));
                                }
                            }
                            else
                            {
                                writer.Append(AppLogic.ro_NotApplicable);
                            }
                        }
                        else
                        {
                            writer.Append("<b><font color=red>" + AppLogic.GetString("admin.common.OrderNotFoundUC", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</b></font>");
                        }
                        rs.Close();
                    }
                }
            }

            writer.Append("<p align=\"center\"><a href=\"javascript:self.close();\">" + AppLogic.GetString("admin.common.Close", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a></p>");
            ltContent.Text = writer.ToString();
        }

    }
}
