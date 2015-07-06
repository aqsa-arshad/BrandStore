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
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for recurring.
    /// </summary>
    public partial class recurring : AdminPageBase
    {

        // NOTE: THIS PAGE DOES NOT PROCESS GATEWAY AUTOBILL RECURRING ORDERS!
        // NOTE: USE THE RECURRINGIMPORT.ASPX PAGE FOR THOSE!!
        
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            SectionTitle = "Recurring Shipments " + CommonLogic.IIF(!CommonLogic.QueryStringCanBeDangerousContent("Show").Equals("ALL", StringComparison.InvariantCultureIgnoreCase), AppLogic.GetString("admin.recurring.DueToday", SkinID, LocaleSetting), AppLogic.GetString("admin.recurring.AllPending", SkinID, LocaleSetting));

            RenderMarkup();
        }

        private void RenderMarkup()
        {
            StringBuilder output = new StringBuilder();

            if (CommonLogic.QueryStringBool("ProcessAll"))
            {
                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rsp = DB.GetRS("Select distinct(OriginalRecurringOrderNumber) from ShoppingCart where RecurringSubscriptionID='' and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + " and NextRecurringShipDate<" + DB.SQuote(Localization.ToDBShortDateString(System.DateTime.Now.AddDays(1))), conn))
                    {
                        RecurringOrderMgr rmgr = new RecurringOrderMgr(EntityHelpers, GetParser);
                        while (rsp.Read())
                        {
                            output.Append(String.Format(AppLogic.GetString("admin.recurring.ProcessingNextOccurrence", SkinID, LocaleSetting),DB.RSFieldInt(rsp, "OriginalRecurringOrderNumber").ToString()));
                            output.Append(rmgr.ProcessRecurringOrder(DB.RSFieldInt(rsp, "OriginalRecurringOrderNumber")));
                            output.Append("...<br/>");
                        }
                    }
                }
            }

            int OriginalRecurringOrderNumber = CommonLogic.QueryStringUSInt("OriginalRecurringOrderNumber");
            int ProcessCustomerID = CommonLogic.QueryStringUSInt("ProcessCustomerID");

            if (ProcessCustomerID != 0 && OriginalRecurringOrderNumber != 0)
            {
                output.Append(String.Format(AppLogic.GetString("admin.recurring.ProcessingNextOccurrence", SkinID, LocaleSetting),OriginalRecurringOrderNumber.ToString()));
                RecurringOrderMgr rmgr = new RecurringOrderMgr(EntityHelpers, GetParser);
                output.Append(rmgr.ProcessRecurringOrder(OriginalRecurringOrderNumber));
                output.Append("...<br/>");
            }

            output.Append("<br/><ul>");
            bool PendingOnly = (!CommonLogic.QueryStringCanBeDangerousContent("Show").Equals("ALL", StringComparison.InvariantCultureIgnoreCase));

            if (PendingOnly)
            {
                if (DB.GetSqlN("Select count(*) as N from ShoppingCart   with (NOLOCK)  where CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + " and NextRecurringShipDate<" + DB.SQuote(Localization.ToDBDateTimeString(System.DateTime.Now.AddDays(1)))) > 0)
                {
                    output.Append("<li><b><a href=\"" + AppLogic.AdminLinkUrl("recurring.aspx") + "?processall=true\">" + AppLogic.GetString("admin.recurring.ProcessChargesAll", SkinID, LocaleSetting) + "</a></b> " + AppLogic.GetString("admin.recurring.ProcessChargesSingle", SkinID, LocaleSetting) + "</li>");
                }
                else
                {
                    output.Append("<li><b>" + AppLogic.GetString("admin.recurring.NoRecurringShipmentsDueToday", SkinID, LocaleSetting) + "</b></li>");
                }
            }
            else
            {
                if (DB.GetSqlN("Select count(*) as N from ShoppingCart   with (NOLOCK)  where CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()) == 0)
                {
                    output.Append("<li><b>" + AppLogic.GetString("admin.recurring.NoActiveRecurringOrdersFound", SkinID, LocaleSetting) + "</b></li>");
                }
            }
            if (AppLogic.AppConfigBool("Recurring.UseGatewayInternalBilling"))
            {
                output.Append("<li><b>" + AppLogic.GetString("admin.recurring.NoteAutobillGatewayOrders", SkinID, LocaleSetting) + "</b></li>");
            }
            output.Append("</ul>");

            using (SqlConnection conn2 = DB.dbConn())
            {
                conn2.Open();
                using (IDataReader rsr = DB.GetRS("Select distinct OriginalRecurringOrderNumber, CustomerID from ShoppingCart  with (NOLOCK)  where CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + CommonLogic.IIF(PendingOnly, " and NextRecurringShipDate<" + DB.SQuote(Localization.ToDBShortDateString(System.DateTime.Now.AddDays(1))), "") + " order by OriginalRecurringOrderNumber desc", conn2))
                {
                    while (rsr.Read())
                    {
                        output.Append(AppLogic.GetRecurringCart(EntityHelpers, GetParser, new Customer(DB.RSFieldInt(rsr, "CustomerID")), DB.RSFieldInt(rsr, "OriginalRecurringOrderNumber"), SkinID, false));
                    }
                }
            }

            ltContent.Text = output.ToString();
        }

    }
}
