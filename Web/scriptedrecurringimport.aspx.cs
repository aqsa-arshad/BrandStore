// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefront
{
	/// <summary>
    /// Summary description for scriptedrecurringimport.
	/// </summary>
    public partial class scriptedrecurringimport : System.Web.UI.Page
    {
        DateTime dtLastRun = System.DateTime.MinValue;
        String m_GW = AppLogic.ActivePaymentGatewayCleaned();

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (!IsPostBack)
            {
                String result = AppLogic.ro_OK;

                if (!AppLogic.ThereAreRecurringGatewayAutoBillOrders())
                {
                    result = "There are no Auto-Ship recurring orders active in your store.";
                }
                else
                {
                    dtLastRun = Localization.ParseDBDateTime(AppLogic.AppConfig("Recurring.GatewayLastImportedDate"));
                    if (dtLastRun.AddDays((double)1.0) >= DateTime.Today)
                    {
                        result = "Nothing to process... You are already up to date.";
                    }
                    else
                    {
                        if (m_GW == Gateway.ro_GWVERISIGN || m_GW == Gateway.ro_GWPAYFLOWPRO)
                        {
                            // we can run it
                        }
                        else
                        {
                            result = "Gateway " + m_GW + " not supported.";
                        }

                        if (result == AppLogic.ro_OK)
                        {
                            String StatusXML = GetGatewayStatus();
                            result = ProcessData(StatusXML);
                        }
                    }
                }

                litResults.Text = result.Replace("\n","<br>");
                AppLogic.SendMail(AppLogic.AppConfig("StoreName") + " - Scripted Recurring Payment Import", result, false);
            }
        }

        protected String GetGatewayStatus()
        {
            String sResults = String.Empty;

            RecurringOrderMgr rmgr = new RecurringOrderMgr(AppLogic.MakeEntityHelpers(), null);
            String Status = rmgr.GetAutoBillStatusFile(m_GW, out sResults);
            if (Status == AppLogic.ro_OK)
            {
                return sResults;
            }
            else
            {
                return String.Empty;
            }
        }

        protected String ProcessData(String StatusXML)
        {
            String result = String.Empty;

            DateTime dtRun = dtLastRun;
            if (dtRun == System.DateTime.MinValue)
            {
                dtRun = DateTime.Today.AddDays((double)-1); // Defaults to yesterday
            }
            else
            {
                if (m_GW == Gateway.ro_GWVERISIGN || m_GW == Gateway.ro_GWPAYFLOWPRO)
                {
                    dtRun = DateTime.Today.AddDays((double)-1); // Always runs through yesterday
                }
                else
                {
                    dtRun = dtLastRun.AddDays((double)1.0); // other gateways default to one day period
                }
            }

            if (StatusXML.Length == 0 || !StatusXML.Contains("<TX "))
            {
                result = "Nothing to process... No new data.";
            }
            else
            {
                RecurringOrderMgr rmgr = new RecurringOrderMgr(AppLogic.MakeEntityHelpers(), null);
                String sResults = String.Empty;
                String Status = rmgr.ProcessAutoBillStatusFile(m_GW, StatusXML, out sResults);
                if (Status == AppLogic.ro_OK)
                {
                    result = sResults;
                }
                else
                {
                    result = Status;
                }
            }

            AppLogic.SetAppConfig("Recurring.GatewayLastImportedDate", Localization.ToDBDateTimeString(dtRun));

            return result;
        }
    }
}
