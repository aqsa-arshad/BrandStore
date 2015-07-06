// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for recurringimport.
	/// </summary>
    public partial class recurringimport : AdminPageBase 
	{
        String m_GW;
        DateTime dtLastRun = System.DateTime.MinValue;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            dtLastRun = Localization.ParseDBDateTime(AppLogic.AppConfig("Recurring.GatewayLastImportedDate"));
            if (dtLastRun > System.DateTime.MinValue)
            {
                lblLastRun.Text = "Last import was from " + Localization.ToThreadCultureShortDateString(dtLastRun) + "&nbsp;&nbsp;";
            }

            m_GW = AppLogic.ActivePaymentGatewayCleaned();
            btnGetGatewayStatus.Text = String.Format(AppLogic.GetString("GetAutoBillStatusFile", SkinID, LocaleSetting), CommonLogic.IIF(dtLastRun > System.DateTime.MinValue, AppLogic.GetString("admin.recurringimport.Next", SkinID, LocaleSetting), AppLogic.GetString("admin.recurringimport.Todays", SkinID, LocaleSetting)), m_GW);
            if (!IsPostBack)
            {
                if (dtLastRun.AddDays((double)1.0) >= DateTime.Today)
                {
                    txtInputFile.Text = AppLogic.GetString("admin.recurringimport.NothingToProcess", SkinID, LocaleSetting);
                    btnGetGatewayStatus.Enabled = false;
                }

                if (!AppLogic.ThereAreRecurringGatewayAutoBillOrders())
                {
                    pnlMain.Visible = false;
                    pnlNotSupported.Visible = true;
                }
                else
                {
                    GatewayProcessor GWActual = GatewayLoader.GetProcessor(m_GW);
                    if (GWActual != null && GWActual.RecurringSupportType() == RecurringSupportType.Normal)
                    {
                        btnGetGatewayStatus.Visible = true;
                        pnlMain.Visible = true;
                        pnlNotSupported.Visible = false;
                    }
                    else if (GWActual != null && GWActual.RecurringSupportType() == RecurringSupportType.Extended)
                    {
                        btnGetGatewayStatus.Visible = false;
                        btnProcessFile.Visible = true;
                        pnlMain.Visible = true;
                        pnlNotSupported.Visible = false;
                        PastePromptLabel.Text = PastePromptLabel.Text + "<br />" + AppLogic.GetString("admin.recurringimport.RawTextContents", SkinID, LocaleSetting);
                    }
                    else
                    {
                        pnlMain.Visible = false;
                        pnlNotSupported.Visible = true;
                    }
                }
            }
            else
            {
            }
        }

        protected void btnGetGatewayStatus_Click(object sender, EventArgs e)
        {
            txtResults.Text = "";
            btnGetGatewayStatus.Enabled = false;
            RecurringOrderMgr rmgr = new RecurringOrderMgr(AppLogic.MakeEntityHelpers(), null);
            btnProcessFile.Visible = true;
            btnProcessFile.Enabled = true;
            String sResults = String.Empty;
            String Status = rmgr.GetAutoBillStatusFile(m_GW, out sResults);
            if (Status == AppLogic.ro_OK)
            {
                txtInputFile.Text = sResults;
            }
            else
            {
                txtInputFile.Text = Status;
            }
        }

        protected void btnProcessFile_Click(object sender, EventArgs e)
        {
            txtResults.Visible = true;

            if (m_GW == Gateway.ro_GWVERISIGN || m_GW == Gateway.ro_GWPAYFLOWPRO)
            {
                btnProcessFile.Enabled = false;
            }

            dtLastRun = Localization.ParseDBDateTime(AppLogic.AppConfig("Recurring.GatewayLastImportedDate"));
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
                    dtRun = DateTime.Today.AddDays((double)-1); // Flag for yesterday
                } 
            }

            if ( dtRun >= DateTime.Today &&
                (m_GW == Gateway.ro_GWVERISIGN || m_GW == Gateway.ro_GWPAYFLOWPRO) )
            {
                txtInputFile.Text = AppLogic.GetString("admin.recurringimport.NothingToProcess", SkinID, LocaleSetting);
                btnGetGatewayStatus.Enabled = false;
                return;
            }


            if (txtInputFile.Text.Length == 0)
            {
                txtResults.Text = AppLogic.GetString("admin.recurringimport.NothingToProcessForget", SkinID, LocaleSetting);
            }
            else
            {
                RecurringOrderMgr rmgr = new RecurringOrderMgr(AppLogic.MakeEntityHelpers(), null);
                String sResults = String.Empty;
                String Status = rmgr.ProcessAutoBillStatusFile(m_GW, txtInputFile.Text, out sResults);
                if (Status == AppLogic.ro_OK)
                {
                    txtResults.Text = sResults;
                }
                else
                {
                    txtResults.Text = Status;
                }
            }

            btnGetGatewayStatus.Enabled = true;
            AppLogic.SetAppConfig("Recurring.GatewayLastImportedDate", Localization.ToDBDateTimeString(dtRun));
            lblLastRun.Text = String.Format(AppLogic.GetString("admin.recurringimport.LastImport", SkinID, LocaleSetting),Localization.ToThreadCultureShortDateString(dtRun));
            dtLastRun = dtRun;
        }
}
}
