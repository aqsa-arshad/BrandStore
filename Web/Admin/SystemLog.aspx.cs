// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using AspDotNetStorefrontCommon;
using AspDotNetStorefrontCore;
using Telerik.Web.UI;

namespace AspDotNetStorefrontAdmin
{
    public partial class SystemLog : AdminPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                btnClear.Attributes.Add("onclick", "return confirm('" + AppLogic.GetString("admin.systemlog.aspx.14", AppLogic.GetStoreSkinID(AppLogic.StoreID()), Localization.GetDefaultLocale()) + "');");

                ddlSeverity.Items.Add(new ListItem("All"));
                foreach (string s in Enum.GetNames(typeof(MessageSeverityEnum)))
                {
                    ddlSeverity.Items.Add(new ListItem(s));
                }

                ddlType.Items.Add(new ListItem("All"));
                foreach (string s in Enum.GetNames(typeof(MessageTypeEnum)))
                {
                    ddlType.Items.Add(new ListItem(s));
                }

                CultureInfo ci = new CultureInfo(Localization.GetDefaultLocale());

                dpStartDate.Culture = ci;
                dpEndDate.Culture = ci;

                dpStartDate.SelectedDate = System.DateTime.Now.AddDays(AppLogic.AppConfigNativeInt("System.MaxLogDays") * -1);
                dpEndDate.SelectedDate = System.DateTime.Now;
                
            }
            
            SqlDataSource1.ConnectionString = DB.GetDBConn();
            SqlDataSource1.SelectCommand = "SELECT [SysLogID], [Message], [Type], [Severity], [CreatedOn] FROM [aspdnsf_SysLog] ORDER BY [CreatedOn] DESC";

            SqlDataSource2.ConnectionString = DB.GetDBConn();
            SqlDataSource2.SelectCommand = "SELECT [SysLogID], [Details] FROM [aspdnsf_SysLog] WHERE [SysLogID] = @SysLogID";

        }

        protected void btnClear_OnClick(Object sender, EventArgs e)
        {
            SysLog.Clear();
            Response.Redirect(AppLogic.AdminLinkUrl("default.aspx"));
        }

        protected void btnExport_OnClick(Object sender, EventArgs e)
        {
            SqlParameter[] spa = {DB.CreateSQLParameter("@Severity", SqlDbType.NVarChar, 100, ddlSeverity.SelectedItem.Text, ParameterDirection.Input),
                                 DB.CreateSQLParameter("@Type", SqlDbType.NVarChar, 100, ddlType.SelectedItem.Text, ParameterDirection.Input),
                                 DB.CreateSQLParameter("@StartDate", SqlDbType.DateTime, 100, dpStartDate.SelectedDate, ParameterDirection.Input),
                                 DB.CreateSQLParameter("@EndDate", SqlDbType.DateTime, 100, dpEndDate.SelectedDate, ParameterDirection.Input)};

            StringBuilder logFile = new StringBuilder();

            logFile.Append("----------------------------------------------------------------------\r\n");
            logFile.Append("----------------------------------------------------------------------\r\n");
            logFile.Append("SYSTEM INFORMATION \r\n");
            logFile.Append("----------------------------------------------------------------------\r\n");
            logFile.Append("----------------------------------------------------------------------\r\n");

            logFile.Append("\r\n");
            logFile.Append("Version:            " + CommonLogic.GetVersion() + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("On Live Server:     " + AppLogic.OnLiveServer().ToString() + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("UseSSL:             " + AppLogic.UseSSL().ToString() + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Is Secure Page:     " + CommonLogic.IsSecureConnection().ToString() + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Changed Admin Dir:  " + CommonLogic.IIF(AppLogic.AppConfig("AdminDir").ToLowerInvariant() == "admin", "No", "Yes") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Caching:            " + (AppLogic.AppConfigBool("CacheMenus") ? AppLogic.GetString("admin.common.OnUC", AppLogic.GetStoreSkinID(AppLogic.StoreID()), Localization.GetDefaultLocale()) : AppLogic.GetString("admin.common.OffUC", AppLogic.AppConfigUSInt("DefaultSkinID"), Localization.GetDefaultLocale())) + "\r\n");
            logFile.Append("\r\n");
            logFile.Append("----------------------LICENSE INFO----------------------\r\n");
            logFile.Append("\r\n");
            logFile.Append("License Status:     " + AspDotNetStorefront.Global.LicenseInfo("status") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Current Page:       " + CommonLogic.GetThisPageName(true) + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("License ID:         " + AspDotNetStorefront.Global.LicenseInfo("id") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("License Created:    " + AspDotNetStorefront.Global.LicenseInfo("createdon") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Licensed To:        " + AspDotNetStorefront.Global.LicenseInfo("licensename") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("License Version:    " + AspDotNetStorefront.Global.LicenseInfo("licenseversion") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("License Domain:     " + AspDotNetStorefront.Global.LicenseInfo("licensedomains") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Assembly Name:      " + AspDotNetStorefront.Global.LicenseInfo("assemblyname") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Assembly Versions:  " + AspDotNetStorefront.Global.LicenseInfo("assemblyversion") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Assemnbly Domain:   " + AspDotNetStorefront.Global.LicenseInfo("assemblydomain") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Product Level:      " + AspDotNetStorefront.Global.LicenseInfo("productlevel") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Is ML:              " + AspDotNetStorefront.Global.LicenseInfo("productisml") + "\r\n");
            logFile.Append("\r\n");
            logFile.Append("----------------------LOCALIZATION----------------------\r\n");
            logFile.Append("\r\n");
            logFile.Append("Web Config Locale:  " + Localization.GetDefaultLocale() + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("SQL Server Locale:  " + Localization.GetSqlServerLocale() + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Store Currency:     " + AppLogic.AppConfig("Localization.StoreCurrency") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Store Currency Code:" + AppLogic.AppConfig("Localization.StoreCurrencyNumericCode") + "\r\n");
            logFile.Append("\r\n");
            logFile.Append("----------------------GATEWAY INFO----------------------\r\n");
            logFile.Append("\r\n");
            logFile.Append("Payment Gateway:    " + AppLogic.ActivePaymentGatewayRAW() + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Use Live Transactions:" + (AppLogic.AppConfigBool("UseLiveTransactions") ? AppLogic.GetString("admin.splash.aspx.20", AppLogic.GetStoreSkinID(AppLogic.StoreID()), Localization.GetDefaultLocale()) : AppLogic.GetString("admin.splash.aspx.21", AppLogic.AppConfigUSInt("DefaultSkinID"), Localization.GetDefaultLocale())) + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Transaction Mode:   " + AppLogic.AppConfig("TransactionMode").ToString() + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Payment Methods:    " + AppLogic.AppConfig("PaymentMethods").ToString() + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Primary Currency:   " + Localization.GetPrimaryCurrency() + "\r\n");
            logFile.Append("\r\n");
            logFile.Append("----------------------SHIPPING INFO----------------------\r\n");
            logFile.Append("\r\n");
            logFile.Append("Shipping Rates Table:" + DB.GetSqlS("select Name as S from dbo.ShippingCalculation where Selected=1") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Origin State:       " + AppLogic.AppConfig("RTShipping.OriginState") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Origin Zip:         " + AppLogic.AppConfig("RTShipping.OriginZip") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Origin Country:     " + AppLogic.AppConfig("RTShipping.OriginCountry") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Free Shipping Threshold: " + AppLogic.AppConfigNativeDecimal("FreeShippingThreshold").ToString() + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Free Shipping Method:" + AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn") + "\r\n");
            logFile.Append("----------------------\r\n");
            logFile.Append("Allow Rate Selection:" + CommonLogic.IIF(AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"), "On", "Off") + "\r\n");
            logFile.Append("\r\n\r\n");

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();

                using (SqlDataReader rs = DB.ExecuteStoredProcReader("aspdnsf_getSysLog", spa, conn))
                {

                    while (rs.Read())
                    {
                        logFile.Append("----------------------------------------------------------------------\r\n");
                        logFile.Append("----------------------------------------------------------------------\r\n");
                        logFile.Append(AppLogic.GetString("admin.systemlog.aspx.1", AppLogic.GetStoreSkinID(AppLogic.StoreID()), Localization.GetDefaultLocale()) + ": ");
                        logFile.Append(DB.RSFieldInt(rs, "SysLogID").ToString() + "  " + DB.RSFieldDateTime(rs,"CreatedOn").ToString() + "  \r\n");
                        logFile.Append("----------------------------------------------------------------------\r\n");
                        logFile.Append("----------------------------------------------------------------------\r\n");
                        logFile.Append("\r\n\r\n");
                        logFile.Append(DB.RSField(rs, "Message"));
                        logFile.Append("\r\n\r\n");
                        logFile.Append(DB.RSField(rs, "Details"));
                        logFile.Append("\r\n\r\n");
                    }

                    Response.Clear();
                    Response.ContentType = "application/octet-stream";
                    Response.AddHeader("Content-Disposition", "attachment; filename=SystemLog.txt");
                    Response.Write(logFile.ToString());

                    Response.Flush();
                    Response.Close();
                }
            }
        }

        protected void RadGrid1_ItemDataBound(Object sender, GridItemEventArgs e)
        {
            //Replace C# line breaks with HTML friendly breaks
            if (e.Item is GridDataItem && (e.Item.OwnerTableView.Name == "DetailTable"))
            {
                GridDataItem gi = (GridDataItem)e.Item;
                gi["Details"].Text = gi["Details"].Text.Replace("\n", "<br/>");
            }

            //Switch the background color based on the type of message
            if (e.Item is GridDataItem && (e.Item.OwnerTableView.Name == "MasterTable"))
            {
                GridDataItem gi = (GridDataItem)e.Item;
                
                if(gi["Severity"].Text == "Error")
                {
                    e.Item.BackColor = System.Drawing.Color.Pink;
                }
                else if (gi["Severity"].Text == "Message")
                {
                    e.Item.BackColor = System.Drawing.Color.LightGreen;
                }
                else if (gi["Severity"].Text == "Alert")
                {
                    e.Item.BackColor = System.Drawing.Color.LightYellow;
                }
                
            }

        }

    }
}
