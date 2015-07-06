// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{

    public partial class _default : AdminPageBase
    {
        private int m_SkinID = 1;

        public enum DateRanges
        {
            Today = 0,
            Yesterday = 1,
            ThisWeek = 2,
            LastWeek = 3,
            ThisMonth = 4,
            LastMonth = 5,
            ThisYear = 6,
            LastYear = 7,
            AllTime = 8
        }

        public enum CompareDateRanges
        {
            CompareYear = 0,
            CompareMonths = 1,
            CompareWeeks = 2
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (CommonLogic.QueryStringCanBeDangerousContent("flushcache").Length != 0 || CommonLogic.QueryStringCanBeDangerousContent("resetcache").Length != 0 || CommonLogic.QueryStringCanBeDangerousContent("clearcache").Length != 0)
            {
                foreach (DictionaryEntry dEntry in HttpContext.Current.Cache)
                {
                    HttpContext.Current.Cache.Remove(dEntry.Key.ToString());
                }
                AppLogic.m_RestartApp();
                Response.Redirect(AppLogic.AdminLinkUrl("default.aspx"));
            }

            resetError("", false);

            // perform security audit on this website, and present the merchant with a list of issues needing attention before the site goes live
            if (!AppLogic.AppConfigBool("SkipSecurityAudit") && ThisCustomer.IsAdminSuperUser)
            {
                try
                {
                    SecurityAudit();
                }
                catch { }
            }
            else
            {
                pnlSecurity.Visible = false;
                cpxSecurity.Enabled = false;
            }

            //Show low stock warning table
            if (AppLogic.AppConfigBool("ShowAdminLowStockAudit") && ThisCustomer.IsAdminSuperUser)
            {
                try
                {
                    LowStockAudit();
                }
                catch { }
            }
            else
            {
                pnlLowStock.Visible = false;
                cpxLowStock.Enabled = false;
            }


            CustomerSession.StaticClear();
            ltDateTime.Text = Localization.ToNativeDateTimeString(System.DateTime.Now);
            ltExecutionMode.Text = CommonLogic.IIF(IntPtr.Size == 8, "64 Bit", "32 Bit");

            loadcontrolsStringResource();

            if (!IsPostBack)
            {
                DateTime NextEncryptKeyChangeDate = System.DateTime.MinValue;
                try
                {
                    NextEncryptKeyChangeDate = System.DateTime.Parse(Security.GetEncryptParam("NextKeyChange"));
                }
                catch { }
                bool StoringCCInDB = AppLogic.StoreCCInDB();
                if (StoringCCInDB && NextEncryptKeyChangeDate < System.DateTime.Now)
                {
                    ChangeEncryptKeyReminder.Visible = true;
                }
                
                loadInformation();
                loadGrids();
                loadFeeds();
                //DisplayStatsOptions(rblStats.SelectedIndex);


                if (ThisCustomer.IsAdminSuperUser)
                {
                    //get the user preference from profile
                    if (Profile.StatsView.Length == 0)
                    {
                        Profile.StatsView = CommonLogic.IIF(AppLogic.AppConfigBool("Admin_OrderStatisticIsChart"), "Chart", "Tabular");
                        loadSummaryReport(CommonLogic.IIF(AppLogic.AppConfigBool("Admin_OrderStatisticIsChart"), "Chart", "Tabular"));
                    }
                    else
                    {
                        loadSummaryReport(Profile.StatsView.ToString());
                    }
                }
                else
                {
                    divStats.Visible = false;
                    StatsTable.Visible = false;
                }

                InitPatchInformation();
            }

            CheckPostbackMinimized();
        }

        private void InitPatchInformation()
        {
            //Load version xmlpackages
            ArrayList xmlPackages = LocalReadAdminXmlPackages("versioninfo");
            StringBuilder sb = new StringBuilder();
            bool VersionFileFound = false;
            foreach (String s in xmlPackages)
            {
                VersionFileFound = true;
                XmlPackage2 p = new XmlPackage2(s, null, SkinID);
                sb.Append(p.TransformString());
            }
            if (VersionFileFound)
            {
                litPatchInfo.Text = sb.ToString();
                pnlPatchInfo.Visible = true;
            }
        }

        static public ArrayList LocalReadAdminXmlPackages(String PackageFilePrefix) //This method is duplicated in SP1 Applogic as "ReadAdminXmlPackages". We moved it here to provide the same functionality in the Admin Service Pack
        {
            // create an array to hold the list of files
            ArrayList fArray = new ArrayList();

            // now check common area:
            String SFP = CommonLogic.SafeMapPath(CommonLogic.IIF(AppLogic.IsAdminSite, String.Empty, "admin/") + "XmlPackages/bogus.htm").Replace("bogus.htm", String.Empty);
            DirectoryInfo dirInfo = new DirectoryInfo(SFP);
            FileSystemInfo[] myDir = dirInfo.GetFiles(CommonLogic.IIF(PackageFilePrefix.Length != 0, PackageFilePrefix + ".*.xml.config", "*.xml.config"));
            for (int i = 0; i < myDir.Length; i++)
            {
                fArray.Add(myDir[i].ToString().ToLowerInvariant());
            }

            if (fArray.Count != 0)
            {
                // sort the files alphabetically
                fArray.Sort(0, fArray.Count, null);
            }
            return fArray;
        }

        private const string CLIENT_MINIMIZED = "0";

        private void CheckPostbackMinimized()
        {
            if (colLeft_Visible.Value == CLIENT_MINIMIZED)
            {
                colLeft.Style["display"] = "none";
                pnlShowLeftPanel.Style["display"] = "";
            }
            else
            {
                colLeft.Style["display"] = "";
                pnlShowLeftPanel.Style["display"] = "none";
            }

            if (divSecurityAudit_Content_Visible.Value == CLIENT_MINIMIZED)
            {
                imgMinimize_SecurityAudit.ImageUrl = "~/App_Themes/Admin_Default/images/icon_restore.jpg";
                divSecurityAudit_Content.Style["display"] = "none";
            }
            else
            {
                divSecurityAudit_Content.Style["display"] = "";
            }

            if (divStatistics_Content_Visible.Value == CLIENT_MINIMIZED)
            {
                imgMinimize_Statistics.ImageUrl = "~/App_Themes/Admin_Default/images/icon_restore.jpg";
                divStatistics_Content.Style["display"] = "none";
            }
            else
            {
                divStatistics_Content.Style["display"] = "";
            }

            if (divFeeds_Content_Visible.Value == CLIENT_MINIMIZED)
            {
                imgMinimize_Feeds.ImageUrl = "~/App_Themes/Admin_Default/images/icon_restore.jpg";
                divFeeds_Content.Style["display"] = "none";
            }
            else
            {
                divFeeds_Content.Style["display"] = "";
            }

            if (divLatestOrders_Content_Visible.Value == CLIENT_MINIMIZED)
            {
                imgMinimize_LatestOrders.ImageUrl = "~/App_Themes/Admin_Default/images/icon_restore.jpg";
                divLatestOrders_Content.Style["display"] = "none";
            }
            else
            {
                divLatestOrders_Content.Style["display"] = "";
            }

            if (divLatestRegisteredCustomers_Content_Visible.Value == CLIENT_MINIMIZED)
            {
                imgMinimize_LatestRegisteredCustomers.ImageUrl = "~/App_Templates/Admin_Default/images/icon_restore.jpg";
                divLatestRegisteredCustomers_Content.Style["display"] = "none";
            }
            else
            {
                divLatestRegisteredCustomers_Content.Style["display"] = "";
            }
        }

        /// <summary>
        /// Performs an inventory audit and displays a table of products with low stock
        /// </summary>
        protected void LowStockAudit()
        {
            int productCount = 0;

            string sql = "Select p.Name, pv.Name as VariantName, i.Size, i.Color, case when IsNull(Quan, 0) > 0 Then Quan Else Inventory End [Inventory] From Product p, ProductVariant pv Left Join Inventory i on pv.VariantId = i.VariantId Where p.Deleted=0 and pv.Deleted=0 and p.ProductId = pv.ProductId And case when IsNull(Quan, 0) > 0 Then Quan Else Inventory End < " + AppLogic.AppConfigNativeInt("SendLowStockWarningsThreshold");

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(sql, con))
                {
                    while(rs.Read())
                    {
                        WriteStockRow(DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale()),
                            DB.RSFieldByLocale(rs, "VariantName", Localization.GetDefaultLocale()),
                            DB.RSFieldByLocale(rs, "Size", Localization.GetDefaultLocale()),
                            DB.RSFieldByLocale(rs, "Color", Localization.GetDefaultLocale()), 
                            rs.GetInt32(4).ToString());
                        productCount++;
                    }
                }
            }

            if (productCount > 0)
            {
                pnlLowStock.Visible = true;
            }
            else
            {
                pnlLowStock.Visible = false;
                cpxLowStock.Enabled = false;
            }
        }

        protected void WriteStockRow(string name, string variantName, string size, string color, string quantity)
        {
            TableRow row = new TableRow();
            TableCell nameCell = new TableCell();
            TableCell variantNameCell = new TableCell();
            TableCell sizeCell = new TableCell();
            TableCell colorCell = new TableCell();
            TableCell quantityCell = new TableCell();

            Literal nameText = new Literal();
            Literal variantNameText = new Literal();
            Literal sizeText = new Literal();
            Literal colorText = new Literal();
            Literal quantityText = new Literal();

            nameText.Text = name;
            nameCell.Controls.Add(nameText);
            variantNameText.Text = variantName;
            variantNameCell.Controls.Add(variantNameText);
            sizeText.Text = size;
            sizeCell.Controls.Add(sizeText);
            colorText.Text = color;
            colorCell.Controls.Add(colorText);
            quantityText.Text = quantity;
            quantityCell.Controls.Add(quantityText);

            row.Controls.Add(nameCell);
            row.Controls.Add(variantNameCell);
            row.Controls.Add(sizeCell);
            row.Controls.Add(colorCell);
            row.Controls.Add(quantityCell);

            if (tblLowStock.Rows.Count % 2 == 0)
                row.CssClass = "gridRow";
            else
                row.CssClass = "gridAlternatingRow";

            tblLowStock.Controls.Add(row);
            
        }


        /// <summary>
        /// Performs a security audit and displays a table providing the admin guidance for resolving configuration errors
        /// </summary>
        protected void SecurityAudit()
        {
            int errorCount = 0;

            // 1. ensure that SSL is working on the admin site.  An issue with LiveServer can cause SSL not to function.
            if (!CommonLogic.IsSecureConnection())
            {
                WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.SSL", 1, ThisCustomer.LocaleSetting), true);
                errorCount++;
            }

            // 2. check for path element containing /admin/. We do not allow Admin sites to be located at the default /admin/ path. Too easy to guess.
            if (Request.Path.IndexOf("/admin/default.aspx", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.PathElement", 1, ThisCustomer.LocaleSetting), true);
                errorCount++;
            }

            // 3. remove or change admin@aspdotnetstorefront.com. Cannot use the default credentials long-term.
            Customer defaultAdmin = new Customer("admin@aspdotnetstorefront.com");
            if (defaultAdmin.EMail == "admin@aspdotnetstorefront.com")
            {
                WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.DefaultAdmin", 1, ThisCustomer.LocaleSetting), true);
                errorCount++;
            }
            defaultAdmin = null;

            // 4. check MailMe_Server AppConfig Setting. Cannot Allow blank MailMe_Server AppConfig.
            string sMailServer = AppLogic.AppConfig("MailMe_Server");
            if (sMailServer.Equals(AppLogic.ro_TBD, StringComparison.InvariantCultureIgnoreCase) ||
                sMailServer.Equals("MAIL.YOURDOMAIN.COM", StringComparison.InvariantCultureIgnoreCase) ||
                sMailServer.Length == 0)
            {
                WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.MailServer", 1, ThisCustomer.LocaleSetting), true);
                errorCount++;
            }

            // 5. check for version.aspx. Should be deleted.
            if (File.Exists(CommonLogic.SafeMapPath("~/version.aspx")))
            {
                WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.VersionAspx", 1, ThisCustomer.LocaleSetting), true);
                errorCount++;
            }

            // 6. check for admin\assetmanager folder. Should be deleted. 
            if (Directory.Exists(CommonLogic.SafeMapPath("assetmanager")))
            {
                WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.AssetManager", 1, ThisCustomer.LocaleSetting), true);
                errorCount++;
            }

            // 7. check for match between path and AdminDir. Verify that AdminDir is set correctly.
            if (Request.Path.IndexOf("/" + AppLogic.AppConfig("AdminDir") + "/default.aspx", StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.AdminDir", 1, ThisCustomer.LocaleSetting), true);
                errorCount++;
            }

            // 8. check web.config customErrors for Off. Should be RemoteOnly or On.
            if (AppLogic.TrustLevel == AspNetHostingPermissionLevel.Unrestricted || AppLogic.TrustLevel == AspNetHostingPermissionLevel.High)
            {
                CustomErrorsSection customErrors = (CustomErrorsSection)WebConfigurationManager.OpenWebConfiguration("~").GetSection("system.web/customErrors");
                if (customErrors.Mode == CustomErrorsMode.Off)
                {
                    WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.CustomErrors", 1, ThisCustomer.LocaleSetting), false);
                    errorCount++;
                }
                customErrors = null;
            }

            // 9. check for debug=true in web.config. Should be false on a live site.
            if (AppLogic.TrustLevel == AspNetHostingPermissionLevel.Unrestricted || AppLogic.TrustLevel == AspNetHostingPermissionLevel.High)
            {
                CompilationSection CompileSection = (CompilationSection)WebConfigurationManager.OpenWebConfiguration("~").GetSection("system.web/compilation");
                if (CompileSection.Debug == true)
                {
                    WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.Debug", 1, ThisCustomer.LocaleSetting), false);
                    errorCount++;
                }
            }

            // 10. check encryption on web.config. Must be encrypted as the last step before going Live.
            if ((AppLogic.TrustLevel == AspNetHostingPermissionLevel.Unrestricted || AppLogic.TrustLevel == AspNetHostingPermissionLevel.High)
                && !WebConfigurationManager.OpenWebConfiguration("~").GetSection("appSettings").SectionInformation.IsProtected)
            {
                WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.Encryption", 1, ThisCustomer.LocaleSetting), false);
                errorCount++;
            }

            // 11. check write permissions on web.config. Must have write-permission to encrypt, then have read-only permission after encryption.
            if ((AppLogic.TrustLevel == AspNetHostingPermissionLevel.Unrestricted || AppLogic.TrustLevel == AspNetHostingPermissionLevel.High)
                && FileIsWriteable(CommonLogic.SafeMapPath("~/web.config")))
            {
                WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.WebConfigWritable", 1, ThisCustomer.LocaleSetting), false);
                errorCount++;
            }

            // 12. check non-write permissions on root. Cannot allow root folder to have write permission. 
            if ((AppLogic.TrustLevel == AspNetHostingPermissionLevel.Unrestricted || AppLogic.TrustLevel == AspNetHostingPermissionLevel.High)
                && FolderIsWriteable(CommonLogic.SafeMapPath("~/")))
            {
                WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.RootWritable", 1, ThisCustomer.LocaleSetting), false);
                errorCount++;
            }

			bool payPalEnabled = AppLogic.AppConfig("PaymentMethods").ContainsIgnoreCase("PayPal");

			// 13. PayPal is not enabled. 
			if (!payPalEnabled)
			{
				WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.PaypalNotEnabled", 1, ThisCustomer.LocaleSetting), false);
				errorCount++;
			}

			// 14. PayPal is enabled, but you're not using Bill Me Later. 
			if (payPalEnabled && !AppLogic.AppConfigBool("PayPal.Express.ShowBillMeLaterButton"))
			{
				WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.BillMeLaterNotEnabled", 1, ThisCustomer.LocaleSetting), false);
				errorCount++;
			}

			// 15. Checkout By Amazon is not enabled.
			if (!AppLogic.AppConfig("PaymentMethods").ContainsIgnoreCase("CheckoutByAmazon"))
			{
				WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.CheckoutByAmazonNotEnabled", 1, ThisCustomer.LocaleSetting), false);
				errorCount++;
			}

			bool dotFeedInstalled = AppConfigManager.AppConfigExists("DotFeed.AccessKey");

			// 16. DotFeed is installed but not enabled. 
			if (dotFeedInstalled && AppLogic.AppConfig("DotFeed.AccessKey") == String.Empty)
			{
				WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.DotFeedNotEnabled", 1, ThisCustomer.LocaleSetting), false);
				errorCount++;
			}

			// 17. DotFeed is not installed.
			if (!dotFeedInstalled)
			{
				WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.DotFeedNotInstalled", 1, ThisCustomer.LocaleSetting), false);
				errorCount++;
			}
			
			// 18. Your site is using the default Search Engine Meta Title & Description tags.
			if (AppLogic.AppConfig("SE_MetaTitle").ContainsIgnoreCase("Enter your site title here")
				|| AppLogic.AppConfig("SE_MetaDescription").ContainsIgnoreCase("enter your site description here")
				|| AppLogic.AppConfig("SE_MetaKeywords").ContainsIgnoreCase("enter your site keywords here"))
			{
				WriteAuditRow(AppLogic.GetString("admin.splash.aspx.security.MetaTagsNotSet", 1, ThisCustomer.LocaleSetting), false);
				errorCount++;
			}

            if (errorCount > 0)
            {
                pnlSecurity.Visible = true;
            }
            else
            {
                pnlSecurity.Visible = false;
                cpxSecurity.Enabled = false;
            }
        }

        /// <summary>
        /// Adds a row to the tblSecurityAudit table control
        /// </summary>
        /// <param name="message">The error message to be displayed</param>
        /// <param name="iserror">Determines whether a warning icon or error icon is displayed</param>
        protected void WriteAuditRow(string message, bool iserror)
        {
            TableRow row = new TableRow();
            TableCell imageCell = new TableCell();
            TableCell textCell = new TableCell();
            Literal cellText = new Literal();

            System.Web.UI.WebControls.Image iconImage = new System.Web.UI.WebControls.Image();
            if (iserror)
            {
                iconImage.ImageUrl = "~/App_Themes/Admin_Default/images/icons/error.png";
            }
            else
            {
                iconImage.ImageUrl = "~/App_Themes/Admin_Default/images/icons/warning.png";
            }

            imageCell.Controls.Add(iconImage);
            cellText.Text = message;
            textCell.Controls.Add(cellText);
            textCell.CssClass = "splash_SecurityAudit";
            row.Controls.Add(imageCell);
            row.Controls.Add(textCell);
            tblSecurityAudit.Controls.Add(row);
        }

        protected static bool FileIsWriteable(string filename)
        {
            WindowsIdentity principal = WindowsIdentity.GetCurrent();
            if (File.Exists(filename))
            {
                FileInfo fi = new FileInfo(filename);
                if (fi.IsReadOnly)
                {
                    return false;
                }

                System.Security.AccessControl.AuthorizationRuleCollection acl = fi.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
                for (int i = 0; i < acl.Count; i++)
                {
                    System.Security.AccessControl.FileSystemAccessRule rule = (System.Security.AccessControl.FileSystemAccessRule)acl[i];
                    if (principal.User.Equals(rule.IdentityReference))
                    {
                        if (System.Security.AccessControl.AccessControlType.Deny.Equals(rule.AccessControlType))
                        {
                            if ((((int)System.Security.AccessControl.FileSystemRights.Write) & (int)rule.FileSystemRights) == (int)(System.Security.AccessControl.FileSystemRights.Write))
                            {
                                return false;
                            }
                        }
                        else if (System.Security.AccessControl.AccessControlType.Allow.Equals(rule.AccessControlType))
                        {
                            if ((((int)System.Security.AccessControl.FileSystemRights.Write) & (int)rule.FileSystemRights) == (int)(System.Security.AccessControl.FileSystemRights.Write))
                            {
                                return true;
                            }
                        }
                    }
                }

            }
            else
            {
                return false;
            }
            return false;
        }

        protected static bool FolderIsWriteable(string foldername)
        {
            WindowsIdentity principal = WindowsIdentity.GetCurrent();
            if (Directory.Exists(foldername))
            {
                DirectoryInfo fi = new DirectoryInfo(foldername);
                System.Security.AccessControl.AuthorizationRuleCollection acl = fi.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
                for (int i = 0; i < acl.Count; i++)
                {
                    System.Security.AccessControl.FileSystemAccessRule rule = (System.Security.AccessControl.FileSystemAccessRule)acl[i];
                    if (principal.User.Equals(rule.IdentityReference))
                    {
                        if (System.Security.AccessControl.AccessControlType.Deny.Equals(rule.AccessControlType))
                        {
                            if ((((int)System.Security.AccessControl.FileSystemRights.Write) & (int)rule.FileSystemRights) == (int)(System.Security.AccessControl.FileSystemRights.Write))
                            {
                                return false;
                            }
                        }
                        else if (System.Security.AccessControl.AccessControlType.Allow.Equals(rule.AccessControlType))
                        {
                            if ((((int)System.Security.AccessControl.FileSystemRights.Write) & (int)rule.FileSystemRights) == (int)(System.Security.AccessControl.FileSystemRights.Write))
                            {
                                return true;
                            }
                        }
                    }
                }

            }
            else
            {
                return false;
            }
            return false;
        }


        public void loadcontrolsStringResource()
        {
            //lblOrderHeader.Text = AppLogic.GetString("admin.charts.StatisticsHeader", 1, ThisCustomer.LocaleSetting);
            //lblOrderHeader.Text = AppLogic.GetString("admin.charts.OrderHeader", 1, ThisCustomer.LocaleSetting);
            //lblCustomerHeader.Text = AppLogic.GetString("admin.charts.CustomerHeader", 1, ThisCustomer.LocaleSetting);

            //rblStats.Items[0].Text = AppLogic.GetString("admin.charts.rbl.ViewStats", 1, ThisCustomer.LocaleSetting);
            //rblStats.Items[0].Value = AppLogic.GetString("admin.charts.rbl.ViewStats", 1, ThisCustomer.LocaleSetting);
            //rblStats.Items[1].Text = AppLogic.GetString("admin.charts.rbl.CompareStats", 1, ThisCustomer.LocaleSetting);
            //rblStats.Items[1].Value = AppLogic.GetString("admin.charts.rbl.CompareStats", 1, ThisCustomer.LocaleSetting);

            //lblDateRangeOption.Text = AppLogic.GetString("admin.charts.dateRange.optionText", 1, ThisCustomer.LocaleSetting);
            //ddOrdersDateRange.Items[0].Text = AppLogic.GetString("admin.charts.dateRange.Today", 1, ThisCustomer.LocaleSetting);
            //ddOrdersDateRange.Items[1].Text = AppLogic.GetString("admin.charts.dateRange.Yesterday", 1, ThisCustomer.LocaleSetting);
            //ddOrdersDateRange.Items[2].Text = AppLogic.GetString("admin.charts.dateRange.ThisWeek", 1, ThisCustomer.LocaleSetting);
            //ddOrdersDateRange.Items[3].Text = AppLogic.GetString("admin.charts.dateRange.LastWeek", 1, ThisCustomer.LocaleSetting);
            //ddOrdersDateRange.Items[4].Text = AppLogic.GetString("admin.charts.dateRange.ThisMonth", 1, ThisCustomer.LocaleSetting);
            //ddOrdersDateRange.Items[5].Text = AppLogic.GetString("admin.charts.dateRange.LastMonth", 1, ThisCustomer.LocaleSetting);
            //ddOrdersDateRange.Items[6].Text = AppLogic.GetString("admin.charts.dateRange.ThisYear", 1, ThisCustomer.LocaleSetting);
            //ddOrdersDateRange.Items[7].Text = AppLogic.GetString("admin.charts.dateRange.LastYear", 1, ThisCustomer.LocaleSetting);
            //ddOrdersDateRange.Items[8].Text = AppLogic.GetString("admin.charts.dateRange.AllTime", 1, ThisCustomer.LocaleSetting);

            //lblchartsType.Text = AppLogic.GetString("admin.charts.chartType.Label", 1, ThisCustomer.LocaleSetting);
            //ddChartType.Items[0].Text = AppLogic.GetString("admin.charts.chartType.Bar", 1, ThisCustomer.LocaleSetting);
            //ddChartType.Items[1].Text = AppLogic.GetString("admin.charts.chartType.Line", 1, ThisCustomer.LocaleSetting);

            //btnCompare.Text = AppLogic.GetString("admin.charts.compare", 1, ThisCustomer.LocaleSetting);
            //btnCompareCustomer.Text = AppLogic.GetString("admin.charts.compare", 1, ThisCustomer.LocaleSetting);
            //ddComparedTransactionType.Items[0].Text = AppLogic.GetString("admin.charts.compare.transactionType1", 1, ThisCustomer.LocaleSetting);
            //ddComparedTransactionType.Items[1].Text = AppLogic.GetString("admin.charts.compare.transactionType2", 1, ThisCustomer.LocaleSetting);
            //ddComparedTransactionType.Items[2].Text = AppLogic.GetString("admin.charts.compare.transactionType3", 1, ThisCustomer.LocaleSetting);
            //ddComparedTransactionType.Items[3].Text = AppLogic.GetString("admin.charts.compare.transactionType4", 1, ThisCustomer.LocaleSetting);

            //ddComparedCustomer.Items[0].Text = AppLogic.GetString("admin.charts.customerChartLegend1", 1, ThisCustomer.LocaleSetting);
            //ddComparedCustomer.Items[1].Text = AppLogic.GetString("admin.charts.customerChartLegend2", 1, ThisCustomer.LocaleSetting);

            //ddCompareDateRange.Items[0].Text = AppLogic.GetString("admin.charts.compare.dateRange1", 1, ThisCustomer.LocaleSetting);
            //ddCompareDateRange.Items[1].Text = AppLogic.GetString("admin.charts.compare.dateRange2", 1, ThisCustomer.LocaleSetting);
            //ddCompareDateRange.Items[2].Text = AppLogic.GetString("admin.charts.compare.dateRange3", 1, ThisCustomer.LocaleSetting);

            //lblComparedTo1.Text = AppLogic.GetString("admin.charts.compareTo", 1, ThisCustomer.LocaleSetting);
            //lblComparedTo2.Text = AppLogic.GetString("admin.charts.compareTo", 1, ThisCustomer.LocaleSetting);
            //lblComparedTo3.Text = AppLogic.GetString("admin.charts.compareTo", 1, ThisCustomer.LocaleSetting);

            //lblLoading.Text = AppLogic.GetString("admin.charts.loading", 1, ThisCustomer.LocaleSetting);

        }

        public void LoadCommonStringResources()
        {


        }

        public void buildDDMonthCompare(DropDownList ddListControl, DropDownList ddListControl2)
        {
            CultureInfo info = Thread.CurrentThread.CurrentCulture;
            string monthDisplay = string.Empty;

            ddListControl.Items.Clear();
            ddListControl2.Items.Clear();
            int defaultMonthSelected = 0;

            for (int i = 1; i <= 12; i++)
            {
                monthDisplay = info.DateTimeFormat.GetMonthName(i);
                ddListControl.Items.Add(new ListItem(monthDisplay, i.ToString()));
                ddListControl2.Items.Add(new ListItem(monthDisplay, i.ToString()));

                if (DateTime.Now.Month == i)
                {
                    defaultMonthSelected = i - 1;
                }
            }

            //if (rblStats.SelectedIndex == 1)
            //{                
            //    string ddMonth1 = string.Empty;
            //    string ddMonth2 = string.Empty;

            //    switch(ddCompareDateRange.SelectedIndex)
            //    {                  
            //        case 1 :
            //            if (Profile.MonthCompareSelectedMonth1.Length == 0 && Profile.MonthCompareSelectedMonth2.Length ==0)
            //    {
            //        ddListControl.SelectedIndex = defaultMonthSelected;

            //        if (ddCompareDateRange.SelectedIndex == 1) 
            //        {
            //            if (defaultMonthSelected == 0)
            //            {
            //                ddListControl2.SelectedIndex = 11;
            //            }
            //            else
            //            {
            //                ddListControl2.SelectedIndex = defaultMonthSelected - 1;
            //            }
            //        }

            //        if (ddCompareDateRange.SelectedIndex == 2)
            //        {
            //            ddListControl2.SelectedIndex = defaultMonthSelected;
            //        }
            //    }
            //    else
            //    {
            //                ddListControl.SelectedIndex = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(Profile.MonthCompareSelectedMonth1), Profile.MonthCompareSelectedMonth1, "0"));                                            
            //                ddListControl2.SelectedIndex = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(Profile.MonthCompareSelectedMonth2), Profile.MonthCompareSelectedMonth2, "0"));                
            //    }           
            //            break;
            //        case 2 :

            //            if (Profile.WeekCompareSelectedMonth1.Length == 0 && Profile.WeekCompareSelectedMonth2.Length == 0)
            //            {
            //                ddListControl.SelectedIndex = defaultMonthSelected;

            //                if (ddCompareDateRange.SelectedIndex == 1)
            //                {
            //                    if (defaultMonthSelected == 0)
            //                    {
            //                        ddListControl2.SelectedIndex = 11;
            //                    }
            //                    else
            //                    {
            //                        ddListControl2.SelectedIndex = defaultMonthSelected - 1;
            //                    }
            //                }

            //                if (ddCompareDateRange.SelectedIndex == 2)
            //                {
            //                    ddListControl2.SelectedIndex = defaultMonthSelected;
            //                }
            //            }
            //            else
            //            {
            //                ddListControl.SelectedIndex = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(Profile.WeekCompareSelectedMonth1), Profile.WeekCompareSelectedMonth1, "0"));                                            
            //                ddListControl2.SelectedIndex = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(Profile.WeekCompareSelectedMonth2), Profile.WeekCompareSelectedMonth2, "0"));                                            
            //            }      
            //            break;
            //    }        
            //}           
        }

        protected void loadFeeds()
        {
            divFeeds.Visible = AppLogic.AppConfigBool("Admin.ShowNewsFeed");
            cpxFeeds.Enabled = AppLogic.AppConfigBool("Admin.ShowNewsFeed");
            pnlNewsFeed.Visible = AppLogic.AppConfigBool("Admin.ShowNewsFeed");
            pnlSponsorFeed.Visible = AppLogic.AppConfigBool("Admin.ShowSponsorFeed");
        }

        protected void loadSummaryReport(string view)
        {
            StatsTable.Visible = true;
            List<Statistic> stats = Statistic.GetCustomerStatistic();
            gridCustomerStats.DataSource = stats;
            gridCustomerStats.DataBind();

            StatsTable.Visible = true;
            List<Statistic> orderStats = Statistic.GetOrdersStatistic(AppLogic.ro_TXStateAuthorized, false, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            gridAuthorized.DataSource = orderStats;
            gridAuthorized.DataBind();

            List<Statistic> orderStats2 = Statistic.GetOrdersStatistic(AppLogic.ro_TXStateCaptured, false, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            gridCaptured.DataSource = orderStats2;
            gridCaptured.DataBind();

            List<Statistic> orderStats3 = Statistic.GetOrdersStatistic(AppLogic.ro_TXStateVoided, true, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            gridVoided.DataSource = orderStats3;
            gridVoided.DataBind();

            List<Statistic> orderStats4 = Statistic.GetOrdersStatistic(AppLogic.ro_TXStateRefunded, true, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            gridRefunded.DataSource = orderStats4;
            gridRefunded.DataBind();

            ViewState.Add("StatsView", "Chart");
            Profile.StatsView = "Tabular";
        }

        protected void loadGrids()
        {
            //Orders
            string SummaryReportFields = "OrderNumber,OrderDate,OrderTotal,FirstName,LastName,ShippingMethod,isnull(MaxMindFraudScore, -1) MaxMindFraudScore ";
            String summarySQL = "SELECT TOP 50 " + SummaryReportFields + " from Orders with (NOLOCK) where IsNew=1 ORDER BY OrderDate DESC";

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();

                using (IDataReader rs = DB.GetRS(summarySQL, con))
                {
                    gOrders.DataSource = rs;
                    gOrders.DataBind();
                }


                //Customers
                SummaryReportFields = "CustomerID,RegisterDate,FirstName,LastName ";
                summarySQL = "SELECT TOP 25 " + SummaryReportFields + " from Customer with (NOLOCK) WHERE IsRegistered=1 ORDER BY RegisterDate DESC";

                using (IDataReader rs = DB.GetRS(summarySQL, con))
                {
                    gCustomers.DataSource = rs;
                    gCustomers.DataBind();
                }

            }
        }

        protected void loadInformation()
        {
            ltOnLiveServer.Text = AppLogic.OnLiveServer().ToString();
            ltServerPortSecure.Text = CommonLogic.IsSecureConnection().ToString();
            ltStoreVersion.Text = CommonLogic.GetVersion();
            lnkCacheSwitch.Text = (AppLogic.AppConfigBool("CacheMenus") ?
                AppLogic.GetString("admin.common.OnUC", m_SkinID, ThisCustomer.LocaleSetting) + ": " +
                AppLogic.GetString("admin.splash.aspx.19", m_SkinID, ThisCustomer.LocaleSetting) : AppLogic.GetString("admin.common.OffUC", m_SkinID, ThisCustomer.LocaleSetting) + ": " +
                AppLogic.GetString("admin.splash.aspx.18", m_SkinID, ThisCustomer.LocaleSetting));
            ltWebConfigLocaleSetting.Text = Localization.GetDefaultLocale();
            ltSQLLocaleSetting.Text = Localization.GetSqlServerLocale();
            ltCustomerLocaleSetting.Text = ThisCustomer.LocaleSetting;
            ltPaymentGateway.Text = AppLogic.ActivePaymentGatewayRAW();
            ltUseLiveTransactions.Text = (AppLogic.AppConfigBool("UseLiveTransactions") ? AppLogic.GetString("admin.splash.aspx.20", m_SkinID, ThisCustomer.LocaleSetting) : AppLogic.GetString("admin.splash.aspx.21", m_SkinID, ThisCustomer.LocaleSetting));
            ltTransactionMode.Text = AppLogic.AppConfig("TransactionMode").ToString();
            ltPaymentMethods.Text = AppLogic.AppConfig("PaymentMethods").ToString();
            ltTrustLevel.Text = AppLogic.TrustLevel.ToString();



            CardinalEnabled.Text = AppLogic.AppConfigBool("CardinalCommerce.Centinel.Enabled").ToString();
            ltMicroPayEnabled.Text = AppLogic.MicropayIsEnabled().ToString();
            StoreCC.Text = AppLogic.StoreCCInDB().ToString();
            GatewayRecurringBilling.Text = AppLogic.AppConfigBool("Recurring.UseGatewayInternalBilling").ToString();

            litBuySafeStatus.Text = GetBuySafeStatus();

            ltUseSSL.Text = AppLogic.UseSSL().ToString();
            PrimaryCurrency.Text = Localization.GetPrimaryCurrency();

            if (!ThisCustomer.IsAdminSuperUser)
            {
                MonthlyMaintenancePrompt.Visible = false;
            }

            if (!ThisCustomer.IsAdminSuperUser)
            {
                VersionInfoDetails.Visible = false;
            }
        }

        private string GetBuySafeStatus()
        {
            String retValue = "";
            Boolean FullyFuncitonal = true;
            Boolean BuySafeWorking = AspDotNetStorefrontBuySafe.BuySafeController.IsWorking();
            if (!BuySafeWorking)
            {
                FullyFuncitonal = false;
                retValue += "False";
            }
            else
            {
                retValue += "True";
                AspDotNetStorefrontBuySafe.BuySafeStoreStatuses statuses = AspDotNetStorefrontBuySafe.BuySafeController.GetStoreStatuses();
                int disfunctonalStores = statuses.UnRegisteredStores.Count;
                if (disfunctonalStores != 0)
                {
                    FullyFuncitonal = false;
                    int functionalStores = statuses.RegisteredStores.Count;
                    int totalstores = functionalStores + disfunctonalStores;
                    retValue += String.Format(" ({0} of {1} stores)", functionalStores, totalstores);
                }
            }

            if (!FullyFuncitonal)
            {
                retValue += " <a href=\"buysafeSetup.aspx\">Learn more</a>";
            }
            return retValue;
        }

        protected void resetError(string error, bool isError)
        {
            if (error.Length > 0)
            {
                divLtError.Visible = true;
                if (isError)
                {
                    ltError.Text += "<font class=\"noticeMsg\">" + AppLogic.GetString("admin.common.Error", m_SkinID, ThisCustomer.LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;" + error + "<br />";
                }
                else
                {
                    ltError.Text += "<font class=\"noticeMsg\">" + AppLogic.GetString("admin.common.Notice", m_SkinID, ThisCustomer.LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;" + error + "<br />";
                }
            }
            else
            {
                divLtError.Visible = false;
                ltError.Text = "";
            }
        }

        protected void gOrders_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                //Order
                e.Row.Cells[0].Text = AppLogic.GetString("admin.common.Order", m_SkinID, ThisCustomer.LocaleSetting);
                //Date
                e.Row.Cells[1].Text = AppLogic.GetString("admin.common.Date", m_SkinID, ThisCustomer.LocaleSetting);
                //Customer
                e.Row.Cells[2].Text = AppLogic.GetString("admin.common.Customer", m_SkinID, ThisCustomer.LocaleSetting);
                //Shipping
                e.Row.Cells[3].Text = AppLogic.GetString("admin.common.Shipping", m_SkinID, ThisCustomer.LocaleSetting);
                //Total
                e.Row.Cells[4].Text = AppLogic.GetString("admin.common.Total", m_SkinID, ThisCustomer.LocaleSetting);
            }
        }

        protected void gCustomers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                //CustomerID
                e.Row.Cells[0].Text = AppLogic.GetString("admin.common.CustomerID", m_SkinID, ThisCustomer.LocaleSetting);
                //Date
                e.Row.Cells[1].Text = AppLogic.GetString("admin.common.Date", m_SkinID, ThisCustomer.LocaleSetting);
                //Customer
                e.Row.Cells[2].Text = AppLogic.GetString("admin.common.Customer", m_SkinID, ThisCustomer.LocaleSetting);
            }
        }

        protected void linkbtnSwitchView_Click(object sender, EventArgs e)
        {
            if (ThisCustomer.IsAdminSuperUser)
            {
                loadSummaryReport(ViewState["StatsView"].ToString());
            }
            else
            {
                //chartTable.Visible = false;
                StatsTable.Visible = false;
            }
        }


        /// <summary>
        /// Represent Order Statistics
        /// </summary>
        public class Statistic
        {
            #region Private Properties

            //this is for the chart type statistics
            private decimal m_shippingcost;
            private decimal m_subTotal;

            private decimal m_authorized;
            private decimal m_captured;
            private decimal m_voided;
            private decimal m_refunded;
            private decimal m_tax;

            private decimal m_authorized2;
            private decimal m_captured2;
            private decimal m_voided2;
            private decimal m_refunded2;

            private int m_authorizedCount;
            private int m_capturedCount;
            private int m_voidedCount;
            private int m_refundedCount;

            private int m_authorizedCount2;
            private int m_capturedCount2;
            private int m_voidedCount2;
            private int m_refundedCount2;

            private DateTime m_comparedFrom;
            private DateTime m_comparedTo;

            private string m_dayDisplay;
            private string m_dayDisplay2;

            private DateTime m_dateDisplay;
            private DateTime m_dateDisplay2;

            //this is for the table type statistics
            private string m_today;
            private string m_thisWeek;
            private string m_thisMonth;
            private string m_thisYear;
            private string m_allTime;
            private string m_customerType;

            //for customer Graph
            int m_RegisteredCustomers;
            int m_AnonCustomers;
            int m_RegisteredCustomers2;
            int m_AnonCustomers2;

            //for pie chart
            string m_averageName;
            decimal m_average;

            private string m_orderSummaryName;



            #endregion


            #region Constructor

            public Statistic() { }

            public Statistic(decimal shippingTotalAmount, decimal SubtotalAmount, decimal VoidedTotalAmount, decimal RefundedTotalAmount, decimal TaxTotalAmount)
            {
                m_shippingcost = shippingTotalAmount;
                m_subTotal = SubtotalAmount;
                m_voided = VoidedTotalAmount;
                m_refunded = RefundedTotalAmount;
                m_tax = TaxTotalAmount;
            }

            public Statistic(string today, string thisWeek, string thisMonth, string thisYear, string allTime)
            {
                m_today = today;
                m_thisWeek = thisWeek;
                m_thisMonth = thisMonth;
                m_thisYear = thisYear;
                m_allTime = allTime;
            }

            public Statistic(string summaryName, string today, string thisWeek, string thisMonth, string thisYear, string allTime)
            {
                m_orderSummaryName = summaryName;
                m_today = today;
                m_thisWeek = thisWeek;
                m_thisMonth = thisMonth;
                m_thisYear = thisYear;
                m_allTime = allTime;
            }

            public Statistic(string averageName, decimal averageValue)
            {
                m_averageName = averageName;
                m_average = averageValue;
            }

            #endregion


            #region Public Properties for tabular statistics

            public string Today
            {
                get { return m_today; }
                set { m_today = value; }
            }

            public string ThisWeek
            {
                get { return m_thisWeek; }
                set { m_thisWeek = value; }
            }

            public string ThisMonth
            {
                get { return m_thisMonth; }
                set { m_thisMonth = value; }
            }

            public string ThisYear
            {
                get { return m_thisYear; }
                set { m_thisYear = value; }
            }

            public string AllTime
            {
                get { return m_allTime; }
                set { m_allTime = value; }
            }

            public string CustomerType
            {
                get { return m_customerType; }
                set { m_customerType = value; }
            }

            public string OrderSummaryName
            {
                get { return m_orderSummaryName; }
                set { m_orderSummaryName = value; }
            }

            #endregion


            #region Public Properties for chart type statistics

            public DateTime DateDisplayName
            {
                get { return m_dateDisplay; }
                set { m_dateDisplay = value; }
            }

            public DateTime DateDisplayName2
            {
                get { return m_dateDisplay2; }
                set { m_dateDisplay2 = value; }
            }

            public string DayDisplayName
            {
                get { return m_dayDisplay; }
                set { m_dayDisplay = value; }
            }

            public string DayDisplayName2
            {
                get { return m_dayDisplay2; }
                set { m_dayDisplay2 = value; }
            }

            public decimal ShippingCost
            {
                get { return m_shippingcost; }
                set { m_shippingcost = value; }
            }

            public decimal Subtotal
            {
                get { return m_subTotal; }
                set { m_subTotal = value; }
            }

            public decimal Authorized
            {
                get { return m_authorized; }
                set { m_authorized = value; }
            }

            public int AuthorizedCount
            {
                get { return m_authorizedCount; }
                set { m_authorizedCount = value; }
            }

            public decimal Authorized2
            {
                get { return m_authorized2; }
                set { m_authorized2 = value; }
            }

            public int AuthorizedCount2
            {
                get { return m_authorizedCount2; }
                set { m_authorizedCount2 = value; }
            }

            public decimal Captured
            {
                get { return m_captured; }
                set { m_captured = value; }
            }

            public int CapturedCount
            {
                get { return m_capturedCount; }
                set { m_capturedCount = value; }
            }

            public decimal Captured2
            {
                get { return m_captured2; }
                set { m_captured2 = value; }
            }

            public int CapturedCount2
            {
                get { return m_capturedCount2; }
                set { m_capturedCount2 = value; }
            }

            public decimal Voided
            {
                get { return m_voided; }
                set { m_voided = value; }
            }

            public int VoidedCount
            {
                get { return m_voidedCount; }
                set { m_voidedCount = value; }
            }

            public decimal Voided2
            {
                get { return m_voided2; }
                set { m_voided2 = value; }
            }

            public int VoidedCount2
            {
                get { return m_voidedCount2; }
                set { m_voidedCount2 = value; }
            }

            public decimal Refunded
            {
                get { return m_refunded; }
                set { m_refunded = value; }
            }

            public int RefundedCount
            {
                get { return m_refundedCount; }
                set { m_refundedCount = value; }
            }

            public decimal Refunded2
            {
                get { return m_refunded2; }
                set { m_refunded2 = value; }
            }

            public int RefundedCount2
            {
                get { return m_refundedCount2; }
                set { m_refundedCount2 = value; }
            }

            public decimal Tax
            {
                get { return m_tax; }
                set { m_tax = value; }
            }

            public DateTime ComparedFrom
            {
                get { return m_comparedFrom; }
                set { m_comparedFrom = value; }
            }

            public DateTime ComparedTo
            {
                get { return m_comparedTo; }
                set { m_comparedTo = value; }
            }

            #endregion


            #region Public Properties for pie chart statistics

            public string AverageName
            {
                get { return m_averageName; }
                set { m_averageName = value; }
            }

            public decimal Average
            {
                get { return m_average; }
                set { m_average = value; }
            }

            #endregion


            #region public properties for chart customer stats

            public int RegistredCustomers
            {
                get { return m_RegisteredCustomers; }
                set { m_RegisteredCustomers = value; }
            }

            public int AnonCustomers
            {
                get { return m_AnonCustomers; }
                set { m_AnonCustomers = value; }
            }

            public int RegistredCustomers2
            {
                get { return m_RegisteredCustomers2; }
                set { m_RegisteredCustomers2 = value; }
            }

            public int AnonCustomers2
            {
                get { return m_AnonCustomers2; }
                set { m_AnonCustomers2 = value; }
            }

            #endregion


            #region methods use for charts

            public void GetCustomerSummary(DateTime comparedFrom, DateTime comparedTo, CompareDateRanges daterange, string compareCustomerType)
            {
                int day = comparedFrom.Day;
                int day2 = comparedTo.Day;
                int month = comparedFrom.Month;
                int month2 = comparedTo.Month;
                int year = comparedFrom.Year;
                int year2 = comparedTo.Year;
                int index;
                int index2;

                int[] values = new int[2];
                string[] sql = new string[2];

                int transField = 0;
                switch (compareCustomerType)
                {
                    case "Anonymous":
                        transField = 0;
                        break;
                    case "Registered":
                        transField = 1;
                        break;
                }


                if (daterange == CompareDateRanges.CompareMonths || daterange == CompareDateRanges.CompareWeeks)
                {
                    sql[0] = "select count(*) as " + compareCustomerType + " from customer  where (DatePart(yy, Registerdate) = " + year + " and DatePart(mm, Registerdate) = " + month + " and DatePart(day, Registerdate) = " + day + ") and IsRegistered = " + transField;
                    sql[1] = "select count(*) as " + compareCustomerType + " from customer  where (DatePart(yy, Registerdate) = " + year2 + " and DatePart(mm, Registerdate) = " + month2 + " and DatePart(day, Registerdate) = " + day2 + ") and IsRegistered = " + transField;
                }

                if (daterange == CompareDateRanges.CompareYear)
                {
                    sql[0] = "select count(*) as " + compareCustomerType + " from customer  where (DatePart(yy, Registerdate) = " + year + " and DatePart(mm, Registerdate) = " + month + ") and IsRegistered = " + transField;
                    sql[1] = "select count(*) as " + compareCustomerType + " from customer  where (DatePart(yy, Registerdate) = " + year2 + " and DatePart(mm, Registerdate) = " + month2 + ") and IsRegistered = " + transField;
                }

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    for (int c = 0; c < 2; c++)
                    {
                        using (IDataReader rs = DB.GetRS(sql[c], conn))
                        {
                            if (rs.Read())
                            {
                                values[c] = DB.RSFieldInt(rs, compareCustomerType);
                            }
                        }
                    }
                }

                m_comparedFrom = comparedFrom;
                m_comparedTo = comparedTo;

                if (comparedFrom < comparedTo)
                {
                    m_dateDisplay = comparedFrom;
                    m_dateDisplay2 = comparedTo;

                }
                else
                {
                    m_dateDisplay = comparedTo;
                    m_dateDisplay2 = comparedFrom;
                }

                index = CommonLogic.IIF(comparedFrom < comparedTo, 0, 1);
                index2 = CommonLogic.IIF(index == 0, 1, 0);

                if (compareCustomerType.Equals("Anonymous"))
                {
                    m_AnonCustomers = values[index];
                    m_AnonCustomers2 = values[index2];
                }

                if (compareCustomerType.Equals("Registered"))
                {
                    m_RegisteredCustomers = values[index];
                    m_RegisteredCustomers2 = values[index2];
                }

            }

            /// <summary>
            /// Gets the Summary of Orders based from the specified dates and transaction state
            /// </summary>
            /// <param name="comparedFrom">the date compared</param>
            /// <param name="comparedTo">the date compared to</param>
            /// <param name="daterange">the date range type</param>
            /// <param name="compareTransactionType">the transaction state</param>
            public void GetOrderSummary(DateTime comparedFrom, DateTime comparedTo, CompareDateRanges daterange, string compareTransactionType)
            {
                int day = comparedFrom.Day;
                int day2 = comparedTo.Day;
                int month = comparedFrom.Month;
                int month2 = comparedTo.Month;
                int year = comparedFrom.Year;
                int year2 = comparedTo.Year;
                int index;
                int index2;

                string[] transType = { AppLogic.ro_TXStateAuthorized, AppLogic.ro_TXStateCaptured, AppLogic.ro_TXStateVoided, AppLogic.ro_TXStateRefunded };
                decimal[] values = new decimal[2];
                int[] count = new int[2];
                string[] sql = new string[2];
                string tableCol = string.Empty;

                string transField = string.Empty;
                switch (compareTransactionType)
                {
                    case "Authorized":
                        transField = "AuthorizedOn";
                        break;
                    case "Captured":
                        transField = "CapturedOn";
                        break;
                    case "Voided":
                        transField = "VoidedOn";
                        break;
                    case "Refunded":
                        transField = "RefundedOn";
                        break;
                }

                if (compareTransactionType.Equals(AppLogic.ro_TXStateAuthorized, StringComparison.CurrentCultureIgnoreCase) || compareTransactionType.Equals(AppLogic.ro_TXStateCaptured, StringComparison.CurrentCultureIgnoreCase))
                {
                    tableCol = "orderSubtotal";
                }

                if (compareTransactionType.Equals(AppLogic.ro_TXStateVoided, StringComparison.CurrentCultureIgnoreCase) || compareTransactionType.Equals(AppLogic.ro_TXStateRefunded, StringComparison.CurrentCultureIgnoreCase))
                {
                    tableCol = "OrderTotal";
                }

                if (daterange == CompareDateRanges.CompareMonths || daterange == CompareDateRanges.CompareWeeks)
                {

                    sql[0] = "select sum(" + tableCol + ") AS " + compareTransactionType + ", Count(" + tableCol + ") AS " + compareTransactionType + "Count from orders with (NOLOCK) where ((DATEPART(mm, " + transField + ") = " + month + ") and (DATEPART(day, " + transField + ") = " + day + ") and (DATEPART(yy, " + transField + ") = " + year + ")) and EditedOn IS NULL and transactionstate = '" + compareTransactionType + "'";
                    sql[1] = "select sum(" + tableCol + ") AS " + compareTransactionType + ", Count(" + tableCol + ") AS " + compareTransactionType + "Count from orders with (NOLOCK) where ((DATEPART(mm, " + transField + ") = " + month2 + ") and (DATEPART(day, " + transField + ") = " + day2 + ") and (DATEPART(yy, " + transField + ") = " + year2 + ")) and EditedOn IS NULL and transactionstate = '" + compareTransactionType + "'";
                }

                if (daterange == CompareDateRanges.CompareYear)
                {
                    sql[0] = "select sum(" + tableCol + ") AS " + compareTransactionType + ", Count(" + tableCol + ") AS " + compareTransactionType + "Count from orders with (NOLOCK) where ((DATEPART(mm, " + transField + ") = " + month + ") and (DATEPART(yy, " + transField + ") = " + year + ")) and EditedOn IS NULL and transactionstate = '" + compareTransactionType + "'";
                    sql[1] = "select sum(" + tableCol + ") AS " + compareTransactionType + ", Count(" + tableCol + ") AS " + compareTransactionType + "Count from orders with (NOLOCK) where ((DATEPART(mm, " + transField + ") = " + month2 + ") and (DATEPART(yy, " + transField + ") = " + year2 + ")) and EditedOn IS NULL and transactionstate = '" + compareTransactionType + "'";
                }

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    for (int c = 0; c < 2; c++)
                    {
                        using (IDataReader rs = DB.GetRS(sql[c], conn))
                        {
                            if (rs.Read())
                            {
                                values[c] = DB.RSFieldDecimal(rs, compareTransactionType);
                                count[c] = DB.RSFieldInt(rs, compareTransactionType + "Count");
                            }
                        }
                    }
                }

                m_comparedFrom = comparedFrom;
                m_comparedTo = comparedTo;

                if (comparedFrom < comparedTo)
                {
                    m_dateDisplay = comparedFrom;
                    m_dateDisplay2 = comparedTo;

                }
                else
                {
                    m_dateDisplay = comparedTo;
                    m_dateDisplay2 = comparedFrom;
                }


                index = CommonLogic.IIF(comparedFrom < comparedTo, 0, 1);
                index2 = CommonLogic.IIF(index == 0, 1, 0);
                if (compareTransactionType.Equals(AppLogic.ro_TXStateAuthorized, StringComparison.CurrentCultureIgnoreCase))
                {
                    m_authorized = values[index];
                    m_authorizedCount = count[index];
                    m_authorized2 = values[index2];
                    m_authorizedCount2 = count[index2];
                }

                if (compareTransactionType.Equals(AppLogic.ro_TXStateCaptured, StringComparison.CurrentCultureIgnoreCase))
                {
                    m_captured = values[index];
                    m_capturedCount = count[index];
                    m_captured2 = values[index2];
                    m_capturedCount2 = count[index2];
                }

                if (compareTransactionType.Equals(AppLogic.ro_TXStateVoided, StringComparison.CurrentCultureIgnoreCase))
                {
                    m_voided = values[index];
                    m_voidedCount = count[index];
                    m_voided2 = values[index2];
                    m_voidedCount2 = count[index2];
                }

                if (compareTransactionType.Equals(AppLogic.ro_TXStateRefunded, StringComparison.CurrentCultureIgnoreCase))
                {
                    m_refunded = values[index];
                    m_refundedCount = count[index];
                    m_refunded2 = values[index2];
                    m_refundedCount2 = count[index2];
                }
            }

            public void GetOrderSummary(DateTime orderdate, DateRanges daterange, DateIntervalTypeEnum displayInterval)
            {
                CultureInfo info = Thread.CurrentThread.CurrentCulture;
                DayOfWeek dayName = info.Calendar.GetDayOfWeek(orderdate);
                int day = orderdate.Day;
                int month = orderdate.Month;
                int year = orderdate.Year;
                string[] transType = { AppLogic.ro_TXStateAuthorized, AppLogic.ro_TXStateCaptured, AppLogic.ro_TXStateVoided, AppLogic.ro_TXStateRefunded };
                string[] transField = { "AuthorizedOn", "CapturedOn", "VoidedOn", "RefundedOn" };
                decimal[] values = new decimal[4];
                int[] count = new int[4];
                string sql = string.Empty;

                for (int i = 0; i < 4; i++)
                {
                    string tableCol = CommonLogic.IIF(i < 2, "orderSubtotal", "OrderTotal");

                    if (daterange == DateRanges.ThisWeek || daterange == DateRanges.LastWeek || daterange == DateRanges.ThisMonth || daterange == DateRanges.LastMonth || daterange == DateRanges.Today || daterange == DateRanges.Yesterday)
                    {
                        sql = "select sum(" + tableCol + ") AS " + transType[i] + ", Count(" + tableCol + ") AS " + transType[i] + "Count from orders with (NOLOCK) where ((DATEPART(mm, " + transField[i] + ") = " + month + ") and (DATEPART(day, " + transField[i] + ") = " + day + ") and (DATEPART(yy, " + transField[i] + ") = " + year + ")) and EditedOn IS NULL and transactionstate = '" + transType[i] + "'";
                        m_dayDisplay = dayName.ToString();
                    }

                    if (daterange == DateRanges.ThisYear || daterange == DateRanges.LastYear)
                    {
                        sql = "select sum(" + tableCol + ") AS " + transType[i] + ", Count(" + tableCol + ") AS " + transType[i] + "Count from orders with (NOLOCK) where ((DATEPART(mm, " + transField[i] + ") = " + month + ") and (DATEPART(yy, " + transField[i] + ") = " + year + ")) and EditedOn IS NULL and transactionstate = '" + transType[i] + "'";
                    }

                    if (daterange == DateRanges.AllTime)
                    {
                        if (displayInterval == DateIntervalTypeEnum.Day)
                        {
                            sql = "select sum(" + tableCol + ") AS " + transType[i] + ", Count(" + tableCol + ") AS " + transType[i] + "Count from orders with (NOLOCK) where ((DATEPART(mm, " + transField[i] + ") = " + month + ") and (DATEPART(day, " + transField[i] + ") = " + day + ") and (DATEPART(yy, " + transField[i] + ") = " + year + ")) and EditedOn IS NULL and transactionstate = '" + transType[i] + "'";
                        }

                        if (displayInterval == DateIntervalTypeEnum.Month)
                        {
                            sql = "select sum(" + tableCol + ") AS " + transType[i] + ", Count(" + tableCol + ") AS " + transType[i] + "Count from orders with (NOLOCK) where ((DATEPART(mm, " + transField[i] + ") = " + month + ") and (DATEPART(yy, " + transField[i] + ") = " + year + ")) and EditedOn IS NULL and transactionstate = '" + transType[i] + "'";
                        }

                        if (displayInterval == DateIntervalTypeEnum.Year)
                        {
                            sql = "select sum(" + tableCol + ") AS " + transType[i] + ", Count(" + tableCol + ") AS " + transType[i] + "Count from orders with (NOLOCK) where ((DATEPART(yy, " + transField[i] + ") = " + year + ")) and EditedOn IS NULL and transactionstate = '" + transType[i] + "'";
                        }
                    }

                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS(sql, conn))
                        {
                            if (rs.Read())
                            {
                                values[i] = DB.RSFieldDecimal(rs, transType[i]);
                                count[i] = DB.RSFieldInt(rs, transType[i] + "Count");
                            }
                        }
                    }
                }

                m_authorized = values[0];
                m_captured = values[1];
                m_voided = values[2];
                m_refunded = values[3];

                m_authorizedCount = count[0];
                m_capturedCount = count[1];
                m_voidedCount = count[2];
                m_refundedCount = count[3];
            }

            public void GetOrderSummary(DateTime orderdate, DateRanges daterange)
            {
                GetOrderSummary(orderdate, daterange, DateIntervalTypeEnum.Day);
            }

            public void GetCustomerStats(DateTime registerDate, DateRanges daterange, DateIntervalTypeEnum displayInterval)
            {
                CultureInfo info = Thread.CurrentThread.CurrentCulture;
                DayOfWeek dayName = info.Calendar.GetDayOfWeek(registerDate);
                int year = registerDate.Year;
                int month = registerDate.Month;
                int day = registerDate.Day;
                string sql = string.Empty;

                string SuperuserFilter = string.Empty;

                IntegerCollection superUserIds = AppLogic.GetSuperAdminCustomerIDs();
                if (superUserIds.Count > 0)
                {
                    SuperuserFilter = String.Format("and Customer.CustomerID not in ({0})", superUserIds.ToString());
                }

                for (int i = 0; i < 2; i++)
                {
                    if (daterange == DateRanges.ThisWeek || daterange == DateRanges.LastWeek || daterange == DateRanges.ThisMonth || daterange == DateRanges.LastMonth || daterange == DateRanges.Today || daterange == DateRanges.Yesterday)
                    {
                        sql = string.Format("select count(*) as Count from customer  where (DatePart(yy, Registerdate) = {0} and DatePart(mm, Registerdate) = {1} and DatePart(day, Registerdate) = {2}) and IsRegistered = {3} {4}", year, month, day, i, SuperuserFilter);
                        m_dayDisplay = dayName.ToString();
                    }

                    if (daterange == DateRanges.ThisYear || daterange == DateRanges.LastYear)
                    {
                        sql = string.Format("select count(*) as Count from customer  where (DatePart(yy, Registerdate) = {0} and DatePart(mm, Registerdate) = {1}) and IsRegistered = {2} {3}", year, month, i, SuperuserFilter);
                    }

                    if (daterange == DateRanges.AllTime)
                    {
                        if (displayInterval == DateIntervalTypeEnum.Day)
                        {
                            sql = string.Format("select count(*) as Count from customer  where (DatePart(yy, Registerdate) = {0} and DatePart(mm, Registerdate) = {1} and DatePart(day, Registerdate) = {2}) and IsRegistered = {3} {4}", year, month, day, i, SuperuserFilter);
                        }

                        if (displayInterval == DateIntervalTypeEnum.Month)
                        {
                            sql = string.Format("select count(*) as Count from customer  where (DatePart(yy, Registerdate) = {0} and DatePart(mm, Registerdate) = {1}) and IsRegistered = {2} {3}", year, month, i, SuperuserFilter);
                        }

                        if (displayInterval == DateIntervalTypeEnum.Year)
                        {
                            sql = string.Format("select count(*) as Count from customer  where (DatePart(yy, Registerdate) = {0}) and IsRegistered = {1} {2}", year, i, SuperuserFilter);
                        }
                    }

                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS(sql, conn))
                        {
                            if (rs.Read())
                            {
                                if (i == 0)
                                {
                                    AnonCustomers = DB.RSFieldInt(rs, "Count");
                                }
                                else
                                {
                                    RegistredCustomers = DB.RSFieldInt(rs, "Count");
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// get the customer count for the last 30 days 
            /// </summary>
            /// <param name="registerDate">customers date of registration</param>
            public void GetCustomerStats(DateTime registerDate, DateRanges daterange)
            {
                GetCustomerStats(registerDate, daterange, DateIntervalTypeEnum.Day);
            }

            /// <summary>
            /// get the year count where there are order records in the ORDER table. Returns the year of last order and the year count
            /// </summary>
            /// <param name="yearOfLastOrder">returns the year of last order</param>
            /// <param name="yearCount">returns the year count where there are orders</param>
            public static void GetOrderYearCount(out int yearOfLastRecord, out int yearCount)
            {
                DateTime firstOrderDate = DateTime.MinValue;
                DateTime lastOrderDate = DateTime.MinValue;
                DateTime firstRegDate = DateTime.MinValue;
                DateTime lastRegDate = DateTime.MinValue;

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("SELECT TOP(1) OrderDate AS FirstOrder FROM Orders WITH (NOLOCK) ORDER BY OrderDate ASC", conn))
                    {
                        if (rs.Read())
                        {
                            firstOrderDate = DB.RSFieldDateTime(rs, "FirstOrder");
                        }
                    }
                }

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("SELECT TOP(1) OrderDate AS LastOrder FROM Orders WITH (NOLOCK) ORDER BY OrderDate DESC", conn))
                    {
                        if (rs.Read())
                        {
                            lastOrderDate = DB.RSFieldDateTime(rs, "LastOrder");
                        }
                    }
                }

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("SELECT TOP(1) RegisterDate AS FirstRegistered FROM Customer WITH (NOLOCK) ORDER BY RegisterDate ASC", conn))
                    {
                        if (rs.Read())
                        {
                            firstRegDate = DB.RSFieldDateTime(rs, "FirstRegistered");
                        }
                    }
                }

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("SELECT TOP(1) RegisterDate AS LastRegistered FROM Customer WITH (NOLOCK) ORDER BY RegisterDate DESC", conn))
                    {
                        if (rs.Read())
                        {
                            lastRegDate = DB.RSFieldDateTime(rs, "LastRegistered");
                        }
                    }
                }

                int lastYearToValidate = CommonLogic.IIF(lastOrderDate.Year > lastRegDate.Year, lastOrderDate.Year, lastRegDate.Year);
                int firstYeartoValidate = CommonLogic.IIF(firstOrderDate.Year < firstRegDate.Year, firstOrderDate.Year, firstRegDate.Year);

                yearOfLastRecord = CommonLogic.IIF(lastYearToValidate > DateTime.Now.Year, lastYearToValidate, DateTime.Now.Year);

                yearCount = yearOfLastRecord - firstYeartoValidate;

                if (firstOrderDate == DateTime.MinValue && lastOrderDate == DateTime.MinValue)
                {
                    yearCount = -1;
                }
            }

            /// <summary>
            /// get the year count where there are order records in the ORDER table. Returns containing 
            /// </summary>
            /// <param name="firstDate">when this method returns, it contains the first order date on the Order Table. </param>
            /// <param name="yearCount">when this method returns, it contains the year count where there are orders</param>
            /// <param name="hasOrder">when this method returns, it contains a boolean that tells if there are orders existing</param>
            public static void GetStoreAge(string chartControlId, out int yearCount, ref int monthCount, ref bool hasOrder, out TimeSpan storeTimeSpan, out DateTime firstDate)
            {
                DateTime firstOrderDate = DateTime.MinValue;
                DateTime lastOrderDate = DateTime.MinValue;
                DateTime firstRegDate = DateTime.MinValue;
                DateTime lastRegDate = DateTime.MinValue;

                if (chartControlId.Equals("OrdersChart"))
                {
                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS("SELECT TOP(1) OrderDate AS FirstOrder FROM Orders WITH (NOLOCK) ORDER BY OrderDate ASC", conn))
                        {
                            if (rs.Read())
                            {
                                firstOrderDate = DB.RSFieldDateTime(rs, "FirstOrder");
                                if (firstOrderDate != DateTime.MinValue) // need to check if there is actually an order existing. 
                                {
                                    hasOrder = true;
                                }
                            }
                        }
                    }

                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS("SELECT TOP(1) OrderDate AS LastOrder FROM Orders WITH (NOLOCK) ORDER BY OrderDate DESC", conn))
                        {
                            if (rs.Read())
                            {
                                lastOrderDate = DB.RSFieldDateTime(rs, "LastOrder");
                            }
                        }
                    }

                    yearCount = DateTime.Now.Year - firstOrderDate.Year;

                    if (firstOrderDate == DateTime.MinValue && lastOrderDate == DateTime.MinValue)
                    {
                        yearCount = -1;
                    }

                    storeTimeSpan = lastOrderDate.Subtract(firstOrderDate);
                    firstDate = firstOrderDate;
                    if (storeTimeSpan.Days < 365)
                    {
                        monthCount = lastOrderDate.Month - firstOrderDate.Month;
                    }
                }
                else
                {
                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS("SELECT TOP(1) RegisterDate AS FirstRegistered FROM Customer WITH (NOLOCK) ORDER BY RegisterDate ASC", conn))
                        {
                            if (rs.Read())
                            {
                                firstRegDate = DB.RSFieldDateTime(rs, "FirstRegistered");
                                if (firstRegDate != DateTime.MinValue)
                                {
                                    hasOrder = true;
                                }
                            }
                        }
                    }

                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS("SELECT TOP(1) RegisterDate AS LastRegistered FROM Customer WITH (NOLOCK) ORDER BY RegisterDate DESC", conn))
                        {
                            if (rs.Read())
                            {
                                lastRegDate = DB.RSFieldDateTime(rs, "LastRegistered");
                            }
                        }
                    }

                    yearCount = DateTime.Now.Year - firstRegDate.Year;

                    if (firstRegDate == DateTime.MinValue && lastRegDate == DateTime.MinValue)
                    {
                        yearCount = -1;
                    }

                    storeTimeSpan = lastRegDate.Subtract(firstRegDate);
                    firstDate = firstRegDate;
                    if (storeTimeSpan.Days < 365)
                    {
                        monthCount = lastRegDate.Month - firstRegDate.Month;
                    }
                }
            }

            public static int GetStoreDayAge(string chartControlId)
            {
                TimeSpan daysDiff;
                DateTime firstOrderDate = DateTime.MinValue;
                DateTime lastOrderDate = DateTime.MinValue;
                DateTime firstRegDate = DateTime.MinValue;
                DateTime lastRegDate = DateTime.MinValue;

                if (chartControlId.Equals("OrdersChart"))
                {
                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS("SELECT TOP(1) OrderDate AS FirstOrder FROM Orders WITH (NOLOCK) ORDER BY OrderDate ASC", conn))
                        {
                            if (rs.Read())
                            {
                                firstOrderDate = DB.RSFieldDateTime(rs, "FirstOrder");
                            }
                        }
                    }

                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS("SELECT TOP(1) OrderDate AS LastOrder FROM Orders WITH (NOLOCK) ORDER BY OrderDate DESC", conn))
                        {
                            if (rs.Read())
                            {
                                lastOrderDate = DB.RSFieldDateTime(rs, "LastOrder");
                            }
                        }
                    }

                    daysDiff = lastOrderDate.Subtract(firstOrderDate);
                }
                else
                {
                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS("SELECT TOP(1) RegisterDate AS FirstRegistered FROM Customer WITH (NOLOCK) ORDER BY RegisterDate ASC", conn))
                        {
                            if (rs.Read())
                            {
                                firstRegDate = DB.RSFieldDateTime(rs, "FirstRegistered");
                            }
                        }
                    }

                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS("SELECT TOP(1) RegisterDate AS LastRegistered FROM Customer WITH (NOLOCK) ORDER BY RegisterDate DESC", conn))
                        {
                            if (rs.Read())
                            {
                                lastRegDate = DB.RSFieldDateTime(rs, "LastRegistered");
                            }
                        }
                    }

                    daysDiff = lastRegDate.Subtract(firstRegDate);
                }

                return daysDiff.Days;
            }

            /// <summary>
            /// returns the dates of a given week number and given date
            /// </summary>
            /// <param name="dateToProcess">the date to process</param>
            /// <param name="weekNumber">the week number to calculate what days belongs to the specified week number</param>
            /// <param name="counter">the day counter</param>
            /// <returns>the date</returns>
            public static DateTime GetWeekDatesOfMonth(DateTime dateToProcess, int weekNumber, int counter)
            {
                CultureInfo info = Thread.CurrentThread.CurrentCulture;
                DayOfWeek firstDay = info.DateTimeFormat.FirstDayOfWeek;
                DayOfWeek dayFrom = info.Calendar.GetDayOfWeek(dateToProcess);
                int firstDayDiff = dayFrom - firstDay;
                int lastDayDiff = 7 - firstDayDiff;
                int daysInMonth = info.Calendar.GetDaysInMonth(dateToProcess.Year, dateToProcess.Month);
                int numberOfWeeks = CommonLogic.IIF(daysInMonth % 7 == 0, 4, 5);

                Dictionary<int, List<DateTime>> weekDates = new Dictionary<int, List<DateTime>>();
                List<DateTime> dates = new List<DateTime>(); ;
                DateTime thisDate = new DateTime();


                for (int a = 0; a < 7; a++)//but add in the collection only the days that is on that month
                {
                    //add to the list of dates
                    thisDate = new DateTime(dateToProcess.Year, dateToProcess.Month, a + 1).AddDays(-firstDayDiff);
                    dates.Add(thisDate);
                }

                if (weekNumber == 2)
                {
                    dates.Clear();
                    thisDate = thisDate.AddDays(1);
                    dates.Add(thisDate);

                    for (int m = 0; m < 6; m++)
                    {
                        thisDate = thisDate.AddDays(1);
                        dates.Add(thisDate);
                    }
                }

                if (weekNumber > 2)
                {
                    dates.Clear();
                    thisDate = thisDate.AddDays(1).AddDays((weekNumber - 2) * 7);
                    dates.Add(thisDate);

                    for (int k = 0; k < 6; k++)
                    {
                        thisDate = thisDate.AddDays(1);
                        dates.Add(thisDate);
                    }
                }

                return dates[counter];
            }

            /// <summary>
            /// Calculates only the week day of a particuler month and year. This does not include the day of previous or next month's days that will fall on the week of the specified month
            /// </summary>
            /// <param name="dateToProcess">DateTime to extract year and months from</param>
            /// <returns>returns collection of int dates</returns>
            public static Dictionary<int, List<int>> GetWeekDatesOfMonth(DateTime dateToProcess)
            {
                CultureInfo info = Thread.CurrentThread.CurrentCulture;
                DayOfWeek firstDay = info.DateTimeFormat.FirstDayOfWeek;
                DayOfWeek dayFrom = info.Calendar.GetDayOfWeek(dateToProcess);
                int daydiff = dayFrom - firstDay;
                int lastDayDiff;
                int daysInMonth = info.Calendar.GetDaysInMonth(dateToProcess.Year, dateToProcess.Month);


                if (daydiff == 0)
                {
                    lastDayDiff = 7;
                }
                else
                {
                    lastDayDiff = 7 - daydiff;
                }

                int numberOfWeeks = 0;
                if (daysInMonth % 7 == 0)
                {
                    if (daydiff == 0)
                    {
                        numberOfWeeks = 4;
                    }
                    else
                    {
                        numberOfWeeks = 5;
                    }
                }
                else
                {
                    if (daysInMonth == 31 && daydiff >= 5)
                    {
                        numberOfWeeks = 6;
                    }
                    else
                    {
                        if (daysInMonth == 30 && daydiff == 6)
                        {
                            numberOfWeeks = 6;
                        }
                        else
                        {
                            numberOfWeeks = 5;
                        }
                    }

                }

                int counter = 0;

                Dictionary<int, List<int>> weekDates = new Dictionary<int, List<int>>();
                List<int> dates;

                //populate the week 1st options 
                for (int i = 0; i < numberOfWeeks; i++)
                {
                    if (i == 0)//ok lets get the first week
                    {
                        dates = new List<int>();
                        for (int a = 0; a < lastDayDiff; a++)//but add in the collection only the days that is on that month
                        {
                            //add to the list of dates
                            dates.Add(a + 1);
                            counter++;
                        }
                        //then add to the dictionary
                        weekDates.Add((i + 1), dates);
                    }
                    else
                    {
                        int counter2 = counter + 7;
                        dates = new List<int>();
                        for (int b = counter; counter < counter2; b++)
                        {
                            dates.Add(b + 1);
                            counter++;

                            if (counter >= daysInMonth)
                            {
                                break;
                            }
                        }
                        weekDates.Add((i + 1), dates);
                    }
                }
                return weekDates;
            }

            #endregion


            #region methods use for tabular statistic

            /// <summary>
            /// Returns a collection of customer statistics
            /// </summary>
            /// <returns>Collection of Customer Statistics</returns>
            public static List<Statistic> GetCustomerStatistic()
            {
                List<Statistic> StatsCollection = new List<Statistic>();
                string SuperuserFilter = string.Empty;

                IntegerCollection superUserIds = AppLogic.GetSuperAdminCustomerIDs();
                if (superUserIds.Count > 0)
                {
                    SuperuserFilter = String.Format("and Customer.CustomerID not in ({0})", superUserIds.ToString());
                }


                for (int i = 0; i < 2; i++)
                {
                    using (SqlConnection conn = DB.dbConn())
                    {
                        string sql = string.Format("select sum(case when datediff(dy, RegisterDate, getdate()) = 0 then 1 else 0 end) Today, sum(case when datediff(wk, RegisterDate, getdate()) = 0 then 1 else 0 end) ThisWeek, sum(case when datediff(mm, RegisterDate, getdate()) = 0 then 1 else 0 end) ThisMonth, sum(case when datediff(yy, RegisterDate, getdate()) = 0 then 1 else 0 end) ThisYear, count(*) AllTime from Customer with (NOLOCK) where IsRegistered = {0} {1}", i, SuperuserFilter);
                        conn.Open();
                        using (IDataReader rs = DB.GetRS(sql, conn))
                        {
                            if (rs.Read())
                            {
                                Statistic stats = new Statistic(DB.RSFieldInt(rs, "Today").ToString(), DB.RSFieldInt(rs, "ThisWeek").ToString(),
                                    DB.RSFieldInt(rs, "ThisMonth").ToString(), DB.RSFieldInt(rs, "ThisYear").ToString(), DB.RSFieldInt(rs, "AllTime").ToString());

                                string a = string.Empty;
                                if (i == 0)
                                {
                                    stats.m_customerType = "# Anon Customers";
                                }
                                else
                                {
                                    stats.m_customerType = "# Registering Customers";
                                }

                                StatsCollection.Add(stats);
                            }
                        }
                    }
                }

                return StatsCollection;
            }

            /// <summary>
            /// returns a collection of Order Statistics
            /// </summary>
            /// <param name="TransactionState"></param>
            /// <param name="SummaryLinesOnly"></param>
            /// <param name="SkinID"></param>
            /// <param name="LocaleSetting"></param>
            /// <returns></returns>
            public static List<Statistic> GetOrdersStatistic(string TransactionState, bool SummaryLinesOnly, int SkinID, string LocaleSetting)
            {

                string TranField = string.Empty;
                switch (TransactionState)
                {
                    case "AUTHORIZED":
                        TranField = "AuthorizedOn";
                        break;
                    case "CAPTURED":
                        TranField = "CapturedOn";
                        break;
                    case "VOIDED":
                        TranField = "VoidedOn";
                        break;
                    case "REFUNDED":
                        TranField = "RefundedOn";
                        break;
                }

                List<Statistic> StatsCollection = new List<Statistic>();

                StringBuilder sqlquery = new StringBuilder();

                sqlquery.Append("select sum(case when datediff(dy, " + TranField + ", getdate()) = 0 then 1 else 0 end) Today, sum(case when datediff(wk, " + TranField + ", getdate()) = 0 then 1 else 0 end) ThisWeek, sum(case when datediff(mm, " + TranField + ", getdate()) = 0 then 1 else 0 end) ThisMonth, sum(case when datediff(yy, " + TranField + ", getdate()) = 0 then 1 else 0 end) ThisYear, " + CommonLogic.IIF(TranField.Equals("CapturedOn"), "count(*)", " count(" + TranField + ")") + " AllTime from Orders with (NOLOCK) where EditedOn IS NULL and TransactionState=" + DB.SQuote(TransactionState.ToUpperInvariant()));
                sqlquery.AppendLine();
                if (!SummaryLinesOnly)
                {
                    sqlquery.Append("select sum(case when datediff(dy, " + TranField + ", getdate()) = 0 then OrderSubtotal else 0 end) Today, sum(case when datediff(wk, " + TranField + ", getdate()) = 0 then OrderSubtotal else 0 end) ThisWeek, sum(case when datediff(mm, " + TranField + ", getdate()) = 0 then OrderSubtotal else 0 end) ThisMonth, sum(case when datediff(yy, " + TranField + ", getdate()) = 0 then OrderSubtotal else 0 end) ThisYear, sum(case when " + TranField + " is not null then OrderSubtotal else 0 end) AllTime from Orders with (NOLOCK) where EditedOn IS NULL and TransactionState=" + DB.SQuote(TransactionState.ToUpperInvariant()));
                    sqlquery.AppendLine();
                    sqlquery.Append("select sum(case when datediff(dy, " + TranField + ", getdate()) = 0 then OrderTax else 0 end) Today, sum(case when datediff(wk, " + TranField + ", getdate()) = 0 then OrderTax else 0 end) ThisWeek, sum(case when datediff(mm, " + TranField + ", getdate()) = 0 then OrderTax else 0 end) ThisMonth, sum(case when datediff(yy, " + TranField + ", getdate()) = 0 then OrderTax else 0 end) ThisYear, sum(case when " + TranField + " is not null then OrderTax else 0 end) AllTime from Orders with (NOLOCK) where EditedOn IS NULL and TransactionState=" + DB.SQuote(TransactionState.ToUpperInvariant()));
                    sqlquery.AppendLine();
                    sqlquery.Append("select sum(case when datediff(dy, " + TranField + ", getdate()) = 0 then OrderShippingCosts else 0 end) Today, sum(case when datediff(wk, " + TranField + ", getdate()) = 0 then OrderShippingCosts else 0 end) ThisWeek, sum(case when datediff(mm, " + TranField + ", getdate()) = 0 then OrderShippingCosts else 0 end) ThisMonth, sum(case when datediff(yy, " + TranField + ", getdate()) = 0 then OrderShippingCosts else 0 end) ThisYear, sum(case when " + TranField + " is not null then OrderShippingCosts else 0 end) AllTime from Orders with (NOLOCK) where EditedOn IS NULL and TransactionState=" + DB.SQuote(TransactionState.ToUpperInvariant()));
                    sqlquery.AppendLine();
                }
                sqlquery.Append("select sum(case when datediff(dy, " + TranField + ", getdate()) = 0 then OrderTotal else 0 end) Today, sum(case when datediff(wk, " + TranField + ", getdate()) = 0 then OrderTotal else 0 end) ThisWeek, sum(case when datediff(mm, " + TranField + ", getdate()) = 0 then OrderTotal else 0 end) ThisMonth, sum(case when datediff(yy, " + TranField + ", getdate()) = 0 then OrderTotal else 0 end) ThisYear, sum(case when " + TranField + " is not null then OrderTotal else 0 end) AllTime from Orders with (NOLOCK) where EditedOn IS NULL and TransactionState=" + DB.SQuote(TransactionState.ToUpperInvariant()));
                sqlquery.AppendLine();

                if (!SummaryLinesOnly)
                {
                    sqlquery.Append("exec aspdnsf_OrderAvgSummary " + DB.SQuote(TransactionState));
                    sqlquery.AppendLine();
                }

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS(sqlquery.ToString(), conn))
                    {
                        if (rs.Read())
                        {
                            Statistic stats = new Statistic("# Orders", DB.RSFieldInt(rs, "Today").ToString(), DB.RSFieldInt(rs, "ThisWeek").ToString(),
                                DB.RSFieldInt(rs, "ThisMonth").ToString(), DB.RSFieldInt(rs, "ThisYear").ToString(), DB.RSFieldInt(rs, "AllTime").ToString());

                            StatsCollection.Add(stats);
                        }
                        if (!SummaryLinesOnly)
                        {
                            if (rs.NextResult() && rs.Read())
                            {
                                Statistic stats2 = new Statistic("Order SubTotals",
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "Today"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisWeek"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisMonth"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisYear"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "AllTime"), true));
                                StatsCollection.Add(stats2);
                            }
                            if (rs.NextResult() && rs.Read())
                            {
                                Statistic stats3 = new Statistic("Order Tax",
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "Today"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisWeek"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisMonth"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisYear"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "AllTime"), true));
                                StatsCollection.Add(stats3);
                            }
                            if (rs.NextResult() && rs.Read())
                            {
                                Statistic stats4 = new Statistic("Order Shipping Costs",
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "Today"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisWeek"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisMonth"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisYear"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "AllTime"), true));
                                StatsCollection.Add(stats4);
                            }

                        }
                        if (rs.NextResult() && rs.Read())
                        {
                            Statistic stats5 = new Statistic("Order Total",
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "Today"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisWeek"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisMonth"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisYear"), true),
                                    Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "AllTime"), true));
                            StatsCollection.Add(stats5);
                        }

                        if (!SummaryLinesOnly)
                        {
                            if (rs.NextResult() && rs.Read())
                            {
                                Statistic stats5 = new Statistic("Average Order Size",
                                   Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "Today"), true),
                                   Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisWeek"), true),
                                   Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisMonth"), true),
                                   Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "ThisYear"), true),
                                   Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.RSFieldDecimal(rs, "AllTime"), true));
                                StatsCollection.Add(stats5);
                            }
                        }

                    }

                }

                return StatsCollection;
            }

            #endregion

        }
        protected void lnkCacheSwitch_Click(object sender, EventArgs e)
        {
            bool cache = AppLogic.AppConfigBool("CacheMenus");
            // toggle the value
            cache = !cache;

            AspDotNetStorefrontCore.AppConfig config = AppLogic.GetAppConfigRouted("CacheMenus", AppLogic.StoreID());
            if (config != null)
            {
                config.ConfigValue = cache.ToString();
            }

            if (AppLogic.AppConfigBool("CacheMenus"))
            {
                // text
                lnkCacheSwitch.Text = AppLogic.GetString("admin.common.OnUC", m_SkinID, ThisCustomer.LocaleSetting) + ": " +
                                        AppLogic.GetString("admin.splash.aspx.19", m_SkinID, ThisCustomer.LocaleSetting);
            }
            else
            {
                // text
                lnkCacheSwitch.Text = AppLogic.GetString("admin.common.OffUC", m_SkinID, ThisCustomer.LocaleSetting) + ": " +
                                        AppLogic.GetString("admin.splash.aspx.18", m_SkinID, ThisCustomer.LocaleSetting);
            }
        }
    }
}
