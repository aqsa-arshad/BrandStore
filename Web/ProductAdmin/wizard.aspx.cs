// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontBuySafe;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;
using AssetServer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontAdmin
{
    public partial class wizard : AdminPageBase
    {
        IDictionary<String, String[]> _PaymentOptionsByCountry = new Dictionary<String, String[]>();
           
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (!IsPostBack)
            {
                if (!ThisCustomer.IsAdminSuperUser)
                {
                    resetError("Insufficient Permission!", true);
                    divMain.Visible = false;
                }
                else
                {
                    divMain.Visible = true;
                    loadData();
                }

                EncryptWebConfigRow.Visible = (AppLogic.TrustLevel == AspNetHostingPermissionLevel.Unrestricted);
                MachineKeyRow.Visible = (AppLogic.TrustLevel == AspNetHostingPermissionLevel.Unrestricted);
            }
            Page.Form.DefaultButton = btnSubmitBottom.UniqueID;

            String amazonPrompt = "Limited Time: <a href='{0}' target='_blank'>No Processing Fees</a>";
            if (DateTime.Now > new DateTime(2011, 12, 1))
            {
                amazonPrompt = "<a href='{0}' target='_blank'>Benefits for your business</a>";
            }
            litAmazonPrompt.Text = amazonPrompt.FormatWith("http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=checkoutbyamazon&type=benefits");
        }

        protected void ddlCountries_SelectedIndexChanged(Object sender, EventArgs e)
        {
            SetPaymentOptionVisibility(ddlCountries.SelectedValue);
            BuildGatewayList();
        }

        protected void repGateways_DataBinding(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RadioButton rb = e.Item.FindControl("rbGateway") as RadioButton;
                LinkButton btnConfigureGateway = e.Item.FindControl("btnConfigureGateway") as LinkButton;
                Image imgPayPal = e.Item.FindControl("imgPayPal") as Image;
                
                GatewayData item = e.Item.DataItem as GatewayData;

                Control trGateway = e.Item.FindControl("trGateway");
                if (trGateway != null)
                {
                    trGateway.Visible = IsPaymentOptionAvailable(item.DisplayName, ddlCountries.SelectedValue);
                }

                if (item.DisplayName.Contains("PayPal"))                  
                    imgPayPal.Visible = true;

                if(item.DisplayName.ToUpper().Contains("PAYFLOW"))
                    imgPayPal.ImageUrl = "images/PayPal_OnBoarding_PayFlow.png";

                if (AppLogic.AppConfig("PaymentGateway", 0, false).EqualsIgnoreCase("PayFlowPro"))
                {
                    var payFlowProProduct = AppLogic.GetAppConfig(0, "PayFlowPro.Product");
                    rb.Checked = item.DisplayName == payFlowProProduct.ConfigValue;                    
                }
                else
                    rb.Checked = AppLogic.AppConfig("PaymentGateway", 0, false).EqualsIgnoreCase(item.GatewayIdentifier);

                if (item.IsInstalled)
                {
                    GatewayProcessor gp = GatewayLoader.GetProcessor(item.GatewayIdentifier);
                    if (gp != null)
                    {
                        IConfigurationAtom atom = gp.GetConfigurationAtom();
                        rb.Enabled = atom == null || atom.IsConfigured(0) || atom.IsConfigured(AppLogic.StoreID());
                    }
                }
                else
                {
                    rb.Enabled = false;
                    btnConfigureGateway.Visible = false;
                }
                if (item.GatewayIdentifier != null && item.GatewayIdentifier.EqualsIgnoreCase("manual"))
                    btnConfigureGateway.Visible = false;
            }
        }

        private void SetGatewayRBEnabled()
        {
            foreach (RepeaterItem e in repGateways.Items)
            {
                RadioButton rb = e.FindControl("rbGateway") as RadioButton;
                LinkButton btnConfigureGateway = e.FindControl("btnConfigureGateway") as LinkButton;
                HiddenField hfGatewayIdentifier = e.FindControl("hfGatewayIdentifier") as HiddenField;

                try
                {
                    GatewayProcessor gp = GatewayLoader.GetProcessor(hfGatewayIdentifier.Value);

                    IConfigurationAtom atom = gp.GetConfigurationAtom();
                    rb.Enabled = atom == null || atom.IsConfigured(0) || atom.IsConfigured(AppLogic.StoreID());
                    
                }
                catch // the gateway doesn't exist.
                {
                    rb.Enabled = false;
                    btnConfigureGateway.Visible = false;
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            _PaymentOptionsByCountry.Add("PayPal Express Checkout", new String[] { "US", "CA", "AR", "AM", "AW", "AU", "AT", "BS", "BE", "BZ", "BM", "BO", "BR", "BG", "KY", "CL", "CN", "CO", "CR", "HR", "CY", "CZ", "DK", "DO", "EC", "FI", "FR", "DE", "GI", "GR", "GT", "HK", "HU", "IN", "ID", "IE", "IL", "IT", "JM", "JP", "JO", "KZ", "KW", "MY", "MX", "NL", "NZ", "NO", "PE", "PH", "PL", "PT", "QA", "RO", "RU", "SA", "SG", "SI", "ZA", "ES", "SE", "CH", "TW", "TH", "TR", "UA", "AE", "GB", "UY", "VE" });
            _PaymentOptionsByCountry.Add("PayPal Payments Advanced", new String[] { "US" });
            _PaymentOptionsByCountry.Add("PayPal Payments Standard", new String[] { "US", "CA", "AR", "AM", "AW", "AU", "AT", "BS", "BE", "BZ", "BM", "BO", "BR", "BG", "KY", "CL", "CN", "CO", "CR", "HR", "CY", "CZ", "DK", "DO", "EC", "FI", "FR", "DE", "GI", "GR", "GT", "HK", "HU", "IN", "ID", "IE", "IL", "IT", "JM", "JP", "JO", "KZ", "KW", "MY", "MX", "NL", "NZ", "NO", "PE", "PH", "PL", "PT", "QA", "RO", "RU", "SA", "SG", "SI", "ZA", "ES", "SE", "CH", "TW", "TH", "TR", "UA", "AE", "GB", "UY", "VE" });
            _PaymentOptionsByCountry.Add("PayPal PayFlow Link Only", new String[] { "US", "CA" });
            _PaymentOptionsByCountry.Add("PayPal PayFlow Pro", new String[] { "US", "CA", "AU", "NZ" });
            _PaymentOptionsByCountry.Add("PayPal Website Payments Pro", new String[] { "US", "UK", "CA" });

            if (!IsPostBack)
            {
                using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(DB.GetDBConn()))
                {
                    connection.Open();
                    using (IDataReader reader = DB.GetRS("select TwoLetterISOCode, Name from Country Order By DisplayOrder", connection))
                    {
                        ddlCountries.DataSource = reader;
                        ddlCountries.DataValueField = "TwoLetterISOCode";
                        ddlCountries.DataTextField = "Name";
                        ddlCountries.DataBind();
                    }
                    ddlCountries.SelectedIndex = 0;
                }

                BuildGatewayList();
            }
            
            base.OnInit(e);
        }

        protected void repGateways_ItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            if (!e.CommandName.EqualsIgnoreCase("ShowConfiguration"))
                return;

            GatewayProcessor gp = GatewayLoader.GetProcessor(e.CommandArgument as string);
            if (gp == null)
                return;

            HiddenField hfGatewayIdentifier = e.Item.FindControl("hfGatewayIdentifier") as HiddenField;
            if (hfGatewayIdentifier != null && hfGatewayIdentifier.Value == "PayFlowPro")
            {
                HiddenField hfGatewayProductIdentifier = e.Item.FindControl("hfGatewayProductIdentifier") as HiddenField;
                LaunchGatewayConfiguration(gp, String.Format("Gateway.{0}.ConfigAtom.xml", hfGatewayProductIdentifier.Value.Replace(" ", "").Replace(" ", "")));
            }
            else
                LaunchGatewayConfiguration(gp);
        }

        private void LaunchGatewayConfiguration(GatewayProcessor gateway)
        {
            LaunchGatewayConfiguration(gateway, String.Empty);
        }

        private void LaunchGatewayConfiguration(GatewayProcessor gateway, String resourceName)
        {
            GatewayConfigurationAtom.AtomConfigurationDataSource = gateway.GetConfigurationAtom(resourceName);
            GatewayConfigurationAtom.DataBind();
            GatewayConfigurationAtom.Show();
        }

        protected void ShowModalAtomByXMLFile_Click(object sender, EventArgs e)
        {
            LinkButton lb = sender as LinkButton;
            FileConfigurationAtom.SetConfigurationFile(lb.CommandArgument);
            FileConfigurationAtom.Show();
        }

        protected void GatewayConfigurationAtom_Saved(object sender, EventArgs e)
        {
            SetGatewayRBEnabled();
        }

        protected void loadData()
        {
            bool BadSSL = CommonLogic.QueryStringBool("BadSSL");
            if (BadSSL)
            {
                resetError("No SSL certificate was found on your site. Please check with your hosting company! You must be able to invoke your store site using https:// before turning SSL on in this admin site!", false);
            }

            
            String PM = AppLogic.AppConfig("PaymentMethods", 0, false).ToUpperInvariant();


            BuildPaymentMethodList(PM);    
            
            if (AppLogic.TrustLevel == AspNetHostingPermissionLevel.Unrestricted)
            {
                Configuration webconfig = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
                AppSettingsSection appsettings = (AppSettingsSection)webconfig.GetSection("appSettings");
                rblEncrypt.Items.FindByValue(appsettings.SectionInformation.IsProtected.ToString().ToLowerInvariant()).Selected = true;

                MachineKeySection mkeysec = (MachineKeySection)webconfig.GetSection("system.web/machineKey");

                if (mkeysec.ValidationKey.Equals("autogenerate", StringComparison.InvariantCultureIgnoreCase))
                {
                    rblStaticMachineKey.Items.FindByValue("false").Selected = true;
                    ltStaticMachineKey.Text = AppLogic.GetString("admin.wizard.SetStaticMachineKey", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
                else
                {
                    rblStaticMachineKey.Items.FindByValue("false").Selected = true;
                    ltStaticMachineKey.Text = AppLogic.GetString("admin.wizard.ChangeStaticMachineKey", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }

            }

            switch (BuySafeController.GetBuySafeState())
	        {
		        case BuySafeState.NotEnabledFreeTrialAvailable:
                 break;
                case BuySafeState.EnabledFullUserAfterFreeTrial:
                case BuySafeState.EnabledOnFreeTrial:
                    pnlBuySafeActive.Visible = true;
                    pnlBuySafeInactive.Visible = false;
                    litBuySafeActiveMsg.Text = "buySAFE is enabled";
                 break;
                case BuySafeState.EnabledFreeTrialExpired:
                case BuySafeState.NotEnabledFreeTrialExpired:
                    pnlBuySafeActive.Visible = true;
                    pnlBuySafeInactive.Visible = false;
                    litBuySafeActiveMsg.Text = "<span style='line-height:normal;'>Trial expired. Please contact buySAFE to enable your account.<br />Call 1.888.926.6333 | Email: <a href='mailto:customersupport@buysafe.com'>customersupport@buysafe.com</a></span>";
                 break;
                case BuySafeState.Error:
                    pnlBuySafeActive.Visible = true;
                    pnlBuySafeInactive.Visible = false;
                    litBuySafeActiveMsg.Text = "<span style='line-height:normal;'>Please contact buySAFE to enable your account.<br />Call 1.888.926.6333 | Email: <a href='mailto:customersupport@buysafe.com'>customersupport@buysafe.com</a></span>";
                 break;
	        }
        }

        public void BuildPaymentMethodList(string payMethod)
        {
            List<string> paymentMethods = new List<string>();
            string[] paymentMethodsCommaSeparated = payMethod.Split(',');
            foreach (string paymentMethod in paymentMethodsCommaSeparated)
                paymentMethods.Add(paymentMethod.ToLowerInvariant().Trim());

            cbxCreditCard.Checked = paymentMethods.Contains("Credit Card".ToLowerInvariant());
            cbxCheckoutByAmazon.Checked = paymentMethods.Contains("CheckoutByAmazon".ToLowerInvariant());            
            cbxPayPalExpress.Checked = paymentMethods.Contains("PayPalExpress".ToLowerInvariant());
            cbxPayPal.Checked = paymentMethods.Contains("PayPal".ToLowerInvariant());
            cbxRequestQuote.Checked = paymentMethods.Contains("Request Quote".ToLowerInvariant());
            cbxPurchaseOrder.Checked = paymentMethods.Contains("Purchase Order".ToLowerInvariant());
            cbxCheckByMail.Checked = paymentMethods.Contains("Check By Mail".ToLowerInvariant());
            cbxCOD.Checked = paymentMethods.Contains("C.O.D.".ToLowerInvariant());
            cbxECheck.Checked = paymentMethods.Contains("ECHECK".ToLowerInvariant());
            cbxCardinalMyCheck.Checked = paymentMethods.Contains("CARDINALMYECHECK".ToLowerInvariant());
            cbxMicroPay.Checked = paymentMethods.Contains("MICROPAY".ToLowerInvariant()) || ("MICROPAY".Equals(AppLogic.ro_PMMicropay, StringComparison.InvariantCultureIgnoreCase) && AppLogic.MicropayIsEnabled());
            cbxMoneyBookers.Checked = paymentMethods.Contains("Moneybookers Quick Checkout".ToLowerInvariant());
        }

        private void SetPaymentOptionVisibility(String currentCountry)
        {
            if (String.IsNullOrEmpty(currentCountry))
                return;
                        
            trPayPalExpress.Visible = IsPaymentOptionAvailable("PayPal Express Checkout", currentCountry);            
            trPayPalWebsitePaymentsStandard.Visible = IsPaymentOptionAvailable("PayPal Payments Standard", currentCountry);
        }

        private Boolean IsPaymentOptionAvailable(String paymentOption, String currentCountry)
        {
            if (String.IsNullOrEmpty(paymentOption))
                return true;

            if (String.IsNullOrEmpty(currentCountry))
                return true;

            if (!_PaymentOptionsByCountry.ContainsKey(paymentOption))
                return true;

            return _PaymentOptionsByCountry[paymentOption].Contains(currentCountry);
        }
     
        public void BuildGatewayList()
        {
            List<GatewayData> ds = new List<GatewayData>();
            String downloadLink = "<br /><a href='{1}' onclick='showGatewayDirections('{2}');'>{0}</a>";
            if (repGateways.DataSource == null)
            {
                Dictionary<String, List<AssetServerAsset>> serverAssets = AssetServer.AssetServerAsset.GetAssetServerAssets();
                IEnumerable<string> availibleGateways = GatewayLoader.GetAvailableGatewayNames();
                
                foreach (String s in availibleGateways)
                {
                    GatewayProcessor GWActual = GatewayLoader.GetProcessor(s);
                    GatewayData gd = new GatewayData();
                    gd.DisplayName = GWActual.DisplayName(ThisCustomer.LocaleSetting);
                    gd.AdministratorSetupPrompt = GWActual.AdministratorSetupPrompt;
                    if (serverAssets.ContainsKey(GWActual.TypeName))
                    {
                        if (serverAssets[GWActual.TypeName].Count == 0)
                            return;

                        AssetServer.AssetVersion dllVersion = new AssetVersion(GWActual.Version);
                        AssetServer.AssetVersion availibleVersion = new AssetVersion(serverAssets[GWActual.TypeName][0].Version);

                        if (availibleVersion.CompareTo(dllVersion) > 0)
                        {
                            gd.AdministratorSetupPrompt += "<b>Download Update</b>";
                            foreach (AssetServerAsset asa in serverAssets[GWActual.TypeName])
                                gd.AdministratorSetupPrompt += String.Format(downloadLink, asa.Title + " (" + asa.Version + ")", asa.Link, CommonLogic.IIF(String.IsNullOrEmpty(asa.DownloadInstructions), String.Empty, HttpContext.Current.Server.HtmlEncode(asa.DownloadInstructions)));
                        }
                        
                        serverAssets.Remove(GWActual.TypeName);
                    }
                    gd.IsInstalled = true;
                    gd.GatewayIdentifier = s;
                    ds.Add(gd);
                }
                //
                foreach (KeyValuePair<String, List<AssetServerAsset>> sa in serverAssets)
                {
                    if(sa.Value.Count == 0)
                        break;
                    GatewayData gd = new GatewayData();
                    gd.DisplayName = sa.Value[0].Title;
                    gd.IsInstalled = false;
                    StringBuilder setupPrompt = new StringBuilder();
                    setupPrompt.Append("<b>Download</b>");
                    foreach (AssetServerAsset asa in sa.Value)
                        setupPrompt.AppendFormat(downloadLink, asa.Title, asa.Link, CommonLogic.IIF(String.IsNullOrEmpty(asa.DownloadInstructions), String.Empty, HttpContext.Current.Server.HtmlEncode(asa.DownloadInstructions)));
                    gd.AdministratorSetupPrompt = setupPrompt.ToString();
                    ds.Add(gd);
                }

                ds.Add(CreateGatewayData("PayPal Payflow Link", "PayFlowPro", "(also enables PayPal Express Checkout) - See <a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=paypalpayflowlink&type=manual' target='_blank'>Manual</a>."));
                ds.Add(CreateGatewayData("PayPal Payments Advanced", "PayFlowPro", "(also enables PayPal Express Checkout) - See <a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=paypalpaymentsadvanced&type=manual' target='_blank'>Manual</a>."));
                
                ds = ds.Where(gd => IsPaymentOptionAvailable(gd.DisplayName, ddlCountries.SelectedValue)).ToList();

                ds = SortGatewayList(ds);

                repGateways.DataSource = ds;
                repGateways.DataBind();
            }
        }

        private GatewayData CreateGatewayData(String displayName, String gatewayIdentifier, String administratorSetupPrompt)
        {
            GatewayData gatewayData = new GatewayData();
            gatewayData.DisplayName = displayName;
            gatewayData.AdministratorSetupPrompt = administratorSetupPrompt;
            gatewayData.IsInstalled = true;
            gatewayData.GatewayIdentifier = gatewayIdentifier;
            return gatewayData;
        }

        private List<GatewayData> SortGatewayList(List<GatewayData> ds)
        {
            return ds.OrderByDescending(d => d.IsInstalled)
                .ThenByDescending(d => d.DisplayName.StartsWith("Manual"))
                .ThenByDescending(d => d.DisplayName.StartsWith("PayPal Payments Advanced"))
                .ThenByDescending(d => d.DisplayName.StartsWith("PayPal Payments Pro"))
                .ThenByDescending(d => d.DisplayName.StartsWith("PayPal Payflow Link"))
                .ThenByDescending(d => d.DisplayName.StartsWith("PayPal Payflow Pro"))
                .ThenByDescending(d => d.DisplayName.Contains("Authorize.net"))
                .ThenBy(d => d.DisplayName).ToList();
        }

        private string GetExternalDownloads(List<AssetServerAsset> ListOfAssets, string AboveVersion, Boolean IsUpdate)
        {
            StringBuilder ret = new StringBuilder();
            if (IsUpdate)
                ret.Append("<b>Update:</b> ");
            else
                ret.Append("<b>Download:</b> ");
            string Seperator = " | ";
            foreach (AssetServerAsset asset in ListOfAssets)
            {
                AssetVersion thisVersion = new AssetVersion(asset.Version);
                AssetVersion comparedVersion = new AssetVersion(AboveVersion);
                
                if (thisVersion.CompareTo(comparedVersion) > 0)
                {
                    ret.AppendFormat("<a href='{0}' target='_blank'>{1} {2}</a>{3}", asset.Link, asset.Title, asset.Version, Seperator);
                }
            }
            String retS = ret.ToString();
            if (retS.Length > Seperator.Length && retS.EndsWith(Seperator))
                retS = retS.Substring(0, retS.Length - Seperator.Length);

            return retS;
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class='noticeMsg'>NOTICE:</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class='errorMsg'>ERROR:</font>&nbsp;&nbsp;&nbsp;";

            if (error.Length > 0)
                str += error + "";
            else
                str = "";

            ltError.Text = str;
        }
        
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            Page.Validate();
            if (!Page.IsValid)
            {
                return;
            }

            bool BadSSL = false;

            // save the config settings:
            AtomStoreZip.Save();
            AtomLiveServer.Save();

            StringBuilder errors = new StringBuilder();

            if (AppLogic.TrustLevel == AspNetHostingPermissionLevel.Unrestricted)
            {
                WebConfigManager webMgr = new WebConfigManager();
                if (webMgr.ProtectWebConfig != rblEncrypt.SelectedValue.Equals("true", StringComparison.InvariantCultureIgnoreCase) || rblStaticMachineKey.SelectedValue.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    webMgr.ProtectWebConfig = rblEncrypt.SelectedValue.Equals("true", StringComparison.InvariantCultureIgnoreCase);

                    if (rblStaticMachineKey.SelectedValue.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                    {
                        webMgr.SetMachineKey = true;
                        webMgr.ValidationKeyGenMethod = WebConfigManager.KeyGenerationMethod.Auto;
                        webMgr.DecryptKeyGenMethod = WebConfigManager.KeyGenerationMethod.Auto;
                    }

                    List<Exception> saveWebConfigExceptions = webMgr.Commit();

                    WebConfigManager webMgrNew = new WebConfigManager();

                    if (saveWebConfigExceptions.Count > 0 && (webMgr.ProtectWebConfig != webMgrNew.ProtectWebConfig || rblStaticMachineKey.SelectedValue.EqualsIgnoreCase("true")))
                    {
                        if (webMgr.ProtectWebConfig != webMgrNew.ProtectWebConfig)
                            errors.Append("Your web config encryption could not be changed due to the following error(s): <br />");
                        if (rblStaticMachineKey.SelectedValue.EqualsIgnoreCase("true"))
                            errors.Append("Could not set static machine key due to the following error(s): <br />");
                        foreach (Exception ex in saveWebConfigExceptions)
                            errors.Append(ex.Message + "<br />");
                    }
                }
            }

            if (AtomStoreUseSSL.GetValue(AppLogic.StoreID()).ToBool() || AtomStoreUseSSL.GetValue(0).ToBool())
            {
                BadSSL = true;
                String WorkerWindowInSSL = String.Empty;
                List<string> urlsToTry = new List<string>();
                urlsToTry.Add(AppLogic.GetStoreHTTPLocation(false).Replace("http://", "https://") + "empty.htm");
                urlsToTry.Add(AppLogic.GetStoreHTTPLocation(false).Replace("http://", "https://").Replace("https://", "https://www.") + "empty.htm");

                foreach (String urlToTry in urlsToTry)
                {
                    if (BadSSL)
                    {
                        WorkerWindowInSSL = CommonLogic.AspHTTP(urlToTry, 10);

                        if (!String.IsNullOrEmpty(WorkerWindowInSSL) && WorkerWindowInSSL.IndexOf("Worker") != -1)
                        {
                            AtomStoreUseSSL.Save();
                            BadSSL = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                AtomStoreUseSSL.Save();
            }
            
            AtomLiveServer.Save();
            AtomStoreCurrency.Save();
            AtomStoreCurrencyNumeric.Save();
            AtomStoreName.Save();
            AtomStoreLiveTransactions.Save();

            string temp = GetCheckedPaymentMethods();

            AppConfig config = AppLogic.GetAppConfig(0, "UseSSL");
            config = AppLogic.GetAppConfig(0, "PaymentMethods");
            if (config != null)
            {
                if (temp.Length > 0)
                {
                    config.ConfigValue = temp;
                }
                else
                {
                    config.ConfigValue = string.Empty;
                }
            }

            config = AppLogic.GetAppConfig(0, "PaymentGateway");
            if (config != null)
            {
                string newGateway = getSelectedGateway();
                string newGatewayProduct = getSelectedGatewayProduct();

                if (!String.IsNullOrEmpty(newGateway))
                    config.ConfigValue = newGateway;

                if (newGateway == "PayFlowPro")
                {
                    var payFlowProProduct = AppLogic.GetAppConfig(0, "PayFlowPro.Product");
                    payFlowProProduct.ConfigValue = newGatewayProduct;

                    // If PPA Gateway is selected, then set the PPA Method
                    if (newGatewayProduct == "PayPal Payments Advanced")
                    {
                        if (!temp.Contains("PayPalPaymentsAdvanced"))
                        {
                            var ppaConfig = AppLogic.GetAppConfig(0, "PaymentMethods");
                            ppaConfig.ConfigValue += ", PayPalPaymentsAdvanced";
                        }
                    }

                    // if any PayFlow gateway is selected, select PayPalExpress
                    if (!temp.Contains("PayPalExpress"))
                    {
                        var ppeConfig = AppLogic.GetAppConfig(0, "PaymentMethods");
                        ppeConfig.ConfigValue += ", PayPalExpress";
                        cbxPayPalExpress.Checked = true;
                    }
                }
            }

            if ("WIZARD".Equals(AppLogic.AppConfig("OrderShowCCPwd", 0, false), StringComparison.InvariantCultureIgnoreCase))
            {
                config = AppLogic.GetAppConfig(0, "OrderShowCCPwd");
                if (config != null)
                {
                    config.ConfigValue = CommonLogic.GetRandomNumber(1000, 1000000).ToString() + CommonLogic.GetRandomNumber(1000, 1000000).ToString() + CommonLogic.GetRandomNumber(1000, 1000000).ToString();
                }
            }

            string BuySafeMessage = string.Empty;

            if (rblBuySafeEnabled.SelectedIndex == 1)
            {
                BuySafeRegistrationStatus bss = BuySafeController.BuySafeOneClickSignup();
                if (!bss.Sucessful)
                {
                    BuySafeMessage = "<br/><b style='color:red;'>buySAFE could not be enabled.{0}";
                    errors.Append( string.Format(BuySafeMessage, (string.IsNullOrEmpty(bss.ErrorMessage) ? "" : " Error message: " + bss.ErrorMessage)));
                }
            }

            if (BadSSL)
                errors.Append("No SSL certificate was found on your site. Please check with your hosting company! You must be able to invoke your store site using https:// before turning SSL on in this admin site!<br />");

            if (errors.ToString().Length > 0)
            {
                resetError(errors.ToString(), true);
            }
            else
            {
                resetError( "Configuration Wizard completed successfully.", false );
            }

            loadData();
        }

        private string GetCheckedPaymentMethods()
        {
            String temp = "";
            if (cbxCreditCard.Checked)
                temp += ", Credit Card";
            if (cbxCheckoutByAmazon.Checked)
                temp += ", CheckoutByAmazon";
            if (cbxPayPalExpress.Checked)
                temp += ", PayPalExpress";
            if (cbxPayPal.Checked)
                temp += ", PayPal";
            if (cbxRequestQuote.Checked)
                temp += ", Request Quote";
            if (cbxPurchaseOrder.Checked)
                temp += ", Purchase Order";
            if (cbxCheckByMail.Checked)
                temp += ", Check By Mail";
            if (cbxCOD.Checked)
                temp += ", C.O.D.";
            if (cbxECheck.Checked)
                temp += ", ECHECK";
            if (cbxCardinalMyCheck.Checked)
                temp += ", CARDINALMYECHECK";
            if (cbxMicroPay.Checked)
                temp += ", MICROPAY";
            if (cbxMoneyBookers.Checked)
                temp += ", Moneybookers Quick Checkout";

            if (temp.Length == 0)
                return string.Empty;

            return temp.Substring(1);
        }

        private string getSelectedGateway()
        {
            foreach (RepeaterItem ri in repGateways.Items)
            {
                GroupRadioButton grb = ri.FindControl("rbGateway") as GroupRadioButton;
                HiddenField hfGatewayIdentifier = ri.FindControl("hfGatewayIdentifier") as HiddenField;
                if (grb.Checked)
                    return hfGatewayIdentifier.Value;
            }
            return null;
        }

        private string getSelectedGatewayProduct()
        {
            foreach (RepeaterItem ri in repGateways.Items)
            {
                GroupRadioButton grb = ri.FindControl("rbGateway") as GroupRadioButton;
                HiddenField hfGatewayProductIdentifier = ri.FindControl("hfGatewayProductIdentifier") as HiddenField;
                if (grb.Checked)
                    return hfGatewayProductIdentifier.Value;
            }
            return null;
        }
    }

    class GatewayData
    {
        public String DisplayName { get; set; }
        public String AdministratorSetupPrompt { get; set; }
        public Boolean IsInstalled { get; set; }
        public String GatewayIdentifier { get; set; }
        public GatewayData() { }
    }
}
